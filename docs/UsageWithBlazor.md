# Usage with Blazor Components
The **SignalingComponentBase** provides a Blazor component base for the signaling technology. All signal values that are accessed during rendering (specified inside the razor template or any code that is synchronously run while rendering) leads to a subscription and triggers a re-rendering if changed. Further the component base inherits from the **SignalingContext** allowing values to diretly match the components lifetime.

```
// Inherit from the SignalingComponentBase
@inherits SignalingComponentBase
 
// Access value while rendering
Value: @Value.Get()<br/>

@code{
    // Instantiate signal within context
    public Signal<int> Value { get; }
    
    public Page()
    {
        Value = new(this, 0);
    }
}
```
