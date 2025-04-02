using DG.Tweening;
using NewFolder.VR.Locomotion;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

public class HeightChanger : MonoBehaviour
{
    [TabGroup("Info"), ReadOnly] public bool IsLandAnimatePlaying { get; private set; }
    [field: SerializeField, TabGroup("Info")] public bool IsHeightByHead { get; set; } = true;
    [TabGroup("Info"), ReadOnly] public float MaxHeight { get; private set; }
    [TabGroup("Info"), ReadOnly] public float MinHeight{ get; private set; }
    [TabGroup("Info"), ReadOnly] public float BaseHeight{ get; private set; }
    [TabGroup("Info"), ReadOnly] public float CurrentHeight => _locomotionRig.BodyCollider.height;
    
    [SerializeField] private LocomotionRig _locomotionRig;
    [SerializeField] private Transform _moveRig;
    [SerializeField] private Transform _rawRig;
    [SerializeField] private Transform _physRig;
    [SerializeField] private Locomotion _locomotion;
    [SerializeField] private Speedometer _speedometer;
    [Space]
    [SerializeField] private Vector2 landVelocityRange;
    [SerializeField] private AnimationCurve landHeadMoveCurve;
    [SerializeField] private AnimationCurve goDownCurve;
    [SerializeField] private AnimationCurve goUpCurve;
    [SerializeField] private float headMoveSpeed;
    [SerializeField] private float headRotationMaxAngle;
    
    private float _calmEnvelope;
    private float _envelopeCenter;
    private float _heightChangeSpeed;
    
    private Coroutine _returnBaseHeightCoroutine;
    
    // #region InjectServices----------------------------------------------------------------------------------------
    //
    // private AudioFMODSystem _audioFMODSystem;
    //
    // [Inject]
    // private void Construct(AudioFMODSystem audioFMODSystem)
    // {
    //     _audioFMODSystem = audioFMODSystem;
    // }
    //
    // #endregion

    private void Awake()
    {
        BaseHeight = _locomotionRig.BodyCollider.height;
        MaxHeight = BaseHeight * 1.2f;
        MinHeight = BaseHeight * 0.5f;
        _calmEnvelope = 0.1f;
        _envelopeCenter = _locomotionRig.Head.localPosition.y * 2;
    }

    private void Start()
    {
        // StartCoroutine(ReturnBaseHeight());
    }

    private void Update()
    {
        SetHeight();
    }

    // private IEnumerator ReturnBaseHeight()
    // {
    //     while (true)
    //     {
    //         if (/*_speedometer.WorldLinearSpeed > 0.5f*/ true)
    //         {
    //             // _heightChangeSpeed = _speedometer.WorldLinearSpeed * 0.01f;
    //             _heightChangeSpeed = 0.01f;
    //         }
    //         else
    //         {
    //             _heightChangeSpeed = 0f;
    //         }
    //         
    //         if (_locomotionRig.BodyCollider.height > _baseHeight + 0.01f)
    //         {
    //             _locomotionRig.BodyCollider.height -= _heightChangeSpeed;
    //             _locomotionRig.BellyCollider.height -= _heightChangeSpeed;
    //         }
    //         else if (_locomotionRig.BodyCollider.height < _baseHeight - 0.01f)
    //         {
    //             _locomotionRig.BodyCollider.height += _heightChangeSpeed;
    //             _locomotionRig.BellyCollider.height += _heightChangeSpeed;
    //         }
    //         
    //         yield return null;
    //     }
    // }

    private void SetHeight() // set collider height by Y camera position
    {
        var headPosition = _locomotionRig.Head.localPosition.y * 2;
        var targetHeight = headPosition;
        
        if (IsHeightByHead)
        {
            if (_speedometer.WorldLinearSpeed < 1f)
            {
                if (targetHeight < MinHeight)
                {
                    targetHeight = Mathf.Lerp(targetHeight, MinHeight, 0.5f);
                }
                else if (targetHeight > MaxHeight)
                {
                    targetHeight = Mathf.Lerp(targetHeight, MaxHeight, 0.5f);
                }
            }
            else
            {
                targetHeight = Mathf.Lerp(targetHeight, BaseHeight, 0.15f);
            }
        }
        else
        {
            targetHeight = Mathf.Lerp(targetHeight, BaseHeight, 0.3f);
        }
        
        _locomotionRig.BodyCollider.height = targetHeight;
        // _locomotionRig.BellyCollider.height = targetHeight;
    }

