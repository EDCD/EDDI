using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ShipInterdictedEvent (
        DateTime timestamp,
        bool succeeded,
        bool submitted,
        bool iscommander,
        bool isThargoid,
        string interdictor,
        CombatRating rating,
        string faction,
        string power )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Ship interdicted";
        public const string DESCRIPTION = "Triggered when your ship is interdicted by another ship";
        public const string SAMPLE = "{\"timestamp\":\"2016-09-21T07:00:17Z\",\"event\":\"Interdicted\",\"Submitted\":true,\"Interdictor\":\"Torval's Shield\",\"IsPlayer\":false,\"Faction\":\"Zemina Torval\",\"Power\":\"Empire\"}";

        [PublicAPI("If the interdiction attempt was successful")]
        public bool succeeded { get; private set; } = succeeded;

        [PublicAPI("If the commander submitted to the interdiction")]
        public bool submitted { get; private set; } = submitted;

        [PublicAPI("If the player carrying out the interdiction is a commander (as opposed to an NPC)")]
        public bool iscommander { get; private set; } = iscommander;

        [PublicAPI( "If a Thargoid is carrying out the interdiction" )]
        public bool isthargoid { get; private set; } = isThargoid;

        [PublicAPI("The name of the commander or NPC carrying out the interdiction")]
        public string interdictor { get; private set; } = interdictor;

        [PublicAPI("The combat rating of the commander or NPC carrying out the interdiction")]
        public string rating { get; private set; } = rating?.localizedName;

        [PublicAPI("The faction of the NPC carrying out the interdiction")]
        public string faction { get; private set; } = faction;

        [PublicAPI("The power of the NPC carrying out the interdiction")]
        public string power { get; private set; } = power;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var succeded = edType == "Interdicted";
            var submitted = JsonParsing.getOptionalBool( data, "Submitted" ) ?? false;
            var interdictor = JsonParsing.getString( data, "Interdictor" );
            var iscommander = JsonParsing.getOptionalBool( data, "IsPlayer" ) ?? false;
            var isThargoid = JsonParsing.getOptionalBool( data, "isThargoid" ) ?? false;

            if ( !string.IsNullOrEmpty( JsonParsing.getString( data, "Interdictor_Localised" ) ) )
            {
                // This is an NPC with a symbolic name
                interdictor = NpcAuthorityShip.EDNameExists( interdictor )
                    ? NpcAuthorityShip.FromEDName( interdictor )?.localizedName
                    : JsonParsing.getString( data, "Interdictor_Localised" );
            }
            else if ( isThargoid )
            {
                interdictor = NpcAuthorityShip.Thargoid.localizedName;
            }
            else if ( string.IsNullOrEmpty( interdictor ) && !data.ContainsKey( "Interdictor" ) )
            {
                // This matches the pattern for an unknown ship interdiction attempt
                interdictor = NpcAuthorityShip.UNKNOWN.localizedName;
            }

            var rank = JsonParsing.getOptionalInt( data, "CombatRank" );
            var rating = rank is null ? null : CombatRating.FromRank( (int)rank );
            var faction = EventParsing.FactionName( data, "Faction" );
            var power = JsonParsing.getString( data, "Power" );

            events.Add( new ShipInterdictedEvent( timestamp, succeded, submitted, iscommander, isThargoid, interdictor, rating, faction, power ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
