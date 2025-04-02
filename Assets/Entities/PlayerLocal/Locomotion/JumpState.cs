using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpState : LocomotionState, IJumpState
    {
        [SerializeField] private LayerMask surfaceMask;
        [SerializeField] private float twoHandJumpDelay = 0.1f;
        [SerializeField] private float minHandVelocityForJump = 0.25f;
        [SerializeField] private float minHandSpeedForJump = 0.25f;
        [SerializeField] private float defaultJumpImpulseMultiplier = 0.75f;
        [SerializeField] private float correctedJumpImpulseMultiplier = 1f;
        [SerializeField] private float slowDownCooldown = 0.5f;
        [SerializeField] private float jumpDrag = 0.2f;
        [SerializeField, Range(0, 1)] private float postJumpLandBreak = 0.25f;
        [SerializeField] private PhysicMaterial playerDefaultMaterial, playerBrakeMaterial;
        [SerializeField] private LineRenderer trajectoryLine;

        private float _jumpLastTime;
        private float _hitLastTimeLeft;
        private float _hitLastTimeRight;
        private float _jumpSpeedLeft;
        private float _jumpSpeedRight;
        private Transform _lastSurfaceObject;
        
        public bool IsJumped { get; private set; }
        public bool CanJump { get; set; } = true;
        public float JumpSinceLastTime => _jumpLastTime;
        public float JumpForce { get; private set; }

        public override Type GetStateType() => typeof(IJumpState);
        
        private Coroutine _landAfterJumpCoroutine;

        public override void Initialize(Locomotion locomotion)
        {
            base.Initialize(locomotion);

            Locomotion.LocomotionRig.LeftHandLocomotion.OnHandHit += OnHandLocomotionHit;
            Locomotion.LocomotionRig.RightHandLocomotion.OnHandHit += OnHandLocomotionHit;
        }

        private void OnHandLocomotionHit(MoveData moveData)
        {
            if (moveData.collidedObject.TryGetComponent<JumpingDetectorMarker>(out _))
            {
                var velocity = moveData.rawLocalVelocity;
                if (moveData.realHandTransform.handSide == VRController.Left)
                {
                    _hitLastTimeLeft = Time.time;
                    _jumpSpeedLeft = moveData.speed;
                }
                else
                {
                    _hitLastTimeRight = Time.time;
                    _jumpSpeedRight = moveData.speed;
                }
            }
        }

        protected override bool CanEnterState(IList<MoveData> moveData)
        {
            if (IsJumped && Time.time - _jumpLastTime > slowDownCooldown)
            {
                bool isHandTouchWalkable = false;
                
                foreach (var data in moveData)
                {
                    if (data.collidedObject.gameObject.layer == LayerMask.NameToLayer("Walkable"))
                    {
                        isHandTouchWalkable = true;
                        break;
                    }
                }
                
                Locomotion.SetDrag(jumpDrag);
                Locomotion.SetGravity(Physics.gravity / 2f);
                
                if (isHandTouchWalkable || Locomotion.IsGrounded)
                {
                    Debug.Log($"{isHandTouchWalkable} | {Locomotion.IsGrounded}");
                    
                    if (_landAfterJumpCoroutine != null)
                        StopCoroutine(LandAfterJump());
                    _landAfterJumpCoroutine = StartCoroutine(LandAfterJump());
            
                    if (isHandTouchWalkable)
                    {
                        Locomotion.PlayerRigidbody.velocity = -Locomotion.Gravity * Time.fixedDeltaTime;
                    }
                }
                
                return false;
            }

            var willJump = CanJump && !IsJumped && 
                           Mathf.Abs(_hitLastTimeLeft - _hitLastTimeRight) < twoHandJumpDelay && 
                           Time.time - _hitLastTimeLeft < twoHandJumpDelay && 
                           Time.time - _hitLastTimeRight < twoHandJumpDelay && 
                           _jumpSpeedRight > minHandSpeedForJump && 
                           _jumpSpeedLeft > minHandSpeedForJump;
            
            return willJump;
        }

        protected override void OnUpdate(IList<MoveData> moveData)
        {
            Locomotion.SetDrag(jumpDrag);
            Locomotion.SetGravity(Physics.gravity / 2f);
            
            var finalJumpForce = (_jumpSpeedLeft + _jumpSpeedRight) / 2f;
            
            Locomotion.PlayerRigidbody.AddForce(Vector3.up * finalJumpForce, ForceMode.Impulse);
            
            JumpForce = finalJumpForce;
            
            if (!IsJumped)
                _jumpLastTime = Time.time;

            IsJumped = true;
            Locomotion.BodyCollider.sharedMaterial = playerBrakeMaterial;
            Locomotion.LocomotionRig.LeftHandLocomotion.XRController.SendHapticImpulse(0.15f, 0.1f);
            Locomotion.LocomotionRig.RightHandLocomotion.XRController.SendHapticImpulse(0.15f, 0.1f);
        }
        
        public override void OnReset()
        {
            base.OnReset();
            _jumpLastTime = 0;
            _hitLastTimeLeft = 0;
            _hitLastTimeRight = 0;
        }
        
        private IEnumerator LandAfterJump(bool immediate = false)
        {
            Locomotion.RestoreDrag();
            Locomotion.RestoreGravity();
            IsJumped = false;
            Locomotion.PlayerRigidbody.velocity *= 1 - postJumpLandBreak;
        
            if (!immediate)
                yield return new WaitForSeconds(0.25f);
            Locomotion.BodyCollider.sharedMaterial = playerDefaultMaterial;
        }
    }