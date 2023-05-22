using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEddnResponder.Properties;
using EddiEddnResponder.Sender;
using EddiEvents;
using EddiStatusService;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using Utilities;

namespace EddiEddnResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to EDDN.
    /// </summary>
    public class EDDNResponder : IEddiResponder
    {
        // Schema reference: https://github.com/EDCD/EDDN/tree/master/schemas

        public readonly EDDNState eddnState;
        public readonly List<ISchema> schemas = new List<ISchema>();
        public readonly List<ICapiSchema> capiSchemas = new List<ICapiSchema>();

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

        [UsedImplicitly]
        public EDDNResponder() : this(StarSystemSqLiteRepository.Instance)
        { }

        public EDDNResponder(IStarSystemRepository starSystemRepository, bool unitTesting = false)
        {
            EDDNSender.unitTesting = unitTesting;

            // Configure our state tracking toolkit
            eddnState = new EDDNState(starSystemRepository);

            // Populate our schemas list
            GetSchemas();

            // Handle status changes
            StatusService.StatusUpdatedEvent += StatusServiceOnStatusUpdatedEvent;

            // Handle Companion App station data
            CompanionAppService.Instance.CombinedStationEndpoints.StationUpdatedEvent += FrontierApiOnStationUpdatedEvent;
            
            Logging.Info($"Initialized {ResponderName()}");
        }

        private void FrontierApiOnStationUpdatedEvent(object sender, CompanionApiEndpointEventArgs e)
        {
            Logging.Debug($"Handling Frontier API data", new Dictionary<string, object>()
                {
                    { "Frontier API Data", e },
                    { "EDDN State", eddnState }
                });
            foreach (var schema in capiSchemas)
            {
                // The same Frontier API data may be handled by multiple schemas so we always iterate through each.
                schema.Handle(e.profileJson, e.marketJson, e.shipyardJson, e.fleetCarrierJson, e.fromLegacyServer, eddnState);
            }
        }

        private void StatusServiceOnStatusUpdatedEvent(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                eddnState.Location.GetLocationInfo(status);
            }
        }

        public void Handle(Event theEvent)
        {
            if (EDDI.Instance.inTelepresence)
            {
                // We don't do anything whilst in Telepresence
                return;
            }

            if (string.IsNullOrEmpty(theEvent.raw))
            {
                // A null value may indicate a synthetic event used to pass data within EDDI
                // (which should always be ignored)
                return;
            }

            Logging.Debug("Received event " + theEvent.raw);

            var data = Deserializtion.DeserializeData(theEvent.raw);
            var edType = JsonParsing.getString(data, "event");

            if (string.IsNullOrEmpty(edType) || data == null) { return; }

            // Attempt to obtain available game version data from the active event 
            eddnState.GameVersion.GetVersionInfo(edType, data);

            // Except as noted above, always attempt to obtain available location data from the active event 
            eddnState.Location.GetLocationInfo(edType, data);

            if (theEvent.fromLoad)
            {
                // Don't do anything further with data acquired during log loading,
                // just update our internal state and move on
                return;
            }

            // Handle events
            Logging.Debug($"Handling {edType} journal event", new Dictionary<string, object>()
                {
                    { "Event", data },
                    { "EDDN State", eddnState }
                });
            foreach (var schema in schemas)
            {
                if (schema.Handle(edType, ref data, eddnState))
                {
                    break;
                }
            }
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        { }

        public void Reload()
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
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                        var schema = type.InvokeMember(type.Name,
                            BindingFlags.CreateInstance,
                            null, null, null) as ISchema;
                        schemas.Add(schema);
                    }
                    if (type.GetInterfaces().Contains(typeof(ICapiSchema)))
                    {
                        // Ensure that the static constructor of the class has been run
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                        var capiSchema = type.InvokeMember(type.Name,
                            BindingFlags.CreateInstance,
                            null, null, null) as ICapiSchema;
                        capiSchemas.Add(capiSchema);
                    }
                }
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }
    }
}
