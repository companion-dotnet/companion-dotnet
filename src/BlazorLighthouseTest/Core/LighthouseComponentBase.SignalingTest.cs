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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await otherComponent.CallBaseInvokeAsync(
            otherComponent.CallBaseStateHasChanged);

        otherBuildRenderTreeAction.Invocations.Clear();

        // act
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await otherComponent.CallBaseInvokeAsync(
            otherComponent.CallBaseStateHasChanged);

        otherBuildRenderTreeAction.Invocations.Clear();

        // act
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Invocations.Clear();

        // act & assert
        component.Dispose();
        signal.Set(2);

        Assert.Equal(1, siganlValue);
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Never);

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);
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

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

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

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                signal1.Get();
                signal2.Get();

                signal1.Set(3);
                signal2.Set(4);

                recalculationCount++;
                value = signal2.Get();
            });

        // act
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        // assert
        Assert.Equal(4, value);
        buildRenderTreeAction.Verify(
            obj => obj(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TestAleadyRenderedWhileWaitingForInvokeAsync()
    {
        // arrange
        var recalculationCount = 0;
        var value = 0;

        var signal1 = new Signal<int>(1);
        var signal2 = new Signal<int>(2);

        var taskCompletionSource1 = new TaskCompletionSource();
        var taskCompletionSource2 = new TaskCompletionSource();

        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                signal1.Get();
                signal2.Get();

                taskCompletionSource1.SetResult();
                taskCompletionSource2.Task.Wait();

                signal1.Set(4);

                recalculationCount++;
                value = signal2.Get();
            });

        // act
        var task = Task.Run(
            () => component.CallBaseInvokeAsync(
                component.CallBaseStateHasChanged));

        await taskCompletionSource1.Task;

        taskCompletionSource1 = new();
        signal2.Set(6);

        taskCompletionSource2.SetResult();
        await task;

        // assert
        Assert.Equal(6, value);
        buildRenderTreeAction.Verify(
            obj => obj(),
            Times.Exactly(2));
    }
}
