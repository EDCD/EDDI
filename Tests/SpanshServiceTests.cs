using EddiCore;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text;
using Tests.Properties;

namespace UnitTests
{
    [TestClass]
    public class SpanshServiceTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestSpanshCarrierRoute()
        {
            // Arrange
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=NLTT 13249", @"{""min_max"":[{""id64"":2869440882065,""name"":""NLTT 13249"",""x"":-38.8125,""y"":9.3125,""z"":-60.53125},{""id64"":44853758275,""name"":""HIP 13249"",""x"":71.0,""y"":-646.5625,""z"":-407.34375},{""id64"":216669342059,""name"":""NLTT 14641"",""x"":97.625,""y"":-65.1875,""z"":35.03125},{""id64"":83449746098,""name"":""NLTT 2408"",""x"":-259.1875,""y"":-147.84375,""z"":-161.75},{""id64"":908553360066,""name"":""NLTT 16391"",""x"":42.90625,""y"":-8.65625,""z"":-81.875},{""id64"":908620468962,""name"":""NLTT 35146"",""x"":86.65625,""y"":9.75,""z"":74.9375},{""id64"":1041269524835,""name"":""NLTT 18977"",""x"":-43.9375,""y"":49.78125,""z"":-64.34375},{""id64"":1458309108442,""name"":""NLTT 54244"",""x"":42.4375,""y"":-68.6875,""z"":52.15625},{""id64"":1733187048146,""name"":""NLTT 11599"",""x"":48.78125,""y"":-62.09375,""z"":-11.71875},{""id64"":670148273569,""name"":""NLTT 1796"",""x"":-55.28125,""y"":-279.625,""z"":-13.78125},{""id64"":670149059985,""name"":""NLTT 6667"",""x"":-49.8125,""y"":-33.4375,""z"":-55.28125},{""id64"":670149125569,""name"":""NLTT 48288"",""x"":-60.4375,""y"":-13.09375,""z"":56.3125},{""id64"":2282942829266,""name"":""NLTT 8653"",""x"":44.15625,""y"":-101.03125,""z"":-20.03125},{""id64"":2868904076721,""name"":""NLTT 46709"",""x"":-65.3125,""y"":25.875,""z"":23.6875},{""id64"":2869709579681,""name"":""NLTT 30929"",""x"":-8.3125,""y"":84.65625,""z"":-12.25},{""id64"":2870782731705,""name"":""NLTT 4671"",""x"":69.34375,""y"":-96.125,""z"":36.125},{""id64"":3107442332362,""name"":""NLTT 6637"",""x"":-25.3125,""y"":-82.96875,""z"":-52.90625},{""id64"":7267487393161,""name"":""NLTT 14879"",""x"":-31.71875,""y"":14.0,""z"":-67.5},{""id64"":3382454555338,""name"":""NLTT 19808"",""x"":46.09375,""y"":26.78125,""z"":-43.71875},{""id64"":2832698741474,""name"":""NLTT 40287"",""x"":38.40625,""y"":33.78125,""z"":92.75}],""values"":[""NLTT 13249"",""HIP 13249"",""NLTT 14641"",""NLTT 2408"",""NLTT 16391"",""NLTT 35146"",""NLTT 18977"",""NLTT 54244"",""NLTT 11599"",""NLTT 1796"",""NLTT 6667"",""NLTT 48288"",""NLTT 8653"",""NLTT 46709"",""NLTT 30929"",""NLTT 4671"",""NLTT 6637"",""NLTT 14879"",""NLTT 19808"",""NLTT 40287""]}" );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=Sagittarius A*", @"{""min_max"":[{""id64"":20578934,""name"":""Sagittarius A*"",""x"":25.21875,""y"":-20.90625,""z"":25899.96875},{""id64"":9955165356,""name"":""A Velorum"",""x"":2116.375,""y"":-191.90625,""z"":-168.96875},{""id64"":1356850348,""name"":""a Velorum"",""x"":1847.15625,""y"":-55.78125,""z"":-138.625},{""id64"":359334384322,""name"":""a Puppis"",""x"":339.25,""y"":-40.8125,""z"":-86.96875},{""id64"":3386376805,""name"":""a Centauri"",""x"":264.9375,""y"":153.375,""z"":339.90625},{""id64"":669074466345,""name"":""A Capricorni"",""x"":-132.375,""y"":-296.8125,""z"":320.125},{""id64"":57153684668,""name"":""A Cen"",""x"":391.9375,""y"":50.3125,""z"":160.03125},{""id64"":10645195219,""name"":""a Lupi"",""x"":806.1875,""y"":288.71875,""z"":1026.40625},{""id64"":2007863857882,""name"":""A Bootis"",""x"":-72.28125,""y"":218.5625,""z"":38.84375},{""id64"":216753195363,""name"":""A Carinae"",""x"":502.78125,""y"":-200.6875,""z"":-58.5625},{""id64"":5597258940,""name"":""104 Aquarii A"",""x"":-229.65625,""y"":-796.5,""z"":126.09375},{""id64"":560182856067,""name"":""28 A Aquilae"",""x"":-248.625,""y"":-5.75,""z"":227.84375},{""id64"":358461936370,""name"":""1 Aquarii A"",""x"":-155.6875,""y"":-95.0,""z"":145.21875},{""id64"":6605501013,""name"":""59 Andromedae A"",""x"":-158.21875,""y"":-93.625,""z"":-187.875},{""id64"":4488778134528,""name"":""Aiphaits AS-A a0"",""x"":-8178.375,""y"":11.21875,""z"":33503.25},{""id64"":545086002277,""name"":""Aunair AF-A f1015"",""x"":-472.15625,""y"":1640.5625,""z"":20796.6875},{""id64"":550839986436,""name"":""Agnaiz AF-A e128"",""x"":-8694.125,""y"":-1079.71875,""z"":22039.625},{""id64"":503751120293,""name"":""Aishaish AF-A f938"",""x"":845.8125,""y"":326.40625,""z"":33682.125},{""id64"":508225858372,""name"":""Aemonz AF-A e118"",""x"":4173.0625,""y"":-1027.84375,""z"":33515.28125},{""id64"":516673308868,""name"":""Aunair AA-A e120"",""x"":-1261.78125,""y"":1378.46875,""z"":20709.4375}],""values"":[""Sagittarius A*"",""A Velorum"",""a Velorum"",""a Puppis"",""a Centauri"",""A Capricorni"",""A Cen"",""a Lupi"",""A Bootis"",""A Carinae"",""104 Aquarii A"",""28 A Aquilae"",""1 Aquarii A"",""59 Andromedae A"",""Aiphaits AS-A a0"",""Aunair AF-A f1015"",""Agnaiz AF-A e128"",""Aishaish AF-A f938"",""Aemonz AF-A e118"",""Aunair AA-A e120""]}" );
            fakeSpanshRestClient.Expect( "fleetcarrier/route?source=NLTT 13249&capacity_used=25000&calculate_starting_fuel=1&destinations=Sagittarius A*", "{\"job\":\"F2B5B476-4458-11ED-9B9F-5DE194EB4526\",\"status\":\"queued\"}" );
            fakeSpanshRestClient.Expect( "results/F2B5B476-4458-11ED-9B9F-5DE194EB4526", DeserializeJsonResource<string>( Resources.SpanshCarrierResult ) );

