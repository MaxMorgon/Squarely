using System.Collections.Generic;

namespace Squarely.Simulation.Tests
{
    public static class ReplayTestHelpers
    {
        /// <summary>
        /// Plays valid moves with a seeded choice among all valid placements, until the game resolves or maxMoves is reached.
        /// </summary>
        public static GameReplay RecordGame(SimConfig config, ulong seed, int maxMoves = 500, bool endWithEndGame = false)
        {
            var sim = new Sim(config, seed);
            var recorder = new ReplayRecorder(sim);
            var choice = new System.Random((int)seed);

            for (int move = 0; move < maxMoves && !sim.IsResolved; move++)
            {
                List<PlacePieceCommand> moves = TestHelpers.AllValidMoves(sim);
                recorder.Execute(moves[choice.Next(moves.Count)]);
            }

            if (endWithEndGame)
                recorder.Execute(new EndGameCommand());

            return recorder.Build();
        }
    }
}
