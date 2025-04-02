using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class LocomotionState : MonoBehaviour, ILocomotionState
{
    protected Locomotion Locomotion { get; private set; }
    [field: SerializeField, ReadOnly, BoxGroup("Debug info")]
    public bool IsCanEnterOrActive { get; protected set; }
    public virtual void Initialize(Locomotion locomotion)
    {
        Locomotion = locomotion;
    }
    protected abstract bool CanEnterState(IList<MoveData> moveData);
    protected abstract void OnUpdate(IList<MoveData> moveData);
    internal bool UpdateLocomotionState(IList<MoveData> moveData)
    {
        IsCanEnterOrActive = CanEnterState(moveData);
        if (IsCanEnterOrActive)
            OnUpdate(moveData);
        return IsCanEnterOrActive;
    }
    public virtual void OnExit()
    {
    }
    public abstract Type GetStateType();
    public virtual void OnReset()
    {
        IsCanEnterOrActive = false;
    }
}