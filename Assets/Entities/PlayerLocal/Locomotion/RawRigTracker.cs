using NewFolder.VR.Locomotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ND_Locomotion.Scripts.Locomotion
{
    public class RawRigTracker: MonoBehaviour
    {
        [TabGroup("Info"), ReadOnly] public Vector3 PositionOffset;
        [TabGroup("Info"), ReadOnly] public Vector3 RotationOffset;
        
        [TabGroup("Links"), SerializeField] private LocomotionRig locomotionRig;
        [TabGroup("Links"), SerializeField] private InputActionReference headPosition, headRotation;
        [TabGroup("Links"), SerializeField] private InputActionReference leftHandPosition, leftHandRotation;
        [TabGroup("Links"), SerializeField] private InputActionReference rightHandPosition, rightHandRotation;
        
        [Space, Header("Settings")]
        public bool IsControllersTracking = true;
        
        private float _heightOffset;
        private bool _isInReset;
        

        private void Start()
        {
            headPosition.action.Enable();
            headRotation.action.Enable();
            leftHandPosition.action.Enable();
            leftHandRotation.action.Enable();
            rightHandPosition.action.Enable();
            rightHandRotation.action.Enable();
            
            var startPosition = headPosition.action.ReadValue<Vector3>();
            _heightOffset = -startPosition.y + PositionOffset.y;
        }
        
        private void Update()
        {
            SetOffset();
        }

        private void OnEnable()
        {
            InputSystem.onAfterUpdate += FollowRig;
        }
        
        private void OnDisable()
        {
            InputSystem.onAfterUpdate -= FollowRig;
        }

        private void FollowRig()
        {
            if (!enabled) return;
            var hmdLocalPosition = headPosition.action.ReadValue<Vector3>();
            var offset = Vector3.up * _heightOffset; 
            
            locomotionRig.RawInput.Headset.localPosition = hmdLocalPosition + offset;
            locomotionRig.RawInput.Headset.localRotation = headRotation.action.ReadValue<Quaternion>();
            locomotionRig.RawInput.Headset.localRotation = Quaternion.Euler(locomotionRig.RawInput.Headset.localRotation.eulerAngles + RotationOffset);

            if (IsControllersTracking)
            {
                locomotionRig.RawInput.LeftController.localPosition = leftHandPosition.action.ReadValue<Vector3>() + offset;
                locomotionRig.RawInput.LeftController.localRotation = leftHandRotation.action.ReadValue<Quaternion>();
                //locomotionRig.RawInput.LeftHand.localRotation = Quaternion.Euler(locomotionRig.RawInput.LeftHand.localRotation.eulerAngles + RotationOffset);

                locomotionRig.RawInput.RightController.localPosition = rightHandPosition.action.ReadValue<Vector3>() + offset;
                locomotionRig.RawInput.RightController.localRotation = rightHandRotation.action.ReadValue<Quaternion>();
                //locomotionRig.RawInput.RightHand.localRotation = Quaternion.Euler(locomotionRig.RawInput.RightHand.localRotation.eulerAngles + RotationOffset);
            }

            _heightOffset = Mathf.Lerp(_heightOffset, -hmdLocalPosition.y + PositionOffset.y,
                10 * Time.fixedDeltaTime);
        }
        
        private void SetOffset()
        {
            PositionOffset.y = locomotionRig.BodyCollider.height * 0.5f;
        }
        
        // [Button]
        // public void ResetHead()
        // {
        //     StartCoroutine(ResetCoroutine());
        //     
        //     var hmdLocalPosition = Vector3.zero;
        //     var offset = Vector3.up * _heightOffset;
        //     
        //     locomotionRig.RawInput.Head.localPosition = hmdLocalPosition + offset;
        //     locomotionRig.RawInput.Head.localRotation = Quaternion.identity;
        // }
        //
        // private IEnumerator ResetCoroutine()
        // {
        //     _isInReset = true;
        //     yield return new WaitForSeconds(2);
        //     _isInReset = false;
        // }
    }
}