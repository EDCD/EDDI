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

        public long? missionid { get; private set; }

        public string name { get; private set; }

        public string faction { get; private set; }

        public string commodity { get; private set; }

        public int? amount { get; private set; }

        public bool communal { get; private set; }

        public long reward { get; private set; }

        public List<CommodityAmount> commodityrewards { get; private set; }

        public long donation { get; private set; }

        public string rewardCommodity { get; private set; }

        public int rewardAmount { get; private set; }

        public MissionCompletedEvent(DateTime timestamp, long? missionid, string name, string faction, Commodity commodity, int? amount, bool communal, long reward, List<CommodityAmount> commodityrewards, long donation) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.faction = faction;
            this.commodity = (commodity == null ? null : commodity.name);
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
