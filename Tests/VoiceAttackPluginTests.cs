using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using EddiIPC_Service.Client;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiJournalMonitor;
using EddiVoiceAttackAdapter;
using EddiVoiceAttackResponder;
using JetBrains.Annotations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Tests.Properties;
using Utilities;

namespace Tests
{
    public class MockVAProxy
    {
        [ UsedImplicitly ] public List<KeyValuePair<string, string>> vaLog = new();

        private readonly Dictionary<string, string> vaStrings = new();
        private readonly Dictionary<string, decimal?> vaDecimals = new();
        private readonly Dictionary<string, int?> vaIntegers = new();
        private readonly Dictionary<string, short?> vaShorts = new();
        private readonly Dictionary<string, bool?> vaBooleans = new();
        private readonly Dictionary<string, DateTime?> vaDates = new();

        [ UsedImplicitly ] 
        public System.Version VAVersion => new( 1, 16, 0 );

        [ UsedImplicitly ]
        public void WriteToLog ( string msg, string color = null )
        {
            vaLog.Add( new KeyValuePair<string, string>( msg, color ) );
        }

        #pragma warning disable IDE0060

        [UsedImplicitly]
        public string GetText ( string varName, bool retrieveFromProfile = false )
        {
            return vaStrings.TryGetValue( varName, out var s ) ? s : null;
        }

        [ UsedImplicitly ]
        public void SetText ( string varName, object value, bool saveToProfile = false )
        {
            vaStrings[ varName ] = value?.ToString();
        }

        [ UsedImplicitly ]
        public int? GetInt ( string varName, bool retrieveFromProfile = false )
        {
            return vaIntegers.TryGetValue(varName, out var i) ? i : null;
        }

        [ UsedImplicitly ]
        public void SetInt ( string varName, int? value, bool saveToProfile = false )
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
        public bool? GetBoolean ( string varName, bool retrieveFromProfile = false )
        {
            return vaBooleans.TryGetValue( varName, out var b ) ? b : null;
        }

        [ UsedImplicitly ]
        public void SetBoolean ( string varName, bool? value, bool saveToProfile = false )
        {
            vaBooleans[ varName ] = value;
        }

        [ UsedImplicitly ]
        public decimal? GetDecimal ( string varName, bool retrieveFromProfile = false )
        {
            return vaDecimals.TryGetValue( varName, out var d ) ? d : null;
        }

        [ UsedImplicitly ]
        public void SetDecimal ( string varName, decimal? value, bool saveToProfile = false )
        {
            vaDecimals[ varName ] = value;
        }

        [ UsedImplicitly ]
        public DateTime? GetDate ( string varName, bool retrieveFromProfile = false )
        {
            return vaDates.TryGetValue( varName, out var dt ) ? dt : null;
        }

        [ UsedImplicitly ]
        public void SetDate ( string varName, DateTime? value, bool saveToProfile = false )
        {
            vaDates[ varName ] = value;
        }

        #pragma warning restore IDE0060

        [UsedImplicitly ]
        public bool ContainsKey ( string varName )
        {
            return vaStrings.ContainsKey( varName ) || 
                   vaDecimals.ContainsKey( varName ) ||
                   vaIntegers.ContainsKey( varName ) || 
                   vaShorts.ContainsKey( varName ) ||
                   vaBooleans.ContainsKey( varName ) || 
                   vaDates.ContainsKey(varName);
        }

        public void ApplyRuntimeEvent( EventData eventData )
        {
            ArgumentNullException.ThrowIfNull( eventData );

            if ( !string.Equals( eventData.EventType, "va_runtime", StringComparison.OrdinalIgnoreCase ) ||
                 !string.Equals( eventData.EventName, "command_action", StringComparison.OrdinalIgnoreCase ) )
            {
                return;
            }

            if ( eventData.EventPayload.TryGetValue( "actions", out var batchedActions ) &&
                 batchedActions is IEnumerable<object> actions )
            {
                foreach ( var actionPayload in actions.OfType<IDictionary<string, object>>() )
                {
                    ApplyRuntimeAction( actionPayload );
                }
                return;
            }

            ApplyRuntimeAction( eventData.EventPayload );
        }

