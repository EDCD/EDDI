﻿using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class MissionMonitorTests : TestBase
    {
        MissionMonitor missionMonitor = new MissionMonitor();
        Mission mission;
        string line;
        List<Event> events;

        [TestInitialize]
        public void StartTestMissionMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestMissionsConfig()
        {
            string missionConfigJson = @"{
                ""missions"": [{
                    ""name"": ""Mission_Courier_Elections"",
                    ""localisedname"": ""Courier required for sensitive poll data"",
                    ""targetTypeEDName"": null,
                    ""missionid"": 413563499,
                    ""typeEDName"": ""Courier"",
                    ""statusEDName"": ""Active"",
                    ""originsystem"": ""HIP 20277"",
                    ""originstation"": ""Fabian City"",
                    ""originreturn"": false,
                    ""faction"": ""Calennero Shared"",
                    ""factionstate"": null,
                    ""influence"": ""Med"",
                    ""reputation"": ""Med"",
                    ""legal"": true,
                    ""wing"": false,
                    ""shared"": false,
                    ""communal"": false,
                    ""reward"": 99656,
                    ""commodity"": null,
                    ""amount"": 0,
                    ""destinationsystem"": ""Bakara"",
                    ""destinationstation"": ""Arzachel Colony"",
                    ""destinationsystems"": [],
                    ""passengertypeEDName"": null,
                    ""passengerwanted"": null,
                    ""passengervips"": null,
                    ""target"": null,
                    ""targetfaction"": ""Bakara Incorporated"",
                    ""expiry"": ""2018-08-26T13:02:37Z""
                },
                {
                    ""name"": ""Mission_Delivery"",
                    ""localisedname"": ""Deliver 180 units of Basic Medicines"",
                    ""targetTypeEDName"": null,
                    ""missionid"": 413563678,
                    ""typeEDName"": ""Delivery"",
                    ""statusEDName"": ""Active"",
                    ""originsystem"": ""HIP 20277"",
                    ""originstation"": ""Fabian City"",
                    ""originreturn"": false,
                    ""faction"": ""HIP 20277 Inc"",
                    ""factionstate"": null,
                    ""influence"": ""Med"",
                    ""reputation"": ""Med"",
                    ""legal"": true,
                    ""wing"": false,
                    ""shared"": false,
                    ""communal"": false,
                    ""reward"": 1586260,
                    ""commodity"": ""Basic Medicines"",
                    ""amount"": 180,
                    ""destinationsystem"": ""Hirapa"",
                    ""destinationstation"": ""Payne-Gaposchkin Port"",
                    ""destinationsystems"": [],
                    ""passengertypeEDName"": null,
                    ""passengerwanted"": null,
                    ""passengervips"": null,
                    ""target"": null,
                    ""targetfaction"": ""Hirapa Energy Company"",
                    ""expiry"": ""2018-08-26T13:02:38Z""
                },
                {
                    ""name"": ""Mission_Salvage_Planet"",
                    ""localisedname"": ""Salvage 4 Landmines"",
                    ""targetTypeEDName"": null,
                    ""missionid"": 413563829,
                    ""typeEDName"": ""Salvage"",
                    ""statusEDName"": ""Active"",
                    ""originsystem"": ""HIP 20277"",
                    ""originstation"": ""Fabian City"",
                    ""originreturn"": true,
                    ""faction"": ""HIP 20277 Inc"",
                    ""factionstate"": null,
                    ""influence"": ""Med"",
                    ""reputation"": ""Med"",
                    ""legal"": true,
                    ""wing"": false,
                    ""shared"": false,
                    ""communal"": false,
                    ""reward"": 465824,
                    ""commodity"": ""Landmines"",
                    ""amount"": 4,
                    ""destinationsystem"": ""Carthage"",
                    ""destinationstation"": null,
                    ""destinationsystems"": [],
                    ""passengertypeEDName"": null,
                    ""passengerwanted"": null,
                    ""passengervips"": null,
                    ""target"": null,
                    ""targetfaction"": null,
                    ""expiry"": ""2018-08-29T00:56:33Z""
                }],
                ""missionsCount"": 3,
                ""missionWarning"": 60
            }";
            // Save original data
            MissionMonitorConfiguration data = MissionMonitorConfiguration.FromFile();

            MissionMonitorConfiguration config = MissionMonitorConfiguration.FromJsonString(missionConfigJson);
            Assert.AreEqual(config.missionsCount, config.missions.Count);

            mission = config.missions.ToList().FirstOrDefault(m => m.missionid == 413563499);
            Assert.AreEqual("Courier", mission.typeEDName);
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.IsFalse(mission.originreturn);
            Assert.IsTrue(mission.legal);
            Assert.IsFalse(mission.shared);
            Assert.IsFalse(mission.wing);
            Assert.AreEqual(99656, mission.reward);

            mission = config.missions.ToList().FirstOrDefault(m => m.missionid == 413563678);
            Assert.AreEqual("Delivery", mission.typeEDName);
            Assert.AreEqual(180, mission.amount);

            mission = config.missions.ToList().FirstOrDefault(m => m.missionid == 413563829);
            Assert.AreEqual("Salvage", mission.typeEDName);
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.AreEqual(4, mission.amount);
            Assert.IsTrue(mission.originreturn);

            // Restore original data
            data.ToFile();
        }

        [TestMethod]
        public void TestMissionEventsScenario()
        {
            // Save original data
            MissionMonitorConfiguration missionData = MissionMonitorConfiguration.FromFile();

            missionMonitor.initializeMissionMonitor(new MissionMonitorConfiguration());

            //MissionsEvent
            line = @"{""timestamp"":""2018-08-25T23:27:21Z"", ""event"":""Missions"", ""Active"":[ { ""MissionID"":413563499, ""Name"":""Mission_Courier_Elections_name"", ""PassengerMission"":false, ""Expires"":48916 }, { ""MissionID"":413563678, ""Name"":""Mission_Delivery_name"", ""PassengerMission"":false, ""Expires"":48917 }, { ""MissionID"":413563829, ""Name"":""Mission_Salvage_Planet_name"", ""PassengerMission"":false, ""Expires"":264552 } ], ""Failed"":[  ], ""Complete"":[  ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionsEvent((MissionsEvent)events[0]);
            Assert.AreEqual(3, missionMonitor.missions.Count);
            Assert.AreEqual(3, missionMonitor.missions.Where(m => m.statusEDName == "Active").Count());

            //CargoDepotEvent - 'Shared'
            line = @"{ ""timestamp"":""2018-08-26T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748365, ""UpdateType"":""WingUpdate"", ""CargoType"":""Gold"", ""Count"":20, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":20, ""TotalItemsToDeliver"":54, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleCargoDepotEvent((CargoDepotEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748365);
            Assert.AreEqual(4, missionMonitor.missions.Count);
            Assert.AreEqual("CollectWing", mission.typeEDName);
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.IsTrue(mission.originreturn);
            Assert.IsTrue(mission.wing);
            Assert.IsTrue(mission.shared);
            line = @"{ ""timestamp"":""2018-08-26T02:56:16Z"", ""event"":""CargoDepot"", ""MissionID"":413748365, ""UpdateType"":""Deliver"", ""CargoType"":""Gold"", ""Count"":34, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":54, ""TotalItemsToDeliver"":54, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            missionMonitor._handleCargoDepotEvent((CargoDepotEvent)events[0]);
            Assert.AreEqual(3, missionMonitor.missions.Count);

            //MissionAbandonedEvent
            line = @"{ ""timestamp"":""2018-08-26T00:50:48Z"", ""event"":""MissionAbandoned"", ""Name"":""Mission_Courier_Elections_name"", ""Fine"":50000, ""MissionID"":413563499 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor.handleMissionAbandonedEvent((MissionAbandonedEvent)events[0]);
            Assert.AreEqual("Failed", missionMonitor.missions.SingleOrDefault(m => m.missionid == 413563499)?.statusEDName);
            missionMonitor._postHandleMissionAbandonedEvent((MissionAbandonedEvent)events[0]);
            Assert.AreEqual(2, missionMonitor.missions.Count);

            //MissionAcceptedEvent - 'AltruismCredits'
            line = "{ \"timestamp\":\"2018-09-17T02:54:16Z\", \"event\":\"MissionAccepted\", \"Faction\":\"Merope Expeditionary Fleet\", \"Name\":\"Mission_AltruismCredits\", \"LocalisedName\":\"Donate 450,000 Cr to the cause\", \"Donation\":\"450000\", \"Expiry\":\"2018-09-17T05:01:28Z\", \"Wing\":false, \"Influence\":\"Med\", \"Reputation\":\"Med\", \"MissionID\":419646649 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 419646649);
            Assert.AreEqual(3, missionMonitor.missions.Count);
            Assert.IsTrue(mission.originreturn);

            //MissionAcceptedEvent - 'Collect'
            line = @"{ ""timestamp"":""2018-08-26T00:50:48Z"", ""event"":""MissionAccepted"", ""Faction"":""Calennero State Industries"", ""Name"":""Mission_Collect_Industrial"", ""LocalisedName"":""Industry needs 54 units of Tantalum"", ""Commodity"":""$Tantalum_Name;"", ""Commodity_Localised"":""Tantalum"", ""Count"":54, ""DestinationSystem"":""HIP 20277"", ""DestinationStation"":""Fabian City"", ""Expiry"":""2018-08-27T00:48:38Z"", ""Wing"":false, ""Influence"":""Med"", ""Reputation"":""Med"", ""Reward"":1909532, ""MissionID"":413748324 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748324);
            Assert.AreEqual(4, missionMonitor.missions.Count);
            Assert.AreEqual("Collect", mission.typeEDName);
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.IsTrue(mission.originreturn);
            Assert.IsTrue(mission.legal);
            Assert.IsFalse(mission.wing);

            // Verify duplication protection
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            Assert.AreEqual(1, missionMonitor.missions.ToList().Where(m => m.missionid == 413748324).Count());
            Assert.AreEqual(4, missionMonitor.missions.Count);

            //CargoDepotEvent - 'Collect'
            line = @"{ ""timestamp"":""2018-08-26T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748324, ""UpdateType"":""Deliver"", ""CargoType"":""Tantalum"", ""Count"":54, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":54, ""TotalItemsToDeliver"":54, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleCargoDepotEvent((CargoDepotEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748324);
            Assert.AreEqual("Complete", mission.statusEDName);

            //MissionAcceptedEvent - 'Permit'
            line = "{ \"timestamp\":\"2018-09-19T01:12:57Z\", \"event\":\"MissionAccepted\", \"Faction\":\"Sublime Order of van Maanen's Star\", \"Name\":\"MISSION_genericPermit1\", \"LocalisedName\":\"Permit Acquisition Opportunity\", \"Wing\":false, \"Influence\":\"None\", \"Reputation\":\"None\", \"MissionID\":420098082 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 420098082);
            Assert.AreEqual(5, missionMonitor.missions.Count);

            //MissionAcceptedEvent - 'Smuggle'
            line = @"{ ""timestamp"":""2018-08-29T20:51:56Z"", ""event"":""MissionAccepted"", ""Faction"":""Gcirithang Crimson Mafia"", ""Name"":""Mission_Smuggle_Famine"", ""LocalisedName"":""Smuggle 36 units of Narcotics to combat famine"", ""Commodity"":""$BasicNarcotics_Name;"", ""Commodity_Localised"":""Narcotics"", ""Count"":36, ""DestinationSystem"":""Carcinus"", ""DestinationStation"":""Wye-Delta Station"", ""Expiry"":""2018-08-30T20:55:33Z"", ""Wing"":false, ""Influence"":""Med"", ""Reputation"":""Med"", ""Reward"":180818, ""MissionID"":414732731 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 414732731);
            Assert.AreEqual(6, missionMonitor.missions.Count);
            Assert.AreEqual("Smuggle", mission.typeEDName);
            Assert.IsFalse(mission.originreturn);
            Assert.IsFalse(mission.legal);

            //MissionCompletedEvent
            line = @"{ ""timestamp"":""2018-08-26T00:40:14Z"", ""event"":""MissionCompleted"", ""Faction"":""HIP 20277 Inc"", ""Name"":""Mission_Salvage_Planet_name"", ""MissionID"":413563829, ""Commodity"":""$Landmines_Name;"", ""Commodity_Localised"":""Landmines"", ""Count"":4, ""DestinationSystem"":""Carthage"", ""Reward"":465824, ""FactionEffects"":[ { ""Faction"":""HIP 20277 Inc"", ""Effects"":[ { ""Effect"":""$MISSIONUTIL_Interaction_Summary_civilUnrest_down;"", ""Effect_Localised"":""$#MinorFaction; are happy to report improved civil contentment, making a period of civil unrest unlikely."", ""Trend"":""DownGood"" } ], ""Influence"":[ { ""SystemAddress"":84053791442, ""Trend"":""UpGood"" } ], ""Reputation"":""UpGood"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor.handleMissionCompletedEvent((MissionCompletedEvent)events[0]);
            Assert.AreEqual("Complete", missionMonitor.missions.SingleOrDefault(m => m.missionid == 413563829)?.statusEDName);
            missionMonitor._postHandleMissionCompletedEvent((MissionCompletedEvent)events[0]);
            Assert.AreEqual(5, missionMonitor.missions.Count);

            //MissionFailedEvent
            line = @"{ ""timestamp"":""2018-08-26T00:50:48Z"", ""event"":""MissionFailed"", ""Name"":""Mission_Collect_Industrial"", ""Fine"":50000, ""MissionID"":413748324 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);

            CrimeMonitorConfiguration crimeData = CrimeMonitorConfiguration.FromFile();
            CrimeMonitor crimeMonitor = new CrimeMonitor();
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748324);
            long fine = ((MissionFailedEvent)events[0]).fine;
            crimeMonitor._handleMissionFine(events[0].timestamp, mission, fine);
            FactionRecord record = crimeMonitor.criminalrecord.ToList().FirstOrDefault(r => r.faction == mission.faction);
            Assert.IsNotNull(record);
            Assert.AreEqual(50000, record.fines);
            FactionReport report = record.factionReports.FirstOrDefault(r => r.crimeDef == Crime.FromEDName("missionFine"));
            Assert.IsNotNull(report);
            Assert.AreEqual(50000, report.amount);
            crimeData.ToFile();

            missionMonitor.handleMissionFailedEvent((MissionFailedEvent)events[0]);
            Assert.AreEqual("Failed", missionMonitor.missions.SingleOrDefault(m => m.missionid == 413748324)?.statusEDName); 
            missionMonitor._postHandleMissionFailedEvent((MissionFailedEvent)events[0]);
            Assert.AreEqual(4, missionMonitor.missions.Count);

            //MissionCompletedEvent - Donation
            line = @"{ ""timestamp"":""2018-12-18T19:14:32Z"", ""event"":""MissionCompleted"", ""Faction"":""Movement for Rabakshany Democrats"", ""Name"":""Mission_AltruismCredits_name"", ""MissionID"":442085549, ""Donation"":""1000000"", ""Donated"":1000000, ""FactionEffects"":[ { ""Faction"":""Movement for Rabakshany Democrats"", ""Effects"":[ { ""Effect"":""$MISSIONUTIL_Interaction_Summary_EP_up;"", ""Effect_Localised"":""The economic status of $#MinorFaction; has improved in the $#System; system."", ""Trend"":""UpGood"" } ], ""Influence"":[ { ""SystemAddress"":8605201797850, ""Trend"":""UpGood"", ""Influence"":""+++++"" } ], ""ReputationTrend"":""UpGood"", ""Reputation"":""++"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            MissionCompletedEvent mcEvent = (MissionCompletedEvent)events[0];
            Assert.AreEqual(1000000, mcEvent.donation);

            // Restore original data
            missionData.ToFile();
        }

        [TestMethod]
        public void TestCommunityGoalRewardEvent()
        {
            string line = @"{ ""timestamp"":""2019-01-27T06:14:02Z"", ""event"":""CommunityGoalReward"", ""CGID"":568, ""Name"":""Distant Worlds Mining Initiative"", ""System"":""Omega Sector VE-Q b5-15"", ""Reward"":23000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            MissionCompletedEvent mcEvent = (MissionCompletedEvent)events[0];
            Assert.IsInstanceOfType(mcEvent, typeof(MissionCompletedEvent));
        }

        [TestMethod]
        public void TestCommunityGoalScenario()
        {
            // Save original data
            MissionMonitorConfiguration missionData = MissionMonitorConfiguration.FromFile();

            missionMonitor.initializeMissionMonitor(new MissionMonitorConfiguration());

            // MissionsEvent
            line = @"{ ""timestamp"":""2020-10-24T01:39:13Z"", ""event"":""Missions"", ""Active"":[  ], ""Failed"":[  ], ""Complete"":[  ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionsEvent((MissionsEvent)events[0]);
            Assert.AreEqual(0, missionMonitor.missions.Count);

            // CommunityGoalJoin
            line = @"{ ""timestamp"":""2020-10-24T05:05:25Z"", ""event"":""CommunityGoalJoin"", ""CGID"":619, ""Name"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""System"":""LFT 625"" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            Assert.AreEqual(1, missionMonitor.missions.Count);
            Assert.AreEqual(619, missionMonitor.missions[0].missionid);
            Assert.AreEqual("Federal Initiative to Deliver Supplies for Marlinist Refugees", missionMonitor.missions[0].localisedname);
            Assert.AreEqual("LFT 625", missionMonitor.missions[0].originsystem);

            // CommunityGoal (active)
            line = @"{ ""timestamp"":""2020-10-25T05:05:51Z"", ""event"":""CommunityGoal"", ""CurrentGoals"":[ { ""CGID"":619, ""Title"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:27Z"", ""IsComplete"":false, ""CurrentTotal"":14801430, ""PlayerContribution"":5516, ""NumContributors"":4560, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":""All systems supported"" }, ""TopRankSize"":10, ""PlayerInTopRank"":false, ""TierReached"":""Tier 1"", ""PlayerPercentileBand"":25, ""Bonus"":600000 }, { ""CGID"":620, ""Title"":""Federal Initiative to Protect Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:17Z"", ""IsComplete"":false, ""CurrentTotal"":6029526816, ""PlayerContribution"":281617, ""NumContributors"":2879, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":"""" }, ""TopRankSize"":10, ""PlayerInTopRank"":false, ""TierReached"":""Tier 2"", ""PlayerPercentileBand"":100, ""Bonus"":150000 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleCommunityGoalEvent((CommunityGoalEvent)events[0]);
            Assert.AreEqual("Fox Enterprise", missionMonitor.missions[0].originstation);
            Assert.AreEqual("Active", missionMonitor.missions[0].statusEDName);
            Assert.AreEqual(600000, missionMonitor.missions[0].reward);

            // CommunityGoal (completed)
            line = @"{ ""timestamp"":""2020-10-30T01:22:34Z"", ""event"":""CommunityGoal"", ""CurrentGoals"":[ { ""CGID"":619, ""Title"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:27Z"", ""IsComplete"":true, ""CurrentTotal"":33417668, ""PlayerContribution"":6304, ""NumContributors"":0, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":""All systems supported"" }, ""TopRankSize"":10, ""PlayerInTopRank"":true, ""TierReached"":""Tier 3"", ""PlayerPercentileBand"":0, ""Bonus"":10000000 }, { ""CGID"":620, ""Title"":""Federal Initiative to Protect Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:17Z"", ""IsComplete"":true, ""CurrentTotal"":18046921733, ""PlayerContribution"":4899320, ""NumContributors"":0, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":"""" }, ""TopRankSize"":10, ""PlayerInTopRank"":true, ""TierReached"":""Tier 3"", ""PlayerPercentileBand"":0, ""Bonus"":10000000 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleCommunityGoalEvent((CommunityGoalEvent)events[0]);
            Assert.AreEqual("Claim", missionMonitor.missions[0].statusEDName);
            Assert.AreEqual(10000000, missionMonitor.missions[0].reward);

            // CommunityGoalReward
            line = @"{ ""timestamp"":""2020-10-30T01:24:27Z"", ""event"":""CommunityGoalReward"", ""CGID"":619, ""Name"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""System"":""LFT 625"", ""Reward"":10000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._postHandleMissionCompletedEvent((MissionCompletedEvent)events[0]);
            Assert.AreEqual(1, missionMonitor.missions.Count);

            // MissionsEvent
            line = @"{ ""timestamp"":""2020-10-30T01:30:27Z"", ""event"":""Missions"", ""Active"":[  ], ""Failed"":[  ], ""Complete"":[  ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            missionMonitor._handleMissionsEvent((MissionsEvent)events[0]);
            Assert.AreEqual(0, missionMonitor.missions.Count);

            // Restore original data
            missionData.ToFile();
        }

        [TestCleanup]
        private void StopTestMissionMonitor()
        {
        }
    }
}
