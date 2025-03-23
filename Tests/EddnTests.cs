using EddiCore;
using EddiEddnResponder;
using EddiEddnResponder.Schemas;
using EddiEddnResponder.Toolkit;
using EddiEvents;
using EddiJournalMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EddnTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private EDDNResponder makeTestEDDNResponder()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "system/2724879894859", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemPleiadesSector_HR_W_d1_79 ) );
            fakeSpanshRestClient.Expect( "system/1183229809290", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemPleiadesSector_GW_W_c1_4 ) );
            fakeSpanshRestClient.Expect( "system/10477373803", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemSol ) );
            fakeSpanshRestClient.Expect( "system/5068463809865", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemHyadesSector_DL_X_b1_2 ) );
            fakeSpanshRestClient.Expect( "system/35835461971465", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemOmegaSector_DM_M_b7_16 ) );
            fakeSpanshRestClient.Expect( "system/3107509474002", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemArtemis ) );

            var responder = new EDDNResponder();
            responder.SetUnitTesting(true);
            responder.Start();
            return responder;
        }

        [TestMethod]
        public void TestEddnSchemaInitialization()
        {
            var responder = makeTestEDDNResponder();
            Assert.IsTrue( responder.schemas.Any() );
            Assert.IsTrue( responder.capiSchemas.Any() );
        }

        [TestMethod]
        public void TestEDDNResponderGoodMatch()
        {
            var responder = makeTestEDDNResponder();
            responder.eddnState.Location.systemName = "Sol";
            responder.eddnState.Location.systemAddress = 10477373803;
            responder.eddnState.Location.systemX = 0.0M;
            responder.eddnState.Location.systemY = 0.0M;
            responder.eddnState.Location.systemZ = 0.0M;

            var confirmed = responder.eddnState.Location.ConfirmNameAndCoordinates();

            Assert.IsTrue(confirmed);
            Assert.AreEqual("Sol", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)10477373803, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemX);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemY);
            Assert.AreEqual(0.0M, responder.eddnState.Location.systemZ);
        }

        [TestMethod]
        public void TestEDDNResponderMismatchedStarPos()
        {
            var responder = makeTestEDDNResponder();
            // Intentionally place our EDDN responder in a state with incorrect coordinates (from Sol).
            // The 'Docked' event does include systemName and systemAddress, so we set those here.
            responder.eddnState.Location.systemName = "Artemis";
            responder.eddnState.Location.systemAddress = 3107509474002;
            responder.eddnState.Location.systemX = 0.0M;
            responder.eddnState.Location.systemY = 0.0M;
            responder.eddnState.Location.systemZ = 0.0M;

            var confirmed = responder.eddnState.Location.ConfirmNameAndCoordinates();

            Assert.IsFalse(confirmed);
            Assert.AreEqual("Artemis", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)3107509474002, responder.eddnState.Location.systemAddress);
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
        }

        [TestMethod]
        public void TestEDDNResponderMismatchedSystemAddress()
        {
            var responder = makeTestEDDNResponder();
            // Intentionally place our EDDN responder in a state with incorrect SystemAddress (from Artemis).
            responder.eddnState.Location.systemName = "Sol";
            responder.eddnState.Location.systemAddress = 3107509474002;
            responder.eddnState.Location.systemX = 0.0M;
            responder.eddnState.Location.systemY = 0.0M;
            responder.eddnState.Location.systemZ = 0.0M;

            var confirmed = responder.eddnState.Location.ConfirmNameAndCoordinates();

            Assert.IsFalse(confirmed);
            Assert.AreEqual( 3107509474002U, responder.eddnState.Location.systemAddress );
            Assert.IsNull( responder.eddnState.Location.systemX );
            Assert.IsNull( responder.eddnState.Location.systemY );
            Assert.IsNull( responder.eddnState.Location.systemZ );
            Assert.IsNull( responder.eddnState.Location.systemName );
        }

        [TestMethod]
        public void TestEDDNResponderMismatchedName()
        {
            var responder = makeTestEDDNResponder();
            // Tests that procedurally generated body names match the procedurally generated system name
            responder.eddnState.Location.systemName = "Pleiades Sector HR-W d1-79";
            responder.eddnState.Location.systemAddress = 2724879894859;
            responder.eddnState.Location.systemX = -80.62500M;
            responder.eddnState.Location.systemY = -146.65625M;
            responder.eddnState.Location.systemZ = -343.25000M;

            var confirmed = responder.eddnState.Location.ConfirmScan( "Hyades Sector DL-X b1-2 A 1" );

            Assert.IsFalse( confirmed );
            Assert.AreEqual(0U, responder.eddnState.Location.systemAddress );
            Assert.IsNull( responder.eddnState.Location.systemX );
            Assert.IsNull( responder.eddnState.Location.systemY );
            Assert.IsNull( responder.eddnState.Location.systemZ );
            Assert.IsNull( responder.eddnState.Location.systemName );
        }

        [TestMethod]
        public void TestEDDNResponderDockedEvent()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "system/670417429889", @"{""record"":{""allegiance"":""Independent"",""bodies"":[{""distance_to_arrival"":0.0,""estimated_mapping_value"":4578,""estimated_scan_value"":1207,""id"":72058264455357825,""id64"":72058264455357825,""is_main_star"":true,""name"":""Diaguandri A"",""subtype"":""M (Red dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":126.087216,""estimated_mapping_value"":4574,""estimated_scan_value"":1206,""id"":108087061474321793,""id64"":108087061474321793,""is_main_star"":null,""name"":""Diaguandri B"",""subtype"":""M (Red dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":13774.910406,""estimated_mapping_value"":4561,""estimated_scan_value"":1202,""id"":144115858493285761,""id64"":144115858493285761,""is_main_star"":null,""name"":""Diaguandri C"",""subtype"":""L (Brown dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":119.657659,""estimated_mapping_value"":85193,""estimated_scan_value"":23448,""id"":648519016758781313,""id64"":648519016758781313,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":9,""subtype"":""Sulphur Dioxide Gas Vent"",""type"":""Gas Vent"",""value"":0},{""count"":15,""subtype"":""Sulphur Dioxide Fumarole"",""type"":""Fumarole"",""value"":0}],""name"":""Diaguandri B 1"",""subtype"":""Metal-rich body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":117.391251,""estimated_mapping_value"":38774,""estimated_scan_value"":10672,""id"":684547813777745281,""id64"":684547813777745281,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":5,""subtype"":""Sulphur Dioxide Gas Vent"",""type"":""Gas Vent"",""value"":0},{""count"":8,""subtype"":""Silicate Magma Lava Spout"",""type"":""Lava Spout"",""value"":0},{""count"":4,""subtype"":""Silicate Vapour Gas Vent"",""type"":""Gas Vent"",""value"":0},{""count"":13,""subtype"":""Sulphur Dioxide Fumarole"",""type"":""Fumarole"",""value"":0}],""name"":""Diaguandri B 2"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":494.822908,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":720576610796709249,""id64"":720576610796709249,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":5,""subtype"":""Settlement"",""type"":""Surface Station"",""value"":0},{""count"":1,""subtype"":""Crater Outpost"",""type"":""Surface Station"",""value"":0}],""name"":""Diaguandri AB 1"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":768.725047,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":756605407815673217,""id64"":756605407815673217,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":6,""subtype"":""Settlement"",""type"":""Surface Station"",""value"":0},{""count"":1,""subtype"":""Installation"",""type"":""Surface Station"",""value"":0}],""name"":""Diaguandri AB 2"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1714.310889,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":792634204834637185,""id64"":792634204834637185,""is_main_star"":null,""name"":""Diaguandri AB 3"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":13653.909546,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1008806986948420993,""id64"":1008806986948420993,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":1,""subtype"":""Settlement"",""type"":""Surface Station"",""value"":0}],""name"":""Diaguandri C 1"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":13634.357128,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1044835783967384961,""id64"":1044835783967384961,""is_main_star"":null,""name"":""Diaguandri C 2"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":13291.970999,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1080864580986348929,""id64"":1080864580986348929,""is_main_star"":null,""landmark_value"":1000000,""landmarks"":[{""count"":1,""subtype"":""Settlement"",""type"":""Surface Station"",""value"":0},{""count"":29,""subtype"":""Bacterium Vesicula"",""type"":""Bacterium"",""value"":1000000}],""name"":""Diaguandri C 3"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":14404.952978,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1116893378005312897,""id64"":1116893378005312897,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":3,""subtype"":""Settlement"",""type"":""Surface Station"",""value"":0}],""name"":""Diaguandri C 4"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""}],""body_count"":12,""controlling_minor_faction"":""EXO"",""controlling_minor_faction_state"":""Expansion"",""controlling_power"":""Li Yong-Rui"",""estimated_mapping_value"":153227,""estimated_scan_value"":41235,""government"":""Democracy"",""id64"":670417429889,""known_permit"":false,""landmark_value"":1000000,""minor_faction_presences"":[{""influence"":0.01002,""name"":""Cartel of Diaguandri"",""state"":""None""},{""influence"":0.019038,""name"":""Diaguandri Interstellar"",""state"":""None""},{""influence"":0.641283,""name"":""EXO"",""state"":""Expansion""},{""influence"":0.028056,""name"":""Natural Diaguandri Regulatory State"",""state"":""None""},{""influence"":0.094188,""name"":""Revolutionary Party of Diaguandri"",""state"":""Expansion""},{""influence"":0.039078,""name"":""The Brotherhood of the Dark Circle"",""state"":""None""},{""influence"":0.168337,""name"":""Ukrainian Pilots Federation"",""state"":""Boom""}],""name"":""Diaguandri"",""needs_permit"":false,""population"":10326175,""power"":[""Li Yong-Rui""],""power_state"":""Stronghold"",""primary_economy"":""High Tech"",""region"":""Inner Orion Spur"",""secondary_economy"":""Refinery"",""security"":""Medium"",""stations"":[{""distance_to_arrival"":11.654488,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3705379584,""medium_pads"":4,""name"":""T9Z-3QX"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":24.00141,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3708263168,""medium_pads"":4,""name"":""VHX-G9B"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""market_id"":3706526976,""name"":""JFX-19W"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":13636.17671,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3700500224,""medium_pads"":4,""name"":""Y8M-79Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":564.24725,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703587840,""medium_pads"":4,""name"":""J8Q-99Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3709141504,""medium_pads"":4,""name"":""T9V-79F"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":135.338012,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3703767040,""medium_pads"":4,""name"":""V4X-67V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":168.022596,""has_large_pad"":true,""has_market"":true,""market_id"":3706017280,""name"":""H0L-TVV"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703558400,""medium_pads"":4,""name"":""V4B-9QM"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":169.182074,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3708489472,""medium_pads"":4,""name"":""T5J-06V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1642.977265,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3702963456,""medium_pads"":4,""name"":""JZV-GKJ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""market_id"":3702868480,""name"":""V3T-TTW"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709727488,""medium_pads"":4,""name"":""HHV-GHZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1597.785206,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3702719488,""medium_pads"":4,""name"":""K5V-18N"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1109.92547,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""market_id"":3701299712,""name"":""X0J-2QZ"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703045120,""medium_pads"":4,""name"":""X8G-48Y"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":807.339228,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""market_id"":3707917824,""name"":""VBJ-29T"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3708438528,""medium_pads"":4,""name"":""HNT-58X"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704801792,""medium_pads"":4,""name"":""H7Q-20L"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3703465472,""medium_pads"":4,""name"":""XZB-30Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""market_id"":3700552192,""name"":""GHX-N7Z"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":511.491189,""has_large_pad"":true,""has_market"":true,""has_shipyard"":true,""market_id"":3707057920,""name"":""HZW-05L"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706940672,""medium_pads"":4,""name"":""V3F-B4W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704632832,""medium_pads"":4,""name"":""Q2M-NQW"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":561.220661,""has_large_pad"":true,""has_market"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3705312512,""medium_pads"":4,""name"":""VZB-71N"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":722.731031,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706326528,""medium_pads"":4,""name"":""J2M-88F"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":280.450907,""has_large_pad"":true,""has_market"":true,""market_id"":3703411200,""name"":""JFW-7XZ"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":192.974367,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3703919872,""medium_pads"":4,""name"":""V9J-56J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":296.409045,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3710379008,""medium_pads"":4,""name"":""Q1B-80J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3707664384,""medium_pads"":4,""name"":""H3G-87J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""large_pads"":8,""market_id"":3710609408,""medium_pads"":4,""name"":""W0W-39W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":570.427526,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3705681152,""medium_pads"":4,""name"":""VZX-15T"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":564.270686,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706887424,""medium_pads"":4,""name"":""T0G-G3B"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":123.306847,""has_large_pad"":true,""has_market"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3709277696,""medium_pads"":4,""name"":""X9Z-78Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3700751104,""medium_pads"":4,""name"":""Q2K-23L"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":532.77776,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706567168,""medium_pads"":4,""name"":""HZK-WTH"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":796.445255,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3700645888,""medium_pads"":4,""name"":""NHH-5QZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":497.02956,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":5,""market_id"":3223343616,""medium_pads"":14,""name"":""Ray Gateway"",""small_pads"":13,""type"":""Coriolis Starport""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3703284480,""medium_pads"":4,""name"":""K0F-6HY"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":11.311482,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3710278400,""medium_pads"":4,""name"":""G4N-86K"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3702676992,""medium_pads"":4,""name"":""V8Y-G4W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":186.633368,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704360448,""medium_pads"":4,""name"":""KBW-32W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1710.458688,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709779456,""medium_pads"":4,""name"":""HNT-18V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":582.902682,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3702586368,""medium_pads"":4,""name"":""K6B-BTZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3708648192,""medium_pads"":4,""name"":""HBL-7XZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709673728,""medium_pads"":4,""name"":""H7T-L7K"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3700722176,""medium_pads"":4,""name"":""T0J-72Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3701482240,""medium_pads"":4,""name"":""G6Y-14V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":149.257907,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3701516288,""medium_pads"":4,""name"":""Q8Q-0VQ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":134.722704,""has_large_pad"":true,""has_market"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3706943744,""medium_pads"":4,""name"":""G1X-26N"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704933120,""medium_pads"":4,""name"":""Q7K-84F"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":769.718609,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3705192192,""medium_pads"":4,""name"":""J0J-9XZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3706259456,""medium_pads"":4,""name"":""KLV-64Q"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":494.129785,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""large_pads"":8,""market_id"":3705547264,""medium_pads"":4,""name"":""KBK-G4H"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""market_id"":3707299072,""name"":""HZV-56T"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":145.014047,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""large_pads"":8,""market_id"":3709038336,""medium_pads"":4,""name"":""Q2M-BKB"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":860.708164,""has_large_pad"":true,""has_market"":true,""market_id"":3706174464,""name"":""NFZ-45Z"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3707240448,""medium_pads"":4,""name"":""H2Z-17V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":669.036881,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703765504,""medium_pads"":4,""name"":""QLV-55W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":473.302517,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706520832,""medium_pads"":4,""name"":""H3B-BXM"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":51.798345,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704278016,""medium_pads"":4,""name"":""V3T-N6Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709251328,""medium_pads"":4,""name"":""V7F-4HZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":539.865752,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3701596160,""medium_pads"":4,""name"":""Q0W-58J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":185.307901,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709247744,""medium_pads"":4,""name"":""Q2L-5HY"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":195.522472,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3700063232,""medium_pads"":4,""name"":""H6Q-51W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709762816,""medium_pads"":4,""name"":""KBQ-L3W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":467.060758,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706940928,""medium_pads"":4,""name"":""H0Z-32W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703304192,""medium_pads"":4,""name"":""K1N-1QJ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3708630528,""medium_pads"":4,""name"":""X3N-72Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":14404.950424,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3700754944,""medium_pads"":4,""name"":""JNF-94L"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":124.020382,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706828800,""medium_pads"":4,""name"":""V9Q-T2J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1295.793639,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3702923776,""medium_pads"":4,""name"":""G1W-45G"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3701505280,""medium_pads"":4,""name"":""K7M-23W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3702341888,""medium_pads"":4,""name"":""B5V-5TZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3705665792,""medium_pads"":4,""name"":""X2Z-N7J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":138.61976,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3707513856,""medium_pads"":4,""name"":""QLH-TCZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":186.155129,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3705496576,""medium_pads"":4,""name"":""T7F-NQZ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":2105.801593,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3704760064,""medium_pads"":4,""name"":""V3W-07W"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":543.39342,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3708620544,""medium_pads"":4,""name"":""T4W-49Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":653.709946,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3707547392,""medium_pads"":4,""name"":""H0H-2XY"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":915.789125,""has_large_pad"":true,""has_market"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3705790976,""medium_pads"":4,""name"":""JBW-8KM"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":53.174338,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""market_id"":3701644800,""name"":""KNJ-W7G"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":2066.715949,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""market_id"":3707997184,""name"":""Q1L-24Q"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1675.873556,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3702018304,""medium_pads"":4,""name"":""V2F-T2L"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":653.70535,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3702931456,""medium_pads"":4,""name"":""T1Z-BQT"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":13649.521054,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3708017920,""medium_pads"":4,""name"":""HBM-GTG"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":185.650939,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703707136,""medium_pads"":4,""name"":""XFW-83G"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":179.066837,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3706810112,""medium_pads"":4,""name"":""VHM-BVW"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3708135680,""medium_pads"":4,""name"":""VLQ-32G"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3702998016,""medium_pads"":4,""name"":""Q3G-5VW"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":652.751078,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706154240,""medium_pads"":4,""name"":""V6V-67V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3710637824,""medium_pads"":4,""name"":""J6X-4HJ"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":745.78541,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3702839552,""medium_pads"":4,""name"":""X3H-B9G"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":638.744474,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3703303424,""medium_pads"":4,""name"":""TFJ-THG"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3710063104,""medium_pads"":4,""name"":""GBM-T6T"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":701.97689,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704295680,""medium_pads"":4,""name"":""V8J-51J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3710200832,""medium_pads"":4,""name"":""KBG-70V"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3704273920,""medium_pads"":4,""name"":""H3L-P1T"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3703191808,""medium_pads"":4,""name"":""V5T-LTL"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":515.213417,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3703612160,""medium_pads"":4,""name"":""T9V-7HY"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3704676352,""medium_pads"":4,""name"":""K4X-34Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":60.534721,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3709196032,""medium_pads"":4,""name"":""TZH-8KT"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3707729408,""medium_pads"":4,""name"":""T0V-7KW"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":1597.646465,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3701621504,""medium_pads"":4,""name"":""J9V-2TN"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":6.102864,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3707772416,""medium_pads"":4,""name"":""WNL-67Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3707776512,""medium_pads"":4,""name"":""HNH-40J"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":769.795534,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3705880832,""medium_pads"":4,""name"":""GLW-G0K"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":170.143565,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":8,""market_id"":3705542656,""medium_pads"":4,""name"":""WNH-G9Z"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":494.822908,""has_large_pad"":false,""market_id"":3510379776,""name"":""al-Khowarizmi Horizons""},{""distance_to_arrival"":494.822908,""has_large_pad"":false,""market_id"":3510380288,""name"":""Bulmer Enterprise""},{""distance_to_arrival"":556.280766,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894833408,""medium_pads"":0,""name"":""Heighton Manufacturing Facility"",""small_pads"":0,""type"":""Settlement""},{""distance_to_arrival"":492.87981,""has_large_pad"":true,""has_market"":true,""has_outfitting"":true,""large_pads"":2,""market_id"":3510379520,""medium_pads"":2,""name"":""Rothman Vista"",""small_pads"":4,""type"":""Planetary Outpost""},{""distance_to_arrival"":464.153578,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894832640,""medium_pads"":0,""name"":""Nnadi Agricultural Biome"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":574.886376,""has_large_pad"":true,""has_market"":true,""large_pads"":2,""market_id"":3894833664,""medium_pads"":0,""name"":""Emeagwali's Rest"",""small_pads"":2,""type"":""Settlement""},{""distance_to_arrival"":477.565978,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894833152,""medium_pads"":0,""name"":""Inoue Industrial Site"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":464.728467,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894832896,""medium_pads"":0,""name"":""Clarke Botanical Habitat"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":768.725047,""has_large_pad"":false,""market_id"":3510380544,""name"":""Rondon Stop""},{""distance_to_arrival"":768.725047,""has_large_pad"":false,""market_id"":3510380032,""name"":""Collins Installation""},{""distance_to_arrival"":724.332105,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894829824,""medium_pads"":0,""name"":""Bonfils Nutrition Collection"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":757.080864,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894830848,""medium_pads"":0,""name"":""Yarovy Chemical Depot"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":697.967902,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894830336,""medium_pads"":0,""name"":""Cohen Cultivation Hub"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":724.366657,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894830592,""medium_pads"":0,""name"":""Parekh Chemical Holdings"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":684.83436,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894831104,""medium_pads"":0,""name"":""Zadzisai Industrial Plant"",""small_pads"":0,""type"":""Settlement""},{""distance_to_arrival"":755.558277,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894830080,""medium_pads"":0,""name"":""Suarez Agricultural Exchange"",""small_pads"":2,""type"":""Settlement""},{""distance_to_arrival"":13612.123613,""has_large_pad"":true,""has_market"":true,""large_pads"":2,""market_id"":3894831360,""medium_pads"":0,""name"":""Koo's Globe"",""small_pads"":2,""type"":""Settlement""},{""distance_to_arrival"":13249.640834,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894833920,""medium_pads"":1,""name"":""Garrido's Locus"",""small_pads"":0,""type"":""Settlement""},{""distance_to_arrival"":14381.680554,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894831616,""medium_pads"":1,""name"":""Pawlikowski Boarding Zone"",""small_pads"":0,""type"":""Settlement""},{""distance_to_arrival"":14401.53618,""has_large_pad"":true,""has_market"":true,""large_pads"":1,""market_id"":3894832384,""medium_pads"":0,""name"":""Endo Engineering Facility"",""small_pads"":0,""type"":""Settlement""},{""distance_to_arrival"":14337.006159,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894831872,""medium_pads"":0,""name"":""Brewer Manufacturing Forge"",""small_pads"":1,""type"":""Settlement""},{""distance_to_arrival"":14331.090322,""has_large_pad"":false,""has_market"":true,""large_pads"":0,""market_id"":3894832128,""medium_pads"":0,""name"":""Gokhale Chemical Depot"",""small_pads"":1,""type"":""Settlement""}],""thargoid_war_failure_state"":""None"",""thargoid_war_state"":""None"",""thargoid_war_success_state"":""None"",""updated_at"":""2025-01-05 10:19:35+00"",""x"":-41.0625,""y"":-62.15625,""z"":-103.25}}" );

            var line = @"{
                ""timestamp"": ""2018-07-30T04:50:32Z"",
                ""event"": ""FSDJump"",
                ""StarSystem"": ""Diaguandri"",
                ""SystemAddress"": 670417429889,
                ""StarPos"": [-41.06250, -62.15625, -103.25000],
                ""SystemAllegiance"": ""Independent"",
                ""SystemEconomy"": ""$economy_HighTech;"",
                ""SystemEconomy_Localised"": ""High Tech"",
                ""SystemSecondEconomy"": ""$economy_Refinery;"",
                ""SystemSecondEconomy_Localised"": ""Refinery"",
                ""SystemGovernment"": ""$government_Democracy;"",
                ""SystemGovernment_Localised"": ""Democracy"",
                ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
                ""SystemSecurity_Localised"": ""Medium Security"",
                ""Population"": 10303479,
                ""JumpDist"": 8.018,
                ""FuelUsed"": 0.917520,
                ""FuelLevel"": 29.021893,
                ""Factions"": [{
                    ""Name"": ""Diaguandri Interstellar"",
                    ""FactionState"": ""Election"",
                    ""Government"": ""Corporate"",
                    ""Influence"": 0.072565,
                    ""Allegiance"": ""Independent"",
                    ""RecoveringStates"": [{
                        ""State"": ""Boom"",
                        ""Trend"": 0
                    }]
                },
                {
                    ""Name"": ""People's MET 20 Liberals"",
                    ""FactionState"": ""Boom"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.092445,
                    ""Allegiance"": ""Federation""
                },
                {
                    ""Name"": ""Pilots Federation Local Branch"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.000000,
                    ""Allegiance"": ""PilotsFederation""
                },
                {
                    ""Name"": ""Natural Diaguandri Regulatory State"",
                    ""FactionState"": ""CivilWar"",
                    ""Government"": ""Dictatorship"",
                    ""Influence"": 0.009940,
                    ""Allegiance"": ""Independent""
                },
                {
                    ""Name"": ""Cartel of Diaguandri"",
                    ""FactionState"": ""CivilWar"",
                    ""Government"": ""Anarchy"",
                    ""Influence"": 0.009940,
                    ""Allegiance"": ""Independent"",
                    ""PendingStates"": [{
                        ""State"": ""Bust"",
                        ""Trend"": 0
                    }]
                },
                {
                    ""Name"": ""Revolutionary Party of Diaguandri"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.050696,
                    ""Allegiance"": ""Federation"",
                    ""PendingStates"": [{
                        ""State"": ""Bust"",
                        ""Trend"": 1
                    }]
                },
                {
                    ""Name"": ""The Brotherhood of the Dark Circle"",
                    ""FactionState"": ""Election"",
                    ""Government"": ""Corporate"",
                    ""Influence"": 0.078529,
                    ""Allegiance"": ""Independent"",
                    ""PendingStates"": [{
                        ""State"": ""CivilUnrest"",
                        ""Trend"": 0
                    }],
                    ""RecoveringStates"": [{
                        ""State"": ""Boom"",
                        ""Trend"": 0
                    }]
                },
                {
                    ""Name"": ""EXO"",
                    ""FactionState"": ""Boom"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.685885,
                    ""Allegiance"": ""Independent"",
                    ""PendingStates"": [{
                        ""State"": ""Expansion"",
                        ""Trend"": 0
                    }]
                }],
                ""SystemFaction"": {
                    ""Name"": ""EXO"",
                    ""FactionState"": ""Boom""
                }
            }";

            var line2 = @"{
                ""timestamp"": ""2018-07-30T06:07:47Z"",
                ""event"": ""Docked"",
                ""StationName"": ""Ray Gateway"",
                ""StationType"": ""Coriolis"",
                ""StarSystem"": ""Diaguandri"",
                ""SystemAddress"": 670417429889,
                ""MarketID"": 3223343616,
                ""StationFaction"": {
                    ""Name"": ""EXO"",
                    ""FactionState"": ""Boom""
                },
                ""StationGovernment"": ""$government_Democracy;"",
                ""StationGovernment_Localised"": ""Democracy"",
                ""StationServices"": [""Dock"",
                ""Autodock"",
                ""BlackMarket"",
                ""Commodities"",
                ""Contacts"",
                ""Exploration"",
                ""Missions"",
                ""Outfitting"",
                ""CrewLounge"",
                ""Rearm"",
                ""Refuel"",
                ""Repair"",
                ""Shipyard"",
                ""Tuning"",
                ""Workshop"",
                ""MissionsGenerated"",
                ""FlightController"",
                ""StationOperations"",
                ""Powerplay"",
                ""SearchAndRescue"",
                ""MaterialTrader"",
                ""TechBroker""],
                ""StationEconomy"": ""$economy_HighTech;"",
                ""StationEconomy_Localised"": ""HighTech"",
                ""StationEconomies"": [{
                    ""Name"": ""$economy_HighTech;"",
                    ""Name_Localised"": ""HighTech"",
                    ""Proportion"": 0.800000
                },
                {
                    ""Name"": ""$economy_Refinery;"",
                    ""Name_Localised"": ""Refinery"",
                    ""Proportion"": 0.200000
                }],
                ""DistFromStarLS"": 566.487976,
                ""LandingPads"":{ ""Small"":13, ""Medium"":14, ""Large"":5 }
            }";

            var events = JournalMonitor.ParseJournalEntry(line);
            Assert.AreEqual(1, events.Count);
            var @jumpedEvent = (JumpedEvent)events[0];

            events = JournalMonitor.ParseJournalEntry(line2);
            Assert.AreEqual(1, events.Count);
            var @dockedEvent = (DockedEvent)events[0];

            var responder = makeTestEDDNResponder();
            responder.Handle(@jumpedEvent);
            responder.Handle(@dockedEvent);

            // Test that data available from the event is set correctly
            Assert.AreEqual("Diaguandri", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)670417429889, responder.eddnState.Location.systemAddress);
            Assert.AreEqual("Ray Gateway", responder.eddnState.Location.stationName);
            Assert.AreEqual(3223343616, responder.eddnState.Location.marketId);

            // Test metadata not in the event itself but retrieved from memory and confirmed by our local database
            Assert.AreEqual(-41.06250M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-62.15625M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-103.25000M, responder.eddnState.Location.systemZ);
        }

        [TestMethod()]
        public void TestMyReputationDataStripping()
        {
            var line = @"{
                ""timestamp"": ""2018-11-19T01:06:17Z"",
                ""event"": ""Location"",
                ""Docked"": false,
                ""StarSystem"": ""BD+48738"",
                ""SystemAddress"": 908352033466,
                ""StarPos"": [-93.53125, -24.46875, -114.71875],
                ""SystemAllegiance"": ""Independent"",
                ""SystemEconomy"": ""$economy_Extraction;"",
                ""SystemEconomy_Localised"": ""Extraction"",
                ""SystemSecondEconomy"": ""$economy_Industrial;"",
                ""SystemSecondEconomy_Localised"": ""Industrial"",
                ""SystemGovernment"": ""$government_Cooperative;"",
                ""SystemGovernment_Localised"": ""Cooperative"",
                ""SystemSecurity"": ""$SYSTEM_SECURITY_medium;"",
                ""SystemSecurity_Localised"": ""MediumSecurity"",
                ""Population"": 2530147,
                ""Body"": ""LinnaeusEnterprise"",
                ""BodyID"": 28,
                ""BodyType"": ""Station"",
                ""Factions"": [{
                    ""Name"": ""IndependentBD+48738Liberals"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.037000,
                    ""Allegiance"": ""Federation"",
                    ""Happiness"": ""$Faction_HappinessBand2;"",
                    ""Happiness_Localised"": ""Happy"",
                    ""MyReputation"": 0.000000
                },
                {
                    ""Name"": ""PilotsFederationLocalBranch"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Democracy"",
                    ""Influence"": 0.000000,
                    ""Allegiance"": ""PilotsFederation"",
                    ""Happiness"": """",
                    ""MyReputation"": 100.000000
                },
                {
                    ""Name"": ""NewBD+48738Focus"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Dictatorship"",
                    ""Influence"": 0.046000,
                    ""Allegiance"": ""Independent"",
                    ""Happiness"": ""$Faction_HappinessBand2;"",
                    ""Happiness_Localised"": ""Happy"",
                    ""MyReputation"": 0.000000
                },
                {
                    ""Name"": ""BD+48738CrimsonTravelCo"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Corporate"",
                    ""Influence"": 0.032000,
                    ""Allegiance"": ""Independent"",
                    ""Happiness"": ""$Faction_HappinessBand2;"",
                    ""Happiness_Localised"": ""Happy"",
                    ""MyReputation"": 0.000000
                },
                {
                    ""Name"": ""BD+48738CrimsonPosse"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Anarchy"",
                    ""Influence"": 0.010000,
                    ""Allegiance"": ""Independent"",
                    ""Happiness"": ""$Faction_HappinessBand2;"",
                    ""Happiness_Localised"": ""Happy"",
                    ""MyReputation"": 0.000000
                },
                {
                    ""Name"": ""Laniakea"",
                    ""FactionState"": ""None"",
                    ""Government"": ""Cooperative"",
                    ""Influence"": 0.875000,
                    ""Allegiance"": ""Independent"",
                    ""Happiness"": ""$Faction_HappinessBand2;"",
                    ""Happiness_Localised"": ""Happy"",
                    ""MyReputation"": 0.000000,
                    ""PendingStates"": [{
                        ""State"": ""Expansion"",
                        ""Trend"": 0
                    }]
                }],
                ""SystemFaction"": {
                    ""Name"": ""Laniakea"",
                    ""FactionState"": ""None""
                }
            }";

            var data = Deserializtion.DeserializeData(line);
            data = PersonalDataStripper.Strip( data, "Location" );

            if ( data == null )
            {
                Assert.Fail();
            }
            else
            {
                data.TryGetValue( "Factions", out var factionsVal );
                if ( factionsVal == null )
                {
                    Assert.Fail();
                }
                else
                {
                    var factions = (List<object>)factionsVal;
                    foreach ( var faction in factions )
                    {
                        Assert.IsFalse( ( (IDictionary<string, object>)faction ).ContainsKey( "MyReputation" ) );
                    }
                }
            }
        }

        [TestMethod]
        public void TestStripPersonalData()
        {
            // we fail if any key with the value "bad" survives the test
            IDictionary<string, object> data = new Dictionary<string, object>()
            {
                { "good key", "good" },
                { "ActiveFine", "bad" },
                { "CockpitBreach", "bad" },
                { "BoostUsed", "bad" },
                { "FuelLevel", "bad" },
                { "FuelUsed", "bad" },
                { "JumpDist", "bad" },
                { "Wanted", "bad" },
                { "Latitude", "bad" },
                { "Longitude", "bad" },
                {
                    "Factions", new List<object>()
                    {
                        new Dictionary<string, object>()
                        {
                            { "good key", "good"},
                            { "MyReputation", "bad" },
                            { "SquadronFaction", "bad" },
                            { "HappiestSystem", "bad" },
                            { "HomeSystem", "bad" },
                            { "blah_Localised", "bad" },
                        }
                    }
                }
            };

            data = PersonalDataStripper.Strip( data );

            void testKeyValuePair(KeyValuePair<string, object> kvp)
            {
                if (kvp.Value as string == "bad")
                {
                    Assert.Fail($"key '{kvp.Key}' should have been stripped.");
                }
            }

            void testDictionary(IDictionary<string, object> dict)
            {
                foreach (var kvp in dict)
                {
                    var value = kvp.Value;
                    if (value is string)
                    {
                        testKeyValuePair(kvp);
                    }
                    if (value is IList<object>)
                    {
                        IList<object> list = value as List<object>;
                        Assert.IsNotNull(list);
                        foreach (var item in list)
                        {
                            testDictionary((IDictionary<string, object>)item);
                        }
                    }
                }
            }

            testDictionary(data);
        }

        [TestMethod, DoNotParallelize]
        public void TestRawJournalEventLocationData()
        {
            var location = @"{ ""timestamp"":""2018-12-16T20:08:31Z"", ""event"":""Location"", ""Docked"":false, ""StarSystem"":""Pleiades Sector GW-W c1-4"", ""SystemAddress"":1183229809290, ""StarPos"":[-81.62500,-151.31250,-383.53125], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""Body"":""Pleiades Sector GW-W c1-4"", ""BodyID"":0, ""BodyType"":""Star"" }";
            var jump = @"{ ""timestamp"":""2018-12-16T20:10:15Z"", ""event"":""FSDJump"", ""StarSystem"":""Pleiades Sector HR-W d1-79"", ""SystemAddress"":2724879894859, ""StarPos"":[-80.62500,-146.65625,-343.25000], ""SystemAllegiance"":""Independent"", ""SystemEconomy"":""$economy_Extraction;"", ""SystemEconomy_Localised"":""Extraction"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_Prison;"", ""SystemGovernment_Localised"":""Detention Centre"", ""SystemSecurity"":""$SYSTEM_SECURITY_high;"", ""SystemSecurity_Localised"":""High Security"", ""Population"":0, ""JumpDist"":40.562, ""FuelUsed"":2.827265, ""FuelLevel"":11.702736, ""Factions"":[ { ""Name"":""Independent Detention Foundation"", ""FactionState"":""None"", ""Government"":""Prison"", ""Influence"":0.000000, ""Allegiance"":""Independent"", ""Happiness"":""$Faction_HappinessBand2;"", ""Happiness_Localised"":""Happy"", ""MyReputation"":0.000000 }, { ""Name"":""Pilots Federation Local Branch"", ""FactionState"":""None"", ""Government"":""Democracy"", ""Influence"":0.000000, ""Allegiance"":""PilotsFederation"", ""Happiness"":"""", ""MyReputation"":100.000000 } ], ""SystemFaction"":""Independent Detention Foundation"" }";
            var scan = @"{ ""timestamp"":""2018-12-16T20:10:21Z"", ""event"":""Scan"", ""ScanType"":""AutoScan"", ""StarSystem"":""Pleiades Sector HR-W d1-79"", ""SystemAddress"":2724879894859, ""BodyName"":""Pleiades Sector HR-W d1-79"", ""BodyID"":0, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""F"", ""StellarMass"":1.437500, ""Radius"":855515008.000000, ""AbsoluteMagnitude"":3.808395, ""Age_MY"":1216, ""SurfaceTemperature"":6591.000000, ""Luminosity"":""Vab"", ""RotationPeriod"":261918.156250, ""AxialTilt"":0.000000 }";
            var scan2 = @"{ ""timestamp"":""2018-12-16T20:28:02Z"", ""event"":""Scan"", ""ScanType"":""Detailed"", ""StarSystem"":""Hyades Sector DL-X b1-2"", ""SystemAddress"":5068463809865, ""BodyName"":""Hyades Sector DL-X b1-2"", ""BodyID"":0, ""DistanceFromArrivalLS"":0.000000, ""StarType"":""M"", ""StellarMass"":0.367188, ""Radius"":370672928.000000, ""AbsoluteMagnitude"":9.054306, ""Age_MY"":586, ""SurfaceTemperature"":2993.000000, ""Luminosity"":""Va"", ""RotationPeriod"":167608.859375, ""AxialTilt"":0.000000, ""Rings"":[ { ""Name"":""Hyades Sector DL-X b1-2 A Belt"", ""RingClass"":""eRingClass_MetalRich"", ""MassMT"":5.4671e+13, ""InnerRad"":7.1727e+08, ""OuterRad"":1.728e+09 }, { ""Name"":""Hyades Sector DL-X b1-2 B Belt"", ""RingClass"":""eRingClass_Icy"", ""MassMT"":8.7822e+14, ""InnerRad"":6.3166e+10, ""OuterRad"":1.5917e+11 } ] }";
            var jump2 = @"{ ""timestamp"":""2019-01-27T07:23:38Z"", ""event"":""FSDJump"", ""StarSystem"":""Omega Sector OO-G a11-0"", ""SystemAddress"":5213552532472, ""StarPos"":[-1433.53125,-94.15625,5326.34375], ""SystemAllegiance"":"""", ""SystemEconomy"":""$economy_None;"", ""SystemEconomy_Localised"":""None"", ""SystemSecondEconomy"":""$economy_None;"", ""SystemSecondEconomy_Localised"":""None"", ""SystemGovernment"":""$government_None;"", ""SystemGovernment_Localised"":""None"", ""SystemSecurity"":""$GAlAXY_MAP_INFO_state_anarchy;"", ""SystemSecurity_Localised"":""Anarchy"", ""Population"":0, ""JumpDist"":56.848, ""FuelUsed"":4.741170, ""FuelLevel"":21.947533 }";
            var scan3 = @"{""timestamp"":""2019-01-27T07:07:46Z"",""event"":""Scan"",""ScanType"":""AutoScan"", ""StarSystem"":""Omega Sector DM-M b7-16"", ""SystemAddress"":35835461971465, ""BodyName"":""Omega Sector DM-M b7-16 A B Belt Cluster 8"",""BodyID"":23,""Parents"":[{""Ring"":15},{""Star"":1},{""Null"":0}],""DistanceFromArrivalLS"":646.57074}";

            var responder = makeTestEDDNResponder();

            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "system/2724879894859", @"{""record"":{""allegiance"":""Independent"",""bodies"":[{""distance_to_arrival"":0.0,""estimated_mapping_value"":4641,""estimated_scan_value"":1226,""id"":2724879894859,""id64"":2724879894859,""is_main_star"":true,""name"":""Pleiades Sector HR-W d1-79"",""subtype"":""F (White) Star"",""type"":""Star""},{""distance_to_arrival"":454.386801,""estimated_mapping_value"":49252,""estimated_scan_value"":13556,""id"":36031521898858827,""id64"":36031521898858827,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 1"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":454.154897,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":72060318917822795,""id64"":72060318917822795,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 1 a"",""subtype"":""Rocky body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":803.76456,""estimated_mapping_value"":539455,""estimated_scan_value"":148474,""id"":108089115936786763,""id64"":108089115936786763,""is_main_star"":null,""landmark_value"":1689800,""landmarks"":[{""count"":40,""subtype"":""Bacterium Cerbrus"",""type"":""Bacterium"",""value"":1689800}],""name"":""Pleiades Sector HR-W d1-79 2"",""subtype"":""High metal content world"",""terraforming_state"":""Terraformable"",""type"":""Planet""},{""distance_to_arrival"":3667.60576,""estimated_mapping_value"":49634,""estimated_scan_value"":13661,""id"":144117912955750731,""id64"":144117912955750731,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 3"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":4629.244388,""estimated_mapping_value"":50019,""estimated_scan_value"":13767,""id"":180146709974714699,""id64"":180146709974714699,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 4"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":5983.985215,""estimated_mapping_value"":51712,""estimated_scan_value"":14233,""id"":216175506993678667,""id64"":216175506993678667,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 5"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":5989.290685,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":252204304012642635,""id64"":252204304012642635,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 5 a"",""subtype"":""Rocky body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":9275.474154,""estimated_mapping_value"":49656,""estimated_scan_value"":13667,""id"":288233101031606603,""id64"":288233101031606603,""is_main_star"":null,""name"":""Pleiades Sector HR-W d1-79 6"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""}],""body_count"":9,""estimated_mapping_value"":798811,""estimated_scan_value"":219584,""government"":""Prison"",""id64"":2724879894859,""known_permit"":false,""landmark_value"":1689800,""name"":""Pleiades Sector HR-W d1-79"",""needs_permit"":false,""population"":0,""primary_economy"":""Extraction"",""region"":""Inner Orion Spur"",""secondary_economy"":""None"",""security"":""High"",""stations"":[{""distance_to_arrival"":12.733588,""has_large_pad"":true,""has_market"":true,""market_id"":3704722944,""name"":""K2V-6KM"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3705692672,""medium_pads"":4,""name"":""QBX-36M"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":739.114411,""has_large_pad"":true,""has_market"":true,""market_id"":3701756672,""name"":""K5B-1TN"",""type"":""Drake-Class Carrier""},{""distance_to_arrival"":0.0,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3706688256,""medium_pads"":4,""name"":""T6Z-86N"",""small_pads"":4,""type"":""Drake-Class Carrier""},{""distance_to_arrival"":803.778309,""has_large_pad"":true,""has_outfitting"":true,""has_shipyard"":true,""large_pads"":1,""market_id"":128833431,""medium_pads"":2,""name"":""The Penitent"",""small_pads"":4,""type"":""Mega ship""},{""distance_to_arrival"":803.755358,""has_large_pad"":true,""has_market"":true,""large_pads"":8,""market_id"":3700423936,""medium_pads"":4,""name"":""K5B-W9K"",""small_pads"":4,""type"":""Drake-Class Carrier""}],""thargoid_war_failure_state"":""None"",""thargoid_war_state"":""None"",""thargoid_war_success_state"":""None"",""updated_at"":""2025-01-04 21:25:17+00"",""x"":-80.625,""y"":-146.65625,""z"":-343.25}}" );
            fakeSpanshRestClient.Expect( "system/35835461971465", @"{""record"":{""bodies"":[{""distance_to_arrival"":0.0,""estimated_mapping_value"":4578,""estimated_scan_value"":1207,""id"":36064632480935433,""id64"":36064632480935433,""is_main_star"":true,""name"":""Omega Sector DM-M b7-16 A"",""subtype"":""M (Red dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":189794.564566,""estimated_mapping_value"":4571,""estimated_scan_value"":1205,""id"":108122226518863369,""id64"":108122226518863369,""is_main_star"":false,""name"":""Omega Sector DM-M b7-16 B"",""subtype"":""M (Red dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":190048.367022,""estimated_mapping_value"":4561,""estimated_scan_value"":1202,""id"":216208617575755273,""id64"":216208617575755273,""is_main_star"":false,""name"":""Omega Sector DM-M b7-16 C"",""subtype"":""L (Brown dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":190047.00114,""estimated_mapping_value"":4558,""estimated_scan_value"":1201,""id"":252237414594719241,""id64"":252237414594719241,""is_main_star"":false,""name"":""Omega Sector DM-M b7-16 D"",""subtype"":""T (Brown dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":190061.854838,""estimated_mapping_value"":4554,""estimated_scan_value"":1200,""id"":288266211613683209,""id64"":288266211613683209,""is_main_star"":false,""name"":""Omega Sector DM-M b7-16 E"",""subtype"":""T (Brown dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":10.308674,""estimated_mapping_value"":51348,""estimated_scan_value"":14133,""id"":504438993727467017,""id64"":504438993727467017,""is_main_star"":null,""landmark_value"":0,""landmarks"":[{""count"":1,""subtype"":""Silicate Magma Lava Spout"",""type"":""Lava Spout"",""value"":0}],""name"":""Omega Sector DM-M b7-16 A 1"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1173.491233,""estimated_mapping_value"":106212,""estimated_scan_value"":29233,""id"":864726963917106697,""id64"":864726963917106697,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 2"",""subtype"":""Class II gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1171.715883,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":972813354973998601,""id64"":972813354973998601,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 2 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1170.625538,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1044870949011926537,""id64"":1044870949011926537,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 2 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1170.638765,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1080899746030890505,""id64"":1080899746030890505,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 2 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1176.959369,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1116928543049854473,""id64"":1116928543049854473,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 2 d"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1176.861614,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1152957340068818441,""id64"":1152957340068818441,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 2 e"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1782.305279,""estimated_mapping_value"":3701,""estimated_scan_value"":944,""id"":1188986137087782409,""id64"":1188986137087782409,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3"",""subtype"":""Gas giant with water-based life"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1781.645737,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1225014934106746377,""id64"":1225014934106746377,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1782.880546,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1261043731125710345,""id64"":1261043731125710345,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1776.066994,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1297072528144674313,""id64"":1297072528144674313,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1776.547189,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1369130122182602249,""id64"":1369130122182602249,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 d"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1776.516999,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1405158919201566217,""id64"":1405158919201566217,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 e"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1771.433217,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1477216513239494153,""id64"":1477216513239494153,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 f"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1771.395844,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1513245310258458121,""id64"":1513245310258458121,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 g"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1769.156348,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1549274107277422089,""id64"":1549274107277422089,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 h"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1785.692608,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1585302904296386057,""id64"":1585302904296386057,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 3 i"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":3062.411917,""estimated_mapping_value"":14688,""estimated_scan_value"":4043,""id"":1621331701315350025,""id64"":1621331701315350025,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 4"",""subtype"":""Class I gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":3061.343515,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1729418092372241929,""id64"":1729418092372241929,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 4 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":3062.752169,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1765446889391205897,""id64"":1765446889391205897,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 4 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":3063.665569,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1801475686410169865,""id64"":1801475686410169865,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 4 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":3066.873318,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1837504483429133833,""id64"":1837504483429133833,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 4 d"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":3065.713107,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1873533280448097801,""id64"":1873533280448097801,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 A 4 e"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189799.226866,""estimated_mapping_value"":44024,""estimated_scan_value"":12117,""id"":2053677265542917641,""id64"":2053677265542917641,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 B 1"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189788.942423,""estimated_mapping_value"":43784,""estimated_scan_value"":12051,""id"":2089706062561881609,""id64"":2089706062561881609,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 B 2"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189784.048799,""estimated_mapping_value"":45986,""estimated_scan_value"":12657,""id"":2125734859580845577,""id64"":2125734859580845577,""is_main_star"":null,""landmark_value"":1471900,""landmarks"":[{""count"":2,""subtype"":""Bark Mounds"",""type"":""Bark Mounds"",""value"":1471900}],""name"":""Omega Sector DM-M b7-16 B 3"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189821.475661,""estimated_mapping_value"":44431,""estimated_scan_value"":12229,""id"":2197792453618773513,""id64"":2197792453618773513,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 B 4"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189821.442312,""estimated_mapping_value"":43933,""estimated_scan_value"":12092,""id"":2233821250637737481,""id64"":2233821250637737481,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 B 5"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189773.188914,""estimated_mapping_value"":44115,""estimated_scan_value"":12142,""id"":2269850047656701449,""id64"":2269850047656701449,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 B 6"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":190531.02806,""estimated_mapping_value"":16582,""estimated_scan_value"":4564,""id"":2305878844675665417,""id64"":2305878844675665417,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 1"",""subtype"":""Class I gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189393.578125,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2377936438713593353,""id64"":2377936438713593353,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 1 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189392.65625,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2413965235732557321,""id64"":2413965235732557321,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 1 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189393.453125,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2449994032751521289,""id64"":2449994032751521289,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 1 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":189811.73456,""estimated_mapping_value"":11781,""estimated_scan_value"":3243,""id"":2486022829770485257,""id64"":2486022829770485257,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 2"",""subtype"":""Class I gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":188931.46875,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2594109220827377161,""id64"":2594109220827377161,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 2 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":188931.078125,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2630138017846341129,""id64"":2630138017846341129,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 2 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":188936.40625,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2666166814865305097,""id64"":2666166814865305097,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 2 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":188926.046875,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2738224408903233033,""id64"":2738224408903233033,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 2 d"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":188926.0625,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":2774253205922197001,""id64"":2774253205922197001,""is_main_star"":null,""name"":""Omega Sector DM-M b7-16 BCDE 2 e"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""}],""estimated_mapping_value"":553374,""estimated_scan_value"":148963,""government"":""None"",""id64"":35835461971465,""known_permit"":false,""landmark_value"":1471900,""name"":""Omega Sector DM-M b7-16"",""needs_permit"":false,""population"":0,""primary_economy"":""None"",""region"":""Inner Orion Spur"",""secondary_economy"":""None"",""security"":""Anarchy"",""thargoid_war_failure_state"":""None"",""thargoid_war_state"":""None"",""thargoid_war_success_state"":""None"",""updated_at"":""2020-12-04 23:21:03+00"",""x"":-1477.6875,""y"":-105.65625,""z"":5360.25}}" );
            fakeSpanshRestClient.Expect( "system/5068463809865", @"{""record"":{""bodies"":[{""distance_to_arrival"":0.0,""estimated_mapping_value"":4574,""estimated_scan_value"":1206,""id"":5068463809865,""id64"":5068463809865,""is_main_star"":true,""name"":""Hyades Sector DL-X b1-2"",""subtype"":""M (Red dwarf) Star"",""type"":""Star""},{""distance_to_arrival"":7.432798,""estimated_mapping_value"":44536,""estimated_scan_value"":12258,""id"":180149053558629705,""id64"":180149053558629705,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 1"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":12.696543,""estimated_mapping_value"":51178,""estimated_scan_value"":14086,""id"":216177850577593673,""id64"":216177850577593673,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 2"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":21.976877,""estimated_mapping_value"":49354,""estimated_scan_value"":13584,""id"":252206647596557641,""id64"":252206647596557641,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 3"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":38.138871,""estimated_mapping_value"":52733,""estimated_scan_value"":14514,""id"":288235444615521609,""id64"":288235444615521609,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 4"",""subtype"":""High metal content world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":70.811433,""estimated_mapping_value"":376696,""estimated_scan_value"":103678,""id"":360293038653449545,""id64"":360293038653449545,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 5"",""subtype"":""Water world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":70.372731,""estimated_mapping_value"":368647,""estimated_scan_value"":101463,""id"":396321835672413513,""id64"":396321835672413513,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 6"",""subtype"":""Water world"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":121.193351,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":432350632691377481,""id64"":432350632691377481,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 7"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":843.775461,""estimated_mapping_value"":17585,""estimated_scan_value"":4840,""id"":792638602881017161,""id64"":792638602881017161,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 8"",""subtype"":""Class I gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":843.973713,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":864696196918945097,""id64"":864696196918945097,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 8 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":845.790295,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":900724993937909065,""id64"":900724993937909065,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 8 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":839.797623,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":936753790956873033,""id64"":936753790956873033,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 8 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":849.5578,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":972782587975837001,""id64"":972782587975837001,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 8 d"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":850.546434,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1008811384994800969,""id64"":1008811384994800969,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 8 e"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1217.02067,""estimated_mapping_value"":17315,""estimated_scan_value"":4766,""id"":1188955370089620809,""id64"":1188955370089620809,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9"",""subtype"":""Class I gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1218.769019,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1261012964127548745,""id64"":1261012964127548745,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9 a"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1216.517157,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1297041761146512713,""id64"":1297041761146512713,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9 b"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1211.460201,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1333070558165476681,""id64"":1333070558165476681,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9 c"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1219.368899,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1369099355184440649,""id64"":1369099355184440649,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9 d"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1212.84238,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1441156949222368585,""id64"":1441156949222368585,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9 e"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1212.78229,""estimated_mapping_value"":2221,""estimated_scan_value"":500,""id"":1477185746241332553,""id64"":1477185746241332553,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 9 f"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":1497.578075,""estimated_mapping_value"":10968,""estimated_scan_value"":3019,""id"":1585272137298224457,""id64"":1585272137298224457,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 10"",""subtype"":""Class I gas giant"",""terraforming_state"":""Not terraformable"",""type"":""Planet""},{""distance_to_arrival"":2182.543554,""estimated_mapping_value"":2268,""estimated_scan_value"":514,""id"":1801444919412008265,""id64"":1801444919412008265,""is_main_star"":null,""name"":""Hyades Sector DL-X b1-2 11"",""subtype"":""Icy body"",""terraforming_state"":""Not terraformable"",""type"":""Planet""}],""body_count"":23,""estimated_mapping_value"":1022506,""estimated_scan_value"":279928,""government"":""None"",""id64"":5068463809865,""known_permit"":false,""landmark_value"":0,""name"":""Hyades Sector DL-X b1-2"",""needs_permit"":false,""population"":0,""primary_economy"":""None"",""region"":""Inner Orion Spur"",""secondary_economy"":""None"",""security"":""Anarchy"",""thargoid_war_failure_state"":""None"",""thargoid_war_state"":""None"",""thargoid_war_success_state"":""None"",""updated_at"":""2025-01-04 15:00:45+00"",""x"":-43.6875,""y"":-99.21875,""z"":-237.8125}}" );

            var unhandledLocation = Deserializtion.DeserializeData(location);
            responder.eddnState.Location.GetLocationInfo( "Location", unhandledLocation );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "Location", unhandledLocation ) );
            Assert.AreEqual("Pleiades Sector GW-W c1-4", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)1183229809290, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(-81.62500M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-151.31250M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-383.53125M, responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            var unhandledJump = Deserializtion.DeserializeData(jump);
            responder.eddnState.Location.GetLocationInfo( "FSDJump", unhandledJump );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "FSDJump", unhandledJump ) );
            Assert.AreEqual("Pleiades Sector HR-W d1-79", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)2724879894859, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(-80.62500M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-146.65625M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-343.25000M, responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            var unhandledScan = Deserializtion.DeserializeData(scan);
            responder.eddnState.Location.GetLocationInfo( "Scan", unhandledScan );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "Scan", unhandledScan ) );
            Assert.AreEqual("Pleiades Sector HR-W d1-79", responder.eddnState.Location.systemName);
            Assert.AreEqual((ulong)2724879894859, responder.eddnState.Location.systemAddress);
            Assert.AreEqual(-80.62500M, responder.eddnState.Location.systemX);
            Assert.AreEqual(-146.65625M, responder.eddnState.Location.systemY);
            Assert.AreEqual(-343.25000M, responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            // Deliberately scan a procedurally generated body that doesn't match our last known location & verify heuristics catch it
            var unhandledScan2 = Deserializtion.DeserializeData(scan2);
            responder.eddnState.Location.GetLocationInfo( "Scan", unhandledScan2 );
            Assert.IsFalse( responder.eddnState.Location.CheckLocationData( "Scan", unhandledScan2 ) );
            Assert.AreEqual( "Hyades Sector DL-X b1-2", responder.eddnState.Location.systemName);
            Assert.AreEqual( 5068463809865U, responder.eddnState.Location.systemAddress );
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
            Assert.IsNull(responder.eddnState.Location.stationName);
            Assert.IsNull(responder.eddnState.Location.marketId);

            // Reset our position by re-stating the `FSDJump` event
            responder.eddnState.Location.GetLocationInfo( "FSDJump", unhandledJump );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "FSDJump", unhandledJump ) );

            // Deliberately create a mismatch between the system and coordinates, 
            // using the coordinates from our Location event and other data from our FSDJump event
            responder.eddnState.Location.systemX = -81.62500M;
            responder.eddnState.Location.systemY = -151.31250M;
            responder.eddnState.Location.systemZ = -383.53125M;

            // Deliberately scan a body while our coordinates are in a bad state
            responder.eddnState.Location.GetLocationInfo( "Scan", unhandledScan );
            Assert.IsFalse( responder.eddnState.Location.CheckLocationData( "Scan", unhandledScan ) );
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);

            // Reset our position by re-stating the `FSDJump` event
            responder.eddnState.Location.GetLocationInfo( "FSDJump", unhandledJump );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "FSDJump", unhandledJump ) );

            // Deliberately create a mismatch between the system name and address, 
            // using the system name from our Location event and other data from our FSDJump event
            responder.eddnState.Location.systemName = "Pleiades Sector GW-W c1-4";

            // Verify that a scan corrects our bad system name
            responder.eddnState.Location.GetLocationInfo( "Scan", unhandledScan );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "Scan", unhandledScan ) );
            Assert.AreEqual( "Pleiades Sector HR-W d1-79", responder.eddnState.Location.systemName );

            // Set ourselves to a new position using another `FSDJump` event
            var unhandledJump2 = Deserializtion.DeserializeData(jump2);
            responder.eddnState.Location.GetLocationInfo( "FSDJump", unhandledJump2 );
            Assert.IsTrue( responder.eddnState.Location.CheckLocationData( "FSDJump", unhandledJump2 ) );

            // Scan a belt cluster from a different star system
            var unhandledScan3 = Deserializtion.DeserializeData(scan3);
            responder.eddnState.Location.GetLocationInfo( "Scan", unhandledScan3 );
            Assert.IsFalse( responder.eddnState.Location.CheckLocationData( "Scan", unhandledScan3 ) );
            Assert.AreEqual( 35835461971465U, responder.eddnState.Location.systemAddress );
            Assert.IsNull(responder.eddnState.Location.systemX);
            Assert.IsNull(responder.eddnState.Location.systemY);
            Assert.IsNull(responder.eddnState.Location.systemZ);
            Assert.AreEqual( "Omega Sector DM-M b7-16", responder.eddnState.Location.systemName );
        }

        [ TestMethod ]
        public void commoditySchemaJournalTest ()
        {
            // Set up our schema
            var commoditySchema = makeTestEDDNResponder().schemas
                .FirstOrDefault( s => s.GetType() == typeof(CommoditySchema) );
            Assert.IsNotNull( commoditySchema );

            // Set up our initial conditions
            var marketData = Deserializtion.DeserializeData( DeserializeJsonResource<string>( Resources.market ) );
            var eddnState = new EDDNState();

            // Check a few items on our initial data
            Assert.AreEqual( "2020-08-07T17:17:10Z", Dates.FromDateTimeToString( marketData[ "timestamp" ] as DateTime? ) );
            Assert.AreEqual( 3702012928, marketData[ "MarketID" ] as long? );
            Assert.AreEqual( "all", marketData[ "CarrierDockingAccess" ] as string );
            var marketItems = ( marketData[ "Items" ] as IEnumerable<object> );
            Assert.IsNotNull( marketItems );
            Assert.AreEqual( 6, marketItems.Count() );
            if ( marketData[ "Items" ] is List<object> items )
            {
                if ( items[ 0 ] is IDictionary<string, object> item )
                {
                    Assert.AreEqual( 15, item.Keys.Count );
                    Assert.AreEqual( "Painite", item[ "Name_Localised" ] as string );
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue( commoditySchema.Handle( "Market", ref marketData, eddnState ) );

            // Validate the final data
            Assert.AreEqual( "2020-08-07T17:17:10Z", Dates.FromDateTimeToString( marketData[ "timestamp" ] as DateTime? ) );
            Assert.AreEqual( 3702012928, marketData[ "marketId" ] as long? );
            Assert.IsFalse( marketData.ContainsKey( "CarrierDockingAccess" ) );
            var marketCommodities = ( marketData[ "commodities" ] as IEnumerable<object> );
            Assert.IsNotNull( marketCommodities );
            Assert.AreEqual( 6, marketCommodities.Count() );
            if ( marketData[ "commodities" ] is List<JObject> handledItems )
            {
                if ( handledItems[ 0 ] is JObject item )
                {
                    Assert.IsFalse( item.ContainsKey( "id" ) );
                    Assert.IsFalse( item.ContainsKey( "Name_Localised" ) );
                    Assert.IsFalse( item.ContainsKey( "Category" ) );
                    Assert.IsFalse( item.ContainsKey( "Category_Localised" ) );
                    Assert.IsFalse( item.ContainsKey( "Consumer" ) );
                    Assert.IsFalse( item.ContainsKey( "Producer" ) );
                    Assert.IsFalse( item.ContainsKey( "Rare" ) );

                    Assert.AreEqual( 9, item.Count );
                    Assert.IsNotNull( item[ "name" ] );
                    Assert.AreEqual( "painite", item[ "name" ].ToString() );
                    Assert.IsNotNull( item[ "buyPrice" ] );
                    Assert.AreEqual( 0, item[ "buyPrice" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "sellPrice" ] );
                    Assert.AreEqual( 500096, item[ "sellPrice" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "meanPrice" ] );
                    Assert.AreEqual( 0, item[ "meanPrice" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "stockBracket" ] );
                    Assert.AreEqual( 0, item[ "stockBracket" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "demandBracket" ] );
                    Assert.AreEqual( 2, item[ "demandBracket" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "stock" ] );
                    Assert.AreEqual( 0, item[ "stock" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "demand" ] );
                    Assert.AreEqual( 200, item[ "demand" ].ToObject<int?>() );
                    Assert.IsNotNull( item[ "statusFlags" ] );
                    Assert.AreEqual( 1, item[ "statusFlags" ].ToObject<JArray>().Count );
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void commoditySchemaCapiTest()
        {
            // Set up our schema
            var commoditySchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(CommoditySchema));

            // Set up our initial conditions
            var profileJson = JObject.Parse(@"{""lastSystem"":{""id"":99999,""name"":""Oresqu"",""faction"":""independent""}}");
            var marketJson = DeserializeJsonResource<JObject>(Resources.capi_market_Libby_Horizons);
            var eddnState = new EDDNState();

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(marketJson["timestamp"]?.ToObject<DateTime?>()));
            Assert.IsNotNull( marketJson[ "id" ] );
            Assert.AreEqual(3228854528, marketJson["id"].ToObject<long?>());
            Assert.IsNotNull( marketJson[ "outpostType" ] );
            Assert.AreEqual("starport", marketJson["outpostType"].ToString());
            var commodities = marketJson[ "commodities" ];
            Assert.IsNotNull(commodities);
            Assert.AreEqual(117, commodities.Count());
            if (commodities.ToObject<JArray>() is JArray items)
            {
                if (items[0] is JToken item)
                {
                    Assert.AreEqual(13, item.Count());
                    Assert.AreEqual("Agronomic Treatment", (string)item["locName"]);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var handledData = commoditySchema?.Handle(profileJson, marketJson, new JObject(), new JObject(), eddnState);
            Assert.IsNotNull(handledData);

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString((DateTime)handledData["timestamp"]));
            Assert.AreEqual(3228854528, handledData["marketId"] as long?);
            Assert.IsFalse(handledData.ContainsKey("outpostType"));
            Assert.IsTrue(handledData["economies"] is IEnumerable<object>);
            if (handledData["commodities"] is List<object> handledItems)
            {
                Assert.AreEqual( 116, handledItems.Count );
                if ( handledItems[0] is Dictionary<string, object> item)
                {
                    Assert.IsFalse(item.ContainsKey("id"));
                    Assert.IsFalse(item.ContainsKey("locName"));
                    Assert.IsFalse(item.ContainsKey("categoryname"));
                    Assert.IsFalse(item.ContainsKey("legality"));

                    Assert.AreEqual(8, item.Count);
                    Assert.AreEqual("AgronomicTreatment", item["name"].ToString());
                    Assert.AreEqual(0, Convert.ToInt32(item["buyPrice"]));
                    Assert.AreEqual(3336, Convert.ToInt32(item["sellPrice"]));
                    Assert.AreEqual(3155, Convert.ToInt32(item["meanPrice"]));
                    Assert.AreEqual(0, Convert.ToInt32(item["stockBracket"]));
                    Assert.AreEqual(2, Convert.ToInt32(item["demandBracket"]));
                    Assert.AreEqual(0, Convert.ToInt32(item["stock"]));
                    Assert.AreEqual(43, Convert.ToInt32(item["demand"]));
                    Assert.IsFalse(item.ContainsKey("statusFlags"));
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void fcmaterialsSchemaJournalTest()
        {
            // Set up our schema
            var fcmaterialsSchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(FCMaterialsSchema));
            Assert.IsNotNull( fcmaterialsSchema );

            // Set up our initial conditions
            var eddnState = new EDDNState();
            var fcmaterialsData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.FCMaterials));

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString(fcmaterialsData["timestamp"] as DateTime?));
            Assert.AreEqual(3709999999, fcmaterialsData["MarketID"] as long?);
            Assert.AreEqual("Station 42", fcmaterialsData["CarrierName"] as string);
            Assert.AreEqual("X9X-9XX", fcmaterialsData["CarrierID"] as string);
            if (fcmaterialsData["Items"] is List<object> items)
            {
                Assert.AreEqual( 2, items.Count );
                if ( items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(6, item.Keys.Count);
                    Assert.AreEqual("Chemical Superbase", item["Name_Localised"] as string);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(fcmaterialsSchema.Handle("FCMaterials", ref fcmaterialsData, eddnState));

            // Validate the final data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString(fcmaterialsData["timestamp"] as DateTime?));
            Assert.AreEqual(3709999999, fcmaterialsData["MarketID"] as long?);
            Assert.AreEqual("Station 42", fcmaterialsData["CarrierName"] as string);
            Assert.AreEqual("X9X-9XX", fcmaterialsData["CarrierID"] as string);
            if (fcmaterialsData["Items"] is List<object> handledItems)
            {
                Assert.AreEqual( 2, handledItems.Count );
                if ( handledItems.FirstOrDefault() is IDictionary<string, object> item)
                {
                    Assert.IsFalse(item.ContainsKey("Name_Localised"));
                    Assert.AreEqual(5, item.Count);
                    Assert.AreEqual(128961528, Convert.ToInt64(item["id"]));
                    Assert.AreEqual("$chemicalsuperbase_name;", item["Name"].ToString());
                    Assert.AreEqual(500, Convert.ToInt32(item["Price"]));
                    Assert.AreEqual(50, Convert.ToInt32(item["Stock"]));
                    Assert.AreEqual(0, Convert.ToInt32(item["Demand"]));
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void fcmaterialsSchemaCapiTest()
        {
            // Set up our schema
            var fcmaterialsSchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(FCMaterialsSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState();
            var fcMarketJson = DeserializeJsonResource<JObject>(Resources.capi_market_fleet_carrier);

            // Check a few items on our initial data
            Assert.IsNotNull( fcMarketJson[ "timestamp" ] );
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString(fcMarketJson["timestamp"].ToObject<DateTime>()));
            Assert.IsNotNull( fcMarketJson[ "id" ] );
            Assert.AreEqual(3709999999, fcMarketJson["id"].ToObject<long>());
            Assert.IsNotNull( fcMarketJson[ "name" ] );
            Assert.AreEqual("X9X-9XX", fcMarketJson["name"].ToString());
            Assert.IsNotNull( fcMarketJson[ "outpostType" ] );
            Assert.AreEqual("fleetcarrier", fcMarketJson["outpostType"].ToString());
            Assert.IsNotNull( fcMarketJson[ "orders" ] );
            var onFootMicroResources = fcMarketJson["orders"]["onfootmicroresources"];
            Assert.IsNotNull( onFootMicroResources );
            Assert.IsNotNull( onFootMicroResources[ "sales" ] );
            Assert.IsNotNull( onFootMicroResources[ "purchases" ] );
            Assert.AreEqual(1, onFootMicroResources["sales"].Children().Count());
            Assert.AreEqual(16, onFootMicroResources["purchases"].Children().Count());
            if (onFootMicroResources["sales"].Children().Values().FirstOrDefault() is JObject item)
            {
                Assert.AreEqual(5, item.Count);
                Assert.IsNotNull( item[ "locName" ] );
                Assert.AreEqual("Graphene", item["locName"].ToString());
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var handledData = fcmaterialsSchema?.Handle(null, fcMarketJson, null, null, eddnState);
            Assert.IsNotNull(handledData);

            // Validate the final data
            Assert.AreEqual("2022-11-08T03:15:30Z", Dates.FromDateTimeToString((DateTime)handledData["timestamp"]));
            Assert.AreEqual(3709999999, handledData["MarketID"] as long?);
            Assert.AreEqual("X9X-9XX", handledData["CarrierID"] as string);
            if (handledData.TryGetValue("Items", out var handledItemsObj) &&
                handledItemsObj is Dictionary<string, object> handledItems)
            {
                var sales = handledItems["sales"] as Dictionary<string, object> ?? new Dictionary<string, object>();
                var purchases = handledItems["purchases"] as Dictionary<string, object> ?? new Dictionary<string, object>();
                Assert.AreEqual(1, sales.Count);
                Assert.AreEqual(0, purchases.Count);
                if (sales["128064021"] is Dictionary<string, object> handledItem)
                {
                    Assert.IsFalse(handledItem.ContainsKey("locName"));

                    Assert.AreEqual(4, handledItem.Count);
                    Assert.AreEqual(128064021, Convert.ToInt64(handledItem["id"]));
                    Assert.AreEqual("graphene", handledItem["name"].ToString());
                    Assert.AreEqual(1300, Convert.ToInt32(handledItem["price"]));
                    Assert.AreEqual(112, Convert.ToInt32(handledItem["stock"]));
                }
                else
                {
                    Assert.Fail();
                }
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void outfittingSchemaJournalTest()
        {
            // Set up our schema
            var outfittingSchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(OutfittingSchema));
            Assert.IsNotNull( outfittingSchema );

            // Set up our initial conditions
            var eddnState = new EDDNState();
            var outfittingData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.Outfitting));

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-21T00:05:07Z", Dates.FromDateTimeToString(outfittingData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, outfittingData["MarketID"] as long?);
            Assert.AreEqual("Walker Ring", outfittingData["StationName"] as string);
            Assert.AreEqual("Gertrud", outfittingData["StarSystem"] as string);
            if (outfittingData["Items"] is List<object> items)
            {
                Assert.AreEqual( 767, items.Count );
                if ( items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(3, item.Keys.Count);
                    Assert.AreEqual("hpt_cannon_gimbal_huge", item["Name"] as string);
                    Assert.AreEqual(128049444, item["id"] as long?);
                    Assert.AreEqual(4476576, item["BuyPrice"] as long?);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(outfittingSchema.Handle("Outfitting", ref outfittingData, eddnState));

            // Validate the final data
            Assert.AreEqual("2022-11-21T00:05:07Z", Dates.FromDateTimeToString(outfittingData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, outfittingData["marketId"] as long?);
            Assert.AreEqual("Walker Ring", outfittingData["stationName"] as string);
            Assert.AreEqual("Gertrud", outfittingData["systemName"] as string);
            if (outfittingData["modules"] is List<string> modules)
            {
                Assert.AreEqual( 767, modules.Count );
                Assert.AreEqual( "hpt_cannon_gimbal_huge", modules[ 0 ] );
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void outfittingSchemaCapiTest()
        {
            // Set up our schema
            var outfittingSchema = makeTestEDDNResponder()
                .capiSchemas.FirstOrDefault(s => s.GetType() == typeof(OutfittingSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState();
            var shipyardJson = DeserializeJsonResource<JObject>(Resources.capi_shipyard_Abasheli_Barracks);

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(shipyardJson["timestamp"]?.ToObject<DateTime?>()));
            Assert.IsNotNull( shipyardJson[ "id" ] );
            Assert.AreEqual(3544236032, shipyardJson["id"].ToObject<long?>());
            Assert.IsNotNull( shipyardJson[ "name" ] );
            Assert.AreEqual("Abasheli Barracks", shipyardJson["name"].ToString());
            Assert.IsFalse(shipyardJson.ContainsKey("StarSystem"));
            var shipyardModules = shipyardJson[ "modules" ] as IEnumerable<object>;
            Assert.IsNotNull( shipyardModules );
            Assert.AreEqual( 165, shipyardModules.Count() );
            if ( shipyardJson["modules"]?.Children().Values().FirstOrDefault() is JObject item)
            {
                Assert.AreEqual(5, item.Count);
                Assert.IsNotNull( item[ "name" ] );
                Assert.AreEqual("Hpt_ATDumbfireMissile_Fixed_Large", item["name"].ToString());
                Assert.IsNotNull( item[ "category" ] );
                Assert.AreEqual("weapon", item["category"].ToString());
                Assert.IsNotNull( item[ "id" ] );
                Assert.AreEqual(128788700, item["id"].ToObject<long?>());
                Assert.IsNotNull( item[ "cost" ] );
                Assert.AreEqual(1352250, item["cost"].ToObject<long?>());
                Assert.IsNotNull( item[ "sku" ] );
                Assert.AreEqual("ELITE_HORIZONS_V_PLANETARY_LANDINGS", item["sku"].ToString());
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            var profileJson = JObject.Parse(@"{""lastSystem"":{""id"":99999,""name"":""Kurigosages"",""faction"":""independent""}}");
            var outfittingData = outfittingSchema?.Handle(profileJson, null, shipyardJson, null, eddnState);
            Assert.IsNotNull(outfittingData);

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(outfittingData["timestamp"] as DateTime?));
            Assert.AreEqual(3544236032, outfittingData["marketId"] as long?);
            Assert.AreEqual("Abasheli Barracks", outfittingData["stationName"] as string);
            Assert.AreEqual("Kurigosages", outfittingData["systemName"] as string);
            if (outfittingData["modules"] is List<string> modules)
            {
                Assert.AreEqual( 164, modules.Count );
                Assert.AreEqual( "Hpt_ATDumbfireMissile_Fixed_Large", modules[ 0 ] );
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void shipyardSchemaJournalTest()
        {
            // Set up our schema
            var shipyardSchema = makeTestEDDNResponder()
                .schemas.FirstOrDefault(s => s.GetType() == typeof(ShipyardSchema));
            Assert.IsNotNull(shipyardSchema);

            // Set up our initial conditions
            var eddnState = new EDDNState();
            var shipyardData = Deserializtion.
                DeserializeData(DeserializeJsonResource<string>(Resources.Shipyard));

            // Check a few items on our initial data
            Assert.AreEqual("2022-11-21T00:04:40Z", Dates.FromDateTimeToString(shipyardData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, shipyardData["MarketID"] as long?);
            Assert.AreEqual("Walker Ring", shipyardData["StationName"] as string);
            Assert.AreEqual("Gertrud", shipyardData["StarSystem"] as string);
            if (shipyardData["PriceList"] is List<object> items)
            {
                Assert.AreEqual( 18, items.Count );
                if ( items[0] is IDictionary<string, object> item)
                {
                    Assert.AreEqual(3, item.Keys.Count);
                    Assert.AreEqual("sidewinder", item["ShipType"] as string);
                    Assert.AreEqual(128049249, item["id"] as long?);
                    Assert.AreEqual(26520, item["ShipPrice"] as long?);
                }
            }
            else
            {
                Assert.Fail();
            }

            // Apply our "Handle" method to transform the data
            Assert.IsTrue(shipyardSchema.Handle("Shipyard", ref shipyardData, eddnState));

            // Validate the final data
            Assert.AreEqual("2022-11-21T00:04:40Z", Dates.FromDateTimeToString(shipyardData["timestamp"] as DateTime?));
            Assert.AreEqual(3227934976, shipyardData["marketId"] as long?);
            Assert.AreEqual("Walker Ring", shipyardData["stationName"] as string);
            Assert.AreEqual("Gertrud", shipyardData["systemName"] as string);
            if (shipyardData["ships"] is List<string> ships)
            {
                Assert.AreEqual( 18, ships.Count );
                Assert.AreEqual( "sidewinder", ships[ 0 ] );
            }
            else
            {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void shipyardSchemaCapiTest()
        {
            // Set up our schema
            var shipyardSchema = makeTestEDDNResponder().capiSchemas.FirstOrDefault(s => s.GetType() == typeof(ShipyardSchema));

            // Set up our initial conditions
            var eddnState = new EDDNState();
            var shipyardJson = DeserializeJsonResource<JObject>(Resources.capi_shipyard_Abasheli_Barracks);

            // Check a few items on our initial data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(shipyardJson["timestamp"]?.ToObject<DateTime?>()));
            Assert.IsNotNull( shipyardJson[ "id" ] );
            Assert.AreEqual(3544236032, shipyardJson["id"].ToObject<long?>());
            Assert.IsNotNull( shipyardJson[ "name" ] );
            Assert.AreEqual("Abasheli Barracks", shipyardJson["name"].ToString());
            Assert.IsFalse(shipyardJson.ContainsKey("StarSystem"));
            if (shipyardJson["ships"]?["shipyard_list"] is JToken shipyardShips && shipyardShips.Children().Values().FirstOrDefault() is JObject shipyardShip )
            {
                Assert.AreEqual( 5, shipyardShips.Count() );
                Assert.IsNotNull( shipyardShip );
                Assert.IsNotNull( shipyardShip[ "name" ] );
                Assert.IsNotNull( shipyardShip[ "id"] );
                Assert.IsNotNull( shipyardShip[ "basevalue" ] );
                Assert.IsNotNull( shipyardShip[ "sku" ] );
                Assert.AreEqual( "Eagle", shipyardShip[ "name" ].ToString() );
                Assert.AreEqual( 128049255, shipyardShip[ "id" ].ToObject<long?>() );
                Assert.AreEqual( 44800, shipyardShip[ "basevalue" ].ToObject<long?>() );
                Assert.AreEqual( "", shipyardShip[ "sku" ].ToString() );
            }
            else
            {
                Assert.Fail();
            }
            var unavailableShipyardShips = shipyardJson[ "ships" ]?[ "unavailable_list" ] as IEnumerable<object>;
            Assert.IsNotNull( unavailableShipyardShips );
            Assert.AreEqual( 3, unavailableShipyardShips.Count() );

            // Apply our "Handle" method to transform the data
            var profileJson = JObject.Parse(@"{""lastSystem"":{""id"":99999,""name"":""Kurigosages"",""faction"":""independent""}}");
            var shipyardData = shipyardSchema?.Handle(profileJson, null, shipyardJson, null, eddnState);
            Assert.IsNotNull(shipyardData);

            // Validate the final data
            Assert.AreEqual("2020-08-07T17:17:10Z", Dates.FromDateTimeToString(shipyardData["timestamp"] as DateTime?));
            Assert.AreEqual(3544236032, shipyardData["marketId"] as long?);
            Assert.AreEqual("Abasheli Barracks", shipyardData["stationName"] as string);
            Assert.AreEqual("Kurigosages", shipyardData["systemName"] as string);
            Assert.IsFalse(shipyardData["allowCobraMkIV"] as bool? ?? false);
            if (shipyardData["ships"] is List<string> ships)
            {
                Assert.AreEqual( 8, ships.Count );
                Assert.AreEqual( "Eagle", ships[ 0 ] );
            }
            else
            {
                Assert.Fail();
            }
        }
    }
}

