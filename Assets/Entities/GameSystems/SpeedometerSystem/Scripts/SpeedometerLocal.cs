using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class SpeedometerLocal : MonoBehaviour
{
    [TabGroup("Info"), ReadOnly] public float WorldLinearSpeed;
    [TabGroup("Info"), ReadOnly] public float LocalLinearSpeed;
    [TabGroup("Info"), ReadOnly] public float WorldAngularSpeed;
    [TabGroup("Info"), ReadOnly] public float LocalAngularSpeed;
    [TabGroup("Info"), ReadOnly] public Vector3 DirectionGlobal;
    [TabGroup("Info"), ReadOnly] public Vector3 DirectionLocal;

    [TabGroup("Debug"), SerializeField] private TextMesh _worldLinearSpeedDebug;
    [TabGroup("Debug"), SerializeField] private TextMeshProUGUI _localLinearSpeedDebug;
    [TabGroup("Debug"), SerializeField] private TextMeshProUGUI _worldAngularSpeedDebug;
    [TabGroup("Debug"), SerializeField] private TextMeshProUGUI _localAngularSpeedDebug;
    
    public float PreviousTime;
        
    public Vector3 PreviousWorldPosition;
    public Vector3 PreviousLocalPosition;
        
    public Quaternion PreviousWorldRotation;
    public Quaternion PreviousLocalRotation;
    
    private void Awake()
    {
        PreviousTime = Time.time;
        PreviousWorldPosition = transform.position;
        PreviousLocalPosition = transform.localPosition;
        PreviousWorldRotation = transform.rotation;
        PreviousLocalRotation = transform.localRotation;
    }
    
    public void Update() // You can use a persistent updater for improved performance
    {
        CheckLinearSpeed();
        CheckAngularSpeed();
        
        UpdateDebug();
    }

    private void UpdateDebug()
    {
        if (_worldLinearSpeedDebug != null) _worldLinearSpeedDebug.text = WorldLinearSpeed.ToString();
        if (_localLinearSpeedDebug != null) _localLinearSpeedDebug.text = LocalLinearSpeed.ToString();
        if (_worldAngularSpeedDebug != null) _worldAngularSpeedDebug.text = WorldAngularSpeed.ToString();
        if (_localAngularSpeedDebug != null) _localAngularSpeedDebug.text = LocalAngularSpeed.ToString();
    }
    
    private void CheckLinearSpeed()
    {
        if (transform.position == PreviousWorldPosition)
        {
            WorldLinearSpeed = 0f;
            LocalLinearSpeed = 0f;
            return;
        }
        
        LocalLinearSpeed = Vector3.Distance(PreviousLocalPosition, transform.localPosition) / Time.fixedDeltaTime;
        WorldLinearSpeed = Vector3.Distance(PreviousWorldPosition, transform.position) / Time.fixedDeltaTime;
        
        var direction = transform.position - PreviousWorldPosition;
        DirectionGlobal = direction.normalized;
    
        direction = transform.localPosition - PreviousLocalPosition;
        DirectionLocal = direction.normalized;
    
        PreviousWorldPosition = transform.position;
        PreviousLocalPosition = transform.localPosition;
    }
    
    private void CheckAngularSpeed()
    {
        if (transform.rotation == PreviousWorldRotation)
        {
            WorldAngularSpeed = 0f;
            LocalAngularSpeed = 0f;
            return;
        }
        
        float deltaTime = Time.time - PreviousTime;
        Quaternion worldDeltaRotation = transform.rotation * Quaternion.Inverse(PreviousWorldRotation);
        Quaternion localDeltaRotation = transform.localRotation * Quaternion.Inverse(PreviousLocalRotation);
    
        float worldAngle = 0f;
        float localAngle = 0f;
        Vector3 worldAxis = Vector3.zero;
        Vector3 localAxis = Vector3.zero;
        worldDeltaRotation.ToAngleAxis(out worldAngle, out worldAxis);
        localDeltaRotation.ToAngleAxis(out localAngle, out localAxis);
    
        if (deltaTime > 0f)
        {
            WorldAngularSpeed = Mathf.Abs(worldAngle / deltaTime);
            LocalAngularSpeed = Mathf.Abs(localAngle / deltaTime);
        }
        else
        {
            WorldAngularSpeed = 0f;
            LocalAngularSpeed = 0f;
        }
        
        PreviousWorldRotation = transform.rotation;
        PreviousLocalRotation = transform.localRotation;
        PreviousTime = Time.time;
    }
}
