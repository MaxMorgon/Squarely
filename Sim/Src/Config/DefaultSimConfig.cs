using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// The built-in config: 8x8 board, hand of 3, standard piece set. Placeholder values until real tuning.
    /// Pieces cannot rotate, so each orientation is its own entry.
    /// </summary>
    public static class DefaultSimConfig
    {
        public static SimConfig Create()
        {
            return new SimConfig
            {
                BoardWidth = 8,
                BoardHeight = 8,
                HandSize = 3,
                Pieces = CreatePieces(),
                Scoring = new ScoringConfig(),
            };
        }

        private static List<PieceDefinition> CreatePieces()
        {
            return new List<PieceDefinition>
            {
                #region Dots and lines

                new PieceDefinition("dot", "#"),
                new PieceDefinition("line2_h", "##"),
                new PieceDefinition("line2_v", "#/#"),
                new PieceDefinition("line3_h", "###"),
                new PieceDefinition("line3_v", "#/#/#"),
                new PieceDefinition("line4_h", "####"),
                new PieceDefinition("line4_v", "#/#/#/#"),
                new PieceDefinition("line5_h", "#####"),
                new PieceDefinition("line5_v", "#/#/#/#/#"),

                #endregion

                #region Squares and rectangles

                new PieceDefinition("square2", "##/##"),
                new PieceDefinition("square3", "###/###/###"),
                new PieceDefinition("rect2x3", "##/##/##"),
                new PieceDefinition("rect3x2", "###/###"),

                #endregion

                #region Small corners (3 cells)

                new PieceDefinition("corner_bl", "#./##"),
                new PieceDefinition("corner_br", ".#/##"),
                new PieceDefinition("corner_tl", "##/#."),
                new PieceDefinition("corner_tr", "##/.#"),

                #endregion

                #region Big corners (5 cells)

                new PieceDefinition("bigcorner_bl", "#../#../###"),
                new PieceDefinition("bigcorner_br", "..#/..#/###"),
                new PieceDefinition("bigcorner_tl", "###/#../#.."),
                new PieceDefinition("bigcorner_tr", "###/..#/..#"),

                #endregion

                #region L shapes (4 cells)

                new PieceDefinition("l_1", "#./#./##"),
                new PieceDefinition("l_2", "###/#.."),
                new PieceDefinition("l_3", "##/.#/.#"),
                new PieceDefinition("l_4", "..#/###"),
                new PieceDefinition("j_1", ".#/.#/##"),
                new PieceDefinition("j_2", "#../###"),
                new PieceDefinition("j_3", "##/#./#."),
                new PieceDefinition("j_4", "###/..#"),

                #endregion

                #region T shapes

                new PieceDefinition("t_down", "###/.#."),
                new PieceDefinition("t_up", ".#./###"),
                new PieceDefinition("t_right", "#./##/#."),
                new PieceDefinition("t_left", ".#/##/.#"),

                #endregion

                #region S and Z shapes

                new PieceDefinition("s_h", ".##/##."),
                new PieceDefinition("z_h", "##./.##"),
                new PieceDefinition("s_v", "#./##/.#"),
                new PieceDefinition("z_v", ".#/##/#."),

                #endregion
            };
        }
    }
}
