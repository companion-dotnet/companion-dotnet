# Control the lifetime
There are some situations where a **SignalingEffect, DerivedSignal or SignalingComponentBase** subscribes to a value longer than it actually should. This cannot be controlled by default, as the references are tracked internally. To dispose unnecessary subscriptions, a **SignalingContext** can be used. All constructable signaling types (except **SignalingComponentBase**) accept a **SignalingContext**  that defines the lifetime of the specific object as a constructor argument (**SignalingComponentBase** inherits from **SignalingContext** itself). 

## How SignalingContexts solve this
The **SignalingContext** acts as a scope for signals, ensuring that when a component or a set of signals is disposed of, all associated references are also released. As soon as the context gets disposed all values that are referencing that context get disposed instantly and can no longer be used in any way (performing an invalid action would lead to an exception).

## Managing Lifespan with SignalingContext
When defining **Signals, DerivedSignals or SignalingEffects**, attaching them to a **SignalingContext** can help manage their lifespan efficiently. This approach ensures that these reactive primitives are properly disposed of when the context (e.g. a *Blazor* component) is destroyed.

```
// Create signaling context
SignalingContext context = new SignalingContext();

// Create signal within context
Signal<int> signal = new Signal<int>(context, 0);

// Create effect within context
_ = new SignalingEffect(context, () => {
    Console.WriteLine($"The value is: {signal.Get()}");
});

// Create derived signal value within context
DerivedSignal<int> derivedSignal = new DerivedSignal<int>(context, () => {
    return signal.Get() * signal.Get();
}); 

// Dispose all elements created within this context
context.Dispose();
```

Once context.Dispose() is called, all elements referencing this context are automatically cleaned up.
