using System;
using System.Collections.Generic;
using System.Threading;
using Squarely.Simulation;
using UnityEngine;

namespace Squarely.Presentation
{
    /// <summary>
    /// Plays a command's events on the views, in order, through one handler per event type.
    /// Playing a new command first snaps whatever is still pending, so the views are correct right away.
    /// </summary>
    public sealed class EventPlayer
    {
        private readonly Dictionary<Type, ISimEventHandler> _handlers = new Dictionary<Type, ISimEventHandler>();
        private readonly PlaybackViews _views;
        private readonly CancellationToken _stopToken;

        private Playback _current;

        /// <summary>Playback speed multiplier, 1 = as authored. Applies to running playback too.</summary>
        public float Speed { get; set; } = 1f;

        /// <summary>While true, playback time stands still: running steps hold and nothing new advances. Unpausing resumes where it left off.</summary>
        public bool IsPaused { get; set; }

        public bool IsPlaying => _current != null && !_current.IsEnded;

        /// <summary>A command's playback ended, played out or snapped. Not raised when it was stopped.</summary>
        public event Action PlaybackEndEvent;

        /// <param name="views">The views handlers play on.</param>
        /// <param name="stopToken">Cancelled when the views are destroyed: playback then stops without touching them.</param>
        public EventPlayer(PlaybackViews views, CancellationToken stopToken)
        {
            _views = views;
            _stopToken = stopToken;
        }

        /// <summary>Adds the handler for its event type. Events without a handler are skipped.</summary>
        public void AddHandler(ISimEventHandler handler)
        {
            _handlers.Add(handler.EventType, handler);
        }

        /// <summary>Snaps any pending playback, then starts playing the events.</summary>
        /// <param name="dropPoint">Where the pointer dropped the command's piece, in board cell coordinates, or null. See <see cref="PlaybackContext.DropPoint"/>.</param>
        public void Play(IReadOnlyList<ISimEvent> events, Vector2? dropPoint = null)
        {
            Snap();

            var context = new PlaybackContext(_views, events, dropPoint, () => IsPaused ? 0f : Speed, _stopToken);
            _current = new Playback(this, events, context);
            _current.Start();
        }

        /// <summary>
        /// Brings the views to the end of the pending playback at once: running and unstarted steps apply
        /// their end state, and events not handled yet are handled snapped. Does nothing if nothing is playing.
        /// </summary>
        public void Snap()
        {
            if (_current != null)
                _current.Snap();
        }

        private void Handle(ISimEvent simEvent, PlaybackContext context)
        {
            if (_handlers.TryGetValue(simEvent.GetType(), out ISimEventHandler handler))
                handler.Handle(simEvent, context);
        }

        private void OnPlaybackEnded()
        {
            PlaybackEndEvent?.Invoke();
        }

        /// <summary>One command's playback: its events, how far it got, and its context.</summary>
        private sealed class Playback
        {
            private readonly EventPlayer _player;
            private readonly IReadOnlyList<ISimEvent> _events;
            private readonly PlaybackContext _context;

            private int _next;

            public bool IsEnded { get; private set; }

            public Playback(EventPlayer player, IReadOnlyList<ISimEvent> events, PlaybackContext context)
            {
                _player = player;
                _events = events;
                _context = context;
            }

            public void Start()
            {
                _ = RunAsync();
            }

            public void Snap()
            {
                if (IsEnded)
                    return;

                _context.Snap();

                while (_next < _events.Count)
                    _player.Handle(_events[_next++], _context);

                End();
            }

            private async Awaitable RunAsync()
            {
                try
                {
                    while (_next < _events.Count)
                    {
                        if (_context.IsStopped)
                            return;

                        _player.Handle(_events[_next++], _context);
                        await _context.WaitForBlockingRunsAsync();
                    }

                    if (_context.IsStopped)
                        return;

                    await _context.WaitForAllRunsAsync();

                    if (_context.IsStopped)
                        return;

                    End();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);

                    // Keep the views consistent with the sim even when a cue fails.
                    if (!_context.IsStopped)
                        Snap();
                }
            }

            private void End()
            {
                if (IsEnded)
                    return;

                IsEnded = true;
                _player.OnPlaybackEnded();
            }
        }
    }
}
