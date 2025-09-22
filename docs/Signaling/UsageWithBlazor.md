# Usage with Blazor Components
The **SignalingComponentBase** provides a *Blazor* component base for the signaling technology. All signal values that are accessed during rendering (specified inside the *Razor* template or any code that is synchronously run while rendering) leads to a subscription and triggers a re-rendering if changed. Further the component base inherits from the **SignalingContext** allowing values to diretly match the components lifetime.

## Using SignalingComponentBase in a Blazor Component
To leverage automatic updates in a Blazor component, inherit from SignalingComponentBase:

```
// Inherit from the SignalingComponentBase
@inherits SignalingComponentBase
 
// Access value while rendering
Value: @Value.Get()<br/>

<h1>Current Value: @Value.Get()</h1>
<button @onclick="Increment">Increment</button>

@code {
    public Signal<int> Value { get; }

    public MyComponent()
    {
        Value = new(0);
    }

    private void Increment()
    {
        Value.Set(Value.Get() + 1);
    }
}
```

## How it works
- When *Value.Set()* is called, the component automatically re-renders if the signal's value is used during rendering. This ensures that only components dependent on the signal are updated
- No need for *StateHasChanged()* - Signaling detects the update

## Key benefits of using SignalingComponentBase
- Automatic subscription tracking – Any signal accessed inside the *Razor* template automatically triggers reactivity and re-rendering. Signals accessed in the *@code* block will not trigger reactivity
- Efficient updates – Only re-renders when needed, reducing unnecessary UI updates

## Bind the lifetime of a signal to the lifetime of a SignalingComponentBase
The **SignalingComponentBase** is inheriting from *SignalingContext*, allowing to bind the lifetime of a signal to the lifetime of the component itself. Detailed documentation can be found within [SignalingContext.md](./SignalingContext.md).
