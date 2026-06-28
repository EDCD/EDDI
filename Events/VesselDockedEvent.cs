using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VesselDockedEvent ( DateTime timestamp, VesselDefinition vesselDefinition, int id )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Vessel docked";
        public const string DESCRIPTION = "Triggered when you dock a vessel (fighter, SRV, etc) with your ship";
        public static readonly string[] SAMPLES = 
        { 
            @"{ ""timestamp"":""2022-11-24T23:45:10Z"", ""event"":""DockSRV"", ""SRVType"":""combat_multicrew_srv_01"", ""SRVType_Localised"":""SRV Scorpion"", ""ID"":53 }",
            @"{ ""timestamp"":""2026-06-22T20:52:23Z"", ""event"":""DockSRV"", ""SRVType"":""lander01"", ""SRVType_Localised"":""Nomad"", ""ID"":85 }",
            @"{ ""timestamp"":""2026-05-31T23:40:53Z"", ""event"":""DockFighter"", ""ID"":95 }"
        };

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

            var vesselGroup = edType.Replace("Dock", "").ToLowerInvariant(); // e.g. DockFighter or DockSRV
            var id = JsonParsing.getInt(data, "ID");

            VesselDefinition vesselDefinition = null;
            if ( vesselGroup == "srv" )
            {
                vesselDefinition = VesselDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                vesselDefinition.fallbackLocalizedName = JsonParsing.getString( data, "SRVType_Localised" );
            }

            events.Add( new VesselDockedEvent( timestamp, vesselDefinition, id ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
