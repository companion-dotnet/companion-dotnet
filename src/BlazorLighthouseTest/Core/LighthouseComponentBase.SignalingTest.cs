using BlazorLighthouse.Core;
using Moq;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
{
    [Fact]
    public async Task TestValueChanged()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => siganlValue = signal.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal.Set(2);

        // assert
        Assert.Equal(2, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);

        Assert.Null(renderer.HandledException);
    }

    [Fact]
    public async Task TestValueChanged_MultipleComponentRedraws()
    {
        // arrange
        var otherBuildRenderTreeAction = new Mock<Action>();
        var otherComponent = new TestComponent()
        {
            BuildRenderTreeAction = otherBuildRenderTreeAction.Object,
            OnInitializedAction = onInitializedAction.Object,
            OnInitializedAyncAction = onInitializedAyncAction.Object,
            OnParametersSetAction = onParametersSetAction.Object,
            OnParametersSetAsyncAction = onParametersSetAsyncAction.Object,
            OnAfterRenderAction = onAfterRenderAction.Object,
            OnAfterRenderAsyncAction = onAfterRenderAsyncAction.Object,
            ShouldRenderAction = shouldRenderAction.Object,
            DisableStateHasChangedAction = disableStateHasChangedAction.Object
        };

        renderer.Attach(otherComponent);

        var signal = new Signal<int>(1);
        var signalValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                signal.Set(2);
                signal.Set(3);
            });
        otherBuildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => signalValue = signal.Get());

        await otherComponent.ExecuteInvokeAsync(
            otherComponent.ExecuteStateHasChanged);

        otherBuildRenderTreeAction.Invocations.Clear();

        // act
        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        // assert
        Assert.Equal(3, signalValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
        otherBuildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestValueNotChanged()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => siganlValue = signal.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal.Set(1);

        // assert
        Assert.Equal(1, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Never);
    }

    [Fact]
    public async Task TestValueChangedMultipleTimes()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => siganlValue = signal.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal.Set(2);
        signal.Set(3);

        // assert
        Assert.Equal(3, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TestUnreferencedValueChanged()
    {
        // arrange
        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var siganlValue = 0;
        
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => siganlValue = signal1.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(1, siganlValue);
        buildRenderTreeAction.Verify(obj => obj.Invoke(), Times.Never);
    }

    [Fact]
    public async Task TestUnreferencedValueChanged_WasReferenced()
    {
        // arrange
        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var siganlValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                if (signal1.Get() == 3)
                {
                    siganlValue = signal1.Get();
                    return;
                }

                siganlValue = signal2.Get();
            });

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal1.Set(3);
        signal2.Set(4);

        // assert
        Assert.Equal(3, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValue()
    {
        // arrange
        var computedRecalculationCount = 0;
        var computedValue = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var computed = new Computed<int>(() =>
        {
            computedRecalculationCount++;
            return signal1.Get();
        });

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => computedValue = computed.Get() + signal2.Get());

        // act
        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        // assert
        Assert.Equal(3, computedValue);
        Assert.Equal(1, computedRecalculationCount);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValue_ValueChanged()
    {
        // arrange
        var computedRecalculationCount = 0;
        var computedValue = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var computed = new Computed<int>(() =>
        {
            computedRecalculationCount++;
            return signal1.Get();
        });

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => computedValue = computed.Get() + signal2.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal2.Set(3);

        // assert
        Assert.Equal(4, computedValue);
        Assert.Equal(1, computedRecalculationCount);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValue_NestedValueChanged()
    {
        // arrange
        var computedRecalculationCount = 0;
        var computedValue = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var computed = new Computed<int>(() =>
        {
            computedRecalculationCount++;
            return signal1.Get();
        });

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => computedValue = computed.Get() + signal2.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        signal1.Set(3);

        // assert
        Assert.Equal(5, computedValue);
        Assert.Equal(2, computedRecalculationCount);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestNestedComputedValueChanged_MultipleComponentRedraws()
    {

        var otherBuildRenderTreeAction = new Mock<Action>();
        var otherComponent = new TestComponent()
        {
            BuildRenderTreeAction = otherBuildRenderTreeAction.Object,
            OnInitializedAction = onInitializedAction.Object,
            OnInitializedAyncAction = onInitializedAyncAction.Object,
            OnParametersSetAction = onParametersSetAction.Object,
            OnParametersSetAsyncAction = onParametersSetAsyncAction.Object,
            OnAfterRenderAction = onAfterRenderAction.Object,
            OnAfterRenderAsyncAction = onAfterRenderAsyncAction.Object,
            ShouldRenderAction = shouldRenderAction.Object,
            DisableStateHasChangedAction = disableStateHasChangedAction.Object
        };

        renderer.Attach(otherComponent);

        var signal = new Signal<int>(1);
        var computed = new Computed<int>(signal.Get);
        var computedValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                signal.Set(2);
                signal.Set(3);
            });
        otherBuildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => computedValue = computed.Get());

        await otherComponent.ExecuteInvokeAsync(
            otherComponent.ExecuteStateHasChanged);

        otherBuildRenderTreeAction.Invocations.Clear();

        // act
        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        // assert
        Assert.Equal(3, computedValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
        otherBuildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestComponentDisposed()
    {
        // arrange
        var signal = new Signal<int>(1);
        var siganlValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => siganlValue = signal.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act & assert
        component.Dispose();
        signal.Set(2);

        Assert.Equal(1, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Never);

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);
        Assert.IsType<InvalidOperationException>(
            renderer.HandledException);
    }

    [Fact]
    public async Task TestReferencedSignalDisposed()
    {
        // arrange
        var context = new SignalingContext();

        var signal = new Signal<int>(context, 1);
        var siganlValue = 0;

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() => siganlValue = signal.Get());

        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act
        context.Dispose();

        // assert
        Assert.Throws<InvalidOperationException>(
            () => signal.Set(2));

        Assert.Equal(1, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj(),
            Times.Never);
    }

    [Fact]
    public async Task TestMultipleSignalChangesAtOnce()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);
        var signal3 = new Signal<int>(3);

        var taskCompletionSource1 = new TaskCompletionSource();
        var taskCompletionSource2 = new TaskCompletionSource();

        taskCompletionSource1.SetResult();

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                signal1.Get();
                signal2.Get();
                signal3.Get();

                signal1.Set(4);
                signal2.Set(5);

                recalculationCount++;
                value = signal3.Get();
            });

        // act
        await component.ExecuteInvokeAsync(
            component.ExecuteStateHasChanged);

        signal3.Set(6);

        while (recalculationCount < 3)
            ;

        // assert
        Assert.Equal(6, value);
        buildRenderTreeAction.Verify(
            obj => obj(),
            Times.Exactly(3));
    }
}
