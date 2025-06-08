using System;
using System.Collections.Generic;
using System.Timers;

namespace EddiDataDefinitions
{
    public class StarSystemSignalSourceManager : IDisposable
    {
        private readonly Timer _cleanupTimer;
        private readonly List<StarSystem> _starSystems = new List<StarSystem>();
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="StarSystemSignalSourceManager"/> class, which periodically cleans up expired signal sources within registered star systems.
        /// </summary>
        /// <remarks>The cleanup process is performed automatically at the specified interval using a
        /// timer. </remarks>
        /// <param name="intervalMs">The interval, in milliseconds, at which expired signal sources are cleaned up.  Defaults to 30000 ms (30 seconds)
        /// milliseconds (1 minute).</param>
        public StarSystemSignalSourceManager ( double intervalMs = 30000 )
        {
            _cleanupTimer = new Timer( intervalMs );
            _cleanupTimer.Elapsed += ( s, e ) => CleanupExpiredSignalSources();
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Start();
        }

        public void Register ( StarSystem system )
        {
            lock ( _lock )
            {
                if ( !_starSystems.Contains( system ) )
                {
                    _starSystems.Add( system );
                }
            }
        }

        public void Unregister ( StarSystem system )
        {
            lock ( _lock )
            {
                _starSystems.Remove( system );
            }
        }

        private void CleanupExpiredSignalSources ()
        {
            lock ( _lock )
            {
                foreach ( var system in _starSystems )
                {
                    var builder = system.signalSources.ToBuilder();
                    builder.RemoveAll( s => s.expiry != null && s.expiry < DateTime.UtcNow );
                    system.signalSources = builder.ToImmutable();
                }
            }
        }

        public void Dispose ()
        {
            _cleanupTimer?.Dispose();
        }
    }
}
