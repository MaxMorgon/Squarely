using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// The command types a replay code can hold, by tag. <see cref="CreateDefault"/> has the built-in commands.
    /// To add a command type, write an <see cref="ICommandCodec"/> with a new tag and register it.
    /// </summary>
    public sealed class CommandCodecRegistry
    {
        private readonly Dictionary<int, ICommandCodec> _byTag = new Dictionary<int, ICommandCodec>();
        private readonly Dictionary<Type, ICommandCodec> _byType = new Dictionary<Type, ICommandCodec>();

        public static CommandCodecRegistry CreateDefault()
        {
            var registry = new CommandCodecRegistry();
            registry.Register(new PlacePieceCommandCodec());
            registry.Register(new EndGameCommandCodec());
            return registry;
        }

        public void Register(ICommandCodec codec)
        {
            if (codec == null)
                throw new ArgumentNullException(nameof(codec));

            if (_byTag.ContainsKey(codec.Tag))
                throw new ArgumentException($"Command tag {codec.Tag} is already registered.");

            if (_byType.ContainsKey(codec.CommandType))
                throw new ArgumentException($"Command type {codec.CommandType.Name} is already registered.");

            _byTag.Add(codec.Tag, codec);
            _byType.Add(codec.CommandType, codec);
        }

        public bool TryGetByTag(int tag, out ICommandCodec codec)
        {
            return _byTag.TryGetValue(tag, out codec);
        }

        public bool TryGetByType(Type type, out ICommandCodec codec)
        {
            return _byType.TryGetValue(type, out codec);
        }

        private sealed class PlacePieceCommandCodec : ICommandCodec
        {
            public int Tag => 1;

            public Type CommandType => typeof(PlacePieceCommand);

            public void Write(ReplayWriter writer, ISimCommand command)
            {
                var place = (PlacePieceCommand)command;
                writer.WriteVarUInt((ulong)place.PieceInstanceId);
                writer.WriteVarInt(place.Origin.X);
                writer.WriteVarInt(place.Origin.Y);
            }

            public ISimCommand Read(ReplayReader reader)
            {
                int id = reader.ReadVarCount();
                int x = reader.ReadVarInt();
                int y = reader.ReadVarInt();
                return new PlacePieceCommand(id, new GridPos(x, y));
            }
        }

        private sealed class EndGameCommandCodec : ICommandCodec
        {
            public int Tag => 2;

            public Type CommandType => typeof(EndGameCommand);

            public void Write(ReplayWriter writer, ISimCommand command)
            {
            }

            public ISimCommand Read(ReplayReader reader)
            {
                return new EndGameCommand();
            }
        }
    }
}
