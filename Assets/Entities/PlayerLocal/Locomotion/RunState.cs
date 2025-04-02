using System;
using System.Collections.Generic;
using UnityEngine;

public class RunState : LocomotionState, IRunState
{
    [SerializeField] private float _powerMultiplier = 1;
    [SerializeField] private LocomotionRig _locomotionRig;

    private IJumpState _jumpState;
    private IFallState _fallState;
    private IClimbState _climbingState;

    public override Type GetStateType() => typeof(IRunState);

    public float PowerMultiplier
    {
        get => _powerMultiplier;
        set => _powerMultiplier = value;
    }

    public override void Initialize(Locomotion locomotion)
    {
        base.Initialize(locomotion);

        _jumpState = locomotion.GetState<IJumpState>();
        _climbingState = locomotion.GetState<IClimbState>();
        _fallState = locomotion.GetState<IFallState>();
    }

    protected override bool CanEnterState(IList<MoveData> moveData)
    {
        bool canEnter = false;

        for (var i = 0; i < moveData.Count; i++)
        {
            var data = moveData[i];

            if (data.collidedObject.TryGetComponent<RunningDetectorMarker>(out _))
            {
                canEnter = !_jumpState.IsCanEnterOrActive
                           && !_jumpState.IsJumped
                           && !_climbingState.IsCanEnterOrActive
                           && !_climbingState.ClimbingWasActive
                           && !_fallState.IsCanEnterOrActive;

                break;
            }
        }

        return canEnter;
    }

    protected override void OnUpdate(IList<MoveData> moveData)
    {
        for (var i = 0; i < moveData.Count; i++)
        {
            var data = moveData[i];
            Locomotion.AddVelocity(_locomotionRig.BodyLocomotion.forward.normalized * (data.speed * PowerMultiplier),
                data.realHandTransform.handSide);
        }
    }
}