using Companion.Signaling.Core.Internal;
using Companion.Signaling.Core.Internal.Interfaces;

namespace Companion.Signaling.Core;

/// <summary>
/// Runs arbitrary action on signal value changes
/// </summary>
public sealed class Effect : IRefreshable
{
    private readonly Action callback;
    private readonly AccessTracker accessTracker;
    private readonly Lock isRunQueuedLock = new();

    private WeakReference<IRefreshable>? weakReference;
    WeakReference<IRefreshable> IRefreshable.WeakReference
        => weakReference ??= new(this);

    internal bool IsRunQueued { get; private set; } = false;

    /// <summary>
    /// Instantiate a new effect that belongs to no context
    /// </summary>
    /// <param name="callback">Arbitrary action to call</param>
    public Effect(Action callback) : this(null, callback)
    {

    }

    /// <summary>
    /// Instantiate a new effect that belongs to the specified context
    /// </summary>
    /// <param name="context">Context to define lifespan</param>
    /// <param name="callback">Arbitrary action to call</param>
    public Effect(SignalingContext? context, Action callback)
    {
        this.callback = callback;
        accessTracker = new(this, context, true);

        RunCallback();
    }

    private void RunCallback()
    {
        accessTracker.Track(() => {
            IsRunQueued = false;
            callback();
        });
    }

    private bool SetRunQueued()
    {
        lock (isRunQueuedLock)
        {
            return SetRunQueuedSync();
        }
    }

    private bool SetRunQueuedSync()
    {
        if (IsRunQueued)
            return false;

        IsRunQueued = true;
        return true;
    }

    void IRefreshable.Refresh()
    {
        if (!SetRunQueued())
            return;

        RunCallback();
    }

    void IRefreshable.Dispose(WeakReference<AbstractSignal> weakSignal)
    {
        accessTracker.Untrack(weakSignal);
    }
}
