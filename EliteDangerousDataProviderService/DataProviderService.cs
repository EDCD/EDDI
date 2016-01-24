using EliteDangerousDataDefinitions;
using Newtonsoft.Json.Linq;
using System.Net;

namespace EliteDangerousDataProviderService
{
    /// <summary>Access to EDDP data<summary>
    public class DataProviderService
    {
        public static StarSystem GetSystemData(string system)
        {
            var client = new WebClient();
            var response = client.DownloadString("http://api.eddp.co:16161/systems/" + system);
            return StarSystemFromEDDP(response);
        }

        public static StarSystem StarSystemFromEDDP(string data)
        {
            return StarSystemFromEDDP(JObject.Parse(data));
        }

        public static StarSystem StarSystemFromEDDP(dynamic json)
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
