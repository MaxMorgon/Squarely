using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Tunable values for the default dealer (<see cref="WeightedRandomDealer"/>).
    /// </summary>
    [Serializable]
    public sealed class DealingConfig
    {
        /// <summary>
        /// Every dealt hand holds at least one helpful piece for the board at the moment of dealing:
        /// one that can complete a pattern, or, if no piece in the pool can, one that fits.
        /// If no piece in the pool fits, the hand is dealt purely by weight.
        /// </summary>
        public bool GuaranteeHelpfulPiece = false;

        /// <summary>An independent copy: edits to either one don't affect the other.</summary>
        internal DealingConfig Clone()
        {
            return (DealingConfig)MemberwiseClone();
        }
    }
}
