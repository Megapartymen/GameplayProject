using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;

[BurstCompile]
public struct SpeedometerJob : IJobParallelForTransform
{
    public NativeArray<Speedometer.SpeedometerData> SpeedometerDatas;
    public float FixedDeltaTime;
    public float Time;
    
    private Speedometer.SpeedometerData _tempSpeedometerData;
    
    public void Execute(int index, TransformAccess transform)
    {
        var speedometerData = SpeedometerDatas[index];
        
        // Calculate linear speed
        if (transform.position == SpeedometerDatas[index].PreviousWorldPosition)
        {
            speedometerData.WorldLinearSpeed = 0f;
            speedometerData.LocalLinearSpeed = 0f;
        }
        else
        {
            speedometerData.WorldLinearSpeed = Vector3.Distance(SpeedometerDatas[index].PreviousWorldPosition, transform.position) / FixedDeltaTime;
            speedometerData.LocalLinearSpeed = Vector3.Distance(SpeedometerDatas[index].PreviousLocalPosition, transform.localPosition) / FixedDeltaTime;
        }
        
        speedometerData.WorldDirection = (transform.position - SpeedometerDatas[index].PreviousWorldPosition).normalized;
        speedometerData.LocalDirection = (transform.localPosition - SpeedometerDatas[index].PreviousLocalPosition).normalized;
        
        speedometerData.PreviousWorldPosition = transform.position;
        speedometerData.PreviousLocalPosition = transform.localPosition;
        
        // Calculate angular speed
        if (transform.rotation == SpeedometerDatas[index].PreviousWorldRotation)
        {
            speedometerData.WorldAngularSpeed = 0f;
            speedometerData.LocalAngularSpeed = 0f;
        }
        else
        {
            float deltaTime = Time - SpeedometerDatas[index].PreviousTime;
            Quaternion worldDeltaRotation = transform.rotation * Quaternion.Inverse(SpeedometerDatas[index].PreviousWorldRotation);
            Quaternion localDeltaRotation = transform.localRotation * Quaternion.Inverse(SpeedometerDatas[index].PreviousLocalRotation);

            float worldAngle = 0f;
            float localAngle = 0f;
            Vector3 worldAxis = Vector3.zero;
            Vector3 localAxis = Vector3.zero;
            worldDeltaRotation.ToAngleAxis(out worldAngle, out worldAxis);
            localDeltaRotation.ToAngleAxis(out localAngle, out localAxis);

            if (deltaTime > 0f)
            {
                speedometerData.WorldAngularSpeed = Mathf.Abs(worldAngle / deltaTime);
                speedometerData.LocalAngularSpeed = Mathf.Abs(localAngle / deltaTime);
            }
            else
            {
                speedometerData.WorldAngularSpeed = 0f;
                speedometerData.LocalAngularSpeed = 0f;
            }
        }
        
        speedometerData.PreviousWorldRotation = transform.rotation;
        speedometerData.PreviousLocalRotation = transform.localRotation;
        speedometerData.PreviousTime = Time;
        
        SpeedometerDatas[index] = speedometerData;
    }
}
