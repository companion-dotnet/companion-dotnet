# Basic Concepts of Companion .NET
**Signals** and **SignalingEffects** are the main building blocks of *Companion .NET*. **Signals** provide the value stores which can later be used in **DerivedSignal Values** and **SignalingEffects**.

## Signals
**Signals** are simple value stores. They require an initial value, which can be changed later. Any access inside of an **SignalingEffect, DerivedSignal Value or Component** leads to an subscription (this behavior can not be nested).

```
 // Create signal
Signal<int> signal = new Signal<int>(0);

// Set signal value
signal.Set(1);

// Access signal value
int value = signal.Get();
```

## SignalingEffects
**SignalingEffects** are simple subscribers. They accept a callback performing an arbitrary action. Initially, the action is run when the the effect is initialized. Afterwards whenever an accessed **Signal** value changes the **SignalingEffect** is rerun.

```
// Create effect and run callback
_ = new SignalingEffect(() => {
    Console.WriteLine($"The value is: {signal.Get()}");
});

// Runs callback again
signal.Set(1);
```

## DerivedSignal Values
**DerivedSignal Values** combine the fuctionality of **Signals** with the functionality of an **SignalingEffect**. It allows the calculation of a value as it would be done with an **SignalingEffect** and provides this value as a readonly **Signal**.

```
// Create derivedSignal and calculate value
DerivedSignal<int> derivedSignal = new DerivedSignal<int>(() => {
    return signal.Get() * signal.Get();
});

// Calculates value agian
signal.Set(1);

// Access derivedSignal value
int value = derivedSignal.Get();
```
