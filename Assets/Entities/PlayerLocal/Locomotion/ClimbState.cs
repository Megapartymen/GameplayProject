using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ClimbState : LocomotionState, IClimbState
{
    [SerializeField, Range(0, 1)] private float climbVelocityMultiplier = 1f;
    [SerializeField, Range(0, 1)] private float maxClimbZVelocity = 0.2f;
    [SerializeField, Range(0, 1)] private float climbXVelocityPower = 0.75f;
    [SerializeField, Range(0, 3)] private float climbYVelocityPower = 1.15f;
    [SerializeField] private bool clampYSpeed = false;

    [SerializeField, ShowIf("clampYSpeed")]
    private float maxClimbYSpeed = 8;

    [SerializeField] private float climbPastStateMaxWait = 0.4f;
    [SerializeField] private float climbingDrag = 2f;
    [SerializeField, Range(0, 1)] private float fallPower = 0.8f;
    
    private HeightChanger _heightChanger;
    private float _climbPastStateTimer;
    private bool _magnitezeClimb;

    public bool ClimbingWasActive { get; private set; }
    public Vector3 LastClimbDirection { get; private set; }
    public float TimeSinceLastClimb => _climbPastStateTimer;

    public override Type GetStateType() => typeof(IClimbState);

    public override void Initialize(Locomotion locomotion)
    {
        base.Initialize(locomotion);
        
        _heightChanger = FindObjectOfType<HeightChanger>();
    }

    protected override bool CanEnterState(IList<MoveData> moveData)
    {
        if (ClimbingWasActive)
        {
            if (_climbPastStateTimer < climbPastStateMaxWait)
                _climbPastStateTimer += Time.fixedDeltaTime;
            else
                ClimbingWasActive = false;
        }
        
        var isClimbing = false;
        var magnitize = false;
        for (var i = 0; i < moveData.Count; i++)
        {
            var data = moveData[i];

            var surfaceNormal = data.normal;

            isClimbing = Mathf.Abs(Vector3.Dot(Locomotion.BodyForwardDirection, surfaceNormal)) >= 0.6f &&
                         Mathf.Abs(Vector3.Dot(Vector3.up, Locomotion.BodyForwardDirection)) < 0.8f;

            if (isClimbing)
            {
                ClimbingWasActive = true;
                _climbPastStateTimer = 0f;

                _heightChanger.IsHeightByHead = false;
            }
        }

        _magnitezeClimb = magnitize;

        return isClimbing;
    }

    protected override void OnUpdate(IList<MoveData> moveData)
    {
        Locomotion.SetGravity(Locomotion.DefaultGravity * (climbYVelocityPower * fallPower));

        var climbDirection = Vector3.zero;
 
        foreach (var data in moveData)
        {
            var velocity = Vector3.ProjectOnPlane(data.mixedVelocity, data.normal) * climbVelocityMultiplier;

            Vector3 normalizedVelocity = velocity.normalized;
            Vector3 normalizedLookDirection = -data.normal;
            Vector3 sideVector = Vector3.Cross(normalizedVelocity, normalizedLookDirection);
            Vector3 upVector = Vector3.Cross(sideVector, normalizedVelocity);
            Quaternion coordinateSystem = Quaternion.LookRotation(normalizedLookDirection, upVector);
            Vector3 velocityInNewSystem = Quaternion.Inverse(coordinateSystem) * normalizedVelocity;

            velocityInNewSystem.z = 0;
            velocityInNewSystem.x *= climbXVelocityPower;
            Vector3 velocityInOriginalSystem = coordinateSystem * velocityInNewSystem;

            velocity = (velocityInOriginalSystem * velocity.magnitude);
            velocity.y *= climbYVelocityPower;
            if (clampYSpeed)
                velocity.y = Mathf.Clamp(velocity.y, 0, maxClimbYSpeed);

            if (_magnitezeClimb)
                climbDirection += -data.normal;

            Locomotion.AddVelocity(velocity, data.realHandTransform.handSide);
        }

        LastClimbDirection = climbDirection / moveData.Count;
    }

    public override void OnExit()
    {
        Locomotion.RestoreGravity();
        _heightChanger.IsHeightByHead = true;
    }
}