using Companion.Signaling.Core.Internal.Interfaces;

namespace Companion.Signaling.Core;

/// <summary>
/// Defines a scope for the existance of signals. Required if the lifetime of an object participating in signaling should be bound to something.
/// This could for example be a request or an ui component.
/// </summary>
public class SignalingContext : IDisposable
{
    private readonly List<WeakReference<IContextDisposable>> contextDisposables = [];

    private bool isDisposed = false;

    internal Lock LockObject { get; } = new();

    /// <summary>
    /// Dispose the context. Enforces clean up of all signaling ressources allocated by object referencing this context.
    /// They can no longer be used for anything afterwards.
    /// </summary>
    public virtual void Dispose()
    {
        lock (LockObject) 
        {
            DisposeSynchronized();
        }
    }

    internal void RegisterContextDisposable(
        WeakReference<IContextDisposable> weakAccessTracker)
    {
        lock (LockObject) 
        {
            RegisterContextDisposableSynchronized(weakAccessTracker);
        }
    }

    internal void AssertIsNotDisposed()
    {
        if (isDisposed)
            throw new InvalidOperationException("Context is already disposed");
    }

    private void DisposeSynchronized()
    {
        isDisposed = true;
        contextDisposables.ForEach(
            weakContextDisposable =>
            {
                if (weakContextDisposable.TryGetTarget(out var contextDisposable))
                    contextDisposable.Dispose();
            });
    }

    private void RegisterContextDisposableSynchronized(
        WeakReference<IContextDisposable> weakContextDisposable)
    {
        AssertIsNotDisposed();
        contextDisposables.Add(weakContextDisposable);
    }
}
