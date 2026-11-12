using System.Collections.Generic;
using NUnit.Framework;

namespace Squarely.Simulation.Tests
{
    public class ReplayCodecTests
    {
        #region Round trip

        [Test]
        public void RoundTrip_EmptyReplay()
        {
            var replay = new GameReplay(42, 0xDEADBEEFCAFEF00DUL, new ISimCommand[0], 0, false);

            GameReplay decoded = DecodeOk(ReplayCodec.Encode(replay));

            AssertSame(replay, decoded);
        }

        [Test]
        public void RoundTrip_LongGame_WithBothCommandTypes()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 99, maxMoves: 8, endWithEndGame: true);

            Assert.That(replay.Commands.Count, Is.GreaterThan(5));
            Assert.That(replay.Commands[replay.Commands.Count - 1], Is.TypeOf<EndGameCommand>());
            Assert.That(replay.Commands[0], Is.TypeOf<PlacePieceCommand>());

            AssertSame(replay, DecodeOk(ReplayCodec.Encode(replay)));
        }

        [Test]
        public void RoundTrip_ExtremeValues()
        {
            var commands = new ISimCommand[]
            {
                new PlacePieceCommand(int.MaxValue, new GridPos(-5, 100000)),
                new EndGameCommand(),
            };
            var replay = new GameReplay(ulong.MaxValue, ulong.MaxValue, commands, -1234567, true);

            AssertSame(replay, DecodeOk(ReplayCodec.Encode(replay)));
        }

        [Test]
        public void Encode_StartsWithPrefix_AndUsesOnlyUrlSafeCharacters()
        {
            GameReplay replay = ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 3);

            string code = ReplayCodec.Encode(replay);

            Assert.That(code, Does.StartWith("SQ1."));
            Assert.That(code.Substring(4), Does.Match("^[A-Za-z0-9_-]*$"));
        }

        [Test]
        public void Encode_PlacementTakesAboutFourBytes()
        {
            var commands = new List<ISimCommand>();

            for (int id = 1; id <= 120; id++)
                commands.Add(new PlacePieceCommand(id, new GridPos(id % 8, id % 5)));

            string code = ReplayCodec.Encode(new GameReplay(1, 2, commands, 0, false));

            // 4 characters hold 3 bytes. The header is about 14 bytes.
            double bytes = (code.Length - 4) * 3.0 / 4.0;
            double bytesPerCommand = (bytes - 14) / commands.Count;

            Assert.That(bytesPerCommand, Is.LessThan(4.2), $"{bytesPerCommand:F2} bytes per command.");
        }

        [Test]
        public void Decode_IgnoresSurroundingWhitespace()
        {
            var replay = new GameReplay(1, 2, new ISimCommand[] { new EndGameCommand() }, 0, true);

            Assert.That(ReplayCodec.Decode("  " + ReplayCodec.Encode(replay) + "\n").Success, Is.True);
        }

        #endregion

        #region Bad input

        [Test]
        public void Decode_NullEmptyOrWrongPrefix_ReportsWrongPrefix()
        {
            AssertFailure(null, ReplayDecodeFailure.WrongPrefix);
            AssertFailure(string.Empty, ReplayDecodeFailure.WrongPrefix);
            AssertFailure("hello", ReplayDecodeFailure.WrongPrefix);
            AssertFailure("XQ1.AAAA", ReplayDecodeFailure.WrongPrefix);
            AssertFailure("SQ1AAAA", ReplayDecodeFailure.WrongPrefix);
            AssertFailure("SQx.AAAA", ReplayDecodeFailure.WrongPrefix);
        }

        [Test]
        public void Decode_UnknownVersion_ReportsUnknownVersion()
        {
            AssertFailure("SQ2.AAAA", ReplayDecodeFailure.UnknownVersion);
            AssertFailure("SQ0.AAAA", ReplayDecodeFailure.UnknownVersion);
        }

        [Test]
        public void Decode_BadCharacters_ReportsCorruptData()
        {
            AssertFailure("SQ1.AA+/", ReplayDecodeFailure.CorruptData);
            AssertFailure("SQ1.AA==", ReplayDecodeFailure.CorruptData);
            AssertFailure("SQ1.A", ReplayDecodeFailure.CorruptData);
        }

        [Test]
        public void Decode_TruncatedData_ReportsCorruptData()
        {
            string code = ReplayCodec.Encode(ReplayTestHelpers.RecordGame(DefaultSimConfig.Create(), 7));

            for (int length = 4; length < code.Length; length += 3)
            {
                ReplayDecodeResult result = ReplayCodec.Decode(code.Substring(0, length));

                Assert.That(result.Success, Is.False, $"Length {length}");
                Assert.That(result.Failure, Is.EqualTo(ReplayDecodeFailure.CorruptData), $"Length {length}");
            }
        }

        [Test]
        public void Decode_ExtraData_ReportsCorruptData()
        {
            var replay = new GameReplay(1, 2, new ISimCommand[] { new EndGameCommand() }, 0, true);
            string code = ReplayCodec.Encode(replay) + "AA";

            AssertFailure(code, ReplayDecodeFailure.CorruptData);
        }

        [Test]
        public void Decode_HugeCommandCount_ReportsCorruptData_WithoutAllocating()
        {
            var writer = new ReplayWriter();
            writer.WriteVarUInt(1);
            writer.WriteFixedUInt64(2);
            writer.WriteVarInt(0);
            writer.WriteByte(0);
            writer.WriteVarUInt(int.MaxValue);

            AssertFailure(CodeFromBytes(writer.ToArray()), ReplayDecodeFailure.CorruptData);
        }

        [Test]
        public void Decode_UnknownCommandTag_ReportsUnknownCommandTag()
        {
            var writer = new ReplayWriter();
            writer.WriteVarUInt(1);
            writer.WriteFixedUInt64(2);
            writer.WriteVarInt(0);
            writer.WriteByte(0);
            writer.WriteVarUInt(1);
            writer.WriteVarUInt(77);

            AssertFailure(CodeFromBytes(writer.ToArray()), ReplayDecodeFailure.UnknownCommandTag);
        }

        [Test]
        public void Decode_RandomGarbage_NeverThrows()
        {
            var random = new System.Random(1);
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_.+/=SQ";

            for (int i = 0; i < 2000; i++)
            {
                var chars = new char[random.Next(0, 60)];

                for (int c = 0; c < chars.Length; c++)
                    chars[c] = alphabet[random.Next(alphabet.Length)];

                string text = (i % 2 == 0 ? "SQ1." : string.Empty) + new string(chars);

                Assert.DoesNotThrow(() => ReplayCodec.Decode(text), text);
            }
        }

        #endregion

        #region Registry

        [Test]
        public void Registry_DuplicateTag_Throws()
        {
            CommandCodecRegistry registry = CommandCodecRegistry.CreateDefault();

            Assert.Throws<System.ArgumentException>(() => registry.Register(new FakeCodec(1)));
        }

        [Test]
        public void Encode_UnregisteredCommandType_Throws()
        {
            var replay = new GameReplay(1, 2, new ISimCommand[] { new UnknownCommand() }, 0, false);

            Assert.Throws<System.ArgumentException>(() => ReplayCodec.Encode(replay));
        }

        [Test]
        public void Registry_NewCommandType_RoundTrips()
        {
            CommandCodecRegistry registry = CommandCodecRegistry.CreateDefault();
            registry.Register(new FakeCodec(9));
            var replay = new GameReplay(1, 2, new ISimCommand[] { new UnknownCommand() }, 0, false);

            ReplayDecodeResult result = ReplayCodec.Decode(ReplayCodec.Encode(replay, registry), registry);

            Assert.That(result.Success, Is.True);
            Assert.That(result.Replay.Commands[0], Is.TypeOf<UnknownCommand>());
        }

        private sealed class UnknownCommand : ISimCommand
        {
            public CommandResult Execute(Sim sim)
            {
                return CommandResult.Failed("test");
            }
        }

        private sealed class FakeCodec : ICommandCodec
        {
            public int Tag { get; }

            public System.Type CommandType => typeof(UnknownCommand);

            public FakeCodec(int tag)
            {
                Tag = tag;
            }

            public void Write(ReplayWriter writer, ISimCommand command)
            {
            }

            public ISimCommand Read(ReplayReader reader)
            {
                return new UnknownCommand();
            }
        }

        #endregion

        #region Helpers

        private static string CodeFromBytes(byte[] bytes)
        {
            // Reuses the codec's text form through a replay-shaped encode: build the code by hand with the same alphabet.
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";
            var text = new System.Text.StringBuilder("SQ1.");
            int bits = 0;
            int bitCount = 0;

            foreach (byte b in bytes)
            {
                bits = (bits << 8) | b;
                bitCount += 8;

                while (bitCount >= 6)
                {
                    bitCount -= 6;
                    text.Append(alphabet[(bits >> bitCount) & 63]);
                }

                bits &= (1 << bitCount) - 1;
            }

            if (bitCount > 0)
                text.Append(alphabet[(bits << (6 - bitCount)) & 63]);

            return text.ToString();
        }

        private static GameReplay DecodeOk(string code)
        {
            ReplayDecodeResult result = ReplayCodec.Decode(code);

            Assert.That(result.Success, Is.True, result.Message);
            return result.Replay;
        }

        private static void AssertFailure(string code, ReplayDecodeFailure expected)
        {
            ReplayDecodeResult result = ReplayCodec.Decode(code);

            Assert.That(result.Success, Is.False, code);
            Assert.That(result.Failure, Is.EqualTo(expected), $"{code}: {result.Message}");
            Assert.That(result.Replay, Is.Null);
            Assert.That(result.Message, Is.Not.Empty);
        }

        private static void AssertSame(GameReplay expected, GameReplay actual)
        {
            Assert.That(actual.FormatVersion, Is.EqualTo(expected.FormatVersion));
            Assert.That(actual.Seed, Is.EqualTo(expected.Seed));
            Assert.That(actual.ConfigFingerprint, Is.EqualTo(expected.ConfigFingerprint));
            Assert.That(actual.EndScore, Is.EqualTo(expected.EndScore));
            Assert.That(actual.EndIsResolved, Is.EqualTo(expected.EndIsResolved));
            Assert.That(Describe(actual.Commands), Is.EqualTo(Describe(expected.Commands)));
        }

        private static List<string> Describe(IReadOnlyList<ISimCommand> commands)
        {
            var lines = new List<string>();

            foreach (ISimCommand command in commands)
                lines.Add(command.GetType().Name + ": " + command);

            return lines;
        }

        #endregion
    }
}
