using System;
using Utilities;
using Utilities.RegionMap;
using EddiDataDefinitions;

namespace EddiEvents
{
    [PublicAPI]
    public class NebulaEvent : Event
    {
        public const string NAME = "Nebula";
        public const string DESCRIPTION = "Triggered when you are near a new nebula.";
        public const string SAMPLE = "{ \"timestamp\":\"2018-10-29T10:05:21Z\", \"event\":\"FSDJump\", \"StarSystem\":\"Eranin\", \"SystemAddress\":2832631632594, \"StarPos\":[-22.84375,36.53125,-1.18750], \"SystemAllegiance\":\"Independent\", \"SystemEconomy\":\"$economy_Agri;\", \"SystemEconomy_Localised\":\"Agriculture\", \"SystemSecondEconomy\":\"$economy_Refinery;\", \"SystemSecondEconomy_Localised\":\"Refinery\", \"SystemGovernment\":\"$government_Anarchy;\", \"SystemGovernment_Localised\":\"Anarchy\", \"SystemSecurity\":\"$GAlAXY_MAP_INFO_state_anarchy;\", \"SystemSecurity_Localised\":\"Anarchy\", \"Population\":450000, \"JumpDist\":13.334, \"FuelUsed\":0.000000, \"FuelLevel\":25.630281, \"Factions\":[ { \"Name\":\"Eranin Expeditionary Institute\", \"FactionState\":\"None\", \"Government\":\"Cooperative\", \"Influence\":0.170000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000 }, { \"Name\":\"Eranin Peoples Party\", \"FactionState\":\"CivilWar\", \"Government\":\"Communism\", \"Influence\":0.226000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":29.974300, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Pilots Federation Local Branch\", \"FactionState\":\"None\", \"Government\":\"Democracy\", \"Influence\":0.000000, \"Allegiance\":\"PilotsFederation\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":82.918297 }, { \"Name\":\"Eranin Industry\", \"FactionState\":\"Outbreak\", \"Government\":\"Corporate\", \"Influence\":0.209000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand3;\", \"Happiness_Localised\":\"Discontented\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Famine\" }, { \"State\":\"Lockdown\" }, { \"State\":\"Outbreak\" } ] }, { \"Name\":\"Eranin Federal Bridge\", \"FactionState\":\"CivilWar\", \"Government\":\"Dictatorship\", \"Influence\":0.226000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"CivilWar\" } ] }, { \"Name\":\"Mob of Eranin\", \"FactionState\":\"CivilLiberty\", \"Government\":\"Anarchy\", \"Influence\":0.134000, \"Allegiance\":\"Independent\", \"Happiness\":\"$Faction_HappinessBand1;\", \"Happiness_Localised\":\"Elated\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Boom\" }, { \"State\":\"CivilLiberty\" } ] }, { \"Name\":\"Terran Colonial Forces\", \"FactionState\":\"CivilUnrest\", \"Government\":\"Confederacy\", \"Influence\":0.035000, \"Allegiance\":\"Alliance\", \"Happiness\":\"$Faction_HappinessBand2;\", \"Happiness_Localised\":\"Happy\", \"MyReputation\":0.000000, \"ActiveStates\":[ { \"State\":\"Boom\" }, { \"State\":\"CivilUnrest\" } ] } ], \"SystemFaction\":\"Mob of Eranin\", \"FactionState\":\"CivilLiberty\", \"Powers\":[ \"Edmund Mahon\" ], \"PowerplayState\":\"Exploited\" }";

        [PublicAPI("The region object.")]
        public Nebula nebula { get; private set; }

        [PublicAPI("The name of the region we are currently in.")]
        public string name => nebula.name;

        [PublicAPI("The id of the region we are currently in.")]
        public decimal? distance => nebula.distance;

        public NebulaEvent (DateTime timestamp, Nebula nebula) : base(timestamp, NAME)
        {
            this.nebula = nebula;
        }
    }
}