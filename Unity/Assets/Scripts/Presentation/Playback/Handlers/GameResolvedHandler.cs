using System;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Reports that playback reached the game's resolution, after the events before it have played.</summary>
    public sealed class GameResolvedHandler : SimEventHandler<GameResolvedEvent>
    {
        private readonly Action<GameResolvedEvent> _onResolved;

        public GameResolvedHandler(Action<GameResolvedEvent> onResolved)
        {
            _onResolved = onResolved ?? throw new ArgumentNullException(nameof(onResolved));
        }

        protected override void Handle(GameResolvedEvent simEvent, PlaybackContext context)
        {
            _onResolved(simEvent);
        }
    }
}
