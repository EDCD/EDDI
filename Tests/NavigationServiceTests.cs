using EddiConfigService.Configurations;
using EddiCore.GameState;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Tests.Properties;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class NavigationServiceTests : TestBase
    {
        private NavigationService navigationService;
        private TestNavigationRuntimeContext navigationRuntimeContext;
        private FakeSpanshHttpClient fakeSpanshHttpClient;

        private sealed class TestNavigationRuntimeContext : INavigationRuntimeContext
        {
            private readonly EddiGameState gameState = new();

            internal TestNavigationRuntimeContext ()
            {
                GameStateService = new EddiGameStateService(
                    gameState,
                    () => ( null, null, null ),
                    null,
                    null,
                    null,
                    new System.Version( 4, 0 ) );
            }

            internal EddiGameStateService GameStateService { get; }
            internal List<Event> EnqueuedEvents { get; } = [ ];
            public IEddiGameState GameState => gameState;
            public DataProviderService DataProvider { get; init; }
            public NavigationMonitorConfiguration NavigationConfiguration { get; set; } = new();
            public MissionMonitorConfiguration MissionConfiguration { get; } = new();
            public void UpdateSearchSystem ( StarSystem system, decimal distanceLy )
            {
                GameStateService.SearchStarSystem = system;
                GameStateService.SearchDistanceLy = distanceLy;
            }

            public void UpdateSearchStation ( Station station ) => GameStateService.SearchStation = station;
            public Task UpdateDestinationSystemAsync ( ulong? destinationSystemAddress, string destinationSystem = null )
            {
                GameStateService.DestinationStarSystem = null;
                return Task.CompletedTask;
            }
            public void UpdateDestinationDistance ( decimal distanceLy ) => GameStateService.DestinationDistanceLy = distanceLy;
            public void EnqueueEvent ( Event @event ) => EnqueuedEvents.Add( @event );
        }

        [TestInitialize]
        public void Start()
        {
            var dataProvider = CreateIsolatedTestDataProvider( out fakeSpanshHttpClient, out _ );
            navigationRuntimeContext = new TestNavigationRuntimeContext
            {
                DataProvider = dataProvider
            };
            navigationService = new NavigationService( navigationRuntimeContext );

            fakeSpanshHttpClient.Expect(
                @"bodies/search?json={""filters"":{""type"":{""value"":[""Star""]},""subtype"":{""value"":[""A (Blue-White super giant) Star"",""A (Blue-White) Star"",""B (Blue-White super giant) Star"",""B (Blue-White) Star"",""F (White super giant) Star"",""F (White) Star"",""G (White-Yellow super giant) Star"",""G (White-Yellow) Star"",""K (Yellow-Orange giant) Star"",""K (Yellow-Orange) Star"",""M (Red dwarf) Star"",""M (Red giant) Star"",""M (Red super giant) Star"",""O (Blue-White) Star""]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryBodyScoopableStar ) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""material_trader"":{""value"":[""Encoded""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString(Resources.SpanshQueryStationEncodedMtrl) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""material_trader"":{""value"":[""Manufactured""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationManufacturedMtrl ) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""material_trader"":{""value"":[""Raw""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationRawMtrl ) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""technology_broker"":{""value"":[""Guardian""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationGuardianTechBroker ) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""technology_broker"":{""value"":[""Human""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationHumanTechBroker ) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""services"":{""value"":[""Interstellar Factors Contact""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationFacilitator ) );
            fakeSpanshHttpClient.Expect(
                @"stations/search?json={""filters"":{""system_primary_economy"":{""value"":[""Military""]},""type"":{""value"":[""Planetary Port""]},""services"":{""value"":[""Outfitting""]},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationScorpionSRV ) );

            fakeSpanshHttpClient.Expect( "dump/1109989017963",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpAlioth ) );
            fakeSpanshHttpClient.Expect( "systems/field_values/system_names?q=Alioth",
                @"{""min_max"":[{""id64"":1109989017963,""name"":""Alioth"",""x"":-33.65625,""y"":72.46875,""z"":4.125}],""values"":[""Alioth""]}" );
            fakeSpanshHttpClient.Expect( "dump/306253399220",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpAltair ) );
            fakeSpanshHttpClient.Expect( "dump/18263140541865",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpBarnards_Star ) );
            fakeSpanshHttpClient.Expect( "dump/22661186987433",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpEZ_Aquarii ) );
            fakeSpanshHttpClient.Expect( "dump/4717761530219",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpGendalla ) );
            fakeSpanshHttpClient.Expect( "dump/121569805492",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpSirius ) );
            fakeSpanshHttpClient.Expect( "dump/10477373803",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpSol ) );
            fakeSpanshHttpClient.Expect( "dump/5856288576210",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDump61_Cyngi ) );
        }

        [TestMethod]
        [DataRow(QueryType.encoded, null, null, 10000.0, true, "EZ Aquarii", "Magnus Gateway")]
        [DataRow(QueryType.manufactured, null, null, 10000.0, true, "Sirius", "Patterson Enterprise")]
        [DataRow(QueryType.raw, null, null, 10000.0, true, "61 Cygni", "Broglie Terminal")]
        [DataRow(QueryType.guardian, null, null, 10000.0, true, "EZ Aquarii", "Magnus Gateway")]
        [DataRow(QueryType.human, null, null, 10000.0, true, "Altair", "Solo Orbiter")]
        [DataRow(QueryType.scorpion, null, null, 10000.0, true, "Gendalla", "Aksyonov Installation")]
        [DataRow(QueryType.scoop, null, null, 10.0, true, "Sol", null)]
        [DataRow(QueryType.facilitator, null, null, 10000.0, true, "Barnard's Star", "Levi-Strauss Installation" )]
        public async Task TestNavQueryAsync(QueryType query, string stringArg0, string stringArg1, double numericArg, bool prioritizeOrbitalStations, string expectedStarSystem, string expectedStationName)
        {
            // Setup
            var sol = new StarSystem { systemname = "Sol", systemAddress = 10477373803, x = 0.0M, y = 0.0M, z = 0.0M };
            navigationRuntimeContext.GameStateService.CurrentStarSystem = sol;
            navigationRuntimeContext.GameStateService.CurrentShip = ShipDefinitions.FromEDModel( "Anaconda" );

            var result = await navigationService.NavQueryAsync( query, stringArg0, stringArg1, Convert.ToDecimal( numericArg ), prioritizeOrbitalStations ).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedStarSystem, result.system);
            Assert.AreEqual(expectedStationName, result.station);
        }

        [TestMethod]
        public async Task MissionQueryUsesInjectedMissionConfiguration()
        {
            var sol = new StarSystem { systemname = "Sol", systemAddress = 10477373803, x = 0.0M, y = 0.0M, z = 0.0M };
            navigationRuntimeContext.GameStateService.CurrentStarSystem = sol;
            navigationRuntimeContext.MissionConfiguration.missions =
            [
                new Mission( 12345, "Mission_Delivery", DateTime.UtcNow.AddHours( 1 ), MissionStatus.Active )
                {
                    destinationsystem = "Alioth"
                }
            ];

            var result = await navigationService.NavQueryAsync( QueryType.nearest ).ConfigureAwait(false);

            Assert.IsNotNull( result );
            Assert.AreEqual( "Alioth", result.system );
            CollectionAssert.AreEqual( new List<ulong> { 12345 }, result.missionids );
        }

        [TestMethod]
        public async Task UpdateQueryUsesInjectedDestinationGameState()
        {
            var sol = new StarSystem { systemname = "Sol", systemAddress = 10477373803, x = 0.0M, y = 0.0M, z = 0.0M };
            var alioth = new StarSystem { systemname = "Alioth", systemAddress = 1109989017963, x = 10.0M, y = 0.0M, z = 0.0M };
            navigationRuntimeContext.GameStateService.CurrentStarSystem = sol;
            navigationRuntimeContext.GameStateService.DestinationStarSystem = alioth;
            navigationRuntimeContext.NavigationConfiguration.plottedRouteList = new NavWaypointCollection(
                [
                    new NavWaypoint( alioth ) { missionids = [ 67890 ] }
                ] )
            {
                GuidanceEnabled = true
            };

            var result = await navigationService.NavQueryAsync( QueryType.update ).ConfigureAwait(false);

            Assert.IsNotNull( result );
            Assert.AreEqual( "Alioth", result.system );
            CollectionAssert.AreEqual( new List<ulong> { 67890 }, result.missionids );
        }
    }
}
