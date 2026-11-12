using System.Collections.Generic;
using Squarely.Presentation;
using UnityEditor;
using UnityEngine;

namespace Squarely.Editor
{
    /// <summary>
    /// Window to balance sounds: one row per cue of a sound bank, with faders and play buttons,
    /// and a preview of the sounds of a cue sequence with real timing. Works in Edit mode and Play mode.
    /// </summary>
    public sealed class SoundMixerWindow : EditorWindow
    {
        private const float NameWidth = 150f;
        private const float VolumeWidth = 150f;
        private const float PitchWidth = 44f;
        private const float StepWidth = 50f;
        private const float GroupWidth = 130f;
        private const float HapticWidth = 70f;
        private const float PlayWidth = 28f;

        private static readonly Color OddRowTint = new Color(0f, 0f, 0f, 0.18f);

        [SerializeField] private SoundBank _bank;
        [SerializeField] private CueSequence _sequence;
        [SerializeField] private int _level;
        [SerializeField] private int _countFrom;
        [SerializeField] private int _count;
        [SerializeField] private int _clearCount = 1;
        [SerializeField] private int _comboLevel;
        [SerializeField] private int _comboLost;

        private readonly HashSet<SoundCue> _selected = new HashSet<SoundCue>();
        private readonly Dictionary<SoundCue, SerializedObject> _serialized = new Dictionary<SoundCue, SerializedObject>();
        private SoundPreview _preview;
        private Vector2 _scroll;

        [MenuItem("Window/Squarely/Sound Mixer")]
        public static void Open()
        {
            GetWindow<SoundMixerWindow>("Sound Mixer");
        }

        private void OnEnable()
        {
            _preview = new SoundPreview();
        }

        private void OnDisable()
        {
            _preview.Dispose();
            _preview = null;
        }

        private void OnGUI()
        {
            _bank = (SoundBank)EditorGUILayout.ObjectField("Sound Bank", _bank, typeof(SoundBank), false);

            if (_bank == null)
            {
                EditorGUILayout.HelpBox("Pick a Sound Bank to list its cues.", MessageType.Info);
                return;
            }

            DrawToolbar();
            DrawHeader();
            DrawRows();
            DrawEventPreview();
        }

        #region Drawing

        private void DrawToolbar()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _level = EditorGUILayout.IntField("Level for play", _level);

                if (GUILayout.Button("Play selected", GUILayout.Width(100f)))
                    _preview.PlayTogether(_selected, _level);

                if (GUILayout.Button("Select all", GUILayout.Width(70f)))
                    SelectAll();

                if (GUILayout.Button("Select none", GUILayout.Width(80f)))
                    _selected.Clear();

