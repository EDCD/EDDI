using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiConfigService.Configurations
{
    /// <summary>Configuration for the Galnet monitor</summary>
    [JsonObject(MemberSerialization.OptOut), RelativePath(@"\commandermonitor.json")]
    public class CommanderConfiguration : Config
    {

        [JsonProperty( "commanderName" )]
        public string commanderName { get; set; }

        [JsonProperty( "credits" )]
        public ulong? credits { get; set; }

        [JsonIgnore] // This is a temporary dictionary to track status of online friends - it should not be saved to disk
        public List<Friend> friends { get; set; } = new List<Friend>();

        [JsonProperty( "frontierID" )]
        public string frontierID { get; set; }

        [JsonProperty( "gender" )]
        public string gender { get; set; } = "Male";

        [JsonProperty( "phoneticName" )]
        public string phoneticName { get; set; }

        #region Home System Properties

        [JsonProperty( "homeSystemName" )]
        public string homeSystemName { get; set; }

        [JsonProperty( "homeSystemAddress" )]
        public ulong? homeSystemAddress { get; set; }

        [JsonProperty( "homeSystemX" )]
        public decimal? homeSystemX { get; set; }

        [JsonProperty( "homeSystemY" )]
        public decimal? homeSystemY { get; set; }

        [JsonProperty( "homeSystemZ" )]
        public decimal? homeSystemZ { get; set; }

        [JsonProperty( "homeStationName" )]
        public string homeStationName { get; set; }

        [JsonProperty( "homeStationMarketID" )]
        public long? homeStationMarketID { get; set; }

        #endregion 

        #region Powerplay Properties

        [JsonProperty( "power" )]
        public string power
        {

            get => Power?.edname ?? Power.None.edname;
            set
            {
                var pwDef = Power.FromEDName(value);
                this.Power = pwDef;
            }
        }

        [JsonIgnore]
        public Power Power { get; set; } = Power.None;

        [JsonProperty( "powerMerits" )]
        public int? powerMerits { get; set; }

        [JsonProperty( "powerRank" )]
        public int powerRank { get; set; }

        #endregion

        #region Squadron Properties

        [JsonProperty( "squadronName" )]
        public string squadronName { get; set; }

        [JsonProperty( "squadronID" )]
        public string squadronTag { get; set; }

        [JsonProperty( "SquadronRank" )]
        public SquadronRank SquadronRank { get; set; }

        [JsonProperty( "squadronAllegiance" )]
        public string squadronAllegiance
        {

            get => SquadronAllegiance?.edname ?? Superpower.None.edname;
            set
            {
                var saDef = Superpower.FromEDName(value);
                this.SquadronAllegiance = saDef;
            }
        }

        [JsonIgnore]
        public Superpower SquadronAllegiance { get; set; } = Superpower.None;

        [JsonProperty( "squadronPower" )]
        public string squadronPower
        {

            get => SquadronPower?.edname ?? Power.None.edname;
            set
            {
                var spDef = Power.FromEDName(value);
                this.SquadronPower = spDef;
            }
        }

        [JsonIgnore]
        public Power SquadronPower { get; set; } = Power.None;

        [JsonProperty( "squadronSystemName" )]
        public string squadronSystemName { get; set; }

        [JsonProperty( "squadronSystemAddress" )]
        public ulong? squadronSystemAddress { get; set; }

        [JsonProperty( "squadronFaction" )]
        public string squadronFaction { get; set; }

        #endregion

        public DateTime updatedat { get; set; }
    }
}
