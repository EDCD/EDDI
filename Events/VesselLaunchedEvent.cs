using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VesselLaunchedEvent (
        DateTime timestamp,
        string loadout,
        bool playercontrolled,
        VesselDefinition vesselDefinition,
        int id )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Vessel launched";
        public const string DESCRIPTION = "Triggered when you launch a vessel (fighter, SRV, etc.) from your ship";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2022-11-24T23:44:25Z"", ""event"":""LaunchSRV"", ""SRVType"":""combat_multicrew_srv_01"", ""SRVType_Localised"":""SRV Scorpion"", ""Loadout"":""default"", ""ID"":53, ""PlayerControlled"":true }",
            @"{ ""timestamp"":""2026-06-22T20:49:11Z"", ""event"":""LaunchFighter"", ""Loadout"":""base"", ""ID"":85, ""PlayerControlled"":true }",
            @"{ ""timestamp"":""2026-06-27T07:17:08Z"", ""event"":""LaunchFighter"", ""Loadout"":""one"", ""ID"":96, ""PlayerControlled"":false }"
        };

        [PublicAPI("The vessel's loadout")]
        public string loadout { get; private set; } = loadout;

        [PublicAPI("True if the vessel is controlled by the player")]
        public bool playercontrolled { get; private set; } = playercontrolled;

        [PublicAPI( "The ID assigned to the vessel" )]
        public int id { get; private set; } = id;

        [PublicAPI( "The localized vessel description (not available for fighters)" )]
        public string vesselDescription => vesselDefinition?.localizedName;

        [PublicAPI( "The invariant vessel description (not available for fighters)" )]
        public string vesselDescriptionInvariant => vesselDefinition?.invariantName;

        [PublicAPI( "Whether the vessel is controlled via telepresence" )]
        public bool isTelepresence => vesselDefinition?.vesselGroup != VesselGroup.Piloted; // vesselDefinition may be null for fighters

        // Not intended to be public facing at this time
        public VesselDefinition vesselDefinition { get; private set; } = vesselDefinition;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var loadout = JsonParsing.getString(data, "Loadout");
            var playercontrolled = JsonParsing.getBool(data, "PlayerControlled");
            var id = JsonParsing.getInt(data, "ID");
            var vesselGroup = edType.Replace("Launch", "").ToLowerInvariant(); // e.g. LaunchFighter or LaunchSRV

            VesselDefinition vesselDefinition = null;
            if ( vesselGroup == "srv" )
            {
                var edName = JsonParsing.getString(data, "SRVType");
                vesselDefinition = VesselDefinition.FromEDName( edName ) ?? new VesselDefinition( edName, VesselGroup.Piloted );
                vesselDefinition.fallbackLocalizedName = JsonParsing.getString( data, "SRVType_Localised" );
            }

            events.Add( new VesselLaunchedEvent( timestamp, loadout, playercontrolled, vesselDefinition, id ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
