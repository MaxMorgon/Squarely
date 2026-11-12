namespace Squarely.Simulation
{
    /// <summary>Reads what <see cref="ReplayWriter"/> wrote. Throws <see cref="ReplayFormatException"/> on bad data.</summary>
    public sealed class ReplayReader
    {
        private readonly byte[] _bytes;
        private int _position;

        public int Remaining => _bytes.Length - _position;

        public ReplayReader(byte[] bytes)
        {
            _bytes = bytes;
        }

        public byte ReadByte()
        {
            if (_position >= _bytes.Length)
                throw new ReplayFormatException("Unexpected end of data.");

            return _bytes[_position++];
        }

        public ulong ReadVarUInt()
        {
            ulong result = 0;

            for (int shift = 0; shift < 70; shift += 7)
            {
                byte b = ReadByte();

                if (shift == 63 && b > 1)
                    throw new ReplayFormatException("Number is too large.");

                result |= (ulong)(b & 0x7F) << shift;

                if ((b & 0x80) == 0)
                    return result;
            }

            throw new ReplayFormatException("Number is too long.");
        }

        public int ReadVarInt()
        {
            ulong raw = ReadVarUInt();

            if (raw > uint.MaxValue)
                throw new ReplayFormatException("Number is too large.");

            uint value = (uint)raw;
            return (int)(value >> 1) ^ -(int)(value & 1);
        }

        /// <summary>A non-negative int, written as a plain varint.</summary>
        public int ReadVarCount()
        {
            ulong value = ReadVarUInt();

            if (value > int.MaxValue)
                throw new ReplayFormatException("Number is too large.");

            return (int)value;
        }

        public ulong ReadFixedUInt64()
        {
            ulong result = 0;

            for (int i = 0; i < 8; i++)
                result |= (ulong)ReadByte() << (8 * i);

            return result;
        }
    }
}
