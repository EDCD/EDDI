using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FSDEngagedEvent (
        DateTime timestamp,
        string jumptype,
        string systemName,
        ulong? systemAddress,
        string stellarclass,
        bool isTaxi )
        : Event( timestamp, NAME )
    {
        public const string NAME = "FSD engaged";
        public const string DESCRIPTION = "Triggered when your FSD has engaged";
        public static readonly string[] SAMPLES =
        [
            @"{ ""timestamp"":""2016-08-09T08:46:29Z"",""event"":""StartJump"",""JumpType"":""Hyperspace"",""StarClass"":""L"",""StarSystem"":""LFT 926""}",
            @"{ ""timestamp"":""2024-11-10T05:18:53Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""Taxi"":false, ""StarSystem"":""La Rochelle"", ""SystemAddress"":9467047454121, ""StarClass"":""M"" }",
            @"{ ""timestamp"":""2024-11-10T05:49:23Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""Taxi"":false, ""StarSystem"":""Carener"", ""SystemAddress"":8879945552602, ""StarClass"":""K"" }",
            @"{ ""timestamp"":""2023-08-13T09:44:18Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""Taxi"":true, ""StarSystem"":""LHS 547"", ""SystemAddress"":7268024133033, ""StarClass"":""M"" }"
        ];

        [PublicAPI("The target frame (Supercruise/Hyperspace)")]
        public string target { get; private set; } = jumptype;

        [PublicAPI("The class of the destination primary star (only if type is Hyperspace)")]
        public string stellarclass { get; private set; } = stellarclass;

        [PublicAPI("The destination system (only if type is Hyperspace)")]
        public string systemname { get; private set; } = systemName;

        [PublicAPI( "The numeric system address of the destination star system (only if type is Hyperspace)" )]
        public ulong? systemAddress { get; private set; } = systemAddress; // Only set when the fsd target is hyperspace

        [PublicAPI( "Metadata encoded into the unique 64 bit ID for the star system." )]
        public StarSystemId64 id64 => systemAddress is null ? null : new StarSystemId64( (ulong)systemAddress );

        [PublicAPI( "True if traveling via taxi" )]
        public bool taxijump { get; private set; } = isTaxi;

        // Not intended to be user facing

        [ Obsolete("Please prefer using systemname. This obsolete property may still be used in player Cottle scripts") ] 
        public string system => systemname;

        public static bool Handle ( DateTime timestamp, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var target = JsonParsing.getString(data, "JumpType");
            var stellarclass = JsonParsing.getString(data, "StarClass");
            var system = JsonParsing.getString(data, "StarSystem");
            var systemAddress = JsonParsing.getOptionalULong(data, "SystemAddress"); // Present only when the FSD target is hyperspace
            var isTaxi = JsonParsing.getOptionalBool( data, "Taxi" ) ?? false;
            events.Add( new FSDEngagedEvent( timestamp, target, system, systemAddress, stellarclass, isTaxi ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
