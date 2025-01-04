using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Tests.Properties;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class DataProviderTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }
        
        [TestMethod]
        public void TestSqlRepositoryPresent()
        {
            EDDI.Instance.DataProvider = new DataProviderService();
            var starSystemRepository = EDDI.Instance.DataProvider.starSystemRepository;
            starSystemRepository.SaveStarSystem( DeserializeJsonResource<StarSystem>( Resources.sqlStarSystem6 ) );
            var dbData = starSystemRepository.GetSqlStarSystem( 10477373803U );
            Assert.IsNotNull(dbData);
            Assert.AreEqual("Sol", dbData.systemName);
        }

        [TestMethod]
        public void TestSqlRepositoryMissing()
        {
            EDDI.Instance.DataProvider = new DataProviderService();
            var starSystemRepository = EDDI.Instance.DataProvider.starSystemRepository;
            var DBData = starSystemRepository.GetSqlStarSystem(0);
            Assert.IsNull(DBData);
        }

        [TestMethod]
        public void TestVolcanismConversion()
        {
            // Fetch a star system with various types of volcanism
            var sol = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem6);

            // Ariel has no volcanism
            var ariel = sol.bodies.Find(b => b.bodyname == "Ariel");
            Assert.IsNotNull(ariel);
            Assert.IsNull(ariel.volcanism);

            // Europa has water magma
            var europa = sol.bodies.Find(b => b.bodyname == "Europa");
            Assert.IsNotNull(europa);
            Assert.IsNotNull(europa.volcanism);
            Assert.AreEqual("Major", europa.volcanism.invariantAmount);
            Assert.AreEqual("Water", europa.volcanism.invariantComposition);
            Assert.AreEqual("Magma", europa.volcanism.invariantType);
        }

        [TestMethod]
        public void TestVolcanismObject()
        {
            // Hand-crafted body
            string data = @"{""allegiance"":""Federation"",
                ""faction"":""Mother Gaia"",
                ""government"":""Democracy"",
                ""id"":17072,""is_populated"":true,
                ""name"":""Sol"",""systemAddress"":10477373803, ""population"":22780871769,    
                ""power"":""Zachary Hudson"",""power_state"":""Control"",""primary_economy"":""Refinery"",""reserve_type"":""Common"",""security"":""High"",""state"":""Boom"",""updated_at"":1487552337,""x"":0,""y"":0,""z"":0,""bodies"":[{""arg_of_periapsis"":55.19,""atmosphere_composition"":[{""atmosphere_component_id"":3,""atmosphere_component_name"":""Carbon dioxide"",""share"":96.5},{""atmosphere_component_id"":9,""atmosphere_component_name"":""Nitrogen"",""share"":3.5}],""atmosphere_type_id"":6,""atmosphere_type_name"":""Carbon dioxide"",""axis_tilt"":177.3,""created_at"":1466612896,""distance_to_arrival"":361,""earth_masses"":0.815,""gravity"":0.91,""group_id"":6,""group_name"":""Planet"",""id"":4,""is_landable"":0,""is_rotational_period_tidally_locked"":true,""materials"":[{""material_id"":22,""material_name"":""Ruthenium"",""share"":null}],""name"":""Venus"",""orbital_eccentricity"":0.0067,""orbital_inclination"":3.39,""orbital_period"":224.7,""radius"":6052,""rotational_period"":243,""semi_major_axis"":0.72,""solid_composition"":[{""share"":70,""solid_component_id"":3,""solid_component_name"":""Rock""},{""share"":30,""solid_component_id"":2,""solid_component_name"":""Metal""}],""surface_pressure"":93.19,""surface_temperature"":735,""system_id"":17072,""terraforming_state_id"":1,""terraforming_state_name"":""Not terraformable"",""type_id"":30,""type_name"":""High metal content world"",""updated_at"":1477503587,
                ""volcanism"":{""type"":""Geysers"",""composition"":""Iron"",""amount"":""Major""}}]}";

            StarSystem system = JsonConvert.DeserializeObject<StarSystem>(data);
            Assert.IsNotNull(system);
            Body body = system.bodies[0];
            Assert.IsNotNull(body);
            Assert.IsNotNull(body.volcanism);
            Assert.AreEqual("Major", body.volcanism.invariantAmount);
            Assert.AreEqual("Iron", body.volcanism.invariantComposition);
            Assert.AreEqual("Geysers", body.volcanism.invariantType);
        }

        [TestMethod]
        public void TestLegacySystem1()
        {
            // Test legacy data that may be stored in user's local sql databases.
            // Legacy data includes all data stored in user's sql databases prior to version 3.0.1-b2
            // Note that data structures were reorganized at this time to support internationalization.
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);

            Assert.IsNotNull(system);
            Assert.AreEqual("Macay", system.systemname);
            Assert.AreEqual(8898081, system.population);
            Assert.AreEqual(2, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem2()
        {
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem2);

            Assert.IsNotNull(system);
            Assert.AreEqual("Lazdongand", system.systemname);
            Assert.AreEqual(75005, system.population);
            Assert.AreEqual(3, system.stations.Count);
            Assert.AreEqual(0, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem3()
        {
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem3);

            Assert.IsNotNull(system);
            Assert.AreEqual("Aphros", system.systemname);
            Assert.AreEqual(0, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(8, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacySystem4()
        {
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem4);

            Assert.AreEqual("Zhu Baba", system.systemname);
            Assert.AreEqual(159918, system.population);
            Assert.AreEqual(0, system.stations.Count);
            Assert.AreEqual(30, system.bodies.Count);
        }

        [TestMethod]
        public void TestLegacyData()
        {
            // Test legacy data from api.eddp.co
            StarSystem system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);
            Assert.AreEqual("Nijland Terminal", system.stations[0].name);
            Assert.AreEqual("Pinzon Hub", system.stations[1].name);
        }

        [TestMethod]
        public void TestPreservedProperties()
        {
            EDDI.Instance.DataProvider = ConfigureTestDataProvider();

            // Set up our original star systems
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem5);
            var systemsToUpdate = new List<DatabaseStarSystem>
            {
                new DatabaseStarSystem(system.systemname, system.systemAddress, JsonConvert.SerializeObject(system))
            };

            // Set up a copy where we mimic missing data not recovered from the server
            var systemCopy = new StarSystem()
            {
                systemname = system.systemname,
                systemAddress = system.systemAddress,
                x = system.x,
                y = system.y,
                z = system.z
            };
            var updatedSystems = new List<StarSystem>() { systemCopy };

            // Invoke the method under test
            var results = DataProviderService.PreserveUnsyncedProperties( updatedSystems, systemsToUpdate );
            var result = results[0];

            // Evaluate the results. The result must include the preserved data.
            Assert.AreEqual(3, result.scannedbodies);
            Assert.AreEqual(1, result.mappedbodies);
            Assert.AreEqual(20, result.totalbodies);
            Assert.AreEqual(17, result.visits);
            Assert.AreEqual("2017-12-11T06:17:06Z", Dates.FromDateTimeToString(result.lastvisit));

            var body1 = result.bodies?.FirstOrDefault( b => b.bodyname == "HR 6421 1" );
            Assert.AreEqual( "2017-12-11T06:17:06Z", Dates.FromDateTimeToString( body1?.scannedDateTime ) );
            Assert.AreEqual( "2017-12-11T06:17:06Z", Dates.FromDateTimeToString( body1?.mappedDateTime ) );
            Assert.AreEqual( true, body1?.mappedEfficiently ?? false);

            var body2 = result.bodies?.FirstOrDefault( b => b.bodyname == "HR 6421 2" );
            Assert.AreEqual( "2017-12-11T06:17:06Z", Dates.FromDateTimeToString( body2?.scannedDateTime ) );
            Assert.AreEqual( string.Empty, Dates.FromDateTimeToString( body2?.mappedDateTime ) );
            Assert.AreEqual( false, body2?.mappedEfficiently ?? false );
        }
    }
}
