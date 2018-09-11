using EddiDataDefinitions;
using EddiEddbService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Rollbar;
using System.Collections.Generic;

namespace UnitTests
{
    // Tests for the EDDB Service

    [TestClass]
    public class EddbDataTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        public void TestEddbDataMultiParameters()
        {
            List<KeyValuePair<string, object>> queryList = new List<KeyValuePair<string, object>>()
            {
                new KeyValuePair<string, object>(SystemQuery.government, "Anarchy"),
                new KeyValuePair<string, object>(SystemQuery.security, "Anarchy"),
                new KeyValuePair<string, object>(SystemQuery.allegiance, "Empire")
            };

            string data = JsonConvert.SerializeObject(EddbService.GetData(Endpoint.systems, queryList));
            List<StarSystem> response = JsonConvert.DeserializeObject<List<StarSystem>>(data);

            Assert.IsNotNull(response);
            Assert.IsTrue((response[0]).government.ToLowerInvariant() == "anarchy");
            Assert.IsTrue((response[0]).security.ToLowerInvariant() == "anarchy");
            Assert.IsTrue((response[0]).allegiance.ToLowerInvariant() == "empire");
        }

        /*
        [TestMethod]
        public void TestStarSystemCoordinates()
        {
            // Test coordinates
            StarSystem starSystem = EddbService.GetEddbSystem("Merope");

            Assert.AreEqual("merope", starSystem.name.ToLowerInvariant());
            Assert.AreEqual(-78.59375M, starSystem.x);
            Assert.AreEqual(-149.625M, starSystem.y);
            Assert.AreEqual(-340.53125M, starSystem.z);
        }
        */

        /*
        [TestMethod]
        public void TestStarSystemData()
        {

            // Test system & body data
            StarSystem starSystem = StarMapService.GetStarMapSystem("Sol");

            Assert.AreEqual("Sol", starSystem.name);
            Assert.AreEqual((decimal)0, starSystem.x);
            Assert.AreEqual((decimal)0, starSystem.y);
            Assert.AreEqual((decimal)0, starSystem.z);
            Assert.IsNotNull(starSystem.population);
            Assert.IsNotNull(starSystem.allegiance);
            Assert.IsNotNull(starSystem.government);
            Assert.IsNotNull(starSystem.faction);
            Assert.IsNotNull(starSystem.state);
            Assert.IsNotNull(starSystem.security);
            Assert.IsNotNull(starSystem.primaryeconomy);
            Assert.AreEqual("Common", starSystem.reserve);
            Assert.IsNotNull(starSystem.stations.Count);

            Assert.IsNotNull(starSystem);
            Assert.IsNotNull(starSystem.bodies);
            Assert.IsFalse(starSystem.bodies.Count == 0);

        }
        */

        [TestMethod]
        public void TestBodyDataSol()
        {
            Body body = EddbService.Body("Sol");
            Assert.AreEqual(1, body.EDDBID);
            Assert.AreEqual(4792, body.age);
            Assert.IsNull(body.atmosphere);
            Assert.AreEqual(0, body.distance);
            Assert.IsNull(body.earthmass);
            Assert.IsNull(body.eccentricity);
            Assert.IsNull(body.gravity);
            Assert.IsNull(body.inclination);
            Assert.IsNotNull(body.landable);
            Assert.IsFalse((bool)body.landable);
            Assert.IsNotNull(body.mainstar);
            Assert.IsTrue((bool)body.mainstar);
            Assert.IsNull(body.materials);
            Assert.AreEqual("Sol", body.name);
            Assert.IsNull(body.orbitalperiod);
            Assert.IsNull(body.planettype);
            Assert.IsNull(body.pressure);
            Assert.IsNull(body.radius);
            Assert.AreEqual(2.58327455873843, (double)body.rotationalperiod, .001);
            Assert.IsNull(body.semimajoraxis);
            Assert.AreEqual(1.0, (double)body.solarmass, 0.001);
            Assert.AreEqual(1.0, (double)body.solarradius, 0.001);
            Assert.AreEqual("G", body.stellarclass);
            Assert.AreEqual(5778, body.temperature);
            Assert.IsNull(body.terraformstate);
            Assert.AreEqual(-0.219615, (double)body.tilt, .001);
            Assert.AreEqual("Star", body.type);
            Assert.IsNull(body.volcanism);
        }

