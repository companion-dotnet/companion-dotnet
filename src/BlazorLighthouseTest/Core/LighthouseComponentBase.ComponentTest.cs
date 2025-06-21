using BlazorLighthouseTest.Types;
using Microsoft.AspNetCore.Components;
using Moq;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
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
        var renderer2 = RendererFake.Create();

        // act & assert
        Assert.Throws<InvalidOperationException>(
            () => renderer2.Attach(component));
    }
    
    [Fact]
    public void TestOnInitializedAsync()
    {
        // act
        var result = component.ExecuteOnInitializedAsync();

        // assert
        Assert.Equal(Task.CompletedTask, result);
    }
    
    [Fact]
    public void TestOnParametersSetAsync()
    {
        // act
        var result = component.ExecuteOnParametersSetAsync();

        // assert
        Assert.Equal(Task.CompletedTask, result);
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void TestOnAfterRenderAsync(bool firstRender)
    {
        // act
        var result = component.ExecuteOnAfterRenderAsync(firstRender);

        // assert
        Assert.Equal(Task.CompletedTask, result);
    }
    
    [Fact]
    public void TestShouldRender()
    {
        // act
        var result = component.ExecuteShouldRender();

        // assert
        Assert.True(result);
    }
    
    [Fact]
    public void TestEnforceStateHasChanged()
    {
        // act
        var result = component.ExecuteEnforceStateHasChanged();

        // assert
        Assert.True(result);
    }

    [Fact]
    public async Task TestSetParametersAsync()
    {
        // arrange
        var enforceStateHasChangedActionCallCount = 0;
        var onInitializedAyncActionCallCount = 0;
        var onParametersSetAsyncActionCallCount = 0;
        shouldRenderAction.Setup(obj => obj.Invoke())
            .Returns(true);
        enforceStateHasChangedAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                Assert.Equal("Value1", component.Property1);
                Assert.Equal("Value2", component.Property2);
                enforceStateHasChangedActionCallCount++;
            })
            .Returns(true);
        onInitializedAyncAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                onInitializedAction.Verify(
                    obj => obj.Invoke(),
                    Times.Once);
                onInitializedAyncActionCallCount++;
            })
            .Returns(Task.CompletedTask);
        onParametersSetAsyncAction.Setup(obj => obj.Invoke())
            .Callback(() =>
            {
                onParametersSetAction.Verify(
                    obj => obj.Invoke(),
                    Times.Once);
                onParametersSetAsyncActionCallCount++;
            })
            .Returns(Task.CompletedTask);

        var parameters = ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            { nameof(TestComponent.Property1), "Value1" },
            { nameof(TestComponent.Property2), "Value2" }
        });

        // act
        await component.ExecuteInvokeAsync(
            async () => await component.SetParametersAsync(parameters));

        // assert
        enforceStateHasChangedAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
        onInitializedAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
        onInitializedAyncAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
        onParametersSetAction.Verify(
            obj => obj.Invoke(),
            Times.Once);
        onParametersSetAsyncAction.Verify(
            obj => obj.Invoke(),
            Times.Once);

        buildRenderTreeAction.Verify(
            obj => obj.Invoke(),
            Times.Once);

        Assert.Equal(1, enforceStateHasChangedActionCallCount);
        Assert.Equal(1, onInitializedAyncActionCallCount);
        Assert.Equal(1, onParametersSetAsyncActionCallCount);
    }   

    //[Fact]
    //public async Task TestInvokeAsync()
    //{
    //    // act
    //    await component.ExecuteInvokeAsync(
    //        component.ExecuteStateHasChanged);

    //    // assert
    //    renderer.Verify(obj => obj.Invoke(), Times.Once);
    //}

    //[Fact]
    //public async Task TestStateHasChanged()
    //{
    //    // arrange
    //    var buildRenderTree = new Mock<Action>();
    //    var component = new TestComponent(buildRenderTree.Object);

    //    var rendererFake = RendererFake.Create();
    //    rendererFake.Attach(component);

    //    // act
    //    await rendererFake.Dispatcher.InvokeAsync(
    //        component.ExecuteStateHasChanged);

    //    // assert
    //    buildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    //}

    //[Fact]
    //public async Task TestStateHasChanged_MultipleComponentRedraws()
    //{
    //    // arrange
    //    var innerBuildRenderTree = new Mock<Action>();
    //    var innerComponent = new TestComponent(() => innerBuildRenderTree.Object());

    //    var outerBuildRenderTree = new Mock<Action>();
    //    var outerComponent = new TestComponent(() =>
    //    {
    //        outerBuildRenderTree.Object();
    //        innerComponent.ExecuteStateHasChanged();
    //        innerComponent.ExecuteStateHasChanged();
    //    });

    //    var rendererFake = RendererFake.Create();
    //    rendererFake.Attach(innerComponent);
    //    rendererFake.Attach(outerComponent);

    //    // act
    //    await rendererFake.Dispatcher.InvokeAsync(
    //        outerComponent.ExecuteStateHasChanged);

    //    // assert
    //    innerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    //    outerBuildRenderTree.Verify(obj => obj.Invoke(), Times.Once);
    //}
}
