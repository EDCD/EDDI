using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class LocationEvent : Event
    {
        public const string NAME = "Location";
        public const string DESCRIPTION = "Triggered when the commander's location is reported, usually when they reload their game.";
        public const string SAMPLE = "{ \"timestamp\":\"2017-10-01T22:03:04Z\", \"event\":\"Location\", \"Docked\":true, \"StationName\":\"Heyerdahl Hub\", \"StationType\":\"Orbis\", \"StarSystem\":\"Alpha Caeli\", \"StarPos\":[45.219,-43.344,-20.125], \"SystemAddress\": 2106438175083, \"DistFromStarLS\": 1441, \"SystemAllegiance\":\"Federation\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":16796538233, \"Body\":\"Heyerdahl Hub\", \"BodyType\":\"Station\", \"MarketID\": 3223697152, \"Factions\":[ { \"Name\":\"Labour of Alpha Caeli\", \"FactionState\":\"Famine\", \"Government\":\"Democracy\", \"Influence\":0.056000, \"Allegiance\":\"Independent\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":0 } ] }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\" }, { \"Name\":\"Dangarla Creative Corp.\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.081000, \"Allegiance\":\"Federation\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":1 }, { \"State\":\"CivilUnrest\", \"Trend\":1 } ] }, { \"Name\":\"Alpha Caeli Gold Organisation\", \"FactionState\":\"Boom\", \"Government\":\"Corporate\", \"Influence\":0.252000, \"Allegiance\":\"Federation\" }, { \"Name\":\"Alpha Caeli Freedom Party\", \"FactionState\":\"None\", \"Government\":\"Dictatorship\", \"Influence\":0.056000, \"Allegiance\":\"Independent\" }, { \"Name\":\"Blue Natural Holdings\", \"FactionState\":\"None\", \"Government\":\"Corporate\", \"Influence\":0.438000, \"Allegiance\":\"Federation\", \"PendingStates\":[ { \"State\":\"Boom\", \"Trend\":1 } ] }, { \"Name\":\"Alpha Caeli Hand Gang\", \"FactionState\":\"Boom\", \"Government\":\"Anarchy\", \"Influence\":0.046000, \"Allegiance\":\"Independent\" }, { \"Name\":\"Aurora\", \"FactionState\":\"Boom\", \"Government\":\"Corporate\", \"Influence\":0.071000, \"Allegiance\":\"Independent\" } ], \"SystemFaction\":\"Blue Natural Holdings\", \"Powers\":[ \"Edmund Mahon\", \"Zachary Hudson\" ], \"PowerplayState\":\"Contested\" }";

        [PublicAPI("The name of the system in which the commander is located")]
        public string systemname { get; private set; }

        [PublicAPI("The X co-ordinate of the system in which the commander is located")]
        public decimal x { get; private set; }

        [PublicAPI("The Y co-ordinate of the system in which the commander is located")]
        public decimal y { get; private set; }

        [PublicAPI("The Z co-ordinate of the system in which the commander is located")]
        public decimal z { get; private set; }

        [PublicAPI("The distance of the nearest body (when close) from the main star")]
        public decimal? distancefromstar { get; private set; }

        [PublicAPI("The nearest body to the commander")]
        public string bodyname { get; private set; }

        [PublicAPI("The type of the nearest body to the commander")]
        public string bodytype => (bodyType ?? BodyType.None).localizedName;

        [PublicAPI("True if the commander is docked")]
        public bool docked { get; private set; }

        [PublicAPI("The name of the station at which the commander is docked")]
        public string station { get; private set; }

        [PublicAPI("The market ID of the station at which the commander is docked")]
        public long? marketId { get; private set; }

        [PublicAPI("The type of the station at which the commander is docked")]
        public string stationtype => (stationModel ?? StationModel.None).localizedName;

        [PublicAPI("The economy of the system in which the commander resides")]
        public string economy => (Economy ?? Economy.None).localizedName;

        [PublicAPI("The secondary economy of the system in which the commander resides, if any")]
        public string economy2 => (Economy2 ?? Economy.None).localizedName;

        [PublicAPI("The security of the system in which the commander resides")]
        public string security => (securityLevel ?? SecurityLevel.None).localizedName;

        [PublicAPI("The population of the system to which the commander has jumped")]
        public long? population { get; private set; }

        [PublicAPI("The longitude of the commander (if on the ground)")]
        public decimal? longitude { get; private set; }

        [PublicAPI("The latitude of the commander (if on the ground)")]
        public decimal? latitude { get; private set; }

        // Post-3.3.03 faction properties

        [PublicAPI("The faction controlling the system in which the commander is located")]
        public string systemfaction => controllingsystemfaction?.name;

        [PublicAPI("The state of the faction controlling the system in which the commander is located")]
        public string systemstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        [PublicAPI("The government of the system in which the commander is located")]
        public string systemgovernment => (controllingsystemfaction?.Government ?? Government.None).localizedName;

        [PublicAPI("The super power allegiance of the star system in which the commander is located")]
        public string systemallegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        [PublicAPI("The faction controlling the station, if the commander is docked")]
        public string stationfaction => controllingstationfaction?.name;

        [PublicAPI("The state of the faction controlling the station, if the commander is docked")]
        public string stationstate => (controllingstationfaction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        [PublicAPI("The government of the station, if the commander is docked")]
        public string stationgovernment => (controllingstationfaction?.Government ?? Government.None).localizedName;

        [PublicAPI("The super power allegiance of the station, if the commander is docked")]
        public string stationallegiance => (controllingstationfaction?.Allegiance ?? Superpower.None).localizedName;

        // Powerplay properties (only when pledged)

        [PublicAPI("(Only when pledged) The powerplay power exerting influence over the star system. If the star system is `Contested`, this will be empty")]
        public string power => Power.localizedName;

        [PublicAPI("(Only when pledged) The state of powerplay efforts within the star system")]
        public string powerstate => powerState.localizedName;

        // Deprecated, maintained for compatibility with user scripts
        [Obsolete("Use systemname instead")]
        public string system => systemname;

        [Obsolete("Use bodyname instead")]
        public string body => bodyname;

        [Obsolete("Use systemfaction instead")]
        public string faction => controllingsystemfaction?.name;

        [Obsolete("Use systemstate instead")]
        public string factionstate => (controllingsystemfaction?.presences.FirstOrDefault(p => p.systemName == systemname)?.FactionState ?? FactionState.None).localizedName;

        [Obsolete("Use systemgovernment instead")]
        public string government => (controllingsystemfaction?.Government ?? Government.None).localizedName;

        [Obsolete("Use systemallegiance instead")]
        public string allegiance => (controllingsystemfaction?.Allegiance ?? Superpower.None).localizedName;

        // These properties are not intended to be user facing

        public long? systemAddress { get; private set; }

        public Economy Economy { get; private set; } = Economy.None;

        public Economy Economy2 { get; private set; } = Economy.None;

        public Faction controllingsystemfaction { get; private set; }

        public Faction controllingstationfaction { get; private set; }

        public List<Faction> factions { get; private set; }

        public SecurityLevel securityLevel { get; private set; } = SecurityLevel.None;

        public StationModel stationModel { get; private set; } = StationModel.None;

        public BodyType bodyType { get; private set; } = BodyType.None;

        public long? bodyId { get; private set; }

        public Power Power { get; private set; }

        public PowerplayState powerState { get; private set; }
        public bool taxi { get; private set; }
        public bool multicrew { get; private set; }
        public bool inSRV { get; private set; }
        public bool onFoot { get; private set; }

        public LocationEvent(DateTime timestamp, string systemName, decimal x, decimal y, decimal z, long systemAddress, decimal? distancefromstar, string bodyName, long? bodyId, BodyType bodytype, bool docked, string station, StationModel stationtype, long? marketId, Faction systemFaction, Faction stationFaction, Economy economy, Economy economy2, SecurityLevel security, long? population, decimal? longitude, decimal? latitude, List<Faction> factions, Power powerplayPower, PowerplayState powerplayState, bool taxi, bool multicrew, bool inSRV, bool onFoot) : base(timestamp, NAME)
        {
            this.systemname = systemName;
            this.x = x;
            this.y = y;
            this.z = z;
            this.systemAddress = systemAddress;
            this.distancefromstar = distancefromstar;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
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
            this.Power = powerplayPower;
            this.powerState = powerplayState;
            this.taxi = taxi;
            this.multicrew = multicrew;
            this.inSRV = inSRV;
            this.onFoot = onFoot;
        }
    }
}