        [TestMethod]
        public void TestBodyDataAlphaCentauriB()
        {
            Body body = EddbService.Body("Alpha Centauri B");
            Assert.AreEqual(16309, body.EDDBID);
            Assert.AreEqual("Alpha Centauri B", body.name);
            Assert.AreEqual(764, body.systemEDDBID);
            Assert.AreEqual("Star", body.type);
            Assert.AreEqual("K", body.stellarclass);
            Assert.AreEqual(5398, (double)body.distance, 10);
            Assert.AreEqual(5240, body.temperature);
            Assert.IsNotNull(body.mainstar);
            Assert.IsFalse((bool)body.mainstar);
            Assert.AreEqual(9212, body.age);
            Assert.AreEqual(0.855469, (double)body.solarmass, 0.001);
            Assert.AreEqual(0.90351818312491, (double)body.solarradius, 0.001);
            Assert.IsNull(body.volcanism);
            Assert.IsNull(body.atmosphere);
            Assert.IsNull(body.terraformstate);
            Assert.IsNull(body.earthmass);
            Assert.IsNull(body.radius);
            Assert.IsNull(body.gravity);
            Assert.IsNull(body.pressure);
            Assert.AreEqual(9348.9066666667, (double)body.orbitalperiod, .001);
            Assert.AreEqual(6.3673077331842, (double)body.semimajoraxis, .001);
            Assert.AreEqual(0.5179, (double)body.eccentricity, .001);
            Assert.AreEqual(79.205002, (double)body.inclination, .001);
            Assert.AreEqual(296.656494, (double)body.periapsis, .001);
            Assert.AreEqual(4.0409704137731, (double)body.rotationalperiod, .001);
            Assert.AreEqual(0.203023, (double)body.tilt, .001);
            Assert.IsNull(body.tidallylocked);
            Assert.IsNotNull(body.landable);
            Assert.IsFalse((bool)body.landable);
            Assert.IsNull(body.materials);
            Assert.IsNull(body.planettype);
        }

        [TestMethod]
        public void TestBodyDataMercury()
        {
            Body body = EddbService.Body("Mercury");
            Assert.IsNull(body.age);
            Assert.AreEqual("No atmosphere", body.atmosphereclass.invariantName);
            Assert.IsNotNull(body.distance);
            Assert.AreEqual(0.055M, body.earthmass);
            Assert.AreEqual(0.2056M, body.eccentricity);
            Assert.AreEqual(0.37588835103399, (double)body.gravity, .001);
            Assert.AreEqual(7, body.inclination);
            Assert.IsNotNull(body.landable);
            Assert.IsTrue((bool)body.landable);
            Assert.IsNull(body.mainstar);
            Assert.IsNotNull(body.materials);
            Assert.AreEqual(11, body.materials.Count);
            Assert.AreEqual("Iron", body.materials[0].material);
            Assert.AreEqual(23.5, (double)body.materials[0].percentage, 0.1);
            Assert.AreEqual("Mercury", body.materials[10].material);
            Assert.AreEqual(1.0, (double)body.materials[10].percentage, 0.1);
            Assert.AreEqual("Mercury", body.name);
            Assert.AreEqual(88.0, (double)body.orbitalperiod, 0.1);
            Assert.AreEqual("Metal-rich body", body.planettype);
            Assert.AreEqual(0, body.pressure);
            Assert.IsNotNull(body.radius);
            Assert.AreEqual(2439.7M, body.radius);
            Assert.AreEqual(58.6, (double)body.rotationalperiod, 0.1);
            Assert.AreEqual((double)0.38709837299843, (double)body.semimajoraxis, 0.01);
            Assert.IsNull(body.solarmass);
            Assert.IsNull(body.solarradius);
            Assert.IsNull(body.stellarclass);
            Assert.AreEqual(401.965271M, body.temperature);
            Assert.AreEqual("Not terraformable", body.terraformstate);
            Assert.IsNotNull(body.tidallylocked);
            Assert.IsFalse((bool)body.tidallylocked);
            Assert.AreEqual((double)0.036826M, (double)body.tilt, .001);
            Assert.AreEqual("Planet", body.type);
            Assert.IsNull(body.volcanism);
            Assert.AreEqual("Metal", body.solidComposition[0].invariantName);
            Assert.AreEqual(60M, body.solidComposition[0].percent);
            Assert.AreEqual("Rock", body.solidComposition[1].invariantName);
            Assert.AreEqual(40M, body.solidComposition[1].percent);
            Assert.AreEqual(2, body.solidComposition.Count);
        }

