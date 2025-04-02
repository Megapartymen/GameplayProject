using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Serialization;

public class Locomotion : MonoBehaviour, ILocomotion
{
    [SerializeField, TabGroup("States"), ListDrawerSettings(ShowFoldout = false)]
    private LocomotionState[] states;

    [TabGroup("Links"), SerializeField] private LocomotionRig _locomotionRig;
    [TabGroup("Links"), SerializeField] private Rigidbody _playerRigidbody;
    [TabGroup("Links"), SerializeField] private Speedometer _playerSpeedometer;
    [TabGroup("Links"), SerializeField] private LocomotionBody _locomotionBody;

    [Space, Header("Settings")] [SerializeField]
    private float distanceToFloor = 0.2f;

    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float velocityLimitHorizontal = 9.25f, velocityLimitVertical = 8;
    [SerializeField] private float minHandSpeed = 0.7f;
    [SerializeField] private float forceMultiplier = 13;
    [SerializeField, Range(0f, 1f)] private float brakePower = 0.1f;
    [SerializeField] private bool bodyIsKinematic;
    [SerializeField] private bool isBodyFollowingLinear;
    [SerializeField] private bool setToClosestSurfaceOnStart = true;
    [SerializeField] private float bodyColliderXZOffset = 0.08f;
    [SerializeField] private Vector3 _gravity = new Vector3(0, -2.4525f, 0);

    private Dictionary<Type, ILocomotionState> _typedStates;
    private List<MoveData> _currentMoveData;
    private Vector3 _leftHandTotalVelocity;
    private Vector3 _rightHandTotalVelocity;
    private List<LocomotionState> _exitedStates; // non active since last frame
    private Vector3 _startFramePosition;
    private bool _needToChangeGroundedState;
    private Collider[] _groundCheckResults;
    private Vector3 _groundCheckPosition;

    public LocomotionRig LocomotionRig => _locomotionRig;
    public Rigidbody PlayerRigidbody => _playerRigidbody;
    public CapsuleCollider BodyCollider => LocomotionRig.BodyCollider;
    public float ScaleFactor => 1;
    public float ScaledColliderHeight => BodyCollider.height / ScaleFactor;
    public float ScaledColliderRadius => BodyCollider.radius / ScaleFactor;

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public bool UseGravity { get; set; } = true;

    public Vector3 Gravity => _gravity;

    private Collider[] _allColliders;
    private Rigidbody[] _allRigidbodies;
    private Vector3 _lastFrameActiveVelocity;
    private float _defaultDrag;
    private Vector3 _defaultGravity;
    private Vector3 _lastFrameNonZeroVelocity;

    private ProfilerMarker _updateInternalMarker;
    private ProfilerMarker _updateStatesMarker;
    private ProfilerMarker _updateVelocityMarker;

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public bool IsActive { get; private set; }

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public bool IsGrounded { get; private set; }

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public Vector3 GroundPoint { get; private set; } // Lower point of body that touches the ground

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public Vector3 GroundNormal { get; private set; }

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public float LandLastTime { get; private set; }

    [field: SerializeField, ReadOnly, TabGroup("Debug")]
    public Vector3 BodyForwardDirection { get; private set; } // Forward direction of body based on hands location 

    public float VelocityLimitHorizontal => velocityLimitHorizontal;
    public float VelocityLimitVertical => velocityLimitVertical;
    public Vector3 LastFrameActiveVelocity => _lastFrameActiveVelocity;
    public Vector3 LastFrameNonZeroVelocity => _lastFrameNonZeroVelocity;
    public Vector3 ColliderPosition => transform.TransformPoint(BodyCollider.center);

    public Vector3 ChestPosition =>
        transform.TransformPoint(BodyCollider.center + Vector3.up * BodyCollider.center.y * 0.5f);

    public Vector3 DefaultGravity => _defaultGravity;
    [field: SerializeField] public Vector3 ColliderOffset { get; set; }
    public float HandsRadiusMultiplier { get; set; } = 1f;
    public bool BellyColliderLocked { get; set; }

    public Speedometer PlayerSpeedometer => _playerSpeedometer;
    public Dictionary<Type, ILocomotionState> TypedLocomotionStates => _typedStates;
    public bool IsMoving => _playerRigidbody.velocity.sqrMagnitude > 0.0001f;

    public event Action<Collision> OnCollision = (_) => { };

    #region UnityCoprograms----------------------------------------------------------------------------------------

    private void Awake()
    {
        _typedStates = new Dictionary<Type, ILocomotionState>();
        foreach (var state in states)
            _typedStates.Add(state.GetStateType(), state);

        foreach (var state in states)
            state.Initialize(this);

        _currentMoveData = new List<MoveData>(2);

        _locomotionRig.LeftHandLocomotion.OnHandMove += RecordMoveData;
        _locomotionRig.RightHandLocomotion.OnHandMove += RecordMoveData;

        _exitedStates = new List<LocomotionState>(_typedStates.Count);
        _groundCheckResults = new Collider[1];

        _allColliders = transform.parent.GetComponentsInChildren<Collider>();
        _allRigidbodies = transform.parent.GetComponentsInChildren<Rigidbody>();

        _defaultDrag = PlayerRigidbody.drag;
        _defaultGravity = _gravity;
        
        SetLocomotionActivity(true);
    }


