using System;

namespace Squarely.Simulation
{
    /// <summary>
    /// Turns a replay into a short text for copy and paste, and back.
    /// Format: "SQ" + format version + "." + base64url (no padding) of these bytes:
    /// seed (varint), config fingerprint (8 bytes), end score (zigzag varint), resolved flag (1 byte),
    /// command count (varint), then per command its tag (varint) and fields (see <see cref="ICommandCodec"/>).
    /// </summary>
    public static class ReplayCodec
    {
        private const string PrefixStart = "SQ";
        private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

        public static string Encode(GameReplay replay)
        {
            return Encode(replay, CommandCodecRegistry.CreateDefault());
        }

        /// <summary>Throws <see cref="ArgumentException"/> if a command type is not in the registry.</summary>
        public static string Encode(GameReplay replay, CommandCodecRegistry registry)
        {
            if (replay == null)
                throw new ArgumentNullException(nameof(replay));

            var writer = new ReplayWriter();
            writer.WriteVarUInt(replay.Seed);
            writer.WriteFixedUInt64(replay.ConfigFingerprint);
            writer.WriteVarInt(replay.EndScore);
            writer.WriteByte(replay.EndIsResolved ? (byte)1 : (byte)0);
            writer.WriteVarUInt((ulong)replay.Commands.Count);

            foreach (ISimCommand command in replay.Commands)
            {
                if (!registry.TryGetByType(command.GetType(), out ICommandCodec codec))
                    throw new ArgumentException($"No codec for command type {command.GetType().Name}.");

                writer.WriteVarUInt((ulong)codec.Tag);
                codec.Write(writer, command);
            }

            return PrefixStart + GameReplay.CurrentVersion + "." + ToBase64Url(writer.ToArray());
        }

        public static ReplayDecodeResult Decode(string code)
        {
            return Decode(code, CommandCodecRegistry.CreateDefault());
        }

        /// <summary>Never throws on bad input: failures come back in the result.</summary>
        public static ReplayDecodeResult Decode(string code, CommandCodecRegistry registry)
        {
            code = code?.Trim();

            if (string.IsNullOrEmpty(code) || !code.StartsWith(PrefixStart, StringComparison.Ordinal))
                return ReplayDecodeResult.Failed(ReplayDecodeFailure.WrongPrefix, "The code does not start with 'SQ'.");

            int dot = code.IndexOf('.');

            if (dot < 0 || !int.TryParse(code.Substring(PrefixStart.Length, dot - PrefixStart.Length), out int version))
                return ReplayDecodeResult.Failed(ReplayDecodeFailure.WrongPrefix, "The code has no version prefix.");

            if (version != GameReplay.CurrentVersion)
                return ReplayDecodeResult.Failed(ReplayDecodeFailure.UnknownVersion, $"Replay version {version} is not supported.");

            byte[] bytes = FromBase64Url(code.Substring(dot + 1));

            if (bytes == null)
                return ReplayDecodeResult.Failed(ReplayDecodeFailure.CorruptData, "The code is not valid base64url.");

            try
            {
                return ReadReplay(new ReplayReader(bytes), version, registry);
            }
            catch (ReplayFormatException exception)
            {
                return ReplayDecodeResult.Failed(ReplayDecodeFailure.CorruptData, exception.Message);
            }
        }

        private static ReplayDecodeResult ReadReplay(ReplayReader reader, int version, CommandCodecRegistry registry)
        {
            ulong seed = reader.ReadVarUInt();
            ulong fingerprint = reader.ReadFixedUInt64();
            int score = reader.ReadVarInt();
            byte resolvedFlag = reader.ReadByte();

            if (resolvedFlag > 1)
                throw new ReplayFormatException("Invalid resolved flag.");

            int count = reader.ReadVarCount();

            // Each command takes at least one byte, so a larger count can't be true.
            if (count > reader.Remaining)
                throw new ReplayFormatException("Command count is larger than the data.");

            var commands = new ISimCommand[count];

            for (int i = 0; i < count; i++)
            {
                int tag = reader.ReadVarCount();

                if (!registry.TryGetByTag(tag, out ICommandCodec codec))
                    return ReplayDecodeResult.Failed(ReplayDecodeFailure.UnknownCommandTag, $"Unknown command tag {tag} at index {i}.");

                commands[i] = codec.Read(reader);
            }

            if (reader.Remaining != 0)
                throw new ReplayFormatException("Extra data after the last command.");

            return ReplayDecodeResult.Succeeded(new GameReplay(seed, fingerprint, commands, score, resolvedFlag == 1, version));
        }

        #region Base64url

        private static string ToBase64Url(byte[] bytes)
        {
            var chars = new char[(bytes.Length * 4 + 2) / 3];
            int outIndex = 0;

            for (int i = 0; i < bytes.Length; i += 3)
            {
                int b0 = bytes[i];
                int b1 = i + 1 < bytes.Length ? bytes[i + 1] : 0;
                int b2 = i + 2 < bytes.Length ? bytes[i + 2] : 0;
                int chunk = (b0 << 16) | (b1 << 8) | b2;

                chars[outIndex++] = Alphabet[(chunk >> 18) & 63];
                chars[outIndex++] = Alphabet[(chunk >> 12) & 63];

                if (i + 1 < bytes.Length)
                    chars[outIndex++] = Alphabet[(chunk >> 6) & 63];

                if (i + 2 < bytes.Length)
                    chars[outIndex++] = Alphabet[chunk & 63];
            }

            return new string(chars);
        }

        /// <summary>Returns null if the text is not valid unpadded base64url.</summary>
        private static byte[] FromBase64Url(string text)
        {
            if (text.Length % 4 == 1)
                return null;

            var bytes = new byte[text.Length * 3 / 4];
            int bits = 0;
            int bitCount = 0;
            int outIndex = 0;

            foreach (char c in text)
            {
                int value = Alphabet.IndexOf(c);

                if (value < 0)
                    return null;

                bits = (bits << 6) | value;
                bitCount += 6;

                if (bitCount >= 8)
                {
                    bitCount -= 8;
                    bytes[outIndex++] = (byte)(bits >> bitCount);
                    bits &= (1 << bitCount) - 1;
                }
            }

            // Leftover bits must be zero, so each byte array has exactly one text form.
            return bits == 0 ? bytes : null;
        }

        #endregion
    }
}
