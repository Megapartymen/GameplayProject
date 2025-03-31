using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;

public enum VRController
{
    Left,
    Right
}

public enum ControllerButton
{
    Trigger,
    Grip
}

public class VRInputSystem : MonoBehaviour, IGlobalInitializable
{
    [TabGroup("Info"), ReadOnly] public bool IsSystemReady;
    [TabGroup("Info"), ReadOnly] public ActionBasedController LeftController;
    [TabGroup("Info"), ReadOnly] public ActionBasedController RightController;
    
    [TabGroup("Links"), SerializeField] private InputActionAsset _inputActionAsset;

    private List<ActionBasedController> _controllers = new List<ActionBasedController>();

    #region InputVariables----------------------------------------------------------------------------------------

    private InputActionMap _leftControllerMap;
    private InputActionMap _leftLocomotionMap;
    private InputActionMap _rightControllerMap;
    private InputActionMap _rightLocomotionMap;
    private InputActionMap _extendedButtonsMap;
    
    public InputAction ButtonLeftTrigger { get; private set; }
    public InputAction ButtonLeftGrip { get; private set; }
    public InputAction ButtonLeftMenu { get; private set; }
    public InputAction ButtonLeftPrimary { get; private set; }
    public InputAction ButtonLeftSecondary { get; private set; }
    public InputAction LeftJoystickAction { get; private set; }
    public InputAction LeftVelocityAction { get; private set; }
    
    public InputAction ButtonRightTrigger { get; private set; }
    public InputAction ButtonRightGrip { get; private set; }
    public InputAction ButtonRightPrimary { get; private set; }
    public InputAction ButtonRightSecondary { get; private set; }
    public InputAction RightJoystickAction { get; private set; }
    public InputAction RightVelocityAction { get; private set; }

    public bool IsLeftMenuPressed => _isLeftMenuPressed;

    #endregion

    #region OutputVariables---------------------------------------------------------------------------------------

    //Left controller
    private float _currentLeftTriggerValue;
    private bool _isLeftTriggerPressed;
    private float _currentLeftGripValue;
    private bool _isLeftGripPressed;
    private float _currentLeftMenuValue;
    private bool _isLeftMenuPressed;
    private float _currentLeftPrimaryValue;
    private bool _isLeftPrimaryPressed;
    private float _currentLeftSecondaryValue;
    private bool _isLeftSecondaryPressed;

    [HideInInspector] public Vector2 LeftJoystick;
    [HideInInspector] public Vector3 LeftVelocity;

    public Action OnLeftTriggerPressed;
    public Action OnLeftTriggerUnpressed;
    public Action OnLeftGripPressed;
    public Action OnLeftGripUnpressed;
    public Action OnLeftMenuPressed;
    public Action OnLeftMenuUnpressed;
    public Action OnLeftPrimaryPressed;
    public Action OnLeftPrimaryUnpressed;
    public Action OnLeftSecondaryPressed;
    public Action OnLeftSecondaryUnpressed;

    //Right controller
    private float _currentRightTriggerValue;
    private bool _isRightTriggerPressed;
    private float _currentRightGripValue;
    private bool _isRightGripPressed;
    private float _currentRightPrimaryValue;
    private bool _isRightPrimaryPressed;
    private float _currentRightSecondaryValue;
    private bool _isRightSecondaryPressed;
    
    [HideInInspector] public Vector2 RightJoystick;
    [HideInInspector] public Vector3 RightVelocity;
    
    public Action OnRightTriggerPressed;
    public Action OnRightTriggerUnpressed;
    public Action OnRightGripPressed;
    public Action OnRightGripUnpressed;
    public Action OnRightPrimaryPressed;
    public Action OnRightPrimaryUnpressed;
    public Action OnRightSecondaryPressed;
    public Action OnRightSecondaryUnpressed;
    
    //Both controller
    public Action OnAnyButtonPressed;
    public Action OnAnyButtonUnpressed;

    #endregion

    #region InjectServices----------------------------------------------------------------------------------------

    private SceneLoadingService _sceneLoadingService;
    
    [Inject]
    private void Construct(SceneLoadingService sceneLoadingService)
    {
        _sceneLoadingService = sceneLoadingService;
    }

    #endregion
    
