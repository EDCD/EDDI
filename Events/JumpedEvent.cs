﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class JumpedEvent : Event
    {
        public const string NAME = "Jumped";
        public const string DESCRIPTION = "Triggered when you complete a jump to another system";
        public const string SAMPLE = "{\"timestamp\":\"2016-07-21T13:16:49Z\",\"event\":\"FSDJump\",\"StarSystem\":\"LP 98-132\",\"StarPos\":[-26.781,37.031,-4.594],\"Economy\":\"$economy_Extraction;\",\"Allegiance\":\"Federation\",\"Government\":\"$government_Anarchy;\",\"Security\":\"$SYSTEM_SECURITY_high_anarchy;\", \"Population\":14087951,\"JumpDist\":5.230,\"FuelUsed\":0.355614,\"FuelLevel\":12.079949,\"Faction\":\"Brotherhood of LP 98-132\",\"FactionState\":\"Outbreak\"}";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static JumpedEvent()
        {
            VARIABLES.Add("system", "The name of the system to which the commander has jumped");
            VARIABLES.Add("x", "The X co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("y", "The Y co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("z", "The Z co-ordinate of the system to which the commander has jumped");
            VARIABLES.Add("distance", "The distance the commander has jumped, in light years");
            VARIABLES.Add("fuelused", "The amount of fuel used in this jump");
            VARIABLES.Add("fuelremaining", "The amount of fuel remaining after this jump");
            VARIABLES.Add("allegiance", "The allegiance of the system to which the commander has jumped");
            VARIABLES.Add("faction", "The faction controlling the system to which the commander has jumped");
            VARIABLES.Add("factionstate", "The state of the faction controlling the system to which the commander has jumped");
            VARIABLES.Add("economy", "The economy of the system to which the commander has jumped");
            VARIABLES.Add("government", "The government of the system to which the commander has jumped");
            VARIABLES.Add("security", "The security of the system to which the commander has jumped");
            VARIABLES.Add("population", "The population of the system to which the commander has jumped");
        }

        public string system { get; private set; }

        public decimal x { get; private set; }

        public decimal y { get; private set; }

        public decimal z { get; private set; }

        public decimal distance { get; private set; }

        public decimal fuelused { get; private set; }

        public decimal fuelremaining { get; private set; }

        public string allegiance { get; private set; }

        public string faction { get; private set; }

        public string factionstate { get; private set; }
        public string LocalFactionState
        {
            get
            {
                if (factionstate != null && factionstate != "")
                {
                    return State.FromName(factionstate).LocalName;
                }
                else return null;
            }
        }

        public string economy { get; private set; }
        public string LocalEconomy
        {
            get
            {
                if (economy != null && economy != "")
                {
                    return Economy.FromName(economy).LocalName;
                }
                else return null;
            }
        }

        public string government { get; private set; }
        public string LocalGovernment
        {
            get
            {
                if (government != null && government != "")
                {
                    return Government.FromName(government).LocalName;
                }
                else return null;
            }
        }

        public string security { get; private set; }
        public string LocalSecurity
        {
            get
            {
                if (security != null && security != "")
                {
                    return SecurityLevel.FromName(security).LocalName;
                }
                else return null;
            }
        }

        public long? population { get; private set; }

        public JumpedEvent(DateTime timestamp, string system, decimal x, decimal y, decimal z, decimal distance, decimal fuelused, decimal fuelremaining, Superpower allegiance, string faction, State factionstate, Economy economy, Government government, SecurityLevel security, long? population) : base(timestamp, NAME)
        {
            this.system = system;
            this.x = x;
            this.y = y;
            this.z = z;
            this.distance = distance;
            this.fuelused = fuelused;
            this.fuelremaining = fuelremaining;
            this.allegiance = (allegiance == null ? Superpower.None.name : allegiance.name) ;
            this.faction = faction;
            this.factionstate = (factionstate == null ? State.None.name : factionstate.name);
            this.economy = (economy == null ? Economy.None.name : economy.name);
            this.government = (government == null ? Government.None.name : government.name);
            this.security = (security == null ? SecurityLevel.None.name : security.name);
            this.population = population;
        }
    }
}
