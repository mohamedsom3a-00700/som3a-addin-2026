using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Som3a_WPF_UI.Services
{
    public class EventPublishedEventArgs : EventArgs
    {
        public Type EventType { get; }
        public object Event { get; }

        public EventPublishedEventArgs(Type eventType, object @event)
        {
            EventType = eventType;
            Event = @event;
        }
    }

    public class EventSubscribedEventArgs : EventArgs
    {
        public Type EventType { get; }
        public object Subscriber { get; }

        public EventSubscribedEventArgs(Type eventType, object subscriber)
        {
            EventType = eventType;
            Subscriber = subscriber;
        }
    }

    public class SubscriberErrorEventArgs : EventArgs
    {
        public Type EventType { get; }
        public object Event { get; }
        public Exception Exception { get; }

        public SubscriberErrorEventArgs(Type eventType, object @event, Exception exception)
        {
            EventType = eventType;
            Event = @event;
            Exception = exception;
        }
    }

    public interface IEventBus
    {
        void Publish<TEvent>(TEvent @event) where TEvent : class;
        SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
        SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter) where TEvent : class;
        void Unsubscribe(SubscriptionToken token);

        event EventHandler<EventPublishedEventArgs> EventPublished;
        event EventHandler<EventSubscribedEventArgs> EventSubscribed;
        event EventHandler<SubscriberErrorEventArgs> SubscriberError;
    }

    public class SubscriptionToken : IDisposable
    {
        private readonly Action _unsubscribe;
        private bool _disposed;

        internal SubscriptionToken(Guid id, Action unsubscribe)
        {
            Id = id;
            _unsubscribe = unsubscribe;
        }

        internal Guid Id { get; }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _unsubscribe();
            }
        }
    }

    internal sealed class SubscriberEntry
    {
        public Guid TokenId { get; }
        public Type EventType { get; }
        public WeakReference SubscriberRef { get; }
        public Delegate Handler { get; }
        public object? Filter { get; }

        public SubscriberEntry(Guid tokenId, Type eventType, WeakReference subscriberRef, Delegate handler, object? filter)
        {
            TokenId = tokenId;
            EventType = eventType;
            SubscriberRef = subscriberRef;
            Handler = handler;
            Filter = filter;
        }
    }

    public sealed class EventBus : IEventBus
    {
        private readonly ConcurrentDictionary<Type, List<SubscriberEntry>> _subscribers = new();
        private readonly object _lock = new();

        public event EventHandler<EventPublishedEventArgs>? EventPublished;
        public event EventHandler<EventSubscribedEventArgs>? EventSubscribed;
        public event EventHandler<SubscriberErrorEventArgs>? SubscriberError;

        public void Publish<TEvent>(TEvent @event) where TEvent : class
        {
            var eventType = typeof(TEvent);

            if (!_subscribers.TryGetValue(eventType, out var entries))
            {
                return;
            }

            List<SubscriberEntry> snapshot;
            lock (_lock)
            {
                snapshot = entries.ToList();
            }

            foreach (var entry in snapshot)
            {
                var target = entry.SubscriberRef.Target;
                if (target == null)
                {
                    lock (_lock)
                    {
                        entries.RemoveAll(e => e.TokenId == entry.TokenId);
                    }
                    continue;
                }

                if (entry.Filter is Func<TEvent, bool> predicate && !predicate(@event))
                {
                    continue;
                }

                try
                {
                    ((Action<TEvent>)entry.Handler)(@event);
                }
                catch (Exception ex)
                {
                    SubscriberError?.Invoke(this, new SubscriberErrorEventArgs(eventType, @event!, ex));
                }
            }

            EventPublished?.Invoke(this, new EventPublishedEventArgs(eventType, @event!));
        }

        public SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class
        {
            return Subscribe(handler, null);
        }

        public SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool>? filter) where TEvent : class
        {
            var eventType = typeof(TEvent);
            var tokenId = Guid.NewGuid();
            var subscriberRef = new WeakReference(handler.Target);
            var entry = new SubscriberEntry(tokenId, eventType, subscriberRef, handler, filter);

            var entries = _subscribers.GetOrAdd(eventType, _ => new List<SubscriberEntry>());

            lock (_lock)
            {
                if (!entries.Any(e =>
                        e.Handler == (Delegate)handler &&
                        e.SubscriberRef.Target == handler.Target))
                {
                    entries.Add(entry);
                }
            }

            if (handler.Target != null)
            {
                EventSubscribed?.Invoke(this, new EventSubscribedEventArgs(eventType, handler.Target));
            }

            return new SubscriptionToken(tokenId, () => Unsubscribe(tokenId));
        }

        public void Unsubscribe(SubscriptionToken token)
        {
            if (token == null) return;
            Unsubscribe(token.Id);
        }

        private void Unsubscribe(Guid tokenId)
        {
            foreach (var kvp in _subscribers)
            {
                lock (_lock)
                {
                    kvp.Value.RemoveAll(e => e.TokenId == tokenId);
                }
            }
        }
    }
}
