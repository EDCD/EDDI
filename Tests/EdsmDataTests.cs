using EddiDataDefinitions;
using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Tests.Properties;

namespace UnitTests
{
    // Tests for the EDSM Service
    internal class FakeEdsmRestClient : IEdsmRestClient
    {
        public Dictionary<string, string> CannedContent = new Dictionary<string, string>();
        public Dictionary<string, object> CannedData = new Dictionary<string, object>();

        public Uri BuildUri(IRestRequest request)
        {
            return new Uri("fakeEDSM://" + request.Resource);
        }

        public IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            // this will throw if given a resource not in the canned dictionaries: that's OK
            string content = CannedContent[request.Resource];
            T data = (T)CannedData[request.Resource];
            IRestResponse<T> restResponse = new RestResponse<T>
            {
                Content = content,
                Data = data,
                ResponseStatus = ResponseStatus.Completed,
                StatusCode = HttpStatusCode.OK,
            };
            return restResponse;
        }

        public void Expect(string resource, string content, object data)
        {
            CannedContent[resource] = content;
            CannedData[resource] = data;
        }
    }

    [TestClass]
    public class EdsmDataTests : TestBase
    {
        FakeEdsmRestClient fakeEdsmRestClient;
        StarMapService fakeEdsmService;

        [TestInitialize]
        public void start()
        {
            fakeEdsmRestClient = new FakeEdsmRestClient();
            fakeEdsmService = new StarMapService(fakeEdsmRestClient);
            MakeSafe();
        }

        [TestMethod]
        public void TestBodies()
        {
            JObject response = DeserializeJsonResource<JObject>(Resources.edsmBodies);

            List<Body> bodies = fakeEdsmService.ParseStarMapBodiesParallel(response);

            Assert.IsNotNull(bodies);

            // Test our main star and belt
            Body star = bodies.Find(s => s.bodyname == "Shinrarta Dezhra");
            Assert.AreEqual("Shinrarta Dezhra", star.shortname);
            Assert.AreEqual(14923, star.EDSMID);
            Assert.AreEqual("Star", star.bodyType.invariantName);
            Assert.AreEqual("K", star.stellarclass);
            Assert.AreEqual("Shinrarta Dezhra", star.systemname);
            Assert.AreEqual(0, star.distance);
            Assert.IsTrue(Convert.ToBoolean(star.mainstar));
            Assert.AreEqual(8068, star.age);
            Assert.AreEqual("V", star.luminosityclass);
            Assert.AreEqual(7.129517, Convert.ToDouble(star.absolutemagnitude), 0.01);
            Assert.AreEqual(0.648438, Convert.ToDouble(star.solarmass), 0.01);
            Assert.AreEqual(0.7666015240833932, Convert.ToDouble(star.solarradius), 0.01);
            Assert.AreEqual(4343, Convert.ToDouble(star.temperature), 0.01);
            Assert.AreEqual(1847.1559259259259, Convert.ToDouble(star.orbitalperiod), 0.01);
            Assert.AreEqual(577.32870032374433, Convert.ToDouble(star.semimajoraxis), 0.01);
            Assert.AreEqual(0.018651, Convert.ToDouble(star.eccentricity), 0.01);
            Assert.AreEqual(21.11883, Convert.ToDouble(star.inclination), 0.01);
            Assert.AreEqual(201.271866, Convert.ToDouble(star.periapsis), 0.01);
            Assert.AreEqual(3.3057685908564816, Convert.ToDouble(star.rotationalperiod), 0.01);
            Assert.AreEqual(false, star.tidallylocked);
            Assert.AreEqual(-0.041915, Convert.ToDouble(star.tilt), 0.01);
            Assert.AreEqual(1, star.rings?.Count);
            Assert.AreEqual("Rocky", star.rings?[0].invariantComposition);
            Assert.AreEqual(1522275140, star.updatedat);
            Assert.IsNull(star.scannedDateTime);
            Assert.IsNull(star.mappedDateTime);

            // Test value estimation
            Assert.AreEqual(0, star.estimatedvalue);
            star.scannedDateTime = System.DateTime.UtcNow;
            Assert.AreEqual(1212, star.estimatedvalue);

            // Test landable high metal content world
            Body body = bodies.Find(s => s.bodyname == "Shinrarta Dezhra A 1");
            Assert.AreEqual("A 1", body.shortname);
            Assert.AreEqual(7058, body.EDSMID);
            Assert.AreEqual("Planet", body.bodyType.invariantName);
            Assert.AreEqual("High metal content world", body.planetClass.invariantName);
            Assert.AreEqual(40.472694, Convert.ToDouble(body.distance), 0.01);
            Assert.IsTrue(Convert.ToBoolean(body.landable));
            Assert.AreEqual(0.9955175545631726, Convert.ToDouble(body.gravity), 0.01);
            Assert.AreEqual(0.777331, Convert.ToDouble(body.earthmass), 0.01);
            Assert.AreEqual(5635.897, Convert.ToDouble(body.radius), 0.01);
            Assert.AreEqual(581, body.temperature);
            Assert.IsNull(body.pressure);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("No atmosphere", body.atmosphereclass.invariantName);
            Assert.AreEqual(0, body.atmospherecompositions.Count);
            Assert.AreEqual("Rock", body.solidcompositions[0].invariantName);
            Assert.AreEqual(66.83109999999999, Convert.ToDouble(body.solidcompositions[0].percent), 0.01);
            Assert.AreEqual("Metal", body.solidcompositions[1].invariantName);
            Assert.AreEqual(33.1689, Convert.ToDouble(body.solidcompositions[1].percent), 0.01);
            Assert.AreEqual("Not terraformable", body.terraformState.invariantName);
            Assert.AreEqual(10.463153935185185, Convert.ToDouble(body.orbitalperiod), 0.01);
            Assert.AreEqual(40.439289663517812, Convert.ToDouble(body.semimajoraxis), 0.01);
            Assert.AreEqual(0.002692, Convert.ToDouble(body.eccentricity), 0.01);
            Assert.AreEqual(-0.044359, Convert.ToDouble(body.inclination), 0.01);
            Assert.AreEqual(115.330589, Convert.ToDouble(body.periapsis), 0.01);
            Assert.AreEqual(10.341365740740741, Convert.ToDouble(body.rotationalperiod), 0.01);
            Assert.IsTrue(Convert.ToBoolean(body.tidallylocked));
            Assert.AreEqual(0.271899, Convert.ToDouble(body.tilt), 0.01);
            Assert.AreEqual(11, body.materials.Count);
            Assert.AreEqual("Manganese", body.materials[4].name);
            Assert.AreEqual(9.57628, Convert.ToDouble(body.materials[4].percentage), 0.01);
            Assert.AreEqual("Arsenic", body.materials[7].name);
            Assert.AreEqual(2.16406, Convert.ToDouble(body.materials[7].percentage), 0.01);
            Assert.AreEqual(1539922044, body.updatedat);
            Assert.IsNull(body.scannedDateTime);
            Assert.IsNull(body.mappedDateTime);

            // Test value estimation
            Assert.AreEqual(0, body.estimatedvalue);
            body.scannedDateTime = DateTime.UtcNow;
            Assert.AreEqual(14849, body.estimatedvalue);
            body.mappedDateTime = DateTime.UtcNow;
            Assert.AreEqual(64345, body.estimatedvalue);

            // Test terraformed body
            body = bodies.Find(s => s.bodyname == "Founders World");
            Assert.AreEqual("Founders World", body.shortname);
            Assert.AreEqual(12765, body.EDSMID);
            Assert.AreEqual("Planet", body.bodyType.invariantName);
            Assert.AreEqual("Earth-like world", body.planetClass.invariantName);
            Assert.AreEqual(324.465424, Convert.ToDouble(body.distance), 0.01);
            Assert.AreEqual(false, body.landable);
            Assert.AreEqual(0.9327034079798093, Convert.ToDouble(body.gravity), 0.01);
            Assert.AreEqual(0.69, Convert.ToDouble(body.earthmass), 0.01);
            Assert.AreEqual(5485.766, Convert.ToDouble(body.radius), 0.01);
            Assert.AreEqual(298, body.temperature);
            Assert.AreEqual(2.3013969590426844, Convert.ToDouble(body.pressure), 0.01);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("Suitable for water-based life", body.atmosphereclass.invariantName);
            Assert.AreEqual(2, body.atmospherecompositions.Count);
            Assert.AreEqual("Nitrogen", body.atmospherecompositions[0].invariantName);
            Assert.AreEqual(91.2489, Convert.ToDouble(body.atmospherecompositions[0].percent), 0.01);
            Assert.AreEqual("Oxygen", body.atmospherecompositions[1].invariantName);
            Assert.AreEqual(8.69037, Convert.ToDouble(body.atmospherecompositions[1].percent), 0.01);
            Assert.AreEqual("Rock", body.solidcompositions[0].invariantName);
            Assert.AreEqual(70, Convert.ToDouble(body.solidcompositions[0].percent), 0.01);
            Assert.AreEqual("Metal", body.solidcompositions[1].invariantName);
            Assert.AreEqual(30, Convert.ToDouble(body.solidcompositions[1].percent), 0.01);
            Assert.AreEqual("Terraformed", body.terraformState.invariantName);
            Assert.AreEqual(248.72930555555556, Convert.ToDouble(body.orbitalperiod), 0.01);
            Assert.AreEqual(334.33343980921632, Convert.ToDouble(body.semimajoraxis), 0.01);
            Assert.AreEqual(0.034386, Convert.ToDouble(body.eccentricity), 0.01);
            Assert.AreEqual(8.552103, Convert.ToDouble(body.inclination), 0.01);
            Assert.AreEqual(183.237366, Convert.ToDouble(body.periapsis), 0.01);
            Assert.AreEqual(41.96157696759259, Convert.ToDouble(body.rotationalperiod), 0.01);
            Assert.AreEqual(false, body.tidallylocked);
            Assert.AreEqual(0.373026, Convert.ToDouble(body.tilt), 0.01);
            Assert.AreEqual(0, body.materials.Count);
            Assert.AreEqual(1539922044, body.updatedat);
            Assert.IsNull(body.scannedDateTime);
            Assert.IsNull(body.mappedDateTime);

            // Test value estimation
            Assert.AreEqual(0, body.estimatedvalue);
            body.scannedDateTime = DateTime.UtcNow;
            Assert.AreEqual(276297, body.estimatedvalue);
            body.mappedDateTime = DateTime.UtcNow;
            Assert.AreEqual(1197287, body.estimatedvalue);

            // Test volcanic icy body
            body = bodies.Find(s => s.bodyname == "Shinrarta Dezhra AB 1 b");
            Assert.AreEqual("AB 1 b", body.shortname);
            Assert.AreEqual(8138660, body.EDSMID);
            Assert.AreEqual("Moon", body.bodyType.invariantName);
            Assert.AreEqual("Icy body", body.planetClass.invariantName);
            Assert.AreEqual(3250.803223, Convert.ToDouble(body.distance), 0.01);
            Assert.AreEqual(false, body.landable);
            Assert.AreEqual(0.08637144960988087, Convert.ToDouble(body.gravity), 0.01);
            Assert.AreEqual(0.004357, Convert.ToDouble(body.earthmass), 0.01);
            Assert.AreEqual(1432.49525, Convert.ToDouble(body.radius), 0.01);
            Assert.AreEqual(124, body.temperature);
            Assert.AreEqual(0.3520057673328399, Convert.ToDouble(body.pressure), 0.01);
            Assert.IsNotNull(body.volcanism);
            Assert.AreEqual("Minor", body.volcanism.invariantAmount);
            Assert.AreEqual("Nitrogen", body.volcanism.invariantComposition);
            Assert.AreEqual("Magma", body.volcanism.invariantType);
            Assert.AreEqual("Methane", body.atmosphereclass.invariantName);
            Assert.AreEqual(1, body.atmospherecompositions.Count);
            Assert.AreEqual("Ice", body.solidcompositions[0].invariantName);
            Assert.AreEqual(81.6586, Convert.ToDouble(body.solidcompositions[0].percent), 0.01);
            Assert.AreEqual("Rock", body.solidcompositions[1].invariantName);
            Assert.AreEqual(16.653200000000002, Convert.ToDouble(body.solidcompositions[1].percent), 0.01);
            Assert.AreEqual("Metal", body.solidcompositions[2].invariantName);
            Assert.AreEqual(1.6882000000000001, Convert.ToDouble(body.solidcompositions[2].percent), 0.01);
            Assert.AreEqual("Not terraformable", body.terraformState.invariantName);
            Assert.AreEqual(7.854005353009259, Convert.ToDouble(body.orbitalperiod), 0.01);
            Assert.AreEqual(4.9088274262056295, Convert.ToDouble(body.semimajoraxis), 0.01);
            Assert.AreEqual(0.000164, Convert.ToDouble(body.eccentricity), 0.01);
            Assert.AreEqual(0.025755, Convert.ToDouble(body.inclination), 0.01);
            Assert.AreEqual(300.792816, Convert.ToDouble(body.periapsis), 0.01);
            Assert.AreEqual(7.854289641203704, Convert.ToDouble(body.rotationalperiod), 0.01);
            Assert.IsTrue(Convert.ToBoolean(body.tidallylocked));
            Assert.AreEqual(-0.287234, Convert.ToDouble(body.tilt), 0.01);
            Assert.AreEqual(0, body.materials.Count);
            Assert.AreEqual(1539922044, body.updatedat);
            Assert.IsNull(body.scannedDateTime);
            Assert.IsNull(body.mappedDateTime);

            // Test value estimation
            Assert.AreEqual(0, body.estimatedvalue);
            body.scannedDateTime = DateTime.UtcNow;
            Assert.AreEqual(500, body.estimatedvalue);
            body.mappedDateTime = DateTime.UtcNow;
            Assert.AreEqual(1746, body.estimatedvalue);

            // Test ringed gas giant
            body = bodies.Find(s => s.bodyname == "Shinrarta Dezhra AB 2");
            Assert.AreEqual("AB 2", body.shortname);
            Assert.AreEqual(8138626, body.EDSMID);
            Assert.AreEqual("Planet", body.bodyType.invariantName);
            Assert.AreEqual("Class I gas giant", body.planetClass.invariantName);
            Assert.AreEqual(3767.557861, Convert.ToDouble(body.distance), 0.01);
            Assert.AreEqual(false, body.landable);
            Assert.AreEqual(3.8552143758323023, Convert.ToDouble(body.gravity), 0.01);
            Assert.AreEqual(484.303284, Convert.ToDouble(body.earthmass), 0.01);
            Assert.AreEqual(71485.664, Convert.ToDouble(body.radius), 0.01);
            Assert.AreEqual(123, body.temperature);
            Assert.IsNull(body.pressure);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("Gas giant", body.atmosphereclass.invariantName);
            Assert.AreEqual(2, body.atmospherecompositions.Count);
            Assert.AreEqual("Hydrogen", body.atmospherecompositions[0].invariantName);
            Assert.AreEqual(72.8469, Convert.ToDouble(body.atmospherecompositions[0].percent), 0.01);
            Assert.AreEqual("Helium", body.atmospherecompositions[1].invariantName);
            Assert.AreEqual(27.1531, Convert.ToDouble(body.atmospherecompositions[1].percent), 0.01);
            Assert.AreEqual(0, body.solidcompositions.Count);
            Assert.AreEqual("Not terraformable", body.terraformState.invariantName);
            Assert.AreEqual(7354.8133333333335, Convert.ToDouble(body.orbitalperiod), 0.01);
            Assert.AreEqual(3765.2874442358393, Convert.ToDouble(body.semimajoraxis), 0.01);
            Assert.AreEqual(0.007439, Convert.ToDouble(body.eccentricity), 0.01);
            Assert.AreEqual(-0.406597, Convert.ToDouble(body.inclination), 0.01);
            Assert.AreEqual(305.503632, Convert.ToDouble(body.periapsis), 0.01);
            Assert.AreEqual(1.1907941804166666, Convert.ToDouble(body.rotationalperiod), 0.01);
            Assert.AreEqual(false, body.tidallylocked);
            Assert.AreEqual(0.868699, Convert.ToDouble(body.tilt), 0.01);
            Assert.AreEqual(0, body.materials.Count);
            Assert.IsNotNull(body.rings);
            Assert.AreEqual(1, body.rings.Count);
            Assert.AreEqual("Shinrarta Dezhra AB 2 A Ring", body.rings[0].name);
            Assert.AreEqual("Icy", body.rings[0].invariantComposition);
            Assert.AreEqual(239990000000, body.rings[0].mass);
            Assert.AreEqual(130900, body.rings[0].innerradius);
            Assert.AreEqual(273390, body.rings[0].outerradius);
            Assert.AreEqual("Common", body.reserveLevel.invariantName);
            Assert.AreEqual(1539922044, body.updatedat);
            Assert.IsNull(body.scannedDateTime);
            Assert.IsNull(body.mappedDateTime);

            // Test value estimation
            Assert.AreEqual(0, body.estimatedvalue);
            body.scannedDateTime = DateTime.UtcNow;
            Assert.AreEqual(4883, body.estimatedvalue);
            body.mappedDateTime = DateTime.UtcNow;
            Assert.AreEqual(21161, body.estimatedvalue);
        }

        [TestMethod]
        public void TestStations()
        {
            // Test stations data
            JObject response = DeserializeJsonResource<JObject>(Resources.edsmStations);

            List<Station> stations = fakeEdsmService.ParseStarMapStationsParallel(response);

            Assert.IsNotNull(stations);

            // Test Jameson Memorial
            Station station = stations.Find(s => s.name == "Jameson Memorial");
            Assert.AreEqual(65, station.EDSMID);
            Assert.AreEqual(128666762, station.marketId);
            Assert.AreEqual("Orbis Starport", station.Model.invariantName);
            Assert.AreEqual(324.925354M, station.distancefromstar);
            Assert.AreEqual("Pilots Federation", station.Faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", station.Faction.Government.invariantName);
            Assert.AreEqual("Pilots Federation Local Branch", station.Faction.name);
            Assert.AreEqual(80576, station.Faction.EDSMID);
            Assert.AreEqual(2, station.economies.Count);
            Assert.AreEqual("High Tech", station.economies[0]);
            Assert.AreEqual("Industrial", station.economies[1]);
            Assert.IsTrue(Convert.ToBoolean(station.hasmarket));
            Assert.IsTrue(Convert.ToBoolean(station.hasshipyard));
            Assert.IsTrue(Convert.ToBoolean(station.hasoutfitting));
            Assert.IsTrue(Convert.ToBoolean(station.hasdocking));
            Assert.IsTrue(Convert.ToBoolean(station.hasrearm));
            Assert.IsTrue(Convert.ToBoolean(station.hasrefuel));
            Assert.IsTrue(Convert.ToBoolean(station.hasrepair));
            Assert.IsTrue(station.stationServices.Exists(s => s.invariantName == "Commodities"));
            Assert.IsTrue(station.stationServices.Exists(s => s.invariantName == "Technology Broker"));
            Assert.AreEqual(1540189980, station.updatedat);
            Assert.AreEqual(1540189980, station.shipyardupdatedat);
            Assert.AreEqual(1540189980, station.commoditiesupdatedat);
            Assert.AreEqual(1540189980, station.outfittingupdatedat);

            // Test Jameson Base (Engineer's workshop)
            station = stations.Find(s => s.name == "Jameson Base");
            Assert.AreEqual(285, station.EDSMID);
            Assert.AreEqual(128679815, station.marketId);
            Assert.AreEqual("Surface Outpost", station.Model.invariantName);
            Assert.AreEqual(40.333652M, station.distancefromstar);
            Assert.AreEqual("Independent", station.Faction.Allegiance.invariantName);
            Assert.AreEqual("Engineer", station.Faction.Government.invariantName);
            Assert.AreEqual("Lori Jameson", station.Faction.name);
            Assert.IsNull(station.Faction.EDSMID);
            Assert.AreEqual(2, station.economies.Count);
            Assert.AreEqual("Colony", station.economies[0]);
            Assert.AreEqual("None", station.economies[1]);
            Assert.AreEqual(false, station.hasmarket);
            Assert.AreEqual(false, station.hasshipyard);
            Assert.IsTrue(Convert.ToBoolean(station.hasoutfitting));
            Assert.IsTrue(Convert.ToBoolean(station.hasdocking));
            Assert.IsTrue(Convert.ToBoolean(station.hasrearm));
            Assert.IsTrue(Convert.ToBoolean(station.hasrefuel));
            Assert.IsTrue(Convert.ToBoolean(station.hasrepair));
            Assert.IsTrue(station.stationServices.Exists(s => s.invariantName == "Contacts"));
            Assert.AreEqual(1540179943, station.updatedat);
            Assert.AreEqual(null, station.shipyardupdatedat);
            Assert.AreEqual(null, station.commoditiesupdatedat);
            Assert.AreEqual(1540179943, station.outfittingupdatedat);
        }

        [TestMethod]
        public void TestFactions()
        {
            // Test factions data
            JObject response = DeserializeJsonResource<JObject>(Resources.edsmFactions);
            string systemName = (string)response["name"];
            List<Faction> factions = fakeEdsmService.ParseStarMapFactionsParallel(response, systemName);

            Assert.IsNotNull(factions);

            // Test The Dark Wheel
            var faction = factions.Find(s => s.name == "The Dark Wheel");
            var presence = faction.presences.FirstOrDefault(p => p.systemName == systemName);
            Assert.AreEqual(702, faction.EDSMID);
            Assert.AreEqual("Independent", faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", faction.Government.invariantName);
            Assert.AreEqual(49.8M, presence?.influence);
            Assert.AreEqual("Boom", presence?.FactionState?.invariantName);
            Assert.AreEqual(3, presence?.ActiveStates.Count);
            Assert.AreEqual(0, presence?.RecoveringStates.Count);
            Assert.AreEqual(0, presence?.PendingStates.Count);
            Assert.IsTrue(new List<FactionState>() { FactionState.FromEDName("Boom"), FactionState.FromName("Civil liberty"), FactionState.FromName("Election") }.DeepEquals(presence?.ActiveStates));
            Assert.IsNotNull(faction.isplayer);
            Assert.IsFalse(Convert.ToBoolean(faction.isplayer));
            Assert.AreEqual(1539928089, faction.updatedat);

            // Test The Pilots Federation
            faction = factions.Find(s => s.name == "The Pilots Federation");
            presence = faction.presences.FirstOrDefault(p => p.systemName == systemName);
            Assert.AreEqual(61, faction.EDSMID);
            Assert.AreEqual("Independent", faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", faction.Government.invariantName);
            Assert.AreEqual(0M, presence?.influence);
            Assert.AreEqual("None", presence?.FactionState?.invariantName);
            Assert.AreEqual(0, presence?.ActiveStates.Count);
            Assert.AreEqual(0, presence?.RecoveringStates.Count);
            Assert.AreEqual(0, presence?.PendingStates.Count);
            Assert.IsNotNull(faction.isplayer);
            Assert.IsFalse(Convert.ToBoolean(faction.isplayer));
            Assert.AreEqual(1539923616, faction.updatedat);

            // Test LTT 4487 Industry
            faction = factions.Find(s => s.name == "LTT 4487 Industry");
            presence = faction.presences.FirstOrDefault(p => p.systemName == systemName);
            Assert.AreEqual(434, faction.EDSMID);
            Assert.AreEqual("Federation", faction.Allegiance.invariantName);
            Assert.AreEqual("Corporate", faction.Government.invariantName);
            Assert.AreEqual(26.1M, presence?.influence);
            Assert.AreEqual("None", presence?.FactionState?.invariantName);
            Assert.AreEqual(1, presence?.ActiveStates.Count);
            Assert.IsTrue(new List<FactionState>() { FactionState.FromName("Civil war") }.DeepEquals(presence?.ActiveStates));
            Assert.AreEqual(0, presence?.RecoveringStates.Count);
            Assert.AreEqual(1, presence?.PendingStates.Count);
            Assert.IsTrue(new List<FactionTrendingState>() { new FactionTrendingState(FactionState.FromName("Boom"), 1) }.DeepEquals(presence?.PendingStates));
            Assert.IsNotNull(faction.isplayer);
            Assert.IsFalse(Convert.ToBoolean(faction.isplayer));
            Assert.AreEqual(1539928985, faction.updatedat);
        }

        [TestMethod]
        public void TestSystem()
        {
            // Test system
            Dictionary<string, object> response = DeserializeJsonResource<Dictionary<string, object>>(Tests.Properties.Resources.edsmSystem);

            StarSystem system = fakeEdsmService.ParseStarMapSystem(JObject.FromObject(response));

            // Test Shinrarta Dezhra
            Assert.AreEqual("Shinrarta Dezhra", system.systemname);
            Assert.AreEqual(4345, system.EDSMID);
            Assert.AreEqual(55.71875M, system.x);
            Assert.AreEqual(17.59375M, system.y);
            Assert.AreEqual(27.15625M, system.z);
            Assert.IsTrue(system.requirespermit);
            Assert.AreEqual("Founders World", system.permitname);
            Assert.AreEqual("Pilots Federation", system.Faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", system.Faction.Government.invariantName);
            Assert.AreEqual("Pilots Federation Local Branch", system.Faction.name);
            Assert.AreEqual("None", system.Faction.presences.FirstOrDefault(p => p.systemName == system.systemname)?.FactionState?.invariantName);
            Assert.AreEqual(85206935, system.population);
            Assert.AreEqual("Common", system.Reserve.invariantName);
            Assert.AreEqual("High", system.securityLevel.invariantName);
            Assert.AreEqual("High Tech", system.Economies[0].invariantName);
            Assert.AreEqual("Industrial", system.Economies[1].invariantName);
        }

        [TestMethod]
        public void TestSystems()
        {
            // Setup
            string resource = "api-v1/systems";
            string json = Encoding.UTF8.GetString(Resources.CapitalSystems);
            List<JObject> data = new List<JObject> { new JObject(), new JObject(), new JObject() };
            fakeEdsmRestClient.Expect(resource, json, data);

            // Act
            string[] systemNames = new string[] { "Sol", "Achenar", "Alioth" };
            List<StarSystem> starSystems = fakeEdsmService.GetStarMapSystems(systemNames, showCoordinates: true);

            // Assert
            Assert.AreEqual(3, starSystems?.Count);
        }

        [TestMethod]
        public void TestSystemsSphere()
        {
            string resource = "api-v1/sphere-systems";
            string json = Encoding.UTF8.GetString(Resources.sphereAroundSol);
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);
            string systemName = "Sol";

            List<Dictionary<string, object>> sphereSystems = fakeEdsmService.GetStarMapSystemsSphere(systemName, 0, 10, false, false, false, false);
            Assert.AreEqual(12, sphereSystems.Count);
        }

        [TestMethod]
        public void TestSystemsCube()
        {
            // Setup
            string resource = "api-v1/cube-systems";
            string json = Encoding.UTF8.GetString(Resources.cubeSystemsAroundSol);
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);
            string systemName = "Sol";

            List<StarSystem> starSystems = fakeEdsmService.GetStarMapSystemsCube(systemName, 25, false, false, false, false);
            Assert.AreEqual(51, starSystems.Count);
        }

        [TestMethod]
        public void TestSystemsCubeUnknown()
        {
            // Setup
            string resource = "api-v1/cube-systems";
            string json = "[]";
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);

            // Act
            string systemName = "No such system";
            List<StarSystem> starSystems = fakeEdsmService.GetStarMapSystemsCube(systemName, 15, false, false, false, false);

            // Assert
            Assert.IsTrue(starSystems == null || starSystems.Count == 0);
        }

        [TestMethod]
        public void TestUnknown()
        {
            // Setup
            string resource = "api-v1/systems";
            string json = "[]";
            List<JObject> data = new List<JObject>();
            fakeEdsmRestClient.Expect(resource, json, data);

            // Act
            StarSystem system = fakeEdsmService.GetStarMapSystem("No such system", showCoordinates: false);

            // Assert
            // Unknown systems shall return null from here. We create a synthetic system in DataProviderService.cs if this returns null;
            Assert.IsNull(system);
        }

        [TestMethod]
        public void TestTraffic()
        {
            // Test pilot traffic data
            JObject response = DeserializeJsonResource<JObject>(Resources.edsmTraffic);

            Traffic traffic = fakeEdsmService.ParseStarMapTraffic(response);

            Assert.IsNotNull(traffic);
            Assert.AreEqual(9631, traffic.total);
            Assert.AreEqual(892, traffic.week);
            Assert.AreEqual(193, traffic.day);
        }

        [TestMethod]
        public void TestDeaths()
        {
            // Test pilot mortality data
            JObject response = DeserializeJsonResource<JObject>(Resources.edsmDeaths);

            Traffic deaths = fakeEdsmService.ParseStarMapDeaths(response);

            Assert.IsNotNull(deaths);
            Assert.AreEqual(1068, deaths.total);
            Assert.AreEqual(31, deaths.week);
            Assert.AreEqual(4, deaths.day);
        }

        [TestMethod]
        public void TestTrafficUnknown()
        {
            // Setup
            JObject response = new JObject();

            // Act
            Traffic traffic = fakeEdsmService.ParseStarMapTraffic(response);

            // Assert
            Assert.IsNotNull(traffic);
            Assert.AreEqual(0, traffic.total);
            Assert.AreEqual(0, traffic.week);
            Assert.AreEqual(0, traffic.day);
        }
    }
}
