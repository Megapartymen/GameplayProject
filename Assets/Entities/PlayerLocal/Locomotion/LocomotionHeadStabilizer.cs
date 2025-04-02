using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace NewFolder.VR.Locomotion
{
    public class LocomotionHeadStabilizer : MonoBehaviour // Script that freezing head position on needed Y position.
    {
        [SerializeField] private LocomotionRig locomotionRig;
        
        private void Update()
        {
            var hmdLocalPosition = locomotionRig.RawInput.Headset.localPosition;
            var hmdLocalRotation = locomotionRig.RawInput.Headset.localRotation;
            
            // locomotionRig.Head.SetLocalPositionAndRotation(hmdLocalPosition, hmdLocalRotation);
            
            locomotionRig.Head.localPosition = hmdLocalPosition;
            locomotionRig.Head.localRotation = hmdLocalRotation;
        }
    }
}