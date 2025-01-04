using EddiCore;
using EddiDataDefinitions;
using EddiShipMonitor;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests
{
    [TestClass]
    public class CommanderDataTests : TestBase
    {
        [TestInitialize]
        public void start ()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestCommanderFromProfile()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();

            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=TZ Arietis", @"{""min_max"":[{""id64"":13864825529761,""name"":""TZ Arietis"",""x"":-5.375,""y"":-10.59375,""z"":-8.5},{""id64"":58132918979256,""name"":""Arietis Sector TZ-W a2-3"",""x"":-104.46875,""y"":-42.53125,""z"":-191.15625},{""id64"":5067121632641,""name"":""Arietis Sector TZ-P b5-2"",""x"":-127.59375,""y"":-85.71875,""z"":-103.84375},{""id64"":2869172381065,""name"":""Arietis Sector TZ-O b6-1"",""x"":-59.96875,""y"":-23.84375,""z"":-80.8125},{""id64"":40540732934840,""name"":""Arietis Sector TZ-W a2-2"",""x"":-102.4375,""y"":-43.84375,""z"":-189.4375},{""id64"":5356360846008,""name"":""Arietis Sector TZ-W a2-0"",""x"":-100.625,""y"":-43.15625,""z"":-194.4375},{""id64"":5068195636617,""name"":""Arietis Sector TZ-O b6-2"",""x"":-62.46875,""y"":-23.03125,""z"":-84.40625},{""id64"":9465168143745,""name"":""Arietis Sector TZ-P b5-4"",""x"":-133.5625,""y"":-87.4375,""z"":-101.34375},{""id64"":11664191399297,""name"":""Arietis Sector TZ-P b5-5"",""x"":-133.125,""y"":-85.71875,""z"":-102.9375},{""id64"":7266144888193,""name"":""Arietis Sector TZ-P b5-3"",""x"":-128.34375,""y"":-86.34375,""z"":-101.125},{""id64"":22948546890424,""name"":""Arietis Sector TZ-W a2-1"",""x"":-99.90625,""y"":-44.375,""z"":-194.1875},{""id64"":670149125513,""name"":""Arietis Sector TZ-O b6-0"",""x"":-63.375,""y"":-20.375,""z"":-82.75},{""id64"":1773658701,""name"":""53 Arietis"",""x"":-196.84375,""y"":-461.09375,""z"":-664.0625},{""id64"":10426960155,""name"":""62 Arietis"",""x"":-277.875,""y"":-357.25,""z"":-759.28125},{""id64"":83382473354,""name"":""1 Arietis"",""x"":-289.21875,""y"":-361.9375,""z"":-358.15625},{""id64"":83650974346,""name"":""40 Arietis"",""x"":-135.6875,""y"":-271.0,""z"":-348.71875},{""id64"":83718148770,""name"":""36 Arietis"",""x"":-101.0,""y"":-202.6875,""z"":-250.46875},{""id64"":83718181546,""name"":""27 Arietis"",""x"":-96.75,""y"":-180.0,""z"":-203.875},{""id64"":1316130638171,""name"":""10 Arietis"",""x"":-79.09375,""y"":-88.21875,""z"":-105.875},{""id64"":2656177211747,""name"":""31 Arietis"",""x"":-29.0,""y"":-76.375,""z"":-78.53125}],""values"":[""TZ Arietis"",""Arietis Sector TZ-W a2-3"",""Arietis Sector TZ-P b5-2"",""Arietis Sector TZ-O b6-1"",""Arietis Sector TZ-W a2-2"",""Arietis Sector TZ-W a2-0"",""Arietis Sector TZ-O b6-2"",""Arietis Sector TZ-P b5-4"",""Arietis Sector TZ-P b5-5"",""Arietis Sector TZ-P b5-3"",""Arietis Sector TZ-W a2-1"",""Arietis Sector TZ-O b6-0"",""53 Arietis"",""62 Arietis"",""1 Arietis"",""40 Arietis"",""36 Arietis"",""27 Arietis"",""10 Arietis"",""31 Arietis""]}" );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=Alectrona", @"{""min_max"":[{""id64"":9467047323049,""name"":""Alectrona"",""x"":14.53125,""y"":-52.46875,""z"":2.5625}],""values"":[""Alectrona""]}" );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=LTT 1349", @"{""min_max"":[{""id64"":633675387594,""name"":""LTT 1349"",""x"":17.15625,""y"":-66.96875,""z"":-29.3125},{""id64"":1733120037586,""name"":""LTT 13490"",""x"":-1.8125,""y"":92.84375,""z"":-10.96875},{""id64"":2007930868426,""name"":""LTT 13497"",""x"":-38.03125,""y"":95.5625,""z"":-32.0},{""id64"":358730371810,""name"":""LTT 8260"",""x"":-3.625,""y"":-74.25,""z"":88.03125},{""id64"":560266758507,""name"":""LTT 3981"",""x"":96.625,""y"":62.6875,""z"":0.375},{""id64"":10494167403,""name"":""LTT 3799"",""x"":86.65625,""y"":59.78125,""z"":-17.625},{""id64"":83785421506,""name"":""LTT 10918"",""x"":-60.53125,""y"":-9.0625,""z"":-73.78125},{""id64"":83852563170,""name"":""LTT 6139"",""x"":7.375,""y"":43.15625,""z"":57.28125},{""id64"":84053856978,""name"":""LTT 2963"",""x"":106.59375,""y"":-22.1875,""z"":-11.09375},{""id64"":83315200690,""name"":""LTT 10174"",""x"":-340.46875,""y"":-561.84375,""z"":-156.40625},{""id64"":83919672010,""name"":""LTT 12033"",""x"":27.03125,""y"":18.53125,""z"":-59.4375},{""id64"":633675420386,""name"":""LTT 7370"",""x"":28.03125,""y"":-26.46875,""z"":58.21875},{""id64"":633474192074,""name"":""LTT 14474"",""x"":-68.0,""y"":60.46875,""z"":-28.40625},{""id64"":633608278754,""name"":""LTT 8318"",""x"":0.375,""y"":-84.21875,""z"":94.0},{""id64"":633608442578,""name"":""LTT 13470"",""x"":5.1875,""y"":102.84375,""z"":-9.21875},{""id64"":908553327298,""name"":""LTT 2042"",""x"":46.0,""y"":-62.78125,""z"":-73.65625},{""id64"":908620468954,""name"":""LTT 4586"",""x"":89.625,""y"":-12.125,""z"":51.4375},{""id64"":354074724707,""name"":""LTT 760"",""x"":-9.25,""y"":-115.875,""z"":-26.15625},{""id64"":354074741099,""name"":""LTT 740"",""x"":-3.75,""y"":-104.15625,""z"":-18.6875},{""id64"":354074757491,""name"":""LTT 6705"",""x"":-6.78125,""y"":35.59375,""z"":113.75}],""values"":[""LTT 1349"",""LTT 13490"",""LTT 13497"",""LTT 8260"",""LTT 3981"",""LTT 3799"",""LTT 10918"",""LTT 6139"",""LTT 2963"",""LTT 10174"",""LTT 12033"",""LTT 7370"",""LTT 14474"",""LTT 8318"",""LTT 13470"",""LTT 2042"",""LTT 4586"",""LTT 760"",""LTT 740"",""LTT 6705""]}" );
            fakeSpanshRestClient.Expect( "systems/field_values/system_names?q=Kaushpoos", @"{""min_max"":[{""id64"":5069001205129,""name"":""Kaushpoos"",""x"":-1.90625,""y"":65.1875,""z"":-79.65625}],""values"":[""Kaushpoos""]}" );

            fakeSpanshRestClient.Expect( "system/13864825529761",
                Encoding.UTF8.GetString( Tests.Properties.Resources.SpanshQuickStarSystemTz_Arietis ) );
            fakeSpanshRestClient.Expect( "system/9467047323049",
                Encoding.UTF8.GetString( Tests.Properties.Resources.SpanshQuickStarSystemAlectrona ) );
            fakeSpanshRestClient.Expect( "system/633675387594",
                Encoding.UTF8.GetString( Tests.Properties.Resources.SpanshQuickStarSystemLTT_1349 ) );
            fakeSpanshRestClient.Expect( "system/5069001205129",
                Encoding.UTF8.GetString( Tests.Properties.Resources.SpanshQuickStarSystemKaushpoos ) );

            var json = DeserializeJsonResource<JObject>( Tests.Properties.Resources.capi_profile );
            var profile = FrontierApiProfile.FromJson(json);

            Ship ship = FrontierApi.ShipFromJson((JObject)json["ship"]);
            Assert.IsNotNull(ship);
            List<Ship> shipyard = FrontierApi.ShipyardFromJson(ship, json);

            Assert.AreEqual("Testy", profile.Cmdr.name);

            Assert.AreEqual("Python", ship.model);

            Assert.AreEqual(7, ship.powerplant.@class);
            Assert.AreEqual("C", ship.powerplant.grade);
            Assert.AreEqual(9, ship.hardpoints.Count);

            var hardpoint1 = ship.hardpoints[0];
            Assert.AreEqual(3, hardpoint1.size);
            Assert.IsNotNull(hardpoint1.module);
            Assert.AreEqual(3, hardpoint1.size);
            Assert.AreEqual(3, hardpoint1.module.@class);
            Assert.AreEqual("E", hardpoint1.module.grade);
            Assert.AreEqual(126540, hardpoint1.module.price);
            Assert.AreEqual(140600, hardpoint1.module.value);

            Assert.AreEqual("7C", ship.powerplant.@class + ship.powerplant.grade);
            Assert.AreEqual(9, ship.compartments.Count);
            Assert.AreEqual(2, ship.compartments[8].size);
            Assert.AreEqual(null, ship.compartments[8].module);

            Assert.AreEqual(10, ship.cargocapacity);

            // 7 stored ships plus active ship
            Assert.AreEqual(8, shipyard.Count);

            // First stored ship is a Vulture at Snyder Enterprise
            var storedShip1 = shipyard[0];
            Assert.AreEqual("Vulture", storedShip1.model);
            Assert.AreEqual("TZ Arietis", storedShip1.starsystem);
            Assert.AreEqual("Snyder Enterprise", storedShip1.station);

            // Voss Dock has a MarketID of 3226643968
            Assert.AreEqual(3226643968, profile.LastStationMarketID);

            Assert.AreEqual(DateTime.MinValue, profile.timestamp);
        }

        [TestMethod]
        public void TestMarketIDFromProfile()
        {
            JObject json = DeserializeJsonResource<JObject>( Tests.Properties.Resources.capi_profile );
            var profile = FrontierApiProfile.FromJson(json);
            Assert.AreEqual(3226643968, profile.LastStationMarketID);
        }
    }
}
