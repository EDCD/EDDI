using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class CrimeMonitorTests : TestBase
    {
        readonly CrimeMonitor crimeMonitor = new CrimeMonitor();
        FactionRecord record;
        FactionReport report;
        string line;
        List<Event> events;

        readonly string crimeConfigJson = @"{
            ""criminalrecord"": [{
                ""faction"": ""Calennero State Industries"",
                ""allegiance"": ""Empire"",
                ""system"": ""Calennero"",
                ""station"": ""Macdonald Hub"",
                ""claims"": 105168,
                ""fines"": 400,
                ""bounties"": 0,
                ""factionSystems"": [
                    ""Kamadhenu"",
                    ""Manian"",
                    ""Calennero"",
                    ""Bajauie"",
                    ""Arapa"",
                    ""Bragur"",
                    ""Nemgla"",
                    ""Carthage"",
                    ""HIP 20277""
                ],
                ""factionReports"": [{
                    ""timestamp"": ""2019-04-22T03:07:00Z"",
                    ""bounty"": true,
                    ""shipId"": 10,
                    ""crimeEDName"": ""none"",
                    ""system"": ""HIP 20277"",
                    ""station"": null,
                    ""body"": ""HIP 20277 7 B Ring"",
                    ""victim"": ""Belata Mafia"",
                    ""amount"": 105168
                },
                {
                    ""timestamp"": ""2019-04-22T03:21:46Z"",
                    ""bounty"": false,
                    ""shipId"": 10,
                    ""crimeEDName"": ""dockingMinorTresspass"",
                    ""system"": ""HIP 20277"",
                    ""station"": ""Fabian City"",
                    ""body"": null,
                    ""victim"": null,
                    ""amount"": 400
                }]
            },
            {
                ""faction"": ""HIP 20277 Inc"",
                ""allegiance"": ""Independent"",
                ""system"": ""HIP 20277"",
                ""station"": ""Fabian City"",
                ""claims"": 108728,
                ""fines"": 0,
                ""bounties"": 0,
                ""factionSystems"": [
                    ""HIP 20277""
                ],
                ""factionReports"": [{
                    ""timestamp"": ""2019-04-22T03:05:31Z"",
                    ""bounty"": true,
                    ""shipId"": 10,
                    ""crimeEDName"": ""none"",
                    ""system"": ""HIP 20277"",
                    ""station"": null,
                    ""body"": ""HIP 20277 7 B Ring"",
                    ""victim"": ""Belata Mafia"",
                    ""amount"": 58428
                },
                {
                    ""timestamp"": ""2019-04-22T03:08:53Z"",
                    ""bounty"": true,
                    ""shipId"": 10,
                    ""crimeEDName"": ""none"",
                    ""system"": ""HIP 20277"",
                    ""station"": null,
                    ""body"": ""HIP 20277 7 B Ring"",
                    ""victim"": ""Belata Mafia"",
                    ""amount"": 50300
                }]
            },
            {
                ""faction"": ""Constitution Party of Aerial"",
                ""allegiance"": ""Empire"",
                ""system"": ""Aerial"",
                ""station"": ""Flagg Holdings"",
                ""claims"": 62019,
                ""fines"": 0,
                ""bounties"": 0,
                ""factionSystems"": [
                    ""Yarrite"",
                    ""Aerial"",
                    ""Gaula Wu""
                ],
                ""factionReports"": [{
                    ""timestamp"": ""2019-04-22T11:49:44Z"",
                    ""bounty"": false,
                    ""shipId"": 10,
                    ""crimeEDName"": ""none"",
                    ""system"": ""Aerial"",
                    ""station"": null,
                    ""body"": ""Aerial 2"",
                    ""victim"": ""Ankou Blue Federal Holdings"",
                    ""amount"": 33335
                },
                {
                    ""timestamp"": ""2019-04-22T11:51:30Z"",
                    ""bounty"": false,
                    ""shipId"": 10,
                    ""crimeEDName"": ""none"",
                    ""system"": ""Aerial"",
                    ""station"": null,
                    ""body"": ""Aerial 2"",
                    ""victim"": ""Ankou Blue Federal Holdings"",
                    ""amount"": 28684
                }]
            }],
            ""homeSystems"": {
                ""Lavigny's Legion"": ""Carthage"",
                ""Mother Gaia"": ""Sol""
            },
            ""claims"": 275915,
            ""fines"": 400,
            ""bounties"": 0,
            ""profitShare"": 14,
            ""updatedat"": ""2019-04-22T11:51:30Z""
        }";

        [TestInitialize]
        public void StartTestCrimeMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestCrimeConfig()
        {
            // Save original data
            var data = ConfigService.Instance.crimeMonitorConfiguration;

            var config = ConfigService.FromJson<CrimeMonitorConfiguration>(crimeConfigJson);
            Assert.AreEqual(3, config.criminalrecord.Count);
            Assert.AreEqual(275915, config.criminalrecord.Sum(r => r.claims));
            Assert.AreEqual(400, config.criminalrecord.Sum(r => r.fines));

            record = config.criminalrecord.ToList().FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(Superpower.Empire, record.Allegiance);
            Assert.AreEqual("Empire", record.allegiance);
            Assert.AreEqual(105168, record.bountiesAmount);
            Assert.AreEqual(400, record.finesIncurred.Sum(r => r.amount));

            // Verify faction report object 
            Assert.AreEqual(2, record.factionReports.Count);
            report = record.factionReports[0];
            Assert.IsTrue(report.bounty);
            Assert.AreEqual(Crime.None, report.crimeDef);
            Assert.AreEqual("HIP 20277", report.system);
            Assert.AreEqual(105168, report.amount);
            report = record.factionReports[1];
            Assert.IsFalse(report.bounty);
            Assert.AreEqual(Crime.TrespassMinor, report.crimeDef);
            Assert.AreEqual("Fabian City", report.station);

            // Restore original data
            ConfigService.Instance.crimeMonitorConfiguration = data;
        }

        [TestMethod]
        public void TestCrimeEventsScenario()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeBgsRestClient.Expect( "v5/factions?name=Ankou Blue Federal Holdings&page=1", @"{""docs"":[{""_id"":""59e7b73bd22c775be0fe66d7"",""__v"":0,""allegiance"":""empire"",""eddb_id"":12284,""faction_presence"":[{""system_name"":""Aerial"",""system_name_lower"":""aerial"",""system_id"":""59e7b78cd22c775be0fe6a3e"",""state"":""election"",""influence"":0.139653,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""election""}],""pending_states"":[],""recovering_states"":[{""state"":""infrastructurefailure"",""trend"":0}],""conflicts"":[{""type"":""election"",""status"":""active"",""opponent_name"":""Murus Major Industry"",""opponent_name_lower"":""murus major industry"",""opponent_faction_id"":""59e7b78cd22c775be0fe6a3a"",""station_id"":null,""stake"":"""",""stake_lower"":"""",""days_won"":0}],""updated_at"":""2024-12-28T10:26:53.000Z""},{""system_name"":""Chamuluma"",""system_name_lower"":""chamuluma"",""system_id"":""59e7e372d22c775be0ffc841"",""state"":""boom"",""influence"":0.732268,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""boom""}],""pending_states"":[{""state"":""outbreak"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T16:11:14.000Z""},{""system_name"":""HR 1475"",""system_name_lower"":""hr 1475"",""system_id"":""59e7f2e4d22c775be00013a2"",""state"":""boom"",""influence"":0.055,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T22:06:07.000Z""},{""system_name"":""Carthage"",""system_name_lower"":""carthage"",""system_id"":""59e83bf3d22c775be001085f"",""state"":""none"",""influence"":0.106893,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T19:30:58.000Z""},{""system_name"":""Ankou"",""system_name_lower"":""ankou"",""system_id"":""59e92c33d22c775be06fa6a6"",""state"":""none"",""influence"":0.193286,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T02:50:08.000Z""},{""system_name"":""Narakapani"",""system_name_lower"":""narakapani"",""system_id"":""59e9518ad22c775be0ddecec"",""state"":""boom"",""influence"":0.320641,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[{""state"":""outbreak"",""trend"":0}],""conflicts"":[],""updated_at"":""2024-12-27T22:01:20.000Z""},{""system_name"":""Murus"",""system_name_lower"":""murus"",""system_id"":""59e9518ed22c775be0ddf6dc"",""state"":""none"",""influence"":0.366,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[{""state"":""publicholiday"",""trend"":0},{""state"":""war"",""trend"":0}],""conflicts"":[{""type"":""war"",""status"":"""",""opponent_name"":""Lavigny's Legion"",""opponent_name_lower"":""lavigny's legion"",""opponent_faction_id"":""59e7b78cd22c775be0fe6a41"",""station_id"":null,""stake"":"""",""stake_lower"":"""",""days_won"":0}],""updated_at"":""2024-12-28T22:33:15.000Z""},{""system_name"":""Saha"",""system_name_lower"":""saha"",""system_id"":""59ea52f6d22c775be0b5a599"",""state"":""none"",""influence"":0.178287,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T18:33:21.000Z""},{""system_name"":""Maris"",""system_name_lower"":""maris"",""system_id"":""59e7e2f7d22c775be0ffc4e9"",""state"":""war"",""influence"":0.103483,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""war""}],""pending_states"":[],""recovering_states"":[],""conflicts"":[{""type"":""war"",""status"":""active"",""opponent_name"":""Maris Labour"",""opponent_name_lower"":""maris labour"",""opponent_faction_id"":""59e7e2f7d22c775be0ffc4fc"",""station_id"":""5a843cc9d1b6a37c3c343cbd"",""stake"":""Reeves Installation"",""stake_lower"":""reeves installation"",""days_won"":1}],""updated_at"":""2024-12-28T12:15:37.000Z""}],""government"":""corporate"",""name"":""Ankou Blue Federal Holdings"",""name_lower"":""ankou blue federal holdings"",""updated_at"":""2024-12-28T22:06:07.000Z""}],""total"":1,""limit"":10,""page"":1,""pages"":1,""pagingCounter"":1,""hasPrevPage"":false,""hasNextPage"":false,""prevPage"":null,""nextPage"":null}" );
            fakeBgsRestClient.Expect( "v5/factions?name=Natural Amemakarna Movement&page=1", @"{""docs"":[{""_id"":""59e7e0aad22c775be0ffba06"",""__v"":0,""allegiance"":""independent"",""eddb_id"":59823,""faction_presence"":[{""system_name"":""Amemakarna"",""system_name_lower"":""amemakarna"",""system_id"":""59e7e0aad22c775be0ffb9ed"",""state"":""none"",""influence"":0.169154,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-11-05T15:27:01.000Z""}],""government"":""dictatorship"",""name"":""Natural Amemakarna Movement"",""name_lower"":""natural amemakarna movement"",""updated_at"":""2024-11-05T15:27:01.000Z""}],""total"":1,""limit"":10,""page"":1,""pages"":1,""pagingCounter"":1,""hasPrevPage"":false,""hasNextPage"":false,""prevPage"":null,""nextPage"":null}" );
           
            // Save original data
            var data = ConfigService.Instance.crimeMonitorConfiguration;

            var config = ConfigService.FromJson<CrimeMonitorConfiguration>(crimeConfigJson);
            crimeMonitor.readRecord(config);

            // Bond Awarded Event
            line = "{ \"timestamp\":\"2019-04-22T11:51:30Z\", \"event\":\"FactionKillBond\", \"Reward\":32473, \"AwardingFaction\":\"Constitution Party of Aerial\", \"VictimFaction\":\"Ankou Blue Federal Holdings\" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBondAwardedEvent( (BondAwardedEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNotNull(record);
            Assert.AreEqual(3, record.factionReports.Count);
            Assert.AreEqual(94492, record.bondsAmount);

            // Bounty Awarded Event
            line = "{ \"timestamp\":\"2019-04-22T03:13:36Z\", \"event\":\"Bounty\", \"Rewards\":[ { \"Faction\":\"Calennero State Industries\", \"Reward\":22265 } ], \"Target\":\"adder\", \"TotalReward\":22265, \"VictimFaction\":\"Natural Amemakarna Movement\" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBountyAwardedEvent( (BountyAwardedEvent)events[ 0 ], true );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(2, record.factionReports.Count(r => r.bounty && r.crimeDef == Crime.None));
            Assert.AreEqual(127433, record.bountiesAmount);

            // Fine Incurred Event
            line = "{ \"timestamp\":\"2019-04-22T03:21:46Z\", \"event\":\"CommitCrime\", \"CrimeType\":\"dockingMinorTresspass\", \"Faction\":\"Constitution Party of Aerial\", \"Fine\":400 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleFineIncurredEvent( (FineIncurredEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNotNull(record);
            Assert.AreEqual(1, record.factionReports.Count(r => !r.bounty && r.crimeDef != Crime.None));
            Assert.AreEqual(400, record.finesIncurred.Sum(r => r.amount));

            // Bounty Incurred Event
            line = "{ \"timestamp\":\"2019-04-13T03:58:29Z\", \"event\":\"CommitCrime\", \"CrimeType\":\"assault\", \"Faction\":\"Calennero State Industries\", \"Victim\":\"Christofer\", \"Bounty\":400 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBountyIncurredEvent( (BountyIncurredEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            // The fine should be converted to a bounty, resulting in two bounty records.
            Assert.IsNotNull(record);
            Assert.AreEqual(2, record.factionReports.Count(r => r.bounty && r.crimeDef != Crime.None));
            Assert.AreEqual(800, record.bountiesIncurred.Sum(r => r.amount));

            // Redeem Bond Event
            line = "{ \"timestamp\":\"2019-04-09T10:31:31Z\", \"event\":\"RedeemVoucher\", \"Type\":\"CombatBond\", \"Amount\":94492, \"Factions\":[ { \"Faction\":\"Constitution Party of Aerial\", \"Amount\":94492 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBondRedeemedEvent( (BondRedeemedEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNotNull(record);
            Assert.AreEqual(0, record.factionReports.Count(r => !r.bounty && r.crimeDef == Crime.None));

            // Redeem Bounty Event - Multiple
            line = "{ \"timestamp\":\"2019-04-09T10:31:31Z\", \"event\":\"RedeemVoucher\", \"Type\":\"bounty\", \"Amount\":213896, \"Factions\":[ { \"Faction\":\"Calennero State Industries\", \"Amount\":105168 }, { \"Faction\":\"HIP 20277 Inc\", \"Amount\":108728 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBountyRedeemedEvent( (BountyRedeemedEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(0, record.factionReports.Count(r => r.bounty && r.crimeDef == Crime.None));
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "HIP 20277 Inc");
            Assert.IsNull(record);

            // Fine Paid Event
            line = "{ \"timestamp\":\"2019-04-09T15:12:10Z\", \"event\":\"PayFines\", \"Amount\":800, \"AllFines\":true, \"ShipID\":10 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleFinePaidEvent( (FinePaidEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNull(record);
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNull(record);

            // Restore original data
            ConfigService.Instance.crimeMonitorConfiguration = data;
        }

        [TestMethod, DoNotParallelize]
        public void TestCrimeShipTargeted()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeBgsRestClient.Expect( "v5/factions?name=Calennero Crew&page=1", @"{""docs"":[{""_id"":""59e7a7d6d22c775be0fe39ae"",""__v"":0,""allegiance"":""independent"",""eddb_id"":40524,""faction_presence"":[{""system_name"":""Calennero"",""system_name_lower"":""calennero"",""system_id"":""59e7a7d6d22c775be0fe39a9"",""state"":""famine"",""influence"":0.010111,""happiness"":""none"",""active_states"":[{""state"":""famine""},{""state"":""infrastructurefailure""}],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T10:14:59.000Z""}],""government"":""anarchy"",""name"":""Calennero Crew"",""name_lower"":""calennero crew"",""updated_at"":""2024-12-28T10:14:59.000Z""}],""total"":1,""limit"":10,""page"":1,""pages"":1,""pagingCounter"":1,""hasPrevPage"":false,""hasNextPage"":false,""prevPage"":null,""nextPage"":null}" );

            line = "{ \"timestamp\":\"2019-04-24T00:13:35Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"federation_corvette\", \"Ship_Localised\":\"Federal Corvette\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Kurt Pettersen;\", \"PilotName_Localised\":\"Kurt Pettersen\", \"PilotRank\":\"Deadly\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Calennero Crew\", \"LegalStatus\":\"Wanted\", \"Bounty\":295785 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor.handleShipTargetedEvent( (ShipTargetedEvent)events[ 0 ] );
            Assert.IsNotNull(crimeMonitor.shipTargets);
            Assert.AreEqual(1, crimeMonitor.shipTargets.Count);
            var target = crimeMonitor.shipTargets.FirstOrDefault(t => t.name == "Kurt Pettersen");
            Assert.IsNotNull(target);
            Assert.AreEqual(CombatRating.Deadly, target.CombatRank);
            Assert.AreEqual("Calennero Crew", target.faction);
            Assert.AreEqual(Superpower.Independent, target.Allegiance);
            Assert.AreEqual(295785, target.bounty);

            line = "{ \"timestamp\":\"2019-04-24T00:44:32Z\", \"event\":\"FSDJump\", \"StarSystem\":\"HIP 20277\", \"SystemAddress\":84053791442, \"StarPos\":[106.43750,-95.68750,-0.18750], \"SystemAllegiance\":\"Empire\", \"SystemEconomy\":\"$economy_Industrial;\", \"SystemEconomy_Localised\":\"Industrial\", \"SystemSecondEconomy\":\"$economy_Extraction;\", \"SystemSecondEconomy_Localised\":\"Extraction\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":11247202, \"Body\":\"HIP 20277\", \"BodyID\":0, \"BodyType\":\"Star\", \"JumpDist\":7.473, \"FuelUsed\":1.140420, \"FuelLevel\":61.122398, \"SystemFaction\":{ \"Name\":\"Calennero State Industries\", \"FactionState\":\"Boom\" } }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleJumpedEvent();
            Assert.AreEqual(0, crimeMonitor.shipTargets.Count);
        }

        // Test that we're able to detect and correct for simple scenarios where a bounty has been converted to an interstellar bounty
        [TestMethod]
        public void TestCrimeInterstellarFactorsScenario()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeBgsRestClient.Expect( "v5/factions?name=Radio Sidewinder Crew&page=1", @"{""docs"":[{""_id"":""59e7b855d22c775be0fe7538"",""__v"":0,""allegiance"":""independent"",""eddb_id"":74918,""faction_presence"":[{""system_name"":""Tago"",""system_name_lower"":""tago"",""system_id"":""59e7b855d22c775be0fe752d"",""state"":""none"",""influence"":0.339268,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T22:04:00.000Z""},{""system_name"":""Na Chem"",""system_name_lower"":""na chem"",""system_id"":""59e7bbf9d22c775be0fea304"",""state"":""civilliberty"",""influence"":0.602794,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""civilliberty""}],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T22:55:09.000Z""},{""system_name"":""31 Epsilon Librae"",""system_name_lower"":""31 epsilon librae"",""system_id"":""59e7be0cd22c775be0feb935"",""state"":""none"",""influence"":0.623131,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T11:15:55.000Z""},{""system_name"":""37 Librae"",""system_name_lower"":""37 librae"",""system_id"":""59e7b891d22c775be0fe78ba"",""state"":""war"",""influence"":0.749501,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-25T07:24:25.000Z""},{""system_name"":""Aditjargl"",""system_name_lower"":""aditjargl"",""system_id"":""59e7cd23d22c775be0ff2ae8"",""state"":""none"",""influence"":0.357988,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T08:59:58.000Z""},{""system_name"":""Ciguri"",""system_name_lower"":""ciguri"",""system_id"":""59e7ce3ad22c775be0ff34c5"",""state"":""none"",""influence"":0.419323,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T08:47:03.000Z""},{""system_name"":""Tachmetae"",""system_name_lower"":""tachmetae"",""system_id"":""59e8642ed22c775be0015fef"",""state"":""none"",""influence"":0.803,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-23T22:15:58.000Z""},{""system_name"":""HR 5706"",""system_name_lower"":""hr 5706"",""system_id"":""59e8ac18d22c775be0026aca"",""state"":""none"",""influence"":0.365174,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-09T04:54:55.000Z""},{""system_name"":""Wolf 561"",""system_name_lower"":""wolf 561"",""system_id"":""59e9cbe9d22c775be099172c"",""state"":""none"",""influence"":0.06,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T03:42:37.000Z""},{""system_name"":""Kucub Hua"",""system_name_lower"":""kucub hua"",""system_id"":""59eabb7ad22c775be0b71670"",""state"":""none"",""influence"":0.479879,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-08T14:30:49.000Z""},{""system_name"":""Nyanmil"",""system_name_lower"":""nyanmil"",""system_id"":""59e7a8ddd22c775be0fe4db6"",""state"":""civilliberty"",""influence"":0.412762,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-25T03:41:13.000Z""},{""system_name"":""Waka Mu"",""system_name_lower"":""waka mu"",""system_id"":""59eaf57ed22c775be0b794c4"",""state"":""none"",""influence"":0.278884,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-10T13:06:08.000Z""},{""system_name"":""NLTT 41712"",""system_name_lower"":""nltt 41712"",""system_id"":""59e7b891d22c775be0fe78ac"",""state"":""none"",""influence"":0.366337,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T22:42:14.000Z""},{""system_name"":""Mandins"",""system_name_lower"":""mandins"",""system_id"":""5a12035dd22c775be0901d41"",""state"":""none"",""influence"":0.465327,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-07T03:25:19.000Z""},{""system_name"":""Yu Tikua"",""system_name_lower"":""yu tikua"",""system_id"":""59ee4ee8d22c775be01467e3"",""state"":""none"",""influence"":0.356219,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T19:30:55.000Z""},{""system_name"":""Edindui"",""system_name_lower"":""edindui"",""system_id"":""59eb8e97d22c775be0ba0217"",""state"":""none"",""influence"":0.537538,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-24T07:55:17.000Z""},{""system_name"":""Gallaecian"",""system_name_lower"":""gallaecian"",""system_id"":""59ecea1ed22c775be05b3a22"",""state"":""none"",""influence"":0.355065,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-22T05:39:14.000Z""},{""system_name"":""Kannicas"",""system_name_lower"":""kannicas"",""system_id"":""59e9b3ecd22c775be098cceb"",""state"":""none"",""influence"":0.430279,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-19T06:02:17.000Z""},{""system_name"":""Changda"",""system_name_lower"":""changda"",""system_id"":""59e7b892d22c775be0fe78e0"",""state"":""none"",""influence"":0.23,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T23:06:53.000Z""},{""system_name"":""16 Librae"",""system_name_lower"":""16 librae"",""system_id"":""59e93b53d22c775be0895cb1"",""state"":""none"",""influence"":0.686747,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T12:44:51.000Z""},{""system_name"":""GD 175"",""system_name_lower"":""gd 175"",""system_id"":""59e8d171d22c775be0033108"",""state"":""none"",""influence"":0.071358,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T12:21:14.000Z""},{""system_name"":""LTT 5873"",""system_name_lower"":""ltt 5873"",""system_id"":""59eb0b91d22c775be0b7c798"",""state"":""none"",""influence"":0.11976,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-11T00:11:56.000Z""},{""system_name"":""LHS 53"",""system_name_lower"":""lhs 53"",""system_id"":""59e8cdf2d22c775be0031f23"",""state"":""none"",""influence"":0.292659,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T06:04:57.000Z""},{""system_name"":""LP 681-91"",""system_name_lower"":""lp 681-91"",""system_id"":""59e9cbbad22c775be099169c"",""state"":""none"",""influence"":0.224429,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T03:35:19.000Z""},{""system_name"":""Lardhampt"",""system_name_lower"":""lardhampt"",""system_id"":""59e80c7ed22c775be0007963"",""state"":""none"",""influence"":0.143863,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T06:03:42.000Z""},{""system_name"":""LP 560-66"",""system_name_lower"":""lp 560-66"",""system_id"":""59efcce6d22c775be0f8d98e"",""state"":""none"",""influence"":0.428714,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T13:03:49.000Z""},{""system_name"":""Zuben Elgenubi"",""system_name_lower"":""zuben elgenubi"",""system_id"":""59e7ef65d22c775be0000476"",""state"":""none"",""influence"":0.56888,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T08:15:03.000Z""},{""system_name"":""Umbrogo"",""system_name_lower"":""umbrogo"",""system_id"":""59eaba30d22c775be0b7138c"",""state"":""none"",""influence"":0.109328,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-25T14:33:41.000Z""},{""system_name"":""Brulenjan"",""system_name_lower"":""brulenjan"",""system_id"":""59ea0eabd22c775be09a099a"",""state"":""none"",""influence"":0.068,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T10:54:37.000Z""},{""system_name"":""Valhaling"",""system_name_lower"":""valhaling"",""system_id"":""59eba385d22c775be0d69d7e"",""state"":""none"",""influence"":0.059059,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T21:19:14.000Z""},{""system_name"":""Chin"",""system_name_lower"":""chin"",""system_id"":""59e91717d22c775be04ae0bb"",""state"":""none"",""influence"":0.633858,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T22:04:10.000Z""},{""system_name"":""HIP 78983"",""system_name_lower"":""hip 78983"",""system_id"":""59e9198cd22c775be04f3459"",""state"":""none"",""influence"":0.491508,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-19T21:54:13.000Z""},{""system_name"":""HIP 77749"",""system_name_lower"":""hip 77749"",""system_id"":""5a15ccb453582c78c940f74a"",""state"":""none"",""influence"":0.084562,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-24T10:03:22.000Z""},{""system_name"":""Lalande 26630"",""system_name_lower"":""lalande 26630"",""system_id"":""59ee1c96d22c775be0e81bd7"",""state"":""none"",""influence"":0.208,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T08:22:23.000Z""},{""system_name"":""HIP 76292"",""system_name_lower"":""hip 76292"",""system_id"":""59ea4b7ed22c775be0ac6a22"",""state"":""none"",""influence"":0.061815,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T20:23:32.000Z""},{""system_name"":""Savaroju"",""system_name_lower"":""savaroju"",""system_id"":""59e91c92d22c775be0545d12"",""state"":""none"",""influence"":0.069069,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T06:55:47.000Z""},{""system_name"":""Walmiki"",""system_name_lower"":""walmiki"",""system_id"":""5a07ca13d22c775be06f3212"",""state"":""none"",""influence"":0.106362,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T06:12:22.000Z""},{""system_name"":""Pijan"",""system_name_lower"":""pijan"",""system_id"":""59eb4587d22c775be0b8a005"",""state"":""none"",""influence"":0.426295,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-24T00:02:25.000Z""},{""system_name"":""Akeretia"",""system_name_lower"":""akeretia"",""system_id"":""59e92e3bd22c775be0733b4a"",""state"":""none"",""influence"":0.165,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-11-22T18:24:00.000Z""},{""system_name"":""LTT 6139"",""system_name_lower"":""ltt 6139"",""system_id"":""59e8c2e0d22c775be002e4ed"",""state"":""war"",""influence"":0.084562,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""war""}],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[{""type"":""war"",""status"":""active"",""opponent_name"":""LTT 6139 Gold Advanced Solutions"",""opponent_name_lower"":""ltt 6139 gold advanced solutions"",""opponent_faction_id"":""59e8c2e0d22c775be002e501"",""station_id"":null,""stake"":""Dovbush Industrial Site"",""stake_lower"":""dovbush industrial site"",""days_won"":0}],""updated_at"":""2024-12-27T04:33:38.000Z""},{""system_name"":""Almudjing"",""system_name_lower"":""almudjing"",""system_id"":""59ec2456d22c775be035c184"",""state"":""none"",""influence"":0.483,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T07:00:19.000Z""},{""system_name"":""HR 5816"",""system_name_lower"":""hr 5816"",""system_id"":""59ea11f7d22c775be09a166c"",""state"":""none"",""influence"":0.060217,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-25T19:58:35.000Z""},{""system_name"":""Kuo Zhi"",""system_name_lower"":""kuo zhi"",""system_id"":""59f65042d22c775be0d47326"",""state"":""none"",""influence"":0.102564,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T13:14:13.000Z""},{""system_name"":""LTT 6431"",""system_name_lower"":""ltt 6431"",""system_id"":""59f0e83fd22c775be02ef98f"",""state"":""none"",""influence"":0.074925,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T16:59:23.000Z""},{""system_name"":""HR 6269"",""system_name_lower"":""hr 6269"",""system_id"":""5a314c1253582c78c9d2cb57"",""state"":""none"",""influence"":0.127976,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-23T12:44:26.000Z""},{""system_name"":""Boann"",""system_name_lower"":""boann"",""system_id"":""59e7b748d22c775be0fe675c"",""state"":""none"",""influence"":0.083,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-27T07:29:43.000Z""},{""system_name"":""LP 744-46"",""system_name_lower"":""lp 744-46"",""system_id"":""59e86e78d22c775be0017cea"",""state"":""none"",""influence"":0.049751,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-17T18:45:33.000Z""},{""system_name"":""Kalb"",""system_name_lower"":""kalb"",""system_id"":""59ee4545d22c775be00221a5"",""state"":""none"",""influence"":0.055721,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T15:51:42.000Z""},{""system_name"":""Leerham"",""system_name_lower"":""leerham"",""system_id"":""59e89708d22c775be002117b"",""state"":""none"",""influence"":0.145563,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T22:10:32.000Z""},{""system_name"":""Dulla"",""system_name_lower"":""dulla"",""system_id"":""59e96a70d22c775be02c3aff"",""state"":""none"",""influence"":0.10978,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[{""state"":""expansion"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-04T12:07:30.000Z""}],""government"":""democracy"",""name"":""Radio Sidewinder Crew"",""name_lower"":""radio sidewinder crew"",""updated_at"":""2024-12-28T22:55:09.000Z""}],""total"":1,""limit"":10,""page"":1,""pages"":1,""pagingCounter"":1,""hasPrevPage"":false,""hasNextPage"":false,""prevPage"":null,""nextPage"":null}" );

            var line1 = @"{ ""timestamp"":""2022-01-15T18:37:38Z"", ""event"":""CommitCrime"", ""CrimeType"":""assault"", ""Faction"":""Radio Sidewinder Crew"", ""Victim"":""Jim Grady"", ""Bounty"":100 }";
            var line2 = @"{ ""timestamp"":""2022-01-15T18:41:31Z"", ""event"":""PayBounties"", ""Amount"":100, ""Faction"":""$faction_Independent;"", ""Faction_Localised"":""Independent"", ""ShipID"":38, ""BrokerPercentage"":25.000000 }";

            // Save original data
            var data = ConfigService.Instance.crimeMonitorConfiguration;

            // Load a known empty state
            var config = new CrimeMonitorConfiguration();
            crimeMonitor.readRecord(config);

            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=Tachmetae", @"{""min_max"":[{""id64"":2869977949641,""name"":""Tachmetae"",""x"":-0.59375,""y"":60.6875,""z"":84.71875}],""values"":[""Tachmetae""]}" );
            fakeSpanshRestClient.Expect( "dump/2869977949641", Encoding.UTF8.GetString( Tests.Properties.Resources.SpanshStarSystemDumpTachmetae ) );
            
            // Set a bounty with `Radio Sidewinder Crew`
            events = JournalMonitor.ParseJournalEntry(line1);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBountyIncurredEvent( (BountyIncurredEvent)events[ 0 ] );
            Assert.AreEqual(1, crimeMonitor.criminalrecord.Count);
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Radio Sidewinder Crew");
            Assert.IsNotNull(record);
            Assert.AreEqual(1, record.factionReports.Count(r => r.bounty && r.crimeDef != Crime.None));
            Assert.AreEqual(100, record.bountiesIncurred.Sum(r => r.amount));

            // Test whether we're able to identify and remove the bounty after it has been converted to an interstellar bounty
            events = JournalMonitor.ParseJournalEntry(line2);
            Assert.AreEqual(1, events.Count);
            crimeMonitor._handleBountyPaidEvent( (BountyPaidEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Radio Sidewinder Crew");
            Assert.IsNull(record);
            Assert.AreEqual(0, crimeMonitor.criminalrecord.Count);

            // Restore original data
            ConfigService.Instance.crimeMonitorConfiguration = data;
        }
    }
}
