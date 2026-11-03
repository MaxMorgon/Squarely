using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// One play session of the puzzle. Owns all gameplay state and runs the turn flow.
    /// Change state only through <see cref="Execute"/>; read it freely through the properties.
    /// </summary>
    public sealed class Sim
    {
        private readonly IPieceDealer _dealer;
        private readonly IScoringRule _scoringRule;

        public Sim(SimConfig config, ulong seed)
            : this(config, seed, null, null)
        {
        }

        /// <param name="dealer">Null uses a <see cref="WeightedRandomDealer"/> over the config's pieces.</param>
        /// <param name="scoringRule">Null uses a <see cref="DefaultScoringRule"/> with the config's scoring.</param>
        public Sim(SimConfig config, ulong seed, IPieceDealer dealer, IScoringRule scoringRule)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            config.Validate();

            Config = config;
            Seed = seed;
            Random = new DeterministicRandom(seed);
            Board = new Board(config.BoardWidth, config.BoardHeight);
            Hand = new Hand(config.HandSize);

            _dealer = dealer ?? new WeightedRandomDealer(config.Pieces);
            _scoringRule = scoringRule ?? new DefaultScoringRule(config.Scoring);

            // The opening hand is part of the initial state; the view reads it directly.
            RefillHand(new List<ISimEvent>());
            UpdateGameOver(new List<ISimEvent>());
        }

        #region State

        public SimConfig Config { get; }

        public ulong Seed { get; }

        public DeterministicRandom Random { get; }

        public Board Board { get; }

        public Hand Hand { get; }

        public int Score { get; private set; }

        /// <summary>Number of consecutive placements that cleared lines. 0 = no active combo.</summary>
        public int Combo { get; private set; }

        public bool IsGameOver { get; private set; }

        #endregion

        public CommandResult Execute(ISimCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return command.Execute(this);
        }

        #region Turn flow

        internal CommandResult PlacePiece(int slot, GridPos origin)
        {
            if (IsGameOver)
                return CommandResult.Failed("The game is over.");

            if (!Hand.IsValidSlot(slot))
                return CommandResult.Failed($"Slot {slot} does not exist.");

            if (Hand.IsEmpty(slot))
                return CommandResult.Failed($"Slot {slot} is empty.");

            PieceShape piece = Hand.Get(slot);

            if (!Board.CanPlace(piece, origin))
                return CommandResult.Failed($"Piece '{piece.Id}' does not fit at {origin}.");

            var events = new List<ISimEvent>();

            Hand.Take(slot);
            Board.Place(piece, origin, Cell.Block);
            events.Add(new PiecePlacedEvent(slot, piece, origin));

            BoardLines lines = Board.FindFullLines();

            if (!lines.IsEmpty)
            {
                IReadOnlyList<GridPos> clearedCells = Board.ClearLines(lines);
                events.Add(new LinesClearedEvent(lines, clearedCells));
            }

            ApplyScore(piece.CellCount, lines.Count, events);
            RefillHand(events);
            UpdateGameOver(events);

            return CommandResult.Succeeded(events);
        }

        private void ApplyScore(int cellsPlaced, int linesCleared, List<ISimEvent> events)
        {
            ScoreResult result = _scoringRule.Score(cellsPlaced, linesCleared, Combo);

            if (result.Points != 0)
            {
                Score += result.Points;
                events.Add(new ScoreChangedEvent(result.Points, Score));
            }

            if (result.NewCombo != Combo)
            {
                int previous = Combo;
                Combo = result.NewCombo;
                events.Add(new ComboChangedEvent(previous, Combo));
            }
        }

        private void RefillHand(List<ISimEvent> events)
        {
            for (int slot = 0; slot < Hand.Size; slot++)
            {
                if (!Hand.IsEmpty(slot))
                    continue;

                PieceShape piece = _dealer.Deal(Board, Hand, Random);
                Hand.Set(slot, piece);
                events.Add(new PieceDealtEvent(slot, piece));
            }
        }

        private void UpdateGameOver(List<ISimEvent> events)
        {
            for (int slot = 0; slot < Hand.Size; slot++)
            {
                PieceShape piece = Hand.Get(slot);

                if (piece != null && Board.CanPlaceAnywhere(piece))
                    return;
            }

            IsGameOver = true;
            events.Add(new GameOverEvent(Score));
        }

        #endregion
    }
}
