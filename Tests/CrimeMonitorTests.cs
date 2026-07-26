using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class CrimeMonitorTests : TestBase
    {
        readonly CrimeMonitor crimeMonitor = new();
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
            Assert.HasCount( 3, config.criminalrecord);
            Assert.AreEqual(275915, config.criminalrecord.Sum(r => r.claims));
            Assert.AreEqual(400, config.criminalrecord.Sum(r => r.fines));

            record = config.criminalrecord.ToList().FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(Superpower.Empire, record.Allegiance);
            Assert.AreEqual("Empire", record.allegiance);
            Assert.AreEqual(105168, record.bountiesAmount);
            Assert.AreEqual(400, record.finesIncurred.Sum(r => r.amount));

            // Verify faction report object 
            Assert.HasCount( 2, record.factionReports );
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

        [TestMethod, DoNotParallelize]
        public async Task TestCrimeEventsScenario()
        {
            EDDI.Instance.DataProvider = CreateTestDataProvider();
            FakeSpanshHttpClient.Expect( @"systems/search?json={""filters"":{""minor_faction_presences"":[{""name"":{""value"":[""Calennero State Industries""]}}]},""size"":500,""page"":0}", Encoding.UTF8.GetString( Properties.Resources.SpanshQueryFactionCalenneroCrew ) );
            FakeSpanshHttpClient.Expect( @"systems/search?json={""filters"":{""minor_faction_presences"":[{""name"":{""value"":[""Radio Sidewinder Crew""]}}]},""size"":500,""page"":0}", Encoding.UTF8.GetString( Properties.Resources.SpanshQueryFactionRadioSidewinderCrew ) );

            // Save original data
            var data = ConfigService.Instance.crimeMonitorConfiguration;

            var config = ConfigService.FromJson<CrimeMonitorConfiguration>(crimeConfigJson);
            crimeMonitor.readRecord(config);

            // Bond Awarded Event
            line = "{ \"timestamp\":\"2019-04-22T11:51:30Z\", \"event\":\"FactionKillBond\", \"Reward\":32473, \"AwardingFaction\":\"Constitution Party of Aerial\", \"VictimFaction\":\"Radio Sidewinder Crew\" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            await crimeMonitor._handleBondAwardedEventAsync( (BondAwardedEvent)events[ 0 ] ).ConfigureAwait(false);
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNotNull(record);
            Assert.HasCount( 3, record.factionReports );
            Assert.AreEqual(94492, record.bondsAmount);

            // Bounty Awarded Event
            line = "{ \"timestamp\":\"2019-04-22T03:13:36Z\", \"event\":\"Bounty\", \"Rewards\":[ { \"Faction\":\"Calennero State Industries\", \"Reward\":22265 } ], \"Target\":\"adder\", \"TotalReward\":22265, \"VictimFaction\":\"Natural Amemakarna Movement\" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            await crimeMonitor._handleBountyAwardedEventAsync( (BountyAwardedEvent)events[ 0 ] ).ConfigureAwait(false);
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(2, record.factionReports.Count(r => r.bounty && r.crimeDef == Crime.None));
            Assert.AreEqual(127433, record.bountiesAmount);

            // Fine Incurred Event
            line = "{ \"timestamp\":\"2019-04-22T03:21:46Z\", \"event\":\"CommitCrime\", \"CrimeType\":\"dockingMinorTresspass\", \"Faction\":\"Constitution Party of Aerial\", \"Fine\":400 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            await crimeMonitor._handleFineIncurredEventAsync( (FineIncurredEvent)events[ 0 ] ).ConfigureAwait(false);
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNotNull(record);
            Assert.ContainsSingle( r => !r.bounty && r.crimeDef != Crime.None, record.factionReports);
            Assert.AreEqual(400, record.finesIncurred.Sum(r => r.amount));

            // Bounty Incurred Event
            line = "{ \"timestamp\":\"2019-04-13T03:58:29Z\", \"event\":\"CommitCrime\", \"CrimeType\":\"assault\", \"Faction\":\"Calennero State Industries\", \"Victim\":\"Christofer\", \"Bounty\":400 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            await crimeMonitor._handleBountyIncurredEventAsync( (BountyIncurredEvent)events[ 0 ] ).ConfigureAwait(false);
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            // The fine should be converted to a bounty, resulting in two bounty records.
            Assert.IsNotNull(record);
            Assert.AreEqual(2, record.factionReports.Count(r => r.bounty && r.crimeDef != Crime.None));
            Assert.AreEqual(800, record.bountiesIncurred.Sum(r => r.amount));

            // Redeem Bond Event
            line = "{ \"timestamp\":\"2019-04-09T10:31:31Z\", \"event\":\"RedeemVoucher\", \"Type\":\"CombatBond\", \"Amount\":94492, \"Factions\":[ { \"Faction\":\"Constitution Party of Aerial\", \"Amount\":94492 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            crimeMonitor._handleBondRedeemedEvent( (BondRedeemedEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNotNull(record);
            Assert.AreEqual(0, record.factionReports.Count(r => !r.bounty && r.crimeDef == Crime.None));

            // Redeem Bounty Event - Multiple
            line = "{ \"timestamp\":\"2019-04-09T10:31:31Z\", \"event\":\"RedeemVoucher\", \"Type\":\"bounty\", \"Amount\":213896, \"Factions\":[ { \"Faction\":\"Calennero State Industries\", \"Amount\":105168 }, { \"Faction\":\"HIP 20277 Inc\", \"Amount\":108728 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            crimeMonitor._handleBountyRedeemedEvent( (BountyRedeemedEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(0, record.factionReports.Count(r => r.bounty && r.crimeDef == Crime.None));
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "HIP 20277 Inc");
            Assert.IsNull(record);

            // Fine Paid Event
            line = "{ \"timestamp\":\"2019-04-09T15:12:10Z\", \"event\":\"PayFines\", \"Amount\":800, \"AllFines\":true, \"ShipID\":10 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            crimeMonitor._handleFinePaidEvent( (FinePaidEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(800, record.bountiesIncurred.Sum(r => r.amount));
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNull(record);

            // Restore original data
            ConfigService.Instance.crimeMonitorConfiguration = data;
        }

        [TestMethod, DoNotParallelize]
        public async Task TestCrimeShipTargeted()
        {
            EDDI.Instance.DataProvider = CreateTestDataProvider();
            FakeSpanshHttpClient.Expect( "dump/5581611930322", Encoding.UTF8.GetString( Properties.Resources.SpanshStarSystemDumpCalenerro ) );
            EDDI.Instance.CurrentStarSystem = await EDDI.Instance.DataProvider.GetOrFetchStarSystemAsync( 5581611930322 ).ConfigureAwait(false);

            line = "{ \"timestamp\":\"2019-04-24T00:13:35Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"federation_corvette\", \"Ship_Localised\":\"Federal Corvette\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Kurt Pettersen;\", \"PilotName_Localised\":\"Kurt Pettersen\", \"PilotRank\":\"Deadly\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Calennero Crew\", \"LegalStatus\":\"Wanted\", \"Bounty\":295785 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            await crimeMonitor.postHandleShipTargetedEventAsync( (ShipTargetedEvent)events[ 0 ] ).ConfigureAwait(false);
            Assert.IsNotNull(crimeMonitor.shipTargets);
            Assert.HasCount( 1, crimeMonitor.shipTargets );
            var target = crimeMonitor.shipTargets.FirstOrDefault(t => t.name == "Kurt Pettersen");
            Assert.IsNotNull(target);
            Assert.AreEqual(CombatRating.Deadly, target.CombatRank);
            Assert.AreEqual("Calennero Crew", target.faction);
            Assert.AreEqual(Superpower.Independent, target.Allegiance);
            Assert.AreEqual(295785, target.bounty);

            line = "{ \"timestamp\":\"2019-04-24T00:44:32Z\", \"event\":\"FSDJump\", \"StarSystem\":\"HIP 20277\", \"SystemAddress\":84053791442, \"StarPos\":[106.43750,-95.68750,-0.18750], \"SystemAllegiance\":\"Empire\", \"SystemEconomy\":\"$economy_Industrial;\", \"SystemEconomy_Localised\":\"Industrial\", \"SystemSecondEconomy\":\"$economy_Extraction;\", \"SystemSecondEconomy_Localised\":\"Extraction\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":11247202, \"Body\":\"HIP 20277\", \"BodyID\":0, \"BodyType\":\"Star\", \"JumpDist\":7.473, \"FuelUsed\":1.140420, \"FuelLevel\":61.122398, \"SystemFaction\":{ \"Name\":\"Calennero State Industries\", \"FactionState\":\"Boom\" } }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            crimeMonitor._handleJumpedEvent();
            Assert.HasCount( 0, crimeMonitor.shipTargets );
        }

        [TestMethod, DoNotParallelize]
        public void TestFinePaidClearsOnlyFineReports()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var lostRecord = new FactionRecord( "LOST Industrial Technologies" )
            {
                Allegiance = Superpower.Independent,
                fines = 400,
                bounties = 753685
            };
            lostRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T06:40:10Z" ), false,
                Crime.TrespassMinor, "HIP 37722", 400 ) );
            lostRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T06:42:00Z" ), true,
                Crime.Assault, "HIP 37722", 753685 ) );
            crimeMonitor.criminalrecord.Add( lostRecord );

            line = @"{ ""timestamp"":""2026-05-26T06:51:10Z"", ""event"":""PayFines"", ""Amount"":400, ""AllFines"":true, ""ShipID"":143 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            Assert.IsTrue( crimeMonitor._handleFinePaidEvent( (FinePaidEvent)events[ 0 ] ) );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "LOST Industrial Technologies" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 0, record.fines );
            Assert.AreEqual( 753685, record.bounties );
            Assert.HasCount( 0, record.finesIncurred );
            Assert.ContainsSingle( r => r.bounty && r.crimeDef != Crime.None, record.factionReports );
        }

        [TestMethod, DoNotParallelize]
        public void TestFinePaidWithoutFineMatchDoesNotClearUnrelatedBounties()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var bountyRecord = new FactionRecord( "LOST Industrial Technologies" )
            {
                Allegiance = Superpower.Independent,
                bounties = 882242
            };
            bountyRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T14:10:00Z" ), true,
                Crime.Assault, "HIP 37722", 882242 ) );
            crimeMonitor.criminalrecord.Add( bountyRecord );

            line = @"{ ""timestamp"":""2026-05-26T14:14:48Z"", ""event"":""PayFines"", ""Amount"":100000, ""AllFines"":true, ""ShipID"":143 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            Assert.IsFalse( crimeMonitor._handleFinePaidEvent( (FinePaidEvent)events[ 0 ] ) );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "LOST Industrial Technologies" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 882242, record.bounties );
            Assert.ContainsSingle( r => r.bounty && r.crimeDef != Crime.None, record.factionReports );
        }

        [TestMethod, DoNotParallelize]
        public void TestLegacyFinePaidBountyFallbackRequiresUniqueMatch()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var firstRecord = new FactionRecord( "First Faction" )
            {
                Allegiance = Superpower.Independent,
                bounties = 100
            };
            firstRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:00:00Z" ), true,
                Crime.Assault, "HIP 37722", 100 ) );
            var secondRecord = new FactionRecord( "Second Faction" )
            {
                Allegiance = Superpower.Independent,
                bounties = 100
            };
            secondRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:01:00Z" ), true,
                Crime.Assault, "HIP 37722", 100 ) );
            crimeMonitor.criminalrecord.Add( firstRecord );
            crimeMonitor.criminalrecord.Add( secondRecord );

            line = @"{ ""timestamp"":""2026-05-26T10:05:00Z"", ""event"":""PayFines"", ""Amount"":100, ""AllFines"":true, ""ShipID"":143 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            Assert.IsFalse( crimeMonitor._handleFinePaidEvent( (FinePaidEvent)events[ 0 ] ) );
            Assert.HasCount( 2, crimeMonitor.criminalrecord );
            Assert.IsTrue( crimeMonitor.criminalrecord.All( r => r.bounties == 100 ) );

            crimeMonitor._RemoveRecord( secondRecord );

            Assert.IsTrue( crimeMonitor._handleFinePaidEvent( (FinePaidEvent)events[ 0 ] ) );
            Assert.HasCount( 0, crimeMonitor.criminalrecord );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestSameSuperpowerBountyAddsToActiveSuperpowerRecord()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var empireRecord = new FactionRecord( "Empire" )
            {
                Allegiance = Superpower.Empire,
                bounties = 12000,
                interstellarBountyFactions = [ "First Imperial Faction" ]
            };
            empireRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:00:00Z" ), true,
                Crime.Assault, "Achenar", 12000 ) );
            var minorRecord = new FactionRecord( "Second Imperial Faction" )
            {
                Allegiance = Superpower.Empire
            };
            crimeMonitor.criminalrecord.Add( empireRecord );
            crimeMonitor.criminalrecord.Add( minorRecord );

            line = @"{ ""timestamp"":""2026-05-26T10:05:00Z"", ""event"":""CommitCrime"", ""CrimeType"":""assault"", ""Faction"":""Second Imperial Faction"", ""Victim"":""Target"", ""Bounty"":400 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            await crimeMonitor._handleBountyIncurredEventAsync( (BountyIncurredEvent)events[ 0 ] ).ConfigureAwait( false );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Empire" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 12400, record.bounties );
            Assert.Contains( "Second Imperial Faction", record.interstellarBountyFactions );
            Assert.HasCount( 2, record.bountiesIncurred);
            Assert.IsNull( crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Second Imperial Faction" ) );
        }

        [TestMethod, DoNotParallelize]
        public void TestMultiFactionCombatBondRedemptionClearsAllListedFactions()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var firstRecord = new FactionRecord( "First Bond Faction" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 100
            };
            firstRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:00:00Z" ), false,
                Crime.None, "HIP 37722", 100 ) );
            var secondRecord = new FactionRecord( "Second Bond Faction" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 200
            };
            secondRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:01:00Z" ), false,
                Crime.None, "HIP 37722", 200 ) );
            crimeMonitor.criminalrecord.Add( firstRecord );
            crimeMonitor.criminalrecord.Add( secondRecord );

            line = @"{ ""timestamp"":""2026-05-26T10:05:00Z"", ""event"":""RedeemVoucher"", ""Type"":""CombatBond"", ""Amount"":300, ""Factions"":[ { ""Faction"":""First Bond Faction"", ""Amount"":100 }, { ""Faction"":""Second Bond Faction"", ""Amount"":200 } ] }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            Assert.IsTrue( crimeMonitor._handleBondRedeemedEvent( (BondRedeemedEvent)events[ 0 ] ) );

            Assert.HasCount( 0, crimeMonitor.criminalrecord );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestDataVoucherEventsAreOutOfScope()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var payeeRecord = new FactionRecord( "Payee Faction" )
            {
                Allegiance = Superpower.Independent
            };
            crimeMonitor.criminalrecord.Add( payeeRecord );

            line = @"{ ""timestamp"":""2026-05-26T10:00:00Z"", ""event"":""DatalinkVoucher"", ""Reward"":500, ""VictimFaction"":""Victim Faction"", ""PayeeFaction"":""Payee Faction"" }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Payee Faction" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 0, record.claims );
            Assert.HasCount( 0, record.factionReports );

            line = @"{ ""timestamp"":""2026-05-26T10:05:00Z"", ""event"":""RedeemVoucher"", ""Type"":""settlement"", ""Amount"":500, ""Factions"":[ { ""Faction"":""Payee Faction"", ""Amount"":500 } ] }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Payee Faction" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 0, record.claims );
            Assert.HasCount( 0, record.factionReports );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestBountyAwardStoresAndDisplaysJournalAmount()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            EDDI.Instance.CurrentStarSystem = new StarSystem
            {
                systemname = "Power Territory",
                systemAddress = 1,
                Power = Power.ALavignyDuval
            };
            var factionRecord = new FactionRecord( "Awarding Faction" )
            {
                Allegiance = Superpower.Empire
            };
            crimeMonitor.criminalrecord.Add( factionRecord );

            line = @"{ ""timestamp"":""2026-05-26T10:00:00Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""Awarding Faction"", ""Reward"":100 } ], ""Target"":""adder"", ""TotalReward"":100, ""VictimFaction"":""Victim Faction"" }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            await crimeMonitor._handleBountyAwardedEventAsync( (BountyAwardedEvent)events[ 0 ] ).ConfigureAwait( false );

            Assert.AreEqual( 100, factionRecord.baseclaims );
            Assert.AreEqual( 100, factionRecord.claims );
            Assert.AreEqual( 100, factionRecord.basebountyclaims );
            Assert.AreEqual( 100, factionRecord.bountyclaims );
            Assert.AreEqual( 100, factionRecord.bountiesAmount );
            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 100L, variables[ "claims" ].Item2 );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestInShipBountyAwardJournalRewardsRemainUnmultiplied()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            var bountyLines = new[]
            {
                @"{ ""timestamp"":""2026-06-01T06:05:03Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""GAT 2 Exchange"", ""Reward"":118008 }, { ""Faction"":""LOST Industrial Technologies"", ""Reward"":174508 }, { ""Faction"":""Canonn"", ""Reward"":88638 } ], ""PilotName"":""$npc_name_decorate:#name=Hadzihasanovic;"", ""PilotName_Localised"":""Hadzihasanovic"", ""Target"":""vulture"", ""TotalReward"":381154, ""VictimFaction"":""Children of Raxxla"" }",
                @"{ ""timestamp"":""2026-06-01T06:05:36Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""LOST Industrial Technologies"", ""Reward"":209622 }, { ""Faction"":""Pereng Asli General Ltd"", ""Reward"":70854 } ], ""PilotName"":""$npc_name_decorate:#name=Michael Flexarius;"", ""PilotName_Localised"":""Michael Flexarius"", ""Target"":""asp"", ""Target_Localised"":""Asp Explorer"", ""TotalReward"":280476, ""VictimFaction"":""LTT 4730 Noblement"" }",
                @"{ ""timestamp"":""2026-06-01T06:11:23Z"", ""event"":""Bounty"", ""Rewards"":[ { ""Faction"":""LOST Industrial Technologies"", ""Reward"":229024 }, { ""Faction"":""Canonn"", ""Reward"":167112 } ], ""PilotName"":""$npc_name_decorate:#name=Sigurd Arvik;"", ""PilotName_Localised"":""Sigurd Arvik"", ""Target"":""vulture"", ""TotalReward"":396136, ""VictimFaction"":""Children of Raxxla"" }"
            };

            foreach ( var bountyLine in bountyLines )
            {
                events = JournalMonitor.ParseJournalEntry( bountyLine );
                Assert.HasCount( 1, events );
                await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );
            }

            Assert.AreEqual( 118008, crimeMonitor.criminalrecord.Single( r => r.faction == "GAT 2 Exchange" ).claims );
            Assert.AreEqual( 613154, crimeMonitor.criminalrecord.Single( r => r.faction == "LOST Industrial Technologies" ).claims );
            Assert.AreEqual( 70854, crimeMonitor.criminalrecord.Single( r => r.faction == "Pereng Asli General Ltd" ).claims );
            Assert.AreEqual( 255750, crimeMonitor.criminalrecord.Single( r => r.faction == "Canonn" ).claims );
        }

        [TestMethod, DoNotParallelize]
        public void TestBountyVoucherRedemptionUsesJournalAmount()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var factionRecord = new FactionRecord( "Awarding Faction" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 100
            };
            factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:00:00Z" ), true,
                Crime.None, "HIP 37722", 100 )
            {
                claimtype = FactionReport.BountyClaimType
            } );
            crimeMonitor.criminalrecord.Add( factionRecord );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 100, factionRecord.baseclaims );
            Assert.AreEqual( 100L, variables[ "claims" ].Item2 );
            Assert.AreEqual( 100, factionRecord.claims );
            Assert.AreEqual( 100, factionRecord.bountyclaims );

            line = @"{ ""timestamp"":""2026-05-26T10:05:00Z"", ""event"":""RedeemVoucher"", ""Type"":""bounty"", ""Amount"":100, ""Factions"":[ { ""Faction"":""Awarding Faction"", ""Amount"":100 } ] }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            Assert.IsTrue( crimeMonitor._handleBountyRedeemedEvent( (BountyRedeemedEvent)events[ 0 ] ) );
            Assert.IsNull( crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Awarding Faction" ) );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestOnFootRecoverClearsResolvedFineAndBountyCost()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            var eventLines = new[]
            {
                @"{ ""timestamp"":""2026-06-01T04:00:30Z"", ""event"":""CommitCrime"", ""CrimeType"":""onFoot_carryingIllegalData"", ""Faction"":""The Buurian Protectorate"", ""Fine"":12500 }",
                @"{ ""timestamp"":""2026-06-01T04:00:39Z"", ""event"":""CommitCrime"", ""CrimeType"":""onFoot_murder"", ""Faction"":""The Buurian Protectorate"", ""Victim"":""Seerat Douglas"", ""Bounty"":1000 }",
                @"{ ""timestamp"":""2026-06-01T04:00:53Z"", ""event"":""CommitCrime"", ""CrimeType"":""onFoot_murder"", ""Faction"":""The Buurian Protectorate"", ""Victim"":""Garth Cooley"", ""Bounty"":1000 }"
            };

            foreach ( var eventLine in eventLines )
            {
                events = JournalMonitor.ParseJournalEntry( eventLine );
                Assert.HasCount( 1, events );
                await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );
            }

            Assert.AreEqual( 12500, crimeMonitor.criminalrecord.Sum( r => r.fines ) );
            Assert.AreEqual( 2000, crimeMonitor.criminalrecord.Sum( r => r.bounties ) );

            line = @"{ ""timestamp"":""2026-06-01T04:01:39Z"", ""event"":""Resurrect"", ""Option"":""recover"", ""Cost"":14500, ""Bankrupt"":false }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            Assert.AreEqual( 0, crimeMonitor.criminalrecord.Sum( r => r.fines ) );
            Assert.AreEqual( 0, crimeMonitor.criminalrecord.Sum( r => r.bounties ) );
        }

        [TestMethod, DoNotParallelize]
        public void TestFactionRecordSerializesBaseClaimsForRollbackCompatibility()
        {
            var factionRecord = new FactionRecord( "Awarding Faction" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 100
            };
            factionRecord.UpdateFinalClaimValues( 425, 425 );

            var serialized = JsonConvert.SerializeObject( factionRecord );

            Assert.Contains( @"""claims"":100", serialized);
            Assert.IsFalse( serialized.Contains( @"""claims"":425", StringComparison.Ordinal ) );

            var deserialized = JsonConvert.DeserializeObject<FactionRecord>( serialized );

            Assert.IsNotNull( deserialized );
            Assert.AreEqual( 100, deserialized.baseclaims );
            Assert.AreEqual( 100, deserialized.claims );
            Assert.AreEqual( 0, deserialized.bountyclaims );
        }

        [TestMethod, DoNotParallelize]
        public void TestFactionRecordFinalClaimsSetterCreatesDiscrepancyWithoutChangingBaseClaims()
        {
            var factionRecord = new FactionRecord( "Awarding Faction" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 100
            };
            factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T10:00:00Z" ), true,
                Crime.None, "HIP 37722", 100 )
            {
                claimtype = FactionReport.BountyClaimType
            } );
            factionRecord.UpdateFinalClaimValues( 425, 425 );

            factionRecord.claims = 500;

            Assert.AreEqual( 100, factionRecord.baseclaims );
            Assert.AreEqual( 500, factionRecord.claims );
            var discrepancy = factionRecord.factionReports.SingleOrDefault( r => r.crimeDef == Crime.Claim );
            Assert.IsNotNull( discrepancy );
            Assert.AreEqual( 75, discrepancy.amount );
        }

        [TestMethod, DoNotParallelize]
        public void TestClaimsColumnIsEditableForDiscrepancyReports()
        {
            var xamlPath = FindRepositoryFile( Path.Combine( "CrimeMonitor", "ConfigurationWindow.xaml" ) );
            var document = XDocument.Load( xamlPath );
            var claimsColumn = document
                .Descendants()
                .Single( e => e.Name.LocalName == "DataGridNumericColumn" &&
                             (string)e.Attribute( "Header" ) == "{x:Static resx:CrimeMonitor.header_claims}" );

            Assert.AreNotEqual( "True", (string)claimsColumn.Attribute( "IsReadOnly" ) );
            Assert.Contains( "Mode=TwoWay" , (string)claimsColumn.Attribute( "Binding" ));
        }

        [TestMethod, DoNotParallelize]
        public async Task TestCrewWageDoesNotReducePendingDisplayedClaims()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var redeemedRecord = new FactionRecord( "Pereng Asli General Ltd" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 437994
            };
            redeemedRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-31T05:16:30Z" ), false,
                Crime.None, "Col 69 Sector SU-E c12-2", 437994 )
            {
                claimtype = FactionReport.BondClaimType
            } );
            crimeMonitor.criminalrecord.Add( redeemedRecord );

            line = @"{ ""timestamp"":""2026-05-31T05:16:30Z"", ""event"":""RedeemVoucher"", ""Type"":""CombatBond"", ""Amount"":437994, ""Faction"":""Pereng Asli General Ltd"" }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            Assert.IsTrue( crimeMonitor._handleBondRedeemedEvent( (BondRedeemedEvent)events[ 0 ] ) );

            line = @"{ ""timestamp"":""2026-05-31T05:16:30Z"", ""event"":""NpcCrewPaidWage"", ""NpcCrewName"":""Arden Petersen-Quinn"", ""NpcCrewId"":105187904, ""Amount"":43799 }";
            events = JournalMonitor.ParseJournalEntry( line, deferSyntheticEvents: false );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            var pendingRecord = new FactionRecord( "Future Voucher Faction" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 2000
            };
            pendingRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-31T05:20:00Z" ), false,
                Crime.None, "Col 69 Sector SU-E c12-2", 1000 )
            {
                claimtype = FactionReport.BondClaimType
            } );
            pendingRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-31T05:21:00Z" ), true,
                Crime.None, "Col 69 Sector SU-E c12-2", 1000 )
            {
                claimtype = FactionReport.BountyClaimType
            } );
            crimeMonitor.criminalrecord.Add( pendingRecord );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 2000, pendingRecord.baseclaims );
            Assert.AreEqual( 3850L, variables[ "claims" ].Item2 );
            Assert.AreEqual( 1000, pendingRecord.basebountyclaims );
            Assert.AreEqual( 1000, pendingRecord.bountyclaims );
        }

        [TestMethod, DoNotParallelize]
        public void TestSharedCombatBondJournalRewardsApplyDisplayedBondMultiplier()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var rewards = new[]
            {
                37352, 47977, 39967, 22025, 63107, 48684, 77324, 26833, 37352, 31359,
                29967, 31693, 24967, 40457, 33335, 21659, 27473, 24967, 32732, 39840
            };
            var factionRecord = new FactionRecord( "Children of Raxxla" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = rewards.Sum()
            };
            foreach ( var reward in rewards )
            {
                factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-31T23:10:27Z" ), false,
                    Crime.None, "Col 69 Sector TP-E c12-4", reward )
                {
                    claimtype = FactionReport.BondClaimType
                } );
            }
            crimeMonitor.criminalrecord.Add( factionRecord );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();

            Assert.AreEqual( 739070, factionRecord.baseclaims );
            Assert.AreEqual( 2106347L, factionRecord.claims );
            Assert.AreEqual( 2106347L, variables[ "claims" ].Item2 );
        }

        [TestMethod, DoNotParallelize]
        public void TestOnFootCombatBondJournalRewardsApplyOnFootBondMultiplier()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var rewards = new[]
            {
                2278, 2278, 3189, 2911, 2278, 3189, 2079, 1896, 4172, 2911,
                4172, 3189, 3189, 2278, 3189, 2278, 3189, 2278, 2980, 2980,
                2278, 2655, 2655, 1896, 2655, 2079, 2980, 3189, 2655, 2727,
                2980, 3189, 2911, 2980, 2911, 2079, 2278, 3818, 4172, 2278,
                2278
            };
            var factionRecord = new FactionRecord( "Pereng Asli General Ltd" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = rewards.Sum()
            };
            foreach ( var reward in rewards )
            {
                factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-30T21:07:17Z" ), false,
                    Crime.None, "Col 69 Sector TP-E c12-4", reward )
                {
                    claimtype = FactionReport.BondClaimType,
                    claimvehicle = Constants.VEHICLE_LEGS
                } );
            }
            crimeMonitor.criminalrecord.Add( factionRecord );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();

            Assert.AreEqual( 114546, factionRecord.baseclaims );
            Assert.AreEqual( 486660L, factionRecord.claims );
            Assert.AreEqual( 486660L, variables[ "claims" ].Item2 );
        }

        [TestMethod, DoNotParallelize]
        public void TestInShipCombatBondJournalRewardsDoNotApplyOnFootBondMultiplier()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var rewards = new[]
            {
                50987, 32238, 38018, 44840, 85989, 32352, 51881, 31060, 37352, 27473,
                39967, 33684, 37473, 27392, 39840, 46693, 39070, 21833, 57977, 32352,
                33335, 46659, 31833, 42352
            };
            var factionRecord = new FactionRecord( "Children of Raxxla" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = rewards.Sum()
            };
            foreach ( var reward in rewards )
            {
                factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-06-02T06:10:45Z" ), false,
                    Crime.None, "Col 69 Sector TP-E c12-4", reward )
                {
                    claimtype = FactionReport.BondClaimType,
                    claimvehicle = Constants.VEHICLE_SHIP
                } );
            }
            crimeMonitor.criminalrecord.Add( factionRecord );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();

            Assert.AreEqual( 962650, factionRecord.baseclaims );
            Assert.AreEqual( 2743551L, factionRecord.claims );
            Assert.AreEqual( 2743551L, variables[ "claims" ].Item2 );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestSettlementApproachDoesNotChangeInShipCombatBondEstimate()
        {
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );
            var originalVehicle = EDDI.Instance.Vehicle;
            try
            {
                EDDI.Instance.Vehicle = Constants.VEHICLE_SHIP;

                line = @"{ ""timestamp"":""2026-06-02T06:10:00Z"", ""event"":""ApproachSettlement"", ""Name"":""Baade Prospect"", ""MarketID"":3702110464, ""SystemAddress"":670256236121, ""Latitude"":1.0, ""Longitude"":2.0 }";
                events = JournalMonitor.ParseJournalEntry( line );
                Assert.HasCount( 1, events );
                await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

                line = @"{ ""timestamp"":""2026-06-02T06:10:45Z"", ""event"":""FactionKillBond"", ""Reward"":1000, ""AwardingFaction"":""Settlement Context Only"", ""VictimFaction"":""Target Faction"" }";
                events = JournalMonitor.ParseJournalEntry( line );
                Assert.HasCount( 1, events );
                await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

                var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
                record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Settlement Context Only" );

                Assert.IsNotNull( record );
                Assert.AreEqual( 1000, record.baseclaims );
                Assert.AreEqual( 2850L, record.claims );
                Assert.AreEqual( 2850L, variables[ "claims" ].Item2 );
            }
            finally
            {
                EDDI.Instance.Vehicle = originalVehicle;
            }
        }

        [TestMethod, DoNotParallelize]
        public void TestPowerplayBountyVoucherBonusTables()
        {
            Assert.IsTrue( PowerplayBountyVoucherBonus.TryGetBonus( Power.ALavignyDuval, 5, out var aldBonus ) );
            Assert.AreEqual( 0.10M, aldBonus );
            Assert.IsTrue( PowerplayBountyVoucherBonus.TryGetBonus( Power.JeromeArcher, 86, out var archerBonus ) );
            Assert.AreEqual( 0.90M, archerBonus );
            Assert.IsTrue( PowerplayBountyVoucherBonus.TryGetBonus( Power.ALavignyDuval, 100, out var aldMaxBonus ) );
            Assert.AreEqual( 1.00M, aldMaxBonus );
            Assert.IsTrue( PowerplayBountyVoucherBonus.TryGetBonus( Power.DentonPatreus, 42, out var patreusBonus ) );
            Assert.AreEqual( 0.35M, patreusBonus );
            Assert.IsTrue( PowerplayBountyVoucherBonus.TryGetBonus( Power.YuriGrom, 43, out var gromBonus ) );
            Assert.AreEqual( 0.10M, gromBonus );
            Assert.IsTrue( PowerplayBountyVoucherBonus.TryGetBonus( Power.YuriGrom, 48, out var gromRank48Bonus ) );
            Assert.AreEqual( 0.13M, gromRank48Bonus );
            Assert.IsFalse( PowerplayBountyVoucherBonus.TryGetBonus( Power.AislingDuval, 100, out _ ) );
            Assert.IsFalse( PowerplayBountyVoucherBonus.TryGetBonus( Power.ArchonDelaine, 100, out _ ) );
        }

        [TestMethod, DoNotParallelize]
        public void TestPowerplayBountiesAgainstCommanderReductionTables()
        {
            Assert.IsTrue( PowerplayBountyReduction.TryGetReduction( Power.ArchonDelaine, 5, out var rank5Reduction ) );
            Assert.AreEqual( 0.10M, rank5Reduction );
            Assert.IsTrue( PowerplayBountyReduction.TryGetReduction( Power.ArchonDelaine, 48, out var rank48Reduction ) );
            Assert.AreEqual( 0.50M, rank48Reduction );
            Assert.IsTrue( PowerplayBountyReduction.TryGetReduction( Power.ArchonDelaine, 86, out var rank86Reduction ) );
            Assert.AreEqual( 0.90M, rank86Reduction );
            Assert.IsTrue( PowerplayBountyReduction.TryGetReduction( Power.ArchonDelaine, 100, out var rank100Reduction ) );
            Assert.AreEqual( 1.00M, rank100Reduction );
            Assert.IsFalse( PowerplayBountyReduction.TryGetReduction( Power.YuriGrom, 100, out _ ) );
        }

        [TestMethod, DoNotParallelize]
        public void TestPowerplayEventStoresJournalRankExactly()
        {
            line = @"{ ""timestamp"":""2026-05-26T04:42:29Z"", ""event"":""Powerplay"", ""Power"":""Yuri Grom"", ""Rank"":43, ""Merits"":322584, ""TimePledged"":49113620 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            var powerplayEvent = (PowerplayEvent)events[ 0 ];

            Assert.AreEqual( Power.YuriGrom, powerplayEvent.Power );
            Assert.AreEqual( 43, powerplayEvent.rank );
            Assert.AreEqual( 322584, powerplayEvent.merits );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestPowerplayBountyVoucherEstimateActiveInPledgedPowerTerritory()
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            commanderConfig.Power = Power.YuriGrom;
            commanderConfig.powerRank = 43;
            commanderConfig.powerMerits = 322584;
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            var factionRecord = new FactionRecord( "LOST Industrial Technologies" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 150000
            };
            factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T14:10:00Z" ), true,
                Crime.None, "HIP 37722", 100000 )
            {
                claimtype = FactionReport.BountyClaimType
            } );
            factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T14:11:00Z" ), false,
                Crime.None, "HIP 37722", 50000 )
            {
                claimtype = FactionReport.BondClaimType
            } );
            crimeMonitor.criminalrecord.Add( factionRecord );

            line = @"{ ""timestamp"":""2026-05-26T14:12:00Z"", ""event"":""FSDJump"", ""StarSystem"":""Grom Territory"", ""SystemAddress"":670256236121, ""StarPos"":[0,0,0], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_None;"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemGovernment"":""$government_None;"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""Population"":0, ""Body"":""Grom Territory"", ""BodyID"":0, ""BodyType"":""Star"", ""ControllingPower"":""Yuri Grom"", ""Powers"":[ ""Yuri Grom"" ], ""PowerplayState"":""Stronghold"", ""JumpDist"":1, ""FuelUsed"":1, ""FuelLevel"":1 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 0.10M, variables[ "powerplaybountybonus" ].Item2 );
            Assert.AreEqual( 252500L, variables[ "claims" ].Item2 );
            Assert.AreEqual( 110000L, factionRecord.bountyclaims );
            Assert.AreEqual( 252500L, factionRecord.claims );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestPowerplayBountiesAgainstCommanderReductionActiveInPledgedPowerTerritory()
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            commanderConfig.Power = Power.ArchonDelaine;
            commanderConfig.powerRank = 48;
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            var factionRecord = new FactionRecord( "Delaine Territory Faction" )
            {
                Allegiance = Superpower.Independent,
                bounties = 100000
            };
            factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T14:10:00Z" ), true,
                Crime.Murder, "Harma", 100000 ) );
            crimeMonitor.criminalrecord.Add( factionRecord );

            line = @"{ ""timestamp"":""2026-05-26T14:12:00Z"", ""event"":""FSDJump"", ""StarSystem"":""Delaine Territory"", ""SystemAddress"":670256236121, ""StarPos"":[0,0,0], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_None;"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemGovernment"":""$government_None;"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""Population"":0, ""Body"":""Delaine Territory"", ""BodyID"":0, ""BodyType"":""Star"", ""ControllingPower"":""Archon Delaine"", ""Powers"":[ ""Archon Delaine"" ], ""PowerplayState"":""Stronghold"", ""JumpDist"":1, ""FuelUsed"":1, ""FuelLevel"":1 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 0.50M, variables[ "powerplaycrimereduction" ].Item2 );
            Assert.AreEqual( 100000L, variables[ "bounties" ].Item2 );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestDelaineReductionContextTrustsJournalBountyAmount()
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            commanderConfig.Power = Power.ArchonDelaine;
            commanderConfig.powerRank = 48;
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            line = @"{ ""timestamp"":""2026-05-26T14:12:00Z"", ""event"":""FSDJump"", ""StarSystem"":""Delaine Territory"", ""SystemAddress"":670256236121, ""StarPos"":[0,0,0], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_None;"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemGovernment"":""$government_None;"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""Population"":0, ""Body"":""Delaine Territory"", ""BodyID"":0, ""BodyType"":""Star"", ""ControllingPower"":""Archon Delaine"", ""Powers"":[ ""Archon Delaine"" ], ""PowerplayState"":""Stronghold"", ""JumpDist"":1, ""FuelUsed"":1, ""FuelLevel"":1 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            line = @"{ ""timestamp"":""2026-05-26T14:13:00Z"", ""event"":""CommitCrime"", ""CrimeType"":""assault"", ""Faction"":""Delaine Territory Faction"", ""Victim"":""Target"", ""Bounty"":1000 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Delaine Territory Faction" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 1000, record.bounties );
            report = record.bountiesIncurred.SingleOrDefault();
            Assert.IsNotNull( report );
            Assert.AreEqual( 1000, report.amount );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 1000L, variables[ "bounties" ].Item2 );
            Assert.AreEqual( 1000, record.basebounties );

            line = @"{ ""timestamp"":""2026-05-26T14:14:00Z"", ""event"":""PayBounties"", ""Amount"":1000, ""Faction"":""Delaine Territory Faction"", ""ShipID"":1, ""BrokerPercentage"":0.000000 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            Assert.IsNull( crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Delaine Territory Faction" ) );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestDelaineReductionContextTrustsJournalFineAmount()
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            commanderConfig.Power = Power.ArchonDelaine;
            commanderConfig.powerRank = 48;
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            line = @"{ ""timestamp"":""2026-05-26T14:12:00Z"", ""event"":""FSDJump"", ""StarSystem"":""Delaine Territory"", ""SystemAddress"":670256236121, ""StarPos"":[0,0,0], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_None;"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemGovernment"":""$government_None;"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""Population"":0, ""Body"":""Delaine Territory"", ""BodyID"":0, ""BodyType"":""Star"", ""ControllingPower"":""Archon Delaine"", ""Powers"":[ ""Archon Delaine"" ], ""PowerplayState"":""Stronghold"", ""JumpDist"":1, ""FuelUsed"":1, ""FuelLevel"":1 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            line = @"{ ""timestamp"":""2026-05-26T14:13:00Z"", ""event"":""CommitCrime"", ""CrimeType"":""dockingMinorTresspass"", ""Faction"":""Delaine Territory Faction"", ""Fine"":1000 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            record = crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "Delaine Territory Faction" );
            Assert.IsNotNull( record );
            Assert.AreEqual( 1000, record.fines );
            report = record.finesIncurred.SingleOrDefault();
            Assert.IsNotNull( report );
            Assert.AreEqual( 1000, report.amount );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.AreEqual( 1000L, variables[ "fines" ].Item2 );
            Assert.AreEqual( 1000, record.basefines );
        }

        [TestMethod, DoNotParallelize]
        public void TestJournalAmountJsonDoesNotChangeBaseCrimeValues()
        {
            var json = @"{
                ""faction"": ""Ignored Journal Amount"",
                ""bounties"": 500,
                ""factionReports"": [ {
                    ""timestamp"": ""2026-05-26T14:13:00Z"",
                    ""bounty"": true,
                    ""crimeEDName"": ""assault"",
                    ""system"": ""Delaine Territory"",
                    ""amount"": 500,
                    ""journalamount"": 1000
                } ]
            }";

            record = JsonConvert.DeserializeObject<FactionRecord>( json );

            Assert.IsNotNull( record );
            Assert.AreEqual( 500, record.bounties );
            Assert.AreEqual( 500, record.basebounties );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestPowerplayBountyVoucherEstimateInactiveOutsidePledgedPowerTerritory()
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            commanderConfig.Power = Power.YuriGrom;
            commanderConfig.powerRank = 43;
            commanderConfig.powerMerits = 322584;
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            var factionRecord = new FactionRecord( "LOST Industrial Technologies" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 100000
            };
            factionRecord.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T14:10:00Z" ), true,
                Crime.None, "HIP 37722", 100000 )
            {
                claimtype = FactionReport.BountyClaimType
            } );
            crimeMonitor.criminalrecord.Add( factionRecord );

            line = @"{ ""timestamp"":""2026-05-26T14:12:00Z"", ""event"":""FSDJump"", ""StarSystem"":""Imperial Territory"", ""SystemAddress"":670256236122, ""StarPos"":[0,0,0], ""SystemAllegiance"":""Empire"", ""SystemEconomy"":""$economy_None;"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemGovernment"":""$government_None;"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""Population"":0, ""Body"":""Imperial Territory"", ""BodyID"":0, ""BodyType"":""Star"", ""ControllingPower"":""Arissa Lavigny-Duval"", ""Powers"":[ ""Arissa Lavigny-Duval"" ], ""PowerplayState"":""Stronghold"", ""JumpDist"":1, ""FuelUsed"":1, ""FuelLevel"":1 }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );
            await crimeMonitor.PreHandleAsync( events[ 0 ] ).ConfigureAwait( false );

            var variables = crimeMonitor.GetVariableValues().ToRuntimeValueDictionary();
            Assert.IsNull( variables[ "powerplaybountybonus" ].Item2 );
            Assert.AreEqual( 100000L, variables[ "claims" ].Item2 );
            Assert.AreEqual( 100000, factionRecord.claims );
            Assert.AreEqual( 100000, factionRecord.bountyclaims );
        }

        [TestMethod, DoNotParallelize]
        public void TestPowerplayEstimateDoesNotAffectVoucherRedemption()
        {
            var commanderConfig = ConfigService.Instance.commanderConfiguration;
            commanderConfig.Power = Power.YuriGrom;
            commanderConfig.powerRank = 43;
            crimeMonitor.readRecord( new CrimeMonitorConfiguration() );

            var record = new FactionRecord( "LOST Industrial Technologies" )
            {
                Allegiance = Superpower.Independent,
                baseclaims = 100000
            };
            record.factionReports.Add( new FactionReport( DateTime.Parse( "2026-05-26T14:10:00Z" ), true,
                Crime.None, "HIP 37722", 100000 )
            {
                claimtype = FactionReport.BountyClaimType
            } );
            crimeMonitor.criminalrecord.Add( record );

            line = @"{ ""timestamp"":""2026-05-26T14:13:31Z"", ""event"":""RedeemVoucher"", ""Type"":""bounty"", ""Amount"":100000, ""Factions"":[ { ""Faction"":""LOST Industrial Technologies"", ""Amount"":100000 } ] }";
            events = JournalMonitor.ParseJournalEntry( line );
            Assert.HasCount( 1, events );

            Assert.IsTrue( crimeMonitor._handleBountyRedeemedEvent( (BountyRedeemedEvent)events[ 0 ] ) );

            Assert.IsNull( crimeMonitor.criminalrecord.FirstOrDefault( r => r.faction == "LOST Industrial Technologies" ) );
        }

        private static string FindRepositoryFile ( string relativePath )
        {
            var directory = new DirectoryInfo( AppContext.BaseDirectory );
            while ( directory != null )
            {
                var path = Path.Combine( directory.FullName, relativePath );
                if ( File.Exists( path ) )
                {
                    return path;
                }

                directory = directory.Parent;
            }

            throw new FileNotFoundException( $"Could not find {relativePath} from {AppContext.BaseDirectory}." );
        }

        // Test that we're able to detect and correct for simple scenarios where a bounty has been converted to an interstellar bounty
        [TestMethod, DoNotParallelize]
        public async Task TestCrimeInterstellarFactorsScenario()
        {
            var line1 = @"{ ""timestamp"":""2022-01-15T18:37:38Z"", ""event"":""CommitCrime"", ""CrimeType"":""assault"", ""Faction"":""Radio Sidewinder Crew"", ""Victim"":""Jim Grady"", ""Bounty"":100 }";
            var line2 = @"{ ""timestamp"":""2022-01-15T18:41:31Z"", ""event"":""PayBounties"", ""Amount"":125, ""Faction"":""$faction_Independent;"", ""Faction_Localised"":""Independent"", ""ShipID"":38, ""BrokerPercentage"":25.000000 }";

            // Save original data
            var data = ConfigService.Instance.crimeMonitorConfiguration;

            // Load a known empty state
            var config = new CrimeMonitorConfiguration();
            crimeMonitor.readRecord(config);

            EDDI.Instance.DataProvider = CreateTestDataProvider();
            FakeSpanshHttpClient.Expect( @"systems/search?json={""filters"":{""minor_faction_presences"":[{""name"":{""value"":[""Radio Sidewinder Crew""]}}]},""size"":500,""page"":0}", Encoding.UTF8.GetString( Properties.Resources.SpanshQueryFactionRadioSidewinderCrew ) );
            FakeSpanshHttpClient.Expect( "systems/field_values/system_names?q=Tachmetae", @"{""min_max"":[{""id64"":2869977949641,""name"":""Tachmetae"",""x"":-0.59375,""y"":60.6875,""z"":84.71875}],""values"":[""Tachmetae""]}" );
            FakeSpanshHttpClient.Expect( "dump/2869977949641", Encoding.UTF8.GetString( Properties.Resources.SpanshStarSystemDumpTachmetae ) );

            // Set a bounty with `Radio Sidewinder Crew`
            events = JournalMonitor.ParseJournalEntry(line1);
            Assert.HasCount( 1, events );
            await crimeMonitor._handleBountyIncurredEventAsync( (BountyIncurredEvent)events[ 0 ] ).ConfigureAwait( false );
            Assert.HasCount( 1, crimeMonitor.criminalrecord );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Radio Sidewinder Crew");
            Assert.IsNotNull(record);
            Assert.ContainsSingle( r => r.bounty && r.crimeDef != Crime.None, record.factionReports );
            Assert.AreEqual(100, record.bountiesIncurred.Sum(r => r.amount));

            // Test whether we're able to identify and remove the bounty after it has been converted to an interstellar bounty
            events = JournalMonitor.ParseJournalEntry(line2);
            Assert.HasCount( 1, events );
            crimeMonitor._handleBountyPaidEvent( (BountyPaidEvent)events[ 0 ] );
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Radio Sidewinder Crew");
            Assert.IsNull(record);
            Assert.HasCount( 0, crimeMonitor.criminalrecord );

            // Restore original data
            ConfigService.Instance.crimeMonitorConfiguration = data;
        }
    }
}
