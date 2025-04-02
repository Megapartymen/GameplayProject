using UnityEngine;

public interface IClimbState : ILocomotionState
{
    bool ClimbingWasActive { get; }
    Vector3 LastClimbDirection { get; }
    float TimeSinceLastClimb { get; }
}