using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiEddnResponder.Properties;
using EddiEddnResponder.Schemas;
using EddiEddnResponder.Sender;
using EddiEvents;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiEddnResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to EDDN.
    /// </summary>
    public class EDDNResponder : IEddiResponder
    {
        // Schema reference: https://github.com/EDCD/EDDN/tree/master/schemas

        public readonly EDDNState eddnState = new();
        public readonly List<ISchema> schemas = [ ];
        public readonly List<ICapiSchema> capiSchemas = [ ];
        internal readonly EDDNSender eddnSender = new();
        private readonly object stateLock = new();

        public string ResponderName()
        {
            return EddnResources.ResourceManager.GetString( "name", CultureInfo.InvariantCulture );
        }

        public string LocalizedResponderName()
        {
            return EddnResources.name;
        }

        public string ResponderDescription()
        {
            return EddnResources.desc;
        }

        internal void FrontierApiOnStationUpdatedEvent(object sender, CompanionApiEndpointEventArgs e)
        {
            lock ( stateLock )
            {
                if ( eddnState.InSpecialGameInstance ) { return; }
                if ( EDDI.Instance.GameState.inTelepresence ||
                     !eddnState.Location.CheckStationData( e.profileJson, e.marketJson, e.shipyardJson ) )
                {
                    Logging.Debug( "Ignoring Frontier API station data that does not match the current docking context." );
                    return;
                }

                Logging.Debug("Handling Frontier API data", new Dictionary<string, object>()
                {
                    { "Frontier API Data", e },
                    { "EDDN State", eddnState }
                });
                foreach (var schema in capiSchemas)
                {
                    // The same Frontier API data may be handled by multiple schemas so we always iterate through each.
                    schema.Handle( e.profileJson, e.marketJson, e.shipyardJson, e.fleetCarrierJson, eddnState, eddnSender );
                }
            }
        }

        public Task HandleStatusAsync ( Status status )
        {
            lock ( stateLock )
            {
                if ( !eddnState.InSpecialGameInstance )
                {
                    eddnState.Location.GetLocationInfo( status );
                }
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync ( Event @event )
        {
            if ( string.IsNullOrEmpty( @event.raw ) )
            {
                // A null value may indicate a synthetic event used to pass data within EDDI
                // (which should always be ignored)
                return Task.CompletedTask;
            }

            Logging.Debug("Received event " + @event.raw);

            var data = Deserializtion.DeserializeData(@event.raw);
            if ( data == null ) { return Task.CompletedTask; }
            var edType = JsonParsing.getString(data, "event");

            if (string.IsNullOrEmpty(edType)) { return Task.CompletedTask; }

            lock ( stateLock )
            {
                // Track mode changes even during historical loading or CQC telepresence.
                if ( eddnState.UpdateGameMode( edType, data ) )
                {
                    foreach ( var schema in schemas.OfType<FSSSignalDiscoveredSchema>() )
                    {
                        schema.Reset();
                    }
                }
                if ( eddnState.InSpecialGameInstance )
                {
                    // Preserve header metadata without retaining instance location data.
                    eddnState.GameVersion.GetVersionInfo( edType, data );
                    return Task.CompletedTask;
                }
                if ( EDDI.Instance.GameState.inTelepresence )
                {
                    return Task.CompletedTask;
                }

                // Collect and hold necessary state data only from eligible journal context.
                eddnState.GetStateInfo( edType, data );

                if (@event.fromLoad)
                {
                    // Reconstruct state without submitting historical events.
                    return Task.CompletedTask;
                }

                // Handle events
                Logging.Debug($"Handling {edType} journal event", new Dictionary<string, object>()
                {
                    { "Event", data },
                    { "EDDN State", eddnState }
                });
                foreach ( var schema in schemas )
                {
                    if ( schema.Handle( edType, ref data, eddnState, eddnSender ) )
                    {
                        break;
                    }
                }
            }

            return Task.CompletedTask;
        }

        public bool Start()
        {
            // Populate our schemas list
            GetSchemas();

            // Handle Companion App station data
            CompanionAppService.Instance.CombinedStationEndpoints.StationUpdatedEvent += FrontierApiOnStationUpdatedEvent;

            Logging.Info( $"Initialized {ResponderName()}" );

            return true;
        }

        public void Stop ()
        {
            CompanionAppService.Instance.CombinedStationEndpoints.StationUpdatedEvent -= FrontierApiOnStationUpdatedEvent;
        }

        public void Reload ()
        { }

        void GetSchemas()
        {
            foreach (var type in typeof(ISchema).Assembly.GetTypes())
            {
                if (!type.IsInterface && !type.IsAbstract)
                {
                    if (type.GetInterfaces().Contains(typeof(ISchema)))
                    {
                        // Ensure that the static constructor of the class has been run
                        RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                        var schema = type.InvokeMember(type.Name,
                            BindingFlags.CreateInstance,
                            null, null, null) as ISchema;
                        schemas.Add(schema);
                    }
                    if (type.GetInterfaces().Contains(typeof(ICapiSchema)))
                    {
                        // Ensure that the static constructor of the class has been run
                        RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                        var capiSchema = type.InvokeMember(type.Name,
                            BindingFlags.CreateInstance,
                            null, null, null) as ICapiSchema;
                        capiSchemas.Add(capiSchema);
                    }
                }
            }

            if ( schemas.Count == 0 )
            {
                Logging.Warn( "Failed to load EDDN journal schemas." );
            }
            if ( capiSchemas.Count == 0 )
            {
                Logging.Warn( "Failed to load EDDN Frontier API schemas." );
            }
        }

        internal void SetUnitTesting(bool unitTesting)
        {
            eddnSender.unitTesting = unitTesting;
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }
    }
}
