using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VesselLaunchedEvent (
        DateTime timestamp,
        LoadoutDescription loadoutDesc,
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
            @"{ ""timestamp"":""2026-09-04T08:25:49Z"", ""event"":""LaunchVessel"", ""VesselType"":""lander01"", ""VesselType_Localised"":""Nomad"", ""Loadout"":""base"", ""ID"":138, ""PlayerControlled"":true }",
            @"{ ""timestamp"":""2026-09-04T08:32:48Z"", ""event"":""LaunchFighter"", """":""independent_fighter"", ""_Localised"":""Taipan"", ""Loadout"":""one"", ""ID"":138, ""PlayerControlled"":true }"
        };

        [PublicAPI( "The localized vessel description" )]
        public string vesselDescription => vesselDefinition?.localizedName;

        [PublicAPI( "The invariant vessel description" )]
        public string vesselDescriptionInvariant => vesselDefinition?.invariantName;

        [PublicAPI( "The vessel's loadout type" )]
        public string loadout => LoadoutDescription?.edname;

        [PublicAPI( "The vessel's localized loadout description" )]
        public string loadoutDescription => LoadoutDescription?.localizedName;

        [PublicAPI( "The ID assigned to the vessel" )]
        public int id { get; private set; } = id;

        [PublicAPI( "True if the vessel is controlled by the player" )]
        public bool playercontrolled { get; private set; } = playercontrolled;

        [PublicAPI( "Whether the vessel is controlled via telepresence" )]
        public bool isTelepresence => vesselDefinition?.vesselGroup != VesselGroup.Piloted;

        // Not intended to be public facing at this time
        public VesselDefinition vesselDefinition { get; private set; } = vesselDefinition;

        public LoadoutDescription LoadoutDescription { get; private set; } = loadoutDesc;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var loadoutEDName = JsonParsing.getString(data, "Loadout");
            var playercontrolled = JsonParsing.getBool(data, "PlayerControlled");
            var id = JsonParsing.getInt(data, "ID");
            var vesselTypeKey = edType.Replace("Launch", "") + "Type"; // e.g. FighterType, SRVType, or VesselType

            // We've observed missing `FighterType` and `FighterType_Localised` field names in the `LaunchFighter` event.
            if ( data.ContainsKey( "" ) && !data.ContainsKey( $"{vesselTypeKey}" ) )
            {
                vesselTypeKey = "";
            }

            string edName = JsonParsing.getString( data, $"{vesselTypeKey}" );
            VesselDefinition vesselDefinition = VesselDefinition.FromEDName( edName );
            if ( vesselDefinition is not null )
            {
                vesselDefinition.fallbackLocalizedName = JsonParsing.getString( data, $"{vesselTypeKey}_Localised" );
            }
            var loadoutDescription = LoadoutDescription.FromVesselAndLoadoutEDName( edName, loadoutEDName );

            events.Add( new VesselLaunchedEvent( timestamp, loadoutDescription, playercontrolled, vesselDefinition, id ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
