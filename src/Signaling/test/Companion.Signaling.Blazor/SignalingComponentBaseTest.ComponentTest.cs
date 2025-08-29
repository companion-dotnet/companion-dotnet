using Companion.Signaling.Blazor.Test.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Moq;

namespace Companion.Signaling.Blazor.Test;

public partial class SignalingComponentBaseTest
{
    [Fact]
    public void TestRendererInfo()
    {
        // act
        var rendererInfo = component.GetRendererInfo();

        // assert
        Assert.Equal(renderer.GetRendererInfo(), rendererInfo);
    }

    [Fact]
    public void TestAssets()
    {
        // act
        var assets = component.GetAssets();

        // assert
        Assert.Equal(renderer.GetAssets(), assets);
    }

    [Fact]
    public void TestAssignedRenderMode()
    {
        // act
        var assignedRenderMode = component.GetAssignedRenderMode();

        // assert
        Assert.IsType<RendererFake.ComponentRenderMode>(assignedRenderMode);
        Assert.Equal(1, renderer.GetComponentRenderModeCallCount);
    }

    [Fact]
    public void TestAttach()
    {
        // arrange
        var unattachedRenderer = RendererFake.Create();

        // act & assert
        Assert.Throws<InvalidOperationException>(
            () => unattachedRenderer.Attach(component));
    }

