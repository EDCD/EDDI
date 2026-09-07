using EddiEvents;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.EventHandling
{
    internal interface IEventScheduler : IDisposable
    {
        void Schedule ( TimeSpan delay, Func<Event> createEvent );
    }

    internal sealed class EventScheduler ( Action<Event> enqueueEvent ) : IEventScheduler
    {
        private readonly CancellationTokenSource cancellation = new();

        public void Schedule ( TimeSpan delay, Func<Event> createEvent )
        {
            RunAsync( delay, createEvent, cancellation.Token )
                .SafeFireAndForget( ex => Logging.Error( "Transfer arrival scheduling failed", ex ) );
        }

        private async Task RunAsync ( TimeSpan delay, Func<Event> createEvent, CancellationToken token )
        {
            try
            {
                await Task.Delay( delay, token ).ConfigureAwait( false );
                token.ThrowIfCancellationRequested();
                enqueueEvent( createEvent() );
            }
            catch ( OperationCanceledException ) when ( token.IsCancellationRequested ) { }
        }

        public void Dispose ()
        {
            cancellation.Cancel();
            cancellation.Dispose();
        }
    }
}
