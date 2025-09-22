using Companion.Signaling.Core.Internal;
using Companion.Signaling.Core.Internal.Interfaces;

namespace Companion.Signaling.Core;

/// <summary>
/// Calculates values based on other signal values. Result automatically gets updated when those values change.
/// </summary>
/// <typeparam name="T">Result type</typeparam>
public sealed class DerivedSignal<T> : ReadonlySignal<T>, IRefreshable
{
    private readonly Func<T> valueProvider;
    private readonly AccessTracker accessTracker;
    private readonly Lazy<Signal<T>> lazySignal;
    private readonly Lock isEvaluationQueuedLock = new();

    private WeakReference<IRefreshable>? weakReference;
    WeakReference<IRefreshable> IRefreshable.WeakReference
        => weakReference ??= new(this);

    internal bool IsEvaluationQueued { get; private set; } = false;

    /// <summary>
    /// Instantiate a new <see cref="DerivedSignal{T}"/> value that belongs to no <see cref="SignalingContext"/>
    /// </summary>
    /// <param name="valueProvider">Provider for the value</param>
    public DerivedSignal(Func<T> valueProvider) : this(null, valueProvider)
    {

    }

    /// <summary>
    /// Instantiate a new  <see cref="DerivedSignal{T}"/> value that belongs to the specified <see cref="SignalingContext"/>
    /// </summary>
    /// <param name="context">Context to define lifespan</param>
    /// <param name="valueProvider">Provider for the value</param>
    public DerivedSignal(SignalingContext? context, Func<T> valueProvider)
        : base(context ?? new())
    {
        this.valueProvider = valueProvider;
        accessTracker = new(this, context, true);

        lazySignal = new(() => new(EvaluateValueProvider()));
        _ = lazySignal.Value;
    }

    /// <inheritdoc/>
    public override T Get()
    {
        context.AssertIsNotDisposed();
        return lazySignal.Value.Get();
    }

    private T EvaluateValueProvider()
    {
        return accessTracker.Track(() => {
            IsEvaluationQueued = false;
            return valueProvider();
        });
    }

    private bool SetEvaluationQueued()
    {
        lock (isEvaluationQueuedLock)
        {
            return SetEvaluationQueuedSync();
        }
    }

    private bool SetEvaluationQueuedSync()
    {
        if (IsEvaluationQueued)
            return false;

        IsEvaluationQueued = true;
        return true;
    }

    void IRefreshable.Refresh()
    {
        if (!SetEvaluationQueued())
            return;

        lazySignal.Value.Set(EvaluateValueProvider());
    }

    void IRefreshable.Dispose(WeakReference<AbstractSignal> weakSignal)
    {
        accessTracker.Untrack(weakSignal);
    }
}
