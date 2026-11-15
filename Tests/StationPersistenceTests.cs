using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Immutable;
using System.Linq;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class StationPersistenceTests
    {
        [TestMethod]
        public void ServicesAndDockingSurviveJsonRoundTrip ()
        {
            var station = new Station
            {
                name = "Macdonald Settlement",
                marketId = 3227876096,
                Model = StationModel.Coriolis,
                stationServices = [ StationService.Dock, StationService.Refuel, StationService.Commodities ]
            };

            var reloaded = JsonConvert.DeserializeObject<Station>( JsonConvert.SerializeObject( station ) );

            Assert.AreEqual( station.name, reloaded.name );
            Assert.AreEqual( station.marketId, reloaded.marketId );
            Assert.AreEqual( station.Model.edname, reloaded.Model.edname );
            CollectionAssert.AreEquivalent( station.stationServices.Select( s => s.edname ).ToArray(),
                reloaded.stationServices.Select( s => s.edname ).ToArray() );
            Assert.IsTrue( reloaded.hasdocking );
            Assert.IsTrue( reloaded.hasrefuel );
            Assert.IsTrue( reloaded.hasmarket );
        }

        [TestMethod]
        public void ServiceSettersRetainServicesOnNewStation ()
        {
            var station = new Station
            {
                hasdocking = true,
                hasrefuel = true,
                hasrearm = true,
                hasrepair = true,
                hasoutfitting = true,
                hasshipyard = true,
                hasmarket = true,
                hasblackmarket = true
            };

            Assert.HasCount( 8, station.stationServices );
            Assert.IsTrue( station.hasdocking );
            Assert.IsTrue( station.hasrefuel );
            Assert.IsTrue( station.hasrearm );
            Assert.IsTrue( station.hasrepair );
            Assert.IsTrue( station.hasoutfitting );
            Assert.IsTrue( station.hasshipyard );
            Assert.IsTrue( station.hasmarket );
            Assert.IsTrue( station.hasblackmarket );
        }

        [TestMethod]
        [DataRow( 0, 0, 0 )]
        [DataRow( 2, 0, 0 )]
        [DataRow( 2, 3, 0 )]
        [DataRow( 4, 4, 8 )]
        public void LandingPadsSurviveJsonRoundTrip ( int small, int medium, int large )
        {
            var station = new Station
            {
                Model = StationModel.Coriolis,
                landingPads = new StationLandingPads( small, medium, large )
            };

            var json = JsonConvert.SerializeObject( station );
            Assert.IsNotNull( JObject.Parse( json )[ "landingPads" ] );
            var reloaded = JsonConvert.DeserializeObject<Station>( json );

            Assert.AreEqual( small, reloaded.landingPads.Small );
            Assert.AreEqual( medium, reloaded.landingPads.Medium );
            Assert.AreEqual( large, reloaded.landingPads.Large );
            Assert.AreEqual( station.landingPads.LargestPad(), reloaded.landingPads.LargestPad() );
        }

        [TestMethod]
        public void FleetCarrierPadsSurviveJsonRoundTrip ()
        {
            var station = new Station { Model = StationModel.FleetCarrier };
            var reloaded = JsonConvert.DeserializeObject<Station>( JsonConvert.SerializeObject( station ) );

            Assert.AreEqual( 4, reloaded.landingPads.Small );
            Assert.AreEqual( 4, reloaded.landingPads.Medium );
            Assert.AreEqual( 8, reloaded.landingPads.Large );
            Assert.IsTrue( reloaded.IsCarrier() );
        }

        [TestMethod]
        public void OlderCarrierJsonWithoutPadsRestoresFixedPadConfiguration ()
        {
            var json = JObject.FromObject( new Station { Model = StationModel.FleetCarrier } );
            json.Remove( "landingPads" );

            var reloaded = JsonConvert.DeserializeObject<Station>( json.ToString() );

            Assert.IsTrue( reloaded.IsCarrier() );
            Assert.AreEqual( 4, reloaded.landingPads.Small );
            Assert.AreEqual( 4, reloaded.landingPads.Medium );
            Assert.AreEqual( 8, reloaded.landingPads.Large );
        }

        [TestMethod]
        [DataRow( "Coriolis", false, false, false, true )]
        [DataRow( "Outpost", false, false, true, false )]
        [DataRow( "FleetCarrier", false, true, false, false )]
        [DataRow( "Megaship", false, false, false, false )]
        [DataRow( "MegaShipCivilian", false, false, false, false )]
        [DataRow( "SurfaceStation", true, false, false, false )]
        [DataRow( "CraterOutpost", true, false, false, false )]
        [DataRow( "CraterPort", true, false, false, false )]
        [DataRow( "OnFootSettlement", true, false, false, false )]
        [DataRow( "None", false, false, false, false )]
        public void StationClassificationSurvivesJsonRoundTrip (
            string model, bool planetary, bool carrier, bool outpost, bool starport )
        {
            var station = new Station { Model = StationModel.FromEDName( model ) };
            var reloaded = JsonConvert.DeserializeObject<Station>( JsonConvert.SerializeObject( station ) );

            Assert.AreEqual( planetary, reloaded.IsPlanetary() );
            Assert.AreEqual( carrier, reloaded.IsCarrier() );
            Assert.AreEqual( outpost, reloaded.IsOutpost() );
            Assert.AreEqual( starport, reloaded.IsStarport() );
            Assert.AreEqual( model is "Megaship" or "MegaShipCivilian", reloaded.IsMegaShip() );
            Assert.AreEqual( model == "SurfaceStation", reloaded.IsPlanetarySettlement() );
        }

        [TestMethod]
        public void DockableSurfaceStationIsNotAnUndockableSettlementAfterReload ()
        {
            var station = new Station
            {
                Model = StationModel.SurfaceStation,
                stationServices = [ StationService.Dock ]
            };

            var reloaded = JsonConvert.DeserializeObject<Station>( JsonConvert.SerializeObject( station ) );

            Assert.IsTrue( reloaded.IsPlanetary() );
            Assert.IsFalse( reloaded.IsPlanetarySettlement() );
        }

        [TestMethod]
        [DataRow( "{}" )]
        [DataRow( "{\"stationServices\":null}" )]
        [DataRow( "{\"stationServices\":[]}" )]
        public void OlderJsonWithoutPadsAllowsAddingServices ( string json )
        {
            var reloaded = JsonConvert.DeserializeObject<Station>( json );

            Assert.IsEmpty( reloaded.stationServices );
            Assert.IsFalse( reloaded.hasdocking );
            Assert.IsNotNull( reloaded.landingPads );
            Assert.AreEqual( 0, reloaded.landingPads.Small );
            Assert.AreEqual( 0, reloaded.landingPads.Medium );
            Assert.AreEqual( 0, reloaded.landingPads.Large );

            reloaded.stationServices.Add( StationService.Dock );
            Assert.IsTrue( reloaded.hasdocking );
        }

        [TestMethod]
        public void SystemStationCountsSurviveJsonRoundTrip ()
        {
            var system = new StarSystem
            {
                systemname = "Review system",
                systemAddress = 1,
                stations = ImmutableList.Create(
                    new Station { Model = StationModel.Coriolis, stationServices = [ StationService.Dock ] },
                    new Station { Model = StationModel.CraterPort, stationServices = [ StationService.Dock ] },
                    new Station { Model = StationModel.OnFootSettlement, stationServices = [ StationService.Dock ] },
                    new Station { Model = StationModel.SurfaceStation },
                    new Station { Model = StationModel.Coriolis },
                    new Station { Model = StationModel.FleetCarrier, stationServices = [ StationService.Dock ] },
                    new Station { Model = StationModel.Megaship, stationServices = [ StationService.Dock ] },
                    new Station { Model = StationModel.MegaShipCivilian, stationServices = [ StationService.Dock ] } )
            };

            Assert.HasCount( 1, system.orbitalstations );
            Assert.HasCount( 2, system.planetarystations );

            var reloaded = JsonConvert.DeserializeObject<StarSystem>( JsonConvert.SerializeObject( system ) );

            Assert.HasCount( 8, reloaded.stations );
            Assert.HasCount( 1, reloaded.orbitalstations );
            Assert.HasCount( 2, reloaded.planetarystations );
            Assert.HasCount( 1, reloaded.stations.Where( s => s.IsCarrier() ).ToList() );
            Assert.AreEqual( StationModel.Coriolis.edname, reloaded.orbitalstations.Single().Model.edname );
        }
    }
}
