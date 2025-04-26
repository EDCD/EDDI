using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ColonisationBeaconDeployedEvent : Event
    {
        public const string NAME = "Colonisation beacon deployed";
        public const string DESCRIPTION = "Triggered when you deploy a colonisation beacon";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"": ""2025-04-09T08:09:37Z"", ""event"": ""ColonisationBeaconDeployed"" }"
        };

        public ColonisationBeaconDeployedEvent ( DateTime timestamp ) : base( timestamp, NAME )
        { }

        public static bool Handle ( DateTime timestamp, string line, ref List<Event> events, bool fromLogLoad )
        {
            events.Add( new ColonisationBeaconDeployedEvent( timestamp ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
