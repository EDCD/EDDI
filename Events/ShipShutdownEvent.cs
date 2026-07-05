using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Shapes;
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

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad, CancellationTokenSource ShipShutdownCancellationTokenSource )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            if ( ShipShutdownCancellationTokenSource != null )
            {
                // Ignore repetitions when the ship is already in a shut-down state. 
            }
            else
            {
                events.Add( new ShipShutdownEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
            }

            return true;
        }

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
