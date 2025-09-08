using System.Diagnostics.CodeAnalysis;

namespace Companion.Signaling.Core.Internal.Interfaces;

internal interface IRefreshable
{
    public static WeakReference<IRefreshable> None { get; }
        = new EmptyRefreshable().WeakReference;

    internal WeakReference<IRefreshable> WeakReference { get; }

    internal void Refresh();
    internal void Dispose(WeakReference<AbstractSignal> weakSignal);

    [ExcludeFromCodeCoverage]
    public class EmptyRefreshable : IRefreshable
    {
        public WeakReference<IRefreshable> WeakReference { get; }

        public EmptyRefreshable()
        {
            WeakReference = new(this);
        }

        void IRefreshable.Refresh()
        {

        }

        void IRefreshable.Dispose(WeakReference<AbstractSignal> signal)
        {

        }
    }
}
