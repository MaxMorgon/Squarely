#if UNITY_EDITOR || SQUARELY_DEV_TOOLS
using System.Collections.Generic;
using System.Text;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Where the views differ from the sim's state after playback. Empty when they match.</summary>
    public sealed class ViewDrift
    {
        private readonly List<GridPos> _cells = new List<GridPos>();
        private readonly List<int> _handSlots = new List<int>();
        private readonly List<string> _differences = new List<string>();

        /// <summary>The board cells whose view differs from the sim's cell, in board order.</summary>
        public IReadOnlyList<GridPos> Cells => _cells;

        /// <summary>The hand slots whose view shows another piece than the sim's hand.</summary>
        public IReadOnlyList<int> HandSlots => _handSlots;

        public bool ScoreDiffers { get; private set; }

        public bool ComboDiffers { get; private set; }

        public bool IsEmpty => _differences.Count == 0;

        private ViewDrift()
        {
        }

        /// <summary>Compares what the views show with the sim's board, hand, score and combo.</summary>
        public static ViewDrift Find(ShownState shown, IReadOnlyBoard board, IReadOnlyHand hand, int score, int combo)
        {
            var drift = new ViewDrift();

            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    var pos = new GridPos(x, y);
                    string simType = board.GetCell(pos).TypeId;
                    string shownType = shown.Cells[pos];

                    if (shownType != simType)
                    {
                        drift._cells.Add(pos);
                        drift._differences.Add($"cell {pos}: shows {Describe(shownType)}, sim has {Describe(simType)}");
                    }
                }
            }

            for (int slot = 0; slot < hand.Size; slot++)
            {
                PieceInstance simPiece = hand.GetSlot(slot);
                PieceInstance shownPiece = slot < shown.Hand.Length ? shown.Hand[slot] : null;

                if (InstanceId(shownPiece) != InstanceId(simPiece))
                {
                    drift._handSlots.Add(slot);
                    drift._differences.Add($"hand slot {slot}: shows {Describe(shownPiece)}, sim has {Describe(simPiece)}");
                }
            }

            if (shown.Score != score)
            {
                drift.ScoreDiffers = true;
                drift._differences.Add($"score: shows {shown.Score}, sim has {score}");
            }

            if (shown.Combo != combo)
            {
                drift.ComboDiffers = true;
                drift._differences.Add($"combo: shows {shown.Combo}, sim has {combo}");
            }

            return drift;
        }

        /// <summary>One line per difference, or a note that there is none.</summary>
        public override string ToString()
        {
            if (IsEmpty)
                return "Views match the sim.";

            var text = new StringBuilder($"Views differ from the sim in {_differences.Count} place(s) after playback:");

            foreach (string difference in _differences)
                text.Append("\n  ").Append(difference);

            return text.ToString();
        }

        private static int InstanceId(PieceInstance piece)
        {
            return piece != null ? piece.InstanceId : -1;
        }

        private static string Describe(string cellType)
        {
            return cellType == null ? "empty" : $"'{cellType}'";
        }

        private static string Describe(PieceInstance piece)
        {
            return piece == null ? "nothing" : $"piece #{piece.InstanceId} ({piece.Shape.Id})";
        }
    }
}
#endif
