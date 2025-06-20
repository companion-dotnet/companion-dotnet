using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.Logging;
using Moq;

namespace BlazorLighthouseTest.Types;

#pragma warning disable BL0006
internal class RendererFake(
    IServiceProvider serviceProvider,
    ILoggerFactory loggerFactory) 
        : Renderer(serviceProvider, loggerFactory)
{
    public int GetComponentRenderModeCallCount { get; private set; }

    protected override RendererInfo RendererInfo { get; }  = new(nameof(RendererFake), true);
    public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

    public void Attach(IComponent component)
    {
        AssignRootComponentId(component);
    }

    public RendererInfo GetRendererInfo()
    {
        return RendererInfo;
    }

    public ResourceAssetCollection GetAssets()
    {
        return Assets;
    }
    protected override void HandleException(Exception exception)
    {
        throw exception;
    }

    protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
    {
        return Task.CompletedTask;
    }

    protected override IComponentRenderMode? GetComponentRenderMode(IComponent component)
    {
        GetComponentRenderModeCallCount++;
        return new ComponentRenderMode();
    }

    public static RendererFake Create()
    {
        var serviceProvider = new Mock<IServiceProvider>();
        var loggerFactory = LoggerFactory.Create(builder => { });
        return new(serviceProvider.Object, loggerFactory);
    }

    public class ComponentRenderMode : IComponentRenderMode
    {

    }
}