    #region ProjectContextLogic-----------------------------------------------------------------------------------

    private void Init()
    {
        if (IsSystemReady) return;
        
        FindControllers();
        
        IsSystemReady = true;
    }
    
    private void EraseData()
    {
        IsSystemReady = false;
        
        _controllers.Clear();
        LeftController = null;
        RightController = null;
    }
    
    #endregion
    
    public void OnInitialize()
    {
        _sceneLoadingService.OnEndSceneLoading += Init;
        _sceneLoadingService.OnStartSceneLoading += EraseData;
        SetButtonsLinks();
        
        Debug.Log("<color=#E87CFF>[VR INPUT SYSTEM] Input system ready</color>");
    }

    private void Update()
    {
        UpdateAndSendButtonsStates();
    }

    private void FixedUpdate()
    {
        UpdateAndSendPhysBasedStates();
    }

    #region PreparingAndUpdateSystem------------------------------------------------------------------------------

    private void SetButtonsLinks()
    {
        _leftControllerMap = _inputActionAsset.FindActionMap("XRI LeftHand Interaction");
        _leftLocomotionMap = _inputActionAsset.FindActionMap("XRI LeftHand LocomotionRedact");
        _rightControllerMap = _inputActionAsset.FindActionMap("XRI RightHand Interaction");
        _rightLocomotionMap = _inputActionAsset.FindActionMap("XRI RightHand LocomotionRedact");
        _extendedButtonsMap = _inputActionAsset.FindActionMap("XRI Extended Buttons");
        
        ButtonLeftTrigger = _leftControllerMap.FindAction("Activate Value");
        ButtonLeftGrip = _leftControllerMap.FindAction("Select");
        ButtonLeftMenu = _extendedButtonsMap.FindAction("Left Menu Button");
        ButtonLeftPrimary = _extendedButtonsMap.FindAction("Left Primary Button");
        ButtonLeftSecondary = _extendedButtonsMap.FindAction("Left Secondary Button");
        LeftJoystickAction = _extendedButtonsMap.FindAction("Left Joystick");
        LeftVelocityAction = _extendedButtonsMap.FindAction("Left Velocity");
        
        ButtonRightTrigger = _rightControllerMap.FindAction("Activate Value");
        ButtonRightGrip = _rightControllerMap.FindAction("Select");
        ButtonRightPrimary = _extendedButtonsMap.FindAction("Right Primary Button");
        ButtonRightSecondary = _extendedButtonsMap.FindAction("Right Secondary Button");
        RightJoystickAction = _extendedButtonsMap.FindAction("Right Joystick");
        RightVelocityAction = _extendedButtonsMap.FindAction("Right Velocity");
    }

    private void FindControllers()
    {
        var controllers = FindObjectsOfType<ActionBasedController>();

        for (int i = 0; i < controllers.Length; i++)
        {
            _controllers.Add(controllers[i]);
            
            if (controllers[i].GetComponent<VRControllerMarker>().ControllerSide == VRController.Left)
                LeftController = controllers[i];
            else if (controllers[i].GetComponent<VRControllerMarker>().ControllerSide == VRController.Right)
                RightController = controllers[i];
        }
    }

