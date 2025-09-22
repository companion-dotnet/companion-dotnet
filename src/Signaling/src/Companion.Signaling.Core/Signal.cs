using Companion.Signaling.Core.Internal;

namespace Companion.Signaling.Core;

/// <summary>
/// Simple writable value store that allows subscription to changes
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public sealed class Signal<T> : ReadonlySignal<T>
{
    private T value;

    /// <summary>
    /// Instantiate a new <see cref="Signal{T}"/> that belongs to no <see cref="SignalingContext"/>
    /// </summary>
    /// <param name="value">Initial signal value</param>
    public Signal(T value) : this(null, value)
    {

    }

    /// <summary>
    /// Instantiate a new <see cref="Signal{T}"/> that belongs to the specified <see cref="SignalingContext"/>
    /// </summary>
    /// <param name="context">Context to define lifespan</param>
    /// <param name="value">Initial signal value</param>
    public Signal(SignalingContext? context, T value)
        : base(context ?? new())
    {
        this.value = value;
    }

    /// <inheritdoc/>
    public override T Get()
    {
        context.AssertIsNotDisposed();
        TrackingBeacon.Register(this);
        return value;
    }

    /// <summary>
    /// Sets the stored value and notifies all current subscribers that changes happened
    /// Recalculations are run synchronized, if any fail, this method will throw the exception.
    /// </summary>
    /// <param name="value">The new value to store</param>
    public void Set(T value)
    {
        context.AssertIsNotDisposed();
        if (Equals(this.value, value))
            return;

        this.value = value;
        ValueHasChanged();
    }
}
