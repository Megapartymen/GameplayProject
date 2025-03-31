using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Serialization;


[DefaultExecutionOrder(-997)]
public class SpeedometerSystem : MonoBehaviour, IGlobalInitializable
{
    [TabGroup("Info"), SerializeField, Sirenix.OdinInspector.ReadOnly] private Speedometer[] _speedometers;
    [TabGroup("Info"), Sirenix.OdinInspector.ReadOnly] public bool IsSystemReady;
    
    private TransformAccessArray _transformAccessArray;
    private JobHandle _jobHandle;
    
    public NativeArray<Speedometer.SpeedometerData> SpeedometerDatas;

    private bool _updateScheduled;

    private void Awake()
    {
        Init();
    }

    #region ProjectContextLogic-----------------------------------------------------------------------------------

    public void OnInitialize()
    {
        // _sceneLoadingService.OnEndSceneLoading += Init;
        // _sceneLoadingService.OnStartSceneLoading += EraseData;
    }
    
    private void Init()
    {
        if (IsSystemReady) return;
        
        _speedometers = FindObjectsOfType<Speedometer>();
        SpeedometerDatas = new NativeArray<Speedometer.SpeedometerData>(_speedometers.Length, Allocator.Persistent);
        _transformAccessArray = new TransformAccessArray(_speedometers.Length);
        
        for (int i = 0; i < _speedometers.Length; i++)
        {
            SpeedometerDatas[i] = new Speedometer.SpeedometerData();
            _speedometers[i].IndexInSystem = i;
        }
        
        for (int i = 0; i < _speedometers.Length; i++)
        {
            _transformAccessArray.Add(_speedometers[i].transform);
        }
        
        IsSystemReady = true;
    }
    
    private void EraseData()
    {
        IsSystemReady = false;
        
        _speedometers = null;
        SpeedometerDatas.Dispose();
        _transformAccessArray.Dispose();
    }
    
    #endregion
    
    private void Update()
    {
        ScheduleUpdateJob();
    }

    private void ScheduleUpdateJob()
    {
        if (!IsSystemReady) return;
        
        var speedometerJob = new SpeedometerJob()
        {
            SpeedometerDatas = SpeedometerDatas,
            FixedDeltaTime = Time.fixedDeltaTime,
            Time = Time.time
        };
        _jobHandle = speedometerJob.Schedule(_transformAccessArray);
        
        _updateScheduled = true;
        
        _jobHandle.Complete();
        
        _updateScheduled = false;
    }
}
