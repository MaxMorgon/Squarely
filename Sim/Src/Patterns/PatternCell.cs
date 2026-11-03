using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// One cell of a board pattern: where it is relative to the pattern origin, and what it must contain.
    /// </summary>
    public readonly struct PatternCell
    {
        public readonly GridPos Offset;
        public readonly ICellCondition Condition;

        public PatternCell(GridPos offset, ICellCondition condition)
        {
            Offset = offset;
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        }
    }
}
