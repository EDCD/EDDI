using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class PermitAcquiredEvent ( DateTime timestamp, string faction ) : Event( timestamp, NAME )
    {
        public const string NAME = "Permit acquired";
        public const string DESCRIPTION = "Triggered when you acquire a permit from the mission board";
        public const string SAMPLE = "{ \"timestamp\":\"2021-10-20T21:02:15Z\", \"event\":\"MissionAccepted\", \"Faction\":\"Hodack Prison Colony\", \"Name\":\"MISSION_genericPermit1\", \"LocalisedName\":\"Permit Acquisition Opportunity\", \"Wing\":false, \"Influence\":\"None\", \"Reputation\":\"None\", \"MissionID\":814712321 }";

        [PublicAPI("The faction issuing the permit")]
        public string faction { get; private set; } = faction;
    }
}
