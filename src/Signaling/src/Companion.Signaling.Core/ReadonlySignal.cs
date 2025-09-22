namespace Companion.Signaling.Core;

/// <summary>
/// Readonly value store that allows subscription to changes
/// </summary>
/// <typeparam name="T">Value type</typeparam>
public abstract class ReadonlySignal<T> : AbstractSignal
{
    internal ReadonlySignal(SignalingContext context)
        : base(context)
    {

    }

    /// <summary>
    /// Gets the current value stored and registers available subscribers to changes
    /// </summary>
    /// <returns>The current value stored</returns>
    public abstract T Get();
}
