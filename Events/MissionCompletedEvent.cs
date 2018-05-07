using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class MissionCompletedEvent: Event
    {
        public const string NAME = "Mission completed";
        public const string DESCRIPTION = "Triggered when you complete a mission";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-02T13:04:35Z"", ""event"":""MissionCompleted"", ""Faction"":""Values Party of Syntec"", ""Name"":""Mission_Courier_Boom_name"", ""MissionID"":55868124, ""DestinationSystem"":""Syntec"", ""DestinationStation"":""Leavitt City"", ""Reward"":20020, ""CommodityReward"":[ { ""Name"": ""ModularTerminals"", ""Count"": 4 } ] }";

    public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static MissionCompletedEvent()
        {
            VARIABLES.Add("missionid", "The ID of the mission");
            VARIABLES.Add("name", "The name of the mission");
            VARIABLES.Add("faction", "The faction receiving the mission");
            VARIABLES.Add("communal", "True if the mission is a community goal");
            VARIABLES.Add("commodity", "The commodity involved in the mission (if applicable)");
            VARIABLES.Add("amount", "The amount of the commodity involved in the mission (if applicable)");
            VARIABLES.Add("reward", "The monetary reward for completing the mission");
            VARIABLES.Add("commodityrewards", "The commodity rewards for completing the mission");
            VARIABLES.Add("donation", "The monetary donation when completing the mission");
            VARIABLES.Add("rewardCommodity", "The commodity reward name (if applicable)");
            VARIABLES.Add("rewardAmount", "The amount of the commodity reward (if applicable)");
        }

        public long? missionid { get; }

        public string name { get; }

        public string faction { get; }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        public CommodityDefinition commodityDefinition { get; }

        public int? amount { get; }

        public bool communal { get; }

        public long reward { get; }

        public List<CommodityAmount> commodityrewards { get; }

        public long donation { get; }

        public string rewardCommodity { get; }

        public int rewardAmount { get; }

        public MissionCompletedEvent(DateTime timestamp, long? missionid, string name, string faction, CommodityDefinition commodity, int? amount, bool communal, long reward, List<CommodityAmount> commodityrewards, long donation) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.faction = faction;
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.communal = communal;
            this.reward = reward;
            this.commodityrewards = commodityrewards;
            this.donation = donation;
            if (commodityrewards.Count > 0)
            {
                this.rewardCommodity = commodityrewards[0].commodity;
                this.rewardAmount = commodityrewards[0].amount;
            }
        }
    }
}
