using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EliteDangerousDataProviderAppService
{
    public class StarSystem
    {
        public string Name { get; set; }
        public long Population { get; set; }
        public string Allegiance { get; set;  }
        public string Government { get; set; }
        public string Faction { get; set; }
        public string PrimaryEconomy { get; set; }
        public string State { get; set; }
        public string Security { get; set; }
        public string Power { get; set; }
        public string PowerState { get; set; }

        public static StarSystem FromEDDP(dynamic json)
        {
            StarSystem StarSystem = new StarSystem();

            StarSystem.Name = (string)json["name"];
            StarSystem.Population = (long?)json["population"] == null ? 0 : (long)json["population"];
            StarSystem.Allegiance = (string)json["allegiance"];
            StarSystem.Government = (string)json["government"];
            StarSystem.Faction = (string)json["faction"];
            StarSystem.PrimaryEconomy = (string)json["primary_economy"];
            StarSystem.State = (string)json["state"] == "None" ? null : (string)json["state"];
            StarSystem.Security = (string)json["security"];
            StarSystem.Power = (string)json["power"];
            StarSystem.PowerState = (string)json["power_state"];

            return StarSystem;
        }
    }
}