        private void ApplyRuntimeAction( IDictionary<string, object> payload )
        {
            if ( !payload.TryGetValue( "action", out var actionValue ) )
            {
                return;
            }

            var action = actionValue?.ToString();
            if ( string.IsNullOrWhiteSpace( action ) )
            {
                return;
            }

            var key = payload.TryGetValue( "key", out var keyValue )
                ? keyValue?.ToString() ?? string.Empty
                : string.Empty;

            payload.TryGetValue( "value", out var value );

            switch ( action )
            {
                case "write_log":
                    WriteToLog(
                        payload.TryGetValue( "message", out var messageValue )
                            ? messageValue?.ToString() ?? string.Empty
                            : string.Empty,
                        payload.TryGetValue( "color", out var colorValue )
                            ? colorValue?.ToString() ?? "white"
                            : "white" );
                    break;
                case "set_text":
                    SetText( key, value?.ToString() );
                    break;
                case "set_int":
                    SetInt( key, ParseInt( value ) );
                    break;
                case "set_decimal":
                    SetDecimal( key, ParseDecimal( value ) );
                    break;
                case "set_boolean":
                    SetBoolean( key, ParseBoolean( value ) );
                    break;
                case "set_date":
                    SetDate( key, ParseDateTime( value ) );
                    break;
            }
        }

        private static bool? ParseBoolean( object value )
        {
            if ( value == null )
            {
                return null;
            }

            if ( value is bool typed )
            {
                return typed;
            }

            return bool.TryParse( value.ToString(), out var parsed )
                ? parsed
                : null;
        }

        private static DateTime? ParseDateTime( object value )
        {
            if ( value == null )
            {
                return null;
            }

            if ( value is DateTime typed )
            {
                return typed;
            }

            return DateTime.TryParse( value.ToString(), CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var parsed )
                ? parsed
                : null;
        }

        private static decimal? ParseDecimal( object value )
        {
            if ( value == null )
            {
                return null;
            }

            return decimal.TryParse( value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                out var parsed )
                ? parsed
                : null;
        }

        private static int? ParseInt( object value )
        {
            if ( value == null )
            {
                return null;
            }

            return int.TryParse( value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture,
                out var parsed )
                ? parsed
                : null;
        }
    }

    [TestClass, TestCategory("UnitTests")]
    public class VoiceAttackPluginTests : TestBase
    {
        private MockVAProxy mockVAProxy;
        private IDisposable _runtimeEventDispatcherRegistration;
        private readonly List<EventData> _runtimeEvents = [];

        // ReSharper disable once MemberCanBePrivate.Global
        public TestContext TestContext { get; set; } = null!;
        
        [ TestInitialize]
        public void Start()
        {
            MakeSafe();
            ResetVaProxy();
            _runtimeEvents.Clear();
            _runtimeEventDispatcherRegistration = RuntimeEventDispatcher.RegisterDispatcher( ( eventData, _ ) =>
            {
                _runtimeEvents.Add( eventData );
                mockVAProxy.ApplyRuntimeEvent( eventData );
                return Task.FromResult( true );
            } );
        }

        [TestCleanup]
        public void ResetVaProxy ()
        {
            _runtimeEventDispatcherRegistration?.Dispose();
            _runtimeEventDispatcherRegistration = null;
            dynamic vaProxy = new MockVAProxy();
            mockVAProxy = (MockVAProxy)vaProxy;
            VoiceAttackPlugin.VaProxy = mockVAProxy;
        }

