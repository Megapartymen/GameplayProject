using System;
using System.Collections;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public class VisualHandPID : MonoBehaviour
{
    private const float RotationClampAngle = 90f;
    private const float RotationClampForceMultiplier = 0.15f;
    
    [TabGroup("Info"), ReadOnly] public float DistanceToHand;
    [TabGroup("Info"), ReadOnly] public bool IsRotationClamped;
    [TabGroup("Info"), ReadOnly] public float GrabbedObjectMass;
    
    [TabGroup("Links")] public Rigidbody PlayerRigidbody;
    [TabGroup("Links")] public Transform HandController, HandRotationController;
    
    [Space, Header("Settings")]
    public float MoveFrequency;
    public float MoveDamping;
    public float RotationFrequency;
    public float RotationDamping;
    
    private Rigidbody _handRigidbody;
    private float _fixedDeltaTime;
    
    public Action OnRotationClampExcсeded;
    
    private void Awake()
    {
        _handRigidbody = GetComponent<Rigidbody>();
        _fixedDeltaTime = Time.fixedDeltaTime;
        
        MoveFrequency = 60f;
        MoveDamping = 1f;
        RotationFrequency = 20000f;
        RotationDamping = 1000;
    }

    private void Start()
    {
        transform.position = HandController.position;
        transform.rotation = HandController.rotation;
        _handRigidbody.maxAngularVelocity = float.PositiveInfinity;
    }

    private void FixedUpdate()
    {
        PIDMovement();
        PIDRotation();
        
        CheckControllerDistance();
    }

    /*private void LateUpdate()
    {
        
    }*/

    private void PIDMovement()
    {
        //PID movement formula
        float kp = (6f * MoveFrequency) * (6f * MoveFrequency) * 0.25f;
        float kd = 4.5f * MoveFrequency * MoveDamping;
        float g = 1 / (1 + kd * _fixedDeltaTime + kp * _fixedDeltaTime * _fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * _fixedDeltaTime) * g;
        Vector3 force = (HandController.position - transform.position) * ksg + (PlayerRigidbody.velocity - _handRigidbody.velocity) * kdg;

        _handRigidbody.AddForce(force, ForceMode.Acceleration);
    }

    private void PIDRotation()
    {
        //PID rotation formula
        float kp = (6f * RotationFrequency) * (6f * RotationFrequency) * 0.25f;
        float kd = 4.5f * RotationFrequency * RotationDamping;
        float g = 1 / (1 + kd * _fixedDeltaTime + kp * _fixedDeltaTime * _fixedDeltaTime);
        float ksg = kp * g;
        float kdg = (kd + kp * _fixedDeltaTime) * g;
        
        Quaternion q = Quaternion.Euler(HandController.eulerAngles.x, HandController.eulerAngles.y, HandController.eulerAngles.z) * Quaternion.Inverse(transform.rotation);

        // Quaternion W compensation
        if (q.w < 0)
        {
            q.x = -q.x;
            q.y = -q.y;
            q.z = -q.z;
            q.w = -q.w;
        }
        
        q.ToAngleAxis(out float angle, out Vector3 axis);
        axis.Normalize();
        axis *= Mathf.Deg2Rad;
        Vector3 torque = ksg * axis * angle + -_handRigidbody.angularVelocity * kdg;
        
        if (IsRotationClamped)
        {
            torque *= GetForceByAngleDeviation(RotationClampAngle, RotationClampForceMultiplier, GrabbedObjectMass);
        }
    
        // Apply the torque to the first object with the total rotation added
        _handRigidbody.AddTorque(torque, ForceMode.Acceleration);
    }
    
    private void CheckControllerDistance()
    {
        DistanceToHand = Vector3.Distance(transform.position, HandController.position);
    }

    public void UpdateMoveFrequency(float newFrequency)
    {
        MoveFrequency = newFrequency;
    }
    
    public void UpdateMoveDamping(float newFrequency)
    {
        MoveDamping = newFrequency;
    }
    
    private float GetForceByAngleDeviation(float angleClamp, float forceMultiplier, float mass)
    {
        if (mass <= 0.1f) mass = 0.1f;
        
        float force = 1;
        float adaptedMass = mass * 0.2f;
        float angle = Mathf.Abs(Quaternion.Angle(transform.rotation, HandController.rotation));
        
        if (angle < angleClamp)
        {
            force = angle * forceMultiplier * adaptedMass;
        }
        else
        {
            force = angleClamp * forceMultiplier * adaptedMass;
            
            // OnRotationClampExcсeded?.Invoke();
            // Debug.Log("Clamp excсeded");
        }
        
        if (force < 1) force = 1;
        return force;
    }
    
    // public void SetMaxVelocity(float maxLinearVelocity, float maxAngularVelocity, float time)
    // {
    //     StartCoroutine(SetMaxVelocityCoroutine(maxLinearVelocity, maxAngularVelocity, time));
    // }
    
    // private IEnumerator SetMaxVelocityCoroutine(float maxLinearVelocity, float maxAngularVelocity, float time)
    // {
    //     var maxLinearVelocityOld = _handRigidbody.maxLinearVelocity;
    //     var maxAngularVelocityOld = _handRigidbody.maxAngularVelocity;
    //     
    //     _handRigidbody.maxLinearVelocity = maxLinearVelocity;
    //     _handRigidbody.maxAngularVelocity = maxAngularVelocity;
    //     
    //     yield return new WaitForSeconds(time);
    //     
    //     _handRigidbody.maxLinearVelocity = maxLinearVelocityOld;
    //     _handRigidbody.maxAngularVelocity = maxAngularVelocityOld;
    // }
}
