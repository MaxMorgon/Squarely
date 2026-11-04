using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// One dealt piece. The instance id is unique within a sim session (assigned in deal order),
    /// so two identical shapes in the hand can still be told apart by commands.
    /// </summary>
    public sealed class PieceInstance
    {
        public int InstanceId { get; }

        public PieceShape Shape { get; }

        public PieceInstance(int instanceId, PieceShape shape)
        {
            InstanceId = instanceId;
            Shape = shape ?? throw new ArgumentNullException(nameof(shape));
        }

        public override string ToString()
        {
            return $"{Shape.Id}#{InstanceId}";
        }
    }
}
