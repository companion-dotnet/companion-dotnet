using Companion.Signaling.Core.Internal.Interfaces;

namespace Companion.Signaling.Core.Internal;

internal class AccessTracker : IContextDisposable
{
    private readonly IRefreshable refreshable;
    private readonly SignalingContext context;
    private readonly Lock signalsLock = new();

    private HashSet<WeakReference<AbstractSignal>> signals = [];

    public AccessTracker(IRefreshable refreshable, SignalingContext? context)
    {
        this.refreshable = refreshable;
        this.context = context ?? new();

        context?.RegisterContextDisposable(this);
    }

    public void Track(Action action)
    {
        _ = Track<object?>(() =>
        {
            action();
            return null;
        });
    }
    
    public T Track<T>(Func<T> func)
    {
        lock (signalsLock)
        {
            return TrackSynchronized(func);
        }
    }

    public void Untrack(AbstractSignal signal)
    {
        lock (signalsLock)
        {
            UntrackSynchronized(signal);
        }
    }

    public void Dispose()
    {
        lock (signalsLock)
        {
            Untrack();
        }
    }

    private void Untrack()
    {
        foreach (var weakSignal in signals)
        {
            if (weakSignal.TryGetTarget(out var signal))
                signal.UnregisterRefreshable(refreshable);
        }
    }

    private T TrackSynchronized<T>(Func<T> func)
    {
        Untrack();
        context.AssertIsNotDisposed();

        TrackingBeacon.Push(refreshable);
        var value = func();
        signals = TrackingBeacon.Pop();

        return value;
    }

    private void UntrackSynchronized(AbstractSignal signal)
    {
        signals.RemoveWhere(
            weakSignal => weakSignal
                .TryGetTarget(out var referencedSignal)
                    && ReferenceEquals(referencedSignal, signal));
    }
}
