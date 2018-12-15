using EddiDataDefinitions;
using EddiStarMapService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rollbar;
using System.Collections.Generic;
using Utilities;

namespace UnitTests
{
    // Tests for the EDDB Service

    [TestClass]
    public class EdsmDataTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        [DeploymentItem("edsmBodies.json")]
        public void TestBodies()
        {
            string jsonString = System.IO.File.ReadAllText("edsmBodies.json");
            JObject response = JsonConvert.DeserializeObject<JObject>(jsonString);

            PrivateType starMapService = new PrivateType(typeof(StarMapService));
            List<Body> bodies = (List<Body>)starMapService.InvokeStatic("ParseStarMapBodies", new object[] { response });

            Assert.IsNotNull(bodies);

            // Test our main star and belt
            Body star = bodies.Find(s => s.name == "Shinrarta Dezhra");
            Assert.AreEqual("Shinrarta Dezhra", star.shortname);
            Assert.AreEqual(14923, star.EDSMID);
            Assert.AreEqual("Star", star.Type.invariantName);
            Assert.AreEqual("K", star.stellarclass);
            Assert.AreEqual("Shinrarta Dezhra", star.systemname);
            Assert.AreEqual(0, star.distance);
            Assert.IsTrue((bool)star.mainstar);
            Assert.AreEqual(8068, star.age);
            Assert.AreEqual("V", star.luminosityclass);
            Assert.AreEqual(7.129517, (double)star.absoluteMagnitude, 0.01);
            Assert.AreEqual(0.648438, (double)star.solarmass, 0.01);
            Assert.AreEqual(0.7666015240833932, (double)star.solarradius, 0.01);
            Assert.AreEqual(4343, (double)star.temperature, 0.01);
            Assert.AreEqual(1847.1559259259259, (double)star.orbitalperiod, 0.01);
            Assert.AreEqual(577.32870032374433, (double)star.semimajoraxis, 0.01);
            Assert.AreEqual(0.018651, (double)star.eccentricity, 0.01);
            Assert.AreEqual(21.11883, (double)star.inclination, 0.01);
            Assert.AreEqual(201.271866, (double)star.periapsis, 0.01);
            Assert.AreEqual(3.3057685908564816, (double)star.rotationalperiod, 0.01);
            Assert.AreEqual(false, star.tidallylocked);
            Assert.AreEqual(-0.041915, (double)star.tilt, 0.01);
            Assert.AreEqual(1, star.rings?.Count);
            Assert.AreEqual("Rocky", star.rings[0].invariantComposition);
            Assert.AreEqual(1522275140, star.updatedat);

            // Test landable high metal content world
            Body body = bodies.Find(s => s.name == "Shinrarta Dezhra A 1");
            Assert.AreEqual("A 1", body.shortname);
            Assert.AreEqual(7058, body.EDSMID);
            Assert.AreEqual("Planet", body.Type.invariantName);
            Assert.AreEqual("High metal content world", body.planetClass.invariantName);
            Assert.AreEqual(40.472694, (double)body.distance, 0.01);
            Assert.IsTrue((bool)body.landable);
            Assert.AreEqual(0.9955175545631726, (double)body.gravity, 0.01);
            Assert.AreEqual(0.777331, (double)body.earthmass, 0.01);
            Assert.AreEqual(5635.897, (double)body.radius, 0.01);
            Assert.AreEqual(581, body.temperature);
            Assert.IsNull(body.pressure);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("No atmosphere", body.atmosphereclass.invariantName);
            Assert.AreEqual(0, body.atmospherecompositions.Count);
            Assert.AreEqual("Rock", body.solidcompositions[0].invariantName);
            Assert.AreEqual(66.83109999999999, (double)body.solidcompositions[0].percent, 0.01);
            Assert.AreEqual("Metal", body.solidcompositions[1].invariantName);
            Assert.AreEqual(33.1689, (double)body.solidcompositions[1].percent, 0.01);
            Assert.AreEqual("Not terraformable", body.terraformState.invariantName);
            Assert.AreEqual(10.463153935185185, (double)body.orbitalperiod, 0.01);
            Assert.AreEqual(40.439289663517812, (double)body.semimajoraxis, 0.01);
            Assert.AreEqual(0.002692, (double)body.eccentricity, 0.01);
            Assert.AreEqual(-0.044359, (double)body.inclination, 0.01);
            Assert.AreEqual(115.330589, (double)body.periapsis, 0.01);
            Assert.AreEqual(10.341365740740741, (double)body.rotationalperiod, 0.01);
            Assert.IsTrue((bool)body.tidallylocked);
            Assert.AreEqual(0.271899, (double)body.tilt, 0.01);
            Assert.AreEqual(11, body.materials.Count);
            Assert.AreEqual("Manganese", body.materials[4].name);
            Assert.AreEqual(9.57628, (double)body.materials[4].percentage, 0.01);
            Assert.AreEqual("Arsenic", body.materials[7].name);
            Assert.AreEqual(2.16406, (double)body.materials[7].percentage, 0.01);
            Assert.AreEqual(1539922044, body.updatedat);

            // Test terraformed body
            body = bodies.Find(s => s.name == "Founders World");
            Assert.AreEqual("Founders World", body.shortname);
            Assert.AreEqual(12765, body.EDSMID);
            Assert.AreEqual("Planet", body.Type.invariantName);
            Assert.AreEqual("Earth-like world", body.planetClass.invariantName);
            Assert.AreEqual(324.465424, (double)body.distance, 0.01);
            Assert.AreEqual(false, body.landable);
            Assert.AreEqual(0.9327034079798093, (double)body.gravity, 0.01);
            Assert.AreEqual(0.69, (double)body.earthmass, 0.01);
            Assert.AreEqual(5485.766, (double)body.radius, 0.01);
            Assert.AreEqual(298, body.temperature);
            Assert.AreEqual(2.3013969590426844, (double)body.pressure, 0.01);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("Suitable for water-based life", body.atmosphereclass.invariantName);
            Assert.AreEqual(2, body.atmospherecompositions.Count);
            Assert.AreEqual("Nitrogen", body.atmospherecompositions[0].invariantName);
            Assert.AreEqual(91.2489, (double)body.atmospherecompositions[0].percent, 0.01);
            Assert.AreEqual("Oxygen", body.atmospherecompositions[1].invariantName);
            Assert.AreEqual(8.69037, (double)body.atmospherecompositions[1].percent, 0.01);
            Assert.AreEqual("Rock", body.solidcompositions[0].invariantName);
            Assert.AreEqual(70, (double)body.solidcompositions[0].percent, 0.01);
            Assert.AreEqual("Metal", body.solidcompositions[1].invariantName);
            Assert.AreEqual(30, (double)body.solidcompositions[1].percent, 0.01);
            Assert.AreEqual("Terraformed", body.terraformState.invariantName);
            Assert.AreEqual(248.72930555555556, (double)body.orbitalperiod, 0.01);
            Assert.AreEqual(334.33343980921632, (double)body.semimajoraxis, 0.01);
            Assert.AreEqual(0.034386, (double)body.eccentricity, 0.01);
            Assert.AreEqual(8.552103, (double)body.inclination, 0.01);
            Assert.AreEqual(183.237366, (double)body.periapsis, 0.01);
            Assert.AreEqual(41.96157696759259, (double)body.rotationalperiod, 0.01);
            Assert.AreEqual(false, body.tidallylocked);
            Assert.AreEqual(0.373026, (double)body.tilt, 0.01);
            Assert.AreEqual(0, body.materials.Count);
            Assert.AreEqual(1539922044, body.updatedat);

            // Test volcanic icy body
            body = bodies.Find(s => s.name == "Shinrarta Dezhra AB 1 b");
            Assert.AreEqual("AB 1 b", body.shortname);
            Assert.AreEqual(8138660, body.EDSMID);
            Assert.AreEqual("Planet", body.Type.invariantName);
            Assert.AreEqual("Icy body", body.planetClass.invariantName);
            Assert.AreEqual(3250.803223, (double)body.distance, 0.01);
            Assert.AreEqual(false, body.landable);
            Assert.AreEqual(0.08637144960988087, (double)body.gravity, 0.01);
            Assert.AreEqual(0.004357, (double)body.earthmass, 0.01);
            Assert.AreEqual(1432.49525, (double)body.radius, 0.01);
            Assert.AreEqual(124, body.temperature);
            Assert.AreEqual(0.3520057673328399, (double)body.pressure, 0.01);
            Assert.IsNotNull(body.volcanism);
            Assert.AreEqual("Minor", body.volcanism.invariantAmount);
            Assert.AreEqual("Nitrogen", body.volcanism.invariantComposition);
            Assert.AreEqual("Magma", body.volcanism.invariantType);
            Assert.AreEqual("Methane", body.atmosphereclass.invariantName);
            Assert.AreEqual(1, body.atmospherecompositions.Count);
            Assert.AreEqual("Ice", body.solidcompositions[0].invariantName);
            Assert.AreEqual(81.6586, (double)body.solidcompositions[0].percent, 0.01);
            Assert.AreEqual("Rock", body.solidcompositions[1].invariantName);
            Assert.AreEqual(16.653200000000002, (double)body.solidcompositions[1].percent, 0.01);
            Assert.AreEqual("Metal", body.solidcompositions[2].invariantName);
            Assert.AreEqual(1.6882000000000001, (double)body.solidcompositions[2].percent, 0.01);
            Assert.AreEqual("Not terraformable", body.terraformState.invariantName);
            Assert.AreEqual(7.854005353009259, (double)body.orbitalperiod, 0.01);
            Assert.AreEqual(4.9088274262056295, (double)body.semimajoraxis, 0.01);
            Assert.AreEqual(0.000164, (double)body.eccentricity, 0.01);
            Assert.AreEqual(0.025755, (double)body.inclination, 0.01);
            Assert.AreEqual(300.792816, (double)body.periapsis, 0.01);
            Assert.AreEqual(7.854289641203704, (double)body.rotationalperiod, 0.01);
            Assert.IsTrue((bool)body.tidallylocked);
            Assert.AreEqual(-0.287234, (double)body.tilt, 0.01);
            Assert.AreEqual(0, body.materials.Count);
            Assert.AreEqual(1539922044, body.updatedat);

            // Test ringed gas giant
            body = bodies.Find(s => s.name == "Shinrarta Dezhra AB 2");
            Assert.AreEqual("AB 2", body.shortname);
            Assert.AreEqual(8138626, body.EDSMID);
            Assert.AreEqual("Planet", body.Type.invariantName);
            Assert.AreEqual("Class I gas giant", body.planetClass.invariantName);
            Assert.AreEqual(3767.557861, (double)body.distance, 0.01);
            Assert.AreEqual(false, body.landable);
            Assert.AreEqual(3.8552143758323023, (double)body.gravity, 0.01);
            Assert.AreEqual(484.303284, (double)body.earthmass, 0.01);
            Assert.AreEqual(71485.664, (double)body.radius, 0.01);
            Assert.AreEqual(123, body.temperature);
            Assert.IsNull(body.pressure);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("Gas giant", body.atmosphereclass.invariantName);
            Assert.AreEqual(2, body.atmospherecompositions.Count);
            Assert.AreEqual("Hydrogen", body.atmospherecompositions[0].invariantName);
            Assert.AreEqual(72.8469, (double)body.atmospherecompositions[0].percent, 0.01);
            Assert.AreEqual("Helium", body.atmospherecompositions[1].invariantName);
            Assert.AreEqual(27.1531, (double)body.atmospherecompositions[1].percent, 0.01);
            Assert.AreEqual(0, body.solidcompositions.Count);
            Assert.AreEqual("Not terraformable", body.terraformState.invariantName);
            Assert.AreEqual(7354.8133333333335, (double)body.orbitalperiod, 0.01);
            Assert.AreEqual(3765.2874442358393, (double)body.semimajoraxis, 0.01);
            Assert.AreEqual(0.007439, (double)body.eccentricity, 0.01);
            Assert.AreEqual(-0.406597, (double)body.inclination, 0.01);
            Assert.AreEqual(305.503632, (double)body.periapsis, 0.01);
            Assert.AreEqual(1.1907941804166666, (double)body.rotationalperiod, 0.01);
            Assert.AreEqual(false, body.tidallylocked);
            Assert.AreEqual(0.868699, (double)body.tilt, 0.01);
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
        }

