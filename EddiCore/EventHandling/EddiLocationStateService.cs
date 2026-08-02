using EddiCore.GameState;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using JetBrains.Annotations;
using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Utilities;

namespace EddiCore.EventHandling
{
    internal sealed class EddiLocationStateService : IDisposable
    {
        private readonly IEddiEventProcessorContext _context;
        private readonly StarSystemSignalSourceManager _signalSourceManager = new();

        internal EddiLocationStateService ( IEddiEventProcessorContext context )
        {
            _context = context;
        }

        private IEddiGameState GameState => _context.GameState;
        private IEddiGameStateMutator GameStateMutator => _context.GameStateMutator;
        private DataProviderService DataProvider => _context.DataProvider;
        private StarSystem CurrentStarSystem { get => GameState.CurrentStarSystem; set => GameStateMutator.CurrentStarSystem = value; }
        private StarSystem LastStarSystem { get => GameState.LastStarSystem; set => GameStateMutator.LastStarSystem = value; }
        private StarSystem NextStarSystem { get => GameState.NextStarSystem; set => GameStateMutator.NextStarSystem = value; }
        private StarSystem DestinationStarSystem => GameState.DestinationStarSystem;
        private Body CurrentStellarBody { set => GameStateMutator.CurrentStellarBody = value; }

        internal async Task UpdateCurrentSystemAsync ( [NotNull] string systemName, ulong systemAddress )
        {
            try
            {
                if ( string.IsNullOrEmpty( systemName ) || CurrentStarSystem?.systemAddress == systemAddress )
                {
                    return;
                }

                if ( CurrentStarSystem != null )
                {
                    CurrentStarSystem.signalSources = ImmutableList<SignalSource>.Empty;
                    _signalSourceManager.Unregister( CurrentStarSystem );
                    await DataProvider.SaveStarSystemAsync( CurrentStarSystem ).ConfigureAwait( false );
                }

                LastStarSystem = CurrentStarSystem;
                if ( NextStarSystem != null && NextStarSystem.systemAddress == systemAddress )
                {
                    CurrentStarSystem = NextStarSystem;
                    NextStarSystem = null;
                }
                else
                {
                    CurrentStarSystem = await DataProvider.GetOrCreateStarSystemAsync( systemAddress, systemName )
                        .ConfigureAwait( false );
                }

                _signalSourceManager.Register( CurrentStarSystem );

                if ( DestinationStarSystem?.systemAddress == CurrentStarSystem.systemAddress )
                {
                    await _context.updateDestinationSystemAsync( null ).ConfigureAwait( false );
                }
            }
            catch ( Exception e )
            {
                Logging.Error( e.Message, e );
            }
        }

        internal async Task UpdateCurrentStellarBodyAsync ( string bodyName, int? bodyId, string systemName, ulong systemAddress )
        {
            await UpdateCurrentSystemAsync( systemName, systemAddress ).ConfigureAwait( false );

            if ( CurrentStarSystem != null )
            {
                var body = CurrentStarSystem.bodies?.Find( s => s.bodyname == bodyName );
                if ( body == null )
                {
                    body = new Body
                    {
                        bodyname = bodyName,
                        bodyId = bodyId,
                        systemname = systemName,
                        systemAddress = systemAddress,
                    };
                    CurrentStarSystem.AddOrUpdateBody( body );
                }
                CurrentStellarBody = body;
            }
        }

        internal async Task SetNearSurfaceBodyAsync ( NearSurfaceEvent theEvent )
        {
            if ( theEvent.approaching_surface )
            {
                var body = CurrentStarSystem?.bodies?.Find( s => s.bodyname == theEvent.bodyname ) ??
                           new Body
                           {
                               bodyname = theEvent.bodyname, systemname = theEvent.systemname
                           };

                body.systemAddress = theEvent.systemAddress;
                CurrentStellarBody = body;
            }
            else
            {
                CurrentStellarBody = null;
            }
            await UpdateCurrentSystemAsync( theEvent.systemname, theEvent.systemAddress ).ConfigureAwait( false );
        }

        public void Dispose ()
        {
            _signalSourceManager.Dispose();
        }
    }
}
