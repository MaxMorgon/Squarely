using UnityEngine;
using UnityEngine.Audio;

namespace Squarely.Presentation
{
    /// <summary>Plays one looping music track at a time and crossfades to the next one.</summary>
    public sealed class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private AudioMixerGroup _mixerGroup;

        private AudioSource[] _sources;
        private VolumeFade[] _fades;
        private VolumeFade _master;
        private int _active;

        #region Public API

        /// <summary>Fades the current track out and the new one in. The track loops. A track that still plays while it fades out is faded back in, not restarted.</summary>
        public void Play(AudioClip track, float fadeSeconds)
        {
            EnsureCreated();

            if (track == null)
            {
                Stop(fadeSeconds);
                return;
            }

            int other = 1 - _active;
            MusicSwitchRule.Action action = MusicSwitchRule.Choose(
                _sources[_active].clip == track,
                _sources[_active].isPlaying,
                _fades[_active].Target,
                _sources[other].clip == track,
                _sources[other].isPlaying);

            if (action == MusicSwitchRule.Action.Keep)
                return;

            if (action == MusicSwitchRule.Action.FadeActiveBackIn)
            {
                _fades[_active].FadeTo(1f, fadeSeconds);
                return;
            }

            _fades[_active].FadeTo(0f, fadeSeconds);
            _active = other;

            if (action == MusicSwitchRule.Action.FadeOtherBackIn)
            {
                _fades[_active].FadeTo(1f, fadeSeconds);
                return;
            }

            AudioSource next = _sources[_active];
            next.clip = track;
            next.loop = true;
            next.Play();
            _fades[_active].FadeTo(1f, fadeSeconds);
        }

        /// <summary>Fades the volume of all music, e.g. down at game over while the track keeps playing. 1 is full volume.</summary>
        public void FadeTo(float volume, float seconds)
        {
            EnsureCreated();
            _master.FadeTo(Mathf.Clamp01(volume), seconds);
        }

        /// <summary>Fades the track out and stops it.</summary>
        public void Stop(float fadeSeconds)
        {
            EnsureCreated();
            _fades[0].FadeTo(0f, fadeSeconds);
            _fades[1].FadeTo(0f, fadeSeconds);
        }

        #endregion

        private void Update()
        {
            if (_sources == null)
                return;

            float delta = Time.unscaledDeltaTime;
            _master.Tick(delta);

            for (int i = 0; i < _sources.Length; i++)
            {
                _fades[i].Tick(delta);
                _sources[i].volume = _fades[i].Current * _master.Current;

                if (_sources[i].isPlaying && _fades[i].Current <= 0f && _fades[i].Target <= 0f)
                    _sources[i].Stop();
            }
        }

        private void EnsureCreated()
        {
            if (_sources != null)
                return;

            _sources = new AudioSource[2];
            _fades = new[] { new VolumeFade(0f), new VolumeFade(0f) };
            _master = new VolumeFade(1f);

            for (int i = 0; i < _sources.Length; i++)
            {
                _sources[i] = gameObject.AddComponent<AudioSource>();
                _sources[i].playOnAwake = false;
                _sources[i].loop = true;
                _sources[i].volume = 0f;
                _sources[i].outputAudioMixerGroup = _mixerGroup;
            }
        }
    }
}
