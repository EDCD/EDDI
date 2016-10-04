using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class AmmunitionPurchasedEvent : Event
    {
        public const string NAME = "Ammunition purchased";
        public const string DESCRIPTION = "Triggered when you buy ammunition from rearming";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-20T11:13:00Z\", \"event\":\"BuyAmmo\", \"Cost\":36001 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static AmmunitionPurchasedEvent()
        {
            VARIABLES.Add("price", "The price of rearming");
        }

        public decimal price { get; private set; }

        public AmmunitionPurchasedEvent(DateTime timestamp, decimal price) : base(timestamp, NAME)
        {
            this.price = price;
        }
    }
}
