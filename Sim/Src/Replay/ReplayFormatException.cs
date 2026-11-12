using System;

namespace Squarely.Simulation
{
    /// <summary>Thrown by <see cref="ReplayReader"/> on bad data. <see cref="ReplayCodec.Decode(string)"/> catches it.</summary>
    public sealed class ReplayFormatException : Exception
    {
        public ReplayFormatException(string message)
            : base(message)
        {
        }
    }
}
