using Companion.Signaling.Core.Internal.Interfaces;

namespace Companion.Signaling.Core;

/// <summary>
/// Base class for all types of signals
/// </summary>
public abstract class AbstractSignal : IContextDisposable
{
    private readonly Lock refreshablesLock = new();

    /// <summary>
    /// Signaling context for the current signal
    /// </summary>
    protected readonly SignalingContext context;

    private HashSet<WeakReference<IRefreshable>> refreshables = [];

    internal WeakReference<AbstractSignal> WeakReference { get; }

    internal AbstractSignal(SignalingContext context)
    {
        this.context = context;
        WeakReference = new(this);

        context.RegisterContextDisposable(new(this));
    }

    /// <summary>
    /// Notify the signaling system that the value has chagned
    /// </summary>
    internal protected void ValueHasChanged()
    {
        var currentRefreshables = GetRefreshables();
        Refresh(currentRefreshables);
    }

    internal void RegisterRefreshable(
        WeakReference<IRefreshable> weakRefreshable)
    {
        lock (context.LockObject)
        {
            RegisterRefreshableSynchronized(weakRefreshable);
        }
    }

    internal void UnregisterRefreshable(
        WeakReference<IRefreshable> weakRefreshable)
    {
        refreshables.Remove(weakRefreshable);
    }

    private HashSet<WeakReference<IRefreshable>> GetRefreshables()
    {
        lock (refreshablesLock)
        {
            var currentRefreshables = refreshables;
            refreshables = [];
            return currentRefreshables;
        }
    }

    private void RegisterRefreshableSynchronized(
        WeakReference<IRefreshable> weakRefreshable)
    {
        context.AssertIsNotDisposed();
        refreshables.Add(weakRefreshable);
    }

    void IContextDisposable.Dispose()
    {
        foreach (var weakRefreshable in refreshables)
        {
            if (weakRefreshable.TryGetTarget(out var refreshable))
                refreshable.Dispose(WeakReference);
        }

        refreshables.Clear();
    } 

    private static void Refresh(ISet<WeakReference<IRefreshable>> refreshables)
    {
        foreach (var weakRefreshable in refreshables)
        {
            if (weakRefreshable.TryGetTarget(out var refreshable))
                refreshable.Refresh();
        }
    }
}
