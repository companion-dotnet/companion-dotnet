# Basic Concepts of Companion .NET Signaling
*Companion .NET Signaling* provides an API for reactive change detection, therefore adds the possability to re-render components only when it is actually necessary. The concept is pretty much the same as the signals that are available in [Angular](https://angular.dev/guide/signals).

For more information on reactive programming concepts in general, pleaser refer to:
- [Reactive Programming Overview](https://en.wikipedia.org/wiki/Reactive_programming)
- [Signals in Angular](https://angular.dev/guide/signals)
- [Signals in SolidJS](https://www.solidjs.com/docs/latest#signals)

**Signals, DerivedSignals and SignalingEffects** are the main building blocks of *Companion .NET Signaling*. **Signals** provide the value stores which can later be used in **DerivedSignals** and **SignalingEffects**.

## Signals
**Signals** are simple value stores. They require an initial value, which can be changed later. Any access inside of an **DerivedSignal, SignalingEffect or SignalingComponentBase** leads to an subscription (this behavior cannot be nested).

Signals are the foundation of this reactivity system. They:
- Store mutable state
- Automatically notify their dependents (e.g., **SignalingEffect** or **DerivedSignal**) when their value changes

### What are "dependents"?
In this context, dependents are reactive components or computations that rely on the value of a signal. When a signal's value changes, all its dependents are automatically updated to reflect the new state. This ensures that the UI stays in sync with the underlying data without requiring manual updates.

### Key features:
- Type-safe value storage
- Change detection (won't notify if value hasn't changed)

```
 // Create signal
Signal<int> signal = new Signal<int>(0);

// Set signal value
signal.Set(1);

// Access signal value
int value = signal.Get();
```

## SignalingEffects
**SignalingEffects** are simple subscribers. They accept a callback performing an arbitrary action. Initially, the action is run when the the effect is initialized. Afterwards whenever an accessed **Signal** value changes the **Effect** is re-run.

### Common use cases:
- Console logging
- External API calls

```
// Create effect and run callback
_ = new SignalingEffect(() => {
    Console.WriteLine($"The value is: {signal.Get()}");
});

// Runs callback again
signal.Set(1);
```

## DerivedSignals
**DerivedSignals** combine the fuctionality of **Signals** with the functionality of an **SignalingEffects**. It allows the calculation of a value as it would be done with an **SignalingEffects** and provides this value as a readonly **Signal**.

### Characteristics:
- Memoization (cache results until dependencies change)
- Automatic dependency tracking

```
// Create derived signal and calculate value
DerivedSignal<int> derivedSignal = new DerivedSignal<int>(() => {
    return signal.Get() * signal.Get();
});

// Calculates value again
signal.Set(1);

// Access derived signal value
int value = derivedSignal.Get();
```

## How to use Companion .NET Signaling in Blazor Components?
For mor information about the integration within *Blazor* see [UsageWithBlazor.md](./UsageWithBlazor.md).