    private void FixedUpdate()
    {
        if (!IsActive) return;
        UpdateInternal();
        UpdateStates();
        UpdateVelocity();
    }

    private void Update()
    {
        _lastFrameActiveVelocity = PlayerRigidbody.velocity;
        if (_lastFrameActiveVelocity.magnitude > 0.1f)
            _lastFrameNonZeroVelocity = _lastFrameActiveVelocity;

        _locomotionBody.RotateBodyToDirection(GetCrossPointOfControllers());
        _locomotionBody.SetBodyPosition(_locomotionRig.Head.position, transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnCollision.Invoke(collision);
    }

    private void OnCollisionStay(Collision collision)
    {
        var pair = collision.GetContact(0);
        GroundNormal = pair.normal;
    }

    #endregion

    public void SetLocomotionActivity(bool active)
    {
        if (IsActive && !active)
        {
            _lastFrameActiveVelocity = PlayerRigidbody.velocity;
        }
        else
        {
            PlayerRigidbody.velocity = _lastFrameActiveVelocity;
        }

        IsActive = active;
        foreach (var collider in _allColliders) collider.enabled = IsActive;
        foreach (var rigidbody in _allRigidbodies) rigidbody.isKinematic = !IsActive;
        _playerRigidbody.isKinematic = bodyIsKinematic;
        isBodyFollowingLinear = IsActive;
    }

    private void AlignTransformByAnchor(Transform transform, Transform anchor, Transform target,
        out Vector3 position, out Quaternion rotation)
    {
        Quaternion rotationOffset = Quaternion.Inverse(anchor.rotation * Quaternion.Inverse(target.rotation));
        Vector3 positionOffset = transform.position - anchor.position; // -7.4?;
        position = target.position + rotationOffset * positionOffset;
        rotation = rotationOffset * transform.rotation;
    }

    private void RecordMoveData(MoveData moveData)
    {
        _currentMoveData.Add(moveData);
    }

    private void UpdateInternal() // Recalculates internal properties
    {
        _updateInternalMarker.Begin();
        UpdateColliders();

        GroundPoint = new Vector3(_locomotionRig.Head.position.x,
            transform.position.y - ((ScaledColliderHeight * 0.5f) + ScaledColliderRadius),
            _locomotionRig.Head.position.z);


        var velocityDirection =
            _playerRigidbody.velocity.magnitude > 0.001f ? _playerRigidbody.velocity.normalized : Vector3.zero;

        _groundCheckPosition = GroundPoint;

        LocomotionRig.GroundPoint.position = _groundCheckPosition;

        var groundCheckResults = Physics.OverlapSphereNonAlloc(_groundCheckPosition, ScaledColliderRadius,
            _groundCheckResults, groundMask);

        var newGroundState = Physics.OverlapSphereNonAlloc(_groundCheckPosition, ScaledColliderRadius,
            _groundCheckResults, groundMask) > 0;

        IsGrounded = newGroundState;
        if (!IsGrounded)
            LandLastTime = Time.time;

        var averageHandPosition =
            (_locomotionRig.LeftHandLocomotion.transform.position +
             _locomotionRig.RightHandLocomotion.transform.position) / 2f;
        var centerPosition = _locomotionRig.Head.transform.position;
        centerPosition.y = averageHandPosition.y;
        BodyForwardDirection = (averageHandPosition - centerPosition).normalized;
        _updateInternalMarker.End();
    }

    private void UpdateColliders()
    {
        var bodyColliderPos = transform.InverseTransformPoint(_locomotionRig.Head.position);
        
        if (isBodyFollowingLinear)
        {
            BodyCollider.center = new Vector3(bodyColliderPos.x, 0, bodyColliderPos.z) + ColliderOffset;
        }
        else
        {
            BodyCollider.center = Vector3.Lerp(BodyCollider.center,
                new Vector3(bodyColliderPos.x, 0, bodyColliderPos.z) + ColliderOffset, 5 * Time.fixedDeltaTime);
        }
    }

    private void UpdateStates() // Check states conditions and update them
    {
        _updateStatesMarker.Begin();
        _exitedStates.Clear();
        for (var i = 0; i < states.Length; i++)
        {
            var state = states[i];
            var wasActive = state.IsCanEnterOrActive;
            var activeAfterUpdate = state.UpdateLocomotionState(_currentMoveData);
            if (wasActive && !activeAfterUpdate)
                _exitedStates.Add(state);
        }

        _currentMoveData.Clear();

        for (var i = 0; i < _exitedStates.Count; i++)
        {
            var state = _exitedStates[i];
            state.OnExit();
        }

        _updateStatesMarker.End();
    }

    private void UpdateVelocity() // Postprocessing and applying total velocities from both hands
    {
        _updateVelocityMarker.Begin();
        float maxForceMagnitude = Mathf.Max(_leftHandTotalVelocity.magnitude, _rightHandTotalVelocity.magnitude);

        if (maxForceMagnitude != 0 && maxForceMagnitude < minHandSpeed)
        {
            if (!PlayerRigidbody.isKinematic)
                PlayerRigidbody.velocity *= 1 - brakePower;
        }
        else
        {
            Vector3 force =
                Vector3.ClampMagnitude(_leftHandTotalVelocity + _rightHandTotalVelocity, maxForceMagnitude);
            PlayerRigidbody.AddForce(force * forceMultiplier, ForceMode.Force);
        }

        _leftHandTotalVelocity = Vector3.zero;
        _rightHandTotalVelocity = Vector3.zero;

        Vector2 horizontalVelocity = new Vector2(PlayerRigidbody.velocity.x, PlayerRigidbody.velocity.z);
        horizontalVelocity = Vector2.ClampMagnitude(horizontalVelocity, velocityLimitHorizontal);
        float verticalVelocity = Mathf.Clamp(PlayerRigidbody.velocity.y, -velocityLimitVertical, velocityLimitVertical);
        if (!PlayerRigidbody.isKinematic)
            PlayerRigidbody.velocity = new Vector3(horizontalVelocity.x, verticalVelocity, horizontalVelocity.y);

        if (UseGravity)
            PlayerRigidbody.AddForce(_gravity, ForceMode.Acceleration);
        _updateVelocityMarker.End();
    }

    public void AddVelocity(Vector3 velocity, VRController hand) // Add force that will by applied to the body after all stated updated
    {
        if (hand == VRController.Left)
            _leftHandTotalVelocity += velocity;
        if (hand == VRController.Right)
            _rightHandTotalVelocity += velocity;
    }

    private Vector3 GetCrossPointOfControllers()
    {
        Vector3 crossPoint = Vector3.zero;
        Vector3 lookPointByY = new Vector3(LocomotionRig.HeadLookPoint.position.x, LocomotionRig.Head.position.y,
            LocomotionRig.HeadLookPoint.position.z);

        crossPoint = (lookPointByY
                      + LocomotionRig.RawInput.LeftController.position
                      + LocomotionRig.RawInput.RightController.position)
                     / 3f;

        return crossPoint;
    }

    public T GetState<T>() where T : ILocomotionState // Get LocomotionState by type
    {
        _typedStates.TryGetValue(typeof(T), out var state);
        return (T) state;
    }

    public void SetDrag(float newDrag)
    {
        PlayerRigidbody.drag = newDrag;
    }

    public void RestoreDrag()
    {
        PlayerRigidbody.drag = _defaultDrag;
    }

    public void SetGravity(Vector3 gravity)
    {
        _gravity = gravity;
    }

    public void RestoreGravity()
    {
        _gravity = _defaultGravity;
    }

    public void IgnoreBodyCollider(Collider colliderToIgnore)
    {
        Physics.IgnoreCollision(BodyCollider, colliderToIgnore);
    }

    public void ResetAllForces()
    {
        PlayerRigidbody.velocity = Vector3.zero;
        _leftHandTotalVelocity = Vector3.zero;
        _rightHandTotalVelocity = Vector3.zero;
    }

    public void Teleport(Vector3 position)
    {
        transform.position = position;

        LocomotionRig.LeftHandLocomotion.ResetHand();
        LocomotionRig.LeftHandLocomotion.transform.position = position;
        LocomotionRig.RightHandLocomotion.ResetHand();
        LocomotionRig.RightHandLocomotion.transform.position = position;

        ResetAllForces();
    }

    public void ResetPlaySpace() // Reset all locomotion components with headset recentering
    {
        StartCoroutine(ResetPlayspaceInternal());
    }

    private IEnumerator ResetPlayspaceInternal()
    {
        PlayerRigidbody.velocity = Vector3.zero;

        foreach (var state in states) state.OnReset();
        
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        AlignTransformByAnchor(_locomotionRig.TrackingSpace, _locomotionRig.Head.transform, transform,
            out Vector3 pos, out Quaternion rot);
        pos = _locomotionRig.TrackingSpace.parent.InverseTransformPoint(pos);
        pos.y = 0;
        _locomotionRig.TrackingSpace.localPosition = pos;

        if (setToClosestSurfaceOnStart)
        {
            Vector3 castOrigin = BodyCollider.transform.position + BodyCollider.center;
            if (Physics.Raycast(castOrigin, Vector3.down, out RaycastHit hit))
                PlayerRigidbody.position = hit.point + new Vector3(0, ScaledColliderHeight / 2f, 0);
        }

        _locomotionRig.LeftHandLocomotion.ResetHand();
        _locomotionRig.RightHandLocomotion.ResetHand();
    }
}