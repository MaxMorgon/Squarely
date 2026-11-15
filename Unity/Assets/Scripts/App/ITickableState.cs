namespace Squarely.App
{
    /// <summary>
    /// A state that needs a call every frame, such as a timer. The app's entry point drives it
    /// through <see cref="AppStateMachine.Tick"/>.
    /// </summary>
    public interface ITickableState : IAppState
    {
        void Tick(float deltaTime);
    }
}