        [TestMethod, DoNotParallelize]
        public void TestSetState_BatchesRuntimeActions()
        {
            var dict = new Dictionary<string, object>
            {
                [ "1" ] = "test",
                [ "2" ] = 123
            };

            VoiceAttackVariables.setDictionaryValues( dict, "state" );

            Assert.HasCount( 1, _runtimeEvents );
            Assert.AreEqual( "va_runtime", _runtimeEvents[0].EventType );
            Assert.AreEqual( "command_action", _runtimeEvents[0].EventName );
            Assert.IsTrue( _runtimeEvents[0].EventPayload.TryGetValue( "actions", out var actionsPayload ) );
            Assert.IsInstanceOfType( actionsPayload, typeof( List<Dictionary<string, object>> ) );
            Assert.IsGreaterThan( 1, ((List<Dictionary<string, object>>)actionsPayload).Count );
        }

        [TestMethod]
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

            VoiceAttackVariables.setDictionaryValues( dict, "state" );
            Assert.AreEqual( dict.FirstOrDefault( kv => kv.Key == varName ).Value?.ToString() ?? string.Empty,
                mockVAProxy.GetText( "EDDI state " + varName ) );
            Assert.AreEqual( decimalResult is null 
                ? null 
                : (decimal?)decimal.Parse(decimalResult), mockVAProxy.GetDecimal( "EDDI state " + varName ) );
            Assert.AreEqual( integerResult is null 
                ? null 
                : (int?)int.Parse(integerResult), mockVAProxy.GetInt( "EDDI state " + varName ) );
            Assert.AreEqual( booleanResult is null
                ? null 
                : (bool?)bool.Parse(booleanResult), mockVAProxy.GetBoolean( "EDDI state " + varName ) );
        }

