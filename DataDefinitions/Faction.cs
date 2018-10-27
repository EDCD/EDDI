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

        /// <summary> The faction's EDDB ID </summary>
        public long? EDDBID { get; set; }

        /// <summary> The faction's EDSM ID </summary>
        public long? EDSMID { get; set; }

        /// <summary> The faction's allegiance (localized name) </summary>
        [Obsolete("Please use Allegiance instead")]
        public string allegiance => (Allegiance ?? Superpower.None).localizedName;
        /// <summary> The faction's allegiance </summary>
        public Superpower Allegiance { get; set; } = Superpower.None;

        /// <summary> The faction's government (localized name) </summary>
        [Obsolete("Please use Government instead")]
        public string government => (Government ?? Government.None).localizedName;
        /// <summary> The faction's government </summary>
        public Government Government { get; set; } = Government.None;

        /// <summary> The faction's current system state (localized name) </summary>
        [Obsolete("Please use State instead")]
        public string state => (FactionState ?? FactionState.None).localizedName;
        /// <summary> The faction's current system state </summary>
        public FactionState FactionState { get; set; } = FactionState.None;

        /// <summary> The faction's current influence in the system (as a percentage) </summary>
        public decimal? influence { get; set; }

        /// <summary> The faction's home system EDDBID </summary>
        public long? homeSystemEddbId { get; set; }

        /// <summary> Whether the faction is a player faction </summary>
        public bool isplayer { get; set; }

        /// <summary> The last time the information present changed </summary> 
        public long? updatedat => Dates.fromDateTimeStringToSeconds(updatedAt.ToString());
        public DateTime updatedAt { get; set; }
    }
}
