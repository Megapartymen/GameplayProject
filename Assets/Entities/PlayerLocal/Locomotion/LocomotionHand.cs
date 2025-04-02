using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class LocomotionHand : MonoBehaviour, ILocomotionHand
{
    private const float RaycastDistance = 5;

    [SerializeField, TabGroup("Links")] private LocomotionRig locomotionRig;
    [SerializeField, TabGroup("Links")] private Rigidbody rigidbody;
    [SerializeField, TabGroup("Links")] private Transform defaultPose;
    [SerializeField, TabGroup("Links")] private XRBaseController xrController;
    [SerializeField, TabGroup("Links")] private Speedometer speedometer;

    [Space, Header("Settings")] [SerializeField]
    private Vector3 offset;

    [SerializeField] private Vector3 rotationOffset;
    [SerializeField] private VRController handSide;
    [SerializeField] private LayerMask handRaycastMask;
    [SerializeField, Range(0f, 1f)] private float slipping;
    [SerializeField] private bool followController = true;
    [SerializeField] private float minHandSpeed = 0.1f;

    private Vector3 _velocityLocal;
    private Vector3 _velocityWorld;

    private Vector3 _prevPosLocal;
    private Vector3 _prevPosWorld;
    private bool _touch;
    private bool _isTouching;
    private float _deltaTime;
    private Vector3 _localPositionOffset;
    private Quaternion _visualRotationOffset;
    private bool _triggerHittedLastFrame;
    private Transform _rawHandTransform;
    
    public bool Touch => _touch;
    
    public bool IsTouching => _isTouching;

    [ReadOnly] public bool IsDisabled;
    private bool _wasNormallyEntered;
    private Vector3 _lastRaycastDirection;
    private string _lastHittedObject;
    private Collider[] _colliders;
    
    public RealHandTransform RealHandTransform { get; private set; }
    public XRBaseController XRController => xrController;
    public delegate void HandMove(MoveData moveData);
    public event HandMove OnHandMove = (_) => { };
    public event HandMove OnHandHit = (_) => { };

    public float HandSpeed { get; private set; }
    public Vector3 MoveVelocity => _velocityWorld;

    public VRController HandSide => handSide;
    
    public Collider[] Colliders => _colliders;

    #region InjectServices-----------------------------------------------------------------------------------------

    private HapticService _hapticService;

    [Inject]
    private void Construct(HapticService hapticService)
    {
        _hapticService = hapticService;
    }

    #endregion

    private void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>();
    }

    private void Start()
    {
        _deltaTime = Time.fixedDeltaTime;
        _visualRotationOffset = (locomotionRig.TrackingSpace.rotation * Quaternion.Euler(rotationOffset));
        _rawHandTransform = handSide == VRController.Left
            ? locomotionRig.RawInput.LeftController
            : locomotionRig.RawInput.RightController;
    }

    private void FixedUpdate()
    {
        RealHandTransform = GetTargetPosition();
        if (!RealHandTransform.controllerActive) return;

        _velocityLocal = (RealHandTransform.localPosition - _prevPosLocal) / _deltaTime;
        _velocityWorld = (RealHandTransform.position - _prevPosWorld) / _deltaTime;

        if (followController && !rigidbody.isKinematic)
        {
            rigidbody.velocity = (RealHandTransform.position - transform.position) / _deltaTime;
        }

        _prevPosLocal = RealHandTransform.localPosition;
        _prevPosWorld = RealHandTransform.position;
        HandSpeed = speedometer.LocalLinearSpeed;
    }

    private void LateUpdate()
    {
        _triggerHittedLastFrame = false;
    }
    
    private RealHandTransform GetTargetPosition()
    {
        var transforms = new RealHandTransform();

        var handLocalPosition = _rawHandTransform.localPosition;
        var handLocalRotation = _rawHandTransform.localRotation;

        transforms.controllerActive = handLocalPosition.magnitude > 0;
        transforms.localPosition = handLocalPosition;
        transforms.position = locomotionRig.TrackingSpace.TransformPoint(transforms.localPosition);
        transforms.localRotation = handLocalRotation;
        transforms.rotation = locomotionRig.TrackingSpace.rotation * handLocalRotation;

        transforms.position += transforms.rotation * offset;
        transforms.handSide = handSide;
        return transforms;
    }
    
    private bool CalculateNormal(out Vector3 normal, out Vector3 point)
    {
        normal = Vector3.zero;
        point = Vector3.zero;
        Vector3 direction = RealHandTransform.position - transform.position;
        if (direction.sqrMagnitude < 0.0001f)
        {
            if (_lastRaycastDirection.sqrMagnitude == 0)
            {
                return false;
            }

            direction = _lastRaycastDirection;
        }

        _lastRaycastDirection = direction;


        Ray ray = new Ray(transform.position, direction.normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, RaycastDistance, handRaycastMask))
        {
            normal = hit.normal;
            point = hit.point;
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        _wasNormallyEntered = false;
        _isTouching = false;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (_triggerHittedLastFrame)
            return;
        if (!RealHandTransform.controllerActive)
            return;
        if (collider.isTrigger)
            return;

        _wasNormallyEntered = true;
    }

    private void OnTriggerStay(Collider collider)
    {
        if (!_wasNormallyEntered)
            return;

        if (_triggerHittedLastFrame && _lastHittedObject != collider.name)
            return;

        if (!RealHandTransform.controllerActive)
            return;

        if (collider.isTrigger)
            return;

        if (!CalculateNormal(out Vector3 normal, out Vector3 point))
        {
            normal = Vector3.up;
        }

        var moveData = CalculateMoveData(normal, collider);

        if (!_isTouching)
        {
            if (!IsDisabled)
            {
                _hapticService.PlayTouch(handSide);
            }

            OnHandHit(moveData);
        }

        _isTouching = true;

        if (!IsDisabled)
        {
            OnHandMove(moveData);
        }

        _triggerHittedLastFrame = true;
        _lastHittedObject = collider.name;
    }

    public MoveData CalculateMoveData(Vector3 surfaceNormal, Collider collider)
    {
        RealHandTransform = GetTargetPosition();
        var relativeLocalVelocity = locomotionRig.TrackingSpace.TransformVector(-_velocityLocal);
        var worldVelocity = -_velocityWorld;
        var moveData = new MoveData()
        {
            handPosition = transform.position,
            mixedVelocity = Vector3.Lerp(worldVelocity, relativeLocalVelocity, slipping),
            rawWorldVelocity = worldVelocity,
            rawLocalVelocity = relativeLocalVelocity,
            realHandTransform = RealHandTransform,
            normal = surfaceNormal,
            speed = HandSpeed,
            collidedObject = collider.transform
        };
        
        return moveData;
    }

    public void ResetHand()
    {
        rigidbody.ResetInertiaTensor();
        rigidbody.ResetCenterOfMass();
        _isTouching = false;
        ResetVelocities();
        transform.position = RealHandTransform.position;
    }

    public void ResetVelocities()
    {
        rigidbody.velocity = Vector3.zero;
        _velocityLocal = Vector3.zero;
        _velocityWorld = Vector3.zero;
        _prevPosLocal = defaultPose.localPosition;
        _prevPosWorld = defaultPose.position;
    }

    public void IgnoreCollider(Collider colliderToIgnore)
    {
        for (int i = 0; i < Colliders.Length; i++)
        {
            Physics.IgnoreCollision(Colliders[i], colliderToIgnore);
        }
    }
}