#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.InputSystem;

namespace Squarely.Presentation
{
    /// <summary>
    /// Editor and development builds only: a small panel for tuning playback. Replays the last move (also with R),
    /// sets the playback speed and shows the last drift check. Not game UI.
    /// </summary>
    public sealed class PlaybackDebugOverlay : MonoBehaviour
    {
        private static readonly float[] SpeedPresets = { 0.1f, 0.25f, 0.5f, 1f, 2f };

        // Static, so the chosen speed carries over to the next game.
        private static float s_speed = 1f;

        private GameController _game;
        private EventPlayer _player;

        public void Initialize(GameController game, EventPlayer player)
        {
            _game = game;
            _player = player;
            _player.Speed = s_speed;
        }

        #region Unity

        private void Update()
        {
            if (_game == null)
                return;

            Keyboard keyboard = Keyboard.current;

            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                _game.ReplayLastMove();
        }

        private void OnGUI()
        {
            if (_game == null)
                return;

            float scale = Mathf.Max(1f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUILayout.BeginArea(new Rect(8f, 8f, 230f, 140f), GUI.skin.box);
            GUILayout.Label("Playback (debug)");

            GUI.enabled = _game.CanReplayLastMove;

            if (GUILayout.Button("Replay last move (R)"))
                _game.ReplayLastMove();

            GUI.enabled = true;

            GUILayout.BeginHorizontal();

            foreach (float speed in SpeedPresets)
            {
                bool selected = Mathf.Approximately(speed, s_speed);

                if (GUILayout.Toggle(selected, $"{speed:0.##}x", GUI.skin.button) && !selected)
                    SetSpeed(speed);
            }

            GUILayout.EndHorizontal();

            GUILayout.Label(DriftText());
            GUILayout.EndArea();
        }

        #endregion

        private void SetSpeed(float speed)
        {
            s_speed = speed;
            _player.Speed = speed;
        }

        private string DriftText()
        {
            ViewDrift drift = _game.LastDrift;

            if (drift == null)
                return "Drift check: not run yet";

            if (drift.IsEmpty)
                return "Drift check: passed";

            return "<color=red>Drift check: FAILED, see console</color>";
        }
    }
}
#endif
