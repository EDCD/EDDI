using EddiCompanionAppService;
using EddiDataDefinitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tests.Properties;
using Utilities;

namespace Tests
{
    [ TestClass, TestCategory( "UnitTests" ) ]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class CapiTests : TestBase
    {
        [TestMethod ]
        public void TestOutfittingModules ()
        {
            // Test outfitting data
            var incompleteExpectedModules = new List<OutfittingInfoItem>()
            {
                new("Hpt_ATDumbfireMissile_Fixed_Large", "weapon", 1352250),
                new("Hpt_ATMultiCannon_Fixed_Large", "weapon", 1181500),
                new("Hpt_ATMultiCannon_Turret_Large", "weapon", 3821600),
                new("Hpt_FlakMortar_Fixed_Medium", "weapon", 261800),
                new("Hpt_ATDumbfireMissile_Fixed_Medium", "weapon", 540900)
            };

            var json = DeserializeJsonResource<JObject>( Resources.capi_shipyard_Abasheli_Barracks )
                ?.ToObject<JObject>();
            Assert.IsNotNull( json );
            json[ "timestamp" ] =
                DateTime.UtcNow; // We add a timestamp to the json returned from the Frontier API, do the same here.
            var station = FrontierApiStation.FromJson( null, json );
            var actualModules = station.outfitting;

            Assert.HasCount( 165, actualModules );
            foreach ( var expectedModule in incompleteExpectedModules )
            {
                foreach ( var actualModule in actualModules )
                {
                    if ( expectedModule.edName == actualModule.edName )
                    {
                        Assert.IsTrue( expectedModule.DeepEquals( actualModule ) );
                    }
                }
            }
        }

        [ TestMethod ]
        public void TestShipyardShips ()
        {
            // Test shipyard data
            var expectedShips = new List<ShipyardInfoItem>()
            {
                new("Eagle", 44800),
                new("Asp_Scout", 3961154),
                new("SideWinder", 32000),
                new("Vulture", 4925615),
                new("Anaconda", 146969451),
                new("Federation_Dropship", 14314205),
                new("Federation_Gunship", 35814205),
                new("Federation_Dropship_MkII", 19814205)
            };

            var json = DeserializeJsonResource<JObject>( Resources.capi_shipyard_Abasheli_Barracks )
                ?.ToObject<JObject>();
            Assert.IsNotNull( json );
            json[ "timestamp" ] =
                DateTime.UtcNow; // We add a timestamp to the json returned from the Frontier API, do the same here.
            var station = FrontierApiStation.FromJson( null, json );
            var actualShips = station.ships;

            Assert.HasCount( expectedShips.Count, actualShips );
            foreach ( var expectedShip in expectedShips )
            {
                foreach ( var actualShip in actualShips )
                {
                    if ( expectedShip.edModel == actualShip.edModel )
                    {
                        Assert.IsTrue( expectedShip.DeepEquals( actualShip ) );
                    }
                }
            }
        }

        [ TestMethod ]
        public void TestProfileStation ()
        {
            var marketJson = DeserializeJsonResource<JObject>( Resources.capi_market_Libby_Horizons )
                ?.ToObject<JObject>();
            Assert.IsNotNull( marketJson );
            var expectedStation = new FrontierApiStation()
            {
                name = "Libby Horizons",
                marketId = 3228854528,
                economyShares =
                [
                    new FrontierApiEconomyShare( "Refinery", 0.88M ),
                    new FrontierApiEconomyShare( "Industrial", 0.12M )
                ],
                eddnCommodityMarketQuotes =
                [
                    new MarketInfoItem( 128924334, "AgronomicTreatment", "Chemicals", 0, 3336, 3155,
                        CommodityBracket.None, CommodityBracket.Medium, 0, 43, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128049204, "Explosives", "Chemicals", 224, 203, 419, CommodityBracket.High,
                        CommodityBracket.None, 52135, 1, false, [ "Producer" ] ),
                    new MarketInfoItem( 128049202, "HydrogenFuel", "Chemicals", 84, 80, 108, CommodityBracket.High,
                        CommodityBracket.None, 90728, 1, false, [ "Producer" ] ),
                    new MarketInfoItem( 128673850, "HydrogenPeroxide", "Chemicals", 0, 1198, 1209,
                        CommodityBracket.None, CommodityBracket.High, 0, 116055, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128673851, "LiquidOxygen", "Chemicals", 0, 467, 434, CommodityBracket.None,
                        CommodityBracket.Medium, 0, 18513, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128049203, "MineralOil", "Chemicals", 0, 687, 395, CommodityBracket.None,
                        CommodityBracket.Medium, 0, 249414, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128672305, "SurfaceStabilisers", "Chemicals", 416, 390, 663,
                        CommodityBracket.High, CommodityBracket.None, 44411, 1, false,
                        [ "Producer" ] ),
                    new MarketInfoItem( 128961249, "Tritium", "Chemicals", 41179, 40693, 42558, CommodityBracket.Medium,
                        CommodityBracket.None, 10464, 1, false, [ "Producer" ] ),
                    new MarketInfoItem( 128049166, "Water", "Chemicals", 0, 457, 267, CommodityBracket.None,
                        CommodityBracket.Medium, 0, 36527, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128049241, "Clothing", "Consumer Items", 0, 902, 459, CommodityBracket.None,
                        CommodityBracket.Medium, 0, 38689, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128049240, "ConsumerTechnology", "Consumer Items", 0, 7658, 6809,
                        CommodityBracket.None, CommodityBracket.High, 0, 5598, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128049238, "DomesticAppliances", "Consumer Items", 0, 1117, 659,
                        CommodityBracket.None, CommodityBracket.Medium, 0, 14440, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128682048, "SurvivalEquipment", "Consumer Items", 500, 467, 647,
                        CommodityBracket.High, CommodityBracket.None, 116, 1, false,
                        [ "Producer" ] ),
                    new MarketInfoItem( 128049177, "Algae", "Foods", 0, 464, 321, CommodityBracket.None,
                        CommodityBracket.Medium, 0, 7417, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128049182, "Animalmeat", "Foods", 0, 2181, 1524, CommodityBracket.None,
                        CommodityBracket.High, 0, 14121, false, [ "Consumer", "powerplay" ] ),
                    new MarketInfoItem( 128049189, "Coffee", "Foods", 0, 2181, 1504, CommodityBracket.None,
                        CommodityBracket.High, 0, 5194, false, [ "Consumer", "powerplay" ] ),
                    new MarketInfoItem( 128049183, "Fish", "Foods", 0, 1083, 640, CommodityBracket.None,
                        CommodityBracket.High, 0, 45149, false, [ "Consumer", "powerplay" ] ),
                    new MarketInfoItem( 128049184, "FoodCartridges", "Foods", 0, 334, 225, CommodityBracket.None,
                        CommodityBracket.Medium, 0, 1887, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128049178, "FruitAndVegetables", "Foods", 0, 970, 528, CommodityBracket.None,
                        CommodityBracket.High, 0, 18707, false, [ "Consumer", "powerplay" ] ),
                    new MarketInfoItem( 128049180, "Grain", "Foods", 0, 836, 432, CommodityBracket.None,
                        CommodityBracket.High, 0, 99831, false, [ "Consumer", "powerplay" ] ),
                    new MarketInfoItem( 128049185, "SyntheticMeat", "Foods", 0, 810, 487, CommodityBracket.None,
                        CommodityBracket.High, 0, 6223, false, [ "Consumer" ] ),
                    new MarketInfoItem( 128049188, "Tea", "Foods", 0, 2392, 1691, CommodityBracket.None,
                        CommodityBracket.High, 0, 13184, false, [ "Consumer", "powerplay" ] ),
                    new MarketInfoItem( 128673856, "CMMComposite", "Industrial Materials", 0, 6779, 5984,
                        CommodityBracket.None, CommodityBracket.High, 0, 2287, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128672302, "CeramicComposites", "Industrial Materials", 0, 712, 393,
                        CommodityBracket.None, CommodityBracket.High, 0, 35686, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128673857, "CoolingHoses", "Industrial Materials", 0, 1896, 1886,
                        CommodityBracket.None, CommodityBracket.High, 0, 7839, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 128673855, "InsulatingMembrane", "Industrial Materials", 0, 11386, 10691,
                        CommodityBracket.None, CommodityBracket.Medium, 0, 1461, false,
                        [ "Consumer" ] ),
                    new MarketInfoItem( 129015433, "AncientRelicTG", "Salvage", 4798, 4797, 4750, CommodityBracket.None,
                        CommodityBracket.None, 0, 0, false, [ ] )
                ],
                prohibitedCommodities =
                [
                    new KeyValuePair<long, string>( 128049670, "CombatStabilisers" ),
                    new KeyValuePair<long, string>( 128049212, "BasicNarcotics" ),
                    new KeyValuePair<long, string>( 128049213, "Tobacco" ),
                    new KeyValuePair<long, string>( 128049234, "BattleWeapons" ),
                    new KeyValuePair<long, string>( 128667728, "ImperialSlaves" ),
                    new KeyValuePair<long, string>( 128049243, "Slaves" )
                ],
                commoditiesupdatedat = marketJson[ "timestamp" ]?.ToObject<DateTime>() ?? DateTime.MinValue,
                marketJson =
                    DeserializeJsonResource<JObject>( Resources.capi_market_Libby_Horizons )?.ToObject<JObject>(),
                stationServices =
                [
                    new KeyValuePair<string, string>( "dock", "ok" ),
                    new KeyValuePair<string, string>( "contacts", "ok" ),
                    new KeyValuePair<string, string>( "exploration", "ok" ),
                    new KeyValuePair<string, string>( "commodities", "ok" ),
                    new KeyValuePair<string, string>( "refuel", "ok" ),
                    new KeyValuePair<string, string>( "repair", "ok" ),
                    new KeyValuePair<string, string>( "rearm", "ok" ),
                    new KeyValuePair<string, string>( "outfitting", "ok" ),
                    new KeyValuePair<string, string>( "shipyard", "ok" ),
                    new KeyValuePair<string, string>( "crewlounge", "ok" ),
                    new KeyValuePair<string, string>( "powerplay", "ok" ),
                    new KeyValuePair<string, string>( "searchrescue", "ok" ),
                    new KeyValuePair<string, string>( "materialtrader", "ok" ),
                    new KeyValuePair<string, string>( "stationmenu", "ok" ),
                    new KeyValuePair<string, string>( "shop", "ok" ),
                    new KeyValuePair<string, string>( "engineer", "ok" )
                ]
            };
            Assert.IsNotNull( expectedStation.marketJson );
            expectedStation.marketJson[ "timestamp" ] =
                marketJson[ "timestamp" ]
                    ?.ToObject<
                        DateTime>(); // We add a timestamp to the json returned from the Frontier API, do the same here.

            var actualStation = FrontierApiStation.FromJson( marketJson, null );

            // Test commodities separately to minimize redundant data entry
            var incompleteExpectedCommodities = expectedStation.eddnCommodityMarketQuotes;
            var actualCommodities = actualStation.eddnCommodityMarketQuotes;
            Assert.HasCount( 117, actualCommodities );
            foreach ( var expectedCommodity in incompleteExpectedCommodities )
            {
                foreach ( var actualCommodity in actualCommodities )
                {
                    if ( expectedCommodity.EliteID == actualCommodity.EliteID )
                    {
                        Assert.IsTrue( expectedCommodity.DeepEquals( actualCommodity ) );
                    }
                }
            }

            // Compare actual and expected stations, less the commodities we already tested above
            expectedStation.eddnCommodityMarketQuotes = null;
            actualStation.eddnCommodityMarketQuotes = null;
            Assert.IsTrue( expectedStation.DeepEquals( actualStation ) );
        }

        [ TestMethod ]
        public void TestProfileUpdateStation ()
        {
            // Set up our original station
            var originalStation = new Station() { name = "Libby Horizons", marketId = 3228854528, updatedat = 0 };

            // Set up our profile station
            var marketTimestamp = DateTime.UtcNow;
            var marketJson = DeserializeJsonResource<JObject>( Resources.capi_market_Libby_Horizons )
                ?.ToObject<JObject>();
            Assert.IsNotNull( marketJson );
            marketJson[ "timestamp" ] =
                marketTimestamp; // We add a timestamp to the json returned from the Frontier API, do the same here.
            var lastStation = FrontierApiStation.FromJson( marketJson, null );

            var updatedStation = lastStation.UpdateStation( marketTimestamp, originalStation );
            Assert.IsTrue( updatedStation.economyShares.DeepEquals( new List<EconomyShare>()
            {
                new("Refinery", 0.88M), new("Industrial", 0.12M)
            } ) );
            Assert.IsTrue( updatedStation.stationServices.DeepEquals( new List<StationService>()
            {
                StationService.FromEDName( "dock" ),
                StationService.FromEDName( "contacts" ),
                StationService.FromEDName( "exploration" ),
                StationService.FromEDName( "commodities" ),
                StationService.FromEDName( "refuel" ),
                StationService.FromEDName( "repair" ),
                StationService.FromEDName( "rearm" ),
                StationService.FromEDName( "outfitting" ),
                StationService.FromEDName( "shipyard" ),
                StationService.FromEDName( "crewlounge" ),
                StationService.FromEDName( "powerplay" ),
                StationService.FromEDName( "searchrescue" ),
                StationService.FromEDName( "materialtrader" ),
                StationService.FromEDName( "stationmenu" ),
                StationService.FromEDName( "shop" ),
                StationService.FromEDName( "engineer" ),
            } ) );
            Assert.HasCount( 117, updatedStation.commodities );
            Assert.IsTrue( new CommodityMarketQuote( CommodityDefinition.FromEDName( "Tritium" ) )
            {
                buyprice = 41179,
                sellprice = 40693,
                demand = 1,
                demandbracket = CommodityBracket.None,
                stock = 10464,
                stockbracket = CommodityBracket.Medium,
                StatusFlags = [ "Producer" ]
            }.DeepEquals( updatedStation.commodities.FirstOrDefault( c => c.EliteID == 128961249 ) ) );
            Assert.AreEqual( 42558, CommodityDefinition.FromEDName( "Tritium" )?.avgprice );
            Assert.HasCount( 6, updatedStation.prohibited );
            Assert.IsTrue( CommodityDefinition.FromEDName( "Tobacco" )
                .DeepEquals( updatedStation.prohibited.FirstOrDefault( p => p.EliteID == 128049213 ) ) );
            Assert.AreEqual( Dates.fromDateTimeToSeconds( marketTimestamp ), updatedStation.commoditiesupdatedat );
        }

        [ TestMethod, DoNotParallelize ]
        public async Task ObtainDataAsync_WithExpiredToken_RefreshesOnlyOnceForConcurrentRequests ()
        {
            var credentials = new CompanionAppCredentials
            {
                accessToken = "old-access",
                refreshToken = "old-refresh",
                tokenExpiry = DateTime.UtcNow.AddMinutes( -5 )
            };

            var tokenRequests = 0;
            var dataRequests = 0;

            var handler = new FakeCompanionHttpMessageHandler( async ( request, body, cancellationToken ) =>
            {
                if ( request.RequestUri?.AbsoluteUri == "https://auth.frontierstore.net/token" )
                {
                    Interlocked.Increment( ref tokenRequests );

                    // Force overlap so the second obtainDataAsync call reaches the refresh lock.
                    await Task.Delay( 100, cancellationToken ).ConfigureAwait( false );

                    Assert.AreEqual( "POST", request.Method.Method );
                    Assert.Contains( body, "grant_type=refresh_token" );
                    Assert.Contains( body, "refresh_token=old-refresh" );

                    return JsonResponse( HttpStatusCode.OK,
                        @"{""access_token"":""new-access"",""refresh_token"":""new-refresh"",""expires_in"":3600}" );
                }

                if ( request.RequestUri?.AbsoluteUri == "https://companion.orerve.net/profile" )
                {
                    Interlocked.Increment( ref dataRequests );
                    Assert.AreEqual( "Bearer new-access", request.Headers.Authorization?.ToString() );

                    return JsonResponse( HttpStatusCode.OK, @"{""commander"":""Test""}" );
                }

                return JsonResponse( HttpStatusCode.NotFound, "{}" );
            } );

            var service = CreateService( credentials, handler );

            var results = await Task.WhenAll(
                service.obtainDataAsync( "https://companion.orerve.net/profile" ),
                service.obtainDataAsync( "https://companion.orerve.net/profile" ) ).ConfigureAwait( false );

            Assert.AreEqual( 1, tokenRequests, "Concurrent expired-token requests should share one refresh." );
            Assert.AreEqual( 2, dataRequests );
            Assert.IsTrue( results.All( r => r != null ) );
            Assert.AreEqual( "new-access", credentials.accessToken );
            Assert.AreEqual( "new-refresh", credentials.refreshToken );
            Assert.AreEqual( CompanionAppService.State.Authorized, service.CurrentState );
        }

        [ TestMethod, DoNotParallelize ]
        public async Task ObtainDataAsync_WhenDataRequestReturnsUnauthorized_RefreshesAndRetriesWithNewAccessToken ()
        {
            var credentials = new CompanionAppCredentials
            {
                accessToken = "stale-access",
                refreshToken = "refresh-token",
                tokenExpiry = DateTime.UtcNow.AddHours( 1 )
            };

            var dataRequestCount = 0;
            var tokenRequestCount = 0;
            var dataAuthHeaders = new List<string>();

            var handler = new FakeCompanionHttpMessageHandler( ( request, body, _ ) =>
            {
                if ( request.RequestUri?.AbsoluteUri == "https://auth.frontierstore.net/token" )
                {
                    tokenRequestCount++;

                    Assert.AreEqual( "POST", request.Method.Method );
                    Assert.Contains( body, "grant_type=refresh_token" );
                    Assert.Contains( body, "refresh_token=refresh-token" );

                    return Task.FromResult( JsonResponse( HttpStatusCode.OK,
                        @"{""access_token"":""refreshed-access"",""refresh_token"":""refreshed-refresh"",""expires_in"":3600}" ) );
                }

                if ( request.RequestUri?.AbsoluteUri == "https://companion.orerve.net/profile" )
                {
                    dataRequestCount++;
                    dataAuthHeaders.Add( request.Headers.Authorization?.ToString() );

                    return Task.FromResult( dataRequestCount == 1
                        ? JsonResponse( HttpStatusCode.Unauthorized, "{}" )
                        : JsonResponse( HttpStatusCode.OK, @"{""ok"":true}" ) );
                }

                return Task.FromResult( JsonResponse( HttpStatusCode.NotFound, "{}" ) );
            } );

            var service = CreateService( credentials, handler );

            var result = await service.obtainDataAsync( "https://companion.orerve.net/profile" )
                .ConfigureAwait( false );

            Assert.IsNotNull( result );
            Assert.AreEqual( 1, tokenRequestCount );
            Assert.AreEqual( 2, dataRequestCount );
            Assert.AreEqual( "Bearer stale-access", dataAuthHeaders[ 0 ] );
            Assert.AreEqual( "Bearer refreshed-access", dataAuthHeaders[ 1 ] );
            Assert.AreEqual( "refreshed-access", credentials.accessToken );
            Assert.AreEqual( "refreshed-refresh", credentials.refreshToken );
            Assert.AreEqual( CompanionAppService.State.Authorized, service.CurrentState );
        }

        [ TestMethod, DoNotParallelize ]
        public async Task ObtainDataAsync_WhenRefreshEndpointUnavailable_SetsConnectionLostAndKeepsCredentials ()
        {
            var credentials = new CompanionAppCredentials
            {
                accessToken = "old-access",
                refreshToken = "old-refresh",
                tokenExpiry = DateTime.UtcNow.AddMinutes( -5 )
            };

            var handler = new FakeCompanionHttpMessageHandler( ( request, _, _ ) =>
            {
                if ( request.RequestUri?.AbsoluteUri == "https://auth.frontierstore.net/token" )
                {
                    return Task.FromResult( JsonResponse( HttpStatusCode.ServiceUnavailable, "{}" ) );
                }

                return Task.FromResult( JsonResponse( HttpStatusCode.OK, @"{""ok"":true}" ) );
            } );

            var service = CreateService( credentials, handler );

            var result = await service.obtainDataAsync( "https://companion.orerve.net/profile" )
                .ConfigureAwait( false );

            Assert.IsNull( result );
            Assert.AreEqual( CompanionAppService.State.ConnectionLost, service.CurrentState );
            Assert.AreEqual( "old-access", credentials.accessToken );
            Assert.AreEqual( "old-refresh", credentials.refreshToken );
        }

        [ TestMethod, DoNotParallelize ]
        public async Task ObtainDataAsync_WhenRefreshTokenUnauthorized_SetsLoggedOut ()
        {
            var credentials = new CompanionAppCredentials
            {
                accessToken = "old-access",
                refreshToken = "invalid-refresh",
                tokenExpiry = DateTime.UtcNow.AddMinutes( -5 )
            };

            var handler = new FakeCompanionHttpMessageHandler( ( request, _, _ ) =>
            {
                if ( request.RequestUri?.AbsoluteUri == "https://auth.frontierstore.net/token" )
                {
                    return Task.FromResult( JsonResponse( HttpStatusCode.Unauthorized, "{}" ) );
                }

                return Task.FromResult( JsonResponse( HttpStatusCode.OK, @"{""ok"":true}" ) );
            } );

            var service = CreateService( credentials, handler );

            var result = await service.obtainDataAsync( "https://companion.orerve.net/profile" )
                .ConfigureAwait( false );

            Assert.IsNull( result );
            Assert.AreEqual( CompanionAppService.State.LoggedOut, service.CurrentState );
        }

        [ TestMethod, DoNotParallelize ]
        public async Task
            ObtainDataAsync_WhenRefreshResponseIsMissingRefreshToken_SetsConnectionLostAndKeepsExistingCredentials ()
        {
            var credentials = new CompanionAppCredentials
            {
                accessToken = "old-access",
                refreshToken = "old-refresh",
                tokenExpiry = DateTime.UtcNow.AddMinutes( -5 )
            };

            var handler = new FakeCompanionHttpMessageHandler( ( request, _, _ ) =>
            {
                if ( request.RequestUri?.AbsoluteUri == "https://auth.frontierstore.net/token" )
                {
                    return Task.FromResult( JsonResponse( HttpStatusCode.OK,
                        @"{""access_token"":""new-access"",""expires_in"":3600}" ) );
                }

                return Task.FromResult( JsonResponse( HttpStatusCode.OK, @"{""ok"":true}" ) );
            } );

            var service = CreateService( credentials, handler );

            var result = await service.obtainDataAsync( "https://companion.orerve.net/profile" )
                .ConfigureAwait( false );

            Assert.IsNull( result );
            Assert.AreEqual( CompanionAppService.State.ConnectionLost, service.CurrentState );
            Assert.AreEqual( "old-access", credentials.accessToken );
            Assert.AreEqual( "old-refresh", credentials.refreshToken );
        }

        [ TestMethod, DoNotParallelize ]
        public async Task ObtainDataAsync_WhenConnectionLostButAccessTokenStillValid_AllowsRetry ()
        {
            var credentials = new CompanionAppCredentials
            {
                accessToken = "valid-access",
                refreshToken = "valid-refresh",
                tokenExpiry = DateTime.UtcNow.AddHours( 1 )
            };

            var handler = new FakeCompanionHttpMessageHandler( ( request, _, _ ) =>
            {
                Assert.AreEqual( "Bearer valid-access", request.Headers.Authorization?.ToString() );
                return Task.FromResult( JsonResponse( HttpStatusCode.OK, @"{""ok"":true}" ) );
            } );

            var service = CreateService( credentials, handler );

            // This requires an internal test-only helper or the production change that restores
            // Authorized when ConnectionLost has a still-valid token.
            service.SetStateForTesting( CompanionAppService.State.ConnectionLost );

            var result = await service.obtainDataAsync( "https://companion.orerve.net/profile" )
                .ConfigureAwait( false );

            Assert.IsNotNull( result );
            Assert.AreEqual( CompanionAppService.State.Authorized, service.CurrentState );
        }

        private static CompanionAppService CreateService (
            CompanionAppCredentials credentials,
            HttpMessageHandler handler )
        {
            return new CompanionAppService(
                new HttpClient( handler ),
                _ => credentials,
                "test-client-id",
                runStartupRefresh: false );
        }

        private static HttpResponseMessage JsonResponse ( HttpStatusCode statusCode, string json )
        {
            var response = new HttpResponseMessage( statusCode )
            {
                Content = new StringContent( json, Encoding.UTF8, "application/json" )
            };

            response.Headers.Date = DateTimeOffset.UtcNow;
            return response;
        }

        private sealed class FakeCompanionHttpMessageHandler (
            Func<HttpRequestMessage, string, CancellationToken, Task<HttpResponseMessage>> responder )
            : HttpMessageHandler
        {
            protected override async Task<HttpResponseMessage> SendAsync (
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var body = request.Content == null
                    ? null
                    : await request.Content.ReadAsStringAsync( cancellationToken ).ConfigureAwait( false );

                return await responder( request, body, cancellationToken ).ConfigureAwait( false );
            }
        }
    }
}