        [TestMethod]
        public void TestBodyDataEarth()
        {
            Body body = EddbService.Body("Earth");
            Assert.IsNull(body.age);
            // Assert.AreEqual("Earth-like", body.atmosphereclass.invariantName); 
            // Data source reports this as "Suitable for water-based life" but via Ross it is "Earthlike". 
            // Apparently intentional from EDSM (EDDB syncs body data from EDSM): https://github.com/EDSM-NET/Alias/blob/3a904f799a4b7b4dd28f12af80eba38307789c99/Body/Planet/Atmosphere.php#L83
            Assert.AreEqual("Nitrogen", body.atmosphereCompositions[0].invariantName);
            Assert.AreEqual(77.886406M, body.atmosphereCompositions[0].percent);
            Assert.AreEqual("Oxygen", body.atmosphereCompositions[1].invariantName);
            Assert.AreEqual((double)0.401426M, (double)body.tilt, .01);
            Assert.AreEqual("Rock", body.solidComposition[0].invariantName);
            Assert.AreEqual(70M, body.solidComposition[0].percent);
            Assert.AreEqual("Metal", body.solidComposition[1].invariantName);
            Assert.AreEqual(30M, body.solidComposition[1].percent);
            Assert.AreEqual(2, body.solidComposition.Count);
            Assert.AreEqual((double)499.485718M, (double)body.distance, 1);
            Assert.AreEqual(0.0167M, body.eccentricity);
            Assert.IsNotNull(body.landable);
            Assert.IsFalse((bool)body.landable);
            Assert.AreEqual(1.0M, body.earthmass);
            Assert.AreEqual(0.0M, body.inclination);
            Assert.AreEqual((double)365.256M, (double)body.orbitalperiod, 0.1);
            Assert.AreEqual(114.207832M, body.periapsis);
            Assert.AreEqual("Earth-like world", body.planettype);
            Assert.AreEqual(6378.1M, body.radius);
            Assert.AreEqual((double)1.0M, (double)body.rotationalperiod, 0.01);
            Assert.AreEqual((double)1.0M, (double)body.semimajoraxis, 0.01);
            Assert.AreEqual((double)1.0M, (double)body.gravity, .01);
            Assert.AreEqual((double)1.0M, (double)body.pressure, .01);
            Assert.AreEqual(288.0M, body.temperature);
            Assert.AreEqual("Not terraformable", body.terraformstate);
            Assert.IsNotNull(body.tidallylocked);
            Assert.IsFalse((bool)body.tidallylocked);
            Assert.IsNotNull(body.volcanism);
            Assert.AreEqual("Silicate", body.volcanism.invariantComposition);
            Assert.AreEqual("Magma", body.volcanism.invariantType);
            Assert.IsNull(body.volcanism.invariantAmount);
            Assert.IsNull(body.materials);
            Assert.AreEqual("Earth", body.name);
            Assert.IsNull(body.mainstar);
            Assert.IsNull(body.solarmass);
            Assert.IsNull(body.solarradius);
            Assert.IsNull(body.stellarclass);
            Assert.AreEqual("Planet", body.type);
        }

        [TestMethod]
        public void TestBodyDataRingedGasGiant()
        {
            Body body = EddbService.Body("HD 43193 CD 7 a");
            Assert.AreEqual("Planet", body.type);
            Assert.AreEqual("Sudarsky class IV gas giant", body.planettype);
            Assert.IsNotNull(body.distance);
            Assert.IsFalse((bool)body.landable);
            Assert.AreEqual(11.175634423255477, (double)body.gravity, 0.01);
            Assert.AreEqual(1574.405762, (double)body.earthmass, 0.01);
            Assert.AreEqual(75701.896, (double)body.radius, 0.01);
            Assert.AreEqual((double)1021.42M, (double)body.temperature, 0.01);
            Assert.AreEqual(Volcanism.FromName("No volcanism"), body.volcanism);
            Assert.AreEqual("No atmosphere", body.atmosphere);
            Assert.AreEqual("Not terraformable", body.terraformstate);
            Assert.AreEqual(55.31602430555556, (double)body.orbitalperiod, 0.01);
            Assert.AreEqual((double)0.11103302421536, (double)body.semimajoraxis, 0.01);
            Assert.IsFalse(body.eccentricity > 0);
            Assert.AreEqual(28.739708, (double)body.inclination, 0.01);
            Assert.AreEqual(248.851364, (double)body.periapsis, 0.01);
            Assert.AreEqual(57.64743634259259, (double)body.rotationalperiod, 0.01);
            Assert.IsTrue((bool)body.tidallylocked);
            Assert.AreEqual(2, body.rings.Count);
            Assert.AreEqual("Rocky", body.rings[0].invariantComposition);
            Assert.AreEqual(124910M, body.rings[0].innerradius);
            Assert.AreEqual(355470000000M, body.rings[0].mass);
            Assert.AreEqual("HD 43193 CD 7 a A Ring", body.rings[0].name);
            Assert.AreEqual(169800M, body.rings[0].outerradius);
        }

