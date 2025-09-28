using EddiConfigService;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo("Tests")]
namespace EddiJournalMonitor
{
    public class JournalMonitor : LogMonitor, IEddiMonitor
    {
        public JournalMonitor () : base( GetSavedGamesDir(), @"^Journal.*\.[0-9\.]+\.log$",
            ( result, isLogLoadEvent ) =>
                ForwardJournalEntries( result.ToList(), EDDI.Instance.enqueueEvent, isLogLoadEvent ) )
        { }

        private static readonly Regex JsonRegex = new Regex( @"^{.*}$", RegexOptions.Singleline );

        /// <summary>
        /// Holds a delayed event until we see an event of the type specified
        /// </summary>
        private static readonly ConcurrentDictionary<string, ConcurrentBag<Event>> DelayedEventHolder = new ConcurrentDictionary<string, ConcurrentBag<Event>>();

        private enum ShipyardType { [UsedImplicitly] ShipsHere, [UsedImplicitly] ShipsRemote }

        private static readonly Dictionary<long, CancellationTokenSource> carrierJumpCancellationTokenSources =
            new Dictionary<long, CancellationTokenSource>();

        internal static CancellationTokenSource ShipShutdownCancellationTokenSource;

        private static void ForwardJournalEntries ( IList<string> lines, Action<Event> callback, bool isLogLoadEventBatch )
        {
            if ( !lines.Any() ) { return; }

            var events = ParseJournalEntries(lines, isLogLoadEventBatch);

            // Append any delayed events
            foreach ( var @event in events.ToList() )
            {
                if ( DelayedEventHolder.TryRemove( @event.type, out var delayedEvents ) )
                {
                    events.AddRange( delayedEvents );
                }
            }

            // Enqueue events for processing
            events.ForEach(callback);
        }

        public static List<Event> ParseJournalEntries(IList<string> lines, bool fromLogLoad = false)
        {
            var events = lines
                .Where( line => !string.IsNullOrEmpty( line ) )
                .SelectMany( line => ParseJournalEntry( line, fromLogLoad ) )
                .ToList();

            if ( fromLogLoad ) { return events; }

            // Reorder DiscoveryScanEvent to occur after other events in the batch including body and star scans
            if ( events.Any( e => e is DiscoveryScanEvent ) )
            {
                var discoveryScanEvents = events.OfType<DiscoveryScanEvent>().ToList();
                events = events.Except( discoveryScanEvents ).Concat( discoveryScanEvents ).ToList();
            }

            // Reorder FSSSignalDiscovered to occur after other events in the batch including FSDJump events
            if ( events.Any( e => e is JumpedEvent ) )
            {
                var SignalDetectedEvents = events.OfType<SignalDetectedEvent>().ToList();
                events = events.Except( SignalDetectedEvents ).Concat( SignalDetectedEvents ).ToList();
            }

            // Reorder SystemScanComplete to occur after other events in the batch including body and star scans
            if ( events.Any( e => e is SystemScanComplete ) )
            {
                var SystemScanCompleteEvents = events.OfType<SystemScanComplete>().ToList();
                events = events.Except( SystemScanCompleteEvents ).Concat( SystemScanCompleteEvents ).ToList();
            }

            // We will ignore `USSDrop` journal events since they are redundant with `SupercruiseDestinationDrop` but we shall 
            // supplement our response to `SupercruiseDestinationDrop` with a signal source identifier.
            if ( lines.Any( l => l.Contains( "USSDrop" ) ) )
            {
                foreach ( var destinationArrivedEvent in events.OfType<DestinationArrivedEvent>() )
                {
                    destinationArrivedEvent.isSignalSource = true;
                }
            }

            // Handle any other sequential event patterns we wish to handle
            for ( var i = 0; i < events.Count; i++ )
            {
                if ( events[ i ] is ShipShutdownEvent shipShutdownEvent )
                {
                    if ( ( i + 1 ) <= ( events.Count - 1 ) && events[ i + 1 ] is MaterialCollectedEvent mce && mce.edname is "tg_shutdowndata" )
                    {
                        // If a ShipShutdown event is followed by a material collection event for Massive Energy Surge Analytics
                        // (available from Thargoid Titan energy pulses), ship systems are only momentarily impacted /
                        // flickering for a few seconds. Simulate a partial ship system shutdown.
                        shipShutdownEvent.partialshutdown = true;
                    }
                    else
                    {
                        // Simulate a full ship system shutdown and reboot.
                        // Suppress additional shutdown events during this time.
                        ShipShutdownCancellationTokenSource = new CancellationTokenSource();
                        var startTime = DateTime.UtcNow;
                        Task.Run( async () =>
                        {
                            // The ship reboots about 30 seconds after the shutdown occurs unless canceled early.
                            await Task.Delay( TimeSpan.FromSeconds( 30 ), ShipShutdownCancellationTokenSource.Token ).ConfigureAwait(false);
                            EDDI.Instance.enqueueEvent(
                                new ShipShutdownRebootEvent( shipShutdownEvent.timestamp +
                                                             ( DateTime.UtcNow - startTime ) ) );
                            ShipShutdownCancellationTokenSource?.Dispose();
                            ShipShutdownCancellationTokenSource = null;
                        } );
                    }
                }
            }

            return events;
        }

