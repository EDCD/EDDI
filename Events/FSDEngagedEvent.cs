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
        public string system { get; private set; }

        public FSDEngagedEvent(DateTime timestamp, string jumptype, string system, string stellarclass) : base(timestamp, NAME)
        {
            this.target = jumptype;
            this.system = system;
            this.stellarclass = stellarclass;
        }
    }
}
