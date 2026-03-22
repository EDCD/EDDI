using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CommanderContinuedEvent (
        DateTime timestamp,
        string commander,
        string frontierId,
        bool horizons,
        bool odyssey,
        long? shipId,
        string shipEdModel,
        string shipName,
        string shipIdent,
        bool? startedLanded,
        bool? startDead,
        GameMode mode,
        string group,
        long credits,
        long loan,
        decimal? fuel,
        decimal? fuelcapacity,
        string version,
        string build )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Commander continued";
        public const string DESCRIPTION = "Triggered when you continue an existing game";
        public const string SAMPLE = "{\"timestamp\":\"2016-06-10T14:32:03Z\",\"event\":\"LoadGame\",\"Commander\":\"HRC1\",\"FID\":\"F44396\",\"Horizons\":true,\"Ship\":\"CobraMkIII\",\"ShipID\":1,\"GameMode\":\"Group\",\"Group\":\"Mobius\",\"Credits\":600120,\"Loan\":0,\"ShipName\":\"jewel of parhoon\",\"ShipIdent\":\"hr-17f\",\"FuelLevel\":3.964024,\"FuelCapacity\":8}";

        [PublicAPI("The commander's name")]
        public string commander { get; private set; } = commander;

        [PublicAPI("The game version includes the 'Horizons' DLC")]
        public bool horizons { get; private set; } = horizons;

        [PublicAPI("The game version includes the 'Odyssey' DLC")]
        public bool odyssey { get; private set; } = odyssey;

        [PublicAPI("The commander's ship")]
        public string ship => shipEDModel == "TestBuggy" ? Constants.VEHICLE_SRV
            : shipEDModel.Contains("fighter", StringComparison.OrdinalIgnoreCase) ? Constants.VEHICLE_FIGHTER
            : shipEDModel.Contains("suit", StringComparison.OrdinalIgnoreCase ) ? Constants.VEHICLE_LEGS
            : shipEDModel.Contains("taxi", StringComparison.OrdinalIgnoreCase ) ? Constants.VEHICLE_TAXI
            : ShipDefinitions.FromEDModel(shipEDModel, false)?.model;

        [PublicAPI("The ID of the commander's ship")]
        public long? shipid { get; private set; } = shipId; // this serves double duty in the journal - for ships it is the localId (an integer value). For suits, it is the suit ID (a long).

        [PublicAPI("The game mode (Open, Group or Solo)")]
        public string mode { get; private set; } = (mode?.localizedName);

        [PublicAPI("The name of the group (only if mode == Group)")]
        public string group { get; private set; } = group;

        [PublicAPI("The number of credits the commander has")]
        public long credits { get; private set; } = credits;

        [PublicAPI("The current loan the commander has")]
        public long loan { get; private set; } = loan;

        [PublicAPI("The current fuel level of the commander's vehicle")]
        public decimal? fuel { get; private set; } = fuel;

        [PublicAPI("The total fuel capacity of the commander's vehicle")]
        public decimal? fuelcapacity { get; private set; } = fuelcapacity;

        [PublicAPI("True if the commander is starting landed")]
        public bool? startlanded { get; private set; } = startedLanded;

        [PublicAPI("True if the commander is starting dead / at the rebuy screen")]
        public bool? startdead { get; private set; } = startDead;

        // Not intended to be user facing

        public string shipname { get; private set; } = shipName;

        public string shipident { get; private set; } = shipIdent;

        public string frontierID { get; private set; } = frontierId;

        public string shipEDModel { get; private set; } = shipEdModel;

        public string gameversion { get; private set; } = version;

        public string gamebuild { get; private set; } = build;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var commander = JsonParsing.getString(data, "Commander");
            var frontierID = JsonParsing.getString(data, "FID");

            // Active expansions
            var horizons = JsonParsing.getOptionalBool(data, "Horizons") ?? false; // Whether the account has the Horizons DLC
            var odyssey = JsonParsing.getOptionalBool(data, "Odyssey") ?? false; // Whether the account has the Odyssey DLC
            Logging.Info( $"Active expansions... Horizons: {horizons}, Odyssey: {odyssey}." );

            var shipEDModel = JsonParsing.getString(data, "Ship"); // This describes a vehicle, whether ship or otherwise.
                                                                   // If on foot this may be a suit & if in an SRV then this may be an SRV.
            var shipName = JsonParsing.getString(data, "ShipName");
            var shipIdent = JsonParsing.getString(data, "ShipIdent");
            var shipId = JsonParsing.getOptionalLong(data, "ShipID"); // If on foot we'll get a suit ID here, which we need to treat as a long

            // shipId may be null either if we're logging into CQC or if we're logging in while in an Apex taxi service
            if ( shipId == null )
            {
                if ( !string.IsNullOrEmpty( shipEDModel ) && shipEDModel.Contains( "taxi", StringComparison.OrdinalIgnoreCase ) )
                {
                    // This is a taxi
                }
                else
                {
                    // The LoadGame event for entering CQC contains no ship details.
                    // We are entering CQC. Flag it back to EDDI so we can ignore everything that happens until
                    // we're out of CQC again
                    events.Add( new EnteredCQCEvent( timestamp, commander ) { raw = line, fromLoad = fromLogLoad } );
                    return true;
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

            events.Add( new CommanderContinuedEvent( timestamp, commander, frontierID, horizons, odyssey, shipId, shipEDModel, shipName, shipIdent, startedLanded, startDead, mode, group, credits, loan, fuel, fuelCapacity, version, build ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
