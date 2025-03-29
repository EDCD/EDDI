using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FSDEngagedEvent : Event
    {
        public const string NAME = "FSD engaged";
        public const string DESCRIPTION = "Triggered when your FSD has engaged";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2016-08-09T08:46:29Z"",""event"":""StartJump"",""JumpType"":""Hyperspace"",""StarClass"":""L"",""StarSystem"":""LFT 926""}",
            @"{ ""timestamp"":""2024-11-10T05:18:53Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""Taxi"":false, ""StarSystem"":""La Rochelle"", ""SystemAddress"":9467047454121, ""StarClass"":""M"" }",
            @"{ ""timestamp"":""2024-11-10T05:49:23Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""Taxi"":false, ""StarSystem"":""Carener"", ""SystemAddress"":8879945552602, ""StarClass"":""K"" }",
            @"{ ""timestamp"":""2023-08-13T09:44:18Z"", ""event"":""StartJump"", ""JumpType"":""Hyperspace"", ""Taxi"":true, ""StarSystem"":""LHS 547"", ""SystemAddress"":7268024133033, ""StarClass"":""M"" }"
        };

        [PublicAPI("The target frame (Supercruise/Hyperspace)")]
        public string target { get; private set; }

        [PublicAPI("The class of the destination primary star (only if type is Hyperspace)")]
        public string stellarclass { get; private set; }

        [PublicAPI("The destination system (only if type is Hyperspace)")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the destination star system (only if type is Hyperspace)" )]
        public ulong systemAddress { get; private set; } // Only set when the fsd target is hyperspace

        [PublicAPI( "True if traveling via taxi" )]
        public bool taxijump { get; private set; }

        // Not intended to be user facing

        [ Obsolete ] public string system => systemname;

        public FSDEngagedEvent(DateTime timestamp, string jumptype, string systemName, ulong systemAddress, string stellarclass, bool isTaxi) : base(timestamp, NAME)
        {
            this.target = jumptype;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.stellarclass = stellarclass;
            this.taxijump = isTaxi;
        }
    }
}
