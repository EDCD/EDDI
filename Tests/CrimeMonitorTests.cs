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
            //fakeBgsRestClient.Expect( "v5/factions?name=Ankou Blue Federal Holdings&page=1", @"{""docs"":[{""_id"":""59e7b73bd22c775be0fe66d7"",""__v"":0,""allegiance"":""empire"",""eddb_id"":12284,""faction_presence"":[{""system_name"":""Aerial"",""system_name_lower"":""aerial"",""system_id"":""59e7b78cd22c775be0fe6a3e"",""state"":""election"",""influence"":0.139653,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""election""}],""pending_states"":[],""recovering_states"":[{""state"":""infrastructurefailure"",""trend"":0}],""conflicts"":[{""type"":""election"",""status"":""active"",""opponent_name"":""Murus Major Industry"",""opponent_name_lower"":""murus major industry"",""opponent_faction_id"":""59e7b78cd22c775be0fe6a3a"",""station_id"":null,""stake"":"""",""stake_lower"":"""",""days_won"":0}],""updated_at"":""2024-12-28T10:26:53.000Z""},{""system_name"":""Chamuluma"",""system_name_lower"":""chamuluma"",""system_id"":""59e7e372d22c775be0ffc841"",""state"":""boom"",""influence"":0.732268,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""boom""}],""pending_states"":[{""state"":""outbreak"",""trend"":0}],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T16:11:14.000Z""},{""system_name"":""HR 1475"",""system_name_lower"":""hr 1475"",""system_id"":""59e7f2e4d22c775be00013a2"",""state"":""boom"",""influence"":0.055,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T22:06:07.000Z""},{""system_name"":""Carthage"",""system_name_lower"":""carthage"",""system_id"":""59e83bf3d22c775be001085f"",""state"":""none"",""influence"":0.106893,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T19:30:58.000Z""},{""system_name"":""Ankou"",""system_name_lower"":""ankou"",""system_id"":""59e92c33d22c775be06fa6a6"",""state"":""none"",""influence"":0.193286,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-28T02:50:08.000Z""},{""system_name"":""Narakapani"",""system_name_lower"":""narakapani"",""system_id"":""59e9518ad22c775be0ddecec"",""state"":""boom"",""influence"":0.320641,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[{""state"":""outbreak"",""trend"":0}],""conflicts"":[],""updated_at"":""2024-12-27T22:01:20.000Z""},{""system_name"":""Murus"",""system_name_lower"":""murus"",""system_id"":""59e9518ed22c775be0ddf6dc"",""state"":""none"",""influence"":0.366,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[{""state"":""publicholiday"",""trend"":0},{""state"":""war"",""trend"":0}],""conflicts"":[{""type"":""war"",""status"":"""",""opponent_name"":""Lavigny's Legion"",""opponent_name_lower"":""lavigny's legion"",""opponent_faction_id"":""59e7b78cd22c775be0fe6a41"",""station_id"":null,""stake"":"""",""stake_lower"":"""",""days_won"":0}],""updated_at"":""2024-12-28T22:33:15.000Z""},{""system_name"":""Saha"",""system_name_lower"":""saha"",""system_id"":""59ea52f6d22c775be0b5a599"",""state"":""none"",""influence"":0.178287,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-12-26T18:33:21.000Z""},{""system_name"":""Maris"",""system_name_lower"":""maris"",""system_id"":""59e7e2f7d22c775be0ffc4e9"",""state"":""war"",""influence"":0.103483,""happiness"":""$faction_happinessband2;"",""active_states"":[{""state"":""war""}],""pending_states"":[],""recovering_states"":[],""conflicts"":[{""type"":""war"",""status"":""active"",""opponent_name"":""Maris Labour"",""opponent_name_lower"":""maris labour"",""opponent_faction_id"":""59e7e2f7d22c775be0ffc4fc"",""station_id"":""5a843cc9d1b6a37c3c343cbd"",""stake"":""Reeves Installation"",""stake_lower"":""reeves installation"",""days_won"":1}],""updated_at"":""2024-12-28T12:15:37.000Z""}],""government"":""corporate"",""name"":""Ankou Blue Federal Holdings"",""name_lower"":""ankou blue federal holdings"",""updated_at"":""2024-12-28T22:06:07.000Z""}],""total"":1,""limit"":10,""page"":1,""pages"":1,""pagingCounter"":1,""hasPrevPage"":false,""hasNextPage"":false,""prevPage"":null,""nextPage"":null}" );
            //fakeBgsRestClient.Expect( "v5/factions?name=Natural Amemakarna Movement&page=1", @"{""docs"":[{""_id"":""59e7e0aad22c775be0ffba06"",""__v"":0,""allegiance"":""independent"",""eddb_id"":59823,""faction_presence"":[{""system_name"":""Amemakarna"",""system_name_lower"":""amemakarna"",""system_id"":""59e7e0aad22c775be0ffb9ed"",""state"":""none"",""influence"":0.169154,""happiness"":""$faction_happinessband2;"",""active_states"":[],""pending_states"":[],""recovering_states"":[],""conflicts"":[],""updated_at"":""2024-11-05T15:27:01.000Z""}],""government"":""dictatorship"",""name"":""Natural Amemakarna Movement"",""name_lower"":""natural amemakarna movement"",""updated_at"":""2024-11-05T15:27:01.000Z""}],""total"":1,""limit"":10,""page"":1,""pages"":1,""pagingCounter"":1,""hasPrevPage"":false,""hasNextPage"":false,""prevPage"":null,""nextPage"":null}" );
           
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
            fakeSpanshRestClient.Expect( "dump/5581611930322", Encoding.UTF8.GetString( Properties.Resources.SpanshStarSystemDumpCalenerro ) );
            EDDI.Instance.CurrentStarSystem = fakeSpanshService.GetStarSystem( 5581611930322 );

            line = "{ \"timestamp\":\"2019-04-24T00:13:35Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"federation_corvette\", \"Ship_Localised\":\"Federal Corvette\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Kurt Pettersen;\", \"PilotName_Localised\":\"Kurt Pettersen\", \"PilotRank\":\"Deadly\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Calennero Crew\", \"LegalStatus\":\"Wanted\", \"Bounty\":295785 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            crimeMonitor.postHandleShipTargetedEvent( (ShipTargetedEvent)events[ 0 ] );
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
            var line1 = @"{ ""timestamp"":""2022-01-15T18:37:38Z"", ""event"":""CommitCrime"", ""CrimeType"":""assault"", ""Faction"":""Radio Sidewinder Crew"", ""Victim"":""Jim Grady"", ""Bounty"":100 }";
            var line2 = @"{ ""timestamp"":""2022-01-15T18:41:31Z"", ""event"":""PayBounties"", ""Amount"":100, ""Faction"":""$faction_Independent;"", ""Faction_Localised"":""Independent"", ""ShipID"":38, ""BrokerPercentage"":25.000000 }";

            // Save original data
            var data = ConfigService.Instance.crimeMonitorConfiguration;

            // Load a known empty state
            var config = new CrimeMonitorConfiguration();
            crimeMonitor.readRecord(config);

            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "systems/search?={\"filters\":{\"minor_faction_presences\":{\"value\":[\"Radio Sidewinder Crew\"]}},\"size\":500,\"page\":0}", Encoding.UTF8.GetString( Properties.Resources.SpanshQueryFactionRadioSidewinderCrew ) );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=Tachmetae", @"{""min_max"":[{""id64"":2869977949641,""name"":""Tachmetae"",""x"":-0.59375,""y"":60.6875,""z"":84.71875}],""values"":[""Tachmetae""]}" );
            fakeSpanshRestClient.Expect( "dump/2869977949641", Encoding.UTF8.GetString( Properties.Resources.SpanshStarSystemDumpTachmetae ) );

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
