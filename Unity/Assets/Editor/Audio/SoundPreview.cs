using System.Collections.Generic;
using Squarely.Presentation;
using UnityEditor;
using UnityEngine;

namespace Squarely.Editor
{
    /// <summary>
    /// Plays sound cues in the editor, in Edit mode and Play mode, on hidden temporary audio sources.
    /// It ignores cooldown and voice limit, so cues can be balanced against each other.
    /// </summary>
    public sealed class SoundPreview
    {
        private readonly List<AudioSource> _sources = new List<AudioSource>();
        private readonly List<ScheduledSound> _pending = new List<ScheduledSound>();
        private GameObject _host;
        private double _startTime;

        public bool IsPlayingSequence => _pending.Count > 0;

        #region Public API

        /// <summary>Plays one cue now at a level.</summary>
        public void Play(SoundCue cue, int level)
        {
            if (cue == null)
                return;

            if (cue.Clips.Count == 0)
                return;

            AudioSource source = TakeSource();
            cue.ApplyTo(source, level);
            source.Play();
        }

        /// <summary>Plays several cues at the same time.</summary>
        public void PlayTogether(IEnumerable<SoundCue> cues, int level)
        {
            foreach (SoundCue cue in cues)
                Play(cue, level);
        }

        /// <summary>
        /// Plays the sounds of a cue sequence with the timing of its steps. Replaces a sequence that still plays.
        /// </summary>
        public void PlaySequence(CueSequence sequence, SoundLevelInput input)
        {
            _pending.Clear();

            if (sequence == null)
                return;

            _pending.AddRange(SoundSchedule.Build(sequence.Steps, input));
            _startTime = EditorApplication.timeSinceStartup;
            EditorApplication.update -= OnEditorUpdated;
            EditorApplication.update += OnEditorUpdated;
        }

        /// <summary>Stops all sounds and the sequence.</summary>
        public void StopAll()
        {
            _pending.Clear();

            foreach (AudioSource source in _sources)
            {
                if (source != null)
                    source.Stop();
            }
        }

        /// <summary>Stops all sounds and destroys the temporary objects.</summary>
        public void Dispose()
        {
            EditorApplication.update -= OnEditorUpdated;
            StopAll();
            _sources.Clear();

            if (_host != null)
                Object.DestroyImmediate(_host);
        }

        #endregion

        private void OnEditorUpdated()
        {
            double elapsed = EditorApplication.timeSinceStartup - _startTime;

            for (int i = 0; i < _pending.Count;)
            {
                if (_pending[i].Seconds > elapsed)
                {
                    i++;
                    continue;
                }

                Play(_pending[i].Cue, _pending[i].Level);
                _pending.RemoveAt(i);
            }

            if (_pending.Count == 0)
                EditorApplication.update -= OnEditorUpdated;
        }

        private AudioSource TakeSource()
        {
            if (_host == null)
            {
                _host = new GameObject("SoundPreview") { hideFlags = HideFlags.HideAndDontSave };
                _sources.Clear();
            }

            foreach (AudioSource existing in _sources)
            {
                if (existing != null && !existing.isPlaying)
                    return existing;
            }

            var source = _host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sources.Add(source);
            return source;
        }
    }
}
