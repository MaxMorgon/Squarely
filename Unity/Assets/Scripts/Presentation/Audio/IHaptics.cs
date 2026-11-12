namespace Squarely.Presentation
{
    /// <summary>Plays a vibration on the device.</summary>
    public interface IHaptics
    {
        void Play(HapticKind kind);
    }
}