    private void UpdateAndSendButtonsStates()
    {
        SendButtonState(ButtonLeftTrigger.ReadValue<float>(),
            ref _currentLeftTriggerValue, OnLeftTriggerPressed, OnLeftTriggerUnpressed, ref _isLeftTriggerPressed);
        SendButtonState(ButtonRightTrigger.ReadValue<float>(),
            ref _currentRightTriggerValue, OnRightTriggerPressed, OnRightTriggerUnpressed, ref _isRightTriggerPressed);
        SendButtonState(ButtonLeftGrip.ReadValue<float>(),
            ref _currentLeftGripValue, OnLeftGripPressed, OnLeftGripUnpressed, ref _isLeftGripPressed);
        SendButtonState(ButtonRightGrip.ReadValue<float>(),
            ref _currentRightGripValue, OnRightGripPressed, OnRightGripUnpressed, ref _isRightGripPressed);
        SendButtonState(ButtonLeftMenu.ReadValue<float>(),
            ref _currentLeftMenuValue, OnLeftMenuPressed, OnLeftMenuUnpressed, ref _isLeftMenuPressed);
        SendButtonState(ButtonLeftPrimary.ReadValue<float>(),
            ref _currentLeftPrimaryValue, OnLeftPrimaryPressed, OnLeftPrimaryUnpressed, ref _isLeftPrimaryPressed);
        SendButtonState(ButtonRightPrimary.ReadValue<float>(),
            ref _currentRightPrimaryValue, OnRightPrimaryPressed, OnRightPrimaryUnpressed, ref _isRightPrimaryPressed);
        SendButtonState(ButtonLeftSecondary.ReadValue<float>(),
            ref _currentLeftSecondaryValue, OnLeftSecondaryPressed, OnLeftSecondaryUnpressed, ref _isLeftSecondaryPressed);
        SendButtonState(ButtonRightSecondary.ReadValue<float>(),
            ref _currentRightSecondaryValue, OnRightSecondaryPressed, OnRightSecondaryUnpressed, ref _isRightSecondaryPressed);

        LeftJoystick = LeftJoystickAction.ReadValue<Vector2>();
        RightJoystick = RightJoystickAction.ReadValue<Vector2>();
    }

    private void UpdateAndSendPhysBasedStates()
    {
        LeftVelocity = LeftVelocityAction.ReadValue<Vector3>();
        RightVelocity = RightVelocityAction.ReadValue<Vector3>();
    }
    
    private void SendButtonState(float targetButtonValue, ref float currentButtonValue, Action onPressed, Action onUnpressed, ref bool isPressed)
    {
        if (targetButtonValue == currentButtonValue)
            return;

        if (currentButtonValue < 0.5 && targetButtonValue >= 0.5 && !isPressed)
        {
            onPressed?.Invoke();
            OnAnyButtonPressed?.Invoke();
            isPressed = true;
            //Debug.Log("PRESSED");
        }
        else if (currentButtonValue >= 0.5 && targetButtonValue < 0.5 && isPressed)
        {
            onUnpressed?.Invoke();
            OnAnyButtonUnpressed?.Invoke();
            isPressed = false;
            //Debug.Log("UNPRESSED");
        }

        currentButtonValue = targetButtonValue;
    }

    #endregion

    #region Callbacks---------------------------------------------------------------------------------------------

    public bool IsPressed(ActionBasedController controller, ControllerButton button)
    {
        if (!SetControllerFromVariable(controller))
            return false;

        switch (button)
        {
            case ControllerButton.Trigger:
                return controller.activateAction.action.IsPressed();
            case ControllerButton.Grip:
                return controller.selectAction.action.IsPressed();
            default:
                return false;
        }
    }

    #endregion
    
    #region HapticOverrides---------------------------------------------------------------------------------------

    public bool SendHapticImpulse(float amplitude, float duration, ActionBasedController controller)
    {
        if (!SetControllerFromVariable(controller))
            return false;

        return PlayHapticImpulse(amplitude, duration, controller);
    }
    
    public bool SendHapticImpulse(float amplitude, float duration, VRController enumController)
    {
        ActionBasedController controller;
        
        if (SetControllerFromEnum(enumController) != null)
            controller = SetControllerFromEnum(enumController);
        else
            return false;
        
        return PlayHapticImpulse(amplitude, duration, controller);
    }

    private bool PlayHapticImpulse(float amplitude, float duration, ActionBasedController controller)
    {
#if ENABLE_VR || (UNITY_GAMECORE && INPUT_SYSTEM_1_4_OR_NEWER)
        if (controller.hapticDeviceAction.action?.activeControl?.device is XRControllerWithRumble rumbleController)
        {
            rumbleController.SendImpulse(amplitude, duration);
            return true;
        }
#endif

        return false;
    }

    #endregion

    private bool SetControllerFromVariable(ActionBasedController controller)
    {
        if (controller == null)
            return false;
        
        for (int i = 0; i < _controllers.Count; i++)
        {
            if (controller == _controllers[i])
                return true;
        }

        return false;
    }
    
    private ActionBasedController SetControllerFromEnum(VRController controller)
    {
        switch (controller)
        {
            case VRController.Left:
                return LeftController;
            case VRController.Right:
                return RightController;
            default:
                return null;
        }
    }
}
