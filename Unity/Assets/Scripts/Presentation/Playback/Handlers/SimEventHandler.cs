using System;
using Squarely.Simulation;

namespace Squarely.Presentation
{
    /// <summary>Base for a handler of one event type.</summary>
    public abstract class SimEventHandler<TEvent> : ISimEventHandler where TEvent : class, ISimEvent
    {
        public Type EventType => typeof(TEvent);

        void ISimEventHandler.Handle(ISimEvent simEvent, PlaybackContext context)
        {
            Handle((TEvent)simEvent, context);
        }

        /// <inheritdoc cref="ISimEventHandler.Handle"/>
        protected abstract void Handle(TEvent simEvent, PlaybackContext context);
    }
}
