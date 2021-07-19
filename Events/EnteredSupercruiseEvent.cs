using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class EnteredSupercruiseEvent : Event
    {
        public const string NAME = "Entered supercruise";
        public const string DESCRIPTION = "Triggered when your ship enters supercruise";
        public const string SAMPLE = "{ \"timestamp\":\"2021-07-19T05:28:08Z\", \"event\":\"SupercruiseEntry\", \"Taxi\":false, \"Multicrew\":false, \"StarSystem\":\"Azaladshu\", \"SystemAddress\":9467852826025 }";

        [PublicAPI("The system at which the commander has entered supercruise")]
        public string system { get; private set; }

        [PublicAPI("True if the ship is a transport (e.g. taxi or dropship)")]
        public bool? taxi { get; private set; }

        [PublicAPI("True if the ship is belongs to another player")]
        public bool? multicrew { get; private set; }

        // Not intended to be user facing

        public long? systemAddress { get; private set; }

        public EnteredSupercruiseEvent(DateTime timestamp, string system, long? systemAddress, bool? taxi, bool? multicrew) : base(timestamp, NAME)
        {
            this.system = system;
            this.systemAddress = systemAddress;
            this.taxi = taxi;
            this.multicrew = multicrew;
        }
    }
}