    [Theory]
    [InlineData(1, 1, false, false, 1, 1, 1, 1, false, false, true, true, false, false, false, false, false)]
    [InlineData(1, 1, false, false, 1, 1, 1, 1, false, true, true, true, false, false, false, false, false)]
    [InlineData(2, 2, false, false, 1, 1, 1, 1, false, false, true, false, false, false, false, false, false)]
    [InlineData(2, 1, false, false, 1, 1, 1, 1, false, true, true, false, false, false, false, false, false)]
    [InlineData(1, 1, true, false, 1, 1, 1, 1, false, false, true, true, false, false, false, true, false)]
    [InlineData(1, 1, true, false, 1, 1, 1, 1, false, true, true, true, false, false, false, true, false)]
    [InlineData(1, 1, true, false, 1, 1, 1, 1, false, false, true, false, false, false, false, true, false)]
    [InlineData(1, 1, true, false, 1, 1, 1, 1, false, true, true, false, false, false, false, true, false)]
    [InlineData(0, 0, true, false, 1, 1, 1, 0, false, false, true, true, false, false, true, false, false)]
    [InlineData(0, 0, true, false, 1, 1, 1, 0, false, true, true, true, false, false, true, false, false)]
    [InlineData(1, 1, false, false, 1, 1, 1, 1, false, false, true, true, false, false, false, false, true)]
    [InlineData(1, 1, false, false, 1, 1, 1, 1, false, true, true, true, false, false, false, false, true)]
    [InlineData(2, 2, false, true, 1, 1, 1, 1, false, false, false, true, false, false, false, false, false)]
    [InlineData(2, 1, false, true, 1, 1, 1, 1, false, true, false, true, false, false, false, false, false)]
    [InlineData(0, 0, true, true, 1, 0, 0, 0, false, false, false, true, true, false, false, false, false)]
    [InlineData(0, 0, true, true, 1, 0, 0, 0, false, true, false, true, true, false, false, false, false)]
    [InlineData(1, 1, true, true, 1, 1, 0, 0, false, false, false, true, false, true, false, false, false)]
    [InlineData(1, 1, true, true, 1, 1, 0, 0, false, true, false, true, false, true, false, false, false)]
    [InlineData(1, 1, false, false, 0, 0, 1, 1, true, false, true, true, false, false, false, false, false)]
    [InlineData(1, 0, false, false, 0, 0, 1, 1, true, true, true, true, false, false, false, false, false)]
    public async Task TestSetParametersAsync(
       int expectedStateHasChangedCallCount,
       int excpectedBuildRenderTreeCallCount,
       bool expectInvalidOperationException,
       bool expectStateHasChangedAfterInit,
       int expectedOnInitializedCallCount,
       int expectedOnInitializedAsyncCallCount,
       int expectedOnParametersSetCallCount,
       int expectedOnParametersSetAsyncCallCount,
       bool alreadyInitialized,
       bool preventDefaultRendering,
       bool returnCompletedTaskOnOnInitializeAsync,
       bool returnCompletedTaskOnOnParametersSetAsync,
       bool throwOnOnInitialize,
       bool throwOnOnInitializeAsync,
       bool throwOnOnParametersSet,
       bool throwOnOnParametersSetAsync,
       bool cancelOnParameterSetAsync)
    {
        // arrange
        if (alreadyInitialized)
        {
            onInitializedAyncAction.Setup(obj => obj.Invoke())
                .Returns(Task.CompletedTask);
            onParametersSetAsyncAction.Setup(obj => obj.Invoke())
                .Returns(Task.CompletedTask);

            await component.CallBaseInvokeAsync(
                async () => await component.SetParametersAsync(
                    ParameterView.FromDictionary(new Dictionary<string, object?>())));

            onInitializedAction.Reset();
            onInitializedAyncAction.Reset();
            onParametersSetAction.Reset();
            onParametersSetAsyncAction.Reset();

            preventDefaultRenderingAction.Reset();
            buildRenderTreeAction.Reset();
        }

        var onParametersSetAsyncTaskCompletionSource = new TaskCompletionSource();
        Action<TaskCompletionSource> onParameterSetAsyncTaskCompleter =
            taskCompletionSource => taskCompletionSource.SetResult();

        if (throwOnOnParametersSetAsync)
        {
            onParameterSetAsyncTaskCompleter =
                taskCompletionSource => taskCompletionSource.SetException(
                    new InvalidOperationException());
        }

        if (cancelOnParameterSetAsync)
        {
            onParameterSetAsyncTaskCompleter =
                taskCompletionSource => taskCompletionSource.SetCanceled();
        }

        var onInitializedAsyncTaskCompletionSource = new TaskCompletionSource();
        Action<TaskCompletionSource> onInitializedTaskCompleter =
            taskCompletionSource => taskCompletionSource.SetResult();

        if (throwOnOnInitializeAsync)
        {
            onInitializedTaskCompleter =
                taskCompletionSource => taskCompletionSource.SetException(
                    new InvalidOperationException());
        }

        var onInitializedActionCallCount = 0;
        var onInitializedAyncActionCallCount = 0;
        var onParametersSetActionCallCount = 0;
        var onParametersSetAsyncActionCallCount = 0;
        var buildRenderTreeActionCallCount = 0;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        preventDefaultRenderingAction.Setup(obj => obj.Invoke())
            .Returns(preventDefaultRendering);
        onInitializedAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                Assert.Equal("Value1", component.Property1);
                Assert.Equal("Value2", component.Property2);

                onInitializedActionCallCount++;
                if (throwOnOnInitialize)
                    throw new InvalidOperationException();
            });
        onInitializedAyncAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                onInitializedAction.Verify(
                    obj => obj.Invoke(),
                    Times.Once);

                onInitializedAyncActionCallCount++;
                onInitializedAsyncTaskCompletionSource = new TaskCompletionSource();
            })
            .Returns(() =>
            {
                if (returnCompletedTaskOnOnInitializeAsync)
                    onInitializedTaskCompleter(onInitializedAsyncTaskCompletionSource);
                return onInitializedAsyncTaskCompletionSource.Task;
            });

        onParametersSetAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                Assert.Equal("Value1", component.Property1);
                Assert.Equal("Value2", component.Property2);

                onInitializedAyncAction.Verify(
                    obj => obj.Invoke(),
                    Times.Exactly(expectedOnInitializedAsyncCallCount));

                onParametersSetActionCallCount++;
                if (throwOnOnParametersSet)
                    throw new InvalidOperationException();
            });
        onParametersSetAsyncAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                onParametersSetAction.Verify(
                    obj => obj.Invoke(),
                    Times.Once);
                onParametersSetAction.Verify(
                    obj => obj.Invoke(),
                    Times.Once);

                onParametersSetAsyncActionCallCount++;
                onParametersSetAsyncTaskCompletionSource = new TaskCompletionSource();
            })
            .Returns(() =>
            {
                if (returnCompletedTaskOnOnParametersSetAsync)
                    onParameterSetAsyncTaskCompleter(onParametersSetAsyncTaskCompletionSource);
                return onParametersSetAsyncTaskCompletionSource.Task;
            });
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                if (expectStateHasChangedAfterInit && buildRenderTreeActionCallCount == 0)
                {
                    onInitializedAyncAction.Verify(
                        obj => obj.Invoke(),
                        Times.Once);
                    onParametersSetAsyncAction.Verify(
                        obj => obj.Invoke(),
                        Times.Never);
                }
                else
                {
                    onParametersSetAsyncAction.Verify(
                        obj => obj.Invoke(),
                        Times.Once);
                }

                if (!onParametersSetAsyncTaskCompletionSource.Task.IsCompleted)
                    onParameterSetAsyncTaskCompleter(onParametersSetAsyncTaskCompletionSource);

                if (!onInitializedAsyncTaskCompletionSource.Task.IsCompleted)
                    onInitializedTaskCompleter(onInitializedAsyncTaskCompletionSource);

                onParametersSetAsyncTaskCompletionSource = new TaskCompletionSource();
                onInitializedAsyncTaskCompletionSource = new TaskCompletionSource();
                buildRenderTreeActionCallCount++;
            });

        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { nameof(TestComponent.Property1), "Value1" },
            { nameof(TestComponent.Property2), "Value2" }
        });

        Exception? thrownException = null;

        // act
        await component.CallBaseInvokeAsync(
            async () =>
            {
                try
                {
                    await component.SetParametersAsync(parameters);
                }
                catch (Exception exception)
                {
                    thrownException = exception;
                }
            });

        // assert
        if (expectInvalidOperationException)
            Assert.IsType<InvalidOperationException>(thrownException);
        else
            Assert.Null(thrownException);

        onInitializedAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(expectedOnInitializedCallCount));
        onInitializedAyncAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(expectedOnInitializedAsyncCallCount));
        onParametersSetAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(expectedOnParametersSetCallCount));
        onParametersSetAsyncAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(expectedOnParametersSetAsyncCallCount));

        preventDefaultRenderingAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(expectedStateHasChangedCallCount));
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(excpectedBuildRenderTreeCallCount));

        Assert.Equal(
            expectedOnInitializedCallCount,
            onInitializedActionCallCount);
        Assert.Equal(
            expectedOnInitializedAsyncCallCount,
            onInitializedAyncActionCallCount);
        Assert.Equal(
            expectedOnParametersSetCallCount,
            onParametersSetActionCallCount);
        Assert.Equal(
            expectedOnParametersSetAsyncCallCount,
            onParametersSetAsyncActionCallCount);
        Assert.Equal(
            excpectedBuildRenderTreeCallCount,
            buildRenderTreeActionCallCount);
    }

    [Fact]
    public async Task TestStateHasChanged()
    {
        // arrange
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);

        // act
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        // assert
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestStateHasChanged_PreventDefaultRendering()
    {
        // arrange
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        preventDefaultRenderingAction.Setup(obj => obj.Invoke())
            .Returns(true);

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestStateHasChanged_ShouldRender()
    {
        // arrange
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(false);

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);

        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestStateHasChanged_MultipleRendersAtOnce()
    {
        // arrange
        var isFirstBuildRenderTreeActionCall = true;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                if (isFirstBuildRenderTreeActionCall)
                {
                    component.CallBaseStateHasChanged();
                    component.CallBaseStateHasChanged();
                }

                isFirstBuildRenderTreeActionCall = false;
            });

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TestStateHasChanged_MultipleRendersAfterEachOther()
    {
        // arrange
        var isFirstBuildRenderTreeActionCall = true;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                if (isFirstBuildRenderTreeActionCall)
                    component.CallBaseStateHasChanged();

                isFirstBuildRenderTreeActionCall = false;
            });

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(3));
    }

    [Fact]
    public async Task TestStateHasChanged_RendererUninitialized()
    {
        // act & assert
        renderer.ThrowExceptionOnRendering = true;
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await component.CallBaseInvokeAsync(
                component.CallBaseStateHasChanged));

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Never);

        renderer.ThrowExceptionOnRendering = false;
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TestEnforceStateHasChanged()
    {
        // arrange
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        preventDefaultRenderingAction.Setup(obj => obj.Invoke())
            .Returns(true);

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestEnforceStateHasChanged_ShouldRender()
    {
        // arrange
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(false);

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);

        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestEnforceStateHasChanged_MultipleRendersAtOnce()
    {
        // arrange
        var isFirstBuildRenderTreeActionCall = true;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                if (isFirstBuildRenderTreeActionCall)
                {
                    component.CallBaseEnforceStateHasChanged();
                    component.CallBaseEnforceStateHasChanged();
                }

                isFirstBuildRenderTreeActionCall = false;
            });

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TestEnforceStateHasChanged_MultipleRendersAfterEachOther()
    {
        // arrange
        var isFirstBuildRenderTreeActionCall = true;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                if (isFirstBuildRenderTreeActionCall)
                    component.CallBaseEnforceStateHasChanged();

                isFirstBuildRenderTreeActionCall = false;
            });

        // act & assert
        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);
        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(3));
    }

    [Fact]
    public async Task TestEnforceStateHasChanged_RendererUninitialized()
    {
        // arrange, act & assert
        renderer.ThrowExceptionOnRendering = true;
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await component.CallBaseInvokeAsync(
                component.CallBaseEnforceStateHasChanged));

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Never);

        renderer.ThrowExceptionOnRendering = false;
        await component.CallBaseInvokeAsync(
            component.CallBaseEnforceStateHasChanged);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(2));
    }

    [Fact]
    public async Task TestInvokeAsync()
    {
        // act
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        // assert
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestInvokeAsync_AsyncWorkItem()
    {
        // act
        await component.CallBaseInvokeAsync(
            async () =>
            {
                await Task.Yield();
                component.CallBaseStateHasChanged();
            });

        // assert
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
    }

    [Fact]
    public async Task TestDispatchExceptionAsync()
    {
        // arrange
        var exception = new Exception();

        // act
        await component.CallBaseDispatchExceptionAsync(exception);

        // assert
        Assert.Equal(exception, renderer.HandledException);
    }

    [Fact]
    public void TestBuildRenderTree()
    {
        // arrange
        var renderTreeBuilder = new RenderTreeBuilder();

        // act
        component.CallBaseBuildRenderTree(renderTreeBuilder);
    }

    [Fact]
    public void TestOnInitialized()
    {
        // act
        component.CallBaseOnInitialized();
    }

    [Fact]
    public void TestOnInitializedAsync()
    {
        // act
        var result = component.CallBaseOnInitializedAsync();

        // assert
        Assert.Equal(Task.CompletedTask, result);
    }

    [Fact]
    public void TestOnParametersSet()
    {
        // act
        component.CallBaseOnParametersSet();
    }

    [Fact]
    public void TestOnParametersSetAsync()
    {
        // act
        var result = component.CallBaseParametersSetAsync();

        // assert
        Assert.Equal(Task.CompletedTask, result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestOnAfterRender(bool firstRender)
    {
        // act
        component.CallBaseOnAfterRender(firstRender);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestOnAfterRenderAsync(bool firstRender)
    {
        // act
        var result = component.CallBaseOnAfterRenderAsync(firstRender);

        // assert
        Assert.Equal(Task.CompletedTask, result);
    }

    [Fact]
    public void TestShouldRender()
    {
        // act
        var result = component.CallBaseShouldRender();

        // assert
        Assert.True(result);
    }

    [Fact]
    public void TestPreventDefaultRendering()
    {
        // act
        var result = component.CallBasePreventDefaultRendering();

        // assert
        Assert.False(result);
    }

    [Theory]
    [InlineData(1, 1, false, false, true, false, false)]
    [InlineData(1, 0, false, true, true, false, false)]
    [InlineData(2, 2, false, false, false, false, false)]
    [InlineData(1, 1, false, false, true, false, true)]
    [InlineData(1, 1, false, false, false, false, true)]
    [InlineData(1, 1, true, false, true, true, false)]
    [InlineData(1, 1, true, false, false, true, false)]
    public async Task TestHandleEventAsync(
       int expectedStateHasChangedCallCount,
       int excpectedBuildRenderTreeCallCount,
       bool expectInvalidOperationException,
       bool preventDefaultRendering,
       bool returnCompletedTask,
       bool throwOnTaskCompletion,
       bool cancelTask)
    {
        // arrange
        await component.CallBaseInvokeAsync(
            component.CallBaseStateHasChanged);

        preventDefaultRenderingAction.Reset();
        buildRenderTreeAction.Reset();

        var taskCompletionSource = new TaskCompletionSource();
        Action<TaskCompletionSource> taskCompleter =
            taskCompletionSource => taskCompletionSource.SetResult();

        if (throwOnTaskCompletion)
        {
            taskCompleter =
                taskCompletionSource => taskCompletionSource.SetException(
                    new InvalidOperationException());
        }

        if (cancelTask)
        {
            taskCompleter =
                taskCompletionSource => taskCompletionSource.SetCanceled();
        }

        var action = new Mock<Func<object, Task>>();
        action.Setup(obj => obj.Invoke(It.IsAny<object>()))
            .Returns(() =>
            {
                if (returnCompletedTask)
                    taskCompleter(taskCompletionSource);
                return taskCompletionSource.Task;
            });

        var handleEvent = component as IHandleEvent;
        var callback = new EventCallbackWorkItem(
            action.Object);

        var arg = new object();

        var buildRenderTreeActionCallCount = 0;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        preventDefaultRenderingAction.Setup(obj => obj.Invoke())
            .Returns(preventDefaultRendering);
        buildRenderTreeAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                action.Verify(obj => obj.Invoke(arg));

                if (!taskCompletionSource.Task.IsCompleted)
                    taskCompleter(taskCompletionSource);

                taskCompletionSource = new TaskCompletionSource();
                buildRenderTreeActionCallCount++;
            });

        Exception? thrownException = null;

        // act
        await component.CallBaseInvokeAsync(
            async () =>
            {
                try
                {
                    await handleEvent.HandleEventAsync(callback, arg);
                }
                catch (Exception exception)
                {
                    thrownException = exception;
                }
            });

        // assert
        if (expectInvalidOperationException)
            Assert.IsType<InvalidOperationException>(thrownException);
        else
            Assert.Null(thrownException);

        preventDefaultRenderingAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(expectedStateHasChangedCallCount));
        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Exactly(excpectedBuildRenderTreeCallCount));

        Assert.Equal(
            excpectedBuildRenderTreeCallCount,
            buildRenderTreeActionCallCount);
    }

    [Fact]
    public async Task TestHandleAfterRender()
    {
        // arrange
        var handleAfterRender = component as IHandleAfterRender;

        var onAfterRenderAsyncActionCallCount = 0;
        onAfterRenderAsyncAction.Setup(obj => obj.Invoke(It.IsAny<bool>()))
            .Callback(() =>
            {
                onAfterRenderAction.Verify(
                    obj => obj.Invoke(true),
                    Times.Once);

                onAfterRenderAsyncActionCallCount++;
            });

        // act
        await handleAfterRender.OnAfterRenderAsync();

        // assert
        onAfterRenderAction.Verify(
            obj => obj.Invoke(true),
            Times.Once);
        onAfterRenderAsyncAction.Verify(
            obj => obj.Invoke(true),
            Times.Once);

        Assert.Equal(1, onAfterRenderAsyncActionCallCount);
    }

    [Fact]
    public async Task TestHandleAfterRender_AlreadyRendered()
    {
        // arrange
        var handleAfterRender = component as IHandleAfterRender;

        await handleAfterRender.OnAfterRenderAsync();
        onAfterRenderAction.Reset();
        onAfterRenderAsyncAction.Reset();

        var onAfterRenderAsyncActionCallCount = 0;
        onAfterRenderAsyncAction.Setup(obj => obj.Invoke(It.IsAny<bool>()))
            .Callback(() =>
            {
                onAfterRenderAction.Verify(
                    obj => obj.Invoke(false),
                    Times.Once);

                onAfterRenderAsyncActionCallCount++;
            });

        // act
        await handleAfterRender.OnAfterRenderAsync();

        // assert
        onAfterRenderAction.Verify(
            obj => obj.Invoke(false),
            Times.Once);
        onAfterRenderAsyncAction.Verify(
            obj => obj.Invoke(false),
            Times.Once);

        Assert.Equal(1, onAfterRenderAsyncActionCallCount);
    }
}
