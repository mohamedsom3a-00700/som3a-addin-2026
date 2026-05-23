# IEventBus Contract

## Purpose

Decoupled publish/subscribe communication between components. Publishers and subscribers have no direct knowledge of each other.

## Contract

```csharp
public interface IEventBus
{
    void Publish<TEvent>(TEvent @event) where TEvent : class;
    SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler) where TEvent : class;
    SubscriptionToken Subscribe<TEvent>(Action<TEvent> handler, Func<TEvent, bool> filter) where TEvent : class;
    void Unsubscribe(SubscriptionToken token);

    // Diagnostics
    event EventHandler<EventPublishedEventArgs> EventPublished;
    event EventHandler<EventSubscribedEventArgs> EventSubscribed;
    event EventHandler<SubscriberErrorEventArgs> SubscriberError;
}

public class SubscriptionToken : IDisposable
{
    public void Dispose(); // Unsubscribes automatically
}
```

## Behaviors

- **Typed events**: Subscribers receive only events matching their subscribed type. No string keys or type IDs.
- **Weak references**: Dead subscribers (garbage-collected) are automatically pruned on next publish — no manual cleanup needed.
- **Multiple subscribers**: Any number of subscribers can register for the same event type. All receive the event.
- **Subscriber isolation**: If one subscriber throws an exception, other subscribers still receive the event. Exceptions are reported via `SubscriberError` event.
- **Filtering**: Optional predicate per subscriber to receive only matching events (e.g., only events with a specific property value).
- **Unsubscription**: via `SubscriptionToken.Dispose()` or explicit `Unsubscribe()`. Idempotent.
- **No subscribers**: Publishing with zero subscribers is a silent no-op.
- **Publisher pacing**: Publishers are responsible for pacing their own events. The bus does not throttle or coalesce.
- **Observability**: `EventPublished`, `EventSubscribed`, and `SubscriberError` events fire for diagnostics.
