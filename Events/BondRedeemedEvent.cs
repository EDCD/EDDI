using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class BondRedeemedEvent : Event
    {
        public const string NAME = "Bond redeemed";
        public const string DESCRIPTION = "Triggered when you redeem a combat bond";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:10:11Z"", ""event"":""RedeemVoucher"", ""Type"":""CombatBond"",""Faction"":""The Pilots Federation"",""Amount"":1000 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BondRedeemedEvent()
        {
            VARIABLES.Add("faction", "The name of the faction who issued the bond");
            VARIABLES.Add("amount", "The amount rewarded (after any broker fees)");
        }

        [JsonProperty("faction")]
        public string faction { get; private set; }

        [JsonProperty("amount")]
        public long amount { get; private set; }

        public BondRedeemedEvent(DateTime timestamp, string faction, long amount) : base(timestamp, NAME)
        {
            this.faction = faction;
            this.amount = amount;
        }
    }
}
