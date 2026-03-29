using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ColonisationBeaconDeployedEvent ( DateTime timestamp ) : Event( timestamp, NAME )
    {
        public const string NAME = "Colonisation beacon deployed";
        public const string DESCRIPTION = "Triggered when you deploy a colonisation beacon";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"": ""2025-04-09T08:09:37Z"", ""event"": ""ColonisationBeaconDeployed"" }"
        ];

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            if ( fromLogLoad ) { return true; } // Skip handling this during log loading

            events.Add( new ColonisationBeaconDeployedEvent( timestamp ) { raw = line, fromLoad = false } );
            return true;
        }
    }
}
