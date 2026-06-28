using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class VesselDestroyedEvent (
        DateTime timestamp,
        VesselDefinition vesselDefinition,
        int id )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Vessel destroyed";
        public const string DESCRIPTION = "Triggered when your vessel (fighter, SRV, etc.) is destroyed";
        public static readonly string[] SAMPLES = 
        {
            @"{ ""timestamp"":""2026-06-17T06:00:18Z"", ""event"":""FighterDestroyed"", ""ID"":95 }",
            @"{ ""timestamp"":""2025-09-08T02:23:24Z"", ""event"":""SRVDestroyed"", ""ID"":131, ""SRVType"":""testbuggy"", ""SRVType_Localised"":""SRV Scarab"" }",
            @"{ ""timestamp"":""2026-06-23T05:02:23Z"", ""event"":""SRVDestroyed"", ""ID"":329, ""SRVType"":""lander01"", ""SRVType_Localised"":""Nomad"" }"
        };

        [PublicAPI( "The ID assigned to the vessel" )]
        public int id { get; private set; } = id;

        [PublicAPI( "The localized vessel description (not available for fighters)" )]
        public string vesselDescription => vesselDefinition?.localizedName;

        [PublicAPI( "The invariant vessel description (not available for fighters)" )]
        public string vesselDescriptionInvariant => vesselDefinition?.invariantName;

        [PublicAPI( "Whether the vessel is controlled via telepresence" )]
        public bool isTelepresence =>  vesselDefinition?.vesselGroup != VesselGroup.Piloted; // vesselDefinition may be null for fighters

        // Not intended to be public facing at this time
        public VesselDefinition vesselDefinition { get; private set; } = vesselDefinition;

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            var vesselGroup = edType.Replace("Destroyed", "").ToLowerInvariant(); // e.g. FighterDestroyed or SRVDestroyed
            var id = JsonParsing.getInt(data, "ID");

            if ( vesselGroup == "fighter" )
            {
                events.Add( new VesselDestroyedEvent( timestamp, null, id ) { raw = line, fromLoad = false } );
            }

            if ( vesselGroup == "srv" )
            {
                var vesselDefinition = VesselDefinition.FromEDName(JsonParsing.getString(data, "SRVType"));
                vesselDefinition.fallbackLocalizedName = JsonParsing.getString( data, "SRVType_Localised" );
                events.Add( new VesselDestroyedEvent( timestamp, vesselDefinition, id ) { raw = line, fromLoad = false } );
            }

            return true;
        }
    }
}
