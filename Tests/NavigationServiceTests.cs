using EddiCore;
using EddiDataDefinitions;
using EddiNavigationService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;
using Tests.Properties;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class NavigationServiceTests : TestBase
    {
        private NavigationService navigationService;

        [TestInitialize]
        public void Start()
        {
            navigationService = new NavigationService();
            MakeSafe();

            EDDI.Instance.DataProvider = ConfigureTestDataProvider();

            FakeSpanshHttpClient.Expect(
                @"bodies/search?={""filters"":{""type"":{""value"":[""Star""]},""subtype"":{""value"":[""A (Blue-White super giant) Star"",""A (Blue-White) Star"",""B (Blue-White super giant) Star"",""B (Blue-White) Star"",""F (White super giant) Star"",""F (White) Star"",""G (White-Yellow super giant) Star"",""G (White-Yellow) Star"",""K (Yellow-Orange giant) Star"",""K (Yellow-Orange) Star"",""M (Red dwarf) Star"",""M (Red giant) Star"",""M (Red super giant) Star"",""O (Blue-White) Star""]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryBodyScoopableStar ) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""material_trader"":{""value"":[""Encoded""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString(Resources.SpanshQueryStationEncodedMtrl) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""material_trader"":{""value"":[""Manufactured""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationManufacturedMtrl ) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""material_trader"":{""value"":[""Raw""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationRawMtrl ) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""technology_broker"":{""value"":[""Guardian""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationGuardianTechBroker ) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""technology_broker"":{""value"":[""Human""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationHumanTechBroker ) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""services"":{""value"":[""Interstellar Factors Contact""]},""is_planetary"":{""value"":false},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationFacilitator ) );
            FakeSpanshHttpClient.Expect(
                @"stations/search?={""filters"":{""system_primary_economy"":{""value"":[""Military""]},""type"":{""value"":[""Planetary Port""]},""services"":{""value"":[""Outfitting""]},""has_large_pad"":{""value"":true},""distance_to_arrival"":{""comparison"":""<=>"",""value"":[0,10000]}},""sort"":[{""distance"":{""direction"":""asc""}},{""distance_to_arrival"":{""direction"":""asc""}}],""size"":10,""page"":0,""reference_coords"":{""x"":0.0,""y"":0.0,""z"":0.0}}",
                Encoding.UTF8.GetString( Resources.SpanshQueryStationScorpionSRV ) );

            FakeSpanshHttpClient.Expect( "dump/1109989017963",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpAlioth ) );
            FakeSpanshHttpClient.Expect( "dump/306253399220",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpAltair ) );
            FakeSpanshHttpClient.Expect( "dump/18263140541865",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpBarnards_Star ) );
            FakeSpanshHttpClient.Expect( "dump/22661186987433",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpEZ_Aquarii ) );
            FakeSpanshHttpClient.Expect( "dump/4717761530219",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpGendalla ) );
            FakeSpanshHttpClient.Expect( "dump/121569805492",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpSirius ) );
            FakeSpanshHttpClient.Expect( "dump/10477373803",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDumpSol ) );
            FakeSpanshHttpClient.Expect( "dump/5856288576210",
                Encoding.UTF8.GetString( Resources.SpanshStarSystemDump61_Cyngi ) );
        }

        [DataTestMethod, DoNotParallelize]
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
            EDDI.Instance.CurrentStarSystem = sol;
            EDDI.Instance.CurrentShip = ShipDefinitions.FromEDModel( "Anaconda" );

            var result = await navigationService.NavQueryAsync(query, stringArg0, stringArg1, Convert.ToDecimal(numericArg), prioritizeOrbitalStations).ConfigureAwait(false);
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedStarSystem, result.system);
            Assert.AreEqual(expectedStationName, result.station);
        }
    }
}