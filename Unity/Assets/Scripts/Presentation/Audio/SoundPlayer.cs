using System;
using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Plays sound cues on a pool of audio sources. Skips a play that comes inside the cue's cooldown or over its voice limit.
    /// </summary>
    public sealed class SoundPlayer : MonoBehaviour
    {
        private sealed class Voice
        {
            public AudioSource Source;
            public SoundCue Cue;
        }

        [Tooltip("The sounds without a sim event: the UI cues.")]
        [SerializeField] private SoundHooks _hooks;

        private readonly List<Voice> _voices = new List<Voice>();
        private readonly Dictionary<SoundCue, double> _lastPlayTimes = new Dictionary<SoundCue, double>();
        private IHaptics _haptics = new NoHaptics();

        #region Public API

        /// <summary>Sets where the haptics of cues go. Null turns them off.</summary>
        public void SetHaptics(IHaptics haptics)
        {
            _haptics = haptics ?? new NoHaptics();
        }

        /// <summary>Plays a cue. A higher level raises the pitch by the cue's pitch step.</summary>
        /// <returns>False if the play was skipped.</returns>
        public bool Play(SoundCue cue, int level = 0)
        {
            if (cue == null)
                return false;

            if (cue.Clips.Count == 0)
                return false;

            double now = Time.unscaledTimeAsDouble;

            if (!_lastPlayTimes.TryGetValue(cue, out double lastPlayTime))
                lastPlayTime = double.NegativeInfinity;

            if (!SoundGate.CanPlay(now, lastPlayTime, cue.Cooldown, CountVoices(cue), cue.MaxVoices))
                return false;

            Voice voice = TakeVoice();
            _lastPlayTimes[cue] = now;
            voice.Cue = cue;
            cue.ApplyTo(voice.Source, level);
            voice.Source.Play();

            if (cue.Haptic != HapticKind.None)
                _haptics.Play(cue.Haptic);

            return true;
        }

        /// <summary>Plays one of the hook cues. Does nothing without hooks.</summary>
        /// <returns>False if nothing played.</returns>
        public bool PlayHook(Func<SoundHooks, SoundCue> pickCue)
        {
            if (_hooks == null)
                return false;

            return Play(pickCue(_hooks));
        }

        #endregion

        private int CountVoices(SoundCue cue)
        {
            int count = 0;

            foreach (Voice voice in _voices)
            {
                if (voice.Cue == cue && voice.Source.isPlaying)
                    count++;
            }

            return count;
        }

        private Voice TakeVoice()
        {
            foreach (Voice voice in _voices)
            {
                if (!voice.Source.isPlaying)
                    return voice;
            }

            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            var created = new Voice { Source = source };
            _voices.Add(created);
            return created;
        }
    }
}
