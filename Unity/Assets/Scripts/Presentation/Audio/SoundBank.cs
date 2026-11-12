using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The list of all sound cues. The Sound Mixer window edits the cues in it.</summary>
    [CreateAssetMenu(menuName = "Squarely/Sound Bank", fileName = "SoundBank")]
    public sealed class SoundBank : ScriptableObject
    {
        [SerializeField] private List<SoundCue> _cues = new List<SoundCue>();

        public IReadOnlyList<SoundCue> Cues => _cues;
    }
}
