using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;

namespace EddiMissionMonitor
{
    public class MissionAcceptedEvent: Event
    {
        public const string NAME = "Mission accepted";
        public const string DESCRIPTION = "Triggered when you accept a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2016-09-22T05:54:00Z\", \"event\":\"MissionAccepted\", \"Faction\":\"Mafia of Cavins\", \"Name\":\"Mission_PassengerVIP\", \"Commodity\":\"$DomesticAppliances_Name;\", \"Commodity_Localised\":\"Domestic Appliances\", \"Count\":3, \"DestinationSystem\":\"Carnoeck\", \"DestinationStation\":\"Bacon City\", \"Expiry\":\"2016-09-22T07:38:43Z\", \"PassengerCount\":3, \"PassengerVIPs\":true, \"PassengerWanted\":false, \"PassengerType\":\"Tourist\", \"MissionID\":26480079 }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionAcceptedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("localisedname", "The localised name of the mission");
            VARIABLES.Add("faction", "The faction issuing the mission");
            VARIABLES.Add("destinationsystem", "The destination system for the mission (if applicable)");
            VARIABLES.Add("destinationstation", "The destination station for the mission (if applicable)");
            VARIABLES.Add("commodity", "The commodity involved in the mission (if applicable)");
            VARIABLES.Add("amount", "The amount of the commodity,  passengers or targets involved in the mission (if applicable)");
            VARIABLES.Add("wing", "True if the mission allows wing-mates");
            VARIABLES.Add("passengercount", "The number of passengers (if applicable)");
            VARIABLES.Add("passengerwanted", "True if the passengers are wanted (if applicable)");
            VARIABLES.Add("passengertype", "The type of passengers in the mission (if applicable)");
            VARIABLES.Add("passengervips", "True if the passenger is a VIP (if applicable)");
            VARIABLES.Add("target", "Name of the target of the mission (if applicable)");
            VARIABLES.Add("targettype", "Type of the target of the mission (if applicable)");
            VARIABLES.Add("targetfaction", "Faction of the target of the mission (if applicable)");
            VARIABLES.Add("killcount", "The number of targets (if applicable)");
            VARIABLES.Add("communal", "True if the mission is a community goal");
            VARIABLES.Add("expiry", "The expiry date of the mission");
            VARIABLES.Add("reward", "The expected cash reward from the mission");
            VARIABLES.Add("influence", "The increase in the faction's influence in the system gained when completing this mission (None/Low/Med/High)");
            VARIABLES.Add("reputation", "The increase in the commander's reputation with the faction gained when completing this mission (None/Low/Med/High)");
        }

        public long? missionid { get; private set; }
        public string name { get; private set; }
        public string localisedname { get; private set; }
        public string faction { get; private set; }
        public int? reward { get; private set; }
        public string influence { get; private set; }
        public string reputation { get; private set; }
        public bool wing { get; private set; }
        public DateTime? expiry { get; private set; }

        public string commodity { get; private set; }
        public int? amount { get; private set; }

        public string destinationsystem { get; private set; }
        public string destinationstation { get; private set; }

        public string passengertype { get; private set; }
        public bool? passengerwanted { get; private set; }
        public bool? passengervips { get; private set; }

        public string target { get; private set; }
        public string targettype { get; private set; }
        public string targetfaction { get; private set; }

        public bool communal { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public MissionAcceptedEvent(DateTime timestamp, long? missionid, string name, string localisedname, string faction, string destinationsystem, string destinationstation, CommodityDefinition commodity, int? amount, bool? passengerwanted, string passengertype, bool? passengervips, string target, string targettype, string targetfaction, bool communal, DateTime? expiry, string influence, string reputation, int? reward, bool wing) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.localisedname = localisedname;
            this.faction = faction;
            this.destinationsystem = destinationsystem;
            this.destinationstation = destinationstation;
            this.commodity = commodity?.localizedName;
            this.amount = amount;
            this.passengertype = passengertype;
            this.passengerwanted = passengerwanted;
            this.passengervips = passengervips;
            this.target = target;
            this.targettype = targettype;
            this.targetfaction = targetfaction;
            this.communal = communal;
            this.expiry = expiry;
            this.influence = influence;
            this.reputation = reputation;
            this.reward = reward;
            this.wing = wing;
            this.commodityDefinition = commodity;
        }
    }
}
