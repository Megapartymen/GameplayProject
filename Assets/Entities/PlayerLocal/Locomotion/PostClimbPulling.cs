using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PostClimbPulling : LocomotionState, IPostClimbPullingState
{
    [SerializeField] private float pullingSpeed;
    [SerializeField] private float pullingForce;
    [SerializeField] private float postClimbDrag = 5f;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float velocityChangeSpeed = 15;
    [SerializeField] private float climbHeightMultiplier = 1;
    [SerializeField] private float maxDistanceToSurface = 0.36f;

    private IClimbState _climbingState;
    private IJumpState _jumpState;

    private Vector3 _lastDetectedWallPoint;
    private float _lastDetectedRadius;
    private Vector3 _targetLandPosition;
    private float _travelTime;
    private float _lastVelocityMagnitude;
    private float _velocityMultiplier;

    public override Type GetStateType() => typeof(IPostClimbPullingState);

    public override void Initialize(Locomotion locomotion)
    {
        base.Initialize(locomotion);
        _climbingState = Locomotion.GetState<IClimbState>();
        _jumpState = Locomotion.GetState<IJumpState>();
        maxDistanceToSurface /= Locomotion.ScaleFactor;
    }

    protected override bool CanEnterState(IList<MoveData> moveData)
    {
        var ray = new Ray(Locomotion.ChestPosition, _climbingState.LastClimbDirection);
        var raycastDistance = (maxDistanceToSurface + Physics.defaultContactOffset) * 2;
        var detectedHit = Physics.SphereCast(ray, maxDistanceToSurface / 3, out var hit,
            raycastDistance, groundLayerMask);

        if (!IsCanEnterOrActive && detectedHit)
        {
            _lastDetectedWallPoint = hit.point;
            _lastDetectedRadius = Vector3.Distance(Locomotion.ColliderPosition, _lastDetectedWallPoint) *
                                  climbHeightMultiplier;
            var direction = (Locomotion.LocomotionRig.Head.forward + _climbingState.LastClimbDirection) / 2f;
            direction.y = 0f;
            _targetLandPosition =
                _lastDetectedWallPoint + direction * _lastDetectedRadius;
            _travelTime = 0f;
            _lastVelocityMagnitude = Locomotion.PlayerRigidbody.velocity.magnitude;
            _velocityMultiplier = Locomotion.PlayerRigidbody.velocity.magnitude;
        }

        var hasSurfaceForPullUp = false;

        if (!detectedHit)
        {
            var surfaceTestRay = new Ray(_targetLandPosition + Vector3.up * 0.01f, Vector3.down);
            //var checkDistance = Mathf.Max(Locomotion.Rigidbody.velocity.y/Locomotion.ScaleFactor, raycastDistance);
            hasSurfaceForPullUp = Physics.Raycast(surfaceTestRay, out hit, raycastDistance, groundLayerMask) &&
                                  Vector3.Dot(hit.normal, Vector3.up) > 0.75f;
        }

        if (IsCanEnterOrActive)
        {
            return _travelTime <= 1f && !Locomotion.IsGrounded &&
                   !_climbingState.IsCanEnterOrActive &&
                   !_jumpState.IsJumped;
        }

        var needToClimb = !Locomotion.IsGrounded && _velocityMultiplier > 0 && hasSurfaceForPullUp &&
                          !_climbingState.IsCanEnterOrActive &&
                          _climbingState.ClimbingWasActive && !_jumpState.IsJumped;
        return needToClimb;
    }

    protected override void OnUpdate(IList<MoveData> moveData)
    {
        Locomotion.SetDrag(postClimbDrag);
        Locomotion.UseGravity = false;
        Locomotion.ColliderOffset = Vector3.up * 1;
        _travelTime += Time.fixedDeltaTime * pullingSpeed * _velocityMultiplier;

        var travelPosition = CalculateArcPosition(Locomotion.GroundPoint, _targetLandPosition,
            _lastDetectedRadius, _travelTime);

        var direction = (travelPosition - Locomotion.GroundPoint);
        Locomotion.PlayerRigidbody.velocity = Vector3.Lerp(Locomotion.PlayerRigidbody.velocity,
            direction * (pullingForce * _lastVelocityMagnitude), velocityChangeSpeed * Time.fixedDeltaTime);
    }

    private Vector3 CalculateArcPosition(Vector3 start, Vector3 end, float height, float t)
    {
        var mStart = start;
        mStart.y = end.y;
        var p1 = Vector3.Lerp(mStart, end, 0.35f) + Vector3.up * height;
        var p2 = Vector3.Lerp(mStart, end, 0.7f) + Vector3.up * height;
        
        return CalculateBezier(start, p1, p2, end, t);
    }

    public static Vector3 CalculateBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        t = math.clamp(t, 0, 1);
        float tr = 1 - t;
        return tr * tr * tr * p0 + 3 * tr * tr * t * p1 + 3 * tr * t * t * p2 + t * t * t * p3;
    }

    public override void OnExit()
    {
        Locomotion.UseGravity = true;
        Locomotion.RestoreDrag();
        Locomotion.ColliderOffset = Vector3.zero;
    }
}