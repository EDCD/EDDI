using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Uses the Spansh star system dump API (full star system data), e.g. https://www.spansh.co.uk/api/dump/10477373803
        public async Task<StarSystem> GetStarSystemAsync ( ulong systemAddress, bool showMarketDetails, CancellationToken cancellationToken )
        {
            if ( systemAddress == 0 ) { return null; }

            HttpResponseMessage clientResponse = null;
            try
            {
                var requestUri = $"dump/{systemAddress}";
                clientResponse = await spanshHttpClient.GetAsync( requestUri, cancellationToken ).ConfigureAwait( false );
                clientResponse.EnsureSuccessStatusCode();
                var responseJson = await clientResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait( false );

                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Spansh API returned no result" );
                    return null;
                }

                var jResponse = JToken.Parse(responseJson);
                if ( jResponse.Contains( "error" ) )
                {
                    Logging.Debug( $"Spansh API responded with: {jResponse[ "error" ]}" );
                    return null;
                }
                
                return ParseStarSystemDump( jResponse[ "system" ], showMarketDetails );
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled, nothing to do except return.
            }
            catch ( HttpRequestException ) when ( clientResponse?.StatusCode == HttpStatusCode.NotFound )
            {
                Logging.Warn( $"Spansh API has no record corresponding to System Address [ {systemAddress} ]" );
            }
            catch ( Exception ex )
            {
                Logging.Error( "An error occurred while fetching the star system", ex );
            }

            return null;
        }

        public async Task<IList<StarSystem>> GetStarSystemsAsync ( ulong[] systemAddresses, bool showMarketDetails, CancellationToken cancellationToken )
        {
            var starSystems = await Task.WhenAll( systemAddresses.AsParallel().Select( systemAddress =>
                GetStarSystemAsync( systemAddress, showMarketDetails, cancellationToken ) ) ).ConfigureAwait( false );
            return starSystems.RemoveNulls();
                }

        private static StarSystem ParseStarSystemDump ( JToken data, bool showMarketDetails = false )
        {
            try
            {
                var starSystem = new StarSystem
                {
                    systemname = data[ "name" ].ToString(),
                    systemAddress = data[ "id64" ].Value<ulong>(),
                    x = data[ "coords" ]?[ "x" ]?.Value<decimal>(),
                    y = data[ "coords" ]?[ "y" ]?.Value<decimal>(),
                    z = data[ "coords" ]?[ "z" ]?.Value<decimal>(),
                    updatedat = Dates.fromDateTimeToSeconds( JsonParsing.getDateTime("date", data) )
                };

                // Skip parsing star systems lacking essential data - a system name, address, and coordinates
                if ( string.IsNullOrEmpty(starSystem.systemname) || 
                     starSystem.systemAddress == 0 || 
                     starSystem.x is null || 
                     starSystem.y is null || 
                     starSystem.z is null )
                {
                    return null;
                }

                // Populated System Data
                starSystem.population = data[ "population" ]?.Value<long?>();
                if ( starSystem.population > 0 )
                {
                    GetFactionData( starSystem, data );

                    starSystem.Economies = new[]
                    {
                        Economy.FromName(data["primaryEconomy"]?.ToString()) ?? Economy.None,
                        Economy.FromName(data["secondaryEconomy"]?.ToString()) ?? Economy.None
                    }.Where( e => e != Economy.None ).ToList();

                    starSystem.securityLevel = SecurityLevel.FromName( (string)data[ "security" ] ) ??
                                               SecurityLevel.None;

                    starSystem.AddOrUpdateStations( data[ "stations" ]?.AsParallel().Select( stationToken => ParseStation( starSystem, stationToken, null, showMarketDetails ) ).RemoveNulls().ToList() ??
                                                    [ ] );

                    starSystem.Power = Power.FromName( data[ "controllingPower" ]?.ToString() );
                    starSystem.powerState = PowerplayState.FromName( data[ "powerState" ]?.ToString() );
                    starSystem.NearbyPowers = ( data[ "powers" ]?
                        .Select( t => Power.FromName( t.ToString() ) )
                        .Where( p => p != starSystem.Power )
                        .ToHashSet() ?? [ ] ).ToList();
                    starSystem.powerAcquisitionProgress = data[ "powerConflictProgress" ]?
                        .Select( p => new PowerAcquisitionProgress( p[ "power" ]?.ToString(), p[ "progress" ].Value<decimal>() * 100 ) )
                        .OrderByDescending( p => p.progress )
                        .ToList() ?? [ ];
                    starSystem.powerControlProgress = data[ "powerStateControlProgress" ]?.Value<decimal?>() ?? 0;
                    starSystem.powerReinforcementControlPoints = data[ "powerStateReinforcement" ]?.Value<int?>() ?? 0;
                    starSystem.powerUnderminingControlPoints = data[ "powerStateUndermining" ]?.Value<int?>() ?? 0;
                }

                // Get bodies
                starSystem.totalbodies = data[ "bodyCount" ]?.Value<int>() ?? 0;
                starSystem.AddOrUpdateBodies( data[ "bodies" ]?.AsParallel()
                    .Select( b => ParseBody( starSystem, b, showMarketDetails ) ).RemoveNulls().ToList() ?? [ ] );

                starSystem.lastupdated = DateTime.UtcNow;
                return starSystem;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse Spansh star system: {e.Message}", e );
            }
            return null;
        }

        private static void GetFactionData ( StarSystem starSystem, JToken data )
        {
            // Get factions
            starSystem.factions.AddRange( data[ "factions" ]?.Select( f =>
                new Faction
                {
                    name = f[ "name" ]?.ToString(),
                    Allegiance = Superpower.FromNameOrEdName( f[ "allegiance" ]?.ToString() ) ??
                                 Superpower.None,
                    Government = Government.FromName( f[ "government" ]?.ToString() ) ??
                                 Government.None,
                    updatedAt = JsonParsing.getDateTime("date", data),
                    presences =
                    [
                        new FactionPresence()
                        {
                            systemName = starSystem.systemname,
                            systemAddress = starSystem.systemAddress,
                            FactionState =
                                FactionState.FromName( f[ "state" ]?.ToString() ) ??
                                FactionState.None,
                            influence = f[ "influence" ]?.Value<decimal?>() * 100,
                            updatedAt = JsonParsing.getDateTime( "date", data )
                        }
                    ]
                }
            ) ?? new List<Faction>() );

            // Get controlling faction data
            starSystem.Faction = starSystem.factions.FirstOrDefault( f =>
                f.name.Equals( data[ "controllingFaction" ]?[ "name" ]?.ToString(),
                    StringComparison.InvariantCultureIgnoreCase ) );
        }

        private static Body ParseBody ( StarSystem starSystem, JToken bodyData, bool showMarketDetails = false )
        {
            try
            {
                var name = bodyData[ "name" ]?.ToString();
                var id64 = bodyData[ "id64" ].Value<ulong>();
                var bodyId = bodyData[ "bodyId" ]?.Value<long?>();
                var temperatureKelvin = bodyData["surfaceTemperature"]?.Value<decimal?>();
                var type =  bodyData[ "type" ]?.ToString();

                var ascendingNode = bodyData[ "ascendingNode" ]?.Value<decimal?>();
                var axialTiltDegrees = bodyData[ "axialTilt" ]?.Value<decimal?>();
                var distanceLs = bodyData[ "distanceToArrival" ]?.Value<decimal?>();
                var eccentricity = bodyData["orbitalEccentricity"]?.Value<decimal?>();
                var meanAnomaly = bodyData[ "meanAnomaly" ]?.Value<decimal?>();
                var orbitalInclinationDegrees = bodyData[ "orbitalInclination" ]?.Value<decimal?>();
                var orbitalPeriodDays = bodyData[ "orbitalPeriod" ]?.Value<decimal?>();
                var parents = bodyData[ "parents" ]?.ToObject<List<IDictionary<string, int>>>() ??
                              [ ];
                var periapsisDegrees = bodyData[ "argOfPeriapsis" ]?.Value<decimal?>();
                var ringsData = bodyData["rings"] ?? bodyData["belts"];
                var rings = ringsData?.Select( ringToken => new Ring(
                ringToken[ "name" ]?.ToString(),
                RingComposition.FromName( ringToken[ "type" ]?.ToString() ),
                ringToken[ "mass" ]?.Value<decimal?>() ?? 0,
                ringToken[ "innerRadius" ]?.Value<decimal?>() ?? 0,
                ringToken[ "outerRadius" ]?.Value<decimal?>() ?? 0
            ) ).ToList() ?? [ ];
                var rotationalPeriodDays = bodyData[ "rotationalPeriod" ]?.Value<decimal?>();
                var semiMajorAxisLs = ConstantConverters.au2ls( bodyData[ "semiMajorAxis" ]?.Value<decimal?>() );
                // TODO: Add `timestamps` property (for predicting orbital position)?

                // Star properties
                if ( type == "Star" )
                {
                    var star = GetStarData( bodyData, starSystem, name, bodyId, id64, parents, distanceLs,
                    temperatureKelvin, semiMajorAxisLs, eccentricity, orbitalInclinationDegrees, periapsisDegrees,
                    orbitalPeriodDays, rotationalPeriodDays, axialTiltDegrees, rings );
                    return star;
                }

                // Body properties
                if ( type == "Planet" )
                {
                    var planet = GetPlanetData( bodyData, starSystem, name, bodyId, id64, parents, distanceLs,
                    temperatureKelvin, semiMajorAxisLs, eccentricity, orbitalInclinationDegrees, periapsisDegrees,
                    orbitalPeriodDays, rotationalPeriodDays, axialTiltDegrees, rings, showMarketDetails );
                    return planet;
                }
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse body: {e.Message}", e );
            }
            return null;
        }

        private static Body GetStarData( JToken starData, StarSystem starSystem, string starName, long? bodyId, ulong id64,
            List<IDictionary<string, int>> parents, decimal? distanceLs, decimal? temperatureKelvin, decimal? semiMajorAxisLs, decimal? eccentricity,
            decimal? orbitalInclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays,
            decimal? rotationalPeriodDays, decimal? axialTiltDegrees, List<Ring> rings )
        {
            try
            {
                var absoluteMagnitude = starData[ "absoluteMagnitude" ]?.Value<decimal?>();
                var ageMegaYears = starData[ "age" ]?.Value<long?>();
                var luminosityClass = starData[ "luminosity" ]?.ToString();
                //var mainStar = starData[ "mainStar" ]?.Value<bool?>();
                var solarMasses = starData[ "solarMasses" ]?.Value<decimal?>();
                var solarRadius = starData[ "solarRadius" ]?.Value<decimal?>();
                var radiusKm = ( solarRadius * Constants.solarRadiusMeters / 1000 ) ?? 0;
                var stellarclass = StarClass.FromName(starData["subType"]?.ToString())?.edname; // Map back from the name to the edname 
                int? stellarsubclass = null;
                var endOfSpectralClass = ((string)starData["spectralClass"])?.LastOrDefault().ToString();
                if ( int.TryParse( endOfSpectralClass, out var subclass ) )
                {
                    // If our spectralClass ends in a number, we need to separate the class from the subclass
                    stellarsubclass = subclass;
                }
                var star = new Body( starName, bodyId, starSystem.systemname, id64, parents, distanceLs,
                    stellarclass, stellarsubclass, solarMasses, radiusKm, absoluteMagnitude, ageMegaYears,
                    temperatureKelvin, luminosityClass, semiMajorAxisLs, eccentricity,
                    orbitalInclinationDegrees, periapsisDegrees, orbitalPeriodDays, rotationalPeriodDays,
                    axialTiltDegrees, rings, true, false );
                var updatedAt = JsonParsing.getDateTime("updateTime", starData );
                star.updatedat = updatedAt == DateTime.MinValue ? null : (long?)Dates.fromDateTimeToSeconds( updatedAt );
                return star;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse star: {e.Message}", e );
                throw;
            }
        }

        private static Body GetPlanetData( JToken planetData, StarSystem starSystem, string planetName, long? bodyId, ulong id64,
            List<IDictionary<string, int>> parents, decimal? distanceLs, decimal? temperatureKelvin, decimal? semiMajorAxisLs, decimal? eccentricity,
            decimal? orbitalInclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays,
            decimal? rotationalPeriodDays, decimal? axialTiltDegrees, List<Ring> rings, bool showMarketDetails = false )
        {
            try
            {
                // Gas giants receive an empty string. Fix it, since gas giants have atmospheres. 
                var atmosphereClass = ( planetData[ "subType" ]?.ToString().Contains( "gas giant" ) ?? false ) &&
                                  ( string.IsNullOrEmpty( planetData[ "atmosphereType" ]?.ToString() ) ||
                                    planetData[ "atmosphereType" ]?.ToString() == "No atmosphere" )
                ? AtmosphereClass.FromEDName( "GasGiant" )
                : AtmosphereClass.FromName( planetData[ "atmosphereType" ]?.ToString() ) ?? AtmosphereClass.None;

                var atmosphereCompositions = planetData[ "atmosphereComposition" ]?.Select( a =>
                {
                    var atmosComp = a.ToObject<JProperty>();
                    return new AtmosphereComposition( atmosComp.Name, atmosComp.Value.Value<decimal>() );
                } ).OrderByDescending( x => x.percent ).ToList() ?? [ ];

                var earthmass = planetData[ "earthMasses" ]?.Value<decimal?>();
                var gravity = planetData[ "gravity" ]?.Value<decimal?>() ?? 0;
                var landable = planetData["isLandable"]?.Value<bool?>();

                var materials = planetData[ "materials" ]?.Select( m =>
                {
                    var mtrl = m.ToObject<JProperty>();
                    return new MaterialPresence( mtrl.Name, mtrl.Value.Value<decimal>() );
                } ).OrderByDescending( o => o.percentage ).ToList() ?? [ ];

                var planetClass = PlanetClass.FromName( planetData[ "subType" ]?.ToString() ) ?? PlanetClass.None;
                var pressureAtm = planetData["surfacePressure"]?.Value<decimal?>();
                var radiusKm = planetData["radius"]?.Value<decimal?>();
                var reserveLevel = ReserveLevel.FromName( planetData[ "reserveLevel" ]?.ToString() ) ??
                               ReserveLevel.None;
                var tidallylocked = planetData["rotationalPeriodTidallyLocked"]?.Value<bool?>() ?? false;
                // TODO: Add `signals` property (for surface signals)

                var solidCompositions = planetData[ "solidComposition" ]?.Select( c =>
                {
                    var sldComp = c.ToObject<JProperty>();
                    return new SolidComposition( sldComp.Name, sldComp.Value.Value<decimal>() );
                } ).OrderByDescending( x => x.percent ).ToList() ?? [ ];

                starSystem.AddOrUpdateStations(
                    planetData[ "stations" ]?.AsParallel().Select( s => ParseStation( starSystem, s, planetData, showMarketDetails ) ).RemoveNulls().ToList() ??
                    [ ] );

                var terraformState = TerraformState.FromName( planetData[ "terraformingState" ]?.ToString() ) ??
                                 TerraformState.NotTerraformable;

                var volcanism = Volcanism.FromName( planetData[ "volcanismType" ]?.ToString() );

                var planet = new Body( planetName, bodyId, starSystem.systemname, id64, parents, distanceLs, tidallylocked,
                terraformState, planetClass, atmosphereClass, atmosphereCompositions,
                volcanism, earthmass, radiusKm, gravity, temperatureKelvin, pressureAtm, landable, materials,
                solidCompositions, semiMajorAxisLs, eccentricity, orbitalInclinationDegrees,
                periapsisDegrees, orbitalPeriodDays, rotationalPeriodDays, axialTiltDegrees, rings, reserveLevel, 
                true, null, null );
                var updatedAt = JsonParsing.getDateTime("updateTime", planetData );
                planet.updatedat = updatedAt == DateTime.MinValue ? null : (long?)Dates.fromDateTimeToSeconds( updatedAt );
                return planet;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse planet: {e.Message}", e );
                throw;
            }
        }

        private static Station ParseStation ( StarSystem starSystem, JToken stationData, JToken bodyData = null, bool showMarketDetails = false )
        {
            try
            {
                // Spansh does not assign on-foot surface settlements a station type so we have to assign these ourselves.
                var station = new Station
                {
                    systemname = starSystem.systemname,
                    systemAddress = starSystem.systemAddress,
                    name = stationData[ "name" ]?.ToString(),
                    marketId = stationData[ "id" ]?.Value<long?>(),
                    Model = bodyData != null && stationData[ "type" ] is null
                        ? StationModel.OnFootSettlement
                        : FromSpanshStationModel( stationData[ "type" ]?.ToString() ),
                    distancefromstar = stationData[ "distanceToArrival" ]?.Value<decimal?>() ??
                                       bodyData?[ "distanceToArrival" ]?.Value<decimal?>(),
                    // TODO: Add ground settlement body name, body ID, latitude / longitude?
                    Faction = starSystem.factions.FirstOrDefault( f =>
                                  f.name == stationData[ "controllingFaction" ]?.ToString() ) ??
                              ( stationData[ "controllingFaction" ]?.ToString() is null
                                  ? null
                                  : new Faction
                                  {
                                      name = stationData[ "controllingFaction" ]?.ToString() ?? string.Empty,
                                      Allegiance =
                                          Superpower.FromName( stationData[ "allegiance" ]?.ToString() ) ?? Superpower.None,
                                      Government = Government.FromName( stationData[ "government" ]?.ToString() ) ??
                                                   Government.None,
                                  } ),
                    landingPads = new StationLandingPads(
                        stationData[ "landingPads" ]?[ "small" ]?.Value<int>() ?? 0,
                        stationData[ "landingPads" ]?[ "medium" ]?.Value<int>() ?? 0,
                        stationData[ "landingPads" ]?[ "large" ]?.Value<int>() ?? 0 )
                    // Light seconds
                };

                var economyShares = stationData[ "economies" ]?.Select( economyToken =>
                {
                    var econShare = economyToken.ToObject<JProperty>();
                    return new EconomyShare( econShare.Name, econShare.Value.Value<decimal>() );
                } ).OrderByDescending( e => e.proportion ).ToList() ?? [ ];
                var primaryEconomyIndex = economyShares.FindIndex( e =>
                    e.economy.invariantName == stationData[ "primaryEconomy" ]?.ToString() );
                if ( primaryEconomyIndex > 0 )
                {
                    var primaryEconomy = economyShares[ primaryEconomyIndex ];
                    economyShares = economyShares.Except( [ primaryEconomy ] ).Prepend( primaryEconomy ).ToList();
                }

                station.economyShares = economyShares;

                station.stationServices = stationData[ "services" ]?
                    .Select( t => StationService.FromName( t.ToString() ) )
                    .ToList() ?? [ ];

                if ( showMarketDetails )
                {
                    if ( stationData[ "market" ] != null )
                    {
                        station.commodities = stationData[ "market" ]?[ "commodities" ]
                            ?.Select( c =>
                                new CommodityMarketQuote( CommodityDefinition.FromEDName( c[ "symbol" ]?.ToString() ) )
                                {
                                    buyprice = c[ "buyPrice" ]?.Value<decimal?>() ?? 0,
                                    demand = c[ "demand" ]?.Value<int?>() ?? 0,
                                    sellprice = c[ "sellPrice" ]?.Value<decimal?>() ?? 0,
                                    stock = c[ "supply" ]?.Value<int?>() ?? 0
                                }
                            ).ToList() ?? [ ];
                        station.prohibited = stationData[ "market" ]?[ "prohibitedCommodities" ]
                                                 ?.Select( p => CommodityDefinition.FromName( p.ToString() ) )
                                                 .ToList() ??
                                             [ ];
                        var marketUpdatedAt = JsonParsing.getDateTime( "updateTime", stationData[ "market" ] );
                        station.commoditiesupdatedat = marketUpdatedAt == DateTime.MinValue
                            ? null
                            : (long?)Dates.fromDateTimeToSeconds( marketUpdatedAt );
                    }

                    if ( stationData[ "outfitting" ]?[ "modules" ] != null )
                    {
                        station.outfitting = stationData[ "outfitting" ]?[ "modules" ]
                                                 ?.Select( m => Module.FromEDName( m[ "symbol" ]?.ToString() ) )
                                                 .ToList() ??
                                             [ ];
                        var outfittingUpdatedAt = JsonParsing.getDateTime( "updateTime", stationData[ "outfitting" ] );
                        station.outfittingupdatedat = outfittingUpdatedAt == DateTime.MinValue
                            ? null
                            : (long?)Dates.fromDateTimeToSeconds( outfittingUpdatedAt );
                    }

                    if ( stationData[ "shipyard" ]?[ "ships" ] != null )
                    {
                        station.shipyard = stationData[ "shipyard" ]?[ "ships" ]
                                               ?.Select( s => ShipDefinitions.FromEDModel( s[ "symbol" ]?.ToString() ) )
                                               .ToList() ??
                                           [ ];
                        var shipyardUpdatedAt = JsonParsing.getDateTime( "updateTime", stationData[ "shipyard" ] );
                        station.shipyardupdatedat = shipyardUpdatedAt == DateTime.MinValue
                            ? null
                            : (long?)Dates.fromDateTimeToSeconds( shipyardUpdatedAt );
                    }
                }

                var updatedAt = JsonParsing.getDateTime( "updateTime", stationData );
                station.updatedat = updatedAt == DateTime.MinValue
                    ? null
                    : (long?)Dates.fromDateTimeToSeconds( updatedAt );
                return station;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse station: {e.Message}", e );
                return null;
            }
        }

        private static StationModel FromSpanshStationModel ( string spanshModel )
        {
            if ( string.IsNullOrEmpty( spanshModel ) ) { return null; }
            var modelTranslations = new Dictionary<string, StationModel>
            {
                { "Asteroid base", StationModel.AsteroidBase },
                { "Bernal Starport", StationModel.Bernal }, // Ocellus starports are described by the journal as either "Bernal" or "Ocellus"
                { "Civilian Mega Ship", StationModel.MegaShipCivilian },
                { "Civilian Outpost", StationModel.Outpost },
                { "Commercial Outpost", StationModel.Outpost },
                { "Coriolis Starport", StationModel.Coriolis },
                { "Drake-Class Carrier", StationModel.FleetCarrier },
                { "Industrial Outpost", StationModel.Outpost },
                { "Mega ship", StationModel.Megaship },
                { "Military Outpost", StationModel.Outpost },
                { "Mining Outpost", StationModel.Outpost },
                { "Ocellus Starport", StationModel.Ocellus },
                { "Orbis Starport", StationModel.Orbis },
                { "Outpost", StationModel.Outpost},
                { "Planetary Outpost", StationModel.CraterOutpost },
                { "Planetary Port", StationModel.CraterPort },
                { "Scientific Outpost", StationModel.Outpost },
                { "Settlement", StationModel.OnFootSettlement }
            };
            return modelTranslations.TryGetValue( spanshModel, out var model )
                ? model
                : StationModel.FromName( spanshModel );
        }
    }
}