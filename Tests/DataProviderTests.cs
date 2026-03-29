using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class DataProviderTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }
        
        [TestMethod]
        public async Task TestSqlRepositoryPresent()
        {
            EDDI.Instance.DataProvider = DataProviderService.Create();
            var starSystemRepository = EDDI.Instance.DataProvider.starSystemRepository;
            await starSystemRepository.SaveStarSystemAsync( DeserializeJsonResource<StarSystem>( Resources.sqlStarSystem6 ), CancellationToken.None ).ConfigureAwait(false);
            var dbData = await starSystemRepository.GetSqlStarSystemAsync( 10477373803U, CancellationToken.None ).ConfigureAwait(false);
            Assert.IsNotNull(dbData);
            Assert.AreEqual("Sol", dbData.systemName);
        }

        [TestMethod]
        public async Task TestSqlRepositoryMissing()
        {
            EDDI.Instance.DataProvider = DataProviderService.Create();
            var starSystemRepository = EDDI.Instance.DataProvider.starSystemRepository;
            var DBData = await starSystemRepository.GetSqlStarSystemAsync( 0, CancellationToken.None ).ConfigureAwait(false);
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
            var data = @"{""allegiance"":""Federation"",
                ""faction"":""Mother Gaia"",
                ""government"":""Democracy"",
                ""id"":17072,""is_populated"":true,
                ""name"":""Sol"",""systemAddress"":10477373803, ""population"":22780871769,    
                ""power"":""Zachary Hudson"",""power_state"":""Control"",""primary_economy"":""Refinery"",""reserve_type"":""Common"",""security"":""High"",""state"":""Boom"",""updated_at"":1487552337,""x"":0,""y"":0,""z"":0,""bodies"":[{""arg_of_periapsis"":55.19,""atmosphere_composition"":[{""atmosphere_component_id"":3,""atmosphere_component_name"":""Carbon dioxide"",""share"":96.5},{""atmosphere_component_id"":9,""atmosphere_component_name"":""Nitrogen"",""share"":3.5}],""atmosphere_type_id"":6,""atmosphere_type_name"":""Carbon dioxide"",""axis_tilt"":177.3,""created_at"":1466612896,""distance_to_arrival"":361,""earth_masses"":0.815,""gravity"":0.91,""group_id"":6,""group_name"":""Planet"",""id"":4,""is_landable"":0,""is_rotational_period_tidally_locked"":true,""materials"":[{""material_id"":22,""material_name"":""Ruthenium"",""share"":null}],""name"":""Venus"",""orbital_eccentricity"":0.0067,""orbital_inclination"":3.39,""orbital_period"":224.7,""radius"":6052,""rotational_period"":243,""semi_major_axis"":0.72,""solid_composition"":[{""share"":70,""solid_component_id"":3,""solid_component_name"":""Rock""},{""share"":30,""solid_component_id"":2,""solid_component_name"":""Metal""}],""surface_pressure"":93.19,""surface_temperature"":735,""system_id"":17072,""terraforming_state_id"":1,""terraforming_state_name"":""Not terraformable"",""type_id"":30,""type_name"":""High metal content world"",""updated_at"":1477503587,
                ""volcanism"":{""type"":""Geysers"",""composition"":""Iron"",""amount"":""Major""}}]}";

            var system = JsonConvert.DeserializeObject<StarSystem>(data);
            Assert.IsNotNull(system);
            var body = system.bodies[0];
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
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);

            Assert.IsNotNull(system);
            Assert.AreEqual("Macay", system.systemname);
            Assert.AreEqual(8898081, system.population);
            Assert.HasCount( 2, system.stations);
            Assert.HasCount( 0, system.bodies );
        }

        [TestMethod]
        public void TestLegacySystem2()
        {
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem2);

            Assert.IsNotNull(system);
            Assert.AreEqual("Lazdongand", system.systemname);
            Assert.AreEqual(75005, system.population);
            Assert.HasCount( 3, system.stations );
            Assert.HasCount( 0, system.bodies );
        }

        [TestMethod]
        public void TestLegacySystem3()
        {
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem3);

            Assert.IsNotNull(system);
            Assert.AreEqual("Aphros", system.systemname);
            Assert.AreEqual(0, system.population);
            Assert.HasCount( 0, system.stations );
            Assert.HasCount( 8, system.bodies );
        }

        [TestMethod]
        public void TestLegacySystem4()
        {
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem4);

            Assert.AreEqual("Zhu Baba", system.systemname);
            Assert.AreEqual(159918, system.population);
            Assert.HasCount( 0, system.stations );
            Assert.HasCount( 30, system.bodies );
        }

        [TestMethod]
        public void TestLegacyData()
        {
            // Test legacy data from api.eddp.co
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem1);
            Assert.AreEqual("Nijland Terminal", system.stations[0].name);
            Assert.AreEqual("Pinzon Hub", system.stations[1].name);
        }

        [TestMethod]
        public void TestPreservedProperties()
        {
            EDDI.Instance.DataProvider = CreateTestDataProvider();

            // Set up our original star systems
            var system = DeserializeJsonResource<StarSystem>(Resources.sqlStarSystem5);
            var systemsToUpdate = new List<StarSystem>
            {
                system
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
            Assert.IsTrue( body1?.mappedEfficiently ?? false);

            var body2 = result.bodies?.FirstOrDefault( b => b.bodyname == "HR 6421 2" );
            Assert.AreEqual( "2017-12-11T06:17:06Z", Dates.FromDateTimeToString( body2?.scannedDateTime ) );
            Assert.AreEqual( string.Empty, Dates.FromDateTimeToString( body2?.mappedDateTime ) );
            Assert.IsFalse( body2?.mappedEfficiently ?? false );
        }

        [TestMethod]
        public void TestPreservedPropertiesCarryOverFactionReputationAndScannedBodies()
        {
            var visitDate = new DateTime( 2024, 02, 03, 04, 05, 06, DateTimeKind.Utc );
            var systemName = "Preservation Test";
            const ulong systemAddress = 42UL;

            var originalSystem = new StarSystem
            {
                systemname = systemName,
                systemAddress = systemAddress,
                x = 1,
                y = 2,
                z = 3,
                totalbodies = 7,
                factions =
                [
                    new Faction { name = "Faction A", myreputation = 77.5m },
                    new Faction { name = "Faction B", myreputation = 12.5m }
                ]
            };
            originalSystem.visitLog.Add( visitDate );
            originalSystem.AddOrUpdateBodies(
            [
                new Body
                {
                    bodyId = 1,
                    bodyType = BodyType.Planet,
                    bodyname = $"{systemName} 1",
                    systemname = systemName,
                    systemAddress = systemAddress,
                    scannedDateTime = visitDate,
                    mappedDateTime = visitDate,
                    mappedEfficiently = true
                },
                new Body
                {
                    bodyId = 2,
                    bodyType = BodyType.Planet,
                    bodyname = $"{systemName} 2",
                    systemname = systemName,
                    systemAddress = systemAddress,
                    scannedDateTime = visitDate
                },
                new Body
                {
                    bodyId = 3,
                    bodyType = BodyType.Planet,
                    bodyname = $"{systemName} 3",
                    systemname = systemName,
                    systemAddress = systemAddress
                }
            ] );

            var updatedSystem = new StarSystem
            {
                systemname = systemName,
                systemAddress = systemAddress,
                x = 1,
                y = 2,
                z = 3,
                totalbodies = 1,
                factions =
                [
                    new Faction { name = "Faction A" },
                    new Faction { name = "Faction B", myreputation = 0 },
                    new Faction { name = "Faction C", myreputation = 55m }
                ]
            };
            updatedSystem.AddOrUpdateBodies(
            [
                new Body
                {
                    bodyId = 1,
                    bodyType = BodyType.Planet,
                    bodyname = $"{systemName} 1",
                    systemname = systemName,
                    systemAddress = systemAddress
                }
            ] );

            var result = DataProviderService.PreserveUnsyncedProperties( [ updatedSystem ], [ originalSystem ] ).Single();

            Assert.AreEqual( 7, result.totalbodies );
            Assert.AreEqual( 1, result.visits );
            Assert.AreEqual( visitDate, result.lastvisit );
            Assert.AreEqual( 77.5m, result.factions.First( f => f.name == "Faction A" ).myreputation );
            Assert.AreEqual( 12.5m, result.factions.First( f => f.name == "Faction B" ).myreputation );
            Assert.AreEqual( 55m, result.factions.First( f => f.name == "Faction C" ).myreputation );

            var updatedBody = result.bodies.First( b => b.bodyname == $"{systemName} 1" );
            Assert.AreEqual( visitDate, updatedBody.scannedDateTime );
            Assert.AreEqual( visitDate, updatedBody.mappedDateTime );
            Assert.IsTrue( updatedBody.mappedEfficiently );
            Assert.IsNotNull( result.bodies.FirstOrDefault( b => b.bodyname == $"{systemName} 2" ) );
            Assert.IsNull( result.bodies.FirstOrDefault( b => b.bodyname == $"{systemName} 3" ) );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestGetOrFetchStarSystemAsyncPreservesUnsyncedPropertiesFromStaleDatabaseRecordAsync()
        {
            var dataProvider = CreateTestDataProvider();
            EDDI.Instance.DataProvider = dataProvider;

            var staleSystem = CloneStarSystem( DeserializeJsonResource<StarSystem>( Resources.sqlStarSystem5 ) );
            var uniqueSystemAddress = BitConverter.ToUInt64( Guid.NewGuid().ToByteArray(), 0 );
            var uniqueSystemName = $"HR 6421 refresh {Guid.NewGuid():N}";
            RenameStarSystem( staleSystem, uniqueSystemName, uniqueSystemAddress );
            staleSystem.lastupdated = DateTime.UtcNow.AddMonths( -2 );

            await dataProvider.starSystemRepository.SaveStarSystemAsync( staleSystem, CancellationToken.None ).ConfigureAwait( false );

            var fetchedBody = staleSystem.bodies.First( b => b.bodyname == $"{uniqueSystemName} 1" );
            FakeSpanshHttpClient.Expect( $"dump/{uniqueSystemAddress}", CreateSpanshDumpResponse( staleSystem, fetchedBody ) );

            var result = await dataProvider.GetOrFetchStarSystemAsync( uniqueSystemAddress,
                fetchIfMissing: true,
                excludeStaleResults: true,
                showMarketDetails: false,
                fetchEdsmVisitsAndComments: false ).ConfigureAwait( false );

            Assert.IsNotNull( result );
            Assert.AreEqual( staleSystem.totalbodies, result.totalbodies );
            Assert.AreEqual( staleSystem.visits, result.visits );
            Assert.AreEqual( staleSystem.lastvisit, result.lastvisit );

            var body1 = result.bodies.FirstOrDefault( b => b.bodyname == $"{uniqueSystemName} 1" );
            Assert.AreEqual( fetchedBody.scannedDateTime, body1?.scannedDateTime );
            Assert.AreEqual( fetchedBody.mappedDateTime, body1?.mappedDateTime );
            Assert.IsTrue( body1?.mappedEfficiently ?? false );

            var body2 = result.bodies.FirstOrDefault( b => b.bodyname == $"{uniqueSystemName} 2" );
            Assert.IsNotNull( body2 );
            Assert.AreEqual( staleSystem.bodies.First( b => b.bodyname == $"{uniqueSystemName} 2" ).scannedDateTime, body2.scannedDateTime );
        }

        [TestMethod, DoNotParallelize]
        public async Task TestGetOrFetchStarSystemByNameAsyncRefreshesStaleDatabaseRecordAsync()
        {
            var dataProvider = CreateTestDataProvider();
            EDDI.Instance.DataProvider = dataProvider;

            var staleSystem = CloneStarSystem( DeserializeJsonResource<StarSystem>( Resources.sqlStarSystem5 ) );
            var uniqueSystemAddress = BitConverter.ToUInt64( Guid.NewGuid().ToByteArray(), 0 );
            var uniqueSystemName = $"HR 6421 name refresh {Guid.NewGuid():N}";
            RenameStarSystem( staleSystem, uniqueSystemName, uniqueSystemAddress );
            staleSystem.lastupdated = DateTime.UtcNow.AddMonths( -2 );

            await dataProvider.starSystemRepository.SaveStarSystemAsync( staleSystem, CancellationToken.None ).ConfigureAwait( false );

            var fetchedBody = staleSystem.bodies.First( b => b.bodyname == $"{uniqueSystemName} 1" );
            FakeSpanshHttpClient.Expect( $"systems/field_values/system_names?q={uniqueSystemName}", CreateSpanshSystemNameQueryResponse( staleSystem ) );
            FakeSpanshHttpClient.Expect( $"dump/{uniqueSystemAddress}", CreateSpanshDumpResponse( staleSystem, fetchedBody ) );

            var result = await dataProvider.GetOrFetchStarSystemAsync( uniqueSystemName,
                fetchIfMissing: true,
                excludeStaleResults: true,
                showMarketDetails: false,
                fetchEdsmVisitsAndComments: false ).ConfigureAwait( false );

            Assert.IsNotNull( result );
            Assert.AreEqual( staleSystem.systemAddress, result.systemAddress );
            Assert.AreEqual( staleSystem.totalbodies, result.totalbodies );
            Assert.AreEqual( staleSystem.visits, result.visits );
            Assert.AreEqual( staleSystem.lastvisit, result.lastvisit );

            var body2 = result.bodies.FirstOrDefault( b => b.bodyname == $"{uniqueSystemName} 2" );
            Assert.IsNotNull( body2 );
            Assert.AreEqual( staleSystem.bodies.First( b => b.bodyname == $"{uniqueSystemName} 2" ).scannedDateTime, body2.scannedDateTime );
        }
        private static StarSystem CloneStarSystem ( StarSystem starSystem )
        {
            return JsonConvert.DeserializeObject<StarSystem>( JsonConvert.SerializeObject( starSystem ) );
        }

        private static void RenameStarSystem ( StarSystem starSystem, string systemName, ulong systemAddress )
        {
            var originalSystemName = starSystem.systemname;
            starSystem.systemname = systemName;
            starSystem.systemAddress = systemAddress;

            foreach ( var body in starSystem.bodies )
            {
                body.systemname = systemName;
                body.systemAddress = systemAddress;
                if ( body.bodyname == originalSystemName )
                {
                    body.bodyname = systemName;
                }
                else if ( body.bodyname.StartsWith( originalSystemName + " ", StringComparison.Ordinal ) )
                {
                    body.bodyname = systemName + body.bodyname[ originalSystemName.Length.. ];
                }
            }
        }

        private static string CreateSpanshDumpResponse ( StarSystem starSystem, params Body[] fetchedBodies )
        {
            return JsonConvert.SerializeObject( new
            {
                system = new
                {
                    name = starSystem.systemname,
                    id64 = starSystem.systemAddress,
                    coords = new
                    {
                        starSystem.x,
                        starSystem.y,
                        starSystem.z
                    },
                    date = DateTime.UtcNow,
                    bodyCount = 1,
                    bodies = fetchedBodies.Select( body => new
                    {
                        name = body.bodyname,
                        id64 = body.systemAddress ?? starSystem.systemAddress,
                        body.bodyId,
                        type = "Planet",
                        subType = "Metal-rich body",
                        distanceToArrival = body.distance ?? 46m,
                        surfaceTemperature = body.temperature ?? 1385m,
                        parents = body.parents.Count > 0 ? body.parents : [ new Dictionary<string, int> { [ "Star" ] = 0 } ],
                        earthMasses = body.earthmass ?? 1.278431m,
                        gravity = body.gravity ?? 1.91929188703249m,
                        isLandable = body.landable ?? true,
                        radius = body.radius ?? 5205.3825m,
                        rotationalPeriodTidallyLocked = body.tidallylocked ?? true,
                        materials = new Dictionary<string, decimal>
                        {
                            [ "Iron" ] = 36.9m,
                            [ "Nickel" ] = 27.91m
                        },
                        solidComposition = new Dictionary<string, decimal>
                        {
                            [ "Metal" ] = 100m,
                            [ "Ice" ] = 0m,
                            [ "Rock" ] = 0m
                        },
                        terraformingState = "Not terraformable",
                        updateTime = DateTime.UtcNow
                    } )
                }
            } );
        }
        private static string CreateSpanshSystemNameQueryResponse ( StarSystem starSystem )
        {
            return JsonConvert.SerializeObject( new
            {
                values = new[] { starSystem.systemname },
                min_max = new[]
                {
                    new
                    {
                        name = starSystem.systemname,
                        id64 = starSystem.systemAddress,
                        starSystem.x,
                        starSystem.y,
                        starSystem.z
                    }
                }
            } );
        }
    }
}
