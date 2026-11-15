using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Tunables for the pace of a game view.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Game Feel", fileName = "GameFeel")]
    public sealed class GameFeel : ScriptableObject
    {
        [Tooltip("Seconds the game waits, after the last animation and celebration ended, before it reports the game's end.")]
        [SerializeField, Min(0f)] private float _gameEndHold = 0.5f;

        /// <summary>Seconds between the last animation and the report of the game's end.</summary>
        public float GameEndHold => _gameEndHold;
    }
}
