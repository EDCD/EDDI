using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiEvents;
using EddiJournalMonitor;
using EddiVoiceAttackResponder;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    public class MockVAProxy
    {
        [ UsedImplicitly ] public List<KeyValuePair<string, string>> vaLog = new List<KeyValuePair<string, string>>();

        private readonly Dictionary<string, string> vaStrings = new Dictionary<string, string>();
        private readonly Dictionary<string, decimal?> vaDecimals = new Dictionary<string, decimal?>();
        private readonly Dictionary<string, int?> vaIntegers = new Dictionary<string, int?>();
        private readonly Dictionary<string, short?> vaShorts = new Dictionary<string, short?>();
        private readonly Dictionary<string, bool?> vaBooleans = new Dictionary<string, bool?>();
        private readonly Dictionary<string, DateTime?> vaDates = new Dictionary<string, DateTime?>();

        [ UsedImplicitly ]
        public void WriteToLog ( string msg, string color = null )
        {
            vaLog.Add( new KeyValuePair<string, string>( msg, color ) );
        }

        [ UsedImplicitly ]
        public string GetText ( string varName )
        {
            return vaStrings.TryGetValue( varName, out var s ) ? s : null;
        }

        [ UsedImplicitly ]
        public void SetText ( string varName, object value )
        {
            vaStrings[ varName ] = value?.ToString();
        }

        [ UsedImplicitly ]
        public int? GetInt ( string varName )
        {
            return vaIntegers.TryGetValue(varName, out var i) ? i : null;
        }

        [ UsedImplicitly ]
        public void SetInt ( string varName, int? value )
        {
            vaIntegers[ varName ] = value;
        }

        [ UsedImplicitly ]
        public short? GetSmallInt ( string varName )
        {
            return vaShorts.TryGetValue( varName, out var sh ) ? sh : null;
        }

        [ UsedImplicitly ]
        public void SetSmallInt ( string varName, short? value )
        {
            vaShorts[ varName ] = value;
        }

        [ UsedImplicitly ]
        public bool? GetBoolean ( string varName )
        {
            return vaBooleans.TryGetValue( varName, out var b ) ? b : null;
        }

        [ UsedImplicitly ]
        public void SetBoolean ( string varName, bool? value )
        {
            vaBooleans[ varName ] = value;
        }

        [ UsedImplicitly ]
        public decimal? GetDecimal ( string varName )
        {
            return vaDecimals.TryGetValue( varName, out var d ) ? d : null;
        }

        [ UsedImplicitly ]
        public void SetDecimal ( string varName, decimal? value )
        {
            vaDecimals[ varName ] = value;
        }

        [ UsedImplicitly ]
        public DateTime? GetDate ( string varName )
        {
            return vaDates.TryGetValue( varName, out var dt ) ? dt : null;
        }

        [ UsedImplicitly ]
        public void SetDate ( string varName, DateTime? value )
        {
            vaDates[ varName ] = value;
        }

        [ UsedImplicitly ]
        public bool ContainsKey ( string varName )
        {
            return vaStrings.ContainsKey( varName ) || 
                   vaDecimals.ContainsKey( varName ) ||
                   vaIntegers.ContainsKey( varName ) || 
                   vaShorts.ContainsKey( varName ) ||
                   vaBooleans.ContainsKey( varName ) || 
                   vaDates.ContainsKey(varName);
        }
    }

    [TestClass]
    public class VoiceAttackPluginTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [DataTestMethod]
        [DataRow( "1", "1", "1", "1", "true" )] // Value is a string. Numeric results are set to 1 and bool is true.
        [DataRow( "2", "123.45", "123", "123", "true" )] // Value is decimal. Integer and short values are rounded. Value exists so bool is true.
        [DataRow( "3", "1234567.89", "1234568", null, "true" )] // Value is a decimal. Integer value is rounded, value is too large for short and thus is null. Value exists so bool is true.
        [DataRow( "4", "12345", "12345", "12345", "true" )] // Value is a short and qualifies for all numeric types. Value exists so bool is true.
        [DataRow( "5", "1", "1", "1", "true" )] // Value is boolean, numeric values are set to 1.
        [DataRow( "6", "1", "1", "1", "true" )] // Value is boolean, numeric values are set to 1.
        [DataRow( "7", null, null, null, null )] // Value is null, no values.
        [DataRow( "8", "0", "0", "0", "false" )] // Value is zero in all numeric types and false as a boolean.
        [DataRow( "9", "0", "0", "0", "false" )] // Value is zero in all numeric types and false as a boolean.
        [DataRow( "10", "0", "0", "0", "false" )] // Value is zero in all numeric types and false as a boolean.
        public void TestSetState ( string varName, string decimalResult, string integerResult, string shortresult, string booleanResult )
        {
            // Define values using dynamic types for each varName. Expected returns are defined above.
            var dict = new Dictionary<string, object>
            {
                [ "1" ] = "test",
                [ "2" ] = "123.45",
                [ "3" ] = 1234567.89M,
                [ "4" ] = 12345,
                [ "5" ] = 1,
                [ "6" ] = true,
                [ "7" ] = null,
                [ "8" ] = 0,
                [ "9" ] = "false",
                [ "10" ] = string.Empty,
            };

            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;
            VoiceAttackVariables.setDictionaryValues( dict, "state", ref vaProxy );
            Assert.AreEqual( dict.FirstOrDefault(kv => kv.Key == varName).Value?
                .ToString(), mockVAProxy.GetText( "EDDI state " + varName ) );
            Assert.AreEqual( decimalResult is null 
                ? null 
                : (decimal?)decimal.Parse(decimalResult), mockVAProxy.GetDecimal( "EDDI state " + varName ) );
            Assert.AreEqual( integerResult is null 
                ? null 
                : (int?)int.Parse(integerResult), mockVAProxy.GetInt( "EDDI state " + varName ) );
            Assert.AreEqual( shortresult is null 
                ? null 
                : (short?)short.Parse(shortresult), mockVAProxy.GetSmallInt( "EDDI state " + varName ) );
            Assert.AreEqual( booleanResult is null 
                ? null 
                : (bool?)bool.Parse(booleanResult), mockVAProxy.GetBoolean( "EDDI state " + varName ) );
        }

        [TestMethod]
        public void TestVAExplorationDataSoldEvent()
        {
            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;
            string line = @"{ ""timestamp"":""2016-09-23T18:57:55Z"", ""event"":""SellExplorationData"", ""Systems"":[ ""Gamma Tucanae"", ""Rho Capricorni"", ""Dain"", ""Col 285 Sector BR-S b18-0"", ""LP 571-80"", ""Kawilocidi"", ""Irulachan"", ""Alrai Sector MC-M a7-0"", ""Col 285 Sector FX-Q b19-5"", ""Col 285 Sector EX-Q b19-7"", ""Alrai Sector FB-O a6-3"" ], ""Discovered"":[ ""Irulachan"" ], ""BaseValue"":63573, ""Bonus"":1445, ""TotalEarnings"":65018 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(ExplorationDataSoldEvent));
            var ev = events[0] as ExplorationDataSoldEvent;
            Assert.IsNotNull(ev);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = vars.AsVoiceAttackVariables("EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(15, vaVars.Count);
            Assert.AreEqual("Gamma Tucanae", mockVAProxy.GetText("EDDI exploration data sold systems 1"));
            Assert.AreEqual("Rho Capricorni", mockVAProxy.GetText("EDDI exploration data sold systems 2"));
            Assert.AreEqual("Dain", mockVAProxy.GetText("EDDI exploration data sold systems 3"));
            Assert.AreEqual("Col 285 Sector BR-S b18-0", mockVAProxy.GetText("EDDI exploration data sold systems 4"));
            Assert.AreEqual("LP 571-80", mockVAProxy.GetText("EDDI exploration data sold systems 5"));
            Assert.AreEqual("Kawilocidi", mockVAProxy.GetText("EDDI exploration data sold systems 6"));
            Assert.AreEqual("Irulachan", mockVAProxy.GetText("EDDI exploration data sold systems 7"));
            Assert.AreEqual("Alrai Sector MC-M a7-0", mockVAProxy.GetText("EDDI exploration data sold systems 8"));
            Assert.AreEqual("Col 285 Sector FX-Q b19-5", mockVAProxy.GetText("EDDI exploration data sold systems 9"));
            Assert.AreEqual("Col 285 Sector EX-Q b19-7", mockVAProxy.GetText("EDDI exploration data sold systems 10"));
            Assert.AreEqual("Alrai Sector FB-O a6-3", mockVAProxy.GetText("EDDI exploration data sold systems 11"));
            Assert.AreEqual(11, mockVAProxy.GetInt("EDDI exploration data sold systems"));
            Assert.AreEqual(63573M, mockVAProxy.GetDecimal( "EDDI exploration data sold reward" ) );
            Assert.AreEqual(1445M, mockVAProxy.GetDecimal("EDDI exploration data sold bonus"));
            Assert.AreEqual(65018M, mockVAProxy.GetDecimal("EDDI exploration data sold total"));
            foreach (var variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVADiscoveryScanEvent()
        {
            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;
            string line = @"{ ""timestamp"":""2019-10-26T02:15:49Z"", ""event"":""FSSDiscoveryScan"", ""Progress"":0.439435, ""BodyCount"":7, ""NonBodyCount"":3, ""SystemName"":""Outotz WO-A d1"", ""SystemAddress"":44870715523 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(DiscoveryScanEvent));
            DiscoveryScanEvent ev = events[0] as DiscoveryScanEvent;
            Assert.IsNotNull(ev);

            Assert.AreEqual(7, ev.totalbodies);
            Assert.AreEqual(3, ev.nonbodies);
            Assert.AreEqual(44, ev.progress);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = vars.AsVoiceAttackVariables("EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(2, vaVars.Count);
            Assert.AreEqual(7, mockVAProxy.GetInt( "EDDI discovery scan totalbodies"));
            Assert.AreEqual(3, mockVAProxy.GetInt("EDDI discovery scan nonbodies"));
            Assert.IsNull(mockVAProxy.GetInt("EDDI discovery scan progress"));
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVAAsteroidProspectedEvent()
        {
            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;
            string line = "{ \"timestamp\":\"2020-04-10T02:32:21Z\", \"event\":\"ProspectedAsteroid\", \"Materials\":[ { \"Name\":\"LowTemperatureDiamond\", \"Name_Localised\":\"Low Temperature Diamonds\", \"Proportion\":26.078022 }, { \"Name\":\"HydrogenPeroxide\", \"Name_Localised\":\"Hydrogen Peroxide\", \"Proportion\":10.189009 } ], \"MotherlodeMaterial\":\"Alexandrite\", \"Content\":\"$AsteroidMaterialContent_Low;\", \"Content_Localised\":\"Material Content: Low\", \"Remaining\":90.000000 }";
            List<Event> events = JournalMonitor.ParseJournalEntry(line);
            Assert.IsTrue(events.Count == 1);
            Assert.IsInstanceOfType(events[0], typeof(AsteroidProspectedEvent));
            AsteroidProspectedEvent ev = events[0] as AsteroidProspectedEvent;
            Assert.IsNotNull(ev);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = vars.AsVoiceAttackVariables("EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(8, vaVars.Count);
            Assert.AreEqual(90M, mockVAProxy.GetDecimal("EDDI asteroid prospected remaining"));
            Assert.AreEqual("Alexandrite", mockVAProxy.GetText("EDDI asteroid prospected motherlode"));
            Assert.AreEqual("Low Temperature Diamonds", mockVAProxy.GetText("EDDI asteroid prospected commodities 1 commodity"));
            Assert.AreEqual(26.078022M, mockVAProxy.GetDecimal("EDDI asteroid prospected commodities 1 percentage"));
            Assert.AreEqual("Hydrogen Peroxide", mockVAProxy.GetText("EDDI asteroid prospected commodities 2 commodity"));
            Assert.AreEqual(10.189009M, mockVAProxy.GetDecimal("EDDI asteroid prospected commodities 2 percentage"));
            Assert.AreEqual(2, mockVAProxy.GetInt("EDDI asteroid prospected commodities"));
            Assert.AreEqual("Low", mockVAProxy.GetText("EDDI asteroid prospected materialcontent"));
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVAShipFSDEvent()
        {
            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;
            // Test a generated variable name from overlapping strings.
            // The prefix "EDDI ship fsd" should be merged with the formatted child key "fsd status" to yield "EDDI ship fsd status".
            ShipFsdEvent ev = new ShipFsdEvent (DateTime.UtcNow, "ready");
            var vars = new MetaVariables(ev.GetType(), ev).Results;
            
            var vaVars = vars.AsVoiceAttackVariables("EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(2, vaVars.Count);
            Assert.AreEqual("ready", mockVAProxy.GetText("EDDI ship fsd status"));
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVACommodityEjectedEvent()
        {
            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;
            // Test a generated variable name from overlapping strings.
            // The prefix "EDDI ship fsd" should be merged with the formatted child key "fsd status" to yield "EDDI ship fsd status".
            CommodityEjectedEvent ev = new CommodityEjectedEvent(DateTime.UtcNow, CommodityDefinition.FromEDName("Water"), 5, null, true);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.IsNotNull(cottleVars);
            Assert.AreEqual(4, cottleVars.Count);
            Assert.AreEqual("Water", cottleVars.FirstOrDefault(k => k.key == "commodity")?.value);
            Assert.AreEqual(5, cottleVars.FirstOrDefault(k => k.key == "amount")?.value);
            Assert.IsNull(cottleVars.FirstOrDefault(k => k.key == "missionid")?.value);
            Assert.AreEqual(true, cottleVars.FirstOrDefault(k => k.key == "abandoned")?.value);

            var vaVars = vars.AsVoiceAttackVariables("EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(vaProxy); }
            Assert.AreEqual(4, vaVars.Count);
            Assert.AreEqual("Water", mockVAProxy.GetText("EDDI commodity ejected commodity"));
            Assert.AreEqual(5, mockVAProxy.GetInt("EDDI commodity ejected amount"));
            Assert.IsNull(mockVAProxy.GetDecimal("EDDI commodity ejected missionid"));
            Assert.AreEqual(true, mockVAProxy.GetBoolean("EDDI commodity ejected abandoned"));
            foreach (VoiceAttackVariable variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [ TestMethod ]
        public void TestVAShip ()
        {
            dynamic vaProxy = new MockVAProxy();
            var mockVAProxy = (MockVAProxy)vaProxy;

            // Read from our test item "shipMonitor.json"
            var configuration = new ShipMonitorConfiguration();
            try
            {
                configuration = DeserializeJsonResource<ShipMonitorConfiguration>( Resources.shipMonitor );
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Failed to read ship configuration", ex );
                Assert.Fail();
            }

            var krait = configuration.shipyard.FirstOrDefault( s => s.LocalId == 81 );
            var cobraMk3 = configuration.shipyard.FirstOrDefault( s => s.LocalId == 0 );
            Assert.IsNotNull( krait );
            Assert.IsNotNull( cobraMk3 );

            VoiceAttackVariables.setShipValues( krait, "Ship", ref vaProxy );
            Assert.AreEqual( "Krait Mk. II", mockVAProxy.GetText( "Ship model" ) );
            Assert.AreEqual( "The Impact Kraiter", mockVAProxy.GetText("Ship name") );
            Assert.AreEqual( "TK-29K", mockVAProxy.GetText("Ship ident") );
            Assert.AreEqual( "Combat", mockVAProxy.GetText("Ship role") );
            Assert.AreEqual( 201065994, mockVAProxy.GetDecimal( "Ship value" ) );
            Assert.AreEqual( 10053299, mockVAProxy.GetDecimal("Ship rebuy") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship health") );
            Assert.AreEqual( 16, mockVAProxy.GetInt( "Ship cargo capacity" ) );
            Assert.AreEqual( 8, mockVAProxy.GetInt("Ship compartments") );
            Assert.AreEqual( 6, mockVAProxy.GetInt("Ship compartment 1 size") );
            Assert.AreEqual( true, mockVAProxy.GetBoolean( "Ship compartment 1 occupied" ) );
            Assert.AreEqual( 6, mockVAProxy.GetInt("Ship compartment 1 module class") );
            Assert.AreEqual( "C", mockVAProxy.GetText("Ship compartment 1 module grade") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship compartment 1 module health") );
            Assert.AreEqual( 2234799, mockVAProxy.GetDecimal("Ship compartment 1 module cost") );
            Assert.AreEqual( 2696600, mockVAProxy.GetDecimal("Ship compartment 1 module value") );
            Assert.AreEqual( 9, mockVAProxy.GetInt("Ship hardpoints") );
            Assert.AreEqual( true, mockVAProxy.GetBoolean( "Ship large hardpoint 1 occupied" ) );
            Assert.AreEqual( 2, mockVAProxy.GetInt("Ship large hardpoint 1 module class") );
            Assert.AreEqual( "B", mockVAProxy.GetText("Ship large hardpoint 1 module grade") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship large hardpoint 1 module health") );
            Assert.AreEqual( 310425, mockVAProxy.GetDecimal("Ship large hardpoint 1 module cost") );
            Assert.AreEqual( 344916, mockVAProxy.GetDecimal("Ship large hardpoint 1 module value") );

            VoiceAttackVariables.setShipValues( cobraMk3, "Ship", ref vaProxy );
            Assert.AreEqual( "Cobra Mk. III", mockVAProxy.GetText("Ship model") );
            Assert.AreEqual( "The Dynamo", mockVAProxy.GetText("Ship name") );
            Assert.AreEqual( "TK-20C", mockVAProxy.GetText("Ship ident") );
            Assert.AreEqual( "Multipurpose", mockVAProxy.GetText("Ship role") );
            Assert.AreEqual( 8605684, mockVAProxy.GetDecimal("Ship value") );
            Assert.AreEqual( 0, mockVAProxy.GetDecimal("Ship rebuy") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship health") );
            Assert.AreEqual( 0, mockVAProxy.GetInt("Ship cargo capacity") );
            Assert.AreEqual( 0, mockVAProxy.GetInt("Ship compartments") );
            Assert.AreEqual( null, mockVAProxy.GetInt("Ship compartment 1 size") );
            Assert.AreEqual( false, mockVAProxy.GetBoolean("Ship compartment 1 occupied") );
            Assert.AreEqual( null, mockVAProxy.GetInt("Ship compartment 1 module class") );
            Assert.AreEqual( null, mockVAProxy.GetText("Ship compartment 1 module grade") );
            Assert.AreEqual( null, mockVAProxy.GetDecimal("Ship compartment 1 module health") );
            Assert.AreEqual( null, mockVAProxy.GetDecimal("Ship compartment 1 module cost") );
            Assert.AreEqual( null, mockVAProxy.GetDecimal("Ship compartment 1 module value") );
            Assert.AreEqual( 0, mockVAProxy.GetInt("Ship hardpoints") );
            Assert.AreEqual( false, mockVAProxy.GetBoolean("Ship large hardpoint 1 occupied") );
            Assert.AreEqual( null, mockVAProxy.GetInt("Ship large hardpoint 1 module class") );
            Assert.AreEqual( null, mockVAProxy.GetText("Ship large hardpoint 1 module grade") );
            Assert.AreEqual( null, mockVAProxy.GetDecimal("Ship large hardpoint 1 module health") );
            Assert.AreEqual( null, mockVAProxy.GetDecimal("Ship large hardpoint 1 module cost") );
            Assert.AreEqual( null, mockVAProxy.GetDecimal("Ship large hardpoint 1 module value") );
        }
    }
}
