using System.Collections.Generic;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>The views event playback updates, and where effects are spawned.</summary>
    public sealed class PlaybackViews
    {
        public LayerView Board { get; }

        public HandView Hand { get; }

        public ScoreView Score { get; }

        /// <summary>Parent of spawned effects such as particles.</summary>
        public Transform EffectsRoot { get; }

        /// <summary>Plays the sounds of cues, or null if the game has none.</summary>
        public SoundPlayer Sound { get; }

        public PlaybackViews(LayerView board, HandView hand, ScoreView score, Transform effectsRoot, SoundPlayer sound = null)
        {
            Sound = sound;
            Board = board;
            Hand = hand;
            Score = score;
            EffectsRoot = effectsRoot;
        }

        /// <summary>World position of the center of a group of board cells.</summary>
        public Vector3 CenterOf(IReadOnlyList<GridPos> cells)
        {
            if (cells.Count == 0)
                return Board.transform.position;

            Vector3 sum = Vector3.zero;

            foreach (GridPos cell in cells)
                sum += Board.CellWorldPosition(cell);

            return sum / cells.Count;
        }
    }
}
