using System;
using System.Collections;
using ND_Locomotion.Scripts.Locomotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Zenject;

public class PlayerTransformManipulator : MonoBehaviour
{
    [TabGroup("Links"), SerializeField] private Transform _playerCenter;
    [TabGroup("Links"), SerializeField] private Transform[] _trackingSpaces;
    [Space] 
    [TabGroup("Links"), SerializeField] private Rigidbody _playerBody;
    [TabGroup("Links"), SerializeField] private Collider[] _playerBodyColliders;

    private RawRigTracker _rawRigTracker;
    private Locomotion _locomotion;
    private float thumbstickOffset = 0.6f;
    private float snapTurningAngle = 30;
    private bool _thumbstickPressed;

    private VRInputSystem _vrInputSystem;
    private Transform _teleportationTarget;
    private bool _isFreeze;
    private bool _isFade;

    public bool IgnoreTurn;

    [Inject]
    private void Construct(VRInputSystem vrInputSystem, PersistentUpdateService persistentUpdateService)
    {
        _vrInputSystem = vrInputSystem;
    }

    private void Awake()
    {
        _rawRigTracker = FindObjectOfType<RawRigTracker>();
        _locomotion = FindObjectOfType<Locomotion>();
    }

    private void Update()
    {
        if (IgnoreTurn)
            return;

        if (_vrInputSystem.RightJoystick.x > thumbstickOffset && !_thumbstickPressed)
        {
            AddRotation(snapTurningAngle);
            _thumbstickPressed = true;
        }
        else if (_vrInputSystem.RightJoystick.x < -thumbstickOffset && !_thumbstickPressed)
        {
            AddRotation(-snapTurningAngle);
            _thumbstickPressed = true;
        }
        else if (_vrInputSystem.RightJoystick.x <= thumbstickOffset &&
                 _vrInputSystem.RightJoystick.x >= -thumbstickOffset)
        {
            _thumbstickPressed = false;
        }
    }

    private void AddRotation(float angle)
    {
        StartCoroutine(AddRotationCoroutine(angle));
    }

    private IEnumerator AddRotationCoroutine(float angle)
    {
        foreach (var trackingSpace in _trackingSpaces)
        {
            trackingSpace.RotateAround(_playerCenter.position, Vector3.up, angle);
        }

        yield return null;
    }

    [Button]
    private void ResetPosition()
    {
        Vector3 positionDifference = _trackingSpaces[0].position - _playerCenter.position;
        Vector3 cameraCorrect = positionDifference + _rawRigTracker.PositionOffset * 0.5f;

        foreach (var trackingSpace in _trackingSpaces)
        {
            trackingSpace.position += cameraCorrect;
        }
    }

    [Button]
    private void ResetRotation()
    {
        float rotationYDifference = _trackingSpaces[0].rotation.eulerAngles.y - _playerCenter.rotation.eulerAngles.y;
        AddRotation(rotationYDifference);
    }

    [Button]
    public void ResetPositionAndRotation()
    {
        ResetPosition();
        ResetRotation();
    }

    [Button]
    public void TeleportTo(Transform target, float delay = 0, bool isFreeze = false, bool isFade = false,
        float fadeDuration = 0.5f)
    {
        StartCoroutine(TeleportToCoroutine(target, delay, isFreeze, isFade, fadeDuration));
    }

    private IEnumerator TeleportToCoroutine(Transform target, float delay, bool isFreeze, bool isfade,
        float fadeDuration)
    {
        yield return new WaitForSeconds(delay);

        if (isfade)
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        _locomotion.SetLocomotionActivity(false);

        yield return null;

        _teleportationTarget = target;
        _isFreeze = isFreeze;
        _isFade = isfade;

        yield return new WaitForEndOfFrame();
        TeleportTransformLogic();
    }

    private IEnumerator PostTeleportCoroutine(bool isFreeze, bool isFade)
    {
        yield return null;

        _locomotion.SetLocomotionActivity(true);

        if (isFreeze) _playerBody.isKinematic = true;
    }

    private void TeleportTransformLogic()
    {
        foreach (var trackingSpace in _trackingSpaces)
        {
            trackingSpace.position = _teleportationTarget.position;
            trackingSpace.rotation = _teleportationTarget.rotation;
        }

        ResetPosition();
        ResetRotation();
        StartCoroutine(PostTeleportCoroutine(_isFreeze, _isFade));
    }
}