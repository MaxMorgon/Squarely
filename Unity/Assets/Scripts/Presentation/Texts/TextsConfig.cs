using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Every text the player reads, grouped by screen. Each group is meant to become one Unity Localization
    /// string table later, with one entry per field, so a group holds only the texts of its own screen.
    /// Views decide which text to show; this asset decides what it says.
    /// </summary>
    [CreateAssetMenu(menuName = "Squarely/Texts Config", fileName = "Texts")]
    public sealed class TextsConfig : ScriptableObject
    {
        [SerializeField] private MenuTexts _menu = new MenuTexts();
        [SerializeField] private GameHudTexts _gameHud = new GameHudTexts();
        [SerializeField] private GameOverTexts _gameOver = new GameOverTexts();

        public MenuTexts Menu => _menu;

        public GameHudTexts GameHud => _gameHud;

        public GameOverTexts GameOver => _gameOver;
    }
}
