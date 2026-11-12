namespace Squarely.Presentation
{
    /// <summary><see cref="IHaptics"/> that does nothing.</summary>
    public sealed class NoHaptics : IHaptics
    {
        public void Play(HapticKind kind)
        {
        }
    }
}
