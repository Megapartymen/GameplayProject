public interface IJumpState : ILocomotionState
{
    bool IsJumped { get; }
    bool CanJump { get; }
    float JumpSinceLastTime { get; }
    float JumpForce { get; }
}