using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Temporary entry point: creates the game and starts it with the configured setup.
    /// To be replaced by the app flow, which will own the game through its game state.
    /// </summary>
    public sealed class GameBootstrap : MonoBehaviour
    {
        [SerializeField] private GameController _gamePrefab;
        [SerializeField] private Camera _camera;

        [Tooltip("The game mode. Edits apply live, also in play mode. Board and hand size changes need a play mode restart.")]
        [SerializeField] private SimConfigAsset _simConfig;

        [Tooltip("0 picks a new seed for every game.")]
        [SerializeField] private int _seed;

        private void Start()
        {
            ulong? seed = _seed != 0 ? (ulong)_seed : (ulong?)null;

            GameController game = Instantiate(_gamePrefab);
            game.Initialize(new GameSetup(_simConfig.Config, seed), _camera);
        }
    }
}
