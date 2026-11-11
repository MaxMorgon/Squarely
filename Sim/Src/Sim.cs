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

        #region State

        public ulong Seed { get; }

        /// <summary>Current RNG state, for snapshots. The RNG itself is not exposed, so nothing outside can advance it.</summary>
        public ulong RandomState => _random.State;

        public IReadOnlyBoard Board => _board;

        public IReadOnlyHand Hand => _hand;

        /// <summary>All patterns that clear when matched: rows, columns, then custom patterns.</summary>
        public IReadOnlyList<BoardPattern> Patterns => _patterns;

        /// <summary>Pieces dealt so far, including the opening hand. The next piece gets instance id PiecesDealt + 1.</summary>
        public int PiecesDealt { get; private set; }

        public int Score { get; private set; }

        /// <summary>Number of consecutive placements that matched a pattern. 0 = no active combo.</summary>
        public int Combo { get; private set; }

        /// <summary>Null while the game is in progress.</summary>
        public GameResolution Resolution { get; private set; }

        public bool IsResolved => Resolution != null;

        /// <summary>Mutable board for tests and in-assembly code only.</summary>
        internal Board EditableBoard => _board;

        #endregion

        /// <summary>A new game with the built-in rules. See <see cref="Sim(SimConfig, ulong, RuleFactory)"/>.</summary>
        public Sim(SimConfig config, ulong seed)
            : this(config, seed, RuleFactory.CreateDefault())
        {
        }

        /// <summary>
        /// A new game, dealt and ready to play. The sim keeps its own copy of the config:
        /// later edits to <paramref name="config"/> don't affect it.
        /// </summary>
        /// <param name="rules">Creates the dealer, scoring and resolution rules named in the config.</param>
        public Sim(SimConfig config, ulong seed, RuleFactory rules)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            if (rules == null)
                throw new ArgumentNullException(nameof(rules));

            config = config.Clone();
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
            DealNewHand(new List<ISimEvent>());
            CheckResolution(new List<ISimEvent>());
        }

        /// <summary>
        /// Copies all mutable state from the source. Rules and patterns are shared: rules are stateless, patterns are immutable,
        /// and the config the rules were built from is the source's own copy, which nothing edits.
        /// </summary>
        private Sim(Sim source)
        {
            Seed = source.Seed;
            _random = source._random.Clone();
            _board = source._board.Clone();
            _hand = source._hand.Clone();
            _patterns = source._patterns;

            _dealer = source._dealer;
            _scoringRule = source._scoringRule;
            _resolutionRules = source._resolutionRules;

            PiecesDealt = source.PiecesDealt;
            Score = source.Score;
            Combo = source.Combo;
            Resolution = source.Resolution;
        }

        public CommandResult Execute(ISimCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return command.Execute(this);
        }

        /// <summary>
        /// A full run of the command, without changing this sim: the result is exactly what <see cref="Execute"/> would return now,
        /// with every event it would produce.
        /// </summary>
        public CommandResult Preview(ISimCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return Clone().Execute(command);
        }

        /// <summary>
        /// An independent sim in the same state. Commands executed on either one never affect the other.
        /// </summary>
        public Sim Clone()
        {
            return new Sim(this);
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
            if (_hand.IsEmpty)
                DealNewHand(events);

            CheckResolution(events);

            return CommandResult.Succeeded(events);
        }

        private List<PatternMatch> ClearMatches(List<ISimEvent> events)
        {
            List<PatternMatch> matches = PatternMatcher.FindMatches(_board, _patterns);

            if (matches.Count == 0)
                return matches;

            // Per-match cells are collected before clearing, since a cell shared by two matches is cleared once but belongs to both.
            var clearedMatches = new List<ClearedMatch>(matches.Count);
            var positions = new List<GridPos>();

            foreach (PatternMatch match in matches)
            {
                clearedMatches.Add(new ClearedMatch(match, FilledInBoardOrder(match.Positions)));
                positions.AddRange(match.Positions);
            }

            IReadOnlyList<GridPos> clearedCells = _board.ClearCells(positions);
            events.Add(new PatternsClearedEvent(clearedMatches, clearedCells));

            return matches;
        }

        private List<GridPos> FilledInBoardOrder(IReadOnlyList<GridPos> positions)
        {
            var filled = new List<GridPos>();

            foreach (GridPos pos in positions)
            {
                if (!_board.IsEmpty(pos))
                    filled.Add(pos);
            }

            filled.Sort(CompareBoardOrder);
            return filled;
        }

        /// <summary>Bottom row first, left to right, the same order as <see cref="Board.ClearCells"/>.</summary>
        private static int CompareBoardOrder(GridPos a, GridPos b)
        {
            if (a.Y != b.Y)
                return a.Y.CompareTo(b.Y);

            return a.X.CompareTo(b.X);
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

        /// <summary>Deals a piece into every slot of the empty hand.</summary>
        private void DealNewHand(List<ISimEvent> events)
        {
            IReadOnlyList<PieceShape> shapes = _dealer.DealHand(this, _random);

            if (shapes.Count != _hand.Size)
                throw new InvalidOperationException($"Dealer dealt {shapes.Count} pieces for a hand of {_hand.Size}.");

            foreach (PieceShape shape in shapes)
            {
                var piece = new PieceInstance(PiecesDealt + 1, shape);
                PiecesDealt++;

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