                if (GUILayout.Button("Stop", GUILayout.Width(50f)))
                    _preview.StopAll();
            }
        }

        private static void DrawHeader()
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                GUILayout.Space(22f);
                GUILayout.Label("Cue", EditorStyles.miniLabel, GUILayout.Width(NameWidth));
                GUILayout.Label("Volume", EditorStyles.miniLabel, GUILayout.Width(VolumeWidth + 50f));
                GUILayout.Label("Pitch min", EditorStyles.miniLabel, GUILayout.Width(PitchWidth + 12f));
                GUILayout.Label("max", EditorStyles.miniLabel, GUILayout.Width(PitchWidth + 12f));
                GUILayout.Label("Step (st)", EditorStyles.miniLabel, GUILayout.Width(StepWidth + 8f));
                GUILayout.Label("Mixer group", EditorStyles.miniLabel, GUILayout.Width(GroupWidth + 4f));
                GUILayout.Label("Haptic", EditorStyles.miniLabel, GUILayout.Width(HapticWidth));
            }
        }

        private void DrawRows()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(_scroll, GUILayout.MaxHeight(position.height - 190f)))
            {
                _scroll = scroll.scrollPosition;

                int rowIndex = 0;

                foreach (SoundCue cue in _bank.Cues)
                {
                    if (cue == null)
                        continue;

                    DrawRow(cue, rowIndex % 2 == 1);
                    rowIndex++;
                }
            }
        }

        private void DrawRow(SoundCue cue, bool isTinted)
        {
            SerializedObject so = SerializedFor(cue);
            so.Update();

            using (var row = new EditorGUILayout.HorizontalScope())
            {
                if (isTinted && Event.current.type == EventType.Repaint)
                    EditorGUI.DrawRect(row.rect, OddRowTint);

                bool isSelected = EditorGUILayout.Toggle(_selected.Contains(cue), GUILayout.Width(18f));
                SetSelected(cue, isSelected);

                if (GUILayout.Button(cue.name, EditorStyles.label, GUILayout.Width(NameWidth)))
                    EditorGUIUtility.PingObject(cue);

                EditorGUILayout.PropertyField(so.FindProperty("_volume"), GUIContent.none, GUILayout.Width(VolumeWidth));
                GUILayout.Label(DecibelText(cue.Volume), EditorStyles.miniLabel, GUILayout.Width(48f));
                EditorGUILayout.PropertyField(so.FindProperty("_pitchMin"), GUIContent.none, GUILayout.Width(PitchWidth + 10f));
                EditorGUILayout.PropertyField(so.FindProperty("_pitchMax"), GUIContent.none, GUILayout.Width(PitchWidth + 10f));
                EditorGUILayout.PropertyField(so.FindProperty("_pitchStepSemitones"), GUIContent.none, GUILayout.Width(StepWidth));
                EditorGUILayout.PropertyField(so.FindProperty("_mixerGroup"), GUIContent.none, GUILayout.Width(GroupWidth));
                EditorGUILayout.PropertyField(so.FindProperty("_haptic"), GUIContent.none, GUILayout.Width(HapticWidth));

                if (GUILayout.Button("▶", GUILayout.Width(PlayWidth)))
                    _preview.Play(cue, _level);
            }

            so.ApplyModifiedProperties();
        }

        private void DrawEventPreview()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play event", EditorStyles.boldLabel);
            _sequence = (CueSequence)EditorGUILayout.ObjectField("Cue Sequence", _sequence, typeof(CueSequence), false);
            _countFrom = EditorGUILayout.IntField(new GUIContent("Count from", "Start value of the count. Used for the ticks of a count-up."), _countFrom);
            _count = EditorGUILayout.IntField(new GUIContent("Count", "End value of the count. Level source Count."), _count);
            _clearCount = EditorGUILayout.IntField(new GUIContent("Clear count", "Matches cleared by the command."), _clearCount);
            _comboLevel = EditorGUILayout.IntField(new GUIContent("Combo level", "Combo after the command."), _comboLevel);
            _comboLost = EditorGUILayout.IntField(new GUIContent("Combo lost", "Combo that the command lost. Level source Combo Lost."), _comboLost);

            using (new EditorGUI.DisabledScope(_sequence == null))
            {
                if (GUILayout.Button("Play event sounds"))
                    _preview.PlaySequence(_sequence, new SoundLevelInput(_count, _clearCount, _comboLevel, _comboLost, _countFrom));
            }
        }

        #endregion

        #region Helpers

        private SerializedObject SerializedFor(SoundCue cue)
        {
            if (!_serialized.TryGetValue(cue, out SerializedObject so) || so.targetObject == null)
            {
                so = new SerializedObject(cue);
                _serialized[cue] = so;
            }

            return so;
        }

        private void SetSelected(SoundCue cue, bool isSelected)
        {
            if (isSelected)
                _selected.Add(cue);
            else
                _selected.Remove(cue);
        }

        private void SelectAll()
        {
            foreach (SoundCue cue in _bank.Cues)
            {
                if (cue != null)
                    _selected.Add(cue);
            }
        }

        private static string DecibelText(float volume)
        {
            return volume <= 0.0001f ? "-inf dB" : SoundDecibels.FromLinear(volume).ToString("0.0") + " dB";
        }

        #endregion
    }
}