        [TestMethod]
        public void TestBodiesSol()
        {
            List<Body> bodies = EddbService.Bodies("Sol");
            Assert.AreEqual(38, bodies.Count); // TODO: Handle belts (we handle them during scans so it would be inconsistent to not handle them from EDDB)
        }

        /*
        [TestMethod]
        public void TestEddbSystems()
        {
            // Test systems obtained from a list 
            string[] systems = { "Merope", "Sol" };
            List<StarSystem> starSystems = EddbService.GetEddbSystems(systems);

            Assert.IsNotNull(starSystems.Count);

            foreach (StarSystem starSystem in starSystems)
            {
                if (starSystem.name.ToLowerInvariant() == "merope")
                {
                    Assert.AreEqual((decimal)-78.59375, starSystem.x);
                    Assert.AreEqual((decimal)-149.625, starSystem.y);
                    Assert.AreEqual((decimal)-340.53125, starSystem.z);
                    continue;
                }
                else if (starSystem.name == "Sol")
                {
                    Body body = starSystem.bodies.Find(b => b.name == "Earth");
                    Assert.IsNotNull("Earth", body.name);
                    continue;
                }
                else
                {
                    Assert.Fail();
                }
            }
        }
        */

        /*
        [TestMethod]
        public void TestEddbStation()
        {
        // Test system station data
        Station station = EddbService.GetEddbStation("Jameson Memorial");
        Assert.AreEqual("shinrarta dezhra", station.systemname.ToLowerInvariant());
        Assert.IsNotNull(station.name);
        Assert.IsNotNull(station.model);
        Assert.IsNotNull(station.distancefromstar);
        Assert.IsNotNull(station.allegiance);
        Assert.IsNotNull(station.government);
        Assert.IsNotNull(station.primaryeconomy);
        Assert.IsNotNull(station.faction);
        Assert.IsInstanceOfType(station.stationServices, typeof(List<string>));
        }
        */

        [TestMethod]
        public void TestEddbFactionAchali()
        {
            // Test faction data
            Faction faction = EddbService.Faction("Coalition of Achali");
            Assert.AreEqual("Federation", faction.Allegiance.invariantName);
            Assert.AreEqual("Confederacy", faction.Government.invariantName);
            Assert.IsNotNull(faction.state);
            Assert.IsFalse(faction.isplayer);
            Assert.IsNotNull(faction.updatedat);
            Assert.AreEqual(6626, faction.EDDBID);
            Assert.AreEqual(424, faction.homeSystemEddbId);
        }

        [TestMethod]
        public void TestEddbFactionsAchali()
        {
            // Test factions data
            string[] factionList = { "Coalition of Achali", "Party of Achali" };
            List<Faction> factions = EddbService.Factions(factionList);
            Assert.AreEqual(2, factions.Count);

            List<Faction> factionsHome = EddbService.Factions("Achali");
            Assert.AreEqual(5, factionsHome.Count);
        }

        /*
        [TestMethod]
        public void TestUnknown()
        {
        // Test that even unknown systems return a basic response
        StarSystem starSystem = EddbService.GetEddbSystem("Unknown star system");
        Assert.AreEqual("unknown star system", starSystem.name.ToLowerInvariant());
        Assert.IsNull(starSystem.population);
        }
        */

        /*
        [TestMethod]
        public void TestUgrasin()
        {
        // Test randomly
        StarSystem starSystem = StarMapService.GetStarMapSystem("Ugrasin");

        Assert.AreEqual("Ugrasin", starSystem.name);
        Assert.AreEqual(1, starSystem.stations.Count);
        Assert.AreEqual(starSystem.z, (decimal)11.28125);
        }
                */
    }
}