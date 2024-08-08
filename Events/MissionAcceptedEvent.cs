using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionAcceptedEvent : Event
    {
        public const string NAME = "Mission accepted";
        public const string DESCRIPTION = "Triggered when you accept a mission";
        public const string SAMPLE = "{ \"timestamp\":\"2018-11-10T03:25:17Z\", \"event\":\"MissionAccepted\", \"Faction\":\"HIP 20277 Party\", \"Name\":\"Mission_DeliveryWing_Boom\", \"LocalisedName\":\"Boom time delivery of 2737 units of Survival Equipment\", \"Commodity\":\"$SurvivalEquipment_Name;\", \"Commodity_Localised\":\"Survival Equipment\", \"Count\":2737, \"TargetFaction\":\"Guathiti Empire Party\", \"DestinationSystem\":\"Guathiti\", \"DestinationStation\":\"Houtman Landing\", \"Expiry\":\"2018-11-11T02:41:24Z\", \"Wing\":true, \"Influence\":\"+\", \"Reputation\":\"+\", \"Reward\":5391159, \"MissionID\":426330530 }";

        [PublicAPI("The ID of the mission")] 
        public long missionid => Mission.missionid;

        [PublicAPI("The name of the mission")]
        public string name => Mission.name;

        [PublicAPI("The localised name of the mission")]
        public string localisedname => Mission.localisedname;

        [PublicAPI("A localized list of mission tags / attributes")]
        public List<string> tags => Mission.tags;

        [PublicAPI("An unlocalized list of mission tags / attributes")]
        public List<string> invariantTags => Mission.invariantTags;

        [PublicAPI("The faction issuing the mission")]
        public string faction => Mission.faction;

        [PublicAPI("The expected cash reward from the mission")]
        public long? reward => Mission.reward;

        [PublicAPI("The increase in the faction's influence in the system gained when completing this mission, if any")]
        public string influence => Mission.influence;

        [PublicAPI("The increase in the commander's reputation with the faction gained when completing this mission, if any")]
        public string reputation => Mission.reputation;

        [PublicAPI("True if the mission allows wing-mates")]
        public bool wing => Mission.wing;

        [PublicAPI("The expiry date of the mission")]
        public DateTime? expiry => Mission.expiry;

        [PublicAPI("The commodity involved in the mission (if applicable)")]
        public string commodity => Mission.CommodityDefinition?.localizedName;

        [PublicAPI("The micro-resource (on foot item) involved in the mission (if applicable)")]
        public string microresource => Mission.MicroResourceDefinition?.localizedName;

        [PublicAPI(
            "The amount of the commodity or micro-resource, passengers, or targets involved in the mission (if applicable)")]
        public int? amount => Mission.amount;

        [PublicAPI("The destination system for the mission (if applicable)")]
        public string destinationsystem => Mission.destinationsystem;

        [PublicAPI("The destination station for the mission (if applicable)")]
        public string destinationstation => Mission.destinationstation;

        [PublicAPI("The type of passengers in the mission (if applicable)")]
        public string passengertype => Mission.passengertype;

        [PublicAPI("True if the passengers are wanted (if applicable)")]
        public bool? passengerwanted => Mission.passengerwanted;

        [PublicAPI("True if the passenger is a VIP (if applicable)")]
        public bool? passengervips => Mission.passengervips;

        [PublicAPI("Name of the target of the mission (if applicable)")]
        public string target => Mission.target;

        [PublicAPI("Type of the target of the mission (if applicable)")]
        public string targettype => Mission.targettype;

        [PublicAPI("Faction of the target of the mission (if applicable)")]
        public string targetfaction => Mission.targetfaction;

        [PublicAPI("True if the mission is a community goal")]
        public bool communal => Mission.communal;

        [PublicAPI("The mission object")]
        public Mission Mission { get; }

        public MissionAcceptedEvent(DateTime timestamp, Mission mission) : base(timestamp, NAME)
        {
            this.Mission = mission;
        }
    }
}
