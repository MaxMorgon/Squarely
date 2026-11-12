using System;

namespace Squarely.Simulation
{
    /// <summary>Reads and writes one command type in a replay. Register it in <see cref="CommandCodecRegistry"/>.</summary>
    public interface ICommandCodec
    {
        /// <summary>The id written before the command's fields. Must be unique and never change.</summary>
        int Tag { get; }

        Type CommandType { get; }

        void Write(ReplayWriter writer, ISimCommand command);

        /// <summary>Throws <see cref="ReplayFormatException"/> on bad data.</summary>
        ISimCommand Read(ReplayReader reader);
    }
}
