using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiMissionMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class MissionMonitorTests : TestBase
    {
        private readonly MissionMonitor missionMonitor = new();

        [TestInitialize]
        public void StartTestMissionMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestMissionsConfig()
        {
            var missionConfigJson = @"{
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
            var data = ConfigService.Instance.missionMonitorConfiguration;

            var config = ConfigService.FromJson<MissionMonitorConfiguration>(missionConfigJson);
            Assert.HasCount( config.missionsCount, config.missions);

            var mission = config.missions.ToList().FirstOrDefault(m => m.missionid == 413563499);
            Assert.IsNotNull(mission);
            Assert.Contains( "Courier", mission.edTags );
            Assert.Contains( "Election", mission.edTags );
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.IsFalse(mission.originreturn);
            Assert.IsTrue(mission.legal);
            Assert.IsFalse(mission.shared);
            Assert.IsFalse(mission.wing);
            Assert.AreEqual(99656, mission.reward);

            mission = config.missions.ToList().FirstOrDefault(m => m.missionid == 413563678);
            Assert.IsNotNull(mission);
            Assert.Contains( "Delivery", mission.edTags );
            Assert.AreEqual(180, mission.amount);

            mission = config.missions.ToList().FirstOrDefault(m => m.missionid == 413563829);
            Assert.IsNotNull(mission);
            Assert.Contains( "Salvage", mission.edTags );
            Assert.Contains( "Planet", mission.edTags );
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.AreEqual(4, mission.amount);
            Assert.IsTrue(mission.originreturn);

            // Restore original data
            ConfigService.Instance.missionMonitorConfiguration = data;
        }

        [TestMethod, DoNotParallelize]
        public async Task TestMissionEventsScenario()
        {
            EDDI.Instance.DataProvider = CreateTestDataProvider();
            FakeSpanshHttpClient.Expect( "systems/field_values/system_names?q=Calennero", @"{""min_max"":[{""id64"":5581611930322,""name"":""Calennero"",""x"":104.15625,""y"":-99.5625,""z"":-6.15625}],""values"":[""Calennero""]}" );
            FakeSpanshHttpClient.Expect( "systems/field_values/system_names?q=Carthage", @"{""min_max"":[{""id64"":27084711092,""name"":""Carthage"",""x"":100.34375,""y"":-102.125,""z"":7.34375}],""values"":[""Carthage""]}" );
            FakeSpanshHttpClient.Expect( "systems/field_values/system_names?q=HIP 20277", @"{""min_max"":[{""id64"":84053791442,""name"":""HIP 20277"",""x"":106.4375,""y"":-95.6875,""z"":-0.1875},{""id64"":678197209097,""name"":""HD 202776"",""x"":538.0,""y"":-1533.96875,""z"":1532.40625},{""id64"":83583472490,""name"":""HD 202774"",""x"":-176.4375,""y"":-751.78125,""z"":739.8125},{""id64"":52829349036,""name"":""HIP 114084"",""x"":-834.03125,""y"":-122.5,""z"":-257.90625},{""id64"":52829357244,""name"":""HIP 102794"",""x"":-784.03125,""y"":-2.75,""z"":77.5},{""id64"":52833551516,""name"":""HIP 9231"",""x"":-577.4375,""y"":77.0625,""z"":-477.5625},{""id64"":52833551540,""name"":""HIP 104270"",""x"":-677.5,""y"":-15.21875,""z"":24.59375},{""id64"":52833559724,""name"":""HIP 107004"",""x"":-660.8125,""y"":145.125,""z"":-201.75},{""id64"":52837729460,""name"":""HIP 113211"",""x"":-402.53125,""y"":-300.8125,""z"":8.96875},{""id64"":52837737628,""name"":""HIP 13238"",""x"":-449.0625,""y"":-104.125,""z"":-578.40625},{""id64"":52837737652,""name"":""HIP 109818"",""x"":-540.53125,""y"":-60.78125,""z"":-81.625},{""id64"":52837737660,""name"":""HIP 102037"",""x"":-502.84375,""y"":-93.375,""z"":189.5625},{""id64"":52837745828,""name"":""HIP 15069"",""x"":-397.0625,""y"":77.90625,""z"":-424.75},{""id64"":52837745852,""name"":""HIP 97155"",""x"":-505.71875,""y"":95.21875,""z"":91.75},{""id64"":52837745860,""name"":""HIP 94838"",""x"":-447.8125,""y"":30.1875,""z"":322.78125},{""id64"":52837754012,""name"":""HIP 24664"",""x"":-457.25,""y"":257.65625,""z"":-511.65625},{""id64"":52837754052,""name"":""HIP 89895"",""x"":-509.65625,""y"":209.5625,""z"":309.65625},{""id64"":52841915596,""name"":""HIP 105805"",""x"":-357.8125,""y"":-438.09375,""z"":394.40625},{""id64"":52841923740,""name"":""HIP 14412"",""x"":-282.46875,""y"":-232.21875,""z"":-551.375},{""id64"":52841931940,""name"":""HIP 14363"",""x"":-228.375,""y"":-114.65625,""z"":-377.90625}],""values"":[""HIP 20277"",""HD 202776"",""HD 202774"",""HIP 114084"",""HIP 102794"",""HIP 9231"",""HIP 104270"",""HIP 107004"",""HIP 113211"",""HIP 13238"",""HIP 109818"",""HIP 102037"",""HIP 15069"",""HIP 97155"",""HIP 94838"",""HIP 24664"",""HIP 89895"",""HIP 105805"",""HIP 14412"",""HIP 14363""]}" );
            FakeSpanshHttpClient.Expect( "dump/5581611930322", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpCalenerro ) );
            FakeSpanshHttpClient.Expect( "dump/27084711092", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpCarthage ) );
            FakeSpanshHttpClient.Expect( "dump/84053791442", Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpHIP_20277 ) );
            FakeSpanshHttpClient.Expect( @"systems/search?json={""filters"":{""minor_faction_presences"":[{""name"":{""value"":[""Calennero State Industries""]}}]},""size"":500,""page"":0}", Encoding.UTF8.GetString( Resources.SpanshQueryFactionCalenneroCrew ) );
            FakeSpanshHttpClient.Expect( @"stations/search?json={""filters"":{""controlling_minor_faction"":{""value"":[""Calennero State Industries""]}},""size"":1,""page"":0}", Encoding.UTF8.GetString( Resources.SpanshQueryStationFactionCalenneroCrew ) );

            // Save original data
            var missionData = ConfigService.Instance.missionMonitorConfiguration;

            missionMonitor.initializeMissionMonitor(new MissionMonitorConfiguration());

            //MissionsEvent
            var line = @"{""timestamp"":""2018-08-25T23:27:21Z"", ""event"":""Missions"", ""Active"":[ { ""MissionID"":413563499, ""Name"":""Mission_Courier_Elections_name"", ""PassengerMission"":false, ""Expires"":48916 }, { ""MissionID"":413563678, ""Name"":""Mission_Delivery_name"", ""PassengerMission"":false, ""Expires"":48917 }, { ""MissionID"":413563829, ""Name"":""Mission_Salvage_Planet_name"", ""PassengerMission"":false, ""Expires"":264552 } ], ""Failed"":[  ], ""Complete"":[  ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionsEvent((MissionsEvent)events[0]);
            Assert.HasCount( 3, missionMonitor.missions );
            Assert.AreEqual(3, missionMonitor.missions.Count( m => m.statusEDName == "Active" ));

            //CargoDepotEvent - 'Shared'
            line = @"{ ""timestamp"":""2018-08-26T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748365, ""UpdateType"":""WingUpdate"", ""CargoType"":""Gold"", ""Count"":20, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":20, ""TotalItemsToDeliver"":54, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleCargoDepotEvent((CargoDepotEvent)events[0]);
            var mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748365);
            Assert.HasCount( 4, missionMonitor.missions );
            Assert.IsNotNull(mission);
            Assert.Contains( "Collect", mission.edTags );
            Assert.Contains( "Wing", mission.edTags );
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.IsTrue(mission.originreturn);
            Assert.IsTrue(mission.wing);
            Assert.IsTrue(mission.shared);
            Assert.AreEqual( "Gold", mission.CommodityDefinition.invariantName );
            Assert.AreEqual( 0, mission.collected );
            Assert.AreEqual( 20, mission.delivered );
            Assert.AreEqual( 54, mission.amount );
            Assert.AreEqual( 0, mission.startmarketid );
            Assert.AreEqual( 3224777216, mission.endmarketid );
            line = @"{ ""timestamp"":""2018-08-27T02:56:16Z"", ""event"":""CargoDepot"", ""MissionID"":413748365, ""UpdateType"":""Deliver"", ""CargoType"":""Gold"", ""Count"":34, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":54, ""TotalItemsToDeliver"":54, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            missionMonitor._handleCargoDepotEvent((CargoDepotEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748365);
            Assert.IsNotNull( mission );
            Assert.AreEqual(MissionStatus.Claim, mission.statusDef);
            Assert.HasCount( 4, missionMonitor.missions );
            Assert.AreEqual( "Gold", mission.CommodityDefinition.invariantName );
            Assert.AreEqual( 0, mission.collected );
            Assert.AreEqual( 54, mission.delivered );
            Assert.AreEqual( 54, mission.amount );
            Assert.AreEqual( 0, mission.startmarketid );
            Assert.AreEqual( 3224777216, mission.endmarketid );
            line = @"{ ""timestamp"":""2018-08-27T02:56:36Z"", ""event"":""MissionCompleted"", ""Faction"":""Sanctuary Inc"", ""Name"":""Mission_Collect_CivilLiberty_name"", ""LocalisedName"":""Supplier needs 54 units of Gold"", ""MissionID"":413748365, ""Commodity"":""$Gold_Name;"", ""Commodity_Localised"":""Gold"", ""Count"":54, ""DestinationSystem"":""Xi Wangda"", ""DestinationStation"":""Cartier City"", ""Reward"":30448920, ""FactionEffects"":[ { ""Faction"":""Sanctuary Inc"", ""Effects"":[ { ""Effect"":""$MISSIONUTIL_Interaction_Summary_EP_up;"", ""Effect_Localised"":""The economic status of $#MinorFaction; has improved in the $#System; system."", ""Trend"":""UpGood"" } ], ""Influence"":[ { ""SystemAddress"":2869708727713, ""Trend"":""UpGood"", ""Influence"":""+++++"" } ], ""ReputationTrend"":""UpGood"", ""Reputation"":""++"" } ] }";
            events = JournalMonitor.ParseJournalEntry( line );
            missionMonitor._postHandleMissionCompletedEvent( (MissionCompletedEvent)events[ 0 ] );
            Assert.HasCount( 3, missionMonitor.missions );

            //MissionAbandonedEvent
            line = @"{ ""timestamp"":""2018-08-28T00:50:48Z"", ""event"":""MissionAbandoned"", ""Name"":""Mission_Courier_Elections_name"", ""Fine"":50000, ""MissionID"":413563499 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor.handleMissionAbandonedEvent((MissionAbandonedEvent)events[0]);
            mission = missionMonitor.missions.SingleOrDefault( m => m.missionid == 413563499 );
            Assert.IsNotNull( mission );
            Assert.AreEqual("Failed", mission.statusEDName);
            missionMonitor._postHandleMissionAbandonedEvent((MissionAbandonedEvent)events[0]);
            Assert.HasCount( 2, missionMonitor.missions );

            //MissionAcceptedEvent - 'AltruismCredits'
            line = "{ \"timestamp\":\"2018-09-17T02:54:16Z\", \"event\":\"MissionAccepted\", \"Faction\":\"Merope Expeditionary Fleet\", \"Name\":\"Mission_AltruismCredits\", \"LocalisedName\":\"Donate 450,000 Cr to the cause\", \"Donation\":\"450000\", \"Expiry\":\"2018-09-17T05:01:28Z\", \"Wing\":false, \"Influence\":\"Med\", \"Reputation\":\"Med\", \"MissionID\":419646649 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 419646649);
            Assert.IsNotNull( mission );
            Assert.HasCount( 3, missionMonitor.missions );
            Assert.IsTrue(mission.originreturn);

            //MissionAcceptedEvent - 'Collect'
            line = @"{ ""timestamp"":""2018-09-18T00:50:48Z"", ""event"":""MissionAccepted"", ""Faction"":""Calennero State Industries"", ""Name"":""Mission_Collect_Industrial"", ""LocalisedName"":""Industry needs 54 units of Tantalum"", ""Commodity"":""$Tantalum_Name;"", ""Commodity_Localised"":""Tantalum"", ""Count"":54, ""DestinationSystem"":""HIP 20277"", ""DestinationStation"":""Fabian City"", ""Expiry"":""2018-08-27T00:48:38Z"", ""Wing"":false, ""Influence"":""Med"", ""Reputation"":""Med"", ""Reward"":1909532, ""MissionID"":413748324 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748324);
            Assert.HasCount( 4, missionMonitor.missions );
            Assert.IsNotNull(mission);
            Assert.Contains( "Collect", mission.edTags );
            Assert.AreEqual("Active", mission.statusEDName);
            Assert.IsTrue(mission.originreturn);
            Assert.IsTrue(mission.legal);
            Assert.IsFalse(mission.wing);

            // Verify duplication protection
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            Assert.ContainsSingle( m => m.missionid == 413748324, missionMonitor.missions );
            Assert.HasCount( 4, missionMonitor.missions );

            //CargoDepotEvent - 'Collect'
            line = @"{ ""timestamp"":""2018-09-19T02:55:10Z"", ""event"":""CargoDepot"", ""MissionID"":413748324, ""UpdateType"":""Deliver"", ""CargoType"":""Tantalum"", ""Count"":54, ""StartMarketID"":0, ""EndMarketID"":3224777216, ""ItemsCollected"":0, ""ItemsDelivered"":54, ""TotalItemsToDeliver"":54, ""Progress"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleCargoDepotEvent((CargoDepotEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748324);
            Assert.IsNotNull( mission );
            Assert.AreEqual("Claim", mission.statusEDName);

            //MissionAcceptedEvent - 'Smuggle'
            line = @"{ ""timestamp"":""2018-09-21T20:51:56Z"", ""event"":""MissionAccepted"", ""Faction"":""Gcirithang Crimson Mafia"", ""Name"":""Mission_Smuggle_Famine"", ""LocalisedName"":""Smuggle 36 units of Narcotics to combat famine"", ""Commodity"":""$BasicNarcotics_Name;"", ""Commodity_Localised"":""Narcotics"", ""Count"":36, ""DestinationSystem"":""Carcinus"", ""DestinationStation"":""Wye-Delta Station"", ""Expiry"":""2018-08-30T20:55:33Z"", ""Wing"":false, ""Influence"":""Med"", ""Reputation"":""Med"", ""Reward"":180818, ""MissionID"":414732731 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 414732731);
            Assert.IsNotNull( mission );
            Assert.HasCount( 5, missionMonitor.missions );
            Assert.Contains( "Smuggle", mission.edTags );
            Assert.IsFalse(mission.originreturn);
            Assert.IsFalse(mission.legal);

            //MissionCompletedEvent
            line = @"{ ""timestamp"":""2018-09-22T00:40:14Z"", ""event"":""MissionCompleted"", ""Faction"":""HIP 20277 Inc"", ""Name"":""Mission_Salvage_Planet_name"", ""MissionID"":413563829, ""Commodity"":""$Landmines_Name;"", ""Commodity_Localised"":""Landmines"", ""Count"":4, ""DestinationSystem"":""Carthage"", ""Reward"":465824, ""FactionEffects"":[ { ""Faction"":""HIP 20277 Inc"", ""Effects"":[ { ""Effect"":""$MISSIONUTIL_Interaction_Summary_civilUnrest_down;"", ""Effect_Localised"":""$#MinorFaction; are happy to report improved civil contentment, making a period of civil unrest unlikely."", ""Trend"":""DownGood"" } ], ""Influence"":[ { ""SystemAddress"":84053791442, ""Trend"":""UpGood"" } ], ""Reputation"":""UpGood"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor.handleMissionCompletedEvent((MissionCompletedEvent)events[0]);
            mission = missionMonitor.missions.SingleOrDefault( m => m.missionid == 413563829 );
            Assert.IsNotNull( mission );
            Assert.AreEqual("Complete", mission.statusEDName);
            missionMonitor._postHandleMissionCompletedEvent((MissionCompletedEvent)events[0]);
            Assert.HasCount( 4, missionMonitor.missions );

            //MissionFailedEvent
            line = @"{ ""timestamp"":""2018-09-23T00:50:48Z"", ""event"":""MissionFailed"", ""Name"":""Mission_Collect_Industrial"", ""Fine"":5000, ""MissionID"":413748324 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );

            var crimeData = ConfigService.Instance.crimeMonitorConfiguration;
            var crimeMonitor = new CrimeMonitor();
            mission = missionMonitor.missions.ToList().FirstOrDefault(m => m.missionid == 413748324);
            var fine = ((MissionFailedEvent)events[0]).fine;
            await crimeMonitor._handleMissionFineAsync(events[0].timestamp, mission, fine).ConfigureAwait(false);
            var record = crimeMonitor.criminalrecord.ToList().FirstOrDefault(r => r.faction == mission?.faction);
            Assert.IsNotNull(record);
            Assert.AreEqual(5000, record.fines);
            var report = record.factionReports.FirstOrDefault(r => r.crimeDef == Crime.MissionFine);
            Assert.IsNotNull(report);
            Assert.AreEqual(5000, report.amount);
            ConfigService.Instance.crimeMonitorConfiguration = crimeData;

            missionMonitor.handleMissionFailedEvent((MissionFailedEvent)events[0]);
            mission = missionMonitor.missions.SingleOrDefault( m => m.missionid == 413748324 );
            Assert.IsNotNull( mission );
            Assert.AreEqual("Failed", mission.statusEDName); 
            missionMonitor._postHandleMissionFailedEvent((MissionFailedEvent)events[0]);
            Assert.HasCount( 4, missionMonitor.missions );
            var missionVariables = missionMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 3, missionVariables[ "missionsCount" ].Item2 );

            //MissionCompletedEvent - Donation
            line = @"{ ""timestamp"":""2018-12-18T19:14:32Z"", ""event"":""MissionCompleted"", ""Faction"":""Movement for Rabakshany Democrats"", ""Name"":""Mission_AltruismCredits_name"", ""MissionID"":442085549, ""Donation"":""1000000"", ""Donated"":1000000, ""FactionEffects"":[ { ""Faction"":""Movement for Rabakshany Democrats"", ""Effects"":[ { ""Effect"":""$MISSIONUTIL_Interaction_Summary_EP_up;"", ""Effect_Localised"":""The economic status of $#MinorFaction; has improved in the $#System; system."", ""Trend"":""UpGood"" } ], ""Influence"":[ { ""SystemAddress"":8605201797850, ""Trend"":""UpGood"", ""Influence"":""+++++"" } ], ""ReputationTrend"":""UpGood"", ""Reputation"":""++"" } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            var mcEvent = (MissionCompletedEvent)events[0];
            Assert.AreEqual(1000000, mcEvent.donation);

            // Restore original data
            ConfigService.Instance.missionMonitorConfiguration = missionData;
        }

        [TestMethod]
        public async Task MissionTimerEmitsExpirationOnceWithoutChangingStatus()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var eliteRunning = true;
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => eliteRunning, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 1 } );
            var mission = new Mission( 1, "Mission_Courier", now.AddSeconds( 5 ), MissionStatus.Active );

            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var snapshot = new MissionsEvent( now, [ mission ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );

            monitor.ProcessMissionTimers();
            Assert.AreEqual( 1, emittedEvents.OfType<MissionWarningEvent>().Count() );

            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();
            monitor.ProcessMissionTimers();

            Assert.AreEqual( 1, emittedEvents.OfType<MissionExpiredEvent>().Count() );
            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
        }

        [TestMethod]
        public async Task MissionTimerDeadlineExtensionRearmsExpiration()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => true, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var snapshot = new MissionsEvent( now,
                [ new Mission( 2, "Mission_Courier", now.AddSeconds( 5 ), MissionStatus.Active ) ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );

            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();
            var extendedDeadline = now.AddSeconds( 10 );
            snapshot = new MissionsEvent( now,
                [ new Mission( 2, "Mission_Courier", extendedDeadline, MissionStatus.Active ) ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );
            now = extendedDeadline;
            monitor.ProcessMissionTimers();

            Assert.AreEqual( 2, emittedEvents.OfType<MissionExpiredEvent>().Count() );
            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
        }

        [TestMethod]
        public async Task MissionTimerDoesNotArmAlreadyOverdueOrOfflineMissions()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var eliteRunning = true;
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => eliteRunning, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var snapshot = new MissionsEvent( now,
                [ new Mission( 3, "Mission_Courier", now.AddSeconds( -1 ), MissionStatus.Active ) ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );
            monitor.ProcessMissionTimers();

            eliteRunning = false;
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.offline.log", "test", "test" ) );
            snapshot = new MissionsEvent( now,
                [ new Mission( 4, "Mission_Courier", now.AddSeconds( 5 ), MissionStatus.Active ) ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );
            eliteRunning = true;
            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();

            Assert.AreEqual( 0, emittedEvents.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        public async Task MissionTimerDisarmsOnProcessExitAndShutdown()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var eliteRunning = true;
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => eliteRunning, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 1 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var processExitMission = new Mission( 8, "Mission_Courier", now.AddSeconds( 30 ), MissionStatus.Active );
            var snapshot = new MissionsEvent( now,
                [ processExitMission ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( 1, emittedEvents.OfType<MissionWarningEvent>().Count() );

            eliteRunning = false;
            monitor.ProcessMissionTimers();
            eliteRunning = true;
            now = now.AddSeconds( 30 );
            monitor.ProcessMissionTimers();

            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.next.log", "test", "test" ) );
            var shutdownMission = new Mission( 9, "Mission_Courier", now.AddSeconds( 30 ), MissionStatus.Active );
            snapshot = new MissionsEvent( now,
                [ shutdownMission ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( 2, emittedEvents.OfType<MissionWarningEvent>().Count() );
            await monitor.PreHandleAsync( new ShutdownEvent( now ) );
            now = now.AddSeconds( 30 );
            monitor.ProcessMissionTimers();

            Assert.AreEqual( 0, emittedEvents.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        public async Task MissionWarningsRequireCurrentSessionMissionData()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var emittedEvents = new List<Event>();
            var liveMonitor = new MissionMonitor( () => now, () => true, emittedEvents.Add );
            liveMonitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 1 } );
            await liveMonitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var mission = new Mission( 10, "Mission_Courier", now.AddSeconds( 30 ), MissionStatus.Active );
            var snapshot = new MissionsEvent( now, [ mission ] );
            liveMonitor._handleMissionsEvent( snapshot );
            liveMonitor.ArmMissionTimersFromSnapshot( snapshot );

            liveMonitor.ProcessMissionTimers();
            liveMonitor.ProcessMissionTimers();

            var offlineMonitor = new MissionMonitor( () => now, () => false, emittedEvents.Add );
            offlineMonitor.initializeMissionMonitor( new MissionMonitorConfiguration
            {
                missionWarning = 1,
                missions = [ new Mission( 11, "Mission_Courier", now.AddSeconds( 30 ), MissionStatus.Active ) ]
            } );
            offlineMonitor.ProcessMissionTimers();

            Assert.AreEqual( 1, emittedEvents.OfType<MissionWarningEvent>().Count() );
        }

        [TestMethod]
        public async Task MissionTimersIgnoreFailedAndCompletedMissions()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var deadline = now.AddSeconds( 30 );
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => true, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 1 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var failedMission = new Mission( 12, "Mission_OnFoot_Collect", deadline, MissionStatus.Active );
            var completedMission = new Mission( 13, "Mission_OnFoot_Collect", deadline, MissionStatus.Active );
            var snapshot = new MissionsEvent( now, [ failedMission, completedMission ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );

            failedMission.statusDef = MissionStatus.Failed;
            completedMission.statusDef = MissionStatus.Complete;
            Assert.IsNotNull( failedMission.expiry );
            Assert.IsNotNull( completedMission.expiry );

            monitor.ProcessMissionTimers();
            now = deadline;
            monitor.ProcessMissionTimers();

            Assert.AreEqual( 0, emittedEvents.OfType<MissionWarningEvent>().Count() );
            Assert.AreEqual( 0, emittedEvents.OfType<MissionExpiredEvent>().Count() );
        }

        [TestMethod]
        public async Task MissionExpirationDoesNotOverrideAuthoritativeTerminalEventsOrAdvanceWatermark()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => true, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var snapshot = new MissionsEvent( now,
                [
                    new Mission( 5, "Mission_Courier", now.AddSeconds( 5 ), MissionStatus.Active ),
                    new Mission( 6, "Mission_Courier", now.AddSeconds( 5 ), MissionStatus.Active )
                ] );
            monitor._handleMissionsEvent( snapshot );
            monitor.ArmMissionTimersFromSnapshot( snapshot );

            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();
            await monitor.PreHandleAsync( new MissionExpiredEvent( now.AddSeconds( 5 ), 5, "Mission_Courier" ) );
            await monitor.PreHandleAsync( new MissionFailedEvent( now.AddSeconds( 1 ), 5, "Mission_Courier", 0 ) );
            await monitor.PreHandleAsync( new MissionCompletedEvent( now.AddSeconds( 2 ), 6, "Mission_Courier",
                null, null, null, null, false, 0 ) );

            Assert.AreEqual( MissionStatus.Failed, monitor.missions.Single( m => m.missionid == 5 ).statusDef );
            Assert.AreEqual( MissionStatus.Complete, monitor.missions.Single( m => m.missionid == 6 ).statusDef );
        }

        [TestMethod]
        public void MissionsParserPreservesZeroExpiryAsAnActiveSnapshotCondition()
        {
            const string line = "{ \"timestamp\":\"2026-09-20T12:00:00Z\", \"event\":\"Missions\", " +
                                "\"Active\":[{\"MissionID\":20,\"Name\":\"Mission_Courier\",\"Expires\":0}," +
                                "{\"MissionID\":21,\"Name\":\"Mission_Courier\"}]," +
                                "\"Failed\":[],\"Complete\":[] }";

            var missionsEvent = (MissionsEvent)JournalMonitor.ParseJournalEntry( line ).Single();
            var mission = missionsEvent.missions.Single( m => m.missionid == 20 );
            var missionWithoutExpiry = missionsEvent.missions.Single( m => m.missionid == 21 );

            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            Assert.IsNull( mission.expiry );
            Assert.Contains( 20UL, missionsEvent.zeroExpiryMissionIds );
            Assert.AreEqual( MissionStatus.Active, missionWithoutExpiry.statusDef );
            Assert.IsNull( missionWithoutExpiry.expiry );
            Assert.DoesNotContain( 21UL, missionsEvent.zeroExpiryMissionIds );
        }

        [TestMethod, DoNotParallelize]
        public async Task ZeroExpirySnapshotBecomesClaimAfterTenSecondsAndPreservesKnownDeadline()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var knownDeadline = now.AddMinutes( 30 );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
            [
                new Mission( 21, "Mission_Courier", knownDeadline, MissionStatus.Active ),
                new Mission( 22, "Mission_Courier", null, MissionStatus.Active )
            ] ) );

            now = now.AddSeconds( 1 );
            await monitor.PreHandleAsync( new MissionsEvent( now,
            [
                new Mission( 21, "Mission_Courier", null, MissionStatus.Active ),
                new Mission( 22, "Mission_Courier", null, MissionStatus.Active )
            ], [ 21, 22 ] ) );

            now = now.AddSeconds( 9 );
            monitor.ProcessMissionTimers();
            Assert.IsTrue( monitor.missions.All( m => m.statusDef == MissionStatus.Active ) );

            now = now.AddSeconds( 1 );
            monitor.ProcessMissionTimers();

            var knownDeadlineMission = monitor.missions.Single( m => m.missionid == 21 );
            var unknownDeadlineMission = monitor.missions.Single( m => m.missionid == 22 );
            Assert.AreEqual( MissionStatus.Claim, knownDeadlineMission.statusDef );
            Assert.AreEqual( knownDeadline, knownDeadlineMission.expiry );
            Assert.AreEqual( MissionStatus.Claim, unknownDeadlineMission.statusDef );
            Assert.IsNull( unknownDeadlineMission.expiry );
        }

        [TestMethod, DoNotParallelize]
        public async Task FailureDuringZeroExpiryReconciliationPreventsInferredClaim()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 23, "Mission_Courier", null, MissionStatus.Active ) ], [ 23 ] ) );

            now = now.AddSeconds( 5 );
            await monitor.PreHandleAsync( new MissionFailedEvent( now, 23, "Mission_Courier", 0 ) );
            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();

            Assert.AreEqual( MissionStatus.Failed, monitor.missions.Single().statusDef );
        }

        [TestMethod, DoNotParallelize]
        public async Task AuthoritativeTerminalEventsOverrideAnInferredClaim()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
            [
                new Mission( 24, "Mission_Courier", null, MissionStatus.Active ),
                new Mission( 25, "Mission_Courier", null, MissionStatus.Active )
            ], [ 24, 25 ] ) );

            now = now.AddSeconds( 10 );
            monitor.ProcessMissionTimers();
            Assert.IsTrue( monitor.missions.All( m => m.statusDef == MissionStatus.Claim ) );

            now = now.AddSeconds( 1 );
            await monitor.PreHandleAsync( new MissionFailedEvent( now, 24, "Mission_Courier", 0 ) );
            await monitor.PreHandleAsync( new MissionCompletedEvent( now, 25, "Mission_Courier",
                null, null, null, null, false, 0 ) );

            Assert.AreEqual( MissionStatus.Failed, monitor.missions.Single( m => m.missionid == 24 ).statusDef );
            Assert.AreEqual( MissionStatus.Complete, monitor.missions.Single( m => m.missionid == 25 ).statusDef );
        }

        [TestMethod, DoNotParallelize]
        public async Task DeadlineExpirationAndZeroExpirySnapshotUseSeparateTimelines()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var deadline = now.AddSeconds( 5 );
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => true, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 26, "Mission_Courier", deadline, MissionStatus.Active ) ] ) );

            now = deadline;
            monitor.ProcessMissionTimers();
            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
            Assert.AreEqual( 1, emittedEvents.OfType<MissionExpiredEvent>().Count() );

            now = now.AddMinutes( 5 );
            await monitor.PreHandleAsync( new MissionsEvent( deadline,
                [ new Mission( 26, "Mission_Courier", null, MissionStatus.Active ) ], [ 26 ] ) );
            now = now.AddSeconds( 9 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );

            now = now.AddSeconds( 1 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( MissionStatus.Claim, monitor.missions.Single().statusDef );
        }

        [TestMethod, DoNotParallelize]
        public async Task PositiveExpiryReplacementCancelsZeroExpiryReconciliation()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
            [
                new Mission( 27, "Mission_Courier", null, MissionStatus.Active ),
                new Mission( 33, "Mission_Courier", null, MissionStatus.Active )
            ], [ 27, 33 ] ) );

            now = now.AddSeconds( 5 );
            var replacementDeadline = now.AddMinutes( 20 );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 27, "Mission_Courier", replacementDeadline, MissionStatus.Active ) ] ) );
            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();

            var mission = monitor.missions.Single();
            Assert.AreEqual( MissionStatus.Active, mission.statusDef );
            Assert.AreEqual( replacementDeadline, mission.expiry );
        }

        [TestMethod, DoNotParallelize]
        public async Task PositiveExpiryReplacementCannotRaceDueZeroExpiryReconciliation()
        {
            for ( var i = 0; i < 20; i++ )
            {
                var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
                var monitor = new MissionMonitor( () => now, () => true, _ => { } );
                monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
                await monitor.PreHandleAsync( new FileHeaderEvent( now, $"Journal.{i}.log", "test", "test" ) );
                await monitor.PreHandleAsync( new MissionsEvent( now,
                    [ new Mission( 1000UL + (ulong)i, "Mission_Courier", null, MissionStatus.Active ) ],
                    [ 1000UL + (ulong)i ] ) );

                now = now.AddSeconds( 10 );
                var replacementDeadline = now.AddMinutes( 20 );
                var replacement = new MissionsEvent( now,
                    [ new Mission( 1000UL + (ulong)i, "Mission_Courier", replacementDeadline,
                        MissionStatus.Active ) ] );

                await Task.WhenAll(
                    Task.Run( monitor.ProcessMissionTimers ),
                    Task.Run( () => monitor.PreHandleAsync( replacement ) ) );

                var mission = monitor.missions.Single();
                Assert.AreEqual( MissionStatus.Active, mission.statusDef );
                Assert.AreEqual( replacementDeadline, mission.expiry );
            }
        }

        [TestMethod]
        public void RuntimeMissionValuesAreDetachedSnapshots()
        {
            var waypoint = new NavWaypoint( "Test system", 123, 1, 2, 3 ) { visited = false };
            var mission = new Mission( 2000, "Mission_Courier", DateTime.UtcNow.AddHours( 1 ),
                MissionStatus.Active )
            {
                communal = true,
                communalContribution = 42,
                destinationsystems = [ waypoint ]
            };
            var monitor = new MissionMonitor( () => DateTime.UtcNow, () => false, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ mission ] } );

            Assert.IsTrue( monitor.GetVariableValues().TryGetValue<List<Mission>>( "missions", out var snapshots ) );
            var snapshot = snapshots.Single();
            monitor.missions.Single().name = "Mission_Changed";
            monitor.missions.Single().destinationsystems.Single().visited = true;
            monitor.missions.Single().communalContribution = 99;

            Assert.AreEqual( "Mission_Courier", snapshot.name );
            Assert.IsFalse( snapshot.destinationsystems.Single().visited );
            Assert.IsTrue( snapshot.communal );
            Assert.AreEqual( 42, snapshot.communalContribution );
        }

        [TestMethod, DoNotParallelize]
        public void PersistedMissionValuesAreDetachedSnapshots()
        {
            var originalConfiguration = ConfigService.Instance.missionMonitorConfiguration;
            try
            {
                var mission = new Mission( 2001, "Mission_Courier", DateTime.UtcNow.AddHours( 1 ),
                    MissionStatus.Active )
                {
                    destinationsystems = [ new NavWaypoint( "Test system", 456, 4, 5, 6 ) ]
                };
                var monitor = new MissionMonitor( () => DateTime.UtcNow, () => false, _ => { } );
                monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ mission ] } );

                monitor.writeMissions();
                var persistedMission = ConfigService.Instance.missionMonitorConfiguration.missions.Single();
                monitor.missions.Single().name = "Mission_Changed";
                monitor.missions.Single().destinationsystems.Single().visited = true;

                Assert.AreEqual( "Mission_Courier", persistedMission.name );
                Assert.IsFalse( persistedMission.destinationsystems.Single().visited );
            }
            finally
            {
                ConfigService.Instance.missionMonitorConfiguration = originalConfiguration;
            }
        }

        [TestMethod, DoNotParallelize]
        public async Task ProcessExitAndShutdownCancelZeroExpiryReconciliation()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var eliteRunning = true;
            var monitor = new MissionMonitor( () => now, () => eliteRunning, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 34, "Mission_Courier", null, MissionStatus.Active ) ], [ 34 ] ) );

            now = now.AddSeconds( 5 );
            eliteRunning = false;
            monitor.ProcessMissionTimers();
            eliteRunning = true;
            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();
            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );

            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.next.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 34, "Mission_Courier", null, MissionStatus.Active ) ], [ 34 ] ) );
            await monitor.PreHandleAsync( new ShutdownEvent( now ) );
            now = now.AddSeconds( 10 );
            monitor.ProcessMissionTimers();

            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
        }

        [TestMethod, DoNotParallelize]
        public async Task OfflineZeroExpirySnapshotDoesNotStartReconciliation()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var eliteRunning = false;
            var monitor = new MissionMonitor( () => now, () => eliteRunning, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.replay.log", "test", "test" ) );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 28, "Mission_Courier", null, MissionStatus.Active ) ], [ 28 ] ) { fromLoad = true } );

            eliteRunning = true;
            now = now.AddSeconds( 20 );
            monitor.ProcessMissionTimers();

            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
        }

        [TestMethod, DoNotParallelize]
        public async Task StandardMissionSnapshotIsAuthoritativeWithoutInventingFailure()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration
            {
                missionWarning = 0,
                missions =
                [
                    new Mission( 29, "Mission_Courier", now.AddMinutes( 5 ), MissionStatus.Failed ),
                    new Mission( 30, "Mission_Courier", now.AddMinutes( 5 ), MissionStatus.Active ),
                    new Mission( 31, "Mission_Courier", now.AddMinutes( 5 ), MissionStatus.Active )
                ]
            } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var repairedDeadline = now.AddMinutes( 30 );
            await monitor.PreHandleAsync( new MissionsEvent( now,
            [
                new Mission( 29, "Mission_Courier", repairedDeadline, MissionStatus.Active ),
                new Mission( 31, "Mission_Courier", null, MissionStatus.Failed ),
                new Mission( 32, "Mission_Courier", null, MissionStatus.Complete )
            ] ) );

            Assert.HasCount( 3, monitor.missions );
            var repairedMission = monitor.missions.Single( m => m.missionid == 29 );
            Assert.AreEqual( 29UL, repairedMission.missionid );
            Assert.AreEqual( MissionStatus.Active, repairedMission.statusDef );
            Assert.AreEqual( repairedDeadline, repairedMission.expiry );
            Assert.AreEqual( MissionStatus.Failed,
                monitor.missions.Single( m => m.missionid == 31 ).statusDef );
            Assert.AreEqual( MissionStatus.Complete,
                monitor.missions.Single( m => m.missionid == 32 ).statusDef );
            var variables = monitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 1, variables[ "missionsCount" ].Item2 );

            now = now.AddSeconds( 1 );
            await monitor.PreHandleAsync( new MissionsEvent( now,
                [ new Mission( 29, "Mission_Courier", repairedDeadline, MissionStatus.Active ) ] ) );

            Assert.ContainsSingle( monitor.missions );
            Assert.AreEqual( 29UL, monitor.missions.Single().missionid );
        }

        [TestMethod, DoNotParallelize]
        public async Task FailedMissionRemainsTrackedUntilAbandonment()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration
            {
                missions = [ new Mission( 35, "Mission_Courier", now.AddMinutes( 5 ), MissionStatus.Active ) ]
            } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            var failedEvent = new MissionFailedEvent( now, 35, "Mission_Courier", 0 );

            await monitor.PreHandleAsync( failedEvent );
            await monitor.PostHandleAsync( failedEvent );

            Assert.AreEqual( MissionStatus.Failed, monitor.missions.Single().statusDef );
            var variables = monitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 0, variables[ "missionsCount" ].Item2 );

            now = now.AddSeconds( 1 );
            var abandonedEvent = new MissionAbandonedEvent( now, 35, "Mission_Courier", 0 );
            await monitor.PreHandleAsync( abandonedEvent );
            await monitor.PostHandleAsync( abandonedEvent );

            Assert.IsEmpty( monitor.missions );
        }

        [TestMethod, DoNotParallelize]
        public async Task Issue2856SalvageCargoAcrossFsdRemainsActiveAndCanComplete()
        {
            var now = new DateTime( 2026, 9, 27, 12, 0, 0, DateTimeKind.Utc );
            var fullCargoMission = new Mission( 1056406125, "Mission_Salvage_Expansion", now.AddHours( 1 ),
                MissionStatus.Active ) { amount = 4 };
            var partialCargoMission = new Mission( 967262440, "Mission_Salvage_Expansion", now.AddHours( 1 ),
                MissionStatus.Active ) { amount = 3 };
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration
            {
                missions = [ fullCargoMission, partialCargoMission ]
            } );
            var originalCargoConfiguration = ConfigService.Instance.cargoMonitorConfiguration;
            ConfigService.Instance.cargoMonitorConfiguration = new CargoMonitorConfiguration
            {
                cargo =
                [
                    new Cargo( "DamagedEscapePod" )
                    {
                        missionCargo = new Dictionary<ulong, int>
                        {
                            [ fullCargoMission.missionid ] = 4,
                            [ partialCargoMission.missionid ] = 2
                        }
                    }
                ]
            };

            try
            {
                await monitor.PreHandleAsync( new FSDEngagedEvent( now, "Hyperspace", "Col 285 Sector YF-M c8-8",
                    null, "G", false ) );

                Assert.IsTrue( monitor.missions.All( mission => mission.statusDef == MissionStatus.Active ) );

                await monitor.PreHandleAsync( new MissionCompletedEvent( now.AddMinutes( 5 ),
                    fullCargoMission.missionid, fullCargoMission.name, null, null, null, null, false, 0 ) );
                await monitor.PreHandleAsync( new MissionCompletedEvent( now.AddMinutes( 6 ),
                    partialCargoMission.missionid, partialCargoMission.name, null, null, null, null, false, 0 ) );

                Assert.IsTrue( monitor.missions.All( mission => mission.statusDef == MissionStatus.Complete ) );
            }
            finally
            {
                ConfigService.Instance.cargoMonitorConfiguration = originalCargoConfiguration;
            }
        }

        [TestMethod, DoNotParallelize]
        [DataRow( "Mission_Salvage_Expansion" )]
        [DataRow( "Mission_Rescue_Planet" )]
        [DataRow( "Mission_Smuggle_Famine" )]
        public async Task CargoLossAcrossFsdDoesNotInferMissionFailure( string missionName )
        {
            var now = new DateTime( 2026, 9, 27, 12, 0, 0, DateTimeKind.Utc );
            var mission = new Mission( 963626959, missionName, now.AddHours( 1 ), MissionStatus.Active )
            {
                amount = 2
            };
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ mission ] } );
            var originalCargoConfiguration = ConfigService.Instance.cargoMonitorConfiguration;
            ConfigService.Instance.cargoMonitorConfiguration = CreateMissionCargoConfiguration( mission.missionid, 1 );

            try
            {
                await monitor.PreHandleAsync( new FSDEngagedEvent( now, "Supercruise", null, null, null, false ) );

                Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
            }
            finally
            {
                ConfigService.Instance.cargoMonitorConfiguration = originalCargoConfiguration;
            }
        }

        [TestMethod, DoNotParallelize]
        public async Task DeliveryCargoLossAcrossFsdDoesNotInferMissionFailure()
        {
            var now = new DateTime( 2026, 9, 27, 12, 0, 0, DateTimeKind.Utc );
            var mission = new Mission( 970176050, "Mission_Delivery", now.AddHours( 1 ), MissionStatus.Active )
            {
                amount = 10,
                collected = 10,
                delivered = 0
            };
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ mission ] } );
            var originalCargoConfiguration = ConfigService.Instance.cargoMonitorConfiguration;
            ConfigService.Instance.cargoMonitorConfiguration = CreateMissionCargoConfiguration( mission.missionid, 4 );

            try
            {
                await monitor.PreHandleAsync( new FSDEngagedEvent( now, "Hyperspace", "Aulin", null, "K", false ) );

                Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
            }
            finally
            {
                ConfigService.Instance.cargoMonitorConfiguration = originalCargoConfiguration;
            }
        }

        [TestMethod]
        public async Task FsdEngagementDoesNotSuppressOlderAuthoritativeMissionEvents()
        {
            var now = new DateTime( 2026, 9, 27, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration
            {
                missions =
                [
                    new Mission( 41, "Mission_Courier", now.AddHours( 1 ), MissionStatus.Active ),
                    new Mission( 42, "Mission_Courier", now.AddHours( 1 ), MissionStatus.Active )
                ]
            } );

            await monitor.PreHandleAsync( new FSDEngagedEvent( now.AddSeconds( 10 ), "Hyperspace", "Aulin", null,
                "K", false ) );
            await monitor.PreHandleAsync( new MissionCompletedEvent( now.AddSeconds( 5 ), 41, "Mission_Courier",
                null, null, null, null, false, 0 ) );
            await monitor.PreHandleAsync( new MissionFailedEvent( now.AddSeconds( 6 ), 42, "Mission_Courier", 0 ) );

            Assert.AreEqual( MissionStatus.Complete,
                monitor.missions.Single( mission => mission.missionid == 41 ).statusDef );
            Assert.AreEqual( MissionStatus.Failed,
                monitor.missions.Single( mission => mission.missionid == 42 ).statusDef );
        }

        [TestMethod]
        public async Task MissionTimerIgnoresCommunityGoals()
        {
            var now = new DateTime( 2026, 9, 20, 12, 0, 0, DateTimeKind.Utc );
            var emittedEvents = new List<Event>();
            var monitor = new MissionMonitor( () => now, () => true, emittedEvents.Add );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missionWarning = 0 } );
            var communityGoal = new Mission( 7, "MISSION_CommunityGoal", now.AddSeconds( 5 ), MissionStatus.Active )
            {
                communal = true
            };
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration
            {
                missionWarning = 0,
                missions = [ communityGoal ]
            } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            monitor.ArmMissionTimer( communityGoal );

            now = now.AddSeconds( 5 );
            monitor.ProcessMissionTimers();

            Assert.AreEqual( 0, emittedEvents.OfType<MissionExpiredEvent>().Count() );
            Assert.AreEqual( MissionStatus.Active, monitor.missions.Single().statusDef );
        }

        [TestMethod]
        public void TestCommunityGoalRewardEvent()
        {
            var line = @"{ ""timestamp"":""2019-01-27T06:14:02Z"", ""event"":""CommunityGoalReward"", ""CGID"":568, ""Name"":""Distant Worlds Mining Initiative"", ""System"":""Omega Sector VE-Q b5-15"", ""Reward"":23000000 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            var mcEvent = (MissionCompletedEvent)events[0];
            Assert.IsInstanceOfType(mcEvent, typeof(MissionCompletedEvent));
        }

        [TestMethod, DoNotParallelize]
        public async Task LiveAndReplayedCurrentCommunityGoalSnapshotsApplyTheSameState()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var liveEvents = new List<Event>();
            var replayEvents = new List<Event>();
            var liveMonitor = new MissionMonitor( () => now, () => true, liveEvents.Add );
            var replayMonitor = new MissionMonitor( () => now, () => true, replayEvents.Add );
            liveMonitor.initializeMissionMonitor( new MissionMonitorConfiguration() );
            replayMonitor.initializeMissionMonitor( new MissionMonitorConfiguration
                { updatedat = now.AddMinutes( 5 ) } );
            await liveMonitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.live.log", "test", "test" ) );
            await replayMonitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.replay.log", "test", "test" ) );
            var goal = CreateCommunityGoal( 700, now.AddHours( 1 ), false, 250, 3, 25, 500000 );
            var joinedGoal = new Mission( 700, "MISSION_CommunityGoal", null, MissionStatus.Active )
                { communal = true };

            await liveMonitor.PreHandleAsync( new MissionAcceptedEvent( now, joinedGoal ) );
            Assert.ContainsSingle( liveMonitor.missions );
            Assert.IsTrue( liveMonitor.missions.Single().communal );

            // A join is provisional incremental state, so an event older than the persisted watermark is ignored.
            await replayMonitor.PreHandleAsync( new MissionAcceptedEvent( now, joinedGoal ) { fromLoad = true } );
            Assert.IsEmpty( replayMonitor.missions );

            await liveMonitor.PreHandleAsync( new CommunityGoalsEvent( now, [ goal ] ) );
            await replayMonitor.PreHandleAsync( new CommunityGoalsEvent( now, [ goal ] ) { fromLoad = true } );

            var liveMission = liveMonitor.missions.Single();
            var replayMission = replayMonitor.missions.Single();
            Assert.AreEqual( liveMission.statusDef, replayMission.statusDef );
            Assert.AreEqual( liveMission.expiry, replayMission.expiry );
            Assert.AreEqual( liveMission.localisedname, replayMission.localisedname );
            Assert.AreEqual( liveMission.communalContribution, replayMission.communalContribution );
            Assert.AreEqual( liveMission.communalTier, replayMission.communalTier );
            Assert.AreEqual( liveMission.communalPercentileBand, replayMission.communalPercentileBand );
            Assert.IsTrue( liveEvents.OfType<CommunityGoalEvent>().Any() );
            Assert.IsFalse( replayEvents.OfType<CommunityGoalEvent>().Any() );
        }

        [TestMethod, DoNotParallelize]
        public async Task CommunityGoalSnapshotOwnsCompletionAndMembership()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration() );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new CommunityGoalsEvent( now,
            [
                CreateCommunityGoal( 701, now.AddHours( 1 ), false, 100 ),
                CreateCommunityGoal( 702, now.AddHours( 1 ), false, 0 ),
                CreateCommunityGoal( 703, now.AddHours( 1 ), false, 50 )
            ] ) { fromLoad = true } );

            now = now.AddMinutes( 1 );
            await monitor.PreHandleAsync( new CommunityGoalsEvent( now,
            [
                CreateCommunityGoal( 701, now.AddHours( -1 ), true, 125, 4, 10, 1000000 ),
                CreateCommunityGoal( 702, now.AddHours( -1 ), true, 0 )
            ] ) { fromLoad = true } );

            Assert.ContainsSingle( monitor.missions );
            var claimableGoal = monitor.missions.Single();
            Assert.AreEqual( 701UL, claimableGoal.missionid );
            Assert.AreEqual( MissionStatus.Claim, claimableGoal.statusDef );
            Assert.AreEqual( 125, claimableGoal.communalContribution );
            Assert.AreEqual( 1000000, claimableGoal.reward );
            Assert.AreEqual( now.AddHours( -1 ), claimableGoal.expiry );
        }

        [TestMethod, DoNotParallelize]
        public async Task CommunityGoalRewardAndDiscardRemoveTrackedGoals()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var monitor = new MissionMonitor( () => now, () => true, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration() );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.test.log", "test", "test" ) );
            await monitor.PreHandleAsync( new CommunityGoalsEvent( now,
            [
                CreateCommunityGoal( 704, now.AddHours( 1 ), true, 100 ),
                CreateCommunityGoal( 705, now.AddHours( 1 ), false, 0 )
            ] ) { fromLoad = true } );

            now = now.AddSeconds( 1 );
            var rewardEvent = new MissionCompletedEvent( now, 704, "MISSION_CommunityGoal", "Goal 704",
                null, null, null, true, 100000 ) { fromLoad = true };
            await monitor.PreHandleAsync( rewardEvent );
            await monitor.PostHandleAsync( rewardEvent );

            now = now.AddSeconds( 1 );
            // Community goal identity comes from the tracked mission rather than the event name.
            var discardEvent = new MissionAbandonedEvent( now, 705, "UnexpectedMissionName", 0 ) { fromLoad = true };
            await monitor.PreHandleAsync( discardEvent );
            await monitor.PostHandleAsync( discardEvent );

            Assert.IsEmpty( monitor.missions );
        }

        [TestMethod, DoNotParallelize]
        public async Task OfflineReplayPreservesPersistedCommunityGoals()
        {
            var now = new DateTime( 2026, 9, 21, 12, 0, 0, DateTimeKind.Utc );
            var persistedGoals = new[]
            {
                new Mission( 706, "MISSION_CommunityGoal", now.AddHours( -1 ), MissionStatus.Active )
                    { communal = true, localisedname = "Goal 706" },
                new Mission( 707, "MISSION_CommunityGoal", now.AddHours( -1 ), MissionStatus.Claim )
                    { communal = true, localisedname = "Goal 707" },
                new Mission( 708, "MISSION_CommunityGoal", now.AddHours( -1 ), MissionStatus.Active )
                    { communal = true, localisedname = "Goal 708" }
            };
            var monitor = new MissionMonitor( () => now, () => false, _ => { } );
            monitor.initializeMissionMonitor( new MissionMonitorConfiguration { missions = [ .. persistedGoals ] } );
            await monitor.PreHandleAsync( new FileHeaderEvent( now, "Journal.offline.log", "test", "test" ) );

            var joinedGoal = new Mission( 709, "MISSION_CommunityGoal", null, MissionStatus.Active )
                { communal = true };
            await monitor.PreHandleAsync( new MissionAcceptedEvent( now, joinedGoal ) { fromLoad = true } );
            await monitor.PreHandleAsync( new CommunityGoalsEvent( now, [ ] ) { fromLoad = true } );
            var rewardEvent = new MissionCompletedEvent( now.AddSeconds( 1 ), 707, "MISSION_CommunityGoal",
                "Goal 707", null, null, null, true, 100000 ) { fromLoad = true };
            await monitor.PreHandleAsync( rewardEvent );
            await monitor.PostHandleAsync( rewardEvent );
            var discardEvent = new MissionAbandonedEvent( now.AddSeconds( 2 ), 708, "MISSION_CommunityGoal", 0 )
                { fromLoad = true };
            await monitor.PreHandleAsync( discardEvent );
            await monitor.PostHandleAsync( discardEvent );

            Assert.HasCount( 3, monitor.missions );
            Assert.IsTrue( monitor.missions.All( mission => mission.communal ) );
            Assert.AreEqual( MissionStatus.Active,
                monitor.missions.Single( mission => mission.missionid == 706 ).statusDef );
            Assert.AreEqual( MissionStatus.Claim,
                monitor.missions.Single( mission => mission.missionid == 707 ).statusDef );
        }

        [TestMethod]
        public void TestCommunityGoalScenario()
        {
            // Save original data
            var missionData = ConfigService.Instance.missionMonitorConfiguration;

            missionMonitor.initializeMissionMonitor(new MissionMonitorConfiguration());

            // MissionsEvent
            var line = @"{ ""timestamp"":""2020-10-24T01:39:13Z"", ""event"":""Missions"", ""Active"":[  ], ""Failed"":[  ], ""Complete"":[  ] }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionsEvent((MissionsEvent)events[0]);
            Assert.HasCount( 0, missionMonitor.missions );

            // CommunityGoalJoin
            line = @"{ ""timestamp"":""2020-10-24T05:05:25Z"", ""event"":""CommunityGoalJoin"", ""CGID"":619, ""Name"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""System"":""LFT 625"" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionAcceptedEvent((MissionAcceptedEvent)events[0]);
            Assert.HasCount( 1, missionMonitor.missions );
            Assert.AreEqual(619U, missionMonitor.missions[0].missionid);
            Assert.AreEqual("Federal Initiative to Deliver Supplies for Marlinist Refugees", missionMonitor.missions[0].localisedname);
            Assert.AreEqual("LFT 625", missionMonitor.missions[0].originsystem);

            // CommunityGoal (active)
            line = @"{ ""timestamp"":""2020-10-25T05:05:51Z"", ""event"":""CommunityGoal"", ""CurrentGoals"":[ { ""CGID"":619, ""Title"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:27Z"", ""IsComplete"":false, ""CurrentTotal"":14801430, ""PlayerContribution"":5516, ""NumContributors"":4560, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":""All systems supported"" }, ""TopRankSize"":10, ""PlayerInTopRank"":false, ""TierReached"":""Tier 1"", ""PlayerPercentileBand"":25, ""Bonus"":600000 }, { ""CGID"":620, ""Title"":""Federal Initiative to Protect Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:17Z"", ""IsComplete"":false, ""CurrentTotal"":6029526816, ""PlayerContribution"":281617, ""NumContributors"":2879, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":"""" }, ""TopRankSize"":10, ""PlayerInTopRank"":false, ""TierReached"":""Tier 2"", ""PlayerPercentileBand"":100, ""Bonus"":150000 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleCommunityGoalsEvent((CommunityGoalsEvent)events[0]);
            Assert.HasCount( 2, missionMonitor.missions );
            var deliveryGoal = missionMonitor.missions.Single( mission => mission.missionid == 619 );
            Assert.AreEqual("Fox Enterprise", deliveryGoal.originstation);
            Assert.AreEqual("Active", deliveryGoal.statusEDName);
            Assert.AreEqual(600000, deliveryGoal.reward);

            // CommunityGoal (completed)
            line = @"{ ""timestamp"":""2020-10-30T01:22:34Z"", ""event"":""CommunityGoal"", ""CurrentGoals"":[ { ""CGID"":619, ""Title"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:27Z"", ""IsComplete"":true, ""CurrentTotal"":33417668, ""PlayerContribution"":6304, ""NumContributors"":0, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":""All systems supported"" }, ""TopRankSize"":10, ""PlayerInTopRank"":true, ""TierReached"":""Tier 3"", ""PlayerPercentileBand"":0, ""Bonus"":10000000 }, { ""CGID"":620, ""Title"":""Federal Initiative to Protect Supplies for Marlinist Refugees"", ""SystemName"":""LFT 625"", ""MarketName"":""Fox Enterprise"", ""Expiry"":""2020-10-29T06:00:17Z"", ""IsComplete"":true, ""CurrentTotal"":18046921733, ""PlayerContribution"":4899320, ""NumContributors"":0, ""TopTier"":{ ""Name"":""Tier 6"", ""Bonus"":"""" }, ""TopRankSize"":10, ""PlayerInTopRank"":true, ""TierReached"":""Tier 3"", ""PlayerPercentileBand"":0, ""Bonus"":10000000 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleCommunityGoalsEvent((CommunityGoalsEvent)events[0]);
            deliveryGoal = missionMonitor.missions.Single( mission => mission.missionid == 619 );
            Assert.AreEqual("Claim", deliveryGoal.statusEDName);
            Assert.AreEqual(10000000, deliveryGoal.reward);

            // CommunityGoalReward
            line = @"{ ""timestamp"":""2020-10-30T01:24:27Z"", ""event"":""CommunityGoalReward"", ""CGID"":619, ""Name"":""Federal Initiative to Deliver Supplies for Marlinist Refugees"", ""System"":""LFT 625"", ""Reward"":10000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._postHandleMissionCompletedEvent((MissionCompletedEvent)events[0]);
            Assert.HasCount( 1, missionMonitor.missions );  // One of our two community goals is still active. The other (completed) has been removed.

            // MissionsEvent
            line = @"{ ""timestamp"":""2020-10-30T01:30:27Z"", ""event"":""Missions"", ""Active"":[  ], ""Failed"":[  ], ""Complete"":[  ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            missionMonitor._handleMissionsEvent((MissionsEvent)events[0]);
            Assert.HasCount( 1, missionMonitor.missions ); // Standard mission snapshots do not own community goal membership.

            // Restore original data
            ConfigService.Instance.missionMonitorConfiguration = missionData;
        }

        private static CommunityGoal CreateCommunityGoal ( ulong id, DateTime expiry, bool isComplete,
            long contribution, int tier = 1, int percentile = 50, long reward = 100000 ) => new()
        {
            cgid = id,
            name = $"Goal {id}",
            system = "LFT 625",
            station = "Fox Enterprise",
            expiryDateTime = expiry,
            iscomplete = isComplete,
            contribution = contribution,
            Tier = $"Tier {tier}",
            percentileband = percentile,
            tierreward = reward
        };

        private static CargoMonitorConfiguration CreateMissionCargoConfiguration ( ulong missionId, int amount ) =>
            new()
            {
                cargo =
                [
                    new Cargo( "DamagedEscapePod" )
                    {
                        missionCargo = new Dictionary<ulong, int> { [ missionId ] = amount }
                    }
                ]
            };

        [TestMethod]
        public void TestMissionAcceptedToSettlement()
        {
            var line = @"{ ""timestamp"":""2023-01-03T22:14:24Z"", ""event"":""MissionAccepted"", ""Faction"":""Gabjaujavas Dominion"", ""Name"":""Mission_OnFoot_Collect_Contact_MB"", ""LocalisedName"":""Secure a package from Nariah Nieves"", ""Commodity"":""$PersonalDocuments_Name;"", ""Commodity_Localised"":""Personal Documents"", ""Count"":1, ""DestinationSystem"":""Iah Bulu"", ""DestinationSettlement"":""Clothier's Respite"", ""Target"":""Nariah Nieves"", ""Expiry"":""2023-01-04T04:17:30Z"", ""Wing"":false, ""Influence"":""+"", ""Reputation"":""+"", ""Reward"":54212, ""MissionID"":909166596 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            var @event = (MissionAcceptedEvent)events[0];
            Assert.IsInstanceOfType(@event, typeof(MissionAcceptedEvent));
            Assert.AreEqual("Clothier's Respite", @event.destinationstation);
        }
    }
}
