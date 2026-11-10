namespace Squarely.App
{
    /// <summary>
    /// One screen or phase of the app, such as the menu or a running game.
    /// A state creates what it shows when entered and destroys it when exited.
    /// </summary>
    public interface IAppState
    {
        void Enter();

        void Exit();
    }
}
