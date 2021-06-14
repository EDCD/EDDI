using EddiDataDefinitions;
using EddiEvents;
using System;
using Utilities;

namespace EddiMissionMonitor
{
    [PublicAPI]
    public class MissionAcceptedEvent : Event
    {
        public const string NAME = "Mission accepted";
        public const string DESCRIPTION = "Triggered when you accept a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-10T03:25:17Z\", \"event\":\"MissionAccepted\", \"Faction\":\"HIP 20277 Party\", \"Name\":\"Mission_DeliveryWing_Boom\", \"LocalisedName\":\"Boom time delivery of 2737 units of Survival Equipment\", \"Commodity\":\"$SurvivalEquipment_Name;\", \"Commodity_Localised\":\"Survival Equipment\", \"Count\":2737, \"TargetFaction\":\"Guathiti Empire Party\", \"DestinationSystem\":\"Guathiti\", \"DestinationStation\":\"Houtman Landing\", \"Expiry\":\"2018-11-11T02:41:24Z\", \"Wing\":true, \"Influence\":\"+\", \"Reputation\":\"+\", \"Reward\":5391159, \"MissionID\":426330530 }";

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; private set; }

        [PublicAPI("The name of the mission")]
        public string name { get; private set; }

        [PublicAPI("The localised name of the mission")]
        public string localisedname { get; private set; }

        [PublicAPI("The faction issuing the mission")]
        public string faction { get; private set; }

        [PublicAPI("The expected cash reward from the mission")]
        public int? reward { get; private set; }

        [PublicAPI("The increase in the faction's influence in the system gained when completing this mission, if any")]
        public string influence { get; private set; }

        [PublicAPI("The increase in the commander's reputation with the faction gained when completing this mission, if any")]
        public string reputation { get; private set; }

        [PublicAPI("True if the mission allows wing-mates")]
        public bool wing { get; private set; }

        [PublicAPI("The expiry date of the mission")]
        public DateTime? expiry { get; private set; }

        [PublicAPI("The commodity involved in the mission (if applicable)")]
        public string commodity => commodityDefinition?.localizedName;

        [PublicAPI("The micro-resource (on foot item) involved in the mission (if applicable)")]
        public string microresource => microResource?.localizedName;

        [PublicAPI("The amount of the commodity or micro-resource, passengers, or targets involved in the mission (if applicable)")]
        public int? amount { get; private set; }

        [PublicAPI("The destination system for the mission (if applicable)")]
        public string destinationsystem { get; private set; }

        [PublicAPI("The destination station for the mission (if applicable)")]
        public string destinationstation { get; private set; }

        [PublicAPI("The type of passengers in the mission (if applicable)")]
        public string passengertype { get; private set; }

        [PublicAPI("True if the passengers are wanted (if applicable)")]
        public bool? passengerwanted { get; private set; }

        [PublicAPI("True if the passenger is a VIP (if applicable)")]
        public bool? passengervips { get; private set; }

        [PublicAPI("Name of the target of the mission (if applicable)")]
        public string target { get; private set; }

        [PublicAPI("Type of the target of the mission (if applicable)")]
        public string targettype { get; private set; }

        [PublicAPI("Faction of the target of the mission (if applicable)")]
        public string targetfaction { get; private set; }

        [PublicAPI("True if the mission is a community goal")]
        public bool communal { get; private set; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; private set; }

        public MicroResource microResource { get; }

        public MissionAcceptedEvent(DateTime timestamp, long? missionid, string name, string localisedname, string faction, string destinationsystem, string destinationstation, MicroResource microResource, CommodityDefinition commodity, int? amount, bool? passengerwanted, string passengertype, bool? passengervips, string target, string targettype, string targetfaction, bool communal, DateTime? expiry, string influence, string reputation, int? reward, bool wing) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.localisedname = localisedname;
            this.faction = faction;
            this.destinationsystem = destinationsystem;
            this.destinationstation = destinationstation;
            this.commodityDefinition = commodity;
            this.microResource = microResource;
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
        }
    }
}
