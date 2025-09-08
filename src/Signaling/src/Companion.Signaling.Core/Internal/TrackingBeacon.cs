using Companion.Signaling.Core.Internal.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Companion.Signaling.Core.Internal;

internal static class TrackingBeacon
{
    [ThreadStatic]
    private static Stack<TrackingToken>? trackingTokens;

    public static void Push(WeakReference<IRefreshable> refreshable)
    {
        InitializeTrackingTokens();
        trackingTokens.Push(new(refreshable));
    }

    public static void Register(AbstractSignal signal)
    {
        InitializeTrackingTokens();
        if (trackingTokens.Count == 0)
            return;

        var trackingToken = trackingTokens.Peek();
        trackingToken.Signals.Add(signal.WeakReference);
        signal.RegisterRefreshable(trackingToken.WeakRefreshable);
    }

    public static HashSet<WeakReference<AbstractSignal>> Pop()
    {
        InitializeTrackingTokens();

        var trackingToken = new TrackingToken(IRefreshable.None);
        if (trackingTokens.Count != 0)
            trackingToken = trackingTokens.Pop();

        return trackingToken.Signals;
    }

    [MemberNotNull(nameof(trackingTokens))]
    private static void InitializeTrackingTokens()
    {
        trackingTokens ??= [];
    }

    private class TrackingToken(WeakReference<IRefreshable> refreshable)
    {
        public WeakReference<IRefreshable> WeakRefreshable { get; } = refreshable;
        public HashSet<WeakReference<AbstractSignal>> Signals { get; } = [];
    }
}
