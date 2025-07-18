using BlazorLighthouse.Core;
using BlazorLighthouseTest.Types;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Moq;

namespace BlazorLighthouseTest.Core;

public partial class LighthouseComponentBaseTest
{
    private readonly Mock<Action> buildRenderTreeAction;
    private readonly Mock<Action> onInitializedAction;
    private readonly Mock<Func<Task>> onInitializedAyncAction;
    private readonly Mock<Action> onParametersSetAction;
    private readonly Mock<Func<Task>> onParametersSetAsyncAction;
    private readonly Mock<Action<bool>> onAfterRenderAction;
    private readonly Mock<Func<bool, Task>> onAfterRenderAsyncAction;
    private readonly Mock<Func<bool>> shouldRenderAction;
    private readonly Mock<Func<bool>> disableStateHasChangedAction;

    private readonly TestComponent component;

    private readonly RendererFake renderer;

    public LighthouseComponentBaseTest()
    {
        buildRenderTreeAction = new();
        onInitializedAction = new();
        onInitializedAyncAction = new();
        onParametersSetAction = new();
        onParametersSetAsyncAction = new();
        onAfterRenderAction = new();
        onAfterRenderAsyncAction = new();
        shouldRenderAction = new();
        disableStateHasChangedAction = new();

        component = new TestComponent()
        {
            BuildRenderTreeAction = buildRenderTreeAction.Object,
            OnInitializedAction = onInitializedAction.Object,
            OnInitializedAyncAction = onInitializedAyncAction.Object,
            OnParametersSetAction = onParametersSetAction.Object,
            OnParametersSetAsyncAction = onParametersSetAsyncAction.Object,
            OnAfterRenderAction = onAfterRenderAction.Object,
            OnAfterRenderAsyncAction = onAfterRenderAsyncAction.Object,
            ShouldRenderAction = shouldRenderAction.Object,
            DisableStateHasChangedAction = disableStateHasChangedAction.Object
        };

        renderer = RendererFake.Create();
        renderer.Attach(component);
    }

    internal class TestComponent() : LighthouseComponentBase
    {
        public required Action BuildRenderTreeAction { get; init; }
        public required Action OnInitializedAction { get; init; }
        public required Func<Task> OnInitializedAyncAction { get; init; }
        public required Action OnParametersSetAction { get; init; }
        public required Func<Task> OnParametersSetAsyncAction { get; init; }
        public required Action<bool> OnAfterRenderAction { get; init; }
        public required Func<bool, Task> OnAfterRenderAsyncAction { get; init; }
        public required Func<bool> ShouldRenderAction { get; init; }
        public required Func<bool> DisableStateHasChangedAction { get; init; }

        [Parameter]
        public object? Property1 { get; set; }
        [Parameter]
        public object? Property2 { get; set; }

        public RendererInfo GetRendererInfo()
        {
            return RendererInfo;
        }

        public ResourceAssetCollection GetAssets()
        {
            return Assets;
        }

        public IComponentRenderMode? GetAssignedRenderMode()
        {
            return AssignedRenderMode;
        }

        public void ExecuteStateHasChanged()
        {
            StateHasChanged();
        }

        public void ExecuteEnforceStateHasChanged()
        {
            EnforceStateHasChanged();
        }

        public Task ExecuteInvokeAsync(Action workItem)
        {
            return InvokeAsync(workItem);
        }

        public Task ExecuteInvokeAsync(Func<Task> workItem)
        {
            return InvokeAsync(workItem);
        }

        public Task ExecuteDispatchExceptionAsync(Exception exception)
        {
            return DispatchExceptionAsync(exception);
        }

        public Task ExecuteOnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        public Task ExecuteOnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        public Task ExecuteOnAfterRenderAsync(bool firstRender)
        {
            return base.OnAfterRenderAsync(firstRender);
        }

        public bool ExecuteShouldRender()
        {
            return base.ShouldRender();
        }

        public bool ExecuteDisableStateHasChanged()
        {
            return base.DisableStateHasChanged();
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            BuildRenderTreeAction.Invoke();
        }

        protected override void OnInitialized()
        {
            OnInitializedAction.Invoke();
        }

        protected override Task OnInitializedAsync()
        {
             return OnInitializedAyncAction.Invoke();
        }

        protected override void OnParametersSet()
        {
            OnParametersSetAction.Invoke();
        }

        protected override Task OnParametersSetAsync()
        {
            return OnParametersSetAsyncAction.Invoke();
        }

        protected override void OnAfterRender(bool firstRender)
        {
            OnAfterRenderAction.Invoke(firstRender);
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            return OnAfterRenderAsyncAction.Invoke(firstRender);
        }

        protected override bool ShouldRender()
        {
            return ShouldRenderAction.Invoke();
        }

        protected override bool DisableStateHasChanged()
        {
            return DisableStateHasChangedAction.Invoke();
        }
    }
}
