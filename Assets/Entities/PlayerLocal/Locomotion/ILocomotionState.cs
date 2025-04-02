using System;

public interface ILocomotionState
{
    bool IsCanEnterOrActive { get; }
    Type GetStateType();
}