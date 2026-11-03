using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// One play session of the puzzle. Owns all gameplay state and runs the turn flow.
    /// Outside code reads state through read-only views and changes it only through <see cref="Execute"/>.
    /// </summary>
    public sealed class Sim
    {
        private readonly Board _board;
        private readonly Hand _hand;
        private readonly DeterministicRandom _random;
        private readonly IPieceDealer _dealer;
        private readonly IScoringRule _scoringRule;
        private readonly List<IResolutionRule> _resolutionRules;
        private readonly List<BoardPattern> _patterns;

        private int _nextInstanceId = 1;

        public Sim(SimConfig config, ulong seed)
            : this(config, seed, RuleFactory.CreateDefault())
        {
        }

        /// <param name="rules">Creates the dealer, scoring and resolution rules named in the config.</param>
        public Sim(SimConfig config, ulong seed, RuleFactory rules)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            config.Validate();

            Seed = seed;
            _random = new DeterministicRandom(seed);
            _board = new Board(config.BoardWidth, config.BoardHeight);
            _hand = new Hand(config.HandSize);
            _patterns = config.Clearing.BuildPatterns(config.BoardWidth, config.BoardHeight);

            _dealer = rules.CreateDealer(config);
            _scoringRule = rules.CreateScoringRule(config);
            _resolutionRules = rules.CreateResolutionRules(config);

            // The opening hand is part of the initial state; the view reads it directly.
            RefillHand(new List<ISimEvent>());
            CheckResolution(new List<ISimEvent>());
        }

        #region State

        public ulong Seed { get; }

        /// <summary>Current RNG state, for snapshots. The RNG itself is not exposed, so nothing outside can advance it.</summary>
        public ulong RandomState => _random.State;

        public IReadOnlyBoard Board => _board;

        public IReadOnlyHand Hand => _hand;

        /// <summary>All patterns that clear when matched: rows, columns, then custom patterns.</summary>
        public IReadOnlyList<BoardPattern> Patterns => _patterns;

        public int Score { get; private set; }

        /// <summary>Number of consecutive placements that matched a pattern. 0 = no active combo.</summary>
        public int Combo { get; private set; }

        /// <summary>Null while the game is in progress.</summary>
        public GameResolution Resolution { get; private set; }

        public bool IsResolved => Resolution != null;

        /// <summary>Mutable board for tests and in-assembly code only.</summary>
        internal Board EditableBoard => _board;

        #endregion

        public CommandResult Execute(ISimCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return command.Execute(this);
        }

        #region Turn flow

        internal CommandResult PlacePiece(int instanceId, GridPos origin)
        {
            if (IsResolved)
                return CommandResult.Failed("The game is already resolved.");

            int slot = _hand.FindSlot(instanceId);

            if (slot < 0)
                return CommandResult.Failed($"Piece #{instanceId} is not in the hand.");

            PieceInstance piece = _hand.GetSlot(slot);

            if (!_board.CanPlace(piece.Shape, origin))
                return CommandResult.Failed($"Piece '{piece}' does not fit at {origin}.");

            var events = new List<ISimEvent>();

            _hand.Take(instanceId);
            _board.Place(piece.Shape, origin);
            events.Add(new PiecePlacedEvent(piece, slot, origin));

            List<PatternMatch> matches = ClearMatches(events);

            ApplyScore(piece.Shape.CellCount, matches, events);
            RefillHand(events);
            CheckResolution(events);

            return CommandResult.Succeeded(events);
        }

        private List<PatternMatch> ClearMatches(List<ISimEvent> events)
        {
            List<PatternMatch> matches = PatternMatcher.FindMatches(_board, _patterns);

            if (matches.Count == 0)
                return matches;

            var positions = new List<GridPos>();

            foreach (PatternMatch match in matches)
                positions.AddRange(match.Positions);

            IReadOnlyList<GridPos> clearedCells = _board.ClearCells(positions);
            events.Add(new PatternsClearedEvent(matches, clearedCells));

            return matches;
        }

        private void ApplyScore(int cellsPlaced, IReadOnlyList<PatternMatch> matches, List<ISimEvent> events)
        {
            ScoreResult result = _scoringRule.Score(cellsPlaced, matches, Combo);

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
            for (int slot = 0; slot < _hand.Size; slot++)
            {
                if (!_hand.IsSlotEmpty(slot))
                    continue;

                PieceShape shape = _dealer.Deal(_board, _hand, _random);
                var piece = new PieceInstance(_nextInstanceId, shape);
                _nextInstanceId++;

                int dealtSlot = _hand.Add(piece);
                events.Add(new PieceDealtEvent(piece, dealtSlot));
            }
        }

        private void CheckResolution(List<ISimEvent> events)
        {
            foreach (IResolutionRule rule in _resolutionRules)
            {
                GameResolution resolution = rule.Check(this);

                if (resolution == null)
                    continue;

                Resolution = resolution;
                events.Add(new GameResolvedEvent(resolution, Score));
                return;
            }
        }

        #endregion
    }
}
