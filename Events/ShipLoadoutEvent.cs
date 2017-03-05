using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class ShipLoadoutEvent : Event
    {
        public const string NAME = "Ship loadout";
        public const string DESCRIPTION = "Triggered when you obtain the loadout of your ship";
        public const string SAMPLE = @"";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static ShipLoadoutEvent()
        {
            VARIABLES.Add("ship", "The ship");
            VARIABLES.Add("shipid", "The ID of the ship");
            VARIABLES.Add("shipident", "The identification string of the ship");
            VARIABLES.Add("modules", "The modules of the ship");
        }

        public int? shipid { get; private set; }

        public string ship { get; private set; }

        public string shipname { get; private set; }

        public string shipident { get; private set; }

        public List<object> modules { get; private set;  }

        public ShipLoadoutEvent(DateTime timestamp, string ship, int? shipId, string shipName, string shipIdent, List<object> modules) : base(timestamp, NAME)
        {
            this.ship = ship;
            this.shipid = shipId;
            this.shipident = shipIdent;
            this.shipident = shipIdent;
            this.modules = modules;
        }
    }
}
