using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The list of all sound cues and music tracks. The Sound Mixer window edits the cues in it.</summary>
    [CreateAssetMenu(menuName = "Squarely/Sound Bank", fileName = "SoundBank")]
    public sealed class SoundBank : ScriptableObject
    {
        [SerializeField] private List<SoundCue> _cues = new List<SoundCue>();

        [SerializeField] private List<AudioClip> _music = new List<AudioClip>();

        public IReadOnlyList<SoundCue> Cues => _cues;

        public IReadOnlyList<AudioClip> Music => _music;
    }
}
