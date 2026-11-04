using System;
using System.Collections.Generic;

namespace Squarely.Simulation.Tests
{
    public static class TestHelpers
    {
        #region Setup

        /// <summary>
        /// Fills cells from a pattern (same format as pieces: '/' rows, top first, '#' filled),
        /// with the pattern's bottom-left at the board's bottom-left.
        /// </summary>
        public static void Fill(Board board, string pattern)
        {
            string[] rows = pattern.Split('/');

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                int y = rows.Length - 1 - rowIndex;

                for (int x = 0; x < rows[rowIndex].Length; x++)
                {
                    if (rows[rowIndex][x] == '#')
                        board.SetCell(new GridPos(x, y), Cell.Block);
                }
            }
        }

        public static SimConfig SmallConfig(int width, int height, int handSize)
        {
            return new SimConfig
            {
                BoardWidth = width,
                BoardHeight = height,
                HandSize = handSize,
                Pieces = new List<PieceDefinition> { new PieceDefinition("dot", "#") },
            };
        }

        /// <summary>
        /// Creates a sim that deals from the given scripted dealer, registered through the rule factory.
        /// </summary>
        public static Sim CreateSim(SimConfig config, ScriptedDealer dealer, ulong seed = 1)
        {
            RuleFactory rules = RuleFactory.CreateDefault();
            rules.RegisterDealer(ScriptedDealer.Id, _ => dealer);
            config.DealerId = ScriptedDealer.Id;

            return new Sim(config, seed, rules);
        }

        public static int SlotInstanceId(Sim sim, int slot)
        {
            return sim.Hand.GetSlot(slot).InstanceId;
        }

        /// <summary>
        /// The first piece, in slot order, at the first origin where it fits (bottom row first, left to right).
        /// </summary>
        public static PlacePieceCommand FirstValidMove(Sim sim)
        {
            for (int slot = 0; slot < sim.Hand.Size; slot++)
            {
                PieceInstance piece = sim.Hand.GetSlot(slot);

                for (int y = 0; y < sim.Board.Height; y++)
                {
                    for (int x = 0; x < sim.Board.Width; x++)
                    {
                        var origin = new GridPos(x, y);

                        if (sim.Board.CanPlace(piece.Shape, origin))
                            return new PlacePieceCommand(piece.InstanceId, origin);
                    }
                }
            }

            throw new InvalidOperationException("No valid move although the game is not resolved.");
        }

        #endregion

        #region Event queries

        public static T Single<T>(IReadOnlyList<ISimEvent> events) where T : class, ISimEvent
        {
            T found = null;

            foreach (ISimEvent simEvent in events)
            {
                if (!(simEvent is T match))
                    continue;

                if (found != null)
                    throw new InvalidOperationException($"More than one {typeof(T).Name}.");

                found = match;
            }

            if (found == null)
                throw new InvalidOperationException($"No {typeof(T).Name} found.");

            return found;
        }

        #endregion

        #region Comparison

        /// <summary>
        /// A readable trace of a command result, one line per event with all its data, so two results can be compared with one assert.
        /// </summary>
        public static List<string> Describe(CommandResult result)
        {
            var lines = new List<string> { $"success {result.Success}, reason {result.FailureReason}" };

            foreach (ISimEvent simEvent in result.Events)
                lines.Add(Describe(simEvent));

            return lines;
        }

        /// <summary>
        /// A readable dump of all sim state, so tests can check that nothing changed.
        /// </summary>
        public static List<string> DescribeState(Sim sim)
        {
            var lines = new List<string>
            {
                $"score {sim.Score}, combo {sim.Combo}, dealt {sim.PiecesDealt}, rng {sim.RandomState}, resolution {sim.Resolution}",
            };

            for (int slot = 0; slot < sim.Hand.Size; slot++)
                lines.Add($"slot {slot}: {sim.Hand.GetSlot(slot)}");

            for (int y = sim.Board.Height - 1; y >= 0; y--)
            {
                string row = "";

                for (int x = 0; x < sim.Board.Width; x++)
                    row += sim.Board.IsEmpty(new GridPos(x, y)) ? '.' : '#';

                lines.Add(row);
            }

            return lines;
        }

        private static string Describe(ISimEvent simEvent)
        {
            switch (simEvent)
            {
                case PiecePlacedEvent placed:
                    return $"placed {placed.Piece} from slot {placed.Slot} at {placed.Origin}";

                case PatternsClearedEvent cleared:
                    return $"cleared [{string.Join(", ", cleared.Matches)}] cells [{string.Join(", ", cleared.ClearedCells)}]";

                case ScoreChangedEvent score:
                    return $"score +{score.Delta} = {score.Total}";

                case ComboChangedEvent combo:
                    return $"combo {combo.Previous} -> {combo.Current}";

                case PieceDealtEvent dealt:
                    return $"dealt {dealt.Piece} to slot {dealt.Slot}";

                case GameResolvedEvent resolved:
                    return $"resolved {resolved.Resolution}, final score {resolved.FinalScore}";

                default:
                    throw new InvalidOperationException($"Describe does not know {simEvent.GetType().Name}.");
            }
        }

        #endregion
    }
}
