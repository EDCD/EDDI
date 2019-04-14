using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiEvents
{
    public class LocationEvent : Event
    {
        public const string NAME = "Location";
        public const string DESCRIPTION = "Triggered when the commander's location is reported, usually when they reload their game.";
        public const string SAMPLE = "{ \"timestamp\":\"2017-10-01T22:03:04Z\", \"event\":\"Location\", \"Docked\":true, \"StationName\":\"Heyerdahl Hub\", \"StationType\":\"Orbis\", \"StarSystem\":\"Alpha Caeli\", \"StarPos\":[45.219,-43.344,-20.125], \"SystemAddress\": 2106438175083, \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":16796538233, \"Body\":\"Heyerdahl Hub\", \"BodyType\":\"Station\", \"MarketID\": 3223697152, \"Factions\":[ { \"Name\":\"Labour of Alpha Caeli\", \"FactionState\":\"Famine\", \"Government\":\"Democracy\", \"Influence\":0.056000, \"Allegiance\":\"Independent\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":0 } ] }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\" }, { \"Name\":\"Dangarla Creative Corp.\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.081000, \"Allegiance\":\"Federation\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":1 }, { \"State\":\"CivilUnrest\", \"Trend\":1 } ] }, { \"Name\":\"Alpha Caeli Gold Organisation\", \"FactionState\":\"Boom\", \"Government\":\"Corporate\", \"Influence\":0.252000, \"Allegiance\":\"Federation\" }, { \"Name\":\"Alpha Caeli Freedom Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.056000, \"Allegiance\":\"Independent\" }, { \"Name\":\"Blue Natural Holdings\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.438000, \"Allegiance\":\"Federation\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":1 } ] }, { \"Name\":\"Alpha Caeli Hand Gang\", \"FactionState\":\"Boom\", \"Government\":\"Anarchy\", \"Influence\":0.046000, \"Allegiance\":\"Independent\" }, { \"Name\":\"Aurora\", \"FactionState\":\"Boom\", \"Government\":\"Corporate\", \"Influence\":0.071000, \"Allegiance\":\"Independent\" } ], \"SystemFaction\":\"Blue Natural Holdings\" }";

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
            VARIABLES.Add("marketId", "The market ID of the station at which the commander is docked");
            VARIABLES.Add("stationtype", "The type of the station at which the commander is docked");

            // Pre-3.3.03 faction variables
            VARIABLES.Add("faction", "The faction controlling the system in which the commander resides");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system in which the commander resides");
            VARIABLES.Add("government", "The government of the system in which the commander resides");
            VARIABLES.Add("allegiance", "The allegiance of the system in which the commander resides");

            // Post-3.3.03 faction variables
            VARIABLES.Add("systemfaction", "The faction controlling the system in which the commander resides");
            VARIABLES.Add("systemstate", "The state of the faction controlling the system in which the commander resides");
            VARIABLES.Add("systemgovernment", "The government of the system in which the commander resides");
            VARIABLES.Add("stationfaction", "The faction controlling the station, if the commander is docked");
            VARIABLES.Add("stationstate", "The state of the faction controlling the station, if the commander is docked");
            VARIABLES.Add("stationgovernment", "The government of the station, if the commander is docked");

            VARIABLES.Add("economy", "The economy of the system in which the commander resides");
            VARIABLES.Add("economy2", "The secondary economy of the system in which the commander resides, if any");
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

        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        public bool docked { get; private set; }

        public string station { get; private set; }

        public string stationtype => (stationModel ?? StationModel.None).localizedName;

        public string economy => (Economy ?? Economy.None).localizedName;

        public string economy2 => (Economy2 ?? Economy.None).localizedName;


        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        public long? population { get; private set; }

        public decimal? longitude { get; private set; }

        public decimal? latitude { get; private set; }

        // Pre-3.3.03 faction properties to maintain script/profile backwards compatability
        public string faction => controllingsystemfaction?.name;
        public string factionstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemName == system)?.FactionState ?? FactionState.None).localizedName;
        public string government => (controllingsystemfaction?.Government ?? Government.None).localizedName;
        public string allegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        // Post-3.3.03 faction properties
        public string systemfaction => controllingsystemfaction?.name;
        public string systemstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemName == system)?.FactionState ?? FactionState.None).localizedName;
        public string systemgovernment => (controllingsystemfaction?.Government ?? Government.None).localizedName;
        public string systemallegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;
        public string stationfaction => controllingstationfaction?.name;
        public string stationstate => (controllingstationfaction?.presences.FirstOrDefault(p => p.systemName == system)?.FactionState ?? FactionState.None).localizedName;
        public string stationgovernment => (controllingstationfaction?.Government ?? Government.None).localizedName;
        public string stationallegiancet => (controllingstationfaction?.Allegiance ?? Superpower.None).localizedName;

        // These properties are not intended to be user facing
        public long? systemAddress { get; private set; }
        public long? marketId { get; private set; }
        public Economy Economy { get; private set; } = Economy.None;
        public Economy Economy2 { get; private set; } = Economy.None;
        public Faction controllingsystemfaction { get; private set; }
        public Faction controllingstationfaction { get; private set; }
        public List<Faction> factions { get; private set; }
        public SecurityLevel securityLevel { get; private set; } = SecurityLevel.None;
        public StationModel stationModel { get; private set; } = StationModel.None;
        public BodyType bodyType { get; private set; } = BodyType.None;

        public LocationEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, long systemAddress, string body, BodyType bodytype, bool docked, string station, StationModel stationtype, long? marketId, Faction systemFaction, Faction stationFaction, Economy economy, Economy economy2, SecurityLevel security, long? population, decimal? longitude, decimal? latitude, List<Faction> factions) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.systemAddress = systemAddress;
            this.body = body;
            this.bodyType = bodytype;
            this.docked = docked;
            this.station = station;
            this.stationModel = stationtype;
            this.marketId = marketId;
            this.controllingsystemfaction = systemFaction;
            this.controllingstationfaction = stationFaction;
            this.Economy = (economy ?? Economy.None);
            this.Economy2 = (economy2 ?? Economy.None);
            this.securityLevel = security;
            this.population = population;
            this.longitude = longitude;
            this.latitude = latitude;
            this.factions = factions;
        }
    }
}