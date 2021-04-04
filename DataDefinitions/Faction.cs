using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class Faction
    {
        /// <summary> The faction's name </summary>
        [PublicAPI]
        public string name { get; set; }

        /// <summary> The faction's allegiance (localized name) </summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Allegiance instead")]
        public string allegiance => (Allegiance ?? Superpower.None).localizedName;

        /// <summary> The faction's government (localized name) </summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use Government instead")]
        public string government => (Government ?? Government.None).localizedName;

        /// <summary> The pilot's current reputation with the faction (as a percentage) </summary>
        [PublicAPI]
        public decimal? myreputation { get; set; }

        /// <summary> Whether the faction is the pilot's current squadron faction </summary>
        [PublicAPI]
        public bool squadronfaction { get; set; }

        /// <summary> The faction's presence in various systems </summary>
        // As this is quite dynamic data and the data we receive at any given time is likely to be incomplete, 
        // we won't save it to the local database at this time.
        [PublicAPI]
        public List<FactionPresence> presences { get; set; } = new List<FactionPresence>();

        // Not intended to be user facing

        /// <summary> The faction's EDSM ID </summary>
        public long? EDDBID { get; set; }

        /// <summary> The faction's EDSM ID </summary>
        public long? EDSMID { get; set; }

        /// <summary> The faction's allegiance </summary>
        public Superpower Allegiance { get; set; } = Superpower.None;

        /// <summary> The faction's government </summary>
        public Government Government { get; set; } = Government.None;

        /// <summary> Whether the faction is a player faction </summary>
        public bool? isplayer { get; set; }

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeToSeconds(updatedAt);
        public DateTime updatedAt { get; set; }
    }

    public class FactionTrendingState
    {
        [PublicAPI]
        public FactionState factionState { get; private set; }

        [PublicAPI]
        public int trend { get; private set; }

        public FactionTrendingState(FactionState factionState, int trend)
        {
            this.factionState = factionState;
            this.trend = trend;
        }
    }

    public class FactionPresence
    {
        /// <summary> The star system beginning described for this faction </summary>
        [PublicAPI]
        public string systemName { get; set; }

        /// <summary> The faction's current system state (localized name) </summary>
        [PublicAPI, JsonIgnore, Obsolete("Please use FactionState instead")]
        public string state => (FactionState ?? FactionState.None).localizedName;

        /// <summary> The faction's current active states, if any </summary>
        [PublicAPI]
        public List<FactionState> ActiveStates { get; set; } = new List<FactionState>();

        /// <summary> The faction's pending states, if any </summary>
        [PublicAPI]
        public List<FactionTrendingState> PendingStates { get; set; } = new List<FactionTrendingState>();

        /// <summary> The faction's recovering states, if any </summary>
        [PublicAPI]
        public List<FactionTrendingState> RecoveringStates { get; set; } = new List<FactionTrendingState>();

        /// <summary> The faction's current influence in the system (as a percentage) </summary>
        [PublicAPI]
        public decimal? influence { get; set; }

        [PublicAPI, JsonIgnore, Obsolete("Please use Happiness for code instead. This value is for use in Cottle only.")]
        public string happiness => (Happiness ?? Happiness.None).localizedName;

        // Not intended to be user facing

        /// <summary> The faction's current system state </summary>
        public FactionState FactionState { get; set; } = FactionState.None;

        /// <summary> The faction's current happiness within the system </summary>
        public Happiness Happiness { get; set; }

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeToSeconds(updatedAt);
        public DateTime updatedAt { get; set; }

        // Pilot and squadron data

        /// <summary> Whether the system is the happiest system in the pilot's squadron's faction's control </summary>
        public bool squadronhappiestsystem { get; set; }

        /// <summary> True for the squadron's home faction in their home system </summary>
        public bool squadronhomesystem { get; set; }
    }
}