        public static List<Event> ParseJournalEntry(string line, bool fromLogLoad = false, bool fromSpeechResponderTest = false )
        {
            var events = new List<Event>();
            try
            {
                var match = JsonRegex.Match(line);
                if (match.Success)
                {
                    Logging.Debug($"Received event: {line}");
                    var data = Deserializtion.DeserializeData(line);

                    // Every event has a timestamp field
                    var timestamp = DateTime.UtcNow;
                    try
                    {
                        timestamp = JsonParsing.getDateTime("timestamp", data);
                    }
                    catch
                    {
                        Logging.Warn("Event without timestamp; using current time");
                    }

                    // Every event has an event field
                    if (!data.ContainsKey("event"))
                    {
                        Logging.Warn("Event without event field!", line);
                        return events;
                    }

                    // Get the `edType`
                    var edType = JsonParsing.getString(data, "event");
                    
                    var handled = false;
                    try
                    {
                        switch (edType)
                        {
                            #region Startup Events
                              
                            case "Cargo":
                                {
                                    var inventory = new List<CargoInfoItem>();

                                    var vessel = JsonParsing.getString(data, "Vessel") ?? EDDI.Instance.Vehicle;
                                    var cargocarried = JsonParsing.getOptionalInt(data, "Count") ?? 0;
                                    data.TryGetValue("Inventory", out var val);
                                    if (val != null)
                                    {
                                        var inventoryJson = (List<object>)val;
                                        foreach (var cargoJson in inventoryJson.Cast<IDictionary<string, object>>() )
                                        {
                                            var name = JsonParsing.getString(cargoJson, "Name");
                                            var missionid = JsonParsing.getOptionalULong(cargoJson, "MissionID");
                                            var count = JsonParsing.getInt(cargoJson, "Count");
                                            var stolen = JsonParsing.getInt(cargoJson, "Stolen");
                                            var info = new CargoInfoItem(name, missionid, count, stolen);
                                            inventory.Add(info);
                                        }
                                        events.Add(new CargoEvent(timestamp, false, vessel, inventory, cargocarried) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (CargoInfo.TryFromFile(timestamp, vessel, cargocarried, out var info, out line))
                                    {
                                        events.Add(new CargoEvent(timestamp, true, vessel, info.Inventory, cargocarried) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "ClearSavedGame":
                                {
                                    var name = JsonParsing.getString(data, "Name");
                                    var frontierID = JsonParsing.getString(data, "FID");
                                    events.Add(new ClearedSaveEvent(timestamp, name, frontierID) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;       
                            case "Commander":
                                {
                                    var name = JsonParsing.getString(data, "Name");
                                    var frontierID = JsonParsing.getString(data, "FID");
                                    events.Add(new CommanderLoadingEvent(timestamp, name, frontierID) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Fileheader":
                                {
                                    var filename = journalFileName;
                                    var version = JsonParsing.getString(data, "gameversion")?.Trim();
                                    var build = JsonParsing.getString(data, "build")?.Trim();
                                    Logging.Info($"GameVersion: {version}, Build {build}.");
                                    events.Add(new FileHeaderEvent(timestamp, filename, version, build) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Loadout":
                                {
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");
                                    var shipName = JsonParsing.getString(data, "ShipName");
                                    var shipIdent = JsonParsing.getString(data, "ShipIdent");

                                    var hullValue = JsonParsing.getOptionalLong(data, "HullValue");
                                    var modulesValue = JsonParsing.getOptionalLong(data, "ModulesValue");
                                    var hullHealth = EventParsing.sensibleHealth((JsonParsing.getOptionalDecimal(data, "HullHealth") ?? 1) * 100);
                                    var unladenMass = JsonParsing.getOptionalDecimal(data, "UnladenMass") ?? 0;
                                    var maxJumpRange = JsonParsing.getOptionalDecimal(data, "MaxJumpRange") ?? 0;

                                    var rebuy = JsonParsing.getLong(data, "Rebuy");

                                    // If ship is 'hot', then modules are also 'hot'
                                    var hot = JsonParsing.getOptionalBool(data, "Hot") ?? false;

                                    data.TryGetValue("Modules", out val);
                                    var modulesData = (List<object>)val;

                                    string paintjob = null;
                                    var hardpoints = new List<Hardpoint>();
                                    var compartments = new List<Compartment>();
                                    if (modulesData != null)
                                    {
                                        foreach (var moduleData in modulesData.Cast<IDictionary<string, object>>() )
                                        {
                                            // Common items
                                            var slot = JsonParsing.getString(moduleData, "Slot");
                                            var item = JsonParsing.getString(moduleData, "Item");
                                            var enabled = JsonParsing.getBool(moduleData, "On");
                                            var priority = JsonParsing.getInt(moduleData, "Priority");
                                            // Health is as 0->1 but we want 0->100, and to a sensible number of decimal places
                                            var health = JsonParsing.getDecimal(moduleData, "Health") * 100;
                                            if (health < 5)
                                            {
                                                health = Math.Round(health, 1);
                                            }
                                            else
                                            {
                                                health = Math.Round(health);
                                            }

                                            // Some built-in modules don't give "Value" keys in the Loadout event. We'll set them to zero to match the Frontier API.
                                            var price = JsonParsing.getOptionalLong(moduleData, "Value") ?? 0;

                                            // Ammunition
                                            var clip = JsonParsing.getOptionalInt(moduleData, "AmmoInClip");
                                            var hopper = JsonParsing.getOptionalInt(moduleData, "AmmoInHopper");

                                            // Engineering modifications
                                            moduleData.TryGetValue("Engineering", out var engineeringVal);
                                            var modified = engineeringVal != null;
                                            var engineeringData = (Dictionary<string, object>)engineeringVal;
                                            var blueprint = modified ? JsonParsing.getString(engineeringData, "BlueprintName") : null;
                                            var blueprintId = modified ? JsonParsing.getLong(engineeringData, "BlueprintID") : 0;
                                            var level = modified ? JsonParsing.getInt(engineeringData, "Level") : 0;
                                            var modification = Blueprint.FromEliteID(blueprintId, engineeringData)
                                                ?? Blueprint.FromEDNameAndGrade(blueprint, level) ?? Blueprint.None;
                                            var quality = modified ? JsonParsing.getDecimal(engineeringData, "Quality") : 0;
                                            var experimentalEffect = modified ? JsonParsing.getString(engineeringData, "ExperimentalEffect") : null;
                                            var modifiers = new List<EngineeringModifier>();
                                            if (modified)
                                            {
                                                engineeringData.TryGetValue("Modifiers", out var modifiersVal);
                                                var modifiersData = (List<object>)modifiersVal;
                                                foreach (var modifier in modifiersData.Cast<IDictionary<string, object>>() )
                                                {
                                                    try
                                                    {
                                                        var edname = JsonParsing.getString(modifier, "Label");
                                                        var currentValue = JsonParsing.getOptionalDecimal(modifier, "Value");
                                                        var originalValue = JsonParsing.getOptionalDecimal(modifier, "OriginalValue");
                                                        var lessIsGood = JsonParsing.getOptionalInt(modifier, "LessIsGood") == 1;
                                                        var valueStr = JsonParsing.getString(modifier, "ValueStr");
                                                        modifiers.Add(new EngineeringModifier
                                                        {
                                                            EDName = edname,
                                                            currentValue = currentValue,
                                                            originalValue = originalValue,
                                                            lessIsGood = lessIsGood,
                                                            valueStr = valueStr
                                                        });
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        Logging.Error($"Failed to parse engineering modification for item {JsonConvert.SerializeObject(item)}", e);
                                                    }
                                                }
                                            }
                                            if (slot.Contains("Hardpoint"))
                                            {
                                                // This is a hardpoint
                                                var hardpoint = new Hardpoint() { name = slot };
                                                if (hardpoint.name.StartsWith("Tiny"))
                                                {
                                                    hardpoint.size = 0;
                                                }
                                                else if (hardpoint.name.StartsWith("Small", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 1;
                                                }
                                                else if (hardpoint.name.StartsWith("Medium", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 2;
                                                }
                                                else if (hardpoint.name.StartsWith("Large", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 3;
                                                }
                                                else if (hardpoint.name.StartsWith("Huge", StringComparison.InvariantCultureIgnoreCase))
                                                {
                                                    hardpoint.size = 4;
                                                }

                                                var module = new Module(Module.FromEDName(item, moduleData) ?? new Module());
                                                if (module.edname == null)
                                                {
                                                    Logging.Info("Unknown module " + item, JsonConvert.SerializeObject(moduleData));
                                                }
                                                else
                                                {
                                                    module.hot = hot;
                                                    module.enabled = enabled;
                                                    module.priority = priority;
                                                    module.health = health;
                                                    module.price = price;
                                                    module.ammoinclip = clip;
                                                    module.ammoinhopper = hopper;
                                                    module.modified = modified;
                                                    module.modificationEDName = blueprint;
                                                    module.engineermodification = modification;
                                                    module.engineerlevel = level;
                                                    module.engineerquality = quality;
                                                    module.engineerExperimentalEffectEDName = experimentalEffect;
                                                    module.modifiers = modifiers;
                                                    hardpoint.module = module;
                                                    hardpoints.Add(hardpoint);
                                                }
                                            }
                                            else if (slot.Equals("PaintJob", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // This is a paintjob
                                                paintjob = item;
                                            }
                                            else if (slot.Equals("PlanetaryApproachSuite", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore planetary approach suite for now
                                            }
                                            else if (slot.StartsWith("Bobble", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore bobbles
                                            }
                                            else if (slot.StartsWith("Decal", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore decals
                                            }
                                            else if (slot.StartsWith("StringLights", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore string lights
                                            }
                                            else if (slot.Equals("WeaponColour", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore weapon colour
                                            }
                                            else if (slot.Equals("EngineColour", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore engine colour
                                            }
                                            else if (slot.StartsWith("ShipKit", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore ship kits
                                            }
                                            else if (slot.StartsWith("ShipName", StringComparison.InvariantCultureIgnoreCase) || slot.StartsWith("ShipID", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore nameplates
                                            }
                                            else if (slot.Equals("VesselVoice", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore the chosen voice
                                            }
                                            else if (slot.Equals("DataLinkScanner", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore the data link scanner
                                            }
                                            else if (slot.Equals("CodexScanner", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                // Ignore the codex scanner
                                            }
                                            else
                                            {
                                                // This is a compartment
                                                var compartment = EventParsing.ShipCompartment(ship, slot);
                                                // Compartment slots are in the form of "Slotnn_Sizen" or "Militarynn"

                                                var module = new Module(Module.FromEDName(item, moduleData) ?? new Module());
                                                if (module.edname == null)
                                                {
                                                    Logging.Info("Unknown module " + item, JsonConvert.SerializeObject(moduleData));
                                                }
                                                else
                                                {
                                                    module.hot = hot;
                                                    module.enabled = enabled;
                                                    module.priority = priority;
                                                    module.health = health;
                                                    module.price = price;
                                                    module.ammoinclip = clip;
                                                    module.ammoinhopper = hopper;
                                                    module.modified = modified;
                                                    module.modificationEDName = blueprint;
                                                    module.engineermodification = modification;
                                                    module.engineerlevel = level;
                                                    module.engineerquality = quality;
                                                    module.engineerExperimentalEffectEDName = experimentalEffect;
                                                    module.modifiers = modifiers;
                                                    compartment.module = module;
                                                    compartments.Add(compartment);
                                                }
                                            }
                                        }
                                    }
                                    events.Add(new ShipLoadoutEvent(timestamp, ship, shipId, shipName, shipIdent, hullValue, modulesValue, hullHealth, unladenMass, maxJumpRange, rebuy, hot, compartments, hardpoints, paintjob) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;      
                            case "Materials":
                                {
                                    var materials = new List<MaterialAmount>();

                                    data.TryGetValue("Raw", out var val);
                                    if (val != null)
                                    {
                                        var materialsJson = (List<object>)val;
                                        foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                        {
                                            var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    data.TryGetValue("Manufactured", out val);
                                    if (val != null)
                                    {
                                        var materialsJson = (List<object>)val;
                                        foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                        {
                                            var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    data.TryGetValue("Encoded", out val);
                                    if (val != null)
                                    {
                                        var materialsJson = (List<object>)val;
                                        foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                        {
                                            var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    events.Add(new MaterialInventoryEvent(DateTime.UtcNow, materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Missions":
                                {
                                    var possibleStatuses = new [] 
                                    {
                                        MissionStatus.Active, 
                                        MissionStatus.Failed, 
                                        MissionStatus.Complete
                                    };

                                    var missions = new List<Mission>();
                                    foreach (var status in possibleStatuses)
                                    {
                                        data.TryGetValue(status.invariantName, out var val);
                                        var missionLog = (List<object>)val;

                                        foreach (var mission in missionLog)
                                        {
                                            var missionProperties = (Dictionary<string, object>)mission;
                                            var missionId = JsonParsing.getULong(missionProperties, "MissionID");
                                            var name = JsonParsing.getString(missionProperties, "Name");
                                            var localizedName = JsonParsing.getString(missionProperties, "Name_Localised");
                                            var expires = JsonParsing.getDecimal(missionProperties, "Expires");

                                            // Colonization missions use the actual unit timestamp rather than a timestamp offset so we need to handle those as a special case.
                                            var expiry = name == "$Mission_Colonisation_Initial_Name;"
                                                ? Dates.fromTimestamp( Convert.ToInt64( expires ) ) ?? DateTime.MinValue
                                                : timestamp.AddSeconds( (double)expires );

                                            // If mission is 'Active' and expires = 0, then set status to 'Claim'
                                            var missionStatus = status == MissionStatus.Active && expires == 0 
                                                ? MissionStatus.Claim: 
                                                status;
                                            var newMission = new Mission( missionId, name, expiry, missionStatus )
                                            {
                                                localisedname = localizedName
                                            };
                                            if (newMission == null)
                                            {
                                                // Mal-formed mission
                                                Logging.Error("Bad mission entry", mission);
                                            }
                                            else
                                            {
                                                missions.Add(newMission);
                                            }
                                        }
                                    }
                                    events.Add(new MissionsEvent(timestamp, missions) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NewCommander":
                                {
                                    var name = JsonParsing.getString(data, "Name");
                                    var frontierID = JsonParsing.getString(data, "FID");
                                    var package = JsonParsing.getString(data, "Package");
                                    events.Add(new CommanderStartedEvent(timestamp, name, frontierID, package) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LoadGame":
                                {
                                    var commander = JsonParsing.getString(data, "Commander");
                                    var frontierID = JsonParsing.getString(data, "FID");

                                    // Active expansions
                                    var horizons = JsonParsing.getOptionalBool(data, "Horizons") ?? false; // Whether the account has the Horizons DLC
                                    var odyssey = JsonParsing.getOptionalBool(data, "Odyssey") ?? false; // Whether the account has the Odyssey DLC
                                    Logging.Info($"Active expansions... Horizons: {horizons}, Odyssey: {odyssey}.");

                                    var shipEDModel = JsonParsing.getString(data, "Ship"); // This describes a vehicle, whether ship or otherwise.
                                                                                       // If on foot this may be a suit & if in an SRV then this may be an SRV.
                                    var shipName = JsonParsing.getString(data, "ShipName");
                                    var shipIdent = JsonParsing.getString(data, "ShipIdent");
                                    var shipId = JsonParsing.getOptionalLong(data, "ShipID"); // If on foot we'll get a suit ID here, which we need to treat as a long

                                    // shipId may be null either if we're logging into CQC or if we're logging in while in an Apex taxi service
                                    if (shipId == null)
                                    {
                                        if (!string.IsNullOrEmpty(shipEDModel) && shipEDModel.ToLowerInvariant().Contains("taxi"))
                                        {
                                            // This is a taxi
                                        }
                                        else
                                        {
                                            // The LoadGame event for entering CQC contains no ship details.
                                            // We are entering CQC. Flag it back to EDDI so we can ignore everything that happens until
                                            // we're out of CQC again
                                            events.Add(new EnteredCQCEvent(timestamp, commander) { raw = line, fromLoad = fromLogLoad });
                                            handled = true;
                                            break;
                                        }
                                    }
                                    
                                    var startedLanded = JsonParsing.getOptionalBool(data, "StartedLanded");
                                    var startDead = JsonParsing.getOptionalBool(data, "StartDead");

                                    var credits = JsonParsing.getOptionalLong(data, "Credits") ?? 0;
                                    var loan = JsonParsing.getOptionalLong(data, "Loan") ?? 0;

                                    var fuel = JsonParsing.getOptionalDecimal(data, "FuelLevel");
                                    var fuelCapacity = JsonParsing.getOptionalDecimal(data, "FuelCapacity");

                                    var version = JsonParsing.getString(data, "gameversion")?.Trim();
                                    var build = JsonParsing.getString(data, "build")?.Trim();

                                    var mode = GameMode.FromEDName(JsonParsing.getString(data, "GameMode"));
                                    var group = JsonParsing.getString(data, "Group"); // The name of the group, only if the mode is "Group" 

                                    events.Add(new CommanderContinuedEvent(timestamp, commander, frontierID, horizons, odyssey, shipId, shipEDModel, shipName, shipIdent, startedLanded, startDead, mode, group, credits, loan, fuel, fuelCapacity, version, build) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Passengers":
                                {
                                    var passengers = new List<Passenger>();
                                    data.TryGetValue("Manifest", out var val);
                                    var passengerManifest = (List<object>)val;

                                    foreach (var passenger in passengerManifest)
                                    {
                                        var passengerProperties = (Dictionary<string, object>)passenger;
                                        var missionid = JsonParsing.getULong(passengerProperties, "MissionID");
                                        var type = JsonParsing.getString(passengerProperties, "Type");
                                        var vip = JsonParsing.getBool(passengerProperties, "VIP");
                                        var wanted = JsonParsing.getBool(passengerProperties, "Wanted");
                                        var amount = JsonParsing.getInt(passengerProperties, "Count");

                                        var newPassenger = new Passenger(missionid, type, vip, wanted, amount);
                                        if (newPassenger == null)
                                        {
                                            // Mal-formed mission
                                            Logging.Error("Bad mission entry", passenger);
                                        }
                                        else
                                        {
                                            passengers.Add(newPassenger);
                                        }
                                    }
                                    events.Add(new PassengersEvent(timestamp, passengers) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Powerplay":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var rank = JsonParsing.getInt(data, "Rank") + 1; // This is zero based in the journal but not in the Frontier API. Adding +1 here synchronizes the two.
                                    var merits = JsonParsing.getInt(data, "Merits");
                                    var timePledged = TimeSpan.FromSeconds(JsonParsing.getLong(data, "TimePledged"));
                                    events.Add(new PowerplayEvent(timestamp, power, rank, merits, timePledged) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Progress":
                                {
                                    data.TryGetValue("Combat", out var val);
                                    decimal combat = (long?)val ?? 0;
                                    data.TryGetValue("Trade", out val);
                                    decimal trade = (long?)val ?? 0;
                                    data.TryGetValue("Explore", out val);
                                    decimal exploration = (long?)val ?? 0;
                                    data.TryGetValue("CQC", out val);
                                    decimal cqc = (long?)val ?? 0;
                                    data.TryGetValue("Empire", out val);
                                    decimal empire = (long?)val ?? 0;
                                    data.TryGetValue("Federation", out val);
                                    decimal federation = (long?)val ?? 0;
                                    data.TryGetValue("Soldier", out val);
                                    decimal soldier = (long?)val ?? 0;
                                    data.TryGetValue("Exobiologist", out val);
                                    decimal exobiologist = (long?)val ?? 0;

                                    events.Add(new CommanderProgressEvent(timestamp, combat, trade, exploration, cqc, empire, federation, soldier, exobiologist) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Rank":
                                {
                                    data.TryGetValue("Combat", out var val);
                                    var combat = CombatRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Trade", out val);
                                    var trade = TradeRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Explore", out val);
                                    var exploration = ExplorationRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("CQC", out val);
                                    var cqc = CQCRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Empire", out val);
                                    var empire = EmpireRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Federation", out val);
                                    var federation = FederationRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Soldier", out val);
                                    var mercenary = MercenaryRating.FromRank((int)((long?)val ?? 0));
                                    data.TryGetValue("Exobiologist", out val);
                                    var exobiologist = ExobiologistRating.FromRank((int)((long?)val ?? 0));

                                    events.Add(new CommanderRatingsEvent(timestamp, combat, trade, exploration, cqc, empire, federation, mercenary, exobiologist) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Reputation":
                                {
                                    var empire = JsonParsing.getOptionalDecimal(data, "Empire") ?? 0;
                                    var federation = JsonParsing.getOptionalDecimal(data, "Federation") ?? 0;
                                    var independent = JsonParsing.getOptionalDecimal(data, "Independent") ?? 0;
                                    var alliance = JsonParsing.getOptionalDecimal(data, "Alliance") ?? 0;
                                    events.Add(new CommanderReputationEvent(timestamp, empire, federation, independent, alliance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SquadronStartup":
                                {
                                    var name = JsonParsing.getString(data, "SquadronName");
                                    var squadronID = JsonParsing.getOptionalInt( data, "SquadronID" );
                                    var rankID = JsonParsing.getInt(data, "CurrentRank");
                                    var rankName = JsonParsing.getString(data, "CurrentRankName");
                                    var rankNameLocalized = JsonParsing.getString(data, "CurrentRankName_Localised");
                                    var rank = new SquadronRank(rankID, rankName, rankNameLocalized);

                                    events.Add(new SquadronStartupEvent(timestamp, name, squadronID, rank ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Statistics":
                                {
                                    var statistics = JObject.Parse( line ).ToObject<Statistics>();
                                    events.Add(new StatisticsEvent(timestamp, statistics) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Travel Events

                            case "ApproachBody":
                                {
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    events.Add(new NearSurfaceEvent(timestamp, true, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Docked":
                                {
                                    var systemName = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    EventParsing.StationNameAndType(data, out var stationName, out var stationLocalizedName, out var stationModel);

                                    var controllingfaction = EventParsing.Faction(data, "Station", systemName, systemAddress, EDDI.Instance.CurrentStarSystem?.factions);
                                    var distancefromstar = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                    // Get station landing pads data
                                    var landingPads = EventParsing.LandingPadsCollection(data);

                                    // Get station services data
                                    data.TryGetValue("StationServices", out var val);
                                    var stationservices = (val as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
                                    var stationServices = new List<StationService>();
                                    foreach (var service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get station economies and their shares
                                    data.TryGetValue("StationEconomies", out var val2);
                                    var economies = val2 as List<object> ?? new List<object>();
                                    var Economies = new List<EconomyShare>();
                                    foreach (var economyshare in economies.Cast<IDictionary<string, object>>() )
                                    {
                                        var economy = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                        economy.fallbackLocalizedName = JsonParsing.getString(economyshare, "Name_Localised");
                                        var share = JsonParsing.getDecimal(economyshare, "Proportion");
                                        if (economy != Economy.None && share > 0)
                                        {
                                            Economies.Add(new EconomyShare(economy, share));
                                        }
                                    }

                                    var cockpitBreach = JsonParsing.getOptionalBool(data, "CockpitBreach") ?? false;
                                    var wanted = JsonParsing.getOptionalBool(data, "Wanted") ?? false;
                                    var activeFine = JsonParsing.getOptionalBool(data, "ActiveFine") ?? false;

                                    var stationStateEdName = JsonParsing.getString( data, "StationState" );
                                    var stationState = string.IsNullOrEmpty( stationStateEdName )
                                        ? StationState.NormalOperation
                                        : StationState.FromEDName( stationStateEdName );

                                    var station = new Station()
                                    {
                                        name = stationName,
                                        localizedName = stationLocalizedName,
                                        marketId = marketId,
                                        systemname = systemName,
                                        systemAddress = systemAddress,
                                        Model = stationModel,
                                        Faction = controllingfaction,
                                        stationServices = stationServices,
                                        stationState = stationState,
                                        economyShares = Economies,
                                        distancefromstar = distancefromstar,
                                        landingPads = landingPads
                                    };

                                    events.Add(new DockedEvent(timestamp, station, cockpitBreach, wanted, activeFine) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingCancelled":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
                                    events.Add(new DockingCancelledEvent(timestamp, stationLocalizedName ?? stationName, stationType, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingDenied":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
                                    var reason = JsonParsing.getString(data, "Reason");
                                    events.Add(new DockingDeniedEvent(timestamp, stationLocalizedName ?? stationName, stationType, marketId, reason) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingGranted":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
                                    data.TryGetValue("LandingPad", out var val);
                                    var landingPad = (int)(long)val;
                                    events.Add(new DockingGrantedEvent(timestamp, stationLocalizedName ?? stationName, stationType, marketId, landingPad) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingRequested":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );

                                    // Get station landing pads data
                                    var landingPads = EventParsing.LandingPadsCollection(data);

                                    events.Add(new DockingRequestedEvent(timestamp, stationLocalizedName ?? stationName, stationType, marketId, landingPads) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockingTimeout":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    EventParsing.StationNameAndType( data, out var stationName, out var stationLocalizedName, out var stationType );
                                    events.Add(new DockingTimedOutEvent(timestamp, stationLocalizedName ?? stationName, stationType) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;                                
                            case "FSDJump":
                                {
                                    var systemName = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    data.TryGetValue("StarPos", out var val);
                                    var starPos = (List<object>)val;
                                    var x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                    var y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                    var z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;
                                    var starName = JsonParsing.getString(data, "Body"); // Documented by the journal, but apparently never written. We can't rely on this being set.
                                    var fuelUsed = JsonParsing.getDecimal(data, "FuelUsed");
                                    var fuelRemaining = JsonParsing.getDecimal(data, "FuelLevel");
                                    var boostUsed = JsonParsing.getOptionalInt(data, "BoostUsed"); // 1-3 are synthesis, 4 is any supercharge (white dwarf or neutron star)
                                    var distance = JsonParsing.getDecimal(data, "JumpDist");
                                    var economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy")) ?? Economy.None;
                                    var economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy")) ?? Economy.None;
                                    var security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity")) ?? SecurityLevel.None;
                                    var population = JsonParsing.getOptionalLong(data, "Population");

                                    // Parse factions array data
                                    var factions = new List<Faction>();
                                    data.TryGetValue("Factions", out var factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = EventParsing.Factions(factionsVal, systemName, systemAddress);
                                    }
                                    var controllingfaction = EventParsing.Faction(data, "System", systemName, systemAddress, factions);

                                    // Parse conflicts array data
                                    var conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out var conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = EventParsing.FactionConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    EventParsing.PowerplayDetails( data, systemAddress, out var controllingPower,
                                        out var powersInAcquisitionRange, out var powerplayState,
                                        out var powerAcquisitionProgress,
                                        out var powerplayControlProgress, out var powerplayReinforcementControlPoints,
                                        out var powerplayUnderminingControlPoints );

                                    // Thargoid war data (if any)
                                    EventParsing.ThargoidWarData( data, out var thargoidWar );

                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    events.Add( new JumpedEvent( timestamp, systemName, systemAddress, x, y, z,
                                        starName, distance, fuelUsed, fuelRemaining, boostUsed, controllingfaction,
                                        factions, conflicts, economy, economy2, security, population, controllingPower,
                                        powersInAcquisitionRange, powerplayState, powerAcquisitionProgress,
                                        powerplayControlProgress, powerplayReinforcementControlPoints,
                                        powerplayUnderminingControlPoints, taxi, multicrew, thargoidWar )
                                    {
                                        raw = line, fromLoad = fromLogLoad
                                    } );
                                }
                                handled = true;
                                break;
                            case "FSDTarget":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var systemName = JsonParsing.getString(data, "Name");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var remainingJumpsInRoute = JsonParsing.getOptionalInt(data, "RemainingJumpsInRoute") ?? 0;
                                    var starclass = JsonParsing.getString(data, "StarClass");
                                    events.Add(new FSDTargetEvent(timestamp, systemName, systemAddress, remainingJumpsInRoute, starclass) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LeaveBody":
                                {
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    events.Add(new NearSurfaceEvent(timestamp, false, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Liftoff":
                                {
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");
                                    var playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;

                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    // The nearest destination may be a specific destination name or a generic signal source.
                                    // Per the journal manual, the NearestDestination is included if within 50km of a location listed in the nav panel
                                    var nearestdestination = JsonParsing.getString(data, "NearestDestination");
                                    var nearestDestination = SignalSource.FromEDName(nearestdestination) ?? new SignalSource();
                                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                                    if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                    {
                                        nearestDestination.fallbackLocalizedName = localizedName;
                                    }
                                    events.Add(new LiftoffEvent(timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Location":
                                {
                                    var systemName = JsonParsing.getString(data, "StarSystem");

                                    if (systemName == "Training")
                                    {
                                        // Training system; ignore
                                        break;
                                    }

                                    data.TryGetValue("StarPos", out var val);
                                    var starPos = (List<object>)val;
                                    var x = Math.Round(JsonParsing.getDecimal("X", starPos[0]) * 32) / (decimal)32.0;
                                    var y = Math.Round(JsonParsing.getDecimal("Y", starPos[1]) * 32) / (decimal)32.0;
                                    var z = Math.Round(JsonParsing.getDecimal("Z", starPos[2]) * 32) / (decimal)32.0;
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var distFromStarLs = JsonParsing.getOptionalDecimal(data, "DistFromStarLS");

                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
                                    if ( bodyType == BodyType.Planet )
                                    {
                                        var starSystem = EDDI.Instance.DataProvider
                                            .GetOrCreateStarSystemAsync( systemAddress, systemName ).GetAwaiter()
                                            .GetResult();
                                        bodyType = starSystem?.bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == bodyId )?.bodyType ?? bodyType;
                                    }

                                    var docked = JsonParsing.getBool(data, "Docked");
                                    var economy = Economy.FromEDName(JsonParsing.getString(data, "SystemEconomy"));
                                    var economy2 = Economy.FromEDName(JsonParsing.getString(data, "SystemSecondEconomy"));
                                    var security = SecurityLevel.FromEDName(JsonParsing.getString(data, "SystemSecurity"));
                                    var population = JsonParsing.getOptionalLong(data, "Population");

                                    // If docked
                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    string stationName = null;
                                    string stationLocalizedName = null;
                                    StationModel stationtype = null;
                                    if ( marketId != null )
                                    {
                                        EventParsing.StationNameAndType( data, out stationName, out stationLocalizedName, out stationtype );
                                    }

                                    // Get station services data
                                    data.TryGetValue("StationServices", out val);
                                    var stationservices = (val as List<object>)?.Cast<string>()?.ToList() ?? new List<string>();
                                    var stationServices = new List<StationService>();
                                    foreach (var service in stationservices)
                                    {
                                        stationServices.Add(StationService.FromEDName(service));
                                    }

                                    // Get station economies and their shares
                                    data.TryGetValue("StationEconomies", out var val2);
                                    var economies = val2 as List<object> ?? new List<object>();
                                    var Economies = new List<EconomyShare>();
                                    foreach (var economyshare in economies.Cast<IDictionary<string, object>>() )
                                    {
                                        var economyShare = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                        economyShare.fallbackLocalizedName = JsonParsing.getString(economyshare, "Name_Localised");
                                        var share = JsonParsing.getDecimal(economyshare, "Proportion");
                                        if (economyShare != Economy.None && share > 0)
                                        {
                                            Economies.Add(new EconomyShare(economyShare, share));
                                        }
                                    }

                                    // If landed
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    // Parse factions array data
                                    var factions = new List<Faction>();
                                    data.TryGetValue("Factions", out var factionsVal);
                                    if (factionsVal != null)
                                    {
                                        factions = EventParsing.Factions(factionsVal, systemName, systemAddress );
                                    }
                                    var systemfaction = EventParsing.Faction(data, "System", systemName, systemAddress, factions);
                                    var stationfaction = EventParsing.Faction(data, "Station", systemName, systemAddress, factions);

                                    // Parse conflicts array data
                                    var conflicts = new List<Conflict>();
                                    data.TryGetValue("Conflicts", out var conflictsVal);
                                    if (conflictsVal != null)
                                    {
                                        conflicts = EventParsing.FactionConflicts(conflictsVal, factions);
                                    }

                                    // Powerplay data (if pledged)
                                    EventParsing.PowerplayDetails( data, systemAddress, out var controllingPower,
                                        out var powersInAcquisitionRange, out var powerplayState,
                                        out var powerAcquisitionProgress,
                                        out var powerplayControlProgress, out var powerplayReinforcementControlPoints,
                                        out var powerplayUnderminingControlPoints );

                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi") ?? false;
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew") ?? false;
                                    var inSRV = JsonParsing.getOptionalBool(data, "InSRV") ?? false;
                                    var onFoot = JsonParsing.getOptionalBool(data, "OnFoot") ?? false;

                                    // Thargoid war data (if any)
                                    EventParsing.ThargoidWarData( data, out var thargoidWar );

                                    events.Add( new LocationEvent( timestamp, systemName, systemAddress, x, y, z,
                                        distFromStarLs, body, bodyId, bodyType, longitude, latitude, docked,
                                        stationLocalizedName ?? stationName, stationtype, marketId, stationServices, systemfaction,
                                        stationfaction, factions, conflicts, Economies, economy, economy2, security,
                                        population, controllingPower, powersInAcquisitionRange, powerplayState,
                                        powerAcquisitionProgress, powerplayControlProgress,
                                        powerplayReinforcementControlPoints, powerplayUnderminingControlPoints,
                                        taxi, multicrew, inSRV, onFoot, thargoidWar )
                                    {
                                        raw = line,
                                        fromLoad = fromLogLoad
                                    } );
                                }
                                handled = true;
                                break;
                            case "NavRoute":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( NavRouteInfo.TryFromFile(timestamp, true, out var navRoute, out var rawRoute))
                                    {
                                        events.Add(new NavRouteEvent(timestamp, navRoute.Route) { raw = rawRoute, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "NavRouteClear":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( NavRouteInfo.TryFromFile(timestamp, false, out var navRoute, out var rawRoute))
                                    {
                                        events.Add(new NavRouteEvent(timestamp, navRoute.Route) { raw = rawRoute, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "StartJump":
                                {
                                    var target = JsonParsing.getString(data, "JumpType");
                                    var stellarclass = JsonParsing.getString(data, "StarClass");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getOptionalULong(data, "SystemAddress"); // Present only when the FSD target is hyperspace
                                    var isTaxi = JsonParsing.getOptionalBool( data, "Taxi" ) ?? false;
                                    events.Add(new FSDEngagedEvent(timestamp, target, system, systemAddress, stellarclass, isTaxi) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "SupercruiseEntry":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var system = JsonParsing.getString(data, "StarySystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
                                    events.Add(new EnteredSupercruiseEvent(timestamp, system, systemAddress, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SupercruiseExit":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var bodyType = BodyType.FromEDName(JsonParsing.getString(data, "BodyType")) ?? BodyType.None;
                                    if ( bodyType == BodyType.Planet )
                                    {
                                        bodyType = EDDI.Instance.CurrentStarSystem?.bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == bodyId )?.bodyType ??
                                                   bodyType;
                                    }
                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");
                                    events.Add(new EnteredNormalSpaceEvent(timestamp, system, systemAddress, body, bodyId, bodyType, taxi, multicrew) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Touchdown":
                                {
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");
                                    var playercontrolled = JsonParsing.getOptionalBool(data, "PlayerControlled") ?? true;

                                    var taxi = JsonParsing.getOptionalBool(data, "Taxi");
                                    var multicrew = JsonParsing.getOptionalBool(data, "Multicrew");

                                    // The nearest destination may be a specific destination name or a generic signal source.
                                    // Per the journal manual, the NearestDestination is included if within 50km of a location listed in the nav panel
                                    var nearestdestination = JsonParsing.getString(data, "NearestDestination");
                                    var nearestDestination = SignalSource.FromEDName(nearestdestination) ?? new SignalSource();
                                    var localizedName = JsonParsing.getString(data, "SignalName_Localised");
                                    if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                    {
                                        nearestDestination.fallbackLocalizedName = localizedName;
                                    }
                                    events.Add(new TouchdownEvent(timestamp, longitude, latitude, system, systemAddress, body, bodyId, onStation, onPlanet, taxi, multicrew, playercontrolled, nearestDestination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Undocked":
                                {
                                    var stationName = JsonParsing.getString(data, "StationName");
                                    long? marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new UndockedEvent(timestamp, stationName, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Combat (and Combat Reward) Events

                            case "Bounty":
                                {
                                    var target = JsonParsing.getString(data, "Target");
                                    var target_localised = JsonParsing.getString( data, "Target_Localised" );
                                    var victimName = JsonParsing.getString(data, "PilotName_Localised");
                                    var victimFaction = EventParsing.FactionName(data, "VictimFaction");

                                    data.TryGetValue("SharedWithOthers", out var val);
                                    var shared = val != null && (long)val == 1;

                                    long reward;
                                    var rewards = new List<Reward>();

                                    if (data.ContainsKey("Reward"))
                                    {
                                        // Old-style
                                        data.TryGetValue("Reward", out val);
                                        reward = (long)val;
                                        if (reward == 0)
                                        {
                                            // 0-credit reward; ignore
                                            break;
                                        }
                                        var factionName = EventParsing.FactionName(data, "Faction");
                                        rewards.Add(new Reward(factionName, reward));
                                    }
                                    else
                                    {
                                        data.TryGetValue("TotalReward", out val);
                                        reward = (long)val;
                                        if (reward == 0)
                                        {
                                            // 0-credit reward; ignore
                                            break;
                                        }
                                        // Obtain list of rewards
                                        data.TryGetValue("Rewards", out val);
                                        var rewardsData = (List<object>)val;
                                        if (rewardsData != null)
                                        {
                                            foreach (var rewardData in rewardsData.Cast<IDictionary<string, object>>() )
                                            {
                                                var factionName = EventParsing.FactionName(rewardData, "Faction");
                                                rewardData.TryGetValue("Reward", out val);
                                                var factionReward = (long)val;

                                                rewards.Add(new Reward(factionName, factionReward));
                                            }
                                        }
                                    }

                                    events.Add(new BountyAwardedEvent(timestamp, target, target_localised, victimName, victimFaction, reward, rewards, shared) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CapShipBond":
                                {
                                    data.TryGetValue( "Reward", out var val );
                                    var reward = (long)val;
                                    var victimFaction = EventParsing.FactionName(data, "VictimFaction");
                                    var awardingFaction = EventParsing.FactionName(data, "AwardingFaction");
                                    events.Add( new BondAwardedEvent( timestamp, awardingFaction, victimFaction, reward ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "Died":
                                {
                                    Killer parseKiller (IDictionary<string, object> killerData, bool singleKiller)
                                    {
                                        // Property names differ if there is a single killer vs. multiple killers
                                        var name = JsonParsing.getString(killerData, singleKiller ? "KillerName" : "Name");
                                        if (!string.IsNullOrEmpty(JsonParsing.getString(data, singleKiller ? "KillerName_Localised" : "Name_Localised")))
                                        {
                                            // This is an NPC with a symbolic name
                                            name = NpcAuthorityShip.EDNameExists(name)
                                                ? NpcAuthorityShip.FromEDName(name)?.localizedName
                                                : JsonParsing.getString(data, singleKiller ? "KillerName_Localised" : "Name_Localised");
                                        }

                                        var equipment = JsonParsing.getString(killerData, singleKiller ? "KillerShip" : "Ship"); // May be a ship, a suit, etc.
                                        var rating = CombatRating.FromEDName(JsonParsing.getString(killerData, singleKiller ? "KillerRank" : "Rank"));
                                        return new Killer(name, equipment, rating);
                                    }

                                    var killers = new List<Killer>();
                                    if (data.ContainsKey("KillerName"))
                                    {
                                        // Single killer
                                        killers.Add(parseKiller(data, true));
                                    }
                                    if (data.ContainsKey("killers"))
                                    {
                                        // Multiple killers
                                        data.TryGetValue("Killers", out var val);
                                        var killersData = (List<object>)val;
                                        foreach (var killerData in killersData.Cast<IDictionary<string, object>>() )
                                        {
                                            killers.Add(parseKiller(killerData, false));
                                        }
                                    }
                                    events.Add(new DiedEvent(timestamp, killers) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EscapeInterdiction":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var interdictor = JsonParsing.getString(data, "Interdictor");
                                    var iscommander = JsonParsing.getOptionalBool(data, "IsPlayer") ?? false;
                                    var isThargoid = JsonParsing.getOptionalBool(data, "isThargoid") ?? false;

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Interdictor_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        interdictor = NpcAuthorityShip.EDNameExists(interdictor)
                                            ? NpcAuthorityShip.FromEDName(interdictor)?.localizedName
                                            : JsonParsing.getString(data, "Interdictor_Localised");
                                    }
                                    else if ( isThargoid )
                                    {
                                        interdictor = NpcAuthorityShip.Thargoid.localizedName;
                                    }
                                    else if (string.IsNullOrEmpty(interdictor) && !data.ContainsKey("Interdictor"))
                                    {
                                        // This matches the pattern for an unknown ship interdiction attempt
                                        interdictor = NpcAuthorityShip.UNKNOWN.localizedName;
                                    }

                                    events.Add(new ShipInterdictedEvent(timestamp, false, false, iscommander, isThargoid, interdictor, null, null, null) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "FactionKillBond":
                                {
                                    data.TryGetValue("Reward", out var val);
                                    var reward = (long)val;
                                    var victimFaction = EventParsing.FactionName(data, "VictimFaction");
                                    var awardingFaction = EventParsing.FactionName(data, "AwardingFaction");
                                    events.Add(new BondAwardedEvent(timestamp, awardingFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FighterDestroyed":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var vehicle = "fighter";
                                    var fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle, null, fighterId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "HeatDamage":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    events.Add(new HeatDamageEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "HeatWarning":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    events.Add( new HeatWarningEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "HullDamage":
                                {
                                    var health = EventParsing.sensibleHealth(JsonParsing.getDecimal(data, "Health") * 100);
                                    var piloted = JsonParsing.getOptionalBool(data, "PlayerPilot");
                                    var fighter = JsonParsing.getOptionalBool(data, "Fighter");

                                    var vehicle = EDDI.Instance.Vehicle;
                                    if ( piloted == false )
                                    {
                                        if ( fighter == true )
                                        {
                                            vehicle = Constants.VEHICLE_FIGHTER;
                                        }
                                        else if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SRV )
                                        {
                                            vehicle = Constants.VEHICLE_SHIP;
                                        }
                                        else if ( EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP )
                                        {
                                            vehicle = Constants.VEHICLE_SRV;
                                        }
                                    }

                                    events.Add(new HullDamagedEvent(timestamp, vehicle, piloted, health) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Interdicted":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var submitted = JsonParsing.getBool(data, "Submitted");
                                    var interdictor = JsonParsing.getString(data, "Interdictor");
                                    var iscommander = JsonParsing.getOptionalBool(data, "IsPlayer") ?? false;
                                    var isThargoid = JsonParsing.getOptionalBool(data, "IsThargoid") ?? false;
                                    data.TryGetValue("CombatRank", out var val);
                                    var rating = (val == null ? null : CombatRating.FromRank(Convert.ToInt32(val)));
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var power = JsonParsing.getString(data, "Power");

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Interdictor_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        interdictor = NpcAuthorityShip.EDNameExists(interdictor)
                                            ? NpcAuthorityShip.FromEDName(interdictor)?.localizedName
                                            : JsonParsing.getString(data, "Interdictor_Localised");
                                    }
                                    else if ( isThargoid )
                                    {
                                        interdictor = NpcAuthorityShip.Thargoid.localizedName;
                                    }
                                    else if (string.IsNullOrEmpty(interdictor) && !data.ContainsKey("Interdictor") && string.IsNullOrEmpty(faction))
                                    {
                                        // This matches the pattern for an unknown ship interdiction attempt
                                        interdictor = NpcAuthorityShip.UNKNOWN.localizedName;
                                    }

                                    events.Add(new ShipInterdictedEvent(timestamp, true, submitted, iscommander, isThargoid, interdictor, rating, faction, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "Interdiction":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var success = JsonParsing.getBool(data, "Success");
                                    var interdictee = JsonParsing.getString(data, "Interdicted");
                                    var iscommander = JsonParsing.getBool(data, "IsPlayer");
                                    data.TryGetValue("CombatRank", out var val);
                                    var rating = ( val == null ? null : CombatRating.FromRank( Convert.ToInt32( val ) ) );
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var power = JsonParsing.getString(data, "Power");

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Interdicted_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        interdictee = NpcAuthorityShip.EDNameExists(interdictee) 
                                            ? NpcAuthorityShip.FromEDName(interdictee)?.localizedName 
                                            : JsonParsing.getString(data, "Interdicted_Localised");
                                    }

                                    events.Add(new ShipInterdictionEvent(timestamp, success, iscommander, interdictee, rating, faction, power) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "PVPKill":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var victim = JsonParsing.getString(data, "Victim");
                                    data.TryGetValue("CombatRank", out var val);
                                    var rating = (val == null ? null : CombatRating.FromRank((int)(long)val));

                                    events.Add(new KilledEvent(timestamp, victim, rating) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "ShieldState":
                                // As of September 2019, this event no longer appears to be written to the Player Journal.
                                // We still generate an event via the Status Monitor.
                                handled = true;
                                break;
                            case "ShipTargeted":
                                {
                                    var targetlocked = JsonParsing.getBool(data, "TargetLocked");

                                    // Target locked
                                    var scanstage = JsonParsing.getOptionalInt(data, "ScanStage");
                                    VehicleDefinition fighterDef = null;
                                    Ship shipDef = null;
                                    var vehicleEDName = JsonParsing.getString(data, "Ship");
                                    if ( vehicleEDName != null)
                                    {
                                        if (vehicleEDName.Contains("fighter", StringComparison.InvariantCultureIgnoreCase))
                                        {
                                            fighterDef = VehicleDefinition.FromEDName( vehicleEDName );
                                            fighterDef.fallbackLocalizedName = JsonParsing.getString( data, "Ship_Localised" );
                                        }
                                        else
                                        {
                                            shipDef = ShipDefinitions.FromEDModel(vehicleEDName, false);
                                            if ( shipDef is null )
                                            {
                                                shipDef = ShipDefinitions.FromEDModel( vehicleEDName, true );
                                                shipDef.model = JsonParsing.getString( data, "Ship_Localised" );
                                            }
                                        }
                                    }

                                    // Scan stage >= 1
                                    var name = JsonParsing.getString(data, "PilotName");
                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "PilotName_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        name = NpcAuthorityShip.EDNameExists(name) 
                                            ? NpcAuthorityShip.FromEDName(name)?.localizedName 
                                            : JsonParsing.getString(data, "PilotName_Localised");
                                    }

                                    // Sometimes we don't get a localized name when we ought to.
                                    // Strip out any remaining unlocalized content in the name.
                                    const string unlocalizedNameRegex = @"\$.+;";
                                    if ( !string.IsNullOrEmpty(name) && Regex.IsMatch( name, unlocalizedNameRegex ) )
                                    {
                                        var tidiedName = Regex.Replace( name, unlocalizedNameRegex, "" ).Trim();
                                        if ( !string.IsNullOrEmpty(tidiedName) )
                                        {
                                            name = tidiedName;
                                        }
                                    }

                                    var rank = CombatRating.FromEDName(JsonParsing.getString(data, "PilotRank"));

                                    // Scan stage >= 2
                                    var shieldHealth = JsonParsing.getOptionalDecimal(data, "ShieldHealth");
                                    var hullHealth = JsonParsing.getOptionalDecimal(data, "HullHealth");

                                    // Scan stage >= 3
                                    var faction = JsonParsing.getString(data, "Faction");
                                    var legalStatus = LegalStatus.FromEDName(JsonParsing.getString(data, "LegalStatus"));
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var bounty = JsonParsing.getOptionalInt(data, "Bounty");
                                    string subsystemName = null;
                                    decimal? subSystemHealth = null;
                                    if ( data.ContainsKey( "Subsystem" ) )
                                    {
                                        var subsystemEDName = JsonParsing.getString( data, "Subsystem" );
                                        subsystemName = subsystemEDName.StartsWith( "$ext_drive" ) 
                                            ? EddiDataDefinitions.Properties.Modules.Thrusters // The `ShipTargeted` event uses non-standard drive names
                                            : Module.FromEDName( subsystemEDName )?.localizedName;
                                        if ( string.IsNullOrEmpty(subsystemName) )
                                        {
                                            subsystemName = JsonParsing.getString( data, "Subsystem_Localised" );
                                        }
                                        subSystemHealth = JsonParsing.getOptionalDecimal( data, "SubsystemHealth" );
                                    }

                                    events.Add(new ShipTargetedEvent(timestamp, targetlocked, shipDef, fighterDef, scanstage, name, rank, faction, power, legalStatus, bounty, shieldHealth, hullHealth, subsystemName, subSystemHealth) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SRVDestroyed":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var vehicle = "srv";
                                    var srvId = JsonParsing.getOptionalInt(data, "ID");
                                    var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                                    vehicleDefinition.fallbackLocalizedName = JsonParsing.getString(data, "SRVType_Localised");
                                    events.Add(new VehicleDestroyedEvent(timestamp, vehicle, vehicleDefinition, srvId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "UnderAttack":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var target = JsonParsing.getString(data, "Target");
                                    events.Add(new UnderAttackEvent(timestamp, target) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                    break;
                                }

                            #endregion

                            #region Exploration

                            case "BuyExplorationData":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var system = JsonParsing.getString(data, "System");
                                    var price = JsonParsing.getLong(data, "Cost");
                                    events.Add(new ExplorationDataPurchasedEvent(timestamp, system, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSSAllBodiesFound":
                                {
                                    var systemName = JsonParsing.getString(data, "SystemName");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var count = JsonParsing.getInt(data, "Count");
                                    events.Add(new SystemScanComplete(timestamp, systemName, systemAddress, count) { raw = line, fromLoad = fromLogLoad });

                                }
                                handled = true;
                                break;
                            case "FSSBodySignals":
                                {
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var bodyName = JsonParsing.getString(data, "BodyName");
                                    var bodyId = JsonParsing.getLong(data, "BodyID");
                                    data.TryGetValue("Signals", out var signalsVal);

                                    // These are surface signal sources from a body that we've scanned
                                    var surfaceSignals = new List<SignalAmount>();
                                    foreach (var signal in ((List<object>)signalsVal).Cast<IDictionary<string, object>>())
                                    {
                                        var signalSource = JsonParsing.getString(signal, "Type");
                                        var source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                                        var localizedName = JsonParsing.getString(data, "Type_Localised");
                                        if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                        {
                                            source.fallbackLocalizedName = localizedName;
                                        }
                                        var amount = JsonParsing.getInt(signal, "Count");
                                        surfaceSignals.Add(new SignalAmount(source, amount));
                                    }
                                    surfaceSignals = surfaceSignals.OrderByDescending(s => s.amount).ToList();
                                    events.Add(new SurfaceSignalsEvent(timestamp, "FSS", systemAddress, bodyName, bodyId, surfaceSignals) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FSSDiscoveryScan":
                                {
                                    // A journal error was introduced in Odyssey 4.0 Update 17
                                    // where FSSDiscoveryScan would report zero bodies and infinite progress.
                                    // Don't parse `Progress` unless `BodyCount` is greater than zero.

                                    var bodyCount = JsonParsing.getInt(data, "BodyCount"); // total number of stellar bodies in system
                                    var nonBodyCount = JsonParsing.getInt(data, "NonBodyCount"); // total number of non-body signals found
                                    var progress = bodyCount > 0 ? JsonParsing.getDecimal(data, "Progress") : 0; // value from 0-1
                                    events.Add( new DiscoveryScanEvent( timestamp, progress, bodyCount, nonBodyCount ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "FSSSignalDiscovered":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");

                                    var source = EventParsing.SignalSource(data);
                                    source.systemAddress = systemAddress;

                                    source.spawningFaction = EventParsing.FactionName(data, "SpawningFaction") ?? Superpower.None.localizedName; // the minor faction, if relevant
                                    source.SpawningPower = Power.FromEDName( JsonParsing.getString( data, "SpawningPower" ) ); // the Powerplay power, if relevant
                                    source.OpposingPower = Power.FromEDName( JsonParsing.getString( data, "OpposingPower" ) ); // the opposing Powerplay power, if relevant

                                    var secondsRemaining = JsonParsing.getOptionalDecimal(data, "TimeRemaining"); // remaining lifetime in seconds, if relevant
                                    source.expiry = secondsRemaining is null ? (DateTime?)null : timestamp.AddSeconds((double)(secondsRemaining));

                                    var spawningstate = JsonParsing.getString(data, "SpawningState");
                                    var normalizedSpawningState = spawningstate?.Replace("$FactionState_", "")?.Replace("_desc;", "");
                                    source.spawningState = FactionState.FromEDName(normalizedSpawningState) ?? new FactionState();
                                    source.spawningState.fallbackLocalizedName = JsonParsing.getString(data, "SpawningState_Localised");

                                    source.threatLevel = JsonParsing.getOptionalInt(data, "ThreatLevel") ?? 0;
                                    
                                    events.Add(new SignalDetectedEvent(timestamp, systemAddress, source) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MaterialCollected":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int)(long)val;
                                    events.Add(new MaterialCollectedEvent(timestamp, material, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MaterialDiscarded":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int)(long)val;
                                    events.Add(new MaterialDiscardedEvent(timestamp, material, amount) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MaterialDiscovered":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    events.Add(new MaterialDiscoveredEvent(timestamp, material) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "MultiSellExplorationData":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var systems = new List<string>();
                                    data.TryGetValue("Discovered", out var val);
                                    var discovered = (List<object>)val;
                                    foreach (var discoveredSystem in discovered.Cast<IDictionary<string, object>>() )
                                    {
                                        var system = JsonParsing.getString(discoveredSystem, "SystemName");
                                        if (!string.IsNullOrEmpty(system))
                                        {
                                            systems.Add(system);
                                        }
                                    }
                                    data.TryGetValue("BaseValue", out val);
                                    decimal reward = (long)val;
                                    data.TryGetValue("Bonus", out val);
                                    decimal bonus = (long)val;
                                    data.TryGetValue("TotalEarnings", out val);
                                    decimal total = (long)val;
                                    events.Add(new ExplorationDataSoldEvent(timestamp, systems, reward, bonus, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NavBeaconScan":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    data.TryGetValue("NumBodies", out var val);
                                    var numbodies = (int)(long)val;
                                    events.Add(new NavBeaconScanEvent(timestamp, systemAddress, numbodies) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SAAScanComplete":
                                {
                                    var bodyName = JsonParsing.getString(data, "BodyName");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var probesUsed = JsonParsing.getInt(data, "ProbesUsed");
                                    var efficiencyTarget = JsonParsing.getInt(data, "EfficiencyTarget");

                                    // Target may be either a ring or a body
                                    var system = EDDI.Instance.CurrentStarSystem;
                                    Body body = null;

                                    if (system != null && bodyName.EndsWith(" Ring"))
                                    {
                                        // We've mapped a ring. 
                                        Ring ring = null;
                                        var ringedBodies = system.bodies?.Where(b => b?.rings?.Count > 0).ToList();
                                        foreach (var ringedBody in ringedBodies)
                                        {
                                            ring = ringedBody.rings.FirstOrDefault(r => r.name == bodyName);
                                            if (ring != null)
                                            {
                                                body = ringedBody;
                                                break;
                                            }
                                        }
                                        events.Add(new RingMappedEvent(timestamp, bodyName, ring, body, systemAddress, probesUsed, efficiencyTarget) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // Prepare updated map details to update the body in our star system
                                        body = system?.BodyWithID(bodyId);
                                        if (!(body is null))
                                        {
                                            body.scannedDateTime = body.scannedDateTime ?? timestamp;
                                            body.mappedDateTime = timestamp;
                                            body.mappedEfficiently = probesUsed <= efficiencyTarget;
                                            events.Add(new BodyMappedEvent(timestamp, bodyName, body, systemAddress, probesUsed, efficiencyTarget) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "SAASignalsFound":
                                {
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var bodyName = JsonParsing.getString(data, "BodyName");
                                    var bodyId = JsonParsing.getLong(data, "BodyID");
                                    data.TryGetValue("Signals", out var signalsVal);

                                    if (bodyName.EndsWith(" Ring"))
                                    {
                                        // This is the mining hotspots from a ring that we've mapped
                                        var hotspots = new List<CommodityAmount>();
                                        foreach (var signal in ((List<object>)signalsVal).Cast<IDictionary<string, object>>() )
                                        {
                                            var commodityEdName = JsonParsing.getString(signal, "Type");
                                            var type = CommodityDefinition.FromEDName(commodityEdName);
                                            type.fallbackLocalizedName = JsonParsing.getString(signal, "Type_Localised");
                                            var amount = JsonParsing.getInt(signal, "Count");
                                            hotspots.Add(new CommodityAmount(type, amount));
                                        }
                                        hotspots = hotspots.OrderByDescending(h => h.amount).ToList();

                                        events.Add(new RingHotspotsEvent(timestamp, systemAddress, bodyName, bodyId, hotspots) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is surface signal sources from a body that we've mapped
                                        var surfaceSignals = new List<SignalAmount>();
                                        foreach (var signal in ((List<object>)signalsVal).Cast<IDictionary<string, object>>())
                                        {
                                            var signalSource = JsonParsing.getString(signal, "Type");
                                            var source = SignalSource.FromEDName(signalSource) ?? new SignalSource();
                                            var localizedName = JsonParsing.getString(data, "Type_Localised");
                                            if (!string.IsNullOrEmpty(localizedName) && !localizedName.Contains("$"))
                                            {
                                                source.fallbackLocalizedName = localizedName;
                                            }
                                            var amount = JsonParsing.getInt(signal, "Count");
                                            surfaceSignals.Add(new SignalAmount(source, amount));
                                        }
                                        surfaceSignals = surfaceSignals.OrderByDescending(s => s.amount).ToList();
                                        events.Add(new SurfaceSignalsEvent(timestamp, "SAA", systemAddress, bodyName, bodyId, surfaceSignals) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "Scan":
                                {
                                    var name = JsonParsing.getString(data, "BodyName");
                                    var scantype = JsonParsing.getString(data, "ScanType");

                                    var systemName = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");

                                    // Belt
                                    if (name.Contains("Belt Cluster"))
                                    {
                                        // We don't do anything with belt cluster scans at this time.
                                        handled = true;
                                        break;
                                    }

                                    if (name.Contains(" Ring"))
                                    {
                                        // We don't do anything with ring scans at this time.
                                        handled = true;
                                        break;
                                    }

                                    // Common items
                                    var distanceLs = JsonParsing.getDecimal(data, "DistanceFromArrivalLS");
                                    // Need to convert radius from meters (per journal) to kilometers
                                    var radiusKm = JsonParsing.getDecimal(data, "Radius") / 1000;
                                    // Need to convert orbital period from seconds (per journal) to days
                                    var orbitalPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "OrbitalPeriod"));
                                    // Need to convert rotation period from seconds (per journal) to days
                                    var rotationPeriodDays = ConstantConverters.seconds2days(JsonParsing.getOptionalDecimal(data, "RotationPeriod"));
                                    // Need to convert meters to light seconds
                                    var semimajoraxisLs = ConstantConverters.meters2ls(JsonParsing.getOptionalDecimal(data, "SemiMajorAxis"));
                                    var eccentricity = JsonParsing.getOptionalDecimal(data, "Eccentricity");
                                    var orbitalinclinationDegrees = JsonParsing.getOptionalDecimal(data, "OrbitalInclination");
                                    var periapsisDegrees = JsonParsing.getOptionalDecimal(data, "Periapsis");
                                    var axialTiltDegrees = JsonParsing.getOptionalDecimal(data, "AxialTilt");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");
                                    var temperatureKelvin = JsonParsing.getOptionalDecimal(data, "SurfaceTemperature");

                                    // Parent body types and IDs
                                    data.TryGetValue("Parents", out var parentsVal);
                                    var parents = new List<IDictionary<string, object>>();
                                    if (parentsVal != null)
                                    {
                                        foreach (var parent in ((List<object>)parentsVal).Cast<IDictionary<string, object>>() )
                                        {
                                            parents.Add(parent);
                                        }
                                    }

                                    // Scan status
                                    var alreadydiscovered = scantype == "NavBeaconDetail" ? true : JsonParsing.getOptionalBool(data, "WasDiscovered");
                                    var alreadymapped = JsonParsing.getOptionalBool(data, "WasMapped");

                                    // Rings
                                    data.TryGetValue("Rings", out var val);
                                    var ringsData = (List<object>)val;
                                    var rings = new List<Ring>();
                                    if (ringsData != null)
                                    {
                                        foreach (var ringData in ringsData.Cast<IDictionary<string, object>>() )
                                        {
                                            var ringName = JsonParsing.getString(ringData, "Name");
                                            var ringComposition = RingComposition.FromEDName(JsonParsing.getString(ringData, "RingClass"));
                                            var ringMassMegaTons = JsonParsing.getDecimal(ringData, "MassMT");
                                            var ringInnerRadiusKm = JsonParsing.getDecimal(ringData, "InnerRad") / 1000;
                                            var ringOuterRadiusKm = JsonParsing.getDecimal(ringData, "OuterRad") / 1000;

                                            rings.Add(new Ring(ringName, ringComposition, ringMassMegaTons, ringInnerRadiusKm, ringOuterRadiusKm));
                                        }
                                    }

                                    if (data.ContainsKey("StarType"))
                                    {
                                        // Star
                                        var stellarclass = JsonParsing.getString(data, "StarType");
                                        var stellarsubclass = JsonParsing.getOptionalInt(data, "Subclass");
                                        var stellarMass = JsonParsing.getDecimal(data, "StellarMass");
                                        var absoluteMagnitude = JsonParsing.getDecimal(data, "AbsoluteMagnitude");
                                        var luminosityClass = JsonParsing.getString(data, "Luminosity");
                                        data.TryGetValue("Age_MY", out val);
                                        var ageMegaYears = (long)val;

                                        var star = new Body(name, bodyId, systemName, systemAddress, parents, distanceLs, stellarclass, stellarsubclass, stellarMass, radiusKm, absoluteMagnitude, ageMegaYears, temperatureKelvin, luminosityClass, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, alreadydiscovered, alreadymapped)
                                        {
                                            scannedDateTime = (DateTime?)timestamp
                                        };

                                        events.Add(new StarScannedEvent(timestamp, scantype, star) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (data.ContainsKey("PlanetClass"))
                                    {
                                        // Body
                                       var tidallyLocked = JsonParsing.getOptionalBool(data, "TidalLock") ?? false;

                                        var planetClass = PlanetClass.FromEDName(JsonParsing.getString(data, "PlanetClass")) ?? PlanetClass.None;
                                        var earthMass = JsonParsing.getOptionalDecimal(data, "MassEM");

                                        // MKW: Gravity in the Journal is in m/s; must convert it to G
                                        var gravity = ConstantConverters.ms2g(JsonParsing.getDecimal(data, "SurfaceGravity"));

                                        var pressureAtm = ConstantConverters.pascals2atm(JsonParsing.getOptionalDecimal(data, "SurfacePressure"));

                                        var landable = JsonParsing.getOptionalBool(data, "Landable") ?? false;

                                        var reserveLevel = ReserveLevel.FromEDName(JsonParsing.getString(data, "ReserveLevel"));

                                        // The "Atmosphere" is most accurately described through the "AtmosphereType" and "AtmosphereComposition" 
                                        // properties, so we use them in preference to "Atmosphere"

                                        // Gas giants may receive an empty string in place of an atmosphere class string. Fix it, since gas giants definitely have atmospheres. 
                                        var atmosphereClass = planetClass.invariantName.Contains("gas giant") && JsonParsing.getString(data, "AtmosphereType") == string.Empty
                                            ? AtmosphereClass.FromEDName("GasGiant")
                                            : AtmosphereClass.FromEDName(JsonParsing.getString(data, "AtmosphereType")) ?? AtmosphereClass.None;

                                        data.TryGetValue("AtmosphereComposition", out val);
                                        var atmosphereCompositions = new List<AtmosphereComposition>();
                                        if (val != null)
                                        {
                                            if (val is List<object> atmosJson)
                                            {
                                                foreach (var atmoJson in atmosJson.Cast<IDictionary<string, object>>() )
                                                {
                                                    var edComposition = JsonParsing.getString(atmoJson, "Name");
                                                    var percent = JsonParsing.getOptionalDecimal(atmoJson, "Percent");
                                                    if (edComposition != null && percent != null)
                                                    {
                                                        atmosphereCompositions.Add(new AtmosphereComposition(edComposition, (decimal)percent));
                                                    }
                                                }
                                                if (atmosphereCompositions.Count > 0)
                                                {
                                                    atmosphereCompositions = atmosphereCompositions.OrderByDescending(x => x.percent).ToList();
                                                }
                                            }
                                        }

                                        data.TryGetValue("Composition", out val);
                                        var solidCompositions = new List<SolidComposition>();
                                        if (val != null)
                                        {
                                            if (val is Dictionary<string, object> bodyCompsJson)
                                            {
                                                var compositionData = (IDictionary<string, object>)val;
                                                foreach (var kv in compositionData)
                                                {
                                                    var edComposition = kv.Key;
                                                    // The journal gives solid composition as a fraction of 1. Multiply by 100 to convert to a true percentage.
                                                    var percent = ((decimal)(double)kv.Value) * 100;
                                                    if (edComposition != null)
                                                    {
                                                        solidCompositions.Add(new SolidComposition(edComposition, percent));
                                                    }
                                                }
                                                if (solidCompositions.Count > 0)
                                                {
                                                    solidCompositions = solidCompositions.OrderByDescending(x => x.percent).ToList();
                                                }
                                            }
                                        }

                                        data.TryGetValue("Materials", out val);
                                       var materials = new List<MaterialPresence>();
                                        if (val != null)
                                        {
                                            if (val is Dictionary<string, object>)
                                            {
                                                // 2.2 style
                                                var materialsData = (IDictionary<string, object>)val;
                                                foreach (var kv in materialsData)
                                                {
                                                    var material = Material.FromEDName(kv.Key);
                                                    if (material != null)
                                                    {
                                                        materials.Add(new MaterialPresence(material, JsonParsing.getDecimal("Amount", kv.Value)));
                                                    }
                                                }
                                            }
                                            else if (val is List<object> materialsJson) // 2.3 style
                                            {
                                                foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                                {
                                                    var material = Material.FromEDName((string)materialJson["Name"]);
                                                    materials.Add(new MaterialPresence(material, JsonParsing.getDecimal(materialJson, "Percent")));
                                                }
                                            }
                                        }

                                        var terraformState = TerraformState.FromEDName(JsonParsing.getString(data, "TerraformState")) ?? TerraformState.NotTerraformable;
                                        var volcanism = Volcanism.FromName(JsonParsing.getString(data, "Volcanism"));

                                        var body = new Body(name, bodyId, systemName, systemAddress, parents, distanceLs, tidallyLocked, terraformState, planetClass, atmosphereClass, atmosphereCompositions, volcanism, earthMass, radiusKm, gravity, temperatureKelvin, pressureAtm, landable, materials, solidCompositions, semimajoraxisLs, eccentricity, orbitalinclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationPeriodDays, axialTiltDegrees, rings, reserveLevel, alreadydiscovered, alreadymapped)
                                        {
                                            scannedDateTime = (DateTime?)timestamp
                                        };

                                        events.Add(new BodyScannedEvent(timestamp, scantype, body) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "Screenshot":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var filename = JsonParsing.getString(data, "Filename");
                                    data.TryGetValue("Width", out var val);
                                    var width = (int)(long)val;
                                    data.TryGetValue("Height", out val);
                                    var height = (int)(long)val;
                                    var system = JsonParsing.getString(data, "System");
                                    var body = JsonParsing.getString(data, "Body");
                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    events.Add(new ScreenshotEvent(timestamp, filename, width, height, system, body, longitude, latitude) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellExplorationData":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    data.TryGetValue("Systems", out var val);
                                    var systems = ((List<object>)val).Cast<string>().ToList();
                                    data.TryGetValue("Discovered", out val);
                                    var firsts = ((List<object>)val).Cast<string>().ToList();
                                    data.TryGetValue("BaseValue", out val);
                                    decimal reward = (long)val;
                                    data.TryGetValue("Bonus", out val);
                                    decimal bonus = (long)val;
                                    data.TryGetValue("TotalEarnings", out val);
                                    decimal total = (long)val;
                                    events.Add(new ExplorationDataSoldEvent(timestamp, systems, reward, bonus, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Trade (and Mining) Events

                            case "AsteroidCracked":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var bodyName = JsonParsing.getString(data, "Body");
                                    events.Add(new AsteroidCrackedEvent(timestamp, bodyName) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyTradeData":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    events.Add(new TradeDataPurchasedEvent(timestamp, system, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CollectCargo":
                                {
                                    var commodityName = JsonParsing.getString(data, "Type");
                                    var commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    var missionid = JsonParsing.getOptionalULong(data, "MissionID");
                                    var stolen = JsonParsing.getBool(data, "Stolen");
                                    events.Add(new CommodityCollectedEvent(timestamp, commodity, missionid, stolen) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EjectCargo":
                                {
                                    var commodityName = JsonParsing.getString(data, "Type");
                                    var commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    var missionid = JsonParsing.getOptionalLong(data, "MissionID");
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int)(long)val;
                                    var abandoned = JsonParsing.getBool(data, "Abandoned");
                                    events.Add(new CommodityEjectedEvent(timestamp, commodity, amount, missionid, abandoned) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MarketBuy":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var commodityName = JsonParsing.getString(data, "Type");
                                    var commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    var amount = JsonParsing.getInt(data, "Count");
                                    var price = JsonParsing.getInt(data, "BuyPrice");
                                    events.Add(new CommodityPurchasedEvent(timestamp, marketId, commodity, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MarketSell":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var commodityName = JsonParsing.getString(data, "Type");
                                    var commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    var amount = JsonParsing.getInt(data, "Count");
                                    var sellPrice = JsonParsing.getLong(data, "SellPrice");

                                    var buyPrice = JsonParsing.getLong(data, "AvgPricePaid");
                                    // We don't care about buy price, we care about profit per unit
                                    var profit = sellPrice - buyPrice;

                                    var illegal = JsonParsing.getOptionalBool(data, "IllegalGoods") ?? false;
                                    var stolen = JsonParsing.getOptionalBool(data, "StolenGoods") ?? false;
                                    var blackmarket = JsonParsing.getOptionalBool(data, "BlackMarket") ?? false;

                                    events.Add(new CommoditySoldEvent(timestamp, marketId, commodity, amount, sellPrice, profit, illegal, stolen, blackmarket) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MiningRefined":
                                {
                                    var commodityName = JsonParsing.getString(data, "Type");
                                    var commodity = CommodityDefinition.FromEDName(commodityName);
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    events.Add(new CommodityRefinedEvent(timestamp, commodity) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Station Services Events

                            case "BuyAmmo":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;
                                    events.Add(new ShipRestockedEvent(timestamp, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyDrones":
                                {
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int)(long)val;
                                    data.TryGetValue("BuyPrice", out val);
                                    var price = (int)(long)val;
                                    events.Add(new LimpetPurchasedEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;                                
                            case "CargoDepot":
                                {
                                    var missionid = JsonParsing.getULong(data, "MissionID");
                                    var updatetype = JsonParsing.getString(data, "UpdateType");
                                    var startmarketid = JsonParsing.getLong(data, "StartMarketID");
                                    var endmarketid = JsonParsing.getLong(data, "EndMarketID");
                                    var collected = JsonParsing.getInt(data, "ItemsCollected");
                                    var delivered = JsonParsing.getInt(data, "ItemsDelivered");
                                    var totaltodeliver = JsonParsing.getInt(data, "TotalItemsToDeliver");

                                    // Not available in 'WingUpdate'
                                    var commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "CargoType"));
                                    var amount = JsonParsing.getOptionalInt(data, "Count");

                                    // The Progress value represents pending progress for goods in transit: (ItemsCollected-ItemsDelivered)/TotalItemsToDeliver

                                    events.Add(new CargoDepotEvent(timestamp, missionid, updatetype, commodity, amount, startmarketid, endmarketid, collected, delivered, totaltodeliver) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoal":
                                {
                                    // There may be multiple goals in each event.
                                    data.TryGetValue("CurrentGoals", out var goalsVal);
                                    var goalsJson = JsonConvert.SerializeObject(goalsVal);
                                    var goals = JsonConvert.DeserializeObject<List<CommunityGoal>>(goalsJson);
                                    events.Add(new CommunityGoalsEvent(timestamp, goals) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoalDiscard":
                                {
                                    var cgid = JsonParsing.getULong(data, "CGID");
                                    events.Add(new MissionAbandonedEvent(timestamp, cgid, "MISSION_CommunityGoal", 0) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoalJoin":
                                {
                                    var cgid = JsonParsing.getULong(data, "CGID");
                                    var name = JsonParsing.getString(data, "Name");
                                    var system = JsonParsing.getString(data, "System");

                                    var mission = new Mission(cgid, "MISSION_CommunityGoal", null, MissionStatus.Active)
                                    {
                                        localisedname = name,
                                        destinationsystem = system,
                                        originsystem = system,
                                        communal = true
                                    };

                                    events.Add(new MissionAcceptedEvent(timestamp, mission) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommunityGoalReward":
                                {
                                    var cgid = JsonParsing.getULong(data, "CGID");
                                    var name = JsonParsing.getString(data, "Name");
                                    var system = JsonParsing.getString(data, "System");
                                    data.TryGetValue("Reward", out var val);
                                    var reward = (val == null ? 0 : (long)val);

                                    events.Add(new MissionCompletedEvent(timestamp, cgid, "MISSION_CommunityGoal", name, null, null, null, true, reward, null, null, null, null, null, 0) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewAssign":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "Name");
                                    var crewid = JsonParsing.getLong(data, "CrewID");
                                    var role = EventParsing.CrewRole(data, "Role");
                                    events.Add(new CrewAssignedEvent(timestamp, name, crewid, role) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewFire":
                                {
                                    var name = JsonParsing.getString(data, "Name");
                                    var crewid = JsonParsing.getLong(data, "CrewID");
                                    events.Add(new CrewFiredEvent(timestamp, name, crewid) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewHire":
                                {
                                    var name = JsonParsing.getString(data, "Name");
                                    var crewid = JsonParsing.getLong(data, "CrewID");
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var price = JsonParsing.getLong(data, "Cost");
                                    var rating = CombatRating.FromRank(JsonParsing.getInt(data, "CombatRank"));
                                    events.Add(new CrewHiredEvent(timestamp, name, crewid, faction, price, rating) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EngineerContribution":
                                {
                                    var name = JsonParsing.getString(data, "Engineer");
                                    var engineerId = JsonParsing.getLong(data, "EngineerID");
                                    var engineer = Engineer.FromNameOrId(name, engineerId);

                                    var contributionType = JsonParsing.getString(data, "Type"); // (Commodity, materials, Credits, Bond, Bounty)
                                    var amount = JsonParsing.getInt(data, "Quantity");
                                    var total = JsonParsing.getInt(data, "TotalQuantity");
                                    switch (contributionType)
                                    {
                                        case "Commodity":
                                            {
                                                var edname = JsonParsing.getString(data, "Commodity");
                                                var commodityDef = CommodityDefinition.FromEDName( edname );
                                                var commodity = new CommodityAmount(commodityDef, amount);
                                                events.Add(new EngineerContributedEvent(timestamp, engineer, contributionType, amount, total, commodity, null) { raw = line, fromLoad = fromLogLoad });
                                            }
                                            break;
                                        case "Materials":
                                            {
                                                var edname = JsonParsing.getString(data, "Material");
                                                var materialDef = Material.FromEDName( edname );
                                                var material = new MaterialAmount(materialDef, amount);
                                                events.Add(new EngineerContributedEvent(timestamp, engineer, contributionType, amount, total, null, material) { raw = line, fromLoad = fromLogLoad });
                                            }
                                            break;
                                        case "Credits":
                                        case "Bond":
                                        case "Bounty":
                                            {
                                                // We don't currently handle these types.
                                            }
                                            break;
                                    }
                                }
                                handled = true;
                                break;
                            case "EngineerCraft":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var engineer = JsonParsing.getString(data, "Engineer");
                                    var engineerId = JsonParsing.getLong(data, "EngineerID");
                                    var blueprintpEdName = JsonParsing.getString(data, "BlueprintName");
                                    var blueprintId = JsonParsing.getLong(data, "BlueprintID");

                                    data.TryGetValue("Level", out var val);
                                    var level = (int)(long)val;

                                    var quality = JsonParsing.getOptionalDecimal(data, "Quality");
                                    var experimentalEffect = JsonParsing.getString(data, "ApplyExperimentalEffect");

                                    var slot = JsonParsing.getString( data, "Slot" );
                                    var module = Module.FromEDName( JsonParsing.getString( data, "Module" ) );
                                    
                                    var commodities = new List<CommodityAmount>();
                                    var materials = new List<MaterialAmount>();
                                    if (data.TryGetValue("Ingredients", out val))
                                    {
                                        // 2.2 style
                                        if (val is Dictionary<string, object> usedData)
                                        {
                                            foreach (var used in usedData)
                                            {
                                                // Used could be a material or a commodity
                                                var commodity = CommodityDefinition.FromEDName(used.Key);
                                                if (commodity.Category != null)
                                                {
                                                    // This is a real commodity
                                                    commodities.Add(new CommodityAmount(commodity, (int)(long)used.Value));
                                                }
                                                else
                                                {
                                                    // Probably a material then
                                                    var material = Material.FromEDName(used.Key);
                                                    materials.Add(new MaterialAmount(material, (int)(long)used.Value));
                                                }
                                            }
                                        }
                                        else if (val is List<object> materialsJson) // 2.3 style
                                        {
                                            foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                            {
                                                var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                                materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                            }
                                        }
                                    }
                                    events.Add(new ModificationCraftedEvent(timestamp, engineer, engineerId, blueprintpEdName, blueprintId, level, quality, experimentalEffect, materials, commodities, slot, module) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EngineerProgress":
                                {
                                    data.TryGetValue("Engineers", out var val);
                                    if (val != null)
                                    {
                                        // This is a startup entry. 
                                        // Update engineer progress / status data
                                        var engineers = (List<object>)val;
                                        foreach (var engineerData in engineers.Cast<IDictionary<string, object>>())
                                        {
                                            var engineer = EventParsing.Engineer(engineerData);
                                            if (!string.IsNullOrEmpty(engineer?.name))
                                            {
                                                Engineer.AddOrUpdate(engineer);
                                            }
                                        }
                                        // Generate an event to pass the data
                                        events.Add(new EngineerProgressedEvent(timestamp, null, null) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is a progress entry.
                                        var engineer = EventParsing.Engineer(data);
                                        var lastEngineer = Engineer.FromNameOrId(engineer.name, engineer.id).Copy();
                                        Engineer.AddOrUpdate(engineer);
                                        if (engineer.stage != null && engineer.stage != lastEngineer?.stage)
                                        {
                                            events.Add(new EngineerProgressedEvent(timestamp, engineer, "Stage") { raw = line, fromLoad = fromLogLoad });
                                        }
                                        if (engineer.rank != null && engineer.rank != lastEngineer?.rank)
                                        {
                                            events.Add(new EngineerProgressedEvent(timestamp, engineer, "Rank") { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "FetchRemoteModule":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    var module = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));
                                    data.TryGetValue("TransferCost", out val);
                                    var transferCost = (long)val;
                                    var transferTime = JsonParsing.getOptionalLong(data, "TransferTime");

                                    // Probably not useful. We'll get these but we won't tell the end user about them
                                    data.TryGetValue("StorageSlot", out val);
                                    var storageSlot = (int)(long)val;
                                    data.TryGetValue("ServerId", out val);
                                    var serverId = (long)val;

                                    events.Add(new ModuleTransferEvent(timestamp, ship, shipId, storageSlot, serverId, module, transferCost, transferTime) { raw = line, fromLoad = fromLogLoad });

                                    // Generate a secondary event when the module is arriving

                                    if (transferTime.HasValue)
                                    {
                                        Task.Run( async () =>
                                        {
                                            // Include the station and system at which the transfer will arrive
                                            var arrivalStation = EDDI.Instance.CurrentStation?.name ?? string.Empty;
                                            var arrivalSystem = EDDI.Instance.CurrentStarSystem?.systemname ?? string.Empty;
                                            await Task.Delay( (int)transferTime * 1000 ).ConfigureAwait( true );
                                            EDDI.Instance.enqueueEvent( new ModuleArrivedEvent( DateTime.UtcNow, ship, shipId, storageSlot, serverId, module, transferCost, transferTime, arrivalSystem, arrivalStation ) { fromLoad = fromLogLoad } );
                                        } );
                                    }
                                }
                                handled = true;
                                break;
                            case "Market":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var station = JsonParsing.getString(data, "StationName_Localised") ?? JsonParsing.getString(data, "StationName");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    if (MarketInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new MarketEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "MassModuleStore":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    data.TryGetValue("Items", out val);
                                    var items = (List<object>)val;

                                    var slots = new List<string>();
                                    var modules = new List<Module>();

                                    if (items != null)
                                    {
                                        foreach (var item in items.Cast<IDictionary<string, object>>() )
                                        {
                                            var slot = JsonParsing.getString(item, "Slot");
                                            slots.Add(slot);

                                            var module = Module.FromEDName(JsonParsing.getString(item, "Name"));
                                            module.hot = JsonParsing.getBool(item, "Hot");
                                            var engineerModifications = JsonParsing.getString(item, "EngineerModifications");
                                            module.modified = engineerModifications != null;
                                            module.engineerlevel = JsonParsing.getOptionalInt(item, "Level") ?? 0;
                                            module.engineerquality = JsonParsing.getOptionalDecimal(item, "Quality") ?? 0;
                                            module.engineermodification = Blueprint.FromEDNameAndGrade( engineerModifications, Convert.ToInt32(Math.Floor(module.engineerquality)) );
                                            modules.Add(module);
                                        }
                                    }

                                    events.Add(new ModulesStoredEvent(timestamp, ship, shipId, slots, modules, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MaterialTrade":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var traderType = JsonParsing.getString(data, "TraderType");

                                    data.TryGetValue("Paid", out var val);
                                    var paid = (Dictionary<string, object>)val;

                                    var materialEdName = JsonParsing.getString(paid, "Material");
                                    var materialPaid = Material.FromEDName(materialEdName);
                                    var materialPaidQty = JsonParsing.getInt(paid, "Quantity");

                                    if (materialPaid == null)
                                    {
                                        Logging.Info("Unknown material " + materialEdName);
                                        Logging.Info("Unknown material " + materialEdName, JsonConvert.SerializeObject(paid));
                                    }

                                    data.TryGetValue("Received", out val);
                                    var received = (Dictionary<string, object>)val;

                                    var materialReceived = Material.FromEDName(JsonParsing.getString(received, "Material"));
                                    var materialReceivedQty = JsonParsing.getInt(received, "Quantity");

                                    if (materialReceived == null)
                                    {
                                        Logging.Info("Unknown material " + materialEdName, JsonConvert.SerializeObject(received));
                                    }

                                    events.Add(new MaterialTradedEvent(timestamp, marketId, traderType, materialPaid, materialPaidQty, materialReceived, materialReceivedQty) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;

                                    break;
                                }
                            case "MissionAbandoned":
                                {
                                    var missionid = JsonParsing.getULong(data, "MissionID");
                                    var name = JsonParsing.getString(data, "Name");
                                    var fine = JsonParsing.getOptionalLong(data, "Fine") ?? 0;
                                    events.Add(new MissionAbandonedEvent(timestamp, missionid, name, fine) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionAccepted":
                                {
                                    var missionid = JsonParsing.getULong( data, "MissionID" );
                                    data.TryGetValue("Expiry", out var val);
                                    var expiry = (DateTime?)val;
                                    var name = JsonParsing.getString(data, "Name");
                                    var localisedname = JsonParsing.getString(data, "LocalisedName");
                                    if (!string.IsNullOrEmpty(localisedname))
                                    {
                                        localisedname = Regex.Replace(localisedname, @"<.*?>", ""); // Mission localized names may have embedded HTML tags. If so then remove them.
                                    }
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var reward = JsonParsing.getOptionalInt(data, "Reward");
                                    var wing = JsonParsing.getBool(data, "Wing");

                                    if (name == "MISSION_genericPermit1")
                                    {
                                        // This is a permit mission where the permit is granted immediately once it is accepted.
                                        // There are no other mission related events generated from this (no mission completion event).
                                        events.Add(new PermitAcquiredEvent(timestamp, faction) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // Missions with destinations
                                        var destinationsystem = JsonParsing.getString(data, "DestinationSystem");
                                        var destinationstation = JsonParsing.getString(data, "DestinationStation");
                                        var destinationsettlement = JsonParsing.getString(data, "DestinationSettlement");

                                        // Missions with commodities (which may include on-foot micro-resources)
                                        var c = JsonParsing.getString(data, "Commodity");
                                        var fallbackC = JsonParsing.getString(data, "Commodity_Localised");
                                        CommodityDefinition commodity = null;
                                        MicroResource microResource = null;

                                        if (!string.IsNullOrEmpty(c))
                                        {
                                            if (MicroResource.EDNameExists(c))
                                            {
                                                // This is an on-foot micro-resource
                                                microResource = MicroResource.FromEDName(c);
                                                microResource.fallbackLocalizedName = fallbackC;
                                            }
                                            else
                                            {
                                                // This is (probably) a traditional ship commodity
                                                commodity = CommodityDefinition.FromEDName(c);
                                                commodity.fallbackLocalizedName = fallbackC;
                                            }
                                        }
                                        data.TryGetValue("Count", out val);
                                        var amount = (int?)(long?)val;

                                        // Missions with targets
                                        var target = JsonParsing.getString(data, "Target");
                                        var targettype = JsonParsing.getString(data, "TargetType");
                                        var targetfaction = EventParsing.FactionName(data, "TargetFaction");
                                        data.TryGetValue("KillCount", out val);
                                        if (val != null)
                                        {
                                            amount = (int?)(long?)val;
                                        }

                                        // Missions with passengers
                                        var passengercount = JsonParsing.getOptionalInt(data, "PassengerCount");
                                        var passengertype = JsonParsing.getString(data, "PassengerType");
                                        var passengerswanted = JsonParsing.getOptionalBool(data, "PassengerWanted");
                                        var passengervips = JsonParsing.getOptionalBool(data, "PassengerVIPs");
                                        data.TryGetValue("PassengerCount", out val);
                                        if (val != null)
                                        {
                                            amount = (int?)(long?)val;
                                        }

                                        // Impact on influence and reputation
                                        var influence = JsonParsing.getString(data, "Influence");
                                        var reputation = JsonParsing.getString(data, "Reputation");

                                        var mission = new Mission(missionid, name, expiry, MissionStatus.Active)
                                        {
                                            // Common parameters
                                            localisedname = localisedname,
                                            amount = amount ?? 0,
                                            influence = influence,
                                            reputation = reputation,
                                            reward = reward ?? 0,
                                            communal = false,

                                            // Get the minor faction stuff
                                            faction = faction,

                                            // Set mission origin to to the current system & station
                                            originsystem = EDDI.Instance.CurrentStarSystem?.systemname,
                                            originstation = EDDI.Instance.CurrentStation?.name,

                                            // Missions with engineering rewards
                                            CommodityDefinition = commodity,
                                            MicroResourceDefinition = microResource,

                                            // Missions with targets
                                            targetTypeEDName = targettype?.Split('_')?
                                                .ElementAtOrDefault(2)?.Replace(";", ""),
                                            target = target,
                                            targetfaction = targetfaction,

                                            // Missions with passengers
                                            passengertypeEDName = passengertype,
                                            passengervips = passengervips,
                                            passengerwanted = passengerswanted
                                        };

                                        // Missions with multiple destinations
                                        if ( destinationsystem != null && destinationsystem.Contains( "$MISSIONUTIL_MULTIPLE" ) )
                                        {
                                            // If 'chained' mission, get the destination systems
                                            var destinationSystems = destinationsystem
                                                .Replace("$MISSIONUTIL_MULTIPLE_INNER_SEPARATOR;", "#")
                                                .Replace("$MISSIONUTIL_MULTIPLE_FINAL_SEPARATOR;", "#")
                                                .Split('#');

                                            var starSystems = EDDI.Instance.DataProvider.GetOrFetchSystemWaypointsAsync(destinationSystems).GetAwaiter().GetResult();
                                            foreach ( var dest in starSystems )
                                            {
                                                if ( !string.IsNullOrEmpty( dest.systemName ) &&
                                                     dest.x is decimal sx &&
                                                     dest.y is decimal sy &&
                                                     dest.z is decimal sz )
                                                {
                                                    dest.missionids.Add( mission.missionid );
                                                    mission.destinationsystems.Add( dest );
                                                }
                                            }

                                            // Load the first destination system.
                                            mission.destinationsystem = mission.destinationsystems.ElementAtOrDefault( 0 ).systemName;
                                        }
                                        else
                                        {
                                            // Populate destination system and station, depending on mission type
                                            if ( mission.tagsList.Contains( MissionType.Altruism ) )
                                            {
                                                mission.destinationsystem = mission.originsystem;
                                                mission.destinationstation = mission.originstation;
                                            }
                                            else
                                            {
                                                mission.destinationsystem = destinationsystem;
                                                mission.destinationstation = destinationstation ?? destinationsettlement;
                                            }
                                        }

                                        events.Add(new MissionAcceptedEvent(timestamp, mission) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "MissionCompleted":
                                {
                                    var missionid = JsonParsing.getULong(data, "MissionID");
                                    var name = JsonParsing.getString(data, "Name");
                                    var reward = JsonParsing.getOptionalLong( data, "Reward" ) ?? 0;
                                    var donation = JsonParsing.getOptionalLong(data, "Donated") ?? 0;
                                    var faction = EventParsing.FactionName(data, "Faction");

                                    // Missions with commodities (which may include on-foot micro-resources)
                                    var c = JsonParsing.getString(data, "Commodity");
                                    var fallbackC = JsonParsing.getString(data, "Commodity_Localised");
                                    CommodityDefinition commodity = null;
                                    MicroResource microResource = null;

                                    if (!string.IsNullOrEmpty(c))
                                    {
                                        if (MicroResource.EDNameExists(c))
                                        {
                                            // This is an on-foot micro-resource
                                            microResource = MicroResource.FromEDName(c);
                                            microResource.fallbackLocalizedName = fallbackC;
                                        }
                                        else
                                        {
                                            // This is (probably) a traditional ship commodity
                                            commodity = CommodityDefinition.FromEDName(c);
                                            commodity.fallbackLocalizedName = fallbackC;
                                        }
                                    }
                                    var amount = JsonParsing.getOptionalInt(data, "Count");

                                    var permitsAwarded = new List<string>();
                                    data.TryGetValue("PermitsAwarded", out var val);
                                    var permitsAwardedData = (List<object>)val;
                                    if (permitsAwardedData != null)
                                    {
                                        foreach (var permitAwardedData in permitsAwardedData.Cast<IDictionary<string, object>>() )
                                        {
                                            var permitAwarded = JsonParsing.getString(permitAwardedData, "Name");
                                            permitsAwarded.Add(permitAwarded);
                                        }
                                    }

                                    var commodityrewards = new List<CommodityAmount>();
                                    data.TryGetValue("CommodityReward", out val);
                                    var commodityRewardsData = (List<object>)val;
                                    if (commodityRewardsData != null)
                                    {
                                        foreach (var commodityRewardData in commodityRewardsData.Cast<IDictionary<string, object>>() )
                                        {
                                            var rewardCommodity = CommodityDefinition.FromEDName(JsonParsing.getString(commodityRewardData, "Name"));
                                            var count = JsonParsing.getOptionalInt(commodityRewardData, "Count") ?? 0;
                                            if ( rewardCommodity != null )
                                            {
                                                commodityrewards.Add( new CommodityAmount( rewardCommodity, count ) );
                                            }
                                        }
                                    }

                                    var materialsrewards = new List<MaterialAmount>();
                                    var microResourceRewards = new List<MicroResourceAmount>();
                                    data.TryGetValue("MaterialsReward", out val);
                                    var materialsRewardsData = (List<object>)val;
                                    if (materialsRewardsData != null)
                                    {
                                        foreach (var materialsRewardData in materialsRewardsData.Cast<IDictionary<string, object>>() )
                                        {
                                            var m = JsonParsing.getString(materialsRewardData, "Name");
                                            var fallbackM = JsonParsing.getString(materialsRewardData, "Name_Localised");
                                            materialsRewardData.TryGetValue("Count", out val);
                                            var count = (int)(long)val;

                                            if (!string.IsNullOrEmpty(m))
                                            {
                                                if (MicroResource.EDNameExists(m))
                                                {
                                                    // This is an on-foot micro-resource
                                                    microResourceRewards.Add(new MicroResourceAmount(m, null, count, null, fallbackM));
                                                }
                                                else
                                                {
                                                    // This is (probably) a traditional ship material
                                                    var rewardMaterial = Material.FromEDName(m);
                                                    rewardMaterial.fallbackLocalizedName = fallbackM;
                                                    materialsrewards.Add(new MaterialAmount(rewardMaterial, count));
                                                }
                                            }
                                        }
                                    }

                                    var missionFactionEffects = new List<MissionFactionEffect>();
                                    data.TryGetValue("FactionEffects", out val);
                                    var missionFactionEffectsData = (List<object>)val;
                                    if (missionFactionEffectsData != null)
                                    {
                                        foreach (var missionFactionEffectData in missionFactionEffectsData.Cast<IDictionary<string, object>>() )
                                        {
                                            var effectFaction = JsonParsing.getString(missionFactionEffectData, "Faction");
                                            var reputationPlusses = JsonParsing.getString(missionFactionEffectData, "Reputation");

                                            var effects = new List<MissionEffect>();
                                            missionFactionEffectData.TryGetValue("Effects", out val);
                                            var effectsData = (List<object>)val;
                                            if (effectsData != null)
                                            {
                                                foreach (var effectData in effectsData.Cast<IDictionary<string, object>>() )
                                                {
                                                    var edEffect = JsonParsing.getString(effectData, "Effect");
                                                    var localizedEffect = JsonParsing.getString(effectData, "Effect_Localised");
                                                    effects.Add(new MissionEffect(edEffect, localizedEffect));
                                                }
                                            }

                                            var influences = new List<MissionInfluence>();
                                            missionFactionEffectData.TryGetValue("Influence", out val);
                                            var influencesData = (List<object>)val;
                                            if (influencesData != null)
                                            {
                                                foreach (var influenceData in influencesData.Cast<IDictionary<string, object>>() )
                                                {
                                                    var influencedSystemAddress = JsonParsing.getULong(influenceData, "SystemAddress");
                                                    var influencePlusses = JsonParsing.getString(influenceData, "Influence");
                                                    influences.Add(new MissionInfluence(influencedSystemAddress, influencePlusses));
                                                }
                                            }
                                            
                                            missionFactionEffects.Add(new MissionFactionEffect(effectFaction, effects, influences, reputationPlusses));
                                        }
                                    }

                                    events.Add(new MissionCompletedEvent(timestamp, missionid, name, faction, microResource, commodity, amount, false, reward, permitsAwarded, commodityrewards, materialsrewards, microResourceRewards, missionFactionEffects, donation) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionFailed":
                                {
                                    var missionid = JsonParsing.getULong(data, "MissionID");
                                    var name = JsonParsing.getString(data, "Name");
                                    var fine = JsonParsing.getOptionalLong(data, "Fine") ?? 0;
                                    events.Add(new MissionFailedEvent(timestamp, missionid, name, fine) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "MissionRedirected":
                                {
                                    var missionid = JsonParsing.getULong(data, "MissionID");
                                    var name = JsonParsing.getString(data, "Name");
                                    var newdestinationstation = JsonParsing.getString(data, "NewDestinationStation");
                                    var olddestinationstation = JsonParsing.getString(data, "OldDestinationStation");
                                    var newdestinationsystem = JsonParsing.getString(data, "NewDestinationSystem");
                                    var olddestinationsystem = JsonParsing.getString(data, "OldDestinationSystem");
                                    events.Add(new MissionRedirectedEvent(timestamp, missionid, name, newdestinationstation, olddestinationstation, newdestinationsystem, olddestinationsystem) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleBuy":
                                handled = ModulePurchasedEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleBuyAndStore":
                                handled = ModulePurchasedToStorageEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ModuleRetrieve":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    var slot = JsonParsing.getString(data, "Slot");
                                    var module = Module.FromEDName(JsonParsing.getString(data, "RetrievedItem"));
                                    module.hot = JsonParsing.getBool(data, "Hot");
                                    var engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                    module.modified = engineerModifications != null;
                                    module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
                                    module.engineermodification = Blueprint.FromEDNameAndGrade(engineerModifications, module.engineerlevel) ?? Blueprint.None;
                                    module.engineerquality = JsonParsing.getOptionalDecimal(data, "Quality") ?? 0;

                                    // Set retrieved module defaults
                                    module.price = module.value;
                                    module.enabled = true;
                                    module.priority = 1;
                                    module.health = 100;

                                    // Set module cost
                                    data.TryGetValue("Cost", out val);
                                    var cost = JsonParsing.getOptionalLong(data, "Cost");

                                    var swapoutModule = Module.FromEDName(JsonParsing.getString(data, "SwapOutItem"));

                                    events.Add(new ModuleRetrievedEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, swapoutModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleSell":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    var slot = JsonParsing.getString(data, "Slot");
                                    var module = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
                                    data.TryGetValue("SellPrice", out val);
                                    var price = (long)val;

                                    events.Add(new ModuleSoldEvent(timestamp, ship, shipId, slot, module, price, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleSellRemote":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    var module = Module.FromEDName(JsonParsing.getString(data, "SellItem"));
                                    data.TryGetValue("SellPrice", out val);
                                    var price = (long)val;

                                    // Probably not useful. We'll get these but we won't tell the end user about them
                                    data.TryGetValue("StorageSlot", out val);
                                    var storageSlot = (int)(long)val;
                                    data.TryGetValue("ServerId", out val);
                                    var serverId = (long)val;

                                    events.Add(new ModuleSoldFromStorageEvent(timestamp, ship, shipId, storageSlot, serverId, module, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleStore":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    var slot = JsonParsing.getString(data, "Slot");
                                    var module = Module.FromEDName(JsonParsing.getString(data, "StoredItem"));
                                    module.hot = JsonParsing.getBool(data, "Hot");
                                    var engineerModifications = JsonParsing.getString(data, "EngineerModifications");
                                    module.modified = engineerModifications != null;
                                    module.engineerlevel = JsonParsing.getOptionalInt(data, "Level") ?? 0;
                                    module.engineermodification = Blueprint.FromEDNameAndGrade(engineerModifications, module.engineerlevel) ?? Blueprint.None;
                                    module.engineerquality = JsonParsing.getOptionalDecimal(data, "Quality") ?? 0;

                                    data.TryGetValue("Cost", out val);
                                    var cost = JsonParsing.getOptionalLong(data, "Cost");

                                    var replacementModule = Module.FromEDName(JsonParsing.getString(data, "ReplacementItem"));
                                    if (replacementModule != null)
                                    {
                                        replacementModule.price = replacementModule.value;
                                        replacementModule.enabled = true;
                                        replacementModule.priority = 1;
                                        replacementModule.health = 100;
                                        replacementModule.modified = false;
                                    }

                                    events.Add(new ModuleStoredEvent(timestamp, ship, shipId, slot, module, cost, engineerModifications, replacementModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleSwap":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");

                                    var fromSlot = JsonParsing.getString(data, "FromSlot");
                                    var fromModule = Module.FromEDName(JsonParsing.getString(data, "FromItem"));
                                    var toSlot = JsonParsing.getString(data, "ToSlot");
                                    var toModule = Module.FromEDName(JsonParsing.getString(data, "ToItem"));

                                    events.Add(new ModuleSwappedEvent(timestamp, ship, shipId, fromSlot, fromModule, toSlot, toModule, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Outfitting":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var station = JsonParsing.getString(data, "StationName");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    if (OutfittingInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new OutfittingEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "PayBounties":
                                {
                                    data.TryGetValue("Amount", out var val);
                                    var amount = (long)val;
                                    var brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");
                                    var allBounties = JsonParsing.getOptionalBool(data, "AllFines") ?? false;
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    int shipId;
                                    var shipIdLong = JsonParsing.getLong(data, "ShipID");
                                    if (shipIdLong > 4293000000)
                                    {
                                        // This is a suit loadout ID. Use a null value since bounties associated with the commander, rather than the ship, are being paid.
                                        shipId = -1;
                                    }
                                    else
                                    {
                                        shipId = (int)shipIdLong;
                                    }

                                    events.Add(new BountyPaidEvent(timestamp, amount, brokerpercentage, allBounties, faction, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PayFines":
                                {
                                    data.TryGetValue("Amount", out var val);
                                    var amount = (long)val;
                                    var brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");
                                    var allFines = JsonParsing.getOptionalBool(data, "AllFines") ?? false;
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var shipId = 0;
                                    var shipIdLong = JsonParsing.getLong(data, "ShipID");
                                    if (shipIdLong >= 4293000000)
                                    {
                                        // This is a suit loadout ID. Use a -1 value to signal that fines associated with the commander, rather than the ship, are being paid.
                                        shipIdLong = -1;
                                    }
                                    else
                                    {
                                        shipId = (int)shipIdLong;
                                    }

                                    events.Add(new FinePaidEvent(timestamp, amount, brokerpercentage, allFines, faction, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RedeemVoucher":
                                {
                                    var type = JsonParsing.getString(data, "Type");
                                    var rewards = new List<Reward>();

                                    // Obtain list of factions
                                    data.TryGetValue("Factions", out var val);
                                    var factionsData = (List<object>)val;
                                    if (factionsData != null)
                                    {
                                        foreach (var rewardData in factionsData.Cast<IDictionary<string, object>>())
                                        {
                                            var factionName = EventParsing.FactionName(rewardData, "Faction");
                                            rewardData.TryGetValue("Amount", out val);
                                            var factionReward = (long)val;

                                            rewards.Add(new Reward(factionName, factionReward));
                                        }
                                    }
                                    else
                                    {
                                        var factionName = EventParsing.FactionName(data, "Faction");
                                        data.TryGetValue("Amount", out val);
                                        var factionReward = (long)val;

                                        rewards.Add(new Reward(factionName, factionReward));
                                    }
                                    data.TryGetValue("Amount", out val);
                                    var amount = (long)val;

                                    var brokerpercentage = JsonParsing.getOptionalDecimal(data, "BrokerPercentage");

                                    if (type == "bounty")
                                    {
                                        events.Add(new BountyRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (type == "CombatBond")
                                    {
                                        events.Add(new BondRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (type == "trade")
                                    {
                                        events.Add(new TradeVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else if (type == "codex" || type == "settlement" || type == "scannable")
                                    {
                                        events.Add(new DataVoucherRedeemedEvent(timestamp, rewards, amount, brokerpercentage) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        Logging.Warn("Unhandled voucher type " + type, line);
                                    }
                                }
                                handled = true;
                                break;
                            case "RefuelAll":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, true) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RefuelPartial":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var amount = JsonParsing.getDecimal(data, "Amount");
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    events.Add(new ShipRefuelledEvent(timestamp, "Market", price, amount, null, false) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Repair":
                                {
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;

                                    // Starting with version 3.7, the "Repair" event may contain one item or multiple items
                                    // Each item is either a description (e.g. all, wear, hull, paint) or the name of a module
                                    data.TryGetValue("Items", out var itemsVal);
                                    if (itemsVal != null)
                                    {
                                        if (itemsVal is List<object> itemEDNames)
                                        {
                                            events.Add(new ShipRepairedEvent(timestamp, itemEDNames.ConvertAll(o => o.ToString()), price) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                    else
                                    {
                                        // We have a single "item"
                                        var itemEDName = JsonParsing.getString(data, "Item");
                                        if (!string.IsNullOrEmpty(itemEDName))
                                        {
                                            events.Add(new ShipRepairedEvent(timestamp, itemEDName, price) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "RepairAll":
                                {
                                    data.TryGetValue("Cost", out var val);
                                    var price = (long)val;
                                    events.Add(new ShipRepairedEvent(timestamp, "All", price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ScientificResearch":
                                {
                                    data.TryGetValue("Name", out var val);
                                    var material = Material.FromEDName(JsonParsing.getString(data, "Name"));
                                    var amount = JsonParsing.getInt(data, "Count");
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    events.Add(new MaterialDonatedEvent(timestamp, material, amount, marketId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "SearchAndRescue":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var commodityName = JsonParsing.getString(data, "Name");
                                    var commodity = CommodityDefinition.FromEDName(JsonParsing.getString(data, "Name"));
                                    if (commodity == null)
                                    {
                                        Logging.Error("Failed to map cargo type " + commodityName + " to commodity definition", line);
                                    }
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int?)(long?)val;
                                    data.TryGetValue("Reward", out val);
                                    var reward = (val == null ? 0 : (long)val);
                                    events.Add(new SearchAndRescueEvent(timestamp, commodity, amount, reward, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellDrones":
                                {
                                    data.TryGetValue("Count", out var val);
                                    var amount = (int)(long)val;
                                    data.TryGetValue("SellPrice", out val);
                                    var price = (int)(long)val;
                                    events.Add(new LimpetSoldEvent(timestamp, amount, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SellShipOnRebuy":
                                {
                                    data.TryGetValue("SellShipId", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");
                                    data.TryGetValue("ShipPrice", out val);
                                    var price = (long)val;
                                    var system = JsonParsing.getString(data, "System");
                                    events.Add(new ShipSoldOnRebuyEvent(timestamp, ship, shipId, price, system) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SetUserShipName":
                                {
                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "Ship");
                                    var name = JsonParsing.getString(data, "UserShipName");
                                    var ident = JsonParsing.getString(data, "UserShipId");

                                    events.Add(new ShipRenamedEvent(timestamp, ship, shipId, name, ident) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Shipyard":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var station = JsonParsing.getString(data, "StationName");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    if (ShipyardInfo.TryFromFile(timestamp, system, station, marketId, out var info, out var raw))
                                    {
                                        events.Add(new ShipyardEvent(timestamp, marketId, station, system, info) { raw = raw, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "ShipyardBuy":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    // We don't have a ship ID at this point so use the ship type
                                    var ship = JsonParsing.getString(data, "ShipType");

                                    data.TryGetValue("ShipPrice", out var val);
                                    var price = (long)val;

                                    data.TryGetValue("StoreShipID", out val);
                                    var storedShipId = (val == null ? (int?)null : (int)(long)val);
                                    var storedShip = JsonParsing.getString(data, "StoreOldShip");

                                    data.TryGetValue("SellShipID", out val);
                                    var soldShipId = (val == null ? (int?)null : (int)(long)val);
                                    var soldShip = JsonParsing.getString(data, "SellOldShip");

                                    data.TryGetValue("SellPrice", out val);
                                    var soldPrice = (long?)val;
                                    events.Add(new ShipPurchasedEvent(timestamp, ship, price, soldShip, soldShipId, soldPrice, storedShip, storedShipId, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardNew":
                            case "ShipRedeemed":
                                {
                                    data.TryGetValue("NewShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");

                                    events.Add(new ShipDeliveredEvent(timestamp, ship, shipId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardSell":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("SellShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");
                                    data.TryGetValue("ShipPrice", out val);
                                    var price = (long)val;
                                    var system = JsonParsing.getString(data, "System"); // Only written when the ship is in a different star system
                                    events.Add(new ShipSoldEvent(timestamp, ship, shipId, price, system, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ShipyardTransfer":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var toMarketId = JsonParsing.getLong(data, "MarketID");
                                    var fromMarketId = JsonParsing.getLong(data, "ShipMarketID");

                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;

                                    var system = JsonParsing.getString(data, "System");
                                    var distance = JsonParsing.getDecimal(data, "Distance");
                                    var price = JsonParsing.getOptionalLong(data, "TransferPrice");
                                    var time = JsonParsing.getOptionalLong(data, "TransferTime");

                                    var ship = ConfigService.Instance.shipMonitorConfiguration?.shipyard.FirstOrDefault(s => s.LocalId == shipId);
                                    if (ship is null)
                                    {
                                        var shipEDModel = JsonParsing.getString(data, "ShipType");
                                        ship = ShipDefinitions.FromEDModel(shipEDModel);
                                        ship.LocalId = shipId;
                                    }

                                    events.Add(new ShipTransferInitiatedEvent(timestamp, ship, system, distance, price, time, fromMarketId, toMarketId) { raw = line, fromLoad = fromLogLoad });

                                    // Generate secondary event when the ship is arriving
                                    if (time.HasValue)
                                    {
                                        Task.Run( async () =>
                                        {
                                            // Include the station and system at which the transfer will arrive
                                            var arrivalStation = EDDI.Instance.CurrentStation?.name ?? string.Empty;
                                            var arrivalSystem = EDDI.Instance.CurrentStarSystem?.systemname ??
                                                                string.Empty;
                                            await Task.Delay( (int)time * 1000 ).ConfigureAwait( true );
                                            EDDI.Instance.enqueueEvent( new ShipArrivedEvent( DateTime.UtcNow, ship,
                                                arrivalSystem, distance, price, time, arrivalStation, fromMarketId,
                                                toMarketId ) { fromLoad = fromLogLoad } );
                                        } );
                                    }
                                }
                                handled = true;
                                break;
                            case "ShipyardSwap":
                                {
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("ShipID", out var val);
                                    var shipId = (int)(long)val;
                                    var ship = JsonParsing.getString(data, "ShipType");

                                    data.TryGetValue("StoreShipID", out val);
                                    var storedShipId = (val == null ? (int?)null : (int)(long)val);
                                    var storedShip = JsonParsing.getString(data, "StoreOldShip");

                                    data.TryGetValue("SellShipID", out val);
                                    var soldShipId = (val == null ? (int?)null : (int)(long)val);
                                    var soldShip = JsonParsing.getString(data, "SellOldShip");

                                    events.Add(new ShipSwappedEvent(timestamp, ship, shipId, soldShip, soldShipId, storedShip, storedShipId, marketId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "StoredModules":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var storedModules = new List<StoredModule>();

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var station = JsonParsing.getString(data, "StationName");

                                    data.TryGetValue("Items", out var val);
                                    var items = (List<object>)val;
                                    if (items != null)
                                    {
                                        foreach (var item in items.Cast<IDictionary<string, object>>() )
                                        {
                                            var name = JsonParsing.getString(item, "Name");
                                            var module = new Module(Module.FromEDName(name))
                                            {
                                                hot = JsonParsing.getOptionalBool(item, "Hot") ?? false,
                                                price = JsonParsing.getOptionalLong(item, "BuyPrice") ?? 0,
                                            };
                                            item.TryGetValue("EngineerModifications", out val);
                                            module.modificationEDName = JsonParsing.getString(item, "EngineerModifications");
                                            module.modified = !string.IsNullOrEmpty(module.modificationEDName);
                                            module.engineerlevel = module.modified ? JsonParsing.getInt(item, "Level") : 0;
                                            module.engineermodification = Blueprint.FromEDNameAndGrade(module.modificationEDName, module.engineerlevel) ?? Blueprint.None;
                                            module.engineerquality = module.modified ? JsonParsing.getDecimal(item, "Quality") : 0;

                                            var storedModule = new StoredModule
                                            {
                                                module = module,
                                                slot = JsonParsing.getInt(item, "StorageSlot"),
                                                intransit = JsonParsing.getOptionalBool(item, "InTransit") ?? false,
                                                system = JsonParsing.getString(item, "StarSystem"),
                                                marketid = JsonParsing.getOptionalLong(item, "MarketID"),
                                                transfercost = JsonParsing.getOptionalLong(item, "TransferCost"),
                                                transfertime = JsonParsing.getOptionalLong(item, "TransferTime")
                                            };
                                            storedModules.Add(storedModule);
                                        }
                                    }
                                    events.Add(new StoredModulesEvent(timestamp, marketId, station, system, storedModules) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "StoredShips":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var marketId = JsonParsing.getLong(data, "MarketID");
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var station = JsonParsing.getString(data, "StationName");

                                    var shipyard = new List<Ship>();
                                    foreach (var type in Enum.GetNames(typeof(ShipyardType)))
                                    {
                                        data.TryGetValue(type, out var val);
                                        var shipsData = (List<object>)val;
                                        if (shipsData != null)
                                        {
                                            foreach (var shipData in shipsData.Cast<IDictionary<string, object>>() )
                                            {
                                                var shipType = JsonParsing.getString(shipData, "ShipType");
                                                var ship = ShipDefinitions.FromEDModel(shipType);
                                                if (ship != null)
                                                {
                                                    ship.LocalId = JsonParsing.getInt(shipData, "ShipID");
                                                    ship.name = JsonParsing.getString(shipData, "Name");
                                                    ship.value = JsonParsing.getLong(shipData, "Value");
                                                    ship.hot = JsonParsing.getOptionalBool(shipData, "Hot") ?? false;
                                                    ship.intransit = JsonParsing.getOptionalBool(shipData, "InTransit") ?? false;
                                                    ship.transferprice = JsonParsing.getOptionalLong(shipData, "TransferPrice");
                                                    ship.transfertime = JsonParsing.getOptionalLong(shipData, "TransferTime");

                                                    var shipSystemName = JsonParsing.getString(shipData, "StarSystem");
                                                    var shipMarketID = JsonParsing.getOptionalLong( shipData, "ShipMarketID" );
                                                    var stationWaypoint = EDDI.Instance.DataProvider.GetOrFetchStationWaypointAsync(
                                                            shipSystemName ?? system, shipMarketID ?? marketId ).GetAwaiter().GetResult();
                                                    ship.StoredLocation = stationWaypoint is null 
                                                        ? null 
                                                        : new Ship.Location( stationWaypoint );
                                                    ship.distance = ship.DistanceLY( EDDI.Instance.CurrentStarSystem );
                                                    shipyard.Add(ship);
                                                }
                                            }
                                        }
                                    }
                                    events.Add(new StoredShipsEvent(timestamp, marketId, station, system, shipyard) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "TechnologyBroker":
                                {
                                    var brokerType = JsonParsing.getString(data, "BrokerType");
                                    var marketId = JsonParsing.getLong(data, "MarketID");

                                    data.TryGetValue("ItemsUnlocked", out var val);
                                    var itemsUnlocked = (List<object>)val;
                                    var items = new List<Module>();
                                    foreach (var item in itemsUnlocked)
                                    {
                                        var itemProperties = (Dictionary<string, object>)item;
                                        var moduleEdName = JsonParsing.getString(itemProperties, "Name");
                                        var module = Module.FromEDName(moduleEdName);
                                        if (module == null)
                                        {
                                            // Unknown module
                                            Logging.Info("Unknown module " + moduleEdName, JsonConvert.SerializeObject(item));
                                        }
                                        items.Add(module);
                                    }

                                    data.TryGetValue("Commodities", out val);
                                    var commodities = (List<object>)val;
                                    var Commodities = new List<CommodityAmount>();
                                    foreach (var _commodity in commodities.Cast<IDictionary<string, object>>() )
                                    {
                                        var commodityEdName = JsonParsing.getString(_commodity, "Name");
                                        var commodity = CommodityDefinition.FromEDName(commodityEdName);
                                        var count = JsonParsing.getInt(_commodity, "Count");
                                        if (commodity == null)
                                        {
                                            Logging.Info("Unknown commodity " + commodityEdName, JsonConvert.SerializeObject(_commodity));
                                            continue;
                                        }
                                        Commodities.Add(new CommodityAmount(commodity, count));
                                    }

                                    data.TryGetValue("Materials", out val);
                                    var materials = (List<object>)val;
                                    var Materials = new List<MaterialAmount>();
                                    foreach (var _material in materials.Cast<IDictionary<string, object>>() )
                                    {
                                        var materialEdName = JsonParsing.getString(_material, "Name");
                                        var material = Material.FromEDName(materialEdName);
                                        var count = JsonParsing.getInt(_material, "Count");
                                        Materials.Add(new MaterialAmount(material, count));
                                    }

                                    events.Add(new TechnologyBrokerEvent(timestamp, brokerType, marketId, items, Commodities, Materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Powerplay Events

                            case "DeliverPowerMicroResources":
                                {
                                    // { "timestamp":"2024-10-19T15:01:28Z", "event":"DeliverPowerMicroResources", "TotalCount":2, "MicroResources":[ { "Name":"powerelectronics", "Name_Localised":"Electronics Package", "Category":"Item", "Count":2 } ], "MarketID":3223182848 }
                                    if ( data.ContainsKey( "TotalCount" ) )
                                    {
                                        var marketID = JsonParsing.getLong( data, "MarketID" );
                                        var resourceAmounts = new List<MicroResourceAmount>();
                                        if ( data.TryGetValue( "MicroResources", out var val ) )
                                        {
                                            if ( val is List<object> listVal )
                                            {
                                                foreach ( var res in listVal )
                                                {
                                                    if ( res is IDictionary<string, object> microVal )
                                                    {
                                                        var microResource = EventParsing.MicroResource( microVal );
                                                        var amount = JsonParsing.getInt(microVal, "Count");
                                                        if ( microResource != null )
                                                        {
                                                            resourceAmounts.Add( new MicroResourceAmount( microResource, amount ) );
                                                        }
                                                    }
                                                }
                                                events.Add( new PowerMicroResourcesDeliveredEvent( timestamp, marketID, resourceAmounts ) { raw = line, fromLoad = fromLogLoad } );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;
                            case "HoloscreenHacked":
                                {
                                    // {"timestamp":"2024-10-22T20:40:06Z","event":"HoloscreenHacked","PowerBefore":"Aisling Duval","PowerAfter":"Yuri Grom"}

                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading
                                    
                                    var powerBefore = Power.FromEDName( JsonParsing.getString( data, "PowerBefore" ) );
                                    var powerAfter = Power.FromEDName( JsonParsing.getString( data, "PowerAfter" ) );
                                    events.Add(new HoloscreenHackedEvent(timestamp, powerBefore, powerAfter) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayCollect":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue( "Count", out var val );
                                    var amount = (int)(long)val;
                                    // May be a commodity or a microresource
                                    var receivedEdName = JsonParsing.getString( data, "Type" );
                                    var commodityDef = CommodityDefinition.AllOfThem.FirstOrDefault(c => c.edname.Equals(receivedEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( commodityDef != null )
                                    {
                                        commodityDef.fallbackLocalizedName = JsonParsing.getString( data, "Type_Localised" );
                                        events.Add( new PowerCommodityObtainedEvent( timestamp, power, commodityDef, amount ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    var microResourceDef = MicroResource.AllOfThem.FirstOrDefault(m => m.edname.Equals(receivedEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( microResourceDef != null )
                                    {
                                        // Microresources are already handled via the PowerMicroResourcesCollectedEvent
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;
                            case "PowerplayDeliver":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue( "Count", out var val );
                                    var amount = (int)(long)val;
                                    // May be a commodity or a microresource
                                    var deliveredEdName = JsonParsing.getString( data, "Type" );
                                    var commodityDef = CommodityDefinition.AllOfThem.FirstOrDefault(c => c.edname.Equals(deliveredEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( commodityDef != null )
                                    {
                                        commodityDef.fallbackLocalizedName = JsonParsing.getString( data, "Type_Localised" );
                                        events.Add( new PowerCommodityDeliveredEvent( timestamp, power, commodityDef, amount ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    var microResourceDef = MicroResource.AllOfThem.FirstOrDefault(m => m.edname.Equals(deliveredEdName, StringComparison.InvariantCultureIgnoreCase));
                                    if ( microResourceDef != null )
                                    {
                                        // Microresources are already handled via the PowerMicroResourcesDeliveredEvent
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;
                            case "PowerplayJoin":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    events.Add(new PowerJoinedEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayLeave":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    events.Add(new PowerLeftEvent(timestamp, power) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "PowerplayMerits":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var meritsGained = JsonParsing.getInt( data, "MeritsGained" );
                                    var meritsTotal = JsonParsing.getInt( data, "TotalMerits" );
                                    events.Add(new PowerMeritsEvent(timestamp, power, meritsGained, meritsTotal ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "PowerplayRank":
                                {
                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    var rank = JsonParsing.getInt( data, "Rank" );
                                    events.Add( new PowerRankEvent( timestamp, power, rank ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "PowerplayVoucher":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var power = Power.FromEDName(JsonParsing.getString(data, "Power"));
                                    data.TryGetValue("Systems", out var val);
                                    var systems = ((List<object>)val).Cast<string>().ToList();
                                    events.Add(new PowerVoucherReceivedEvent(timestamp, power, systems) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RequestPowerMicroResources":
                                {
                                    // {"timestamp":"2024-10-23T19:59:28Z","event":"RequestPowerMicroResources","TotalCount":3,"MicroResources":[{"Name":"powerspyware","Name_Localised":"Power Tracker Malware","Category":"Data","Count":3}],"MarketID":3930400257}
                                    if ( data.ContainsKey( "TotalCount" ) )
                                    {
                                        var marketID = JsonParsing.getLong( data, "MarketID" );
                                        var resourceAmounts = new List<MicroResourceAmount>();
                                        if ( data.TryGetValue( "MicroResources", out var val ) )
                                        {
                                            if ( val is List<object> listVal )
                                            {
                                                foreach ( var res in listVal )
                                                {
                                                    if ( res is IDictionary<string, object> microVal )
                                                    {
                                                        var microResource = EventParsing.MicroResource( microVal );
                                                        var amount = JsonParsing.getInt(microVal, "Count");
                                                        if ( microResource != null )
                                                        {
                                                            resourceAmounts.Add( new MicroResourceAmount( microResource, amount ) );
                                                        }
                                                    }
                                                }
                                                events.Add( new PowerMicroResourcesCollectedEvent( timestamp, marketID, resourceAmounts ) { raw = line, fromLoad = fromLogLoad } );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // We haven't seen this pattern before. Break unhandled.
                                        break;
                                    }
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Squadron Events

                            case "AppliedToSquadron":
                            case "DisbandedSquadron":
                            case "InvitedToSquadron":
                            case "JoinedSquadron":
                            case "KickedFromSquadron":
                            case "LeftSquadron":
                            case "SquadronCreated":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "SquadronName");
                                    var squadronID = JsonParsing.getOptionalInt( data, "SquadronID" );
                                    var status = edType.Replace("Squadron", "")
                                        .Replace("To", "")
                                        .Replace("From", "")
                                        .ToLowerInvariant();

                                    events.Add(new SquadronStatusEvent(timestamp, name, squadronID, status) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SquadronDemotion":
                            case "SquadronPromotion":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "SquadronName");
                                    var squadronID = JsonParsing.getOptionalInt( data, "SquadronID" );

                                    var oldRankID = JsonParsing.getOptionalInt(data, "OldRank");
                                    var oldRankName = JsonParsing.getString(data, "OldRankName");
                                    var oldRankNameLocalized = JsonParsing.getString(data, "OldRankName_Localised");
                                    var oldRank = new SquadronRank(oldRankID, oldRankName, oldRankNameLocalized);

                                    var newRankID = JsonParsing.getOptionalInt(data, "NewRank");
                                    var newRankName = JsonParsing.getString(data, "NewRankName");
                                    var newRankNameLocalized = JsonParsing.getString(data, "NewRankName_Localised");
                                    var newRank = new SquadronRank(newRankID, newRankName, newRankNameLocalized);

                                    events.Add(new SquadronRankEvent(timestamp, name, squadronID, oldRank, newRank) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Fleet Carrier Events

                            case "CarrierBankTransfer":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var deposit = JsonParsing.getOptionalLong(data, "Deposit") ?? 0;
                                    var withdrawal = JsonParsing.getOptionalLong(data, "Withdraw") ?? 0;
                                    var cmdrBalance = JsonParsing.getOptionalLong(data, "PlayerBalance") ?? 0;
                                    var carrierBalance = JsonParsing.getOptionalLong(data, "CarrierBalance") ?? 0;
                                    events.Add(new CarrierBankTransferEvent(timestamp, carrierID, carrierType, deposit, withdrawal, cmdrBalance, carrierBalance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierBuy":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var carrierCallsign = JsonParsing.getString(data, "Callsign");
                                    var carrierStarSystem = JsonParsing.getString(data, "Location");
                                    var carrierSystemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var price = JsonParsing.getOptionalLong(data, "Price");
                                    events.Add(new CarrierPurchasedEvent(timestamp, carrierID, carrierCallsign, carrierType, carrierStarSystem, carrierSystemAddress, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierCancelDecommission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    events.Add(new CarrierDecommissionCancelledEvent(timestamp, carrierID, carrierType ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierCrewServices":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var operation = JsonParsing.getString(data, "Operation");
                                    var crewRole = StationService.FromEDName(JsonParsing.getString(data, "CrewRole"));
                                    var crewName = JsonParsing.getString(data, "CrewName");
                                    events.Add(new CarrierServiceChangedEvent(timestamp, carrierID, carrierType, operation, crewRole, crewName) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDecommission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var refund = JsonParsing.getULong(data, "ScrapRefund");
                                    var decommissionTimespan = Dates.fromTimestamp(JsonParsing.getOptionalLong(data, "ScrapTime")) - timestamp;
                                    events.Add(new CarrierDecommissionScheduledEvent(timestamp, carrierID, carrierType, refund, decommissionTimespan ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDepositFuel":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var amount = JsonParsing.getInt(data, "Amount");
                                    var total = JsonParsing.getInt(data, "Total");
                                    events.Add(new CarrierFuelDepositEvent(timestamp, carrierID, carrierType, amount, total) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierDockingPermission":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var dockingAccess = JsonParsing.getString(data, "DockingAccess");
                                    var allowNotorious = JsonParsing.getBool(data, "AllowNotorious");
                                    events.Add(new CarrierDockingPermissionEvent(timestamp, carrierID, carrierType, dockingAccess, allowNotorious) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierFinance":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var taxRate = JsonParsing.getOptionalInt(data, "TaxRate") ?? 0;
                                    var reservePercent = JsonParsing.getOptionalInt(data, "ReservePercent") ?? 0;
                                    var carrierBalance = JsonParsing.getLong(data, "CarrierBalance");
                                    var carrierReserveBalance = JsonParsing.getLong(data, "ReserveBalance");
                                    var carrierAvailableBalance = JsonParsing.getLong(data, "CarrierAvailableBalance");
                                    events.Add(new CarrierFinanceEvent(timestamp, carrierID, carrierType, taxRate, reservePercent, carrierBalance, carrierReserveBalance, carrierAvailableBalance) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierJump":
                                {
                                    // Get destination star system data
                                    var systemName = JsonParsing.getString( data, "StarSystem" );
                                    data.TryGetValue( "StarPos", out var starposVal );
                                    var starPos = (List<object>)starposVal;
                                    var x = Math.Round( JsonParsing.getDecimal( "X", starPos?[ 0 ] ) * 32 ) /
                                            (decimal)32.0;
                                    var y = Math.Round( JsonParsing.getDecimal( "Y", starPos?[ 1 ] ) * 32 ) /
                                            (decimal)32.0;
                                    var z = Math.Round( JsonParsing.getDecimal( "Z", starPos?[ 2 ] ) * 32 ) /
                                            (decimal)32.0;
                                    var systemAddress = JsonParsing.getULong( data, "SystemAddress" );
                                    var systemEconomy =
                                        Economy.FromEDName( JsonParsing.getString( data, "SystemEconomy" ) );
                                    var systemEconomy2 =
                                        Economy.FromEDName( JsonParsing.getString( data, "SystemSecondEconomy" ) );
                                    var systemSecurity =
                                        SecurityLevel.FromEDName( JsonParsing.getString( data, "SystemSecurity" ) );
                                    systemSecurity.fallbackLocalizedName =
                                        JsonParsing.getString( data, "SystemSecurity_Localised" );
                                    var systemPopulation = JsonParsing.getOptionalLong( data, "Population" );

                                    // Get destination body data (if any)
                                    var bodyName = JsonParsing.getString( data, "Body" );
                                    var bodyId = JsonParsing.getOptionalLong( data, "BodyID" );
                                    var bodyType = BodyType.FromEDName( JsonParsing.getString( data, "BodyType" ) ) ??
                                                   BodyType.None;
                                    if ( bodyType == BodyType.Planet )
                                    {
                                        var starSystem = EDDI.Instance.DataProvider
                                            .GetOrCreateStarSystemAsync( systemAddress, systemName ).GetAwaiter()
                                            .GetResult();
                                        bodyType = starSystem?.bodies.FirstOrDefault( b => b.bodyId != null && b.bodyId == bodyId )?.bodyType ?? bodyType;
                                    }

                                    // Get carrier data (may not be present when on-foot at a fleet carrier but not docked)
                                    var carrierId = JsonParsing.getOptionalLong( data, "MarketID" );
                                    EventParsing.StationNameAndType( data, out var carrierName, out _, out var carrierType );

                                    // Get carrier services data (may not be present when on-foot at a fleet carrier but not docked)
                                    var stationServices = new List<StationService>();
                                    data.TryGetValue( "StationServices", out var stationserviceVal );
                                    var stationservices =
                                        ( stationserviceVal as List<object> )?.Cast<string>()?.ToList() ??
                                        new List<string>();
                                    foreach ( var service in stationservices )
                                    {
                                        stationServices.Add( StationService.FromEDName( service ) );
                                    }

                                    // Get carrier economies and their shares (may not be present when on-foot at a fleet carrier but not docked)
                                    data.TryGetValue( "StationEconomies", out var economiesVal );
                                    var economies = economiesVal as List<object> ?? new List<object>();
                                    var stationEconomies = new List<EconomyShare>();
                                    foreach ( var economyShareVal in economies )
                                    {
                                        if ( economyShareVal is Dictionary<string, object> economyshare )
                                        {
                                            var economy =
                                                Economy.FromEDName( JsonParsing.getString( economyshare, "Name" ) );
                                            economy.fallbackLocalizedName =
                                                JsonParsing.getString( economyshare, "Name_Localised" );
                                            var share = JsonParsing.getDecimal( economyshare, "Proportion" );
                                            if ( economy != Economy.None && share > 0 )
                                            {
                                                stationEconomies.Add( new EconomyShare( economy, share ) );
                                            }
                                        }
                                    }

                                    // Parse factions array data
                                    var factions = new List<Faction>();
                                    data.TryGetValue( "Factions", out var factionsVal );
                                    if ( factionsVal != null )
                                    {
                                        factions = EventParsing.Factions( factionsVal, systemName, systemAddress );
                                    }
                                    var systemfaction = EventParsing.Faction( data, "System", systemName, systemAddress, factions );
                                    var stationFaction = EventParsing.Faction( data, "Station", systemName, systemAddress, factions );

                                    // Parse conflicts array data
                                    var conflicts = new List<Conflict>();
                                    data.TryGetValue( "Conflicts", out var conflictsVal );
                                    if ( conflictsVal != null )
                                    {
                                        conflicts = EventParsing.FactionConflicts( conflictsVal, factions );
                                    }

                                    // Powerplay data (if pledged)
                                    EventParsing.PowerplayDetails( data, systemAddress, out var controllingPower,
                                        out var powersInAcquisitionRange, out var powerplayState,
                                        out var powerAcquisitionProgress,
                                        out var powerplayControlProgress, out var powerplayReinforcementControlPoints,
                                        out var powerplayUnderminingControlPoints );

                                    // Thargoid war data (if any)
                                    EventParsing.ThargoidWarData( data, out var thargoidWar );

                                    var docked = JsonParsing.getBool( data, "Docked" );
                                    var onFoot = JsonParsing.getOptionalBool( data, "OnFoot" ) ?? false;

                                    events.Add( new CarrierJumpedEvent( timestamp, systemName, systemAddress, x, y, z,
                                        bodyName, bodyId, bodyType, docked, onFoot, carrierName, carrierType, carrierId,
                                        stationServices, systemfaction, stationFaction, factions, conflicts,
                                        stationEconomies, systemEconomy, systemEconomy2, systemSecurity,
                                        systemPopulation, controllingPower, powersInAcquisitionRange, powerplayState,
                                        powerAcquisitionProgress, powerplayControlProgress,
                                        powerplayReinforcementControlPoints, powerplayUnderminingControlPoints,
                                        thargoidWar ) { raw = line, fromLoad = fromLogLoad } );

                                    // Generate secondary events when the carrier jump cooldown completes
                                    if ( carrierId != null )
                                    {
                                        if ( carrierJumpCancellationTokenSources.TryGetValue( (long)carrierId, out var carrierJumpCancellationTS ) )
                                        {
                                            // Cancel any pending cooldown event (to prevent doubling events if the commander is the fleet carrier owner)
                                            carrierJumpCancellationTS.Cancel();
                                        }

                                        if ( !fromLogLoad )
                                        {
                                            Task.Run( async () =>
                                            {
                                                // Cooldown timer starts when the carrier jump is engaged, not when the jump ends
                                                var timeMs = ( Constants.carrierPostJumpSeconds -
                                                               Constants.carrierJumpSeconds ) *
                                                             1000;
                                                await Task.Delay( timeMs ).ConfigureAwait(false);
                                                EDDI.Instance.enqueueEvent(
                                                    new CarrierCooldownEvent( timestamp.AddMilliseconds( timeMs ),
                                                            (long)carrierId,
                                                            carrierName, carrierType, systemName, systemAddress, bodyName, bodyId, bodyType )
                                                        { fromLoad = fromLogLoad } );
                                            } );
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "CarrierJumpRequest":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierId = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var systemName = JsonParsing.getString(data, "SystemName");
                                    var bodyName = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getLong(data, "BodyID");
                                    var departureTime = JsonParsing.getDateTime( "DepartureTime", data );

                                    // There is a bug in the journal output where "Body" can be missing but "BodyID" can be present. Try to Work around that here.
                                    if (string.IsNullOrEmpty(bodyName) && systemAddress > 0)
                                    {
                                        var starSystem = EDDI.Instance.DataProvider.GetOrCreateStarSystemAsync( systemAddress, systemName ).GetAwaiter().GetResult();
                                        bodyName = starSystem?.bodies?.FirstOrDefault(b => b?.bodyId == bodyId)?.bodyname;
                                    }

                                    events.Add(new CarrierJumpRequestEvent(timestamp, systemName, systemAddress, bodyName, bodyId, carrierId, carrierType ) { raw = line, fromLoad = fromLogLoad });

                                    // Cancel any pending carrier jump related events
                                    if (carrierJumpCancellationTokenSources.TryGetValue(carrierId, out var carrierJumpCancellationTS))
                                    {
                                        carrierJumpCancellationTS.Cancel();
                                    }

                                    if (!fromLogLoad)
                                    {
                                        // Generate a new cancellation token source
                                        carrierJumpCancellationTS = new CancellationTokenSource();
                                        carrierJumpCancellationTokenSources[carrierId] = carrierJumpCancellationTS;

                                        // Generate secondary tasks to spawn events when the carrier locks down landing pads and when it begins jumping.
                                        // These may be cancelled via the cancellation token source above.

                                        var departureSeconds = ( departureTime - timestamp ).TotalSeconds;
                                        var tasks = new List<Task>
                                        {
                                            Task.Run(async () =>
                                            {
                                                var timespan = TimeSpan.FromSeconds(departureSeconds - Constants.carrierLandingPadLockdownSeconds);
                                                await Task.Delay(timespan, carrierJumpCancellationTS.Token).ConfigureAwait(false);
                                                EDDI.Instance.enqueueEvent(new CarrierPadsLockedEvent(timestamp.Add(timespan), carrierId, carrierType ) { fromLoad = fromLogLoad });
                                            }, carrierJumpCancellationTS.Token),
                                            Task.Run(async () =>
                                            {
                                                var timespan = TimeSpan.FromSeconds(departureSeconds);
                                                await Task.Delay(timespan, carrierJumpCancellationTS.Token).ConfigureAwait(false);
                                                if ( EDDI.Instance.CurrentStarSystem != null )
                                                {
                                                    var originStarSystem = EDDI.Instance.CurrentStarSystem.systemname;
                                                    var originSystemAddress = EDDI.Instance.CurrentStarSystem.systemAddress;
                                                EDDI.Instance.enqueueEvent(new CarrierJumpEngagedEvent(timestamp.Add(timespan), systemName, systemAddress, originStarSystem, originSystemAddress, bodyName, bodyId, carrierId, carrierType ) { fromLoad = fromLogLoad });
                                                }
                                            }, carrierJumpCancellationTS.Token),
                                            Task.Run(async () =>
                                            {
                                                // This event will be canceled and replaced by an updated `CarrierCooldownEvent` if the owner is aboard the fleet carrier and sees the `CarrierJumpedEvent`.
                                                var timespan = TimeSpan.FromSeconds(departureSeconds + Constants.carrierPostJumpSeconds); // Cooldown timer starts when the carrier jump is engaged, not when the jump ends
                                                await Task.Delay(timespan, carrierJumpCancellationTS.Token).ConfigureAwait(false);
                                                EDDI.Instance.enqueueEvent(new CarrierCooldownEvent(timestamp.Add(timespan), carrierId, null, carrierType, systemName, systemAddress, bodyName, bodyId, null ) { fromLoad = fromLogLoad });
                                            }, carrierJumpCancellationTS.Token)
                                        };

                                        Task.Run(async () =>
                                        {
                                            try
                                            {
                                                await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                                            }
                                            catch (OperationCanceledException)
                                            {
                                                // Tasks were cancelled. Nothing to do here.
                                            }
                                            finally
                                            {
                                                carrierJumpCancellationTokenSources.Remove(carrierId);
                                                carrierJumpCancellationTS.Dispose();
                                            }
                                        });
                                    }
                                }
                                handled = true;
                                break;
                            case "CarrierJumpCancelled":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierId = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );

                                    // Cancel any pending carrier jump related events
                                    if ( carrierJumpCancellationTokenSources.TryGetValue(carrierId, out var carrierJumpCancellationTS))
                                    {
                                        carrierJumpCancellationTS.Cancel();
                                    }
                                    events.Add(new CarrierJumpCancelledEvent(timestamp, carrierId, carrierType ) { raw = line, fromLoad = fromLogLoad });
                                    if (!fromLogLoad)
                                    {
                                        Task.Run(async () =>
                                        {
                                            var timeMs = 60000; // Cooldown timer starts when the carrier jump is cancelled and lasts for one minute
                                            await Task.Delay(timeMs).ConfigureAwait(false);
                                            var carrier = StationModel.SquadronCarrier.edname.Equals( carrierType.edname ) ? EDDI.Instance.SquadronCarrier : EDDI.Instance.FleetCarrier;
                                            EDDI.Instance.enqueueEvent( new CarrierCooldownEvent( timestamp.AddMilliseconds( timeMs ), carrierId, carrier?.callsign, carrierType, carrier?.currentStarSystem, carrier?.currentStarSystemAddress, null, carrier?.currentBodyID, null ) { fromLoad = fromLogLoad } );
                                        } );
                                    }
                                }
                                handled = true;
                                break;
                            case "CarrierLocation":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var systemName = JsonParsing.getString(data, "StarSystem");
                                    var bodyId = JsonParsing.getLong(data, "BodyID");
                                    events.Add(new CarrierLocationEvent(timestamp, carrierID, carrierType, systemAddress, systemName, bodyId ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "CarrierNameChange":
                                {
                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );
                                    var callsign = JsonParsing.getString(data, "Callsign");
                                    var name = JsonParsing.getString(data, "Name");
                                    events.Add(new CarrierNameChangeEvent(timestamp, carrierID, carrierType, callsign, name ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CarrierStats":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierID = JsonParsing.getLong(data, "CarrierID");
                                    var carrierCallsign = JsonParsing.getString(data, "Callsign");
                                    var carrierName = JsonParsing.getString(data, "Name");
                                    var carrierType = StationModel.FromEDName( JsonParsing.getString( data, "CarrierType" ) );

                                    var dockingAccess = JsonParsing.getString(data, "DockingAccess");
                                    var notoriousAccess = JsonParsing.getBool(data, "AllowNotorious");
                                    var fuelLevel = JsonParsing.getInt(data, "FuelLevel");
                                    var jumpRange = JsonParsing.getOptionalDecimal( data, "JumpRangeCurr" ) ?? 500;
                                    var jumpRangeMax = JsonParsing.getOptionalDecimal( data, "JumpRangeMax" ) ?? 500;
                                    var pendingDecommission = JsonParsing.getOptionalBool(data, "PendingDecommission") ?? false;

                                    var crewSpace = 0;
                                    var cargoSpace = 0;
                                    var cargoSpaceReserved = 0;
                                    var shipPacks = 0;
                                    var modulePacks = 0;
                                    var freeSpace = 0;
                                    if (data.TryGetValue("SpaceUsage", out var spaceUsage) && spaceUsage is Dictionary<string, object> space)
                                    {
                                        crewSpace = JsonParsing.getInt(space, "Crew");
                                        cargoSpace = JsonParsing.getInt(space, "Cargo");
                                        cargoSpaceReserved = JsonParsing.getInt(space, "CargoSpaceReserved");
                                        shipPacks = JsonParsing.getInt(space, "ShipPacks");
                                        modulePacks = JsonParsing.getInt(space, "ModulePacks");
                                        freeSpace = JsonParsing.getInt(space, "FreeSpace");
                                    }
                                    var usedSpace = crewSpace 
                                                    + cargoSpace 
                                                    + cargoSpaceReserved 
                                                    + shipPacks 
                                                    + modulePacks;

                                    long bankBalance = 0;
                                    long bankReservedBalance = 0;
                                    long bankAvailableBalance = 0;
                                    if (data.TryGetValue("Finance", out var finances) && finances is Dictionary<string, object> finance)
                                    {
                                        bankBalance = JsonParsing.getLong(finance, "CarrierBalance");
                                        bankReservedBalance = JsonParsing.getLong(finance, "ReserveBalance");
                                        bankAvailableBalance = JsonParsing.getLong(finance, "AvailableBalance");
                                    }

                                    events.Add(new CarrierStatsEvent(timestamp, carrierID, carrierType, carrierCallsign, carrierName, dockingAccess, notoriousAccess, fuelLevel, usedSpace, freeSpace, bankBalance, bankReservedBalance, bankAvailableBalance, jumpRange, jumpRangeMax, pendingDecommission ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Odyssey (On Foot) Events
                                
                            case "Backpack":
                            case "ShipLocker":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( MicroResourceInfo.TryFromFile(timestamp, out var info, out line, $"{edType}.json"))
                                    {
                                        if ( info is null ) { break; }

                                        // Flatten the list
                                        var inventory = new List<MicroResourceAmount>();
                                        inventory.AddRange(info.Components);
                                        inventory.AddRange(info.Consumables);
                                        inventory.AddRange(info.Data);
                                        inventory.AddRange(info.Items);

                                        if (edType == "Backpack")
                                        {
                                            events.Add(new BackpackEvent(timestamp, inventory) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (edType == "ShipLocker")
                                        {
                                            events.Add(new ShipLockerEvent(timestamp, inventory) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "BackpackChange":
                                {
                                    // Note: Also updates backpack.json
                                    var added = MicroResourceInfo.ReadMicroResources("Added", data);
                                    var removed = MicroResourceInfo.ReadMicroResources("Removed", data);
                                    events.Add(new BackpackChangedEvent(timestamp, added, removed) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BookDropship":
                            case "BookTaxi":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var type = edType.Replace("Book", "");
                                    var price = JsonParsing.getOptionalInt(data, "Cost");
                                    var system = JsonParsing.getString(data, "DestinationSystem");
                                    var destination = JsonParsing.getString(data, "DestinationLocation");
                                    events.Add(new BookTransportEvent(timestamp, type, price, system, destination) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "BuyMicroResources":
                                {
                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");

                                    // The `BuyMicroResources` event sometimes contains an array
                                    // (thought to occur primarily when buying from a fleet carrier bartender)
                                    if ( data.ContainsKey("TotalCount") )
                                    {
                                        var price = JsonParsing.getInt( data, "Price" ); // Total price
                                        var resourceAmounts = new List<MicroResourceAmount>();
                                        if ( data.TryGetValue( "MicroResources", out var val ) )
                                        {
                                            if ( val is List<object> listVal )
                                            {
                                                foreach ( var res in listVal )
                                                {
                                                    if ( res is IDictionary<string, object> microVal )
                                                    {
                                                        var microResource = EventParsing.MicroResource( microVal );
                                                        var amount = JsonParsing.getInt(microVal, "Count");
                                                        if ( microResource != null )
                                                        {
                                                            resourceAmounts.Add( new MicroResourceAmount( microResource, amount ) );
                                                        }
                                                    }
                                                }
                                                events.Add( new MicroResourcesPurchasedEvent( timestamp, resourceAmounts, price, marketId ) { raw = line, fromLoad = fromLogLoad } );
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var microResource = EventParsing.MicroResource( data );
                                        var amount = JsonParsing.getInt( data, "Count" );
                                        var price = JsonParsing.getInt(data, "Price"); // Total price
                                        var resourceAmounts = new List<MicroResourceAmount> { new MicroResourceAmount( microResource, amount ) };
                                        events.Add( new MicroResourcesPurchasedEvent( timestamp, resourceAmounts, price, marketId ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                    handled = true;
                                }
                                break;
                            case "BuySuit":
                                {
                                    var edname = JsonParsing.getString(data, "Name");
                                    var fallbackName = JsonParsing.getString(data, "Name_Localised");
                                    var suitId = JsonParsing.getOptionalLong(data, "SuitID");
                                    var price = JsonParsing.getOptionalInt(data, "Price");
                                    var suit = Suit.FromEDName(edname, suitId);
                                    suit.fallbackLocalizedName = fallbackName;
                                    events.Add(new SuitPurchasedEvent(timestamp, suit, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CancelDropship":
                            case "CancelTaxi":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var type = edType.Replace("Cancel", "");
                                    var refund = JsonParsing.getOptionalInt(data, "Refund");
                                    events.Add(new CancelTransportEvent(timestamp, type, refund) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Disembark":
                                {
                                    var fromSRV = JsonParsing.getBool(data, "SRV"); // true if getting out of SRV, false if getting out of a ship 
                                    var fromTaxi = JsonParsing.getBool(data, "Taxi"); //  true when getting out of a transport ship (e.g. Apex Taxi or Frontline Solutions dropship)
                                    var fromMultiCrew = JsonParsing.getBool(data, "Multicrew"); //  true when getting out of another player’s vessel
                                    var fromLocalId = JsonParsing.getOptionalInt(data, "ID"); // player’s ship ID (if player's own vessel)

                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    var onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");

                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    EventParsing.StationNameAndType( data, out var stationName, out _, out var stationModel );  // if at a station

                                    events.Add(new DisembarkEvent(timestamp, fromSRV, fromTaxi, fromMultiCrew, fromLocalId, system, systemAddress, body, bodyId, onStation, onPlanet, stationName, marketId, stationModel) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DropshipDeploy":
                                {
                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    // There are `OnStation` and `OnPlanet` properties, but these are
                                    // always false and always true so we won't bother parsing them.

                                    events.Add(new DropshipDeploymentEvent(timestamp, system, systemAddress, body, bodyId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Embark":
                                {
                                    var toSRV = JsonParsing.getBool(data, "SRV"); // true if getting out of SRV, false if getting out of a ship 
                                    var toTaxi = JsonParsing.getBool(data, "Taxi"); //  true when getting out of a transport ship (e.g. Apex Taxi or Frontline Solutions dropship)
                                    var toMultiCrew = JsonParsing.getBool(data, "Multicrew"); //  true when getting out of another player’s vessel
                                    var toLocalId = JsonParsing.getOptionalInt(data, "ID"); // player’s ship ID (if player's own vessel)

                                    var system = JsonParsing.getString(data, "StarSystem");
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var body = JsonParsing.getString(data, "Body");
                                    var bodyId = JsonParsing.getOptionalInt(data, "BodyID");
                                    var onStation = JsonParsing.getOptionalBool(data, "OnStation");
                                    var onPlanet = JsonParsing.getOptionalBool(data, "OnPlanet");

                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID");
                                    EventParsing.StationNameAndType( data, out var stationName, out _, out var stationModel );  // if at a station

                                    events.Add(new EmbarkEvent(timestamp, toSRV, toTaxi, toMultiCrew, toLocalId, system, systemAddress, body, bodyId, onStation, onPlanet, stationName, marketId, stationModel) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FCMaterials":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var carrierID = JsonParsing.getLong(data, "MarketID");
                                    var carrierName = JsonParsing.getString(data, "CarrierName");
                                    var callsign = JsonParsing.getString(data, "CarrierID");

                                    var raw = Files.FromSavedGames("FCMaterials.json");
                                    if (raw != null)
                                    {
                                        var info = JsonConvert.DeserializeObject<FCMaterialsInfo>(raw);
                                        if (info != null && info.CarrierID == carrierID
                                                         && info.CarrierName == carrierName
                                                         && info.callsign == callsign)
                                        {
                                            events.Add(new FleetCarrierMaterialsEvent(timestamp, carrierID, carrierName, callsign, info) { raw = raw, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Other (Miscellaneous) Events

                            case "AfmuRepairs":
                                {
                                    var item = JsonParsing.getString(data, "Module");
                                    // Item might be a module
                                    var module = Module.FromEDName(item);
                                    if (module != null)
                                    {
                                        if (module.Mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            string mount;
                                            if (module.Mount == ModuleMount.Fixed)
                                            {
                                                mount = "fixed";
                                            }
                                            else if (module.Mount == ModuleMount.Gimballed)
                                            {
                                                mount = "gimballed";
                                            }
                                            else
                                            {
                                                mount = "turreted";
                                            }
                                            item = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.localizedName;
                                        }
                                        else
                                        {
                                            item = module.localizedName;
                                        }
                                    }

                                    // There is an FDev bug that can set `FullyRepaired` to false even when the module health is full,
                                    // so we work around this by relying on the `Health` property rather than the `FullyRepaired` property.
                                    // This appears to be a unique problem with Module Reinforcement Packages.

                                    var health = JsonParsing.getDecimal(data, "Health");
                                    var repairedfully = health == 1M;

                                    events.Add(new ShipAfmuRepairedEvent(timestamp, item, repairedfully, health) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ApproachSettlement":
                                {
                                    // The settlement name may be a proper name or a symbolic name.
                                    var settlementName = JsonParsing.getString(data, "Name").ReplaceEnd('+');

                                    // Symbolic names may include a localized name which may have an number appended to it (e.g. "Ancient Ruins (3)".
                                    // If so, remove the appended number.
                                    var localizedName = JsonParsing.getString(data, "Name_Localised");
                                    if ( !string.IsNullOrEmpty(localizedName) )
                                    {
                                        localizedName = Regex.Replace( localizedName, @"\s\(\d\)", "" ).Trim();
                                    }

                                    var marketId = JsonParsing.getOptionalLong(data, "MarketID"); // Tourist beacons and guardian structures are reported as settlements without MarketID
                                    var systemAddress = JsonParsing.getULong(data, "SystemAddress");
                                    var bodyName = JsonParsing.getString(data, "BodyName");
                                    var bodyId = JsonParsing.getOptionalLong(data, "BodyID");

                                    var latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                                    var longitude = JsonParsing.getOptionalDecimal(data, "Longitude");

                                    var controllingFaction = EventParsing.Faction(data, "Station", EDDI.Instance.CurrentStarSystem?.systemname, systemAddress, EDDI.Instance.CurrentStarSystem?.factions);

                                    // Get station services data
                                    var stationServices = new List<StationService>();
                                    if ( data.TryGetValue( "StationServices", out var val ) )
                                    {
                                        stationServices = ( val as List<object> )
                                            .Cast<string>()
                                            .Select( StationService.FromEDName )
                                            .ToList();
                                    }

                                    // Get station economies and their shares
                                    var Economies = new List<EconomyShare>();
                                    if ( data.TryGetValue( "StationEconomies", out var val2 ) )
                                    {
                                        var economies = ( val2 as List<object> )
                                            .Cast<Dictionary<string, object>>();
                                        foreach ( var economyshare in economies )
                                        {
                                            var economy = Economy.FromEDName(JsonParsing.getString(economyshare, "Name"));
                                            economy.fallbackLocalizedName = JsonParsing.getString( economyshare, "Name_Localised" );
                                            var share = JsonParsing.getDecimal(economyshare, "Proportion");
                                            if ( economy != Economy.None && share > 0 )
                                            {
                                                Economies.Add( new EconomyShare( economy, share ) );
                                            }
                                        }
                                    }

                                    events.Add(new SettlementApproachedEvent(timestamp, settlementName, localizedName, marketId, controllingFaction, stationServices, Economies, systemAddress, bodyName, bodyId, latitude, longitude) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CargoTransfer":
                                {
                                    var toShip = new List<CommodityAmount>();
                                    var toSRV = new List<CommodityAmount>();
                                    var toCarrier = new List<CommodityAmount>();
                                    if ( data.TryGetValue("Transfers", out var transfersVal) )
                                    {
                                        var transfersArray = JArray.FromObject( transfersVal );
                                        foreach ( var transfer in transfersArray )
                                        {
                                            var direction = transfer[ "Direction" ].ToString();
                                            var count = (int)transfer[ "Count" ];
                                            var commodity = CommodityDefinition.FromEDName( transfer[ "Type" ].ToString() );
                                            commodity.fallbackLocalizedName = transfer[ "Type_Localised" ]?.ToString();

                                            // Objects may have a `MissionID` but the legalstatus is not identified so we rtat these items
                                            // as CommodityAmount objects and use the `Cargo` event to update the CargoMonitor.

                                            var commodityAmount = new CommodityAmount( commodity, count );
                                            if ( direction.Equals( "toship", StringComparison.InvariantCultureIgnoreCase ) )
                                            {
                                                toShip.Add( commodityAmount );
                                            }
                                            else if ( direction.Equals( "tosrv", StringComparison.InvariantCultureIgnoreCase ) )
                                            {
                                                toSRV.Add( commodityAmount );
                                            }
                                            else if ( direction.Equals( "tocarrier", StringComparison.InvariantCultureIgnoreCase ) )
                                            {
                                                toCarrier.Add( commodityAmount );
                                            }
                                            else
                                            {
                                                throw new ArgumentException( "Unhandled CargoTransfer `Direction`." );
                                            }
                                        }
                                    }

                                    var cargoTransferEvent = new CargoTransferEvent( timestamp, toShip, toSRV, toCarrier ) { raw = line, fromLoad = fromLogLoad };
                                    if ( fromSpeechResponderTest )
                                    {
                                        events.Add( cargoTransferEvent );
                                    }
                                    else
                                    {
                                        if ( !DelayedEventHolder.TryAdd( CargoEvent.NAME, new ConcurrentBag<Event>( new[] { cargoTransferEvent } ) ) )
                                        {
                                            DelayedEventHolder[ CargoEvent.NAME ].Add( cargoTransferEvent );
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "ChangeCrewRole":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var role = EventParsing.CrewRole(data, "Role");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewRoleChangedEvent(timestamp, role, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CockpitBreached":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    events.Add(new CockpitBreachedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CommitCrime":
                                {
                                    object val;
                                    var crimetype = JsonParsing.getString(data, "CrimeType");
                                    var faction = EventParsing.FactionName(data, "Faction");
                                    var victim = JsonParsing.getString(data, "Victim");

                                    if (!string.IsNullOrEmpty(JsonParsing.getString(data, "Victim_Localised")))
                                    {
                                        // This is an NPC with a symbolic name
                                        victim = NpcAuthorityShip.EDNameExists(victim)
                                            ? NpcAuthorityShip.FromEDName(victim)?.localizedName
                                            : JsonParsing.getString(data, "Victim_Localised");
                                    }

                                    // Might be a fine or a bounty
                                    if (data.ContainsKey("Fine"))
                                    {
                                        data.TryGetValue("Fine", out val);
                                        var fine = (long)val;
                                        events.Add(new FineIncurredEvent(timestamp, crimetype, faction, victim, fine) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        data.TryGetValue("Bounty", out val);
                                        var bounty = (long)val;
                                        events.Add(new BountyIncurredEvent(timestamp, crimetype, faction, victim, bounty) { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "CrewLaunchFighter":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "Crew");
                                    var fighterId = JsonParsing.getInt(data, "ID");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberLaunchedEvent(timestamp, name, fighterId, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberJoins":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberJoinedEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberRoleChange":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "Crew");
                                    var role = EventParsing.CrewRole(data, "Role");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberRoleChangedEvent(timestamp, name, role, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "CrewMemberQuits":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberLeftEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DatalinkScan":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var message = JsonParsing.getString(data, "Message");
                                    events.Add(new DatalinkMessageEvent(timestamp, message) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DatalinkVoucher":
                                {
                                    data.TryGetValue("Reward", out var val);
                                    var reward = (long)val;
                                    var victimFaction = EventParsing.FactionName(data, "VictimFaction");
                                    var payeeFaction = EventParsing.FactionName(data, "PayeeFaction");
                                    events.Add(new DataVoucherAwardedEvent(timestamp, payeeFaction, victimFaction, reward) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DataScanned":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var datalinktype = DataScan.FromEDName(JsonParsing.getString(data, "Type"));
                                    events.Add(new DataScannedEvent(timestamp, datalinktype) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockFighter":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new FighterDockedEvent(timestamp, fighterId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "DockSRV":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var srvId = JsonParsing.getOptionalInt(data, "ID");
                                    var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                                    vehicleDefinition.fallbackLocalizedName = JsonParsing.getString(data, "SRVType_Localised");
                                    events.Add(new SRVDockedEvent(timestamp, vehicleDefinition, srvId) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "EndCrewSession":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var onCrime = JsonParsing.getOptionalBool(data, "OnCrime");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewSessionEndedEvent(timestamp, onCrime, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "FighterRebuilt":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var loadout = JsonParsing.getString(data, "Loadout");
                                    var fighterId = JsonParsing.getInt(data, "ID");
                                    events.Add(new FighterRebuiltEvent(timestamp, loadout, fighterId) { raw = line, fromLoad = fromLogLoad });
                                    handled = true;
                                }
                                break;
                            case "Friends":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var status = JsonParsing.getString(data, "Status");
                                    var name = JsonParsing.getString(data, "Name");
                                    name = name.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var @event = new FriendsEvent( timestamp, name, status ) { raw = line, fromLoad = fromLogLoad };

                                    // Friends events can be written before the commander is loaded and need to be delayed until we have seen a "Commander" event
                                    if ( EDDI.Instance.lastEventOfType.TryGetValue( CommanderLoadingEvent.NAME, out _ ) )
                                    {
                                        if ( !DelayedEventHolder.TryAdd( CommanderLoadingEvent.NAME, new ConcurrentBag<Event>( new[] { @event } ) ) )
                                        {
                                            DelayedEventHolder[ CommanderLoadingEvent.NAME ].Add( @event );
                                        }
                                    }
                                    else
                                    {
                                        events.Add( @event );
                                    }
                                }
                                handled = true;
                                break;
                            case "FuelScoop":
                                {
                                    // We're handling this via the Status Monitor
                                }
                                handled = true;
                                break;
                            case "JetConeBoost":
                                {
                                    var boost = JsonParsing.getDecimal(data, "BoostValue");
                                    events.Add(new JetConeBoostEvent(timestamp, boost) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JetConeDamage":
                                {
                                    var modulename = JsonParsing.getString(data, "Module");
                                    var module = Module.FromEDName(modulename);
                                    if (module != null)
                                    {
                                        if (module.Mount != null)
                                        {
                                            // This is a weapon so provide a bit more information
                                            var mount = module.mount;
                                            modulename = "" + module.@class.ToString() + module.grade + " " + mount + " " + module.localizedName;
                                        }
                                        else
                                        {
                                            modulename = module.localizedName;
                                        }
                                    }

                                    events.Add(new JetConeDamageEvent(timestamp, modulename, module) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "JoinACrew":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewJoinedEvent(timestamp, captain, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "KickCrewMember":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var member = JsonParsing.getString(data, "Crew");
                                    member = member.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewMemberRemovedEvent(timestamp, member, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchDrone":
                                {
                                    var kind = JsonParsing.getString(data, "Type");
                                    events.Add(new LimpetLaunchedEvent(timestamp, kind) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchFighter":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var loadout = JsonParsing.getString(data, "Loadout");
                                    var fighterId = JsonParsing.getInt(data, "ID");
                                    var playerControlled = JsonParsing.getBool(data, "PlayerControlled");
                                    events.Add(new FighterLaunchedEvent(timestamp, loadout, fighterId, playerControlled) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "LaunchSRV":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var loadout = JsonParsing.getString(data, "Loadout");
                                    var playercontrolled = JsonParsing.getBool(data, "PlayerControlled");
                                    var id = JsonParsing.getOptionalInt(data, "ID");
                                    var vehicleDefinition = VehicleDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                                    vehicleDefinition.fallbackLocalizedName = JsonParsing.getString(data, "SRVType_Localised");
                                    events.Add(new SRVLaunchedEvent(timestamp, loadout, playercontrolled, vehicleDefinition, id) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ModuleInfo":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( ModuleInfo.TryFromFile(timestamp, out var info, out line))
                                    {
                                        events.Add(new ModuleInfoEvent(timestamp, info.Modules)
                                            { raw = line, fromLoad = fromLogLoad });
                                    }
                                }
                                handled = true;
                                break;
                            case "Music":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var musicTrack = JsonParsing.getString(data, "MusicTrack");
                                    events.Add(new MusicEvent(timestamp, musicTrack) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "NpcCrewPaidWage":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "NpcCrewName");
                                    var crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    var amount = JsonParsing.getLong(data, "Amount");
                                    
                                    // Delay `Crew paid wage` events to occur after events where the commander receives a payment.
                                    if ( amount > 0 )
                                    {
                                        var crewPaidWageEvent = new CrewPaidWageEvent( timestamp, name, crewid, amount ) { raw = line, fromLoad = fromLogLoad };
                                        if ( fromSpeechResponderTest )
                                        {
                                            events.Add( crewPaidWageEvent );
                                        }
                                        else
                                        {
                                            Task.Run( async () =>
                                            {
                                                await Task.Delay( TimeSpan.FromSeconds( 5 ) ).ConfigureAwait( false );
                                                EDDI.Instance.enqueueEvent( crewPaidWageEvent );
                                            } );
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "NpcCrewRank":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var name = JsonParsing.getString(data, "NpcCrewName");
                                    var crewid = JsonParsing.getLong(data, "NpcCrewId");
                                    data.TryGetValue("RankCombat", out var val);
                                    var rating = CombatRating.FromRank(Convert.ToInt32(val));
                                    events.Add(new CrewPromotionEvent(timestamp, name, crewid, rating) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Promotion":
                                {
                                    object rating = null;
                                    if (data.TryGetValue("Combat", out var val))
                                    {
                                        rating = CombatRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("CQC", out val))
                                    {
                                        rating = CQCRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Trade", out val))
                                    {
                                        rating = TradeRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Explore", out val))
                                    {
                                        rating = ExplorationRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Federation", out val))
                                    {
                                        rating = FederationRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Empire", out val))
                                    {
                                        rating = EmpireRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Soldier", out val))
                                    {
                                        rating = MercenaryRating.FromRank(Convert.ToInt32(val));
                                    }
                                    else if (data.TryGetValue("Exobiologist", out val))
                                    {
                                        rating = ExobiologistRating.FromRank(Convert.ToInt32(val));
                                    }
                                    if (rating != null)
                                    {
                                        var genderPreference = ConfigService.Instance.commanderConfiguration.gender;
                                        events.Add(new CommanderPromotionEvent(timestamp, rating, genderPreference) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "ProspectedAsteroid":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    data.TryGetValue("Materials", out var val); // (array of Name and Proportion)
                                    var commodities = new List<CommodityPresence>();
                                    if (val is List<object> listVal)
                                    {
                                        foreach (var commodityVal in listVal)
                                        {
                                            if (commodityVal is Dictionary<string, object> commodityData)
                                            {
                                                var commodityEdName = JsonParsing.getString(commodityData, "Name");
                                                var commodity = CommodityDefinition.FromEDName(commodityEdName);
                                                var proportion = JsonParsing.getDecimal(commodityData, "Proportion"); // Out of 100
                                                if (commodity != null)
                                                {
                                                    commodity.fallbackLocalizedName = JsonParsing.getString(commodityData, "Name_Localised");
                                                    commodities.Add(new CommodityPresence(commodity, proportion));
                                                }
                                            }
                                        }
                                    }
                                    var content = JsonParsing.getString(data, "Content"); // (a string representing High/Medium/Low material content)
                                    var materialContent = new AsteroidMaterialContent(content)
                                    {
                                        fallbackLocalizedName = JsonParsing.getString(data, "Content_Localised")?.Replace("Material Content: ", "")
                                    };
                                    var remaining = JsonParsing.getDecimal(data, "Remaining"); // Out of 100

                                    // If a motherlode commodity is present
                                    CommodityDefinition motherlodeCommodityDefinition = null;
                                    var motherlodeEDName = JsonParsing.getString(data, "MotherlodeMaterial");
                                    if (!string.IsNullOrEmpty(motherlodeEDName))
                                    {
                                        motherlodeCommodityDefinition = CommodityDefinition.FromEDName(motherlodeEDName);
                                    }

                                    events.Add(new AsteroidProspectedEvent(timestamp, commodities, materialContent, remaining, motherlodeCommodityDefinition) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "QuitACrew":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var captain = JsonParsing.getString(data, "Captain");
                                    captain = captain.Replace("$cmdr_decorate:#name=", "Commander ").Replace(";", "").Replace("&", "Commander ");
                                    var telepresence = EDDI.Instance.inOdyssey ? JsonParsing.getOptionalBool(data, "Telepresence") : true;
                                    events.Add(new CrewLeftEvent(timestamp, captain, telepresence) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "RebootRepair":
                                {
                                    // This event returns a list of slots rather than actual module ednames.
                                    List<string> compartmentsJson = null;
                                    data.TryGetValue( "Modules", out var val );
                                    if ( val is List<string> ls )
                                    {
                                        compartmentsJson = ls;
                                    }
                                    else if ( val is List<object> lo )
                                    {
                                        compartmentsJson = lo.Select( o => o.ToString() ).ToList();
                                    }
                                    events.Add(new ShipRebootedEvent( timestamp, compartmentsJson ) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "ReceiveText":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var from = JsonParsing.getString(data, "From");
                                    var localizedFrom = JsonParsing.getString(data, "From_Localised");
                                    var channel = JsonParsing.getString(data, "Channel");
                                    var message = JsonParsing.getString(data, "Message");
                                    MessageChannel messageChannel;
                                    MessageSource source;

                                    if (from == string.Empty && channel == "npc" && (message.StartsWith("$COMMS_entered") || message.StartsWith("$CHAT_Intro")))
                                    {
                                        // We can safely ignore system messages that initialize the chat system or announce that we entered a channel - no event is needed. 
                                        handled = true;
                                        break;
                                    }

                                    if (
                                        channel == "player" ||
                                        channel == "wing" ||
                                        channel == "friend" ||
                                        channel == "voicechat" ||
                                        channel == "local" ||
                                        channel == "squadron" ||
                                        channel == "starsystem" ||
                                        channel == null
                                    )
                                    {
                                        // Give priority to player messages
                                        if (string.IsNullOrEmpty(channel))
                                        {
                                            // Multicrew messages omit the `channel` property
                                            source = MessageSource.CrewMate;
                                        }
                                        else if (channel == "squadron")
                                        {
                                            source = MessageSource.SquadronMate;
                                        }
                                        else if (channel == "wing")
                                        {
                                            source = MessageSource.WingMate;
                                        }
                                        else
                                        {
                                            source = MessageSource.Commander;
                                        }
                                        messageChannel = MessageChannel.FromEDName(channel ?? "multicrew");
                                        events.Add(new MessageReceivedEvent(timestamp, localizedFrom ?? from, source, true, messageChannel, message) { raw = line, fromLoad = fromLogLoad });
                                    }
                                    else
                                    {
                                        // This is NPC speech.  What's the source?
                                        if (from.Contains("npc_name_decorate"))
                                        {
                                            source = MessageSource.FromMessage(from, message);
                                            from = from.Replace("$npc_name_decorate:#name=", "").Replace(";", "");
                                        }
                                        else if (from.Contains("ShipName_") || from.Contains("_Scenario_"))
                                        {
                                            source = MessageSource.FromMessage(from, message);
                                            if (!string.IsNullOrEmpty(localizedFrom))
                                            {
                                                // This is an NPC with a symbolic name
                                                from = NpcAuthorityShip.EDNameExists(from) 
                                                    ? NpcAuthorityShip.FromEDName(from)?.localizedName 
                                                    : localizedFrom;
                                            }
                                        }
                                        else if (from.StartsWith("$Name_AX_Military; "))
                                        {
                                            source = MessageSource.FromMessage(from, message);
                                            from = from.Replace("$Name_AX_Military; ", "");
                                        }
                                        else if (message.StartsWith("$STATION_") || message.Contains("$Docking"))
                                        {
                                            source = MessageSource.Station;
                                        }
                                        else
                                        {
                                            source = MessageSource.NPC;
                                        }
                                        messageChannel = MessageChannel.FromEDName(channel);
                                        events.Add(new MessageReceivedEvent(timestamp, localizedFrom ?? from, source, false, messageChannel, JsonParsing.getString(data, "Message_Localised"), EDDI.Instance.CurrentStarSystem, EDDI.Instance.CurrentStellarBody, EDDI.Instance.CurrentStation) { raw = line, fromLoad = fromLogLoad });

                                        // See if we also want to spawn a specific event as well?
                                        if (message == "$STATION_NoFireZone_entered;" && EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                                        {
                                            events.Add(new StationNoFireZoneEnteredEvent(timestamp, false) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message == "$STATION_NoFireZone_entered_deployed;")
                                        {
                                            events.Add(new StationNoFireZoneEnteredEvent(timestamp, true) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message == "$STATION_NoFireZone_exited;")
                                        {
                                            events.Add(new StationNoFireZoneExitedEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_StartInterdiction") || message.Contains("_Hitman_Interdiction"))
                                        {
                                            // Find out who is doing the interdicting
                                            source = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCInterdictionCommencedEvent(timestamp, localizedFrom ?? from, source ) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_Attack") || message.Contains("_OnAttackStart") || message.Contains("AttackRun") || message.Contains("OnDeclarePiracyAttack"))
                                        {
                                            // Find out who is doing the attacking
                                            source = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCAttackCommencedEvent(timestamp, localizedFrom ?? from, source ) { raw = line, fromLoad = fromLogLoad });
                                        }
                                        else if (message.Contains("_OnStartScanCargo"))
                                        {
                                            // Find out who is doing the scanning
                                            source = MessageSource.FromMessage(from, message);
                                            events.Add(new NPCCargoScanCommencedEvent(timestamp, localizedFrom ?? from, source ) { raw = line, fromLoad = fromLogLoad });
                                        }
                                    }
                                }
                                handled = true;
                                break;
                            case "RepairDrone":
                                {
                                    var hull = JsonParsing.getOptionalDecimal(data, "HullRepaired");
                                    var cockpit = JsonParsing.getOptionalDecimal(data, "CockpitRepaired");
                                    var corrosion = JsonParsing.getOptionalDecimal(data, "CorrosionRepaired");

                                    events.Add(new ShipRepairDroneEvent(timestamp, hull, cockpit, corrosion) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Resurrect":
                                {
                                    var option = JsonParsing.getString(data, "Option");
                                    var price = JsonParsing.getLong(data, "Cost");
                                    events.Add(new RespawnedEvent(timestamp, option, price) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SelfDestruct":
                                {
                                    events.Add( new SelfDestructEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;
                            case "SendText":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var to = JsonParsing.getString(data, "To");
                                    var message = JsonParsing.getString(data, "Message");
                                    events.Add(new MessageSentEvent(timestamp, to, message) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Shutdown":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    events.Add(new ShutdownEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "Synthesis":
                                {
                                    var synthesis = JsonParsing.getString(data, "Name");

                                    data.TryGetValue("Materials", out var val);
                                    var materials = new List<MaterialAmount>();
                                    // 2.2 style
                                    if (val is Dictionary<string, object> materialsData)
                                    {
                                        if (materialsData != null)
                                        {
                                            foreach (var materialData in materialsData)
                                            {
                                                var material = Material.FromEDName(materialData.Key);
                                                materials.Add(new MaterialAmount(material, (int)(long)materialData.Value));
                                            }
                                        }
                                    }
                                    else if (val is List<object> materialsJson) // 2.3 style
                                    {
                                        foreach (var materialJson in materialsJson.Cast<IDictionary<string, object>>() )
                                        {
                                            var material = Material.FromEDName(JsonParsing.getString(materialJson, "Name"));
                                            materials.Add(new MaterialAmount(material, (int)(long)materialJson["Count"]));
                                        }
                                    }

                                    events.Add(new SynthesisedEvent(timestamp, synthesis, materials) { raw = line, fromLoad = fromLogLoad });
                                }
                                handled = true;
                                break;
                            case "SystemsShutdown":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    if ( ShipShutdownCancellationTokenSource != null )
                                    {
                                        // Ignore repetitions when the ship is already in a shut-down state. 
                                    }
                                    else
                                    {
                                        events.Add( new ShipShutdownEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
                                    }
                                }
                                handled = true;
                                break;
                            case "VehicleSwitch":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    var to = JsonParsing.getString(data, "To");
                                    if (to == "Fighter")
                                    {
                                        events.Add(new ControllingFighterEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                    else if (to == "Mothership")
                                    {
                                        events.Add(new ControllingShipEvent(timestamp) { raw = line, fromLoad = fromLogLoad });
                                        handled = true;
                                    }
                                }
                                break;
                            case "SupercruiseDestinationDrop":
                                {
                                    if ( fromLogLoad ) { handled = true; break; } // Skip handling this during log loading

                                    // "Type" may be either a signal source or proper name.
                                    // If proper name, it might be a fleet carrier with both name and ID.
                                    var type = JsonParsing.getString( data, "Type" );
                                    var typeLocalized = JsonParsing.getString( data, "Type_Localised" );
                                    var threat = JsonParsing.getOptionalInt( data, "Threat" ) ?? 0; // Typically 0 except for USS drops.
                                    var marketID = JsonParsing.getOptionalLong( data, "MarketID" );

                                    if ( type.StartsWith("$") ) 
                                    {
                                        // Symbolic signal source name. Prefer our own localization and fallback using the provided localization string if needed.
                                        var signalSource = SignalSource.FromEDName( type );
                                        signalSource.fallbackLocalizedName = typeLocalized;
                                        type = signalSource.invariantName;
                                        typeLocalized = signalSource.localizedName;
                                    }
                                    else
                                    {
                                        // Destination might be a fleet carrier with name and carrier ID in a single string.
                                        // Check and break apart if needed.
                                        var fleetCarrierRegex = new Regex( "^(.+)(?> )([A-Za-z0-9]{3}-[A-Za-z0-9]{3})$" );
                                        if ( string.IsNullOrEmpty( typeLocalized ) && fleetCarrierRegex.IsMatch( type ) )
                                        {
                                            // Fleet carrier names include both the carrier name and carrier ID, we need to separate them
                                            var fleetCarrierParts = fleetCarrierRegex.Matches( type )[ 0 ].Groups;
                                            if ( fleetCarrierParts.Count == 3 )
                                            {
                                                type = fleetCarrierParts[ 2 ].Value;
                                                typeLocalized = fleetCarrierParts[ 1 ].Value;
                                            }
                                        }
                                    }

                                    events.Add( new DestinationArrivedEvent( timestamp, type, typeLocalized, threat, marketID ) { raw = line, fromLoad = fromLogLoad } );
                                }
                                handled = true;
                                break;

                            #endregion

                            #region Colonisation Events
                            
                            case "ColonisationSystemClaim":
                            case "ColonisationSystemClaimRelease":
                                handled = ColonisationClaimProcessedEvent.Handle( timestamp, edType, line, data, ref events, fromLogLoad );
                                break;
                            case "ColonisationBeaconDeployed":
                                handled = ColonisationBeaconDeployedEvent.Handle( timestamp, line, ref events, fromLogLoad );
                                break;
                            case "ColonisationContribution": 
                                handled = ColonisationContributionEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;
                            case "ColonisationConstructionDepot":
                                handled = ColonisationConstructionDepotEvent.Handle( timestamp, line, data, ref events, fromLogLoad );
                                break;

                            #endregion

                            #region Ignored Events

                            // we silently ignore these, but forward them to the responders
                            case "CodexDiscovery":
                            case "CodexEntry":
                            case "RestockVehicle":
                            case "ScanOrganic":
                            case "SellMicroResources":
                            case "SellOrganicData":

                            // Low priority (for now)
                            case "BuyWeapon":
                            case "CarrierTradeOrder": // Implement when we are ready to handle fleet carrier cargo.
                            case "CarrierModulePack": 
                            case "CarrierShipPack":
                            case "ClearImpound": // Sample: { "timestamp":"2022-10-20T18:01:17Z", "event":"ClearImpound", "ShipType":"asp", "ShipType_Localised":"Asp Explorer", "ShipID":34, "ShipMarketID":3705689344, "MarketID":3705689344 }
                            case "CreateSuitLoadout": 
                            case "DeleteSuitLoadout": 
                            case "LoadoutEquipModule":
                            case "LoadoutRemoveModule":
                            case "RenameSuitLoadout":
                            case "ReservoirReplenished":
                            case "SellSuit":  
                            case "SellWeapon": 
                            case "SuitLoadout":
                            case "SwitchSuitLoadout":
                            case "UpgradeSuit":
                            case "UpgradeWeapon":
                            case "WingAdd":
                            case "WingInvite":
                            case "WingJoin":
                            case "WingLeave":

                            // No plans to support
                            case "CollectItems": // The `BackpackChange` event keeps us up to date.
                            case "Continued": // This indicates that the journal continues in a new file. We should pick this up automatically.
                            case "CrimeVictim": // No need to track crimes committed by other cmdrs. If added, filter out events where the current player is listed as the offender.
                            case "DiscoveryScan": // Probably deprecated / replaced by `FSSDiscoveryScan`
                            case "DropItems": // The `BackpackChange` event keeps us up to date.
                            case "EngineerLegacyConvert": // Unnecessary.
                            case "Resupply": // Seems to be related to resupplying Odyssey backpack items
                            case "ScanBaryCentre": // We do not do anything with scanned barycentres at this time (though the raw event is still passed to the EDDN responder)
                            case "Scanned": // Written at the end of a successful scan, too late to react to this.
                            case "SharedBookmarkToSquadron": // Unnecessary.
                            case "ShipyardRedeem": // Unnecessary, all of the necessary information is already given in the `ShipRedeem` event.
                            case "TradeMicroResources": // This is always followed by `ShipLocker`, which we can use to keep our inventory up to date
                            case "TransferMicroResources": // Removed, no longer written
                            case "UseConsumable": // Seems to include only medkits and energy cells (grenades not included) and it's not needed. The `BackpackChange` event keeps us up to date.
                            case "USSDrop": // Superseded by / redundant with the `SupercruiseDestinationDrop` event.
                            case "WonATrophyForSquadron": // No interesting data here so no reason to add this.
                                break;

                            #endregion

                            default:
                                throw new NotImplementedException($"EDDI has no handler for event type '{edType}'.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // Something went wrong, but an unhandled event will still be passed to the responders.
                        Logging.Warn($"{ex.Message}/r/nRaw event:/r/n{line}", ex);
                    }

                    if (!handled)
                    {
                        Logging.Debug("Unhandled event: " + line);

                        // Pass a basic event so that responders can react appropriately.
                        // For example, the EDSM responder will handle raw events.
                        events.Add(new UnhandledEvent(timestamp, edType) { raw = line, fromLoad = fromLogLoad });
                    }
                }
            }
            catch (JsonReaderException jre)
            {
                Logging.Debug(jre.Message, jre);
                try
                {
                    if ( line.Contains( @"""event"":""BackpackChange""" ) && line.Contains( @"] ""Removed""" ) )
                    {
                        // We've observed a missing comma in the `BackpackChange` event, fix that here.
                        line = line.Replace( @"] ""Removed""", @"], ""Removed""" );
                        return ParseJournalEntry( line, fromLogLoad );
                    }

                    if ( line.Contains( @"""event"":""CarrierNameChange""" ) && line.Contains(@""""": ""SquadronCarrier"",") )
                    {
                        // We've observed a missing CarrierType field name in the `CarrierNameChange` event, fix that here.
                        line = line.Replace( @""""": ""SquadronCarrier"",", @"""CarrierType"": ""SquadronCarrier""," );
                        return ParseJournalEntry( line, fromLogLoad );
                    }
                }
                catch
                {
                    // Unable to recover so re-throw.
                    throw;
                }
            }
            catch (Exception ex)
            {
                Logging.Error($"Exception whilst parsing journal line {line}", ex);
            }
            return events;
        }

        public string MonitorName()
        {
            return "Journal monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.JournalMonitor.name;
        }

        public string MonitorDescription()
        {
            return Properties.JournalMonitor.desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public bool NeedsStart()
        {
            return true;
        }

        public void Start()
        {
            start();
        }

        public void Stop()
        {
            foreach (var carrierJumpCancellationTS in carrierJumpCancellationTokenSources.Values) { carrierJumpCancellationTS.Cancel(); }
            stop();
        }

        public void Reload() { }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        private static string GetSavedGamesDir()
        {
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        internal class NativeMethods
        {
            [DllImport("Shell32.dll")]
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }

        public void PreHandle(Event @event)
        { }

        public void PostHandle(Event @event)
        { }

        public void HandleProfile(JObject profile)
        { }

        public void HandleStatus ( Status status )
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
        {
            return null;
        }
    }
}
