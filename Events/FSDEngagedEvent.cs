using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FSDEngagedEvent : Event
    {
        public const string NAME = "FSD engaged";
        public const string DESCRIPTION = "Triggered when your FSD has engaged";
        public const string SAMPLE = @"{""timestamp"":""2016-08-09T08:46:29Z"",""event"":""StartJump"",""JumpType"":""Hyperspace"",""StarClass"":""L"",""StarSystem"":""LFT 926""}";

        [PublicAPI("The target frame (Supercruise/Hyperspace)")]
        public string target { get; private set; }

        [PublicAPI("The class of the destination primary star (only if type is Hyperspace)")]
        public string stellarclass { get; private set; }

        [PublicAPI("The destination system (only if type is Hyperspace)")]
        public string systemname { get; private set; }

        [PublicAPI( "True if traveling via taxi" )]
        public bool taxijump { get; private set; }

        // Not intended to be user facing

        [ Obsolete ] public string system => systemname;

        public ulong? systemAddress { get; private set; } // Only set when the fsd target is hyperspace

        public FSDEngagedEvent(DateTime timestamp, string jumptype, string systemName, ulong? systemAddress, string stellarclass, bool isTaxi) : base(timestamp, NAME)
        {
            this.target = jumptype;
            this.systemname = systemName;
            this.systemAddress = systemAddress;
            this.stellarclass = stellarclass;
            this.taxijump = isTaxi;
        }
    }
}
