using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class MissionCompletedEvent : Event
    {
        public const string NAME = "Mission completed";
        public const string DESCRIPTION = "Triggered when you complete a mission";
        public const string SAMPLE = @"{ ""timestamp"":""2021-07-24T06:54:44Z"", ""event"":""MissionCompleted"", ""Faction"":""69 G. Carinae Solutions"", ""Name"":""Mission_OnFoot_Onslaught_Offline_MB_name"", ""MissionID"":794589235, ""TargetFaction"":""Amaterasu Silver Brothers"", ""Reward"":269600, ""MaterialsReward"":[ { ""Name"":""SuitSchematic"", ""Name_Localised"":""Suit Schematic"", ""Category"":""$MICRORESOURCE_CATEGORY_Item;"", ""Category_Localised"":""Item"", ""Count"":2 } ], ""FactionEffects"":[ { ""Faction"":""69 G. Carinae Solutions"", ""Effects"":[  ], ""Influence"":[ { ""SystemAddress"":1865920022891, ""Trend"":""UpGood"", ""Influence"":""++"" } ], ""ReputationTrend"":""UpGood"", ""Reputation"":""++"" } ] }";

        [PublicAPI("The ID of the mission")]
        public ulong missionid { get; }

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

        [PublicAPI("The faction effects from completing the mission, as a list")]
        public List<MissionFactionEffect> factionEffects { get; }

        // Not intended to be user facing

        public CommodityDefinition commodityDefinition { get; }

        public MicroResource microResource { get; }

        public MissionCompletedEvent(DateTime timestamp, ulong missionid, string name, string faction, MicroResource microResource, CommodityDefinition commodity, int? amount, bool communal, long reward, List<string> permitsawarded = null, List<CommodityAmount> commodityrewards = null, List<MaterialAmount> materialsrewards = null, List<MicroResourceAmount> microResourceRewards = null, List<MissionFactionEffect> factionEffects = null, long donation = 0) : base(timestamp, NAME)
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
            this.factionEffects = factionEffects;
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

        public static bool Handle ( DateTime timestamp, string edType, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            switch ( edType )
            {
                case "CommunityGoalReward":
                    {
                        var cgid = JsonParsing.getULong(data, "CGID");
                        var name = JsonParsing.getString(data, "Name");
                        data.TryGetValue( "Reward", out var val );
                        var reward = val == null ? 0 : (long)val;

                        events.Add( new MissionCompletedEvent( timestamp, cgid, "MISSION_CommunityGoal", name, null, null, null, true, reward, null, null, null, null, null, 0 ) { raw = line, fromLoad = fromLogLoad } );
                        return true;
                    }
                case "MissionCompleted":
                    {
                        var missionid = JsonParsing.getULong(data, "MissionID");
                        var name = JsonParsing.getString(data, "Name");
                        var reward = JsonParsing.getOptionalLong( data, "Reward" ) ?? 0;
                        var donation = JsonParsing.getOptionalLong(data, "Donated") ?? 0;
                        var faction = EventParsing.FactionName(data, "Faction");

                        // Missions with commodities (which may include on-foot micro-resources)
                        var c = JsonParsing.getString(data, "Commodity");
                        var fallbackC = JsonParsing.getString(data, "Commodity_Localised");
                        CommodityDefinition commodity = null;
                        MicroResource microResource = null;

                        if ( !string.IsNullOrEmpty( c ) )
                        {
                            if ( MicroResource.EDNameExists( c ) )
                            {
                                // This is an on-foot micro-resource
                                microResource = MicroResource.FromEDName( c );
                                microResource.fallbackLocalizedName = fallbackC;
                            }
                            else
                            {
                                // This is (probably) a traditional ship commodity
                                commodity = CommodityDefinition.FromEDName( c );
                                commodity.fallbackLocalizedName = fallbackC;
                            }
                        }
                        var amount = JsonParsing.getOptionalInt(data, "Count");

                        var permitsAwarded = new List<string>();
                        data.TryGetValue( "PermitsAwarded", out var val );
                        var permitsAwardedData = (List<object>)val;
                        if ( permitsAwardedData != null )
                        {
                            foreach ( var permitAwardedData in permitsAwardedData.Cast<IDictionary<string, object>>() )
                            {
                                var permitAwarded = JsonParsing.getString(permitAwardedData, "Name");
                                permitsAwarded.Add( permitAwarded );
                            }
                        }

                        var commodityrewards = new List<CommodityAmount>();
                        data.TryGetValue( "CommodityReward", out val );
                        var commodityRewardsData = (List<object>)val;
                        if ( commodityRewardsData != null )
                        {
                            foreach ( var commodityRewardData in commodityRewardsData.Cast<IDictionary<string, object>>() )
                            {
                                var rewardCommodity = CommodityDefinition.FromEDName(JsonParsing.getString(commodityRewardData, "Name"));
                                var count = JsonParsing.getOptionalInt(commodityRewardData, "Count") ?? 0;
                                if ( rewardCommodity != null )
                                {
                                    commodityrewards.Add( new CommodityAmount( rewardCommodity, count ) );
                                }
                            }
                        }

                        var materialsrewards = new List<MaterialAmount>();
                        var microResourceRewards = new List<MicroResourceAmount>();
                        data.TryGetValue( "MaterialsReward", out val );
                        var materialsRewardsData = (List<object>)val;
                        if ( materialsRewardsData != null )
                        {
                            foreach ( var materialsRewardData in materialsRewardsData.Cast<IDictionary<string, object>>() )
                            {
                                var m = JsonParsing.getString(materialsRewardData, "Name");
                                var fallbackM = JsonParsing.getString(materialsRewardData, "Name_Localised");
                                materialsRewardData.TryGetValue( "Count", out val );
                                var count = (int)(long)val;

                                if ( !string.IsNullOrEmpty( m ) )
                                {
                                    if ( MicroResource.EDNameExists( m ) )
                                    {
                                        // This is an on-foot micro-resource
                                        microResourceRewards.Add( new MicroResourceAmount( m, null, count, null, fallbackM ) );
                                    }
                                    else
                                    {
                                        // This is (probably) a traditional ship material
                                        var rewardMaterial = Material.FromEDName(m);
                                        rewardMaterial.fallbackLocalizedName = fallbackM;
                                        materialsrewards.Add( new MaterialAmount( rewardMaterial, count ) );
                                    }
                                }
                            }
                        }

                        var missionFactionEffects = new List<MissionFactionEffect>();
                        data.TryGetValue( "FactionEffects", out val );
                        var missionFactionEffectsData = (List<object>)val;
                        if ( missionFactionEffectsData != null )
                        {
                            foreach ( var missionFactionEffectData in missionFactionEffectsData.Cast<IDictionary<string, object>>() )
                            {
                                var effectFaction = JsonParsing.getString(missionFactionEffectData, "Faction");
                                var reputationPlusses = JsonParsing.getString(missionFactionEffectData, "Reputation");

                                var effects = new List<MissionEffect>();
                                missionFactionEffectData.TryGetValue( "Effects", out val );
                                var effectsData = (List<object>)val;
                                if ( effectsData != null )
                                {
                                    foreach ( var effectData in effectsData.Cast<IDictionary<string, object>>() )
                                    {
                                        var edEffect = JsonParsing.getString(effectData, "Effect");
                                        var localizedEffect = JsonParsing.getString(effectData, "Effect_Localised");
                                        effects.Add( new MissionEffect( edEffect, localizedEffect ) );
                                    }
                                }

                                var influences = new List<MissionInfluence>();
                                missionFactionEffectData.TryGetValue( "Influence", out val );
                                var influencesData = (List<object>)val;
                                if ( influencesData != null )
                                {
                                    foreach ( var influenceData in influencesData.Cast<IDictionary<string, object>>() )
                                    {
                                        var influencedSystemAddress = JsonParsing.getULong(influenceData, "SystemAddress");
                                        var influencePlusses = JsonParsing.getString(influenceData, "Influence");
                                        influences.Add( new MissionInfluence( influencedSystemAddress, influencePlusses ) );
                                    }
                                }

                                missionFactionEffects.Add( new MissionFactionEffect( effectFaction, effects, influences, reputationPlusses ) );
                            }
                        }

                        events.Add( new MissionCompletedEvent( timestamp, missionid, name, faction, microResource, commodity, amount, false, reward, permitsAwarded, commodityrewards, materialsrewards, microResourceRewards, missionFactionEffects, donation ) { raw = line, fromLoad = fromLogLoad } );
                        return true;
                    }
                default:
                    return false;
            }
        }
    }
}
