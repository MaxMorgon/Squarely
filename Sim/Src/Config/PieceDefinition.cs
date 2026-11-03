namespace Squarely.Simulation
{
    /// <summary>
    /// Config data for one piece in the deal pool.
    /// </summary>
    public sealed class PieceDefinition
    {
        public PieceDefinition()
        {
        }

        public PieceDefinition(string id, string pattern, int weight = 1)
        {
            Id = id;
            Pattern = pattern;
            Weight = weight;
        }

        public string Id { get; set; }

        /// <summary>
        /// Rows separated by '/', top row first. '#' = filled, '.' = empty. See <see cref="PieceShape.FromPattern"/>.
        /// </summary>
        public string Pattern { get; set; }

        /// <summary>
        /// Relative chance of being dealt. 0 removes the piece from the pool.
        /// </summary>
        public int Weight { get; set; } = 1;
    }
}
