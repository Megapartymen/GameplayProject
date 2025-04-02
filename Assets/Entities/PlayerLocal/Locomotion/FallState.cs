using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FallState : LocomotionState, IFallState
    {
        [SerializeField] private HeightChanger _heightChanger;
        
        private IJumpState _jumpState;
        private IClimbState _climbState;
        private Vector3 _defaultGravity;
        private float _previousLandTime;
        [SerializeField] private bool usePause;

        public override Type GetStateType() =>
            typeof(IFallState);

        public override void Initialize(Locomotion locomotion)
        {
            base.Initialize(locomotion);
            _jumpState = locomotion.GetState<IJumpState>();
            _climbState = locomotion.GetState<IClimbState>();
            _defaultGravity = Locomotion.Gravity;
            locomotion.OnCollision += OnLanded;
        }

        private void OnLanded(Collision collision)
        {
            Debug.Log($"Collision: {collision.gameObject.name} | Layer: {LayerMask.LayerToName(collision.gameObject.layer)}");
            
            if (collision.gameObject.layer != LayerMask.NameToLayer("Walkable"))
            {
                return;
            }
            
            var isReadyToLand = _jumpState != null
                                && _climbState != null
                                && !_jumpState.IsCanEnterOrActive
                                && !_climbState.IsCanEnterOrActive;
            
            if (isReadyToLand)
            {
                Locomotion.RestoreDrag();
                Locomotion.RestoreGravity();
                _heightChanger.AnimateLand(Locomotion.LastFrameNonZeroVelocity, _previousLandTime);
                
                Debug.Log("Landed");
            }
            
            _previousLandTime = Time.time;
        }

        protected override bool CanEnterState(IList<MoveData> moveData)
        {
            var isFalling = !Locomotion.IsGrounded
                            && _jumpState != null
                            && _climbState != null
                            && !_jumpState.IsCanEnterOrActive
                            && !_jumpState.IsJumped
                            && !_climbState.ClimbingWasActive;
            
            if (isFalling && usePause) Debug.Break();
            
            return isFalling;
        }

        protected override void OnUpdate(IList<MoveData> moveData)
        {
            Locomotion.SetDrag(0.2f);
            Locomotion.SetGravity(_defaultGravity * 2);
        }
        
        public override void OnExit()
        {
            Locomotion.RestoreDrag();
            Locomotion.RestoreGravity();
        }
    }