public interface ILocomotion
{
    T GetState<T>() where T : ILocomotionState;
}