            // Act
            var result = fakeSpanshService.GetCarrierRoute("NLTT 13249", new[] { "Sagittarius A*" }, 25000);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.FillVisitedGaps);
            Assert.IsFalse(result.GuidanceEnabled);
            Assert.AreEqual(26021.94M, result.RouteDistance);
            Assert.AreEqual(6845, result.RouteFuelTotal);
            Assert.AreEqual(54, result.Waypoints.Count);

            Assert.AreEqual(0M, result.Waypoints[0].distance);
            Assert.AreEqual(26021.94M, result.Waypoints[0].distanceRemaining);
            Assert.AreEqual(0M, result.Waypoints[0].distanceTraveled);
            Assert.AreEqual(6845, result.Waypoints[0].fuelNeeded);
            Assert.AreEqual(0, result.Waypoints[0].fuelUsed);
            Assert.AreEqual(0, result.Waypoints[0].fuelUsedTotal);
            Assert.IsFalse(result.Waypoints[0].hasIcyRing);
            Assert.IsFalse(result.Waypoints[0].hasNeutronStar);
            Assert.IsFalse(result.Waypoints[0].hasPristineMining);
            Assert.AreEqual(0, result.Waypoints[0].index);
            Assert.IsTrue(result.Waypoints[0].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[0].isMissionSystem);
            Assert.AreEqual(0, result.Waypoints[0].missionids.Count);
            Assert.IsFalse(result.Waypoints[0].refuelRecommended);
            Assert.AreEqual((ulong)2869440882065, result.Waypoints[0].systemAddress);
            Assert.AreEqual("NLTT 13249", result.Waypoints[0].systemName);
            Assert.IsFalse(result.Waypoints[0].visited);
            Assert.AreEqual(-38.8125M, result.Waypoints[0].x);
            Assert.AreEqual(9.3125M, result.Waypoints[0].y);
            Assert.AreEqual(-60.53125M, result.Waypoints[0].z);

            Assert.AreEqual(499.52M, result.Waypoints[3].distance);
            Assert.AreEqual(24522.68M, result.Waypoints[3].distanceRemaining);
            Assert.AreEqual(1499.26M, result.Waypoints[3].distanceTraveled);
            Assert.AreEqual(6449, result.Waypoints[3].fuelNeeded);
            Assert.AreEqual(132, result.Waypoints[3].fuelUsed);
            Assert.AreEqual(396, result.Waypoints[3].fuelUsedTotal);
            Assert.IsTrue(result.Waypoints[3].hasIcyRing);
            Assert.IsFalse(result.Waypoints[3].hasNeutronStar);
            Assert.IsTrue(result.Waypoints[3].hasPristineMining);
            Assert.AreEqual(3, result.Waypoints[3].index);
            Assert.IsFalse(result.Waypoints[3].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[3].isMissionSystem);
            Assert.AreEqual(0, result.Waypoints[3].missionids.Count);
            Assert.IsFalse(result.Waypoints[3].refuelRecommended);
            Assert.AreEqual((ulong)18262335236073, result.Waypoints[3].systemAddress);
            Assert.AreEqual("Praea Euq IQ-W b56-8", result.Waypoints[3].systemName);
            Assert.IsFalse(result.Waypoints[3].visited);
            Assert.AreEqual(-51.15625M, result.Waypoints[3].x);
            Assert.AreEqual(-2.96875M, result.Waypoints[3].y);
            Assert.AreEqual(1437.34375M, result.Waypoints[3].z);

            Assert.AreEqual(34.82M, result.Waypoints[53].distance);
            Assert.AreEqual(0M, result.Waypoints[53].distanceRemaining);
            Assert.AreEqual(26021.94M, result.Waypoints[53].distanceTraveled);
            Assert.AreEqual(0, result.Waypoints[53].fuelNeeded);
            Assert.AreEqual(14, result.Waypoints[53].fuelUsed);
            Assert.AreEqual(6845, result.Waypoints[53].fuelUsedTotal);
            Assert.IsFalse(result.Waypoints[53].hasIcyRing);
            Assert.IsFalse(result.Waypoints[53].hasNeutronStar);
            Assert.IsFalse(result.Waypoints[53].hasPristineMining);
            Assert.AreEqual(53, result.Waypoints[53].index);
            Assert.IsTrue(result.Waypoints[53].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[53].isMissionSystem);
            Assert.AreEqual(0, result.Waypoints[53].missionids.Count);
            Assert.IsFalse(result.Waypoints[53].refuelRecommended);
            Assert.AreEqual((ulong)20578934, result.Waypoints[53].systemAddress);
            Assert.AreEqual("Sagittarius A*", result.Waypoints[53].systemName);
            Assert.IsFalse(result.Waypoints[53].visited);
            Assert.AreEqual(25.21875M, result.Waypoints[53].x);
            Assert.AreEqual(-20.90625M, result.Waypoints[53].y);
            Assert.AreEqual(25899.96875M, result.Waypoints[53].z);
        }

        [TestMethod]
        public void TestSpanshGalaxyRoute()
        {
            // Arrange
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=NLTT 13249", @"{""min_max"":[{""id64"":2869440882065,""name"":""NLTT 13249"",""x"":-38.8125,""y"":9.3125,""z"":-60.53125},{""id64"":44853758275,""name"":""HIP 13249"",""x"":71.0,""y"":-646.5625,""z"":-407.34375},{""id64"":216669342059,""name"":""NLTT 14641"",""x"":97.625,""y"":-65.1875,""z"":35.03125},{""id64"":83449746098,""name"":""NLTT 2408"",""x"":-259.1875,""y"":-147.84375,""z"":-161.75},{""id64"":908553360066,""name"":""NLTT 16391"",""x"":42.90625,""y"":-8.65625,""z"":-81.875},{""id64"":908620468962,""name"":""NLTT 35146"",""x"":86.65625,""y"":9.75,""z"":74.9375},{""id64"":1041269524835,""name"":""NLTT 18977"",""x"":-43.9375,""y"":49.78125,""z"":-64.34375},{""id64"":1458309108442,""name"":""NLTT 54244"",""x"":42.4375,""y"":-68.6875,""z"":52.15625},{""id64"":1733187048146,""name"":""NLTT 11599"",""x"":48.78125,""y"":-62.09375,""z"":-11.71875},{""id64"":670148273569,""name"":""NLTT 1796"",""x"":-55.28125,""y"":-279.625,""z"":-13.78125},{""id64"":670149059985,""name"":""NLTT 6667"",""x"":-49.8125,""y"":-33.4375,""z"":-55.28125},{""id64"":670149125569,""name"":""NLTT 48288"",""x"":-60.4375,""y"":-13.09375,""z"":56.3125},{""id64"":2282942829266,""name"":""NLTT 8653"",""x"":44.15625,""y"":-101.03125,""z"":-20.03125},{""id64"":2868904076721,""name"":""NLTT 46709"",""x"":-65.3125,""y"":25.875,""z"":23.6875},{""id64"":2869709579681,""name"":""NLTT 30929"",""x"":-8.3125,""y"":84.65625,""z"":-12.25},{""id64"":2870782731705,""name"":""NLTT 4671"",""x"":69.34375,""y"":-96.125,""z"":36.125},{""id64"":3107442332362,""name"":""NLTT 6637"",""x"":-25.3125,""y"":-82.96875,""z"":-52.90625},{""id64"":7267487393161,""name"":""NLTT 14879"",""x"":-31.71875,""y"":14.0,""z"":-67.5},{""id64"":3382454555338,""name"":""NLTT 19808"",""x"":46.09375,""y"":26.78125,""z"":-43.71875},{""id64"":2832698741474,""name"":""NLTT 40287"",""x"":38.40625,""y"":33.78125,""z"":92.75}],""values"":[""NLTT 13249"",""HIP 13249"",""NLTT 14641"",""NLTT 2408"",""NLTT 16391"",""NLTT 35146"",""NLTT 18977"",""NLTT 54244"",""NLTT 11599"",""NLTT 1796"",""NLTT 6667"",""NLTT 48288"",""NLTT 8653"",""NLTT 46709"",""NLTT 30929"",""NLTT 4671"",""NLTT 6637"",""NLTT 14879"",""NLTT 19808"",""NLTT 40287""]}" );
            fakeSpanshRestClient.Expect("systems/field_values/system_names?q=Soul Sector EL-Y d7", @"{""min_max"":[{""id64"":249938593603,""name"":""Soul Sector EL-Y d7"",""x"":-5043.15625,""y"":85.03125,""z"":-5513.09375},{""id64"":18360574884,""name"":""Soul Sector EL-Y e4"",""x"":-5018.03125,""y"":163.5625,""z"":-5532.4375},{""id64"":215578855235,""name"":""Soul Sector EL-Y d6"",""x"":-5028.625,""y"":88.0625,""z"":-5535.6875},{""id64"":490456762179,""name"":""Soul Sector EL-Y d14"",""x"":-5068.125,""y"":129.4375,""z"":-5537.5},{""id64"":284298331971,""name"":""Soul Sector EL-Y d8"",""x"":-5047.0625,""y"":72.96875,""z"":-5504.46875},{""id64"":3098919636602,""name"":""Soul Sector EL-Y c11"",""x"":-5131.25,""y"":70.25,""z"":-5551.84375},{""id64"":5023064985210,""name"":""Soul Sector EL-Y c18"",""x"":-5114.09375,""y"":65.46875,""z"":-5559.65625},{""id64"":1933565773635,""name"":""Soul Sector EL-Y d56"",""x"":-5094.59375,""y"":81.53125,""z"":-5532.0625},{""id64"":1967925512003,""name"":""Soul Sector EL-Y d57"",""x"":-5093.90625,""y"":56.25,""z"":-5508.21875},{""id64"":2274285915770,""name"":""Soul Sector EL-Y c8"",""x"":-5131.875,""y"":86.53125,""z"":-5557.09375},{""id64"":1796126820163,""name"":""Soul Sector EL-Y d52"",""x"":-5036.78125,""y"":65.96875,""z"":-5475.125},{""id64"":1177651529539,""name"":""Soul Sector EL-Y d34"",""x"":-5071.40625,""y"":108.75,""z"":-5472.625},{""id64"":1418169698115,""name"":""Soul Sector EL-Y d41"",""x"":-5072.6875,""y"":113.34375,""z"":-5498.625},{""id64"":1074572314435,""name"":""Soul Sector EL-Y d31"",""x"":-5032.03125,""y"":124.03125,""z"":-5531.71875},{""id64"":834054145859,""name"":""Soul Sector EL-Y d24"",""x"":-5090.65625,""y"":73.5625,""z"":-5499.28125},{""id64"":899896381050,""name"":""Soul Sector EL-Y c3"",""x"":-5143.96875,""y"":87.53125,""z"":-5568.8125},{""id64"":902773622595,""name"":""Soul Sector EL-Y d26"",""x"":-5103.6875,""y"":118.5625,""z"":-5519.25},{""id64"":662255454019,""name"":""Soul Sector EL-Y d19"",""x"":-5063.5625,""y"":113.8125,""z"":-5516.25},{""id64"":696615192387,""name"":""Soul Sector EL-Y d20"",""x"":-5047.125,""y"":80.0625,""z"":-5489.53125},{""id64"":1452529436483,""name"":""Soul Sector EL-Y d42"",""x"":-5083.28125,""y"":101.21875,""z"":-5493.46875}],""values"":[""Soul Sector EL-Y d7"",""Soul Sector EL-Y e4"",""Soul Sector EL-Y d6"",""Soul Sector EL-Y d14"",""Soul Sector EL-Y d8"",""Soul Sector EL-Y c11"",""Soul Sector EL-Y c18"",""Soul Sector EL-Y d56"",""Soul Sector EL-Y d57"",""Soul Sector EL-Y c8"",""Soul Sector EL-Y d52"",""Soul Sector EL-Y d34"",""Soul Sector EL-Y d41"",""Soul Sector EL-Y d31"",""Soul Sector EL-Y d24"",""Soul Sector EL-Y c3"",""Soul Sector EL-Y d26"",""Soul Sector EL-Y d19"",""Soul Sector EL-Y d20"",""Soul Sector EL-Y d42""]}");
            fakeSpanshRestClient.Expect( "generic/route?source=NLTT 13249&destination=Soul Sector EL-Y d7&is_supercharged=0&use_supercharge=1&use_injections=0&exclude_secondary=0&fuel_power=2.6&fuel_multiplier=0.012&optimal_mass=1800&base_mass=0&tank_size=0&internal_tank_size=1.07&max_fuel_per_jump=8.00&range_boost=0", "{\"job\":\"F2B5B476-4458-11ED-9B9F-5DE194EB4527\",\"status\":\"queued\"}");
            fakeSpanshRestClient.Expect("results/F2B5B476-4458-11ED-9B9F-5DE194EB4527", DeserializeJsonResource<string>(Resources.SpanshGalaxyResult));

            // Act
            var ship = ShipDefinitions.FromEDModel("Anaconda");
            ship.frameshiftdrive = Module.Int_Hyperdrive_Size6_Class5;
            var result = fakeSpanshService.GetGalaxyRoute("NLTT 13249", "Soul Sector EL-Y d7", ship);

            // Assert[9]
            Assert.IsNotNull(result);
            Assert.IsTrue(result.FillVisitedGaps);
            Assert.IsFalse(result.GuidanceEnabled);
            Assert.AreEqual(8178.36M, result.RouteDistance);
            Assert.AreEqual(259, result.Waypoints.Count);

            Assert.AreEqual(0M, result.Waypoints[0].distance);
            Assert.AreEqual(8178.36M, result.Waypoints[0].distanceRemaining);
            Assert.AreEqual(0M, result.Waypoints[0].distanceTraveled);
            Assert.IsFalse(result.Waypoints[0].hasNeutronStar);
            Assert.AreEqual(0, result.Waypoints[0].index);
            Assert.IsFalse(result.Waypoints[0].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[0].isMissionSystem);
            Assert.IsFalse(result.Waypoints[0].isScoopable);
            Assert.AreEqual(0, result.Waypoints[0].missionids.Count);
            Assert.IsFalse(result.Waypoints[0].refuelRecommended);
            Assert.AreEqual((ulong)2869440882065, result.Waypoints[0].systemAddress);
            Assert.AreEqual("NLTT 13249", result.Waypoints[0].systemName);
            Assert.IsFalse(result.Waypoints[0].visited);
            Assert.AreEqual(-38.8125M, result.Waypoints[0].x);
            Assert.AreEqual(9.3125M, result.Waypoints[0].y);
            Assert.AreEqual(-60.53125M, result.Waypoints[0].z);

            Assert.AreEqual(30.06M, result.Waypoints[63].distance);
            Assert.AreEqual(6311.29M, result.Waypoints[63].distanceRemaining);
            Assert.AreEqual(1867.07M, result.Waypoints[63].distanceTraveled);
            Assert.IsTrue(result.Waypoints[63].hasNeutronStar);
            Assert.AreEqual(63, result.Waypoints[63].index);
            Assert.IsFalse(result.Waypoints[63].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[63].isMissionSystem);
            Assert.IsFalse(result.Waypoints[63].isScoopable);
            Assert.AreEqual(0, result.Waypoints[63].missionids.Count);
            Assert.IsFalse(result.Waypoints[63].refuelRecommended);
            Assert.AreEqual((ulong)147647924467, result.Waypoints[63].systemAddress);
            Assert.AreEqual("Outopps AS-B d13-4", result.Waypoints[63].systemName);
            Assert.IsFalse(result.Waypoints[63].visited);
            Assert.AreEqual(-1276.5M, result.Waypoints[63].x);
            Assert.AreEqual(182.53125M, result.Waypoints[63].y);
            Assert.AreEqual(-1182.375M, result.Waypoints[63].z);

            Assert.AreEqual(30.74M, result.Waypoints[254].distance);
            Assert.AreEqual(201.48M, result.Waypoints[254].distanceRemaining);
            Assert.AreEqual(7976.88M, result.Waypoints[254].distanceTraveled);
            Assert.IsFalse(result.Waypoints[254].hasNeutronStar);
            Assert.AreEqual(254, result.Waypoints[254].index);
            Assert.IsFalse(result.Waypoints[254].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[254].isMissionSystem);
            Assert.IsTrue(result.Waypoints[254].isScoopable);
            Assert.AreEqual(0, result.Waypoints[254].missionids.Count);
            Assert.IsTrue(result.Waypoints[254].refuelRecommended);
            Assert.AreEqual((ulong)937166915403, result.Waypoints[254].systemAddress);
            Assert.AreEqual("Hypoae Ain LI-K d8-27", result.Waypoints[254].systemName);
            Assert.IsFalse(result.Waypoints[254].visited);
            Assert.AreEqual(-4917.28125M, result.Waypoints[254].x);
            Assert.AreEqual(74.8125M, result.Waypoints[254].y);
            Assert.AreEqual(-5385.25M, result.Waypoints[254].z);

            Assert.AreEqual(118.13M, result.Waypoints[258].distance);
            Assert.AreEqual(0M, result.Waypoints[258].distanceRemaining);
            Assert.AreEqual(8178.36M, result.Waypoints[258].distanceTraveled);
            Assert.IsFalse(result.Waypoints[258].hasNeutronStar);
            Assert.AreEqual(258, result.Waypoints[258].index);
            Assert.IsFalse(result.Waypoints[258].isDesiredDestination);
            Assert.IsFalse(result.Waypoints[258].isMissionSystem);
            Assert.IsFalse(result.Waypoints[258].isScoopable);
            Assert.AreEqual(0, result.Waypoints[258].missionids.Count);
            Assert.IsFalse(result.Waypoints[258].refuelRecommended);
            Assert.AreEqual((ulong)249938593603, result.Waypoints[258].systemAddress);
            Assert.AreEqual("Soul Sector EL-Y d7", result.Waypoints[258].systemName);
            Assert.IsFalse(result.Waypoints[258].visited);
            Assert.AreEqual(-5043.15625M, result.Waypoints[258].x);
            Assert.AreEqual(85.03125M, result.Waypoints[258].y);
            Assert.AreEqual(-5513.09375M, result.Waypoints[258].z);
        }

        [ TestMethod ]
        public void TestSpanshSystemDumpSol ()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( "dump/10477373803", DeserializeJsonResource<string>( Resources.SpanshStarSystemDumpSol ) );
            var result = fakeSpanshService.GetStarSystem(10477373803U, true);

            Assert.AreEqual( 10477373803U, result.systemAddress);
            Assert.AreEqual( 0.0M, result.x );
            Assert.AreEqual( 0.0M, result.y );
            Assert.AreEqual( 0.0M, result.z );
            Assert.AreEqual( result.systemname, "Sol" );
            Assert.AreEqual( "Refinery", result.Economies[0]?.invariantName );
            Assert.AreEqual( "Service", result.Economies[1]?.invariantName );
            Assert.AreEqual( "Mother Gaia", result.Faction?.name );
            Assert.AreEqual( "Federation", result.Faction?.Allegiance.invariantName );
            Assert.AreEqual( "Democracy", result.Faction?.Government.invariantName );
            Assert.AreEqual( "Boom", result.Faction?.presences[0].FactionState.invariantName );
            Assert.AreEqual( 0.341633M, result.Faction?.presences[ 0 ].influence );
            Assert.AreEqual( 6, result.factions.Count );
            Assert.AreEqual( "Jerome Archer", result.Power?.invariantName );
            Assert.AreEqual( "Stronghold", result.powerState?.invariantName );
            Assert.AreEqual( true, new[]
                {
                    "Edmund Mahon", 
                    "Yuri Grom"
                }.All( n => result.ContestingPowers.Select( p => p.invariantName ).Contains( n ) ) );
            Assert.AreEqual( "High", result.securityLevel?.invariantName );
            Assert.AreEqual( 22780919531L, result.population );
            Assert.AreEqual( 1731650107L, result.updatedat );
            Assert.AreEqual( 40, result.totalbodies );
            Assert.AreEqual( 67, result.stations.Count );

            // Test a star
            var solStar = result.bodies.FirstOrDefault( b => b.mainstar ?? false );
            Assert.IsNotNull( solStar );
            Assert.AreEqual( 4.829987M, solStar.absolutemagnitude );
            Assert.AreEqual( 4792, solStar.age );
            Assert.AreEqual( -0.219615M, solStar.tilt );
            Assert.AreEqual( 0, solStar.bodyId );
            Assert.AreEqual( 0M, solStar.distance );
            Assert.AreEqual( 10477373803U, solStar.systemAddress );
            Assert.AreEqual( "V", solStar.luminosityclass );
            Assert.AreEqual( "Sol", solStar.bodyname );
            Assert.AreEqual( 2.58327451196759M, solStar.rotationalperiod );
            Assert.AreEqual( 1M, solStar.solarmass );
            Assert.AreEqual( 0.999999953989935M, solStar.solarradius );
            Assert.AreEqual( "G", solStar.stellarclass );
            Assert.AreEqual( 2, solStar.stellarsubclass );
            Assert.AreEqual( 5778M, solStar.temperature );
            Assert.AreEqual( BodyType.Star, solStar.bodyType );
            Assert.AreEqual( 1731650107, solStar.updatedat );

            // Test a landable planet
            var mercuryBody = result.bodies.FirstOrDefault( b => b.bodyId == 1 );
            Assert.IsNotNull( mercuryBody );
            Assert.AreEqual( 29.124M, mercuryBody.periapsis );
            //Assert.AreEqual( 48.331001M, mercuryBody.ascendingnode );
            Assert.AreEqual( 0, mercuryBody.atmospherecompositions.Count );
            Assert.AreEqual( AtmosphereClass.None, mercuryBody.atmosphereclass );
            Assert.AreEqual( 0.036826M, mercuryBody.tilt );
            Assert.AreEqual( 220.236331M, mercuryBody.distance );
            Assert.AreEqual( 0.055M, mercuryBody.earthmass );
            Assert.AreEqual( 0.375546038543897M, mercuryBody.gravity );
            Assert.AreEqual( true, mercuryBody.landable );
            Assert.AreEqual( 11, mercuryBody.materials.Count );
            Assert.AreEqual( "Iron", mercuryBody.materials[0].definition.invariantName );
            Assert.AreEqual( 23.508457M, mercuryBody.materials[ 0 ].percentage );
            Assert.AreEqual( "Nickel", mercuryBody.materials[ 1 ].definition.invariantName );
            Assert.AreEqual( 17.780811M, mercuryBody.materials[ 1 ].percentage );
            Assert.AreEqual( "Sulphur", mercuryBody.materials[ 2 ].definition.invariantName );
            Assert.AreEqual( 12.85643M, mercuryBody.materials[ 2 ].percentage );
            //Assert.AreEqual( 235.644209M, mercuryBody.meananomaly );
            Assert.AreEqual( "Mercury", mercuryBody.bodyname );
            Assert.AreEqual( 0.2056M, mercuryBody.eccentricity );
            Assert.AreEqual( 7M, mercuryBody.inclination );
            Assert.AreEqual( 87.9691003097454M, mercuryBody.orbitalperiod );
            Assert.AreEqual( 1, mercuryBody.parents.Count );
            Assert.AreEqual( 0, Convert.ToInt32( mercuryBody.parents[ 0 ][ "Star" ] ) );
            Assert.AreEqual( 2439.7M, mercuryBody.radius );
            Assert.AreEqual( 58.6460011179282M, mercuryBody.rotationalperiod );
            Assert.AreEqual( false, mercuryBody.tidallylocked );
            Assert.AreEqual( 193.16393581521619631491696832M, mercuryBody.semimajoraxis );
            Assert.AreEqual( 3, mercuryBody.solidcompositions.Count );
            Assert.AreEqual( "Metal", mercuryBody.solidcompositions[ 0 ].invariantComposition );
            Assert.AreEqual( 60M, mercuryBody.solidcompositions[ 0 ].percent );
            Assert.AreEqual( "Rock", mercuryBody.solidcompositions[ 1 ].invariantComposition );
            Assert.AreEqual( 40M, mercuryBody.solidcompositions[ 1 ].percent );
            Assert.AreEqual( "Ice", mercuryBody.solidcompositions[ 2 ].invariantComposition );
            Assert.AreEqual( 0M, mercuryBody.solidcompositions[ 2 ].percent );
            Assert.AreEqual( "Metal-rich body", mercuryBody.planetClass.invariantName );
            Assert.AreEqual( 0M, mercuryBody.pressure );
            Assert.AreEqual( 401.965271M, mercuryBody.temperature );
            Assert.AreEqual( "Not terraformable", mercuryBody.terraformState.invariantName );
            Assert.AreEqual( "Planet", mercuryBody.bodyType.invariantName );
            Assert.AreEqual( 1731639333, mercuryBody.updatedat );
            Assert.AreEqual( null, mercuryBody.volcanism );

            // Test a non-landable volcanic moon
            var ioBody = result.bodies.FirstOrDefault( b => b.bodyId == 9 );
            Assert.IsNotNull( ioBody );
            Assert.AreEqual( 0M, ioBody.periapsis);
            //Assert.AreEqual( 0M, ioBody.ascendingnode );
            Assert.AreEqual( 3, ioBody.atmospherecompositions.Count );
            Assert.AreEqual( "Sulphur dioxide", ioBody.atmospherecompositions[ 0 ].invariantComposition );
            Assert.AreEqual( 90M, ioBody.atmospherecompositions[ 0 ].percent );
            Assert.AreEqual( "Silicates", ioBody.atmospherecompositions[ 1 ].invariantComposition );
            Assert.AreEqual( 5M, ioBody.atmospherecompositions[ 1 ].percent );
            Assert.AreEqual( "Oxygen", ioBody.atmospherecompositions[ 2 ].invariantComposition );
            Assert.AreEqual( 2.999999M, ioBody.atmospherecompositions[ 2 ].percent );
            Assert.AreEqual( AtmosphereClass.None, ioBody.atmosphereclass );
            Assert.AreEqual( 0M, ioBody.tilt );
            Assert.AreEqual( 2711.8941M, ioBody.distance );
            Assert.AreEqual( 0.015M, ioBody.earthmass );
            Assert.AreEqual( 0.183781380646477M, ioBody.gravity );
            Assert.AreEqual( false, ioBody.landable );
            //Assert.AreEqual( 262.699694M, ioBody.meananomaly );
            Assert.AreEqual( "Io", ioBody.bodyname );
            Assert.AreEqual( 0.0041M, ioBody.eccentricity );
            Assert.AreEqual( 0.05M, ioBody.inclination );
            Assert.AreEqual( 1.76913767225694M, ioBody.orbitalperiod );
            Assert.AreEqual( 2, ioBody.parents.Count );
            Assert.AreEqual( 7, Convert.ToInt32( ioBody.parents[ 0 ][ "Planet" ] ) );
            Assert.AreEqual( 0, Convert.ToInt32( ioBody.parents[ 1 ][ "Star" ] ) );
            Assert.AreEqual( 1821.3M, ioBody.radius );
            Assert.AreEqual( 1.76913773259259M, ioBody.rotationalperiod );
            Assert.AreEqual( true, ioBody.tidallylocked );
            Assert.AreEqual( 1.4066395732938339718636117257M, ioBody.semimajoraxis );
            Assert.AreEqual( 3, ioBody.solidcompositions.Count );
            Assert.AreEqual( "Rock", ioBody.solidcompositions[ 0 ].invariantComposition );
            Assert.AreEqual( 80M, ioBody.solidcompositions[ 0 ].percent );
            Assert.AreEqual( "Metal", ioBody.solidcompositions[ 1 ].invariantComposition );
            Assert.AreEqual( 20M, ioBody.solidcompositions[ 1 ].percent );
            Assert.AreEqual( "Ice", ioBody.solidcompositions[ 2 ].invariantComposition );
            Assert.AreEqual( 0M, ioBody.solidcompositions[ 2 ].percent );
            Assert.AreEqual( "Rocky body", ioBody.planetClass.invariantName );
            Assert.AreEqual( 0M, ioBody.pressure );
            Assert.AreEqual( 120.066437M, ioBody.temperature );
            Assert.AreEqual( "Not terraformable", ioBody.terraformState.invariantName );
            Assert.AreEqual( "Moon", ioBody.bodyType.invariantName );
            Assert.AreEqual( 1731639333, ioBody.updatedat );
            Assert.AreEqual( "Major", ioBody.volcanism.invariantAmount );
            Assert.AreEqual( "Silicate", ioBody.volcanism.invariantComposition );
            Assert.AreEqual( "Magma", ioBody.volcanism.invariantType );

            // Test a ringed gas giant
            var saturnBody = result.bodies.FirstOrDefault( b => b.bodyId == 13 );
            Assert.IsNotNull( saturnBody );
            Assert.AreEqual( 336.013854M, saturnBody.periapsis );
            //Assert.AreEqual( 113.642812M, saturnBody.ascendingnode );
            Assert.AreEqual( 2, saturnBody.atmospherecompositions.Count );
            Assert.AreEqual( "Hydrogen", saturnBody.atmospherecompositions[ 0 ].invariantComposition );
            Assert.AreEqual( 73.699471M, saturnBody.atmospherecompositions[ 0 ].percent );
            Assert.AreEqual( "Helium", saturnBody.atmospherecompositions[ 1 ].invariantComposition );
            Assert.AreEqual( 26.300531M, saturnBody.atmospherecompositions[ 1 ].percent );
            Assert.AreEqual( AtmosphereClass.GasGiant, saturnBody.atmosphereclass );
            Assert.AreEqual( 0.466526M, saturnBody.tilt );
            Assert.AreEqual( 4826.257317M, saturnBody.distance );
            Assert.AreEqual( 95.159035M, saturnBody.earthmass );
            Assert.AreEqual( 1.11101315386969M, saturnBody.gravity );
            Assert.AreEqual( false, saturnBody.landable );
            //Assert.AreEqual( 263.47047M, saturnBody.meananomaly );
            Assert.AreEqual( "Saturn", saturnBody.bodyname );
            Assert.AreEqual( 0.055723M, saturnBody.eccentricity );
            Assert.AreEqual( 2.48524M, saturnBody.inclination );
            Assert.AreEqual( 10759.2196652183M, saturnBody.orbitalperiod );
            Assert.AreEqual( 1, saturnBody.parents.Count );
            Assert.AreEqual( 0, Convert.ToInt32( saturnBody.parents[ 0 ][ "Star" ] ) );
            Assert.AreEqual( 59000.0M, saturnBody.radius );
            Assert.AreEqual( "Common", saturnBody.reserveLevel.invariantName );
            Assert.AreEqual( 1, saturnBody.rings.Count );
            Assert.AreEqual( 74500000.0M, saturnBody.rings[ 0 ].innerradius );
            Assert.AreEqual( 58071.0M, saturnBody.rings[ 0 ].mass );
            Assert.AreEqual( "D Ring", saturnBody.rings[ 0 ].name );
            Assert.AreEqual( 140180000.0M, saturnBody.rings[ 0 ].outerradius );
            Assert.AreEqual( "Icy", saturnBody.rings[ 0 ].Composition.invariantName );
            Assert.AreEqual( 0.440416655659722M, saturnBody.rotationalperiod );
            Assert.AreEqual( false, saturnBody.tidallylocked );
            Assert.AreEqual( 4781.4714856073191059861018919M, saturnBody.semimajoraxis );
            Assert.AreEqual( "Class I gas giant", saturnBody.planetClass.invariantName );
            Assert.AreEqual( 0M, saturnBody.pressure );
            Assert.AreEqual( 87.99826M, saturnBody.temperature );
            Assert.AreEqual( "Not terraformable", saturnBody.terraformState.invariantName );
            Assert.AreEqual( "Planet", saturnBody.bodyType.invariantName );
            Assert.AreEqual( 1731639333, saturnBody.updatedat );

            // Test orbital stations
            Assert.AreEqual( 9, result.orbitalstations.Count );

            var rescueStation = result.stations.FirstOrDefault( b => b.marketId == 128977009 );
            Assert.IsNotNull( rescueStation );
            Assert.AreEqual( "Federation", rescueStation.Faction.Allegiance.invariantName );
            Assert.AreEqual( "Mother Gaia", rescueStation.Faction.name );
            Assert.AreEqual( FactionState.Boom.invariantName,
                rescueStation.Faction.presences.FirstOrDefault( f => 
                    f.systemAddress == 10477373803U )?.FactionState.invariantName );
            Assert.AreEqual( 496.966721M, rescueStation.distancefromstar );
            Assert.AreEqual( 1, rescueStation.economyShares.Count );
            Assert.AreEqual( "Rescue", rescueStation.economyShares[ 0 ].economy.invariantName );
            Assert.AreEqual( 100M, rescueStation.economyShares[ 0 ].proportion );
            Assert.AreEqual( "Democracy", rescueStation.Faction.Government.invariantName );
            Assert.AreEqual( 11, rescueStation.commodities.Count );
            Assert.AreEqual( "Basic Medicines", rescueStation.commodities[ 0 ].invariantName );
            Assert.AreEqual( 133952461, rescueStation.commodities[ 0 ].demand );
            Assert.AreEqual( 4449M, rescueStation.commodities[ 0 ].sellprice );
            Assert.AreEqual( 0, rescueStation.commodities[ 0 ].stock );
            Assert.AreEqual( 0M, rescueStation.commodities[ 0 ].buyprice );
            Assert.AreEqual( "Rescue Ship - Li Qing Jao", rescueStation.name );
            Assert.AreEqual( 85, rescueStation.outfitting.Count );
            Assert.AreEqual( "Lightweight Alloy", rescueStation.outfitting[0].invariantName );
            Assert.AreEqual( true, new[]
                {
                    "Dock", 
                    "Auto Dock", 
                    "Black Market", 
                    "Market", 
                    "Contacts", 
                    "Missions", 
                    "Outfitting", 
                    "Restock",
                    "Refuel", 
                    "Repair", 
                    "Tuning", 
                    "Workshop", 
                    "Missions Generated", 
                    "Flight Controller",
                    "Station Operations", 
                    "Search and Rescue", 
                    "Station Menu"
                }.All( s => rescueStation.stationServices.Select( svc => svc.invariantName ).Contains( s ) ) );
            Assert.AreEqual( "Megaship", rescueStation.Model.invariantName );
            Assert.AreEqual( 0, rescueStation.shipyard.Count );
            Assert.AreEqual( 1616084366, rescueStation.updatedat );

            var orbitalStation = result.stations.FirstOrDefault( b => b.marketId == 128018176 );
            Assert.IsNotNull( orbitalStation );
            Assert.AreEqual( "Federation", orbitalStation.Faction.Allegiance.invariantName );
            Assert.AreEqual( "Mother Gaia", orbitalStation.Faction.name );
            Assert.AreEqual( FactionState.Boom.invariantName,
                orbitalStation.Faction.presences.FirstOrDefault( f =>
                    f.systemAddress == 10477373803U )?.FactionState.invariantName );
            Assert.AreEqual( 4825.538875M, orbitalStation.distancefromstar );
            Assert.AreEqual( 1, orbitalStation.economyShares.Count );
            Assert.AreEqual( "Refinery", orbitalStation.economyShares[ 0 ].economy.invariantName );
            Assert.AreEqual( 100M, orbitalStation.economyShares[ 0 ].proportion );
            Assert.AreEqual( "Democracy", orbitalStation.Faction.Government.invariantName );
            Assert.AreEqual( 9, orbitalStation.landingPads.Large );
            Assert.AreEqual( 18, orbitalStation.landingPads.Medium );
            Assert.AreEqual( 17, orbitalStation.landingPads.Small );
            Assert.AreEqual( 92, orbitalStation.commodities.Count );
            Assert.AreEqual( "Advanced Catalysers", orbitalStation.commodities[ 0 ].invariantName );
            Assert.AreEqual( 123313, orbitalStation.commodities[ 0 ].demand );
            Assert.AreEqual( 3435M, orbitalStation.commodities[ 0 ].sellprice );
            Assert.AreEqual( 0, orbitalStation.commodities[ 0 ].stock );
            Assert.AreEqual( 0M, orbitalStation.commodities[ 0 ].buyprice );
            Assert.AreEqual( "Titan City", orbitalStation.name );
            Assert.AreEqual( 273, orbitalStation.outfitting.Count );
            Assert.AreEqual( "Reinforced Alloy", orbitalStation.outfitting[ 1 ].invariantName );
            Assert.AreEqual( true, new[]
                {
                    "Dock", 
                    "Auto Dock", 
                    "Market", 
                    "Contacts", 
                    "Universal Cartographics", 
                    "Missions", 
                    "Outfitting", 
                    "Crew Lounge", 
                    "Restock", 
                    "Refuel", 
                    "Repair", 
                    "Shipyard", 
                    "Tuning", 
                    "Workshop", 
                    "Missions Generated", 
                    "Flight Controller", 
                    "Station Operations", 
                    "Powerplay", 
                    "Search and Rescue", 
                    "Station Menu", 
                    "Shop", 
                    "Livery", 
                    "Social Space", 
                    "Bartender", 
                    "Vista Genomics", 
                    "Pioneer Supplies", 
                    "Apex Interstellar", 
                    "Frontline Solutions"
                }.All( s => orbitalStation.stationServices.Select( svc => svc.invariantName ).Contains( s ) ) );
            Assert.AreEqual( "Orbis Starport", orbitalStation.Model.invariantName );
            Assert.AreEqual( 15, orbitalStation.shipyard.Count );
            Assert.AreEqual( "Adder", orbitalStation.shipyard[ 0 ].model );
            Assert.AreEqual( 1731641839, orbitalStation.updatedat );

            // Test a planetary station

            Assert.AreEqual( 53, result.planetarystations.Count );

            var surfaceOutpost = result.stations.FirstOrDefault( b => b.marketId == 3534389760 );
            Assert.IsNotNull( surfaceOutpost );
            Assert.AreEqual( "Federation", surfaceOutpost.Faction.Allegiance.invariantName );
            Assert.AreEqual( "Sol Workers' Party", surfaceOutpost.Faction.name );
            Assert.AreEqual( FactionState.CivilLiberty.invariantName,
                surfaceOutpost.Faction.presences.FirstOrDefault( f =>
                    f.systemAddress == 10477373803U )?.FactionState.invariantName );
            Assert.AreEqual( 220.090497M, surfaceOutpost.distancefromstar );
            Assert.AreEqual( 1, surfaceOutpost.economyShares.Count );
            Assert.AreEqual( "Industrial", surfaceOutpost.economyShares[ 0 ].economy.invariantName );
            Assert.AreEqual( 100M, surfaceOutpost.economyShares[ 0 ].proportion );
            Assert.AreEqual( "Democracy", surfaceOutpost.Faction.Government.invariantName );
            Assert.AreEqual( 2, surfaceOutpost.landingPads.Large );
            Assert.AreEqual( 2, surfaceOutpost.landingPads.Medium );
            Assert.AreEqual( 4, surfaceOutpost.landingPads.Small );
            Assert.AreEqual( 92, surfaceOutpost.commodities.Count );
            Assert.AreEqual( "Walz Depot", surfaceOutpost.name );
            Assert.AreEqual( 216, surfaceOutpost.outfitting.Count );
            Assert.AreEqual( true, new[]
                {
                    "Dock",
                    "Auto Dock",
                    "Market",
                    "Contacts",
                    "Universal Cartographics",
                    "Missions",
                    "Outfitting",
                    "Crew Lounge",
                    "Restock",
                    "Refuel",
                    "Repair",
                    "Shipyard",
                    "Workshop",
                    "Missions Generated",
                    "Flight Controller",
                    "Station Operations",
                    "Powerplay",
                    "Search and Rescue",
                    "Station Menu",
                    "Shop",
                    "Livery",
                    "Social Space",
                    "Bartender",
                    "Vista Genomics",
                    "Pioneer Supplies",
                    "Apex Interstellar",
                    "Frontline Solutions"
                }.All( s => surfaceOutpost.stationServices.Select( svc => svc.invariantName ).Contains( s ) ) );
            Assert.AreEqual( "Surface Outpost", surfaceOutpost.Model.invariantName );
            Assert.AreEqual( 1, surfaceOutpost.shipyard.Count );
            Assert.AreEqual( "Eagle", surfaceOutpost.shipyard[ 0 ].model );
            Assert.AreEqual( 1731646237, surfaceOutpost.updatedat );

            var surfaceSettlement = result.stations.FirstOrDefault( b => b.marketId == 3534392832 );
            Assert.IsNotNull( surfaceSettlement );
            Assert.IsNull( surfaceSettlement.Faction );
            Assert.AreEqual( 2709.476104M, surfaceSettlement.distancefromstar );
            Assert.AreEqual( "Chargaff Reach", surfaceSettlement.name );
            Assert.AreEqual( "Settlement", surfaceSettlement.Model.invariantName );
            Assert.AreEqual( 1730689554, surfaceSettlement.updatedat );
        }

        [ TestMethod ]
        public void TestQuickSystemStations ()
        {
            // Arrange
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();
            fakeSpanshRestClient.Expect( @"system/10477373803", Encoding.UTF8.GetString( Resources.SpanshQuickStarSystemSol ) );

            // Act
            var starSystem = fakeSpanshService.GetQuickStarSystem(10477373803U);

            // Assert
            Assert.AreEqual( "Sol", starSystem.systemname );
            Assert.AreEqual( 10477373803U, starSystem.systemAddress );
            Assert.AreEqual( 0M, starSystem.x );
            Assert.AreEqual( 0M, starSystem.y );
            Assert.AreEqual( 0M, starSystem.z );

            Assert.AreEqual( 67, starSystem.stations.Count );
            Assert.AreEqual( 9, starSystem.orbitalstations.Count );
            Assert.AreEqual( 53, starSystem.planetarystations.Count );
            var station = starSystem.stations.FirstOrDefault(s => s.marketId == 128016384);
            Assert.IsNotNull( station );
            Assert.AreEqual( "Daedalus", station.name );
            Assert.AreEqual( "Orbis Starport", station.Model.invariantName );
        }
    }
}
