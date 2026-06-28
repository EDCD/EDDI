using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipTargetedEvent (
        DateTime timestamp,
        bool targetlocked,
        Ship shipDef,
        VesselDefinition fighterDef,
        int? scanstage,
        string name,
        CombatRating rank,
        string faction,
        Power power,
        LegalStatus legalstatus,
        int? bounty,
        decimal? shieldhealth,
        decimal? hullhealth,
        string subsystem,
        decimal? subsystemhealth )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ship targeted";
        public const string DESCRIPTION = "Triggered when the player selects a non-Thargoid target";
        public static readonly string[] SAMPLES =
        [
            "{ \"timestamp\":\"2020-05-16T08:14:36Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":0 }",
            "{ \"timestamp\":\"2020-05-16T08:14:37Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":1, \"PilotName\":\"$npc_name_decorate:#name=Noni Ryder;\", \"PilotName_Localised\":\"Noni Ryder\", \"PilotRank\":\"Mostly Harmless\" }",
            "{ \"timestamp\":\"2020-05-16T08:14:39Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":2, \"PilotName\":\"$npc_name_decorate:#name=Noni Ryder;\", \"PilotName_Localised\":\"Noni Ryder\", \"PilotRank\":\"Mostly Harmless\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000 }",
            "{ \"timestamp\":\"2020-05-16T08:14:45Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"sidewinder\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Noni Ryder;\", \"PilotName_Localised\":\"Noni Ryder\", \"PilotRank\":\"Mostly Harmless\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Balante Jet Posse\", \"LegalStatus\":\"Wanted\", \"Bounty\":1642 }",
            "{ \"timestamp\":\"2018-05-09T23:19:49Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"adder\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Phoenix;\", \"PilotName_Localised\":\"Phoenix\", \"PilotRank\":\"Competent\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Union Cosmos\", \"LegalStatus\":\"Lawless\", \"Subsystem\":\"$int_powerplant_size3_class3_name;\", \"Subsystem_Localised\":\"Power Plant\", \"SubsystemHealth\":100.000000}"
        ];

        [PublicAPI("True when a ship has been targeted. False when a target has been lost/deselected")]
        public bool targetlocked { get; private set; } = targetlocked;

        [ PublicAPI( "the model of the ship" ) ]
        public string ship => FighterDef != null ? FighterDef.localizedName : ShipDef?.model;

        [PublicAPI("the stage of the ship scan (e.g. 0, 1, 2, or 3)")]
        public int? scanstage { get; private set; } = scanstage;

        [PublicAPI("The name of the pilot (at scan state 1+)")]
        public string name { get; private set; } = name;

        [PublicAPI( "The rank of the pilot (at scan state 1+)" )]
        public string rank => CombatRank?.localizedName;

        [PublicAPI( "The health of the shields (at scan state 2+)" )]
        public decimal? shieldhealth { get; private set; } = shieldhealth;

        [PublicAPI( "The health of the hull (at scan state 2+)" )]
        public decimal? hullhealth { get; private set; } = hullhealth;

        [PublicAPI( "The faction of the pilot (at scan state 3)" )]
        public string faction { get; private set; } = faction;

        [PublicAPI( "The aligned power of the pilot (if player is pledged) (at scan state 3)" )]
        public string power => (Power ?? Power.None)?.localizedName;

        [PublicAPI( "The legal status of the pilot (at scan state 3)" )]
        public string legalstatus => LegalStatus?.localizedName;

        [PublicAPI( "The bounty being offered by system authorities for destruction of the ship (at scan state 3)" )]
        public int? bounty { get; private set; } = bounty;

        [ PublicAPI( "The subsystem targeted (at scan state 3)" ) ]
        public string subsystem { get; private set; } = subsystem;

        [PublicAPI( "The health of the subsystem targeted (at scan state 3)" )]
        public decimal? subsystemhealth { get; private set; } = subsystemhealth;

        // Not intended to be user facing

        public CombatRating CombatRank { get; } = rank;

        public LegalStatus LegalStatus { get; } = legalstatus;

        public Power Power { get; } = power;

        public Ship ShipDef { get; } = shipDef;

        public VesselDefinition FighterDef { get; } = fighterDef;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var targetlocked = JsonParsing.getBool(data, "TargetLocked");

            // Target locked
            var scanstage = JsonParsing.getOptionalInt(data, "ScanStage");
            VesselDefinition fighterDef = null;
            Ship shipDef = null;
            var vehicleEDName = JsonParsing.getString(data, "Ship");
            if ( vehicleEDName != null )
            {
                if ( vehicleEDName.Contains( "fighter", StringComparison.InvariantCultureIgnoreCase ) )
                {
                    fighterDef = VesselDefinition.FromEDName( vehicleEDName );
                    fighterDef.fallbackLocalizedName = JsonParsing.getString( data, "Ship_Localised" );
                }
                else
                {
                    shipDef = ShipDefinitions.FromEDModel( vehicleEDName, false );
                    if ( shipDef is null )
                    {
                        shipDef = ShipDefinitions.FromEDModel( vehicleEDName, true );
                        shipDef.model = JsonParsing.getString( data, "Ship_Localised" );
                    }
                }
            }

            // Scan stage >= 1
            var name = JsonParsing.getString(data, "PilotName");
            if ( !string.IsNullOrEmpty( JsonParsing.getString( data, "PilotName_Localised" ) ) )
            {
                // This is an NPC with a symbolic name
                name = NpcAuthorityShip.EDNameExists( name )
                    ? NpcAuthorityShip.FromEDName( name )?.localizedName
                    : JsonParsing.getString( data, "PilotName_Localised" );
            }

            // Sometimes we don't get a localized name when we ought to.
            // Strip out any remaining unlocalized content in the name.
            if ( !string.IsNullOrEmpty( name ) && GeneratedRegex.UnlocalizedEdNameRegex().IsMatch( name ) )
            {
                var tidiedName = GeneratedRegex.UnlocalizedEdNameRegex().Replace( name, "" ).Trim();
                if ( !string.IsNullOrEmpty( tidiedName ) )
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
                if ( string.IsNullOrEmpty( subsystemName ) )
                {
                    subsystemName = JsonParsing.getString( data, "Subsystem_Localised" );
                }
                subSystemHealth = JsonParsing.getOptionalDecimal( data, "SubsystemHealth" );
            }

            events.Add( new ShipTargetedEvent( timestamp, targetlocked, shipDef, fighterDef, scanstage, name, rank, faction, power, legalStatus, bounty, shieldHealth, hullHealth, subsystemName, subSystemHealth ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
