using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class LocationEvent : Event
    {
        public const string NAME = "Location";
        public const string DESCRIPTION = "Triggered when the commander's location is reported, usually when they reload their game.";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-28T10:54:07Z\", \"event\":\"Location\", \"Docked\":true, \"StationName\":\"Jameson Memorial\", \"StationType\":\"Orbis\", \"StarSystem\":\"Shinrarta Dezhra\", \"StarPos\":[55.719,17.594,27.156], \"Allegiance\":\"Independent\", \"Economy\":\"$economy_HighTech;\", \"Economy_Localised\":\"High tech\", \"Government\":\"$government_Democracy;\", \"Government_Localised\":\"Democracy\", \"Security\":\"$SYSTEM_SECURITY_high;\", \"Security_Localised\":\"High Security\", \"Population\":12501980, \"Body\":\"Jameson Memorial\", \"Faction\":\"The Pilots Federation\", \"FactionState\":\"Boom\" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static LocationEvent()
        {
            VARIABLES.Add("system", "The name of the system in which the commander resides");
            VARIABLES.Add("x", "The X co-ordinate of the system in which the commander resides");
            VARIABLES.Add("y", "The Y co-ordinate of the system in which the commander resides");
            VARIABLES.Add("z", "The Z co-ordinate of the system in which the commander resides");
            VARIABLES.Add("body", "The nearest body to the commander");
            VARIABLES.Add("bodytype", "The type of the nearest body to the commander");
            VARIABLES.Add("docked", "True if the commander is docked");
            VARIABLES.Add("station", "The name of the station at which the commander is docked");
            VARIABLES.Add("stationtype", "The type of the station at which the commander is docked");
            VARIABLES.Add("allegiance", "The allegiance of the system in which the commander resides");
            VARIABLES.Add("faction", "The faction controlling the system in which the commander resides");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system in which the commander resides");
            VARIABLES.Add("economy", "The economy of the system in which the commander resides");
            VARIABLES.Add("government", "The government of the system in which the commander resides");
            VARIABLES.Add("security", "The security of the system in which the commander resides");
            VARIABLES.Add("longitude", "The longitude of the commander (if on the ground)");
            VARIABLES.Add("latitude", "The latitude of the commander (if on the ground)");
            VARIABLES.Add("population", "The population of the system to which the commander has jumped");
        }

        public string system { get; private set; }

        public decimal x { get; private set; }

        public decimal y { get; private set; }

        public decimal z { get; private set; }

        public string body { get; private set; }

        public string bodytype { get; private set; }

        public bool docked { get; private set; }

        public string station { get; private set; }

        public string stationtype { get; private set; }

        public string allegiance { get; private set; }

        public string faction { get; private set; }

        public string economy { get; private set; }

        public string government { get; private set; }

        public string security { get; private set; }

        public long? population { get; private set; }

        public decimal? longitude { get; private set; }

        public decimal? latitude { get; private set; }

        public LocationEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, string body, string bodytype, bool docked, string station, string stationtype, Superpower allegiance, string faction, Economy economy, Government government, SecurityLevel security, long? population, decimal? longitude, decimal? latitude) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.body = body;
            this.bodytype = bodytype;
            this.docked = docked;
            this.station = station;
            this.stationtype = stationtype;
            this.allegiance = (allegiance == null ? Superpower.None.name : allegiance.name);
            this.faction = faction;
            this.economy = (economy == null ? Economy.None.name : economy.name);
            this.government = (government == null ? Government.None.name : government.name);
            this.security = (security == null ? SecurityLevel.Low.name : security.name);
            this.population = population;
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
