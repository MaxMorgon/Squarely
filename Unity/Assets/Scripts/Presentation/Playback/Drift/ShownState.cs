using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>What the game's views show: the cell type in each board slot, the piece in each hand slot, score and combo.</summary>
    public sealed class ShownState
    {
        /// <summary>The cell type each board slot shows, null where it shows no cell.</summary>
        public Grid<string> Cells { get; }

        /// <summary>The piece each hand slot shows, null where it shows none.</summary>
        public PieceInstance[] Hand { get; }

        public int Score { get; }

        public int Combo { get; }

        public ShownState(Grid<string> cells, PieceInstance[] hand, int score, int combo)
        {
            Cells = cells;
            Hand = hand;
            Score = score;
            Combo = combo;
        }
    }
}
