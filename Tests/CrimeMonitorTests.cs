using Eddi;
using EddiCrimeMonitor;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class CrimeMonitorTests : TestBase
    {
        CrimeMonitor crimeMonitor = new CrimeMonitor();
        FactionRecord record;
        FactionReport report;
        string line;
        List<Event> events;

        string crimeConfigJson = @"{
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
        private void StartTestCrimeMonitor()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestCrimeConfig()
        {
            // Save original data
            CrimeMonitorConfiguration data = CrimeMonitorConfiguration.FromFile();

            CrimeMonitorConfiguration config = CrimeMonitorConfiguration.FromJsonString(crimeConfigJson);
            Assert.AreEqual(3, config.criminalrecord.Count);
            Assert.AreEqual(275915, config.claims);
            Assert.AreEqual(400, config.fines);

            record = config.criminalrecord.ToList().FirstOrDefault(r => r.faction == "Calennero State Industries");
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
            Assert.AreEqual(Crime.FromEDName("dockingMinorTresspass"), report.crimeDef);
            Assert.AreEqual("Fabian City", report.station);

            // Restore original data
            data.ToFile();
        }

        [TestMethod]
        public void TestCrimeEventsScenario()
        {
            // Save original data
            CrimeMonitorConfiguration data = CrimeMonitorConfiguration.FromFile();

            var privateObject = new PrivateObject(crimeMonitor);
            CrimeMonitorConfiguration config = CrimeMonitorConfiguration.FromJsonString(crimeConfigJson);
            crimeMonitor.readRecord(config);

            // Bond Awarded Event
            line = "{ \"timestamp\":\"2019-04-22T11:51:30Z\", \"event\":\"FactionKillBond\", \"Reward\":32473, \"AwardingFaction\":\"Constitution Party of Aerial\", \"VictimFaction\":\"Ankou Blue Federal Holdings\" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleBondAwardedEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.AreEqual(3, record.factionReports.Count);
            Assert.AreEqual(94492, record.bondsAmount);

            // Bounty Awarded Event
            line = "{ \"timestamp\":\"2019-04-22T03:13:36Z\", \"event\":\"Bounty\", \"Rewards\":[ { \"Faction\":\"Calennero State Industries\", \"Reward\":22265 } ], \"Target\":\"adder\", \"TotalReward\":22265, \"VictimFaction\":\"Natural Amemakarna Movement\" }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleBountyAwardedEvent", new object[] { events[0], true });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            record.factionReports.FirstOrDefault(r => r.amount == 22265).shipId = 10;
            Assert.AreEqual(2, record.factionReports.Where(r => r.bounty && r.crimeDef == Crime.None).Count());
            Assert.AreEqual(127433, record.bountiesAmount);

            // Fine Incurred Event
            line = "{ \"timestamp\":\"2019-04-22T03:21:46Z\", \"event\":\"CommitCrime\", \"CrimeType\":\"dockingMinorTresspass\", \"Faction\":\"Constitution Party of Aerial\", \"Fine\":400 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleFineIncurredEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            record.factionReports.FirstOrDefault(r => !r.bounty && r.crimeDef != Crime.None).shipId = 10;
            Assert.AreEqual(1, record.factionReports.Where(r => !r.bounty && r.crimeDef != Crime.None).Count());
            Assert.AreEqual(400, record.finesIncurred.Sum(r => r.amount));

            // Bounty Incurred Event
            line = "{ \"timestamp\":\"2019-04-13T03:58:29Z\", \"event\":\"CommitCrime\", \"CrimeType\":\"assault\", \"Faction\":\"Calennero State Industries\", \"Victim\":\"Christofer\", \"Bounty\":400 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleBountyIncurredEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            record.factionReports.FirstOrDefault(r => r.bounty && r.crimeDef != Crime.None).shipId = 10;
            Assert.AreEqual(1, record.factionReports.Where(r => r.bounty && r.crimeDef != Crime.None).Count());
            Assert.AreEqual(400, record.bountiesIncurred.Sum(r => r.amount));

            // Redeem Bond Event
            line = "{ \"timestamp\":\"2019-04-09T10:31:31Z\", \"event\":\"RedeemVoucher\", \"Type\":\"CombatBond\", \"Amount\":94492, \"Factions\":[ { \"Faction\":\"Constitution Party of Aerial\", \"Amount\":94492 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleBondRedeemedEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.AreEqual(0, record.factionReports.Where(r => !r.bounty && r.crimeDef == Crime.None).Count());

            // Redeem Bounty Event - Multiple
            line = "{ \"timestamp\":\"2019-04-09T10:31:31Z\", \"event\":\"RedeemVoucher\", \"Type\":\"bounty\", \"Amount\":213896, \"Factions\":[ { \"Faction\":\"Calennero State Industries\", \"Amount\":105168 }, { \"Faction\":\"HIP 20277 Inc\", \"Amount\":108728 } ] }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleBountyRedeemedEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNotNull(record);
            Assert.AreEqual(0, record.factionReports.Where(r => r.bounty && r.crimeDef == Crime.None).Count());
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "HIP 20277 Inc");
            Assert.IsNull(record);

            // Fine Paid Event
            line = "{ \"timestamp\":\"2019-04-09T15:12:10Z\", \"event\":\"PayFines\", \"Amount\":800, \"AllFines\":true, \"ShipID\":10 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleFinePaidEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.AreEqual(0, record.factionReports.Where(r => !r.bounty && r.crimeDef != Crime.None).Count());
            Assert.AreEqual(0, record.finesIncurred.Sum(r => r.amount));
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Constitution Party of Aerial");
            Assert.IsNull(record);

            // Bounty Paid Event
            line = "{ \"timestamp\":\"2019-04-14T04:43:05Z\", \"event\":\"PayBounties\", \"Amount\":400, \"Faction\":\"$faction_Empire;\", \"Faction_Localised\":\"Empire\", \"ShipID\":10, \"BrokerPercentage\":25.000000 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleBountyPaidEvent", new object[] { events[0] });
            record = crimeMonitor.criminalrecord.FirstOrDefault(r => r.faction == "Calennero State Industries");
            Assert.IsNull(record);

            // Restore original data
            data.ToFile();
        }

        [TestMethod]
        public void TestCrimeShipTargeted()
        {
            var privateObject = new PrivateObject(crimeMonitor);
            crimeMonitor.targetSystem = EDDI.Instance?.CurrentStarSystem?.systemname;
            line = "{ \"timestamp\":\"2019-04-24T00:13:35Z\", \"event\":\"ShipTargeted\", \"TargetLocked\":true, \"Ship\":\"federation_corvette\", \"Ship_Localised\":\"Federal Corvette\", \"ScanStage\":3, \"PilotName\":\"$npc_name_decorate:#name=Kurt Pettersen;\", \"PilotName_Localised\":\"Kurt Pettersen\", \"PilotRank\":\"Deadly\", \"ShieldHealth\":100.000000, \"HullHealth\":100.000000, \"Faction\":\"Calennero Crew\", \"LegalStatus\":\"Wanted\", \"Bounty\":295785 }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("handleShipTargetedEvent", new object[] { events[0] });
            Assert.IsNotNull(crimeMonitor.shipTargets);
            Assert.AreEqual(1, crimeMonitor.shipTargets.Count);
            Target target = crimeMonitor.shipTargets.FirstOrDefault(t => t.name == "Kurt Pettersen");
            Assert.AreEqual(CombatRating.FromEDName("Deadly"), target.CombatRank);
            Assert.AreEqual("Calennero Crew", target.faction);
            Assert.AreEqual(Superpower.Independent, target.Allegiance);
            Assert.AreEqual(295785, target.bounty);

            line = "{ \"timestamp\":\"2019-04-24T00:44:32Z\", \"event\":\"FSDJump\", \"StarSystem\":\"HIP 20277\", \"SystemAddress\":84053791442, \"StarPos\":[106.43750,-95.68750,-0.18750], \"SystemAllegiance\":\"Empire\", \"SystemEconomy\":\"$economy_Industrial;\", \"SystemEconomy_Localised\":\"Industrial\", \"SystemSecondEconomy\":\"$economy_Extraction;\", \"SystemSecondEconomy_Localised\":\"Extraction\", \"SystemGovernment\":\"$government_Corporate;\", \"SystemGovernment_Localised\":\"Corporate\", \"SystemSecurity\":\"$SYSTEM_SECURITY_high;\", \"SystemSecurity_Localised\":\"High Security\", \"Population\":11247202, \"Body\":\"HIP 20277\", \"BodyID\":0, \"BodyType\":\"Star\", \"JumpDist\":7.473, \"FuelUsed\":1.140420, \"FuelLevel\":61.122398, \"SystemFaction\":{ \"Name\":\"Calennero State Industries\", \"FactionState\":\"Boom\" } }";
            events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            privateObject.Invoke("_handleJumpedEvent", new object[] { events[0] });
            Assert.AreEqual(0, crimeMonitor.shipTargets.Count);
        }

        [TestCleanup]
        private void StopTestCrimeMonitor()
        {
        }
    }
}
