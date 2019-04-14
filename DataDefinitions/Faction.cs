using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    public class Faction
    {
        /// <summary> The faction's name </summary>
        public string name { get; set; }

        /// <summary> The faction's EDSM ID </summary>
        public long? EDDBID { get; set; }

        /// <summary> The faction's EDSM ID </summary>
        public long? EDSMID { get; set; }

        /// <summary> The faction's allegiance </summary>
        public Superpower Allegiance { get; set; } = Superpower.None;

        /// <summary> The faction's allegiance (localized name) </summary>
        [JsonIgnore, Obsolete("Please use Allegiance instead")]
        public string allegiance => (Allegiance ?? Superpower.None).localizedName;

        /// <summary> The faction's government </summary>
        public Government Government { get; set; } = Government.None;

        /// <summary> The faction's government (localized name) </summary>
        [JsonIgnore, Obsolete("Please use Government instead")]
        public string government => (Government ?? Government.None).localizedName;

        /// <summary> The faction's presence in various systems </summary>
        // As this is quite dynamic data and the data we receive at any given time is likely to be incomplete, 
        // we won't save it to the local database at this time.
        public List<FactionPresence> presences { get; set; } = new List<FactionPresence>();

        // Pilot and squadron data

        /// <summary> The pilot's current reputation with the faction (as a percentage) </summary>
        public decimal? myreputation { get; set; }

        /// <summary> Whether the faction is the pilot's current squadron faction </summary>
        public bool squadronfaction { get; set; }

        /// <summary> Whether the faction is a player faction </summary>
        public bool? isplayer { get; set; }

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeStringToSeconds(updatedAt.ToString());
        public DateTime updatedAt { get; set; }
    }

    public class FactionTrendingState
    {
        public FactionState factionState { get; private set; }
        public int trend { get; private set; }

        public FactionTrendingState (FactionState factionState, int trend)
        {
            this.factionState = factionState;
            this.trend = trend;
        }
    }

    public class FactionPresence
    {
        /// <summary> The star system beginning described for this faction </summary>
        public string systemName { get; set; }

        /// <summary> The faction's current system state </summary>
        public FactionState FactionState { get; set; } = FactionState.None;

        /// <summary> The faction's current system state (localized name) </summary>
        [JsonIgnore, Obsolete("Please use FactionState instead")]
        public string state => (FactionState ?? FactionState.None).localizedName;

        /// <summary> The faction's current active states, if any </summary>
        public List<FactionState> ActiveStates { get; set; } = new List<FactionState>();

        /// <summary> The faction's pending states, if any </summary>
        public List<FactionTrendingState> PendingStates { get; set; } = new List<FactionTrendingState>();

        /// <summary> The faction's recovering states, if any </summary>
        public List<FactionTrendingState> RecoveringStates { get; set; } = new List<FactionTrendingState>();

        /// <summary> The faction's current influence in the system (as a percentage) </summary>
        public decimal? influence { get; set; }

        /// <summary> The faction's current happiness within the system </summary>
        public Happiness Happiness { get; set; }

        [JsonIgnore, Obsolete("Please use Happiness for code instead. This value is for use in Cottle only.")]
        public string happiness => (Happiness ?? Happiness.None).localizedName;

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeStringToSeconds(updatedAt.ToString());
        public DateTime updatedAt { get; set; }

        // Pilot and squadron data

        /// <summary> Whether the system is the happiest system in the pilot's squadron's faction's control </summary>
        public bool squadronhappiestsystem { get; set; }

        /// <summary> True for the squadron's home faction in their home system </summary>
        public bool squadronhomesystem { get; set; }
    }
}
