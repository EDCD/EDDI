using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class Faction
    {

        [PublicAPI( "The name of the faction" )]
        public string name { get; set; }

        [ PublicAPI( "The allegiance of the faction, if known, as an object" ) ]
        public Superpower Allegiance
        {
            get => _allegiance ?? Superpower.None;
            set => _allegiance = value;
        }
        private Superpower _allegiance;

        [PublicAPI( "The localized allegiance of the faction, if known" ), JsonIgnore, Obsolete("Please use Allegiance instead")]
        public string allegiance => Allegiance.localizedName;

        [ PublicAPI( "The government of the faction, if known, as an object" ) ]
        public Government Government
        {
            get => _government ?? Government.None;
            set => _government = value;
        }
        private Government _government;

        [PublicAPI( "The localized government of the faction, if known" ), JsonIgnore, Obsolete("Please use Government instead")]
        public string government => Government.localizedName;

        [PublicAPI( "Your reputation with the faction, out of 100%" )]
        public decimal? myreputation { get; set; }

        [PublicAPI( "True if the faction is the pilot's current squadron faction" )]
        public bool squadronfaction { get; set; }

        /// <summary> The faction's presence in various systems </summary>
        // As this is quite dynamic data and the data we receive at any given time is likely to be incomplete, 
        // we won't save it to the local database at this time.
        [PublicAPI( "A list of FactionPresence objects. Unless called from the *FactionDetails()* function, only details from the current system will be included here" )]
        public List<FactionPresence> presences { get; set; } = new List<FactionPresence>();

        // Not intended to be user facing

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeToSeconds(updatedAt);
        public DateTime updatedAt { get; set; }
    }

    public class FactionTrendingState
    {
        [PublicAPI]
        public FactionState factionState { get; private set; }

        [PublicAPI]
        public int? trend { get; private set; }

        public FactionTrendingState(FactionState factionState, int? trend)
        {
            this.factionState = factionState;
            this.trend = trend;
        }
    }

    public class FactionPresence
    {
        [PublicAPI( "The system name where this faction hasa presence" )]
        public string systemName { get; set; }

        [PublicAPI( "The unique 64 bit system address where this faction has a presence" )]
        public ulong systemAddress { get; set; }

        [PublicAPI( "The faction's current dominant state in this system, as an object" )]
        public FactionState FactionState
        {
            get => _factionState ?? FactionState.None;
            set => _factionState = value;
        }
        private FactionState _factionState;

        [PublicAPI( "The faction's current (localized) dominant state in this system" ), JsonIgnore, Obsolete("Please use FactionState instead")]
        public string state => FactionState.localizedName;

        [PublicAPI( "The faction's influence level within the system, as a percentage" )]
        public decimal? influence { get; set; }

        [PublicAPI( "(For recently visited systems only) A list of FactionState objects" )]
        public List<FactionState> ActiveStates { get; set; } = new List<FactionState>();

        [PublicAPI( "(For recently visited systems only) A list of pending FactionState objects and trend values" )]
        public List<FactionTrendingState> PendingStates { get; set; } = new List<FactionTrendingState>();

        [PublicAPI( "(For recently visited systems only) A list of recent prior FactionState objects and trend values" )]
        public List<FactionTrendingState> RecoveringStates { get; set; } = new List<FactionTrendingState>();

        [PublicAPI( "(For recently visited systems only) The current happiness level of the faction within the system, as an object" )]
        public Happiness Happiness
        {
            get => _happiness ?? Happiness.None;
            set => _happiness = value;
        }
        private Happiness _happiness;

        [PublicAPI ( "(For recently visited systems only) The current localized happiness level of the faction within the system" ), JsonIgnore, Obsolete("Please use Happiness for code instead. This value is for use in Cottle only.")]
        public string happiness => Happiness.localizedName;

        // Pilot and squadron data

        /// <summary> Whether the system is the happiest system in the pilot's squadron's faction's control </summary>
        public bool squadronhappiestsystem { get; set; }

        [PublicAPI( "(For recently visited systems only) True if the faction is the pilot's current squadron faction" )]
        public bool squadronhomesystem { get; set; }

        // Not intended to be user facing

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeToSeconds(updatedAt);
        public DateTime updatedAt { get; set; }
    }
}
