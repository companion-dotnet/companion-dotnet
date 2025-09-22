# Companion .NET Signaling
*Companion .NET Signaling* provides an API for change detection, therefore adds the possability to re-render components only when it is actually necessary. The concept is pretty much the same as the signals that are available in [Angular](https://angular.dev/guide/signals).

## How does this work?
All values that should be tracked as signals need to be warpped by a *Companion.Signaling.Core.Signal&lt;T&gt;*. Whenever this value is accessed in a pre-defined context (as *Companion.Signaling.Core.DerivedSignal&lt;T&gt;* and *Companion.Signaling.Blazor.SignalingComponentBase* are providing it), a reference is stored internally. Later, when the value is changed (and only when *object.Equals* actually says that the value changed), the object that accessed the signal will be notified.

## How does this work internally?
As non-async code is always run on a specific thread, and it actually is the only thing running on that specific thread at the specific time, a internal *ThreadStatic* ([docs](https://learn.microsoft.com/en-us/dotnet/api/system.threadstaticattribute?view=net-9.0)) variable is used to create a context as mentioned above.

## What features are available?
Currently, there are the following core concepts implemented:

- **Companion.Signaling.Core.Signal&lt;T&gt;**: A simple value store with signaling capability
- **Companion.Signaling.Core.DerivedSignal&lt;T&gt;**: Allow the calculation of a signal based on other signals (it gets recalculated whenever a signal used is changed)
- **Companion.Signaling.Core.SignalingEffect**: Runs arbitrary code that is dependent on some signals (it gets re-run whenever a signal used is changed)
- **Companion.Signaling.Core.SignalingContext**: Exposes control to the lifetime of any of the types mentioned above
- **Companion.Signaling.Blazor.SignalingComponentBase**: A *Blazor* component base which subscribes to signal value changes as *DerivedSignals* and *SignalingEffects* do

## When to use Companion .NET Signaling?
- Your application has complex state relationships
- You need fine-grained reactivity for performance
- You want to minimize unnecessary re-renders

## Where can i find documentation?
All public methods have comments added that are roughly documenting their use. Further there are the following documents explaining the core concepts in detail:

 - [BasiConcepts.md](../../../docs/Signaling/BasicConcepts.md): Everything interesting to know about the signaling types themself
 - [SignalingContext.md](../../../docs/Signaling/SignalingContext.md): Everything interesting to know about the *SignalingContext* mentioned above
 - [UsageWithBlazor.md](../../../docs/Signaling/UsageWithBlazor.md): Everything interesting to know about the integration into *Blazor*

Also a simple example can be found [here](../../Playground).
