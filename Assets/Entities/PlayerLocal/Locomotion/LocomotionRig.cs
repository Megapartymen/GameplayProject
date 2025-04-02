using System;
using System.Collections;
using System.Collections.Generic;
using NewFolder.VR.Locomotion;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class LocomotionRig : MonoBehaviour
{
    [Serializable]
    public class RawRigInput
    {
        public Transform Headset;
        public Transform LeftController;
        public Transform RightController;
    }
        
    [SerializeField, FoldoutGroup("RawRig input", true)] private RawRigInput _rawRigInput;

    [TabGroup("Links"), SerializeField] private Transform _headTransform;
    [TabGroup("Links"), SerializeField] private Transform _headCenter;
    [TabGroup("Links"), SerializeField] private Transform _headLookPoint;
    [TabGroup("Links"), SerializeField] private Transform _bodyLocomotion;
    [TabGroup("Links"), SerializeField] private Transform _trackingSpaceTransform;
    [TabGroup("Links"), SerializeField] private LocomotionHand _leftHandLocomotion;
    [TabGroup("Links"), SerializeField] private LocomotionHand _rightHandLocomotion;
    [TabGroup("Links"), SerializeField] private VisualHandPID _leftPid;
    [TabGroup("Links"), SerializeField] private VisualHandPID _rightPid;
    [TabGroup("Links"), SerializeField] private CapsuleCollider _bodyCollider;
    [TabGroup("Links"), SerializeField] private Transform _groundPoint;
        
    public Transform Head => _headTransform;
    public Transform HeadCenter => _headCenter;
    public Transform HeadLookPoint => _headLookPoint;
    public Transform BodyLocomotion => _bodyLocomotion;
    public Transform TrackingSpace => _trackingSpaceTransform;
    public LocomotionHand LeftHandLocomotion => _leftHandLocomotion;
    public LocomotionHand RightHandLocomotion => _rightHandLocomotion;
    public VisualHandPID LeftPid => _leftPid;
    public VisualHandPID RightPid => _rightPid;
    public CapsuleCollider BodyCollider => _bodyCollider;
    public RawRigInput RawInput => _rawRigInput;
    public Transform GroundPoint => _groundPoint;

    public LocomotionHand GetHand(VRController hand)
    {
        switch (hand)
        {
            case VRController.Left:
                return LeftHandLocomotion;
            case VRController.Right:
                return RightHandLocomotion;
            default:
                return null;
        }
    }
}