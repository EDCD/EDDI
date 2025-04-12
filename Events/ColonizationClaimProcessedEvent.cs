using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ColonizationClaimProcessedEvent : Event
    {
        public const string NAME = "Colonisation claim processed";
        public const string DESCRIPTION = "Triggered when staking or releasing a claim to colonise a star system";
        public static readonly string[] SAMPLES =
        {
            @"{ ""timestamp"":""2025-04-09T03:45:54Z"", ""event"":""ColonisationSystemClaim"", ""StarSystem"":""Col 285 Sector PT-Q c5-24"", ""SystemAddress"":6680653861562 }",
            @"{ ""timestamp"":""2025-04-09T12:33:19Z"", ""event"":""ColonisationSystemClaimRelease"", ""StarSystem"":""Col 285 Sector HM-I b11-2"", ""SystemAddress"":5070075078009 }"
        };

        [PublicAPI("The name of the claimed star system")]
        public string systemname { get; private set; }

        [PublicAPI( "The numeric system address of the claimed star system" )]
        public ulong systemAddress { get; private set; }

        [PublicAPI("True when staking a claim and false when releasing a claim")]
        public bool claimStaked { get; private set; }

        public ColonizationClaimProcessedEvent ( DateTime timestamp, string systemname, ulong systemAddress, bool claimStaked ) : base( timestamp, NAME )
        {
            this.systemname = systemname;
            this.systemAddress = systemAddress;
            this.claimStaked = claimStaked;
        }
    }
}