    public void AnimateLand(Vector3 fallVelocity, float previousLandTime)
    {
        if (fallVelocity.y > -0.5f) return;
        
        // var downPosition = Vector3.down * fallVelocity.y * 0.03f;
        var downPosition = new Vector3(_rawRig.localPosition.x, _rawRig.localPosition.y - fallVelocity.y * 0.03f, _rawRig.localPosition.z);
        
        DOTween.Sequence()
            .Append(DOVirtual.Vector3(_rawRig.localPosition, downPosition, 0.1f, 
                x =>
                {
                    _moveRig.localPosition = x;
                    // _physRig.localPosition = x;
                })).SetEase(goDownCurve)
            .Append(DOVirtual.Vector3(downPosition, _rawRig.localPosition, 0.3f,
                x =>
                {
                    _moveRig.localPosition = x;
                    // _physRig.localPosition = x;
                })).SetEase(goUpCurve)
            ;
        
        // Only for play sound
        // if (Time.time - previousLandTime > 1f)
        //     _audioFMODSystem.PlayJumpLanding();
        
        // var startHeight = _locomotionRig.BodyCollider.height;
        
        // DOTween.Sequence()
        //     .AppendCallback(() =>
        //     {
        //         IsLandAnimatePlaying = true;
        //     })
        //     .Append(DOVirtual.Float(_locomotionRig.BodyCollider.height, _locomotionRig.BodyCollider.height * 0.7f, 0.5f, 
        //         x => _locomotionRig.BodyCollider.height = x))
        //     .Append(DOVirtual.Float(_locomotionRig.BodyCollider.height, startHeight, 1f,
        //         x => _locomotionRig.BodyCollider.height = x))
        //     .AppendCallback(() =>
        //     {
        //         IsLandAnimatePlaying = false;
        //     })
        //     ;
        
        // StartCoroutine(AnimateLandCoroutine(fallVelocity, previousLandTime));
    }
    
    // private IEnumerator AnimateLandCoroutine(Vector3 fallVelocity, float previousLandTime)
    // {
    //     var saveHeadPosition = _locomotionRig.Head.localPosition;
    //     IsLandAnimatePlaying = true;
    //     
    //     var velocity = fallVelocity; 
    //     
    //     // Animate only if fallen from height
    //     if (velocity.y > -0.1f)
    //         yield return null;
    //     
    //     // Only for play sound
    //     if (CurrentTime.time - previousLandTime > 1f)
    //         AudioFMODSystem.Instance.PlayJumpLanding();
    //         
    //     var power = Mathf.Clamp01(Mathf.InverseLerp(landVelocityRange.x, landVelocityRange.y, -velocity.y));
    //     var forceDirection = Vector3.up;
    //     var t = 0f;
    //         
    //     while (t<1)
    //     {
    //         t += CurrentTime.fixedDeltaTime * headMoveSpeed;
    //         var x = landHeadMoveCurve.Evaluate(t);
    //         
    //         // try change collider height
    //         var poweredX = x * power;
    //         _locomotionRig.BodyCollider.height = saveHeadPosition.y - poweredX;
    //         _locomotionRig.BellyCollider.height = saveHeadPosition.y - poweredX;
    //         
    //         // try use offset camera
    //         // _locomotion.Rig.HeadStabilizer.VisualOffset = forceDirection * (x * power);
    //         // _locomotion.Rig.HeadStabilizer.VisualEulerOffset = Vector3.right * (-x * power * headRotationMaxAngle);
    //         
    //         yield return 0;
    //     }
    //     
    //     IsLandAnimatePlaying = false;
    //     _locomotionRig.Head.localPosition = saveHeadPosition;
    // }
}
