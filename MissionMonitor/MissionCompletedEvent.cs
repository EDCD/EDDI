using EddiEvents;
using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiMissionMonitor
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
            VARIABLES.Add("permitsawarded", "The permits rewarded for completing the mission");
            VARIABLES.Add("commodityrewards", "The commodity rewarded for completing the mission");
            VARIABLES.Add("materialsrewards", "The materials rewarded for completing the mission");
            VARIABLES.Add("donation", "The monetary donation when completing the mission");
            VARIABLES.Add("rewardPermit", "The permit reward name (if applicable)");
            VARIABLES.Add("rewardCommodity", "The commodity reward name (if applicable)");
            VARIABLES.Add("rewardCommodityAmount", "The amount of the commodity reward (if applicable)");
            VARIABLES.Add("rewardMaterial", "The material reward name (if applicable)");
            VARIABLES.Add("rewardMaterialAmount", "The amount of the material reward (if applicable)");

        }

        public long? missionid { get; }

        public string name { get; }

        public string faction { get; }

        public string commodity => commodityDefinition?.localizedName ?? "unknown commodity";

        public CommodityDefinition commodityDefinition { get; }

        public int? amount { get; }

        public bool communal { get; }

        public long reward { get; }

        public List<string> permitsawarded { get; }

        public List<CommodityAmount> commodityrewards { get; }

        public List<MaterialAmount> materialsrewards { get; }

        public long donation { get; }

        public string rewardPermit { get; }

        public string rewardCommodity { get; }

        public int rewardCommodityAmount { get; }

        public string rewardMaterial { get; }

        public int rewardMaterialAmount { get; }

        public MissionCompletedEvent(DateTime timestamp, long? missionid, string name, string faction, CommodityDefinition commodity, int? amount, bool communal, long reward, List<string> permitsawarded, List<CommodityAmount> commodityrewards, List<MaterialAmount> materialsrewards, long donation) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.faction = faction;
            this.commodityDefinition = commodity;
            this.amount = amount;
            this.communal = communal;
            this.reward = reward;
            this.permitsawarded = permitsawarded;
            this.commodityrewards = commodityrewards ?? new List<CommodityAmount>();
            this.materialsrewards = materialsrewards ?? new List<MaterialAmount>();
            this.donation = donation;
            if (permitsawarded.Count > 0)
            {
                this.rewardPermit = permitsawarded[0];
            }
            if (this.commodityrewards.Count > 0)
            {
                this.rewardCommodity = commodityrewards[0].commodity;
                this.rewardCommodityAmount = commodityrewards[0].amount;
            }
            if (materialsrewards.Count > 0)
            {
                this.rewardMaterial = materialsrewards[0].material;
                this.rewardMaterialAmount = materialsrewards[0].amount;
            }

        }
    }
}
