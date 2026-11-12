using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>Writes the compact binary form of a replay: varints and fixed 8-byte values.</summary>
    public sealed class ReplayWriter
    {
        private readonly List<byte> _bytes = new List<byte>();

        public byte[] ToArray()
        {
            return _bytes.ToArray();
        }

        public void WriteByte(byte value)
        {
            _bytes.Add(value);
        }

        /// <summary>Unsigned, 7 bits per byte, low bits first.</summary>
        public void WriteVarUInt(ulong value)
        {
            while (value >= 0x80)
            {
                _bytes.Add((byte)(value | 0x80));
                value >>= 7;
            }

            _bytes.Add((byte)value);
        }

        /// <summary>Small values of either sign stay short (zigzag).</summary>
        public void WriteVarInt(int value)
        {
            WriteVarUInt((uint)((value << 1) ^ (value >> 31)));
        }

        public void WriteFixedUInt64(ulong value)
        {
            for (int i = 0; i < 8; i++)
                _bytes.Add((byte)(value >> (8 * i)));
        }
    }
}
