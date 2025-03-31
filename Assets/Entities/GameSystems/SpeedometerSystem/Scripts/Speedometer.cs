using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class Speedometer : MonoBehaviour
{
    public struct SpeedometerData
    {
        public float PreviousTime;
        
        public Vector3 PreviousWorldPosition;
        public Vector3 PreviousLocalPosition;
        
        public Quaternion PreviousWorldRotation;
        public Quaternion PreviousLocalRotation;

        public float WorldLinearSpeed;
        public float LocalLinearSpeed;
        public float WorldAngularSpeed;
        public float LocalAngularSpeed;
        public Vector3 WorldDirection;
        public Vector3 LocalDirection;
    }
    
    private SpeedometerSystem _speedometerSystem;
    
    #region InjectServices----------------------------------------------------------------------------------------
    
    private PersistentUpdateService _persistentUpdateService;
    
    [Inject]
    private void Construct(SpeedometerSystem speedometerSystem, PersistentUpdateService persistentUpdateService)
    {
        _speedometerSystem = speedometerSystem;
        _persistentUpdateService = persistentUpdateService;
    }
    
    #endregion

    #region SpeedDataUpdate---------------------------------------------------------------------------------------
    
    public float WorldLinearSpeed
    {
        get
        {
            if (!_speedometerSystem.IsSystemReady) return default;
            return _speedometerSystem.SpeedometerDatas[IndexInSystem].WorldLinearSpeed;
        }
    }
    public float LocalLinearSpeed
    {
        get
        {
            if (!_speedometerSystem.IsSystemReady) return default;
            return _speedometerSystem.SpeedometerDatas[IndexInSystem].LocalLinearSpeed;
        }
    }
    public float WorldAngularSpeed
    {
        get
        {
            if (!_speedometerSystem.IsSystemReady) return default;
            return _speedometerSystem.SpeedometerDatas[IndexInSystem].WorldAngularSpeed;
        }
    }
    public float LocalAngularSpeed
    {
        get
        {
            if (!_speedometerSystem.IsSystemReady) return default;
            return _speedometerSystem.SpeedometerDatas[IndexInSystem].LocalAngularSpeed;
        }
    }
    public Vector3 DirectionGlobal
    {
        get
        {
            if (!_speedometerSystem.IsSystemReady) return default;
            return _speedometerSystem.SpeedometerDatas[IndexInSystem].WorldDirection;
        }
    }
    public Vector3 DirectionLocal
    {
        get
        {
            if (!_speedometerSystem.IsSystemReady) return default;
            return _speedometerSystem.SpeedometerDatas[IndexInSystem].LocalDirection;
        }
    }

    #endregion
    
    [TabGroup("Info"), ReadOnly] public int IndexInSystem;
    [TabGroup("Info"), ReadOnly] public bool IsMoving;
    [TabGroup("Info"), ReadOnly] public bool IsInAfterMoveState;

    #region DebugInfo---------------------------------------------------------------------------------------------

    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), SerializeField, ReadOnly] private float _worldLinearSpeed;
    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), SerializeField, ReadOnly] private float _localLinearSpeed;
    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), SerializeField, ReadOnly] private float _worldAngularSpeed;
    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), SerializeField, ReadOnly] private float _localAngularSpeed;
    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), SerializeField, ReadOnly] private Vector3 _worldDirection;
    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), SerializeField, ReadOnly] private Vector3 _localDirection;
    
    private bool _isDebugInfoShown;

    [TabGroup("Debug"), HideIf("_isDebugInfoShown"), Button]
    private void ShowDebugInfo()
    {
        //_persistentUpdateService.OnUpdate += UpdateDebugData;
        _isDebugInfoShown = true;
    }
    
    [TabGroup("Debug"), ShowIf("_isDebugInfoShown"), Button]
    private void HideDebugInfo()
    {
        //_persistentUpdateService.OnUpdate -= UpdateDebugData;
        _isDebugInfoShown = false;
    }

    #if UNITY_EDITOR
    private void Update()
    {
        if (_isDebugInfoShown)
        {
            _worldLinearSpeed = WorldLinearSpeed;
            _localLinearSpeed = LocalLinearSpeed;
            _worldAngularSpeed = WorldAngularSpeed;
            _localAngularSpeed = LocalAngularSpeed;
            _worldDirection = DirectionGlobal;
            _localDirection = DirectionLocal;
        }
    }
    #endif

    #endregion
}
