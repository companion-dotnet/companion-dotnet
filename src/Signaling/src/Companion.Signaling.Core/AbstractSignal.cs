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

    internal AbstractSignal(SignalingContext context)
    {
        this.context = context;
        context.RegisterContextDisposable(this);
    }

    /// <summary>
    /// Notify the signaling system that the value has chagned
    /// </summary>
    internal protected void ValueHasChanged()
    {
        var currentRefreshables = GetRefreshables();
        Refresh(currentRefreshables);
    }

    internal void RegisterRefreshable(IRefreshable refreshable)
    {
        lock (context.LockObject)
        {
            RegisterRefreshableSynchronized(refreshable);
        }
    }

    internal void UnregisterRefreshable(IRefreshable refreshable)
    {
        refreshables.RemoveWhere(
            weakRefreshable => weakRefreshable
                .TryGetTarget(out var referencedRefreshable)
                    && ReferenceEquals(referencedRefreshable, refreshable));
    }

    private ISet<WeakReference<IRefreshable>> GetRefreshables()
    {
        lock (refreshablesLock)
        {
            var currentRefreshables = refreshables;
            refreshables = [];
            return currentRefreshables;
        }
    }

    private void RegisterRefreshableSynchronized(IRefreshable refreshable)
    {
        context.AssertIsNotDisposed();
        refreshables.Add(new(refreshable));
    }

    void IContextDisposable.Dispose()
    {
        foreach (var weakRefreshable in refreshables)
        {
            if (weakRefreshable.TryGetTarget(out var refreshable))
                refreshable.Dispose(this);
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
