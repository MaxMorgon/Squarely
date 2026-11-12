using UnityEngine.Audio;

namespace Squarely.Presentation
{
    /// <summary>
    /// The sound and music toggles. Applying them sets the exposed mixer parameters.
    /// It does not save them: the app does that.
    /// </summary>
    public sealed class SoundSettings
    {
        public const string SfxParameter = "SfxVolume";
        public const string MusicParameter = "MusicVolume";

        private readonly AudioMixer _mixer;

        public bool SfxOn { get; private set; } = true;

        public bool MusicOn { get; private set; } = true;

        public SoundSettings(AudioMixer mixer)
        {
            _mixer = mixer;
        }

        /// <summary>Sets both toggles and applies them to the mixer.</summary>
        public void Set(bool sfxOn, bool musicOn)
        {
            SfxOn = sfxOn;
            MusicOn = musicOn;
            Apply();
        }

        /// <summary>Writes the toggles to the mixer. Does nothing without a mixer.</summary>
        public void Apply()
        {
            if (_mixer == null)
                return;

            _mixer.SetFloat(SfxParameter, SoundDecibels.ForToggle(SfxOn));
            _mixer.SetFloat(MusicParameter, SoundDecibels.ForToggle(MusicOn));
        }
    }
}