        [TestMethod]
        [DeploymentItem("edsmStations.json")]
        public void TestStations()
        {
            // Test stations data
            string jsonString = System.IO.File.ReadAllText("edsmStations.json");
            JObject response = JsonConvert.DeserializeObject<JObject>(jsonString);

            PrivateType starMapService = new PrivateType(typeof(StarMapService));
            List<Station> stations = (List<Station>)starMapService.InvokeStatic("ParseStarMapStations", new object[] { response });

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
            Assert.IsTrue((bool)station.hasmarket);
            Assert.IsTrue((bool)station.hasshipyard);
            Assert.IsTrue((bool)station.hasoutfitting);
            Assert.IsTrue((bool)station.hasdocking);
            Assert.IsTrue((bool)station.hasrearm);
            Assert.IsTrue((bool)station.hasrefuel);
            Assert.IsTrue((bool)station.hasrepair);
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
            Assert.AreEqual("Surface Station", station.Model.invariantName);
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
            Assert.IsTrue((bool)station.hasoutfitting);
            Assert.IsTrue((bool)station.hasdocking);
            Assert.IsTrue((bool)station.hasrearm);
            Assert.IsTrue((bool)station.hasrefuel);
            Assert.IsTrue((bool)station.hasrepair);
            Assert.IsTrue(station.stationServices.Exists(s => s.invariantName == "Contacts"));
            Assert.AreEqual(1540179943, station.updatedat);
            Assert.AreEqual(null, station.shipyardupdatedat);
            Assert.AreEqual(null, station.commoditiesupdatedat);
            Assert.AreEqual(1540179943, station.outfittingupdatedat);
        }

