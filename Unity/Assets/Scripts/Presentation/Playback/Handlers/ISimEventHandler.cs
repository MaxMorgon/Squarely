using System;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Plays one type of sim event on the views. Registered with an <see cref="EventPlayer"/>.</summary>
    public interface ISimEventHandler
    {
        Type EventType { get; }

        /// <summary>
        /// Updates the views for the event and starts its cues in the context. Everything the views must end up showing
        /// is either done here, before returning, or by a cue run (its steps or its end action), so snapping reaches it.
        /// </summary>
        void Handle(ISimEvent simEvent, PlaybackContext context);
    }
}