        [TestMethod]
        public void TestVAExplorationDataSoldEvent()
        {
            var line = @"{ ""timestamp"":""2016-09-23T18:57:55Z"", ""event"":""SellExplorationData"", ""Systems"":[ ""Gamma Tucanae"", ""Rho Capricorni"", ""Dain"", ""Col 285 Sector BR-S b18-0"", ""LP 571-80"", ""Kawilocidi"", ""Irulachan"", ""Alrai Sector MC-M a7-0"", ""Col 285 Sector FX-Q b19-5"", ""Col 285 Sector EX-Q b19-7"", ""Alrai Sector FB-O a6-3"" ], ""Discovered"":[ ""Irulachan"" ], ""BaseValue"":63573, ""Bonus"":1445, ""TotalEarnings"":65018 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount(1, events);
            Assert.IsInstanceOfType(events[0], typeof(ExplorationDataSoldEvent));
            var ev = events[0] as ExplorationDataSoldEvent;
            Assert.IsNotNull(ev);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = VoiceAttackVariables.Convert(vars, "EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(); }
            Assert.HasCount( 15, vaVars);
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
            var line = @"{ ""timestamp"":""2019-10-26T02:15:49Z"", ""event"":""FSSDiscoveryScan"", ""Progress"":0.439435, ""BodyCount"":7, ""NonBodyCount"":3, ""SystemName"":""Outotz WO-A d1"", ""SystemAddress"":44870715523 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            Assert.IsInstanceOfType(events[0], typeof(DiscoveryScanEvent));
            var ev = events[0] as DiscoveryScanEvent;
            Assert.IsNotNull(ev);

            Assert.AreEqual(7, ev.totalbodies);
            Assert.AreEqual(3, ev.nonbodies);
            Assert.AreEqual(44, ev.progress);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = VoiceAttackVariables.Convert(vars, "EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(); }
            Assert.HasCount( 2, vaVars );
            Assert.AreEqual(7, mockVAProxy.GetInt( "EDDI discovery scan totalbodies"));
            Assert.AreEqual(3, mockVAProxy.GetInt("EDDI discovery scan nonbodies"));
            Assert.IsNull(mockVAProxy.GetInt("EDDI discovery scan progress"));
            foreach (var variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVAAsteroidProspectedEvent()
        {
            var line = "{ \"timestamp\":\"2020-04-10T02:32:21Z\", \"event\":\"ProspectedAsteroid\", \"Materials\":[ { \"Name\":\"LowTemperatureDiamond\", \"Name_Localised\":\"Low Temperature Diamonds\", \"Proportion\":26.078022 }, { \"Name\":\"HydrogenPeroxide\", \"Name_Localised\":\"Hydrogen Peroxide\", \"Proportion\":10.189009 } ], \"MotherlodeMaterial\":\"Alexandrite\", \"Content\":\"$AsteroidMaterialContent_Low;\", \"Content_Localised\":\"Material Content: Low\", \"Remaining\":90.000000 }";
            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.HasCount( 1, events );
            Assert.IsInstanceOfType(events[0], typeof(AsteroidProspectedEvent));
            var ev = events[0] as AsteroidProspectedEvent;
            Assert.IsNotNull(ev);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var vaVars = VoiceAttackVariables.Convert(vars, "EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(); }
            Assert.HasCount( 8, vaVars );
            Assert.AreEqual(90M, mockVAProxy.GetDecimal("EDDI asteroid prospected remaining"));
            Assert.AreEqual("Alexandrite", mockVAProxy.GetText("EDDI asteroid prospected motherlode"));
            Assert.AreEqual("Low Temperature Diamonds", mockVAProxy.GetText("EDDI asteroid prospected commodities 1 commodity"));
            Assert.AreEqual(26.078022M, mockVAProxy.GetDecimal("EDDI asteroid prospected commodities 1 percentage"));
            Assert.AreEqual("Hydrogen Peroxide", mockVAProxy.GetText("EDDI asteroid prospected commodities 2 commodity"));
            Assert.AreEqual(10.189009M, mockVAProxy.GetDecimal("EDDI asteroid prospected commodities 2 percentage"));
            Assert.AreEqual(2, mockVAProxy.GetInt("EDDI asteroid prospected commodities"));
            Assert.AreEqual("Low", mockVAProxy.GetText("EDDI asteroid prospected materialcontent"));
            foreach (var variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [TestMethod]
        public void TestVACommodityEjectedEvent()
        {
            // Test a generated variable name from overlapping strings.
            var ev = new CommodityEjectedEvent(DateTime.UtcNow, CommodityDefinition.FromEDName("Water"), 5, null, true);

            var vars = new MetaVariables(ev.GetType(), ev).Results;

            var cottleVars = vars.AsCottleVariables();
            Assert.IsNotNull(cottleVars);
            Assert.HasCount( 4, cottleVars );
            Assert.AreEqual("Water", cottleVars.FirstOrDefault(k => k.key == "commodity")?.value ?? string.Empty);
            Assert.AreEqual(5, cottleVars.FirstOrDefault(k => k.key == "amount")?.value ?? string.Empty);
            Assert.IsNull(cottleVars.FirstOrDefault(k => k.key == "missionid")?.value);
            Assert.IsTrue((bool)(cottleVars.FirstOrDefault(k => k.key == "abandoned")?.value ?? false));

            var vaVars = VoiceAttackVariables.Convert(vars, "EDDI", ev.type);
            foreach (var @var in vaVars) { @var.Set(); }
            Assert.HasCount( 4, vaVars );
            Assert.AreEqual("Water", mockVAProxy.GetText("EDDI commodity ejected commodity"));
            Assert.AreEqual(5, mockVAProxy.GetInt("EDDI commodity ejected amount"));
            Assert.IsNull(mockVAProxy.GetDecimal("EDDI commodity ejected missionid"));
            Assert.IsTrue(mockVAProxy.GetBoolean("EDDI commodity ejected abandoned"));
            foreach (var variable in vaVars)
            {
                Assert.IsTrue(mockVAProxy.ContainsKey(variable.key), "Unmatched key");
            }
        }

        [ TestMethod, DoNotParallelize ]
        public void TestVAShip ()
        {
            // Read from our test item "shipMonitor.json"
            var configuration = DeserializeJsonResource<ShipMonitorConfiguration>( Resources.shipMonitor );
            var krait = configuration.shipyard.FirstOrDefault( s => s.LocalId == 81 );
            var cobraMk3 = configuration.shipyard.FirstOrDefault( s => s.LocalId == 0 );
            
            Assert.IsNotNull( krait );
            Assert.IsNotNull( cobraMk3 );

            VoiceAttackVariables.setShipValues( krait, "Ship" );
            Assert.AreEqual( "Krait Mk. II", mockVAProxy.GetText( "Ship model" ) );
            Assert.AreEqual( "The Impact Kraiter", mockVAProxy.GetText("Ship name") );
            Assert.AreEqual( "TK-29K", mockVAProxy.GetText("Ship ident") );
            Assert.AreEqual( "Combat", mockVAProxy.GetText("Ship role") );
            Assert.AreEqual( 201065994, mockVAProxy.GetDecimal( "Ship value" ) );
            Assert.AreEqual( 10053299, mockVAProxy.GetDecimal("Ship rebuy" ) );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship health") );
            Assert.AreEqual( 16, mockVAProxy.GetInt( "Ship cargo capacity" ) );
            Assert.AreEqual( 8, mockVAProxy.GetInt("Ship compartments") );
            Assert.AreEqual( 6, mockVAProxy.GetInt("Ship compartment 0 size") );
            Assert.IsTrue(mockVAProxy.GetBoolean("Ship compartment 0 occupied"));
            Assert.AreEqual( 6, mockVAProxy.GetInt("Ship compartment 0 module class") );
            Assert.AreEqual( "C", mockVAProxy.GetText("Ship compartment 0 module grade") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship compartment 0 module health") );
            Assert.AreEqual( 2234799, mockVAProxy.GetDecimal("Ship compartment 0 module cost") );
            Assert.AreEqual( 2696600, mockVAProxy.GetDecimal("Ship compartment 0 module value") );
            Assert.AreEqual( 9, mockVAProxy.GetInt("Ship hardpoints") );
            Assert.IsTrue(mockVAProxy.GetBoolean("Ship large hardpoint 0 occupied"));
            Assert.AreEqual( 2, mockVAProxy.GetInt("Ship large hardpoint 0 module class") );
            Assert.AreEqual( "B", mockVAProxy.GetText("Ship large hardpoint 0 module grade") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship large hardpoint 0 module health") );
            Assert.AreEqual( 310425, mockVAProxy.GetDecimal("Ship large hardpoint 0 module cost") );
            Assert.AreEqual( 344916, mockVAProxy.GetDecimal("Ship large hardpoint 0 module value") );

            VoiceAttackVariables.setShipValues( cobraMk3, "Ship" );
            Assert.AreEqual( "Cobra Mk. III", mockVAProxy.GetText("Ship model") );
            Assert.AreEqual( "The Dynamo", mockVAProxy.GetText("Ship name") );
            Assert.AreEqual( "TK-20C", mockVAProxy.GetText("Ship ident") );
            Assert.AreEqual( "Multipurpose", mockVAProxy.GetText("Ship role") );
            Assert.AreEqual( 8605684, mockVAProxy.GetDecimal("Ship value" ) );
            Assert.AreEqual( 0, mockVAProxy.GetDecimal("Ship rebuy") );
            Assert.AreEqual( 100M, mockVAProxy.GetDecimal("Ship health") );
            Assert.AreEqual( 0, mockVAProxy.GetInt("Ship cargo capacity") );
            Assert.AreEqual( 0, mockVAProxy.GetInt("Ship compartments") );
            Assert.IsNull( mockVAProxy.GetInt("Ship compartment 0 size") );
            Assert.IsFalse( mockVAProxy.GetBoolean("Ship compartment 0 occupied") );
            Assert.IsNull(mockVAProxy.GetInt("Ship compartment 0 module class") );
            Assert.AreEqual( string.Empty, mockVAProxy.GetText("Ship compartment 0 module grade") );
            Assert.IsNull(mockVAProxy.GetDecimal("Ship compartment 0 module health"));
            Assert.IsNull(mockVAProxy.GetDecimal("Ship compartment 0 module cost"));
            Assert.IsNull(mockVAProxy.GetDecimal("Ship compartment 0 module value"));
            Assert.AreEqual( 0, mockVAProxy.GetInt("Ship hardpoints") );
            Assert.IsFalse(mockVAProxy.GetBoolean("Ship large hardpoint 0 occupied"));
            Assert.IsNull(mockVAProxy.GetInt("Ship large hardpoint 0 module class"));
            Assert.AreEqual( string.Empty, mockVAProxy.GetText("Ship large hardpoint 0 module grade") );
            Assert.IsNull(mockVAProxy.GetDecimal("Ship large hardpoint 0 module health"));
            Assert.IsNull(mockVAProxy.GetDecimal("Ship large hardpoint 0 module cost"));
            Assert.IsNull(mockVAProxy.GetDecimal("Ship large hardpoint 0 module value"));
        }

        [TestMethod, DoNotParallelize]
        public void HasEddiProcessExited_WhenConnectedToExternalInstance_ReturnsFalse()
        {
            var launcherType = typeof( EddiProcessLauncher );
            var managedField = launcherType.GetField( "_managedEddiProcess", BindingFlags.NonPublic | BindingFlags.Static );
            var processField = launcherType.GetField( "_eddiProcess", BindingFlags.NonPublic | BindingFlags.Static );

            Assert.IsNotNull( managedField );
            Assert.IsNotNull( processField );

            var originalManaged = (bool?)managedField.GetValue( null );
            var originalProcess = (Process)processField.GetValue( null );

            try
            {
                managedField.SetValue( null, false );
                processField.SetValue( null, null );

                Assert.IsFalse( EddiProcessLauncher.HasEddiProcessExited() );
            }
            finally
            {
                managedField.SetValue( null, originalManaged );
                processField.SetValue( null, originalProcess );
            }
        }

        [TestMethod, DoNotParallelize]
        public async Task TestVAStarSystem ()
        {
            // Obtain star system data from Sol.
            EDDI.Instance.DataProvider = CreateTestDataProvider();
            FakeSpanshHttpClient.Expect( "dump/10477373803", DeserializeJsonResource<string>( Resources.SpanshStarSystemDumpSol ) );
            var sol = await fakeSpanshService.GetStarSystemAsync( 10477373803U, true, CancellationToken.None ).ConfigureAwait(false);
            Assert.IsNotNull( sol );

            VoiceAttackVariables.setStarSystemValues( sol, "System" );
            Assert.AreEqual( "Sol", mockVAProxy.GetText("System name"));
        }

        [TestMethod]
        public async Task RuntimeReceiver_PascalCaseRuntimeMetadata_AppliesCommandActions()
        {
            var runtimeEventPayload = JObject.Parse(
                """
                {
                  "EventType": "va_runtime",
                  "EventName": "command_action",
                  "EventPayload": {
                    "actions": [
                      { "action": "set_text", "key": "Status vehicle", "value": "Ship" },
                      { "action": "set_boolean", "key": "Status being interdicted", "value": false }
                    ]
                  }
                }
                """ );

            var envelope = MessageEnvelope.Create( MessageTypes.Event, runtimeEventPayload );
            var eventArgs = new MessageReceivedEventArgs( MessageTypes.Event, envelope );

            VoiceAttackRuntimeEventReceiver.HandleMessageReceived( null, eventArgs );

            for ( var i = 0; i < 20 && mockVAProxy.GetText( "Status vehicle" ) != "Ship"; i++ )
            {
                await Task.Delay( 25, TestContext.CancellationToken ).ConfigureAwait( false );
            }

            Assert.AreEqual( "Ship", mockVAProxy.GetText( "Status vehicle" ) );
            Assert.IsFalse( mockVAProxy.GetBoolean( "Status being interdicted" ) );
        }
    }
}