        [TestMethod]
        [DeploymentItem("edsmFactions.json")]
        public void TestFactions()
        {
            // Test factions data
            string jsonString = System.IO.File.ReadAllText("edsmFactions.json");
            JObject response = JsonConvert.DeserializeObject<JObject>(jsonString);

            PrivateType starMapService = new PrivateType(typeof(StarMapService));
            List<Faction> factions = (List<Faction>)starMapService.InvokeStatic("ParseStarMapFactions", new object[] { response });

            Assert.IsNotNull(factions);

            // Test The Dark Wheel
            Faction faction = factions.Find(s => s.name == "The Dark Wheel");
            Assert.AreEqual(702, faction.EDSMID);
            Assert.AreEqual("Independent", faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", faction.Government.invariantName);
            Assert.AreEqual(49.8M, faction.influence);
            Assert.AreEqual("Boom", faction.FactionState.invariantName);
            Assert.IsFalse(faction.isplayer);
            Assert.AreEqual(1539928089, faction.updatedat);

            // Test The Pilots Federation
            faction = factions.Find(s => s.name == "The Pilots Federation");
            Assert.AreEqual(61, faction.EDSMID);
            Assert.AreEqual("Independent", faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", faction.Government.invariantName);
            Assert.AreEqual(0M, faction.influence);
            Assert.AreEqual("None", faction.FactionState.invariantName);
            Assert.IsFalse(faction.isplayer);
            Assert.AreEqual(1539923616, faction.updatedat);

            // Test LTT 4487 Industry
            faction = factions.Find(s => s.name == "LTT 4487 Industry");
            Assert.AreEqual(434, faction.EDSMID);
            Assert.AreEqual("Federation", faction.Allegiance.invariantName);
            Assert.AreEqual("Corporate", faction.Government.invariantName);
            Assert.AreEqual(26.1M, faction.influence);
            Assert.AreEqual("None", faction.FactionState.invariantName);
            Assert.IsFalse(faction.isplayer);
            Assert.AreEqual(1539928985, faction.updatedat);
        }

        [TestMethod]
        [DeploymentItem("edsmSystem.json")]
        public void TestSystem()
        {
            // Test system
            string jsonString = System.IO.File.ReadAllText("edsmSystem.json");
            Dictionary<string, object> response = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

            PrivateType starMapService = new PrivateType(typeof(StarMapService));
            StarSystem system = (StarSystem)starMapService.InvokeStatic("ParseStarMapSystem", new object[] { JObject.FromObject(response), "Shinrarta Dezhra" });

            // Test Shinrarta Dezhra
            Assert.AreEqual("Shinrarta Dezhra", system.name);
            Assert.AreEqual(4345, system.EDSMID);
            Assert.AreEqual(55.71875M, system.x);
            Assert.AreEqual(17.59375M, system.y);
            Assert.AreEqual(27.15625M, system.z);
            Assert.IsTrue(system.requirespermit);
            Assert.AreEqual("Founders World", system.permitname);
            Assert.AreEqual("Pilots Federation", system.Faction.Allegiance.invariantName);
            Assert.AreEqual("Democracy", system.Faction.Government.invariantName);
            Assert.AreEqual("Pilots Federation Local Branch", system.Faction.name);
            Assert.AreEqual("None", system.Faction.FactionState.invariantName);
            Assert.AreEqual(85206935, system.population);
            Assert.AreEqual("Common", system.Reserve.invariantName);
            Assert.AreEqual("High", system.securityLevel.invariantName);
            Assert.AreEqual("High Tech", system.Economies[0].invariantName);
            Assert.AreEqual("Industrial", system.Economies[1].invariantName);
        }

        [TestMethod]
        public void TestSystems()
        {
            string[] systemNames = new string[] { "Sol", "Achenar", "Alioth" };
            List<StarSystem> starSystems = StarMapService.GetStarMapSystems(systemNames, true, false);
            Assert.AreEqual(3, starSystems.Count);
        }

        [TestMethod]
        public void TestUnknown()
        {
            // Unknown systems shall return null from here. We create a synthetic system in DataProviderService.cs if this returns null;
            PrivateType MockStarMapService = new PrivateType(typeof(StarMapService));
            StarSystem system = (StarSystem)MockStarMapService.InvokeStatic("GetStarMapSystem", new object[] { "No such system", false, false });

            Assert.IsNull(system);
        }

        [TestMethod]
        [DeploymentItem("edsmTraffic.json")]
        public void TestTraffic()
        {
            // Test pilot traffic data
            string jsonString = System.IO.File.ReadAllText("edsmTraffic.json");
            JObject response = JsonConvert.DeserializeObject<JObject>(jsonString);

            PrivateType starMapService = new PrivateType(typeof(StarMapService));
            Dictionary<string, object> traffic = (Dictionary<string, object>)starMapService.InvokeStatic("ParseStarMapTraffic", new object[] { response });

            Assert.IsNotNull(traffic);
            Assert.AreEqual(9631, (long?)traffic["total"]);
            Assert.AreEqual(892, (long?)traffic["week"]);
            Assert.AreEqual(193, (long?)traffic["day"]);
        }

        [TestMethod]
        [DeploymentItem("edsmDeaths.json")]
        public void TestDeaths()
        {
            // Test pilot mortality data
            string jsonString = System.IO.File.ReadAllText("edsmDeaths.json");
            JObject response = JsonConvert.DeserializeObject<JObject>(jsonString);

            PrivateType starMapService = new PrivateType(typeof(StarMapService));
            Dictionary<string, object> deaths = (Dictionary<string, object>)starMapService.InvokeStatic("ParseStarMapDeaths", new object[] { response });

            Assert.IsNotNull(deaths);
            Assert.AreEqual(1068, (long?)deaths["total"]);
            Assert.AreEqual(31, (long?)deaths["week"]);
            Assert.AreEqual(4, (long?)deaths["day"]);
        }
    }
}