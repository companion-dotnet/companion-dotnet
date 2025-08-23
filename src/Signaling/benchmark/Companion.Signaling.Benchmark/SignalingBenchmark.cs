using BenchmarkDotNet.Attributes;
using Companion.Signaling.Benchmark.Components;
using Companion.Signaling.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Companion.Signaling.Benchmark;

public class SignalingBenchmark
{
    private readonly HtmlRenderer htmlRenderer;

    public SignalingBenchmark()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();

        htmlRenderer = new HtmlRenderer(serviceProvider, loggerFactory);
    }

    [Benchmark]
    public void AccessVariable()
    {
        var x = 0;
        var y = x;
        _ = y;
    }

    [Benchmark]
    public void AccessSignal()
    {
        var x = new Signal<int>(0);
        var y = x.Get();
        _ = y;
    }

    [Benchmark]
    public void AccessSignalWithinComputed()
    {
        var x = new Signal<int>(0);
        var y = new Computed<int>(x.Get).Get();
        _ = y;
    }

    [Benchmark]
    public async Task RenderDefaultComponent()
    {
        var parameterView = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(DefaultComponent.Value), "value" }
        });

        await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            await htmlRenderer.RenderComponentAsync<DefaultComponent>(parameterView));
    }

    [Benchmark]
    public async Task RenderSignalingComponent()
    {
        var parameterView = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(SignalingComponent.Value), new Signal<string>("value") }
        });

        await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            await htmlRenderer.RenderComponentAsync<SignalingComponent>(parameterView));
    }
}
