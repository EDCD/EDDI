using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
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
            VARIABLES.Add("faction", "The faction issuing the mission");
            VARIABLES.Add("destinationsystem", "The destination system for the mission (if applicable)");
            VARIABLES.Add("destinationstation", "The destination station for the mission (if applicable)");
            VARIABLES.Add("commodity", "The commodity involved in the mission (if applicable)");
            VARIABLES.Add("amount", "The amount of the commodity,  passengers or targets involved in the mission (if applicable)");
            VARIABLES.Add("passengertype", "The type of passengers in the mission (if applicable)");
            VARIABLES.Add("passengerswanted", "True if the passengers are wanted (if applicable)");
            VARIABLES.Add("target", "Name of the target of the mission (if applicable)");
            VARIABLES.Add("targettype", "Type of the target of the mission (if applicable)");
            VARIABLES.Add("targetfaction", "Faction of the target of the mission (if applicable)");
            VARIABLES.Add("communal", "True if the mission is a community goal");
            VARIABLES.Add("expiry", "The expiry date of the mission");
            VARIABLES.Add("influence", "The increase in the faction's influence in the system gained when completing this mission (None/Low/Med/High)");
            VARIABLES.Add("reputation", "The increase in the commander's reputation with the faction gained when completing this mission (None/Low/Med/High)");
        }

        public long? missionid { get; private set; }

        public string name { get; private set; }

        public string faction { get; private set; }

        public string commodity { get; private set; }

        public int? amount { get; private set; }

        public string destinationsystem { get; private set; }

        public string destinationstation { get; private set; }

        public string passengertype { get; private set; }

        public bool? passengerswanted { get; private set; }

        public string target { get; private set; }

        public string targettype { get; private set; }

        public string targetfaction { get; private set; }

        public bool communal { get; private set; }

        public DateTime? expiry { get; private set; }

        public string influence { get; private set; }

        public string reputation { get; private set; }

        public CommodityDefinition commodityDefinition { get; private set; }

        public MissionAcceptedEvent(DateTime timestamp, long? missionid, string name, string faction, string destinationsystem, string destinationstation, CommodityDefinition commodity, int? amount, string passengertype, bool? passengerswanted, string target, string targettype, string targetfaction, bool communal, DateTime? expiry, string influence, string reputation) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.faction = faction;
            this.destinationsystem = destinationsystem;
            this.destinationstation = destinationstation;
            this.commodity = commodity?.localizedName;
            this.amount = amount;
            this.passengertype = passengertype;
            this.passengerswanted = passengerswanted;
            this.target = target;
            this.targettype = targettype;
            this.targetfaction = targetfaction;
            this.communal = communal;
            this.expiry = expiry;
            this.influence = influence;
            this.reputation = reputation;
            this.commodityDefinition = commodity;
        }
    }
}
