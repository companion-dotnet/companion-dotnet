using BenchmarkDotNet.Attributes;
using Companion.Signaling.Benchmark.Components;
using Companion.Signaling.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Companion.Signaling.Benchmark;

public class SignalingBenchmark
{
    private readonly IServiceProvider serviceProvider;

    public SignalingBenchmark()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddTransient<HtmlRenderer>();

        serviceProvider = services.BuildServiceProvider();
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

        await using var htmlRenderer = serviceProvider.GetRequiredService<HtmlRenderer>();
        await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            await htmlRenderer.RenderComponentAsync<DefaultComponent>(parameterView));
    }

    [Benchmark]
    public async Task RenderSignalingComponent()
    {
        var parameterView = ParameterView.FromDictionary(new Dictionary<string, object?>()
        {
            { nameof(DefaultComponent.Value), new Signal<string>("value") }
        });

        await using var htmlRenderer = serviceProvider.GetRequiredService<HtmlRenderer>();
        await htmlRenderer.Dispatcher.InvokeAsync(async () =>
            await htmlRenderer.RenderComponentAsync<SignalingComponent>(parameterView));
    }
}
