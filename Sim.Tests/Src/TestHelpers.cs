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
    }
}
