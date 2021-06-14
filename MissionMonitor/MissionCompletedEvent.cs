using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiMissionMonitor
{
    [PublicAPI]
    public class MissionCompletedEvent : Event
    {
        public const string NAME = "Mission completed";
        public const string DESCRIPTION = "Triggered when you complete a mission";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-02T13:04:35Z"", ""event"":""MissionCompleted"", ""Faction"":""Values Party of Syntec"", ""Name"":""Mission_Courier_Boom_name"", ""MissionID"":55868124, ""DestinationSystem"":""Syntec"", ""DestinationStation"":""Leavitt City"", ""Reward"":20020, ""CommodityReward"":[ { ""Name"": ""ModularTerminals"", ""Count"": 4 } ] }";

        [PublicAPI("The ID of the mission")]
        public long? missionid { get; }

        [PublicAPI("The name of the mission")]
        public string name { get; }

        [PublicAPI("The faction receiving the mission")]
        public string faction { get; }

        [PublicAPI("The commodity involved in the mission (if applicable)")]
        public string commodity => commodityDefinition?.localizedName;

        [PublicAPI("The micro-resource (on foot item) involved in the mission (if applicable)")]
        public string microresource => microResource?.localizedName;

        [PublicAPI("The amount of the commodity or micro-resource involved in the mission (if applicable)")]
        public int? amount { get; }

        [PublicAPI("True if the mission is a community goal")]
        public bool communal { get; }

        [PublicAPI("The monetary reward for completing the mission")]
        public long reward { get; }

        [PublicAPI("The permits rewarded for completing the mission")]
        public List<string> permitsawarded { get; }

        [PublicAPI("The commodity rewarded for completing the mission")]
        public List<CommodityAmount> commodityrewards { get; }

        [PublicAPI("The materials rewarded for completing the mission")]
        public List<MaterialAmount> materialsrewards { get; }

        [PublicAPI("The micro-resource (on foot items) rewarded for completing the mission")]
        public List<MicroResourceAmount> microresourcerewards { get; }

        [PublicAPI("The monetary donation when completing the mission")]
        public long donation { get; }

        [PublicAPI("The permit reward name (if applicable)")]
        public string rewardPermit { get; }

        [PublicAPI("The commodity reward name (if applicable)")]
        public string rewardCommodity { get; }

        [PublicAPI("The amount of the commodity reward (if applicable)")]
        public int rewardCommodityAmount { get; }

        [PublicAPI("The material reward name (if applicable)")]
        public string rewardMaterial { get; }

        [PublicAPI("The amount of the material reward (if applicable)")]
        public int rewardMaterialAmount { get; }

        [PublicAPI("The micro-resource (on foot item) reward name (if applicable)")]
        public string rewardMicroResource { get; }

        [PublicAPI("The amount of the micro-resource (on foot item) reward (if applicable)")]
        public int rewardMicroResourceAmount { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public MicroResource microResource { get; }

        public MissionCompletedEvent(DateTime timestamp, long? missionid, string name, string faction, MicroResource microResource, CommodityDefinition commodity, int? amount, bool communal, long reward, List<string> permitsawarded = null, List<CommodityAmount> commodityrewards = null, List<MaterialAmount> materialsrewards = null, List<MicroResourceAmount> microResourceRewards = null, long donation = 0) : base(timestamp, NAME)
        {
            this.missionid = missionid;
            this.name = name;
            this.faction = faction;
            this.commodityDefinition = commodity;
            this.microResource = microResource;
            this.amount = amount;
            this.communal = communal;
            this.reward = reward;
            this.permitsawarded = permitsawarded;
            this.commodityrewards = commodityrewards;
            this.materialsrewards = materialsrewards;
            this.microresourcerewards = microResourceRewards;
            this.donation = donation;
            if (permitsawarded?.Count > 0)
            {
                this.rewardPermit = permitsawarded[0];
            }
            if (this.commodityrewards?.Count > 0)
            {
                this.rewardCommodity = commodityrewards[0].commodity;
                this.rewardCommodityAmount = commodityrewards[0].amount;
            }
            if (materialsrewards?.Count > 0)
            {
                this.rewardMaterial = materialsrewards[0].material;
                this.rewardMaterialAmount = materialsrewards[0].amount;
            }
            if (microResourceRewards?.Count > 0)
            {
                this.rewardMicroResource = microResourceRewards[0].microResource?.localizedName;
                this.rewardMicroResourceAmount = microResourceRewards[0].amount;
            }
        }
    }
}
