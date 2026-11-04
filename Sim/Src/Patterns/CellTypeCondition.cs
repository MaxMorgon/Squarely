using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Matches cells of one specific type.
    /// </summary>
    public sealed class CellTypeCondition : ICellCondition
    {
        public string TypeId { get; }

        public CellTypeCondition(string typeId)
        {
            if (string.IsNullOrEmpty(typeId))
                throw new ArgumentException("Type id is required.", nameof(typeId));

            TypeId = typeId;
        }

        public bool Matches(Cell cell)
        {
            return string.Equals(cell.TypeId, TypeId, StringComparison.Ordinal);
        }
    }
}
