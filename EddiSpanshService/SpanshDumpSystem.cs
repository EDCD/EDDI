using EddiDataDefinitions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Uses the Spansh star system dump API (full star system data), e.g. https://www.spansh.co.uk/api/dump/10477373803
        [CanBeNull]
        public StarSystem GetStarSystem ( ulong systemAddress, bool showMarketDetails = false )
        {
            if ( systemAddress == 0 ) { return null; }

            var request = new RestRequest( $"dump/{systemAddress}" );
            if ( TryGetStarSystemDump( request, out var fullStarSystem, showMarketDetails ) )
            {
                return fullStarSystem;
            }

            return null;
        }

        [CanBeNull]
        public StarSystem GetStarSystem ( string systemName, bool showMarketDetails = false )
        {
            if ( systemName == null || string.IsNullOrEmpty( systemName ) ) { return null; }
            var systemAddress = GetWaypointsBySystemName( systemName ).FirstOrDefault()?.systemAddress;
            if ( systemAddress == null ) { return null; }
            return GetStarSystem( (ulong)systemAddress, showMarketDetails );
        }

        public IList<StarSystem> GetStarSystems ( ulong[] systemAddresses, bool showMarketDetails = false )
        {
            return systemAddresses.AsParallel()
                .Select( systemAddress => GetStarSystem( systemAddress, showMarketDetails ) )
                .RemoveNulls()
                .ToList();
        }

        private bool TryGetStarSystemDump ( IRestRequest request, out StarSystem fullStarSystem, bool showMarketDetails = false )
        {
            var clientResponse = spanshRestClient.Get( request );
            fullStarSystem = null;
            if ( clientResponse.IsSuccessful )
            {
                if ( string.IsNullOrEmpty( clientResponse.Content ) )
                {
                    Logging.Warn( "Unable to handle server response." );
                }

                try
                {
                    var jResponse = JToken.Parse( clientResponse.Content );
                    if ( jResponse.Contains( "error" ) )
                    {
                        Logging.Debug( "Spansh responded with: " + jResponse[ "error" ] );
                    }

                    fullStarSystem = ParseStarSystemDump( jResponse[ "system" ], showMarketDetails );
                }
                catch ( Exception e )
                {
                    Logging.Error( "Failed to parse Spansh response", e );
                }
            }
            else
            {
                Logging.Warn( "Spansh responded with: " + clientResponse.ErrorMessage, clientResponse.ErrorException );
            }

            return fullStarSystem != null;
        }

        private static StarSystem ParseStarSystemDump ( JToken data, bool showMarketDetails = false )
        {
            try
            {
                var starSystem = new StarSystem
                {
                    systemname = data[ "name" ].ToString(),
                    systemAddress = data[ "id64" ].ToObject<ulong>(),
                    x = data[ "coords" ]?[ "x" ]?.ToObject<decimal>(),
                    y = data[ "coords" ]?[ "y" ]?.ToObject<decimal>(),
                    z = data[ "coords" ]?[ "z" ]?.ToObject<decimal>(),
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
                starSystem.population = data[ "population" ]?.ToObject<long?>();
                if ( starSystem.population > 0 )
                {
                    GetFactionData( starSystem, data );

                    starSystem.Economies = new List<Economy>()
                    {
                        Economy.FromName( data[ "primaryEconomy" ]?.ToString() ) ?? Economy.None,
                        Economy.FromName( data[ "secondaryEconomy" ]?.ToString() ) ?? Economy.None
                    };

                    starSystem.securityLevel = SecurityLevel.FromName( (string)data[ "security" ] ) ??
                                               SecurityLevel.None;

                    starSystem.AddOrUpdateStations( data[ "stations" ]?.AsParallel().Select( stationToken => ParseStation( starSystem, stationToken, null, showMarketDetails ) ).RemoveNulls().ToList() ?? 
                                                    new List<Station>() );

                    starSystem.Power = Power.FromName( data[ "controllingPower" ]?.ToString() );
                    starSystem.powerState = PowerplayState.FromName( data[ "powerState" ]?.ToString() );
                    starSystem.NearbyPowers = ( data[ "powers" ]?
                        .Select( t => Power.FromName( t.ToString() ) )
                        .Where( p => p != starSystem.Power )
                        .ToHashSet() ?? new HashSet<Power>() ).ToList();
                    starSystem.powerAcquisitionProgress = data[ "powerConflictProgress" ]?
                        .ToDictionary( p => Power.FromName( p[ "power" ]?.ToString() ), p => p[ "progress" ].ToObject<decimal>() * 100 )
                        .OrderByDescending( p => p.Value )
                        .ToDictionary( x => x.Key, x => x.Value ) ?? new Dictionary<Power, decimal>();
                    starSystem.powerControlProgress = data[ "powerStateControlProgress" ]?.ToObject<decimal?>() ?? 0;
                    starSystem.powerReinforcementControlPoints = data[ "powerStateReinforcement" ]?.ToObject<int?>() ?? 0;
                    starSystem.powerUnderminingControlPoints = data[ "powerStateUndermining" ]?.ToObject<int?>() ?? 0;
                }

                // Get bodies
                starSystem.totalbodies = data[ "bodyCount" ]?.ToObject<int>() ?? 0;
                starSystem.AddOrUpdateBodies( data[ "bodies" ]?.AsParallel()
                    .Select( b => ParseBody( starSystem, b, showMarketDetails ) ).RemoveNulls().ToList() ?? new List<Body>() );

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
                    presences = new List<FactionPresence>
                    {
                        new FactionPresence()
                        {
                            systemName = starSystem.systemname,
                            systemAddress = starSystem.systemAddress,
                            FactionState =
                                FactionState.FromName( f[ "state" ]?.ToString() ) ??
                                FactionState.None,
                            influence = f[ "influence" ]?.ToObject<decimal?>() * 100,
                            updatedAt = JsonParsing.getDateTime("date", data)
                        }
                    }
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
                var id64 = bodyData[ "id64" ].ToObject<ulong>();
                var bodyId = bodyData[ "bodyId" ]?.ToObject<long?>();
                var temperatureKelvin = bodyData["surfaceTemperature"]?.ToObject<decimal?>();
                var type =  bodyData[ "type" ]?.ToString();

                var ascendingNode = bodyData[ "ascendingNode" ]?.ToObject<decimal?>();
                var axialTiltDegrees = bodyData[ "axialTilt" ]?.ToObject<decimal?>();
                var distanceLs = bodyData[ "distanceToArrival" ]?.ToObject<decimal?>();
                var eccentricity = bodyData["orbitalEccentricity"]?.ToObject<decimal?>();
                var meanAnomaly = bodyData[ "meanAnomaly" ]?.ToObject<decimal?>();
                var orbitalInclinationDegrees = bodyData[ "orbitalInclination" ]?.ToObject<decimal?>();
                var orbitalPeriodDays = bodyData[ "orbitalPeriod" ]?.ToObject<decimal?>();
                var parents = bodyData[ "parents" ]?.ToObject<List<IDictionary<string, object>>>() ??
                          new List<IDictionary<string, object>>();
                var periapsisDegrees = bodyData[ "argOfPeriapsis" ]?.ToObject<decimal?>();
                var ringsData = bodyData["rings"] ?? bodyData["belts"];
                var rings = ringsData?.Select( ringToken => new Ring(
                ringToken[ "name" ]?.ToString(),
                RingComposition.FromName( ringToken[ "type" ]?.ToString() ),
                ringToken[ "mass" ]?.ToObject<decimal?>() ?? 0,
                ringToken[ "innerRadius" ]?.ToObject<decimal?>() ?? 0,
                ringToken[ "outerRadius" ]?.ToObject<decimal?>() ?? 0
            ) ).ToList() ?? new List<Ring>();
                var rotationalPeriodDays = bodyData[ "rotationalPeriod" ]?.ToObject<decimal?>();
                var semiMajorAxisLs = ConstantConverters.au2ls( bodyData[ "semiMajorAxis" ]?.ToObject<decimal?>() );
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
            List<IDictionary<string, object>> parents, decimal? distanceLs, decimal? temperatureKelvin, decimal? semiMajorAxisLs, decimal? eccentricity,
            decimal? orbitalInclinationDegrees, decimal? periapsisDegrees, decimal? orbitalPeriodDays,
            decimal? rotationalPeriodDays, decimal? axialTiltDegrees, List<Ring> rings)
        {
            try
            {
                var absoluteMagnitude = starData[ "absoluteMagnitude" ]?.ToObject<decimal?>();
                var ageMegaYears = starData[ "age" ]?.ToObject<long?>();
                var luminosityClass = starData[ "luminosity" ]?.ToString();
                //var mainStar = starData[ "mainStar" ]?.ToObject<bool?>();
                var solarMasses = starData[ "solarMasses" ]?.ToObject<decimal?>();
                var solarRadius = starData[ "solarRadius" ]?.ToObject<decimal?>();
                var radiusKm = solarRadius * Constants.solarRadiusMeters / 1000 ?? 0;
                var stellarclass = StarClass.FromName((starData["subType"]?.ToString()))?.edname; // Map back from the name to the edname 
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
            List<IDictionary<string, object>> parents, decimal? distanceLs, decimal? temperatureKelvin, decimal? semiMajorAxisLs, decimal? eccentricity,
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
                    return new AtmosphereComposition( atmosComp.Name, atmosComp.Value.ToObject<decimal>() );
                } ).OrderByDescending( x => x.percent ).ToList() ?? new List<AtmosphereComposition>();

                var earthmass = planetData[ "earthMasses" ]?.ToObject<decimal?>();
                var gravity = planetData[ "gravity" ]?.ToObject<decimal?>() ?? 0;
                var landable = planetData["isLandable"]?.ToObject<bool?>();

                var materials = planetData[ "materials" ]?.Select( m =>
                {
                    var mtrl = m.ToObject<JProperty>();
                    return new MaterialPresence( mtrl.Name, mtrl.Value.ToObject<decimal>() );
                } ).OrderByDescending( o => o.percentage ).ToList() ?? new List<MaterialPresence>();

                var planetClass = PlanetClass.FromName( planetData[ "subType" ]?.ToString() ) ?? PlanetClass.None;
                var pressureAtm = planetData["surfacePressure"]?.ToObject<decimal?>();
                var radiusKm = planetData["radius"]?.ToObject<decimal?>();
                var reserveLevel = ReserveLevel.FromName( planetData[ "reserveLevel" ]?.ToString() ) ??
                               ReserveLevel.None;
                var tidallylocked = planetData["rotationalPeriodTidallyLocked"]?.ToObject<bool?>() ?? false;
                // TODO: Add `signals` property (for surface signals)

                var solidCompositions = planetData[ "solidComposition" ]?.Select( c =>
                {
                    var sldComp = c.ToObject<JProperty>();
                    return new SolidComposition( sldComp.Name, sldComp.Value.ToObject<decimal>() );
                } ).OrderByDescending( x => x.percent ).ToList() ?? new List<SolidComposition>();

                starSystem.AddOrUpdateStations(
                    planetData[ "stations" ]?.AsParallel().Select( s => ParseStation( starSystem, s, planetData, showMarketDetails ) ).RemoveNulls().ToList() ??
                    new List<Station>() );

                var terraformState = TerraformState.FromName( planetData[ "terraformingState" ]?.ToString() ) ??
                                 TerraformState.NotTerraformable;

                var volcanism = Volcanism.FromName( planetData[ "volcanismType" ]?.ToString() );

                var planet = new Body( planetName, bodyId, starSystem.systemname, id64, parents, distanceLs, tidallylocked,
                terraformState, planetClass, atmosphereClass, atmosphereCompositions,
                volcanism, earthmass, radiusKm, gravity, temperatureKelvin, pressureAtm, landable, materials,
                solidCompositions, semiMajorAxisLs, eccentricity, orbitalInclinationDegrees,
                periapsisDegrees, orbitalPeriodDays, rotationalPeriodDays, axialTiltDegrees, rings, reserveLevel, true,
                false );
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
                    marketId = stationData[ "id" ]?.ToObject<long?>(),
                    Model = bodyData != null && stationData[ "type" ] is null
                        ? StationModel.OnFootSettlement
                        : FromSpanshStationModel( stationData[ "type" ]?.ToString() ),
                    distancefromstar = stationData[ "distanceToArrival" ]?.ToObject<decimal?>() ??
                                       bodyData?[ "distanceToArrival" ]?.ToObject<decimal?>() // Light seconds
                };

                // TODO: Add ground settlement body name, body ID, latitude / longitude?

                station.Faction =
                    starSystem.factions.FirstOrDefault( f =>
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
                        } );

                station.landingPads = new StationLandingPads(
                    stationData[ "landingPads" ]?[ "small" ]?.ToObject<int>() ?? 0,
                    stationData[ "landingPads" ]?[ "medium" ]?.ToObject<int>() ?? 0,
                    stationData[ "landingPads" ]?[ "large" ]?.ToObject<int>() ?? 0 );

                var economyShares = stationData[ "economies" ]?.Select( economyToken =>
                {
                    var econShare = economyToken.ToObject<JProperty>();
                    return new EconomyShare( econShare.Name, econShare.Value.ToObject<decimal>() );
                } ).OrderByDescending( e => e.proportion ).ToList() ?? new List<EconomyShare>();
                var primaryEconomyIndex = economyShares.FindIndex( e =>
                    e.economy.invariantName == stationData[ "primaryEconomy" ]?.ToString() );
                if ( primaryEconomyIndex > 0 )
                {
                    var primaryEconomy = economyShares[ primaryEconomyIndex ];
                    economyShares = economyShares.Except( new[] { primaryEconomy } ).Prepend( primaryEconomy ).ToList();
                }

                station.economyShares = economyShares;

                station.stationServices = stationData[ "services" ]?
                    .Select( t => StationService.FromName( t.ToString() ) )
                    .ToList() ?? new List<StationService>();

                if ( showMarketDetails )
                {
                    if ( stationData[ "market" ] != null )
                    {
                        station.commodities = stationData[ "market" ]?[ "commodities" ]
                            ?.Select( c =>
                                new CommodityMarketQuote( CommodityDefinition.FromEDName( c[ "symbol" ]?.ToString() ) )
                                {
                                    buyprice = c[ "buyPrice" ]?.ToObject<decimal?>() ?? 0,
                                    demand = c[ "demand" ]?.ToObject<int?>() ?? 0,
                                    sellprice = c[ "sellPrice" ]?.ToObject<decimal?>() ?? 0,
                                    stock = c[ "supply" ]?.ToObject<int?>() ?? 0
                                }
                            ).ToList() ?? new List<CommodityMarketQuote>();
                        station.prohibited = stationData[ "market" ]?[ "prohibitedCommodities" ]
                                                 ?.Select( p => CommodityDefinition.FromName( p.ToString() ) )
                                                 .ToList() ??
                                             new List<CommodityDefinition>();
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
                                             new List<Module>();
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
                                           new List<Ship>();
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