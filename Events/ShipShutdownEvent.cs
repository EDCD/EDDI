using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipShutdownEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Ship shutdown";
        public const string DESCRIPTION = "Triggered when your ship's system is forced to shutdown";
        public const string SAMPLE = @"{ ""timestamp"":""2017-01-05T23:15:06Z"", ""event"":""SystemsShutdown"" }";

        [PublicAPI( "True if shutdown is momentary, with flickering power which does not fully disable the ship" )]
        public bool partialshutdown { get; set; }

        public async Task ScheduleRebootAsync (
            Func<Event, Task> enqueueEvent,
            CancellationToken token )
        {
            // The ship reboots about 30 seconds after the shutdown occurs unless canceled early.
            var startTime = DateTime.UtcNow;

            try
            {
                await Task.Delay( TimeSpan.FromSeconds( 30 ), token ).ConfigureAwait( false );
                var rebootEvent = new ShipShutdownRebootEvent( this.timestamp + (DateTime.UtcNow - startTime));
                await enqueueEvent( rebootEvent ).ConfigureAwait(false);
            }
            catch ( OperationCanceledException )
            {
                // expected, ignore
            }
        }
    }
}
