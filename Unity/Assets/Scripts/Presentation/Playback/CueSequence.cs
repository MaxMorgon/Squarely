using System.Collections.Generic;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// How an event looks and sounds: a list of cue steps played in order on the target its handler builds.
    /// Edits apply from the next time the sequence plays, also in play mode.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Cue Sequence", fileName = "Cues")]
    public sealed class CueSequence : ScriptableObject
    {
        [Tooltip("On: the next event waits until this sequence ends. Off: it plays alongside the next events. " +
                 "A later event that takes over the same cells snaps it to its end first.")]
        [SerializeField] private bool _waitForEnd = true;

        [SerializeReference, CueStepPicker] private List<CueStep> _steps = new List<CueStep>();

        public bool WaitForEnd => _waitForEnd;

        public IReadOnlyList<CueStep> Steps => _steps;
    }
}
