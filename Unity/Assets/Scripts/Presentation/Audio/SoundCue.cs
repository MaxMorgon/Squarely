using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Squarely.Presentation
{
    /// <summary>One sound: its clips and how it plays. Edits apply from the next play.</summary>
    [CreateAssetMenu(menuName = "Squarely/Sound Cue", fileName = "Sound")]
    public sealed class SoundCue : ScriptableObject
    {
        [Tooltip("One clip is picked at random on each play.")]
        [SerializeField] private List<AudioClip> _clips = new List<AudioClip>();

        [Range(0f, 1f)]
        [SerializeField] private float _volume = 1f;

        [Tooltip("Random pitch factor range of each play. 1 is the clip's own pitch.")]
        [SerializeField] private float _pitchMin = 0.95f;

        [SerializeField] private float _pitchMax = 1.05f;

        [Tooltip("Pitch raise per level, in semitones. 0 turns the raise off. Level 0 plays at the base pitch.")]
        [SerializeField] private float _pitchStepSemitones;

        [Tooltip("The highest level that raises the pitch.")]
        [SerializeField] private int _maxLevel = 8;

        [Tooltip("A play within this time after the last one is skipped, in seconds.")]
        [SerializeField] private float _cooldown;

        [Tooltip("A play is skipped while this many voices of this sound play.")]
        [SerializeField] private int _maxVoices = 4;

        [SerializeField] private AudioMixerGroup _mixerGroup;

        [SerializeField] private HapticKind _haptic = HapticKind.None;

        public IReadOnlyList<AudioClip> Clips => _clips;

        /// <summary>Linear volume from 0 to 1.</summary>
        public float Volume => _volume;

        public float PitchMin => _pitchMin;

        public float PitchMax => _pitchMax;

        public float PitchStepSemitones => _pitchStepSemitones;

        public int MaxLevel => _maxLevel;

        public float Cooldown => _cooldown;

        public int MaxVoices => _maxVoices;

        public AudioMixerGroup MixerGroup => _mixerGroup;

        public HapticKind Haptic => _haptic;

        /// <summary>The clip for a random value from 0 up to 1, or null if the cue has no clip.</summary>
        public AudioClip PickClip(float randomT)
        {
            int count = _clips.Count;

            if (count == 0)
                return null;

            int index = Mathf.Clamp(Mathf.FloorToInt(randomT * count), 0, count - 1);
            return _clips[index];
        }

        /// <summary>
        /// Sets up an audio source to play this cue at a level: clip, mixer group, volume and pitch. Does not start it.
        /// </summary>
        /// <returns>False if the cue has no clip.</returns>
        public bool ApplyTo(AudioSource source, int level)
        {
            AudioClip clip = PickClip(Random.value);

            if (clip == null)
                return false;

            source.outputAudioMixerGroup = _mixerGroup;
            source.clip = clip;
            source.volume = _volume;
            source.pitch = PitchFor(level, Random.value);
            return true;
        }

        /// <summary>The pitch factor of a play at a level. <paramref name="randomT"/> from 0 to 1 picks the point in the variation range.</summary>
        public float PitchFor(int level, float randomT)
        {
            return SoundPitch.Calculate(_pitchMin, _pitchMax, randomT, _pitchStepSemitones, level, _maxLevel);
        }
    }
}
