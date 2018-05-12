using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using EddiDataProviderService;
using Rollbar;
using Newtonsoft.Json;

namespace UnitTests
{
    [TestClass]
    public class DataProviderTests
    {
        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        public void TestDataProviderEmptySystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Lagoon Sector GW-V b2-6", null, null, null);
            Assert.IsNotNull(starSystem.stations);
        }

        [TestMethod]
        public void TestDataProviderMalformedSystem()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Malformed with quote\" and backslash\\. So evil", null, null, null);
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestDataProviderBodies()
        {
            // Force obtain the data from remote source
            StarSystem starSystem = DataProviderService.GetSystemData("Sol", null, null, null);
            Assert.IsNotNull(starSystem);

            Assert.IsNotNull(starSystem.bodies);
            Assert.IsFalse(starSystem.bodies.Count == 0);

            Body sol = starSystem.bodies.Find(b => b.type == "Star");
            Assert.AreEqual(4792, sol.age);
            Assert.IsNull(sol.atmosphere);
            Assert.AreEqual(0, sol.distance);
            Assert.IsNull(sol.earthmass);
            Assert.IsNull(sol.eccentricity);
            Assert.IsNull(sol.gravity);
            Assert.IsNull(sol.inclination);
            Assert.IsNotNull(sol.landable);
            Assert.IsFalse((bool)sol.landable);
            Assert.IsNotNull(sol.mainstar);
            Assert.IsTrue((bool)sol.mainstar);
            Assert.IsNull(sol.materials);
            Assert.AreEqual("Sol", sol.name);
            Assert.IsNull(sol.orbitalperiod);
            Assert.IsNull(sol.planettype);
            Assert.IsNull(sol.pressure);
            Assert.IsNull(sol.radius);
            Assert.IsNull(sol.rotationalperiod);
            Assert.IsNull(sol.semimajoraxis);
            Assert.AreEqual(1.0, (double)sol.solarmass, 0.001);
            Assert.AreEqual(1.0, (double)sol.solarradius, 0.001);
            Assert.AreEqual("G", sol.stellarclass);
            Assert.AreEqual("Sol", sol.systemname);
            Assert.AreEqual(5778, sol.temperature);
            Assert.IsNull(sol.terraformstate);
            Assert.IsNotNull(sol.tidallylocked);
            Assert.IsFalse((bool)sol.tidallylocked);
            Assert.IsNull(sol.tilt);
            Assert.AreEqual("Star", sol.type);
            Assert.IsNull(sol.volcanism);

            Body mercury = starSystem.bodies.Find(n => n.name.Equals("Mercury"));
            Assert.IsNull(mercury.age);
            Assert.AreEqual("No atmosphere", mercury.atmosphere);
            Assert.IsNotNull(mercury.distance);
            Assert.AreEqual(0.055M, mercury.earthmass);
            Assert.AreEqual(0.2056M, mercury.eccentricity);
            Assert.AreEqual(0.38M, mercury.gravity);
            Assert.AreEqual(7, mercury.inclination);
            Assert.IsNotNull(mercury.landable);
            Assert.IsTrue((bool)mercury.landable);
            Assert.IsNull(mercury.mainstar);
            Assert.IsNotNull(mercury.materials);
            Assert.AreEqual(11, mercury.materials.Count);
            Assert.AreEqual("Iron", mercury.materials[0].material);
            Assert.AreEqual(23.5, (double)mercury.materials[0].percentage, 0.1);
            Assert.AreEqual("Mercury", mercury.materials[10].material);
            Assert.AreEqual(1.0, (double)mercury.materials[10].percentage, 0.1);
            Assert.AreEqual("Mercury", mercury.name);
            Assert.AreEqual(88.0, (double)mercury.orbitalperiod, 0.1);
            Assert.AreEqual("Metal-rich body", mercury.planettype);
            Assert.IsNotNull(mercury.pressure);
            Assert.IsNotNull(mercury.radius);
            Assert.AreEqual(2440, mercury.radius);
            Assert.AreEqual(58.6, (double)mercury.rotationalperiod, 0.1);
            Assert.AreEqual(0.39, (double)mercury.semimajoraxis, 0.01);
            Assert.IsNull(mercury.solarmass);
            Assert.IsNull(mercury.solarradius);
            Assert.IsNull(mercury.stellarclass);
            Assert.AreEqual("Sol", mercury.systemname);
            Assert.AreEqual(402, mercury.temperature);
            Assert.AreEqual("Not terraformable", mercury.terraformstate);
            Assert.IsNotNull(mercury.tidallylocked);
            Assert.IsFalse((bool)mercury.tidallylocked);
            Assert.AreEqual(2.11M, mercury.tilt);
            Assert.AreEqual("Planet", mercury.type);
            Assert.IsNull(mercury.volcanism);
        }

        [TestMethod]
        public void TestDataProviderUnknown()
        {
            StarSystem starSystem = DataProviderService.GetSystemData("Not appearing in this galaxy", null, null, null);
            Assert.IsNotNull(starSystem);
        }

        [TestMethod]
        public void TestLegacySystem1()
        {
            /// Test legacy data that may be stored in user's local sql databases. 
            /// Legacy data includes all data stored in user's sql databases prior to version 3.0.1-b2
            /// Note that data structures were reorganized at this time to support internationalization.

            string legagySystemSql = @"{
	            ""visits"": 1,
	            ""lastvisit"": ""2016-10-30T08:12:25"",
	            ""comment"": null,
	            ""distancefromhome"": null,
	            ""updatedat"": null,
	            ""lastupdated"": ""2016-10-30T01:12:25.8247964-07:00"",
	            ""EDDBID"": 13018,
	            ""name"": ""Macay"",
	            ""population"": 8898081,
	            ""allegiance"": ""Empire"",
	            ""government"": ""Patronage"",
	            ""faction"": ""HIP22506EmpireLeague"",
	            ""primaryeconomy"": null,
	            ""state"": null,
	            ""security"": null,
	            ""power"": ""ArissaLavigny-Duval"",
	            ""powerstate"": ""Exploited"",
	            ""x"": 127.0,
	            ""y"": -111.09375,
	            ""z"": -59.3125,
	            ""stations"": [{
		            ""updatedat"": null,
		            ""commoditiesupdatedat"": null,
		            ""outfittingupdatedat"": null,
		            ""EDDBID"": 32548,
		            ""name"": ""NijlandTerminal"",
		            ""government"": ""Patronage"",
		            ""faction"": ""HIP22506EmpireLeague"",
		            ""allegiance"": ""Empire"",
		            ""state"": null,
		            ""primaryeconomy"": null,
		            ""distancefromstar"": 44670,
		            ""systemname"": ""Macay"",
		            ""hasrefuel"": true,
		            ""hasrearm"": true,
		            ""hasrepair"": true,
		            ""hasoutfitting"": true,
		            ""hasshipyard"": true,
		            ""hasmarket"": true,
		            ""hasblackmarket"": false,
		            ""model"": ""UnknownStarport"",
		            ""largestpad"": ""Large"",
		            ""economies"": null,
		            ""commodities"": null,
		            ""prohibited"": null,
		            ""outfitting"": null,
		            ""shipyard"": null
	            },
	            {
		            ""updatedat"": null,
		            ""commoditiesupdatedat"": null,
		            ""outfittingupdatedat"": null,
		            ""EDDBID"": 58341,
		            ""name"": ""PinzonHub"",
		            ""government"": ""Anarchy"",
		            ""faction"": ""SocietyofMacay"",
		            ""allegiance"": ""Independent"",
		            ""state"": null,
		            ""primaryeconomy"": null,
		            ""distancefromstar"": null,
		            ""systemname"": ""Macay"",
		            ""hasrefuel"": true,
		            ""hasrearm"": true,
		            ""hasrepair"": true,
		            ""hasoutfitting"": true,
		            ""hasshipyard"": false,
		            ""hasmarket"": false,
		            ""hasblackmarket"": true,
		            ""model"": ""PlanetaryOutpost"",
		            ""largestpad"": ""Large"",
		            ""economies"": null,
		            ""commodities"": null,
		            ""prohibited"": null,
		            ""outfitting"": null,
		            ""shipyard"": null
	            }],
	            ""bodies"": []
            }";

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
            Assert.IsNotNull(system);
            Assert.AreEqual("Macay", system.name);
            Assert.AreEqual(8898081, system.population);
            Assert.AreEqual(2, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem2()
        {
            string legagySystemSql = @"{
	            ""visits"": 1,
	            ""lastvisit"": ""2016-11-05T16:37:04"",
	            ""comment"": null,
	            ""distancefromhome"": null,
	            ""updatedat"": null,
	            ""lastupdated"": ""2016-11-05T09:37:05.0926446-07:00"",
	            ""EDDBID"": 11622,
	            ""name"": ""Lazdongand"",
	            ""population"": 75005,
	            ""allegiance"": ""Independent"",
	            ""government"": ""Feudal"",
	            ""faction"": ""DukesofLazdongand"",
	            ""primaryeconomy"": null,
	            ""state"": null,
	            ""security"": null,
	            ""power"": ""ZeminaTorval"",
	            ""powerstate"": ""Exploited"",
	            ""x"": 142.75,
	            ""y"": -3.03125,
	            ""z"": 62.28125,
	            ""stations"": [{
		            ""updatedat"": null,
		            ""commoditiesupdatedat"": null,
		            ""outfittingupdatedat"": null,
		            ""EDDBID"": 20095,
		            ""name"": ""BorlaugInstallation"",
		            ""government"": ""Feudal"",
		            ""faction"": ""DukesofLazdongand"",
		            ""allegiance"": ""Independent"",
		            ""state"": null,
		            ""primaryeconomy"": null,
		            ""distancefromstar"": null,
		            ""systemname"": ""Lazdongand"",
		            ""hasrefuel"": true,
		            ""hasrearm"": false,
		            ""hasrepair"": true,
		            ""hasoutfitting"": false,
		            ""hasshipyard"": false,
		            ""hasmarket"": true,
		            ""hasblackmarket"": false,
		            ""model"": ""IndustrialOutpost"",
		            ""largestpad"": ""Medium"",
		            ""economies"": null,
		            ""commodities"": null,
		            ""prohibited"": null,
		            ""outfitting"": null,
		            ""shipyard"": null
	            },
	            {
		            ""updatedat"": null,
		            ""commoditiesupdatedat"": null,
		            ""outfittingupdatedat"": null,
		            ""EDDBID"": 20096,
		            ""name"": ""GarneauDock"",
		            ""government"": ""Corporate"",
		            ""faction"": ""LazdongandFortuneSolutions"",
		            ""allegiance"": ""Federation"",
		            ""state"": null,
		            ""primaryeconomy"": null,
		            ""distancefromstar"": null,
		            ""systemname"": ""Lazdongand"",
		            ""hasrefuel"": true,
		            ""hasrearm"": false,
		            ""hasrepair"": true,
		            ""hasoutfitting"": false,
		            ""hasshipyard"": false,
		            ""hasmarket"": true,
		            ""hasblackmarket"": false,
		            ""model"": ""IndustrialOutpost"",
		            ""largestpad"": ""Medium"",
		            ""economies"": null,
		            ""commodities"": null,
		            ""prohibited"": null,
		            ""outfitting"": null,
		            ""shipyard"": null
	            },
	            {
		            ""updatedat"": null,
		            ""commoditiesupdatedat"": null,
		            ""outfittingupdatedat"": null,
		            ""EDDBID"": 52225,
		            ""name"": ""WeierstrassTerminal"",
		            ""government"": ""Cooperative"",
		            ""faction"": ""Co-operativeofLazdongand"",
		            ""allegiance"": ""Independent"",
		            ""state"": null,
		            ""primaryeconomy"": null,
		            ""distancefromstar"": null,
		            ""systemname"": ""Lazdongand"",
		            ""hasrefuel"": true,
		            ""hasrearm"": true,
		            ""hasrepair"": true,
		            ""hasoutfitting"": true,
		            ""hasshipyard"": false,
		            ""hasmarket"": true,
		            ""hasblackmarket"": false,
		            ""model"": ""PlanetaryOutpost"",
		            ""largestpad"": ""Large"",
		            ""economies"": null,
		            ""commodities"": null,
		            ""prohibited"": null,
		            ""outfitting"": null,
		            ""shipyard"": null
	            }],
	            ""bodies"": []
            }";

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
            Assert.IsNotNull(system);
            Assert.AreEqual("Lazdongand", system.name);
            Assert.AreEqual(75005, system.population);
            Assert.AreEqual(3, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem3()
        {
            string legagySystemSql = @"{
	            ""visits"": 1,
	            ""lastvisit"": ""2016-10-29T16:15:26"",
	            ""comment"": null,
	            ""distancefromhome"": null,
	            ""updatedat"": 1493632555,
	            ""lastupdated"": ""2018-02-16T17:16:14.9454298-08:00"",
	            ""EDDBID"": 19951,
	            ""name"": ""Aphros"",
	            ""population"": 0,
	            ""allegiance"": ""None"",
	            ""government"": ""None"",
	            ""faction"": null,
	            ""primaryeconomy"": ""None"",
	            ""state"": null,
	            ""security"": ""Low"",
	            ""power"": null,
	            ""powerstate"": null,
	            ""x"": 57.53125,
	            ""y"": -57.375,
	            ""z"": -6.8125,
	            ""stations"": [],
	            ""bodies"": [{
		            ""temperature"": 2755,
		            ""rings"": null,
		            ""stellarclass"": ""M"",
		            ""solarmass"": 0.292969,
		            ""solarradius"": 0.466540589334483,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": null,
		            ""atmosphere"": null,
		            ""tilt"": null,
		            ""earthmass"": null,
		            ""gravity"": null,
		            ""eccentricity"": null,
		            ""inclination"": null,
		            ""orbitalperiod"": null,
		            ""radius"": null,
		            ""rotationalperiod"": null,
		            ""semimajoraxis"": null,
		            ""pressure"": null,
		            ""terraformstate"": null,
		            ""planettype"": null,
		            ""volcanism"": null,
		            ""materials"": null,
		            ""reserves"": null,
		            ""EDDBID"": 606595,
		            ""type"": ""Star"",
		            ""name"": ""Aphros"",
		            ""systemname"": ""Aphros"",
		            ""age"": 10018,
		            ""distance"": 0,
		            ""landable"": false,
		            ""tidallylocked"": false,
		            ""mainstar"": true,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 373,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 223.999985,
		            ""atmosphere"": ""Methane"",
		            ""tilt"": null,
		            ""earthmass"": 1.59887,
		            ""gravity"": 0.81036592516303,
		            ""eccentricity"": 0.001361,
		            ""inclination"": 0.000407,
		            ""orbitalperiod"": 640.79101851851851851851851852,
		            ""radius"": 8955,
		            ""rotationalperiod"": 1.41568060981482,
		            ""semimajoraxis"": 0.96620802140869,
		            ""pressure"": 5287.3645003701,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": {
			            ""type"": ""Geysers"",
			            ""composition"": ""Water"",
			            ""amount"": null
		            },
		            ""materials"": null,
		            ""reserves"": null,
		            ""EDDBID"": 770829,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros1"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 483,
		            ""landable"": false,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 64,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 229.580292,
		            ""atmosphere"": ""Helium"",
		            ""tilt"": null,
		            ""earthmass"": 1.341259,
		            ""gravity"": 0.75410155353765,
		            ""eccentricity"": 0.001311,
		            ""inclination"": 0.04167,
		            ""orbitalperiod"": 1025.7916666666666666666666667,
		            ""radius"": 8502,
		            ""rotationalperiod"": 1.62514811197917,
		            ""semimajoraxis"": 1.32220597655873,
		            ""pressure"": 0.300070279733531,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": {
			            ""type"": ""Geysers"",
			            ""composition"": ""CarbonDioxide"",
			            ""amount"": ""Minor""
		            },
		            ""materials"": null,
		            ""reserves"": null,
		            ""EDDBID"": 770987,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros2"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 661,
		            ""landable"": false,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 35,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 189.654709,
		            ""atmosphere"": ""Neon-rich"",
		            ""tilt"": null,
		            ""earthmass"": 1.104167,
		            ""gravity"": 0.69707912487955,
		            ""eccentricity"": 0.005228,
		            ""inclination"": 0.067916,
		            ""orbitalperiod"": 6186.2255555555555555555555556,
		            ""radius"": 8024,
		            ""rotationalperiod"": 1.52289333767361,
		            ""semimajoraxis"": 4.38070018193113,
		            ""pressure"": 0.231085673417222,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": {
			            ""type"": ""Geysers"",
			            ""composition"": ""CarbonDioxide"",
			            ""amount"": ""Minor""
		            },
		            ""materials"": null,
		            ""reserves"": null,
		            ""EDDBID"": 771029,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros3"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 2176,
		            ""landable"": false,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 30,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 208.75676,
		            ""atmosphere"": ""Helium"",
		            ""tilt"": null,
		            ""earthmass"": 2.176746,
		            ""gravity"": 0.92392060489566,
		            ""eccentricity"": 0.000007,
		            ""inclination"": -4.148468,
		            ""orbitalperiod"": 9724.891851851851851851851852,
		            ""radius"": 9785,
		            ""rotationalperiod"": 1.50950077763889,
		            ""semimajoraxis"": 5.92265191036573,
		            ""pressure"": 0.440987193131014,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": {
			            ""type"": ""Geysers"",
			            ""composition"": ""Water"",
			            ""amount"": ""Major""
		            },
		            ""materials"": null,
		            ""reserves"": null,
		            ""EDDBID"": 771055,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros4"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 2955,
		            ""landable"": false,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 30,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 37.450123,
		            ""atmosphere"": ""Noatmosphere"",
		            ""tilt"": null,
		            ""earthmass"": 0.04081,
		            ""gravity"": 0.18171434689726,
		            ""eccentricity"": 0.0,
		            ""inclination"": 35.778599,
		            ""orbitalperiod"": 418.97356481481481481481481481,
		            ""radius"": 3021,
		            ""rotationalperiod"": 2.07397822627315,
		            ""semimajoraxis"": 0.0206167092724536,
		            ""pressure"": 0.0,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": null,
		            ""materials"": [{
			            ""material"": ""Sulphur"",
			            ""percentage"": 27.21479
		            },
		            {
			            ""material"": ""Carbon"",
			            ""percentage"": 22.884819
		            },
		            {
			            ""material"": ""Phosphorus"",
			            ""percentage"": 14.651256
		            },
		            {
			            ""material"": ""Iron"",
			            ""percentage"": 11.81673
		            },
		            {
			            ""material"": ""Nickel"",
			            ""percentage"": 8.937679
		            },
		            {
			            ""material"": ""Manganese"",
			            ""percentage"": 4.880188
		            },
		            {
			            ""material"": ""Selenium"",
			            ""percentage"": 4.259345
		            },
		            {
			            ""material"": ""Vanadium"",
			            ""percentage"": 2.901777
		            },
		            {
			            ""material"": ""Cadmium"",
			            ""percentage"": 0.917623
		            },
		            {
			            ""material"": ""Tellurium"",
			            ""percentage"": 0.804531
		            },
		            {
			            ""material"": ""Tin"",
			            ""percentage"": 0.731276
		            }],
		            ""reserves"": null,
		            ""EDDBID"": 771065,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros4a"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 2955,
		            ""landable"": true,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 26,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 264.107178,
		            ""atmosphere"": ""Noatmosphere"",
		            ""tilt"": null,
		            ""earthmass"": 0.144922,
		            ""gravity"": 0.28565922103878,
		            ""eccentricity"": 0.0,
		            ""inclination"": 9.887839,
		            ""orbitalperiod"": 757.47328703703703703703703704,
		            ""radius"": 4541,
		            ""rotationalperiod"": 0.507881130648148,
		            ""semimajoraxis"": 0.0327611216995751,
		            ""pressure"": 0.0000121370046878855,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": null,
		            ""materials"": [{
			            ""material"": ""Sulphur"",
			            ""percentage"": 28.26786
		            },
		            {
			            ""material"": ""Carbon"",
			            ""percentage"": 23.77034
		            },
		            {
			            ""material"": ""Phosphorus"",
			            ""percentage"": 15.218181
		            },
		            {
			            ""material"": ""Iron"",
			            ""percentage"": 12.273974
		            },
		            {
			            ""material"": ""Nickel"",
			            ""percentage"": 9.283521
		            },
		            {
			            ""material"": ""Selenium"",
			            ""percentage"": 4.424159
		            },
		            {
			            ""material"": ""Vanadium"",
			            ""percentage"": 3.014061
		            },
		            {
			            ""material"": ""Zirconium"",
			            ""percentage"": 1.425261
		            },
		            {
			            ""material"": ""Niobium"",
			            ""percentage"": 0.838861
		            },
		            {
			            ""material"": ""Molybdenum"",
			            ""percentage"": 0.801483
		            },
		            {
			            ""material"": ""Antimony"",
			            ""percentage"": 0.682315
		            }],
		            ""reserves"": null,
		            ""EDDBID"": 771131,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros5a"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 3838,
		            ""landable"": true,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            },
	            {
		            ""temperature"": 27,
		            ""rings"": null,
		            ""stellarclass"": null,
		            ""solarmass"": null,
		            ""solarradius"": null,
		            ""chromaticity"": null,
		            ""radiusprobability"": null,
		            ""massprobability"": null,
		            ""tempprobability"": null,
		            ""ageprobability"": null,
		            ""periapsis"": 64.443527,
		            ""atmosphere"": ""Helium"",
		            ""tilt"": null,
		            ""earthmass"": 2.577351,
		            ""gravity"": 0.99759030861711,
		            ""eccentricity"": 0.002399,
		            ""inclination"": -4.200991,
		            ""orbitalperiod"": 14387.933333333333333333333333,
		            ""radius"": 10247,
		            ""rotationalperiod"": 1.60919885706019,
		            ""semimajoraxis"": 7.68998190892031,
		            ""pressure"": 0.508690167776955,
		            ""terraformstate"": ""Notterraformable"",
		            ""planettype"": ""Icybody"",
		            ""volcanism"": {
			            ""type"": ""Geysers"",
			            ""composition"": ""Water"",
			            ""amount"": ""Major""
		            },
		            ""materials"": null,
		            ""reserves"": null,
		            ""EDDBID"": 771126,
		            ""type"": ""Planet"",
		            ""name"": ""Aphros5"",
		            ""systemname"": ""Aphros"",
		            ""age"": null,
		            ""distance"": 3841,
		            ""landable"": false,
		            ""tidallylocked"": false,
		            ""mainstar"": null,
		            ""luminosityclass"": null
	            }]
            }";

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(legagySystemSql);
            Assert.IsNotNull(system);
            Assert.AreEqual("Aphros", system.name);
            Assert.AreEqual(0, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(8, system.bodies.Count);

        }
    }
}
