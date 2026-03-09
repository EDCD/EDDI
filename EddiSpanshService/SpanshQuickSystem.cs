using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpanshService
{
    public partial class SpanshService
    {
        // Uses the Spansh star system quick API (brief star system data), e.g. https://spansh.co.uk/api/system/3932277478106
        // Useful for quickly obtaining sparse system stations.
        public async Task<StarSystem> GetQuickStarSystemAsync( ulong systemAddress, CancellationToken cancellationToken )
        {
            if ( systemAddress == 0 ) { return null; }

            try
            {
                var requestUri = $"system/{systemAddress}";
                var clientResponse = await spanshHttpClient.GetAsync( requestUri, cancellationToken ).ConfigureAwait(false);
                clientResponse.EnsureSuccessStatusCode();
                var responseJson = await clientResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                if ( string.IsNullOrEmpty( responseJson ) )
                {
                    Logging.Warn( "Spansh API returned no result" );
                    return null;
                }

                try
                {
                    var jResponse = JToken.Parse( responseJson );
                    if ( jResponse.Contains( "error" ) )
                    {
                        Logging.Debug( "Spansh responded with: " + jResponse[ "error" ] );
                    }
                    else
                    {
                        return ParseQuickSystem( jResponse[ "record" ] );
                    }
                }
                catch ( Exception e )
                {
                    Logging.Error( "Failed to parse Spansh response", e );
                }
            }
            catch ( TaskCanceledException )
            {
                // Task cancelled, nothing to do except return.
            }
            catch ( HttpRequestException he )
            {
                Logging.Warn( he.Message, he );
                return null;
            }

            return null;
        }

        public async Task<IList<StarSystem>> GetQuickStarSystemsAsync ( ulong[] systemAddresses, CancellationToken cancellationToken )
        {
            var quickSystems = await Task
                .WhenAll( systemAddresses.AsParallel().Select( s => GetQuickStarSystemAsync( s, cancellationToken ) ) )
                .ConfigureAwait( false );
            return quickSystems.RemoveNulls();
        }

        private static StarSystem ParseQuickSystem ( JToken data )
        {
            try
            {
                var starSystem = new StarSystem
                {
                    systemname = data[ "name" ].ToString(),
                    systemAddress = data[ "id64" ].Value<ulong>(),
                    x = data[ "x" ]?.Value<decimal>(),
                    y = data[ "y" ]?.Value<decimal>(),
                    z = data[ "z" ]?.Value<decimal>(),
                    updatedat = Dates.fromDateTimeToSeconds( JsonParsing.getDateTime("updated_at", data) )
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

                starSystem.AddOrUpdateStations( data[ "stations" ]?.AsParallel().Select( s =>
                {
                    var station = ParseQuickStation( s );
                    station.systemname = starSystem.systemname;
                    station.systemAddress = starSystem.systemAddress;
                    return station;
                } ).RemoveNulls().ToList() ?? new List<Station>() );

                starSystem.factions.AddRange(
                    data[ "minor_faction_presences" ]?.AsParallel().Select( ParseQuickFaction ).RemoveNulls()
                        .ToList() ?? new List<Faction>() );

                starSystem.lastupdated = DateTime.UtcNow;
                return starSystem;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse quick star system: {e.Message}", e );
            }
            return null;
        }

        private static Station ParseQuickStation ( JToken stationData )
        {
            // Spansh does not assign on-foot surface settlements a station type so we have to assign these ourselves.
            try
            {
                var station = new Station
                {
                    name = stationData[ "name" ]?.ToString(),
                    marketId = stationData[ "market_id" ]?.Value<long?>(),
                    Model = FromSpanshStationModel( stationData[ "type" ]?.ToString() ) ?? 
                            StationModel.OnFootSettlement,
                    landingPads = new StationLandingPads(
                        stationData[ "small_pads" ]?.Value<int?>() ?? 0,
                        stationData[ "medium_pads" ]?.Value<int?>() ?? 0,
                        stationData[ "large_pads" ]?.Value<int?>() ?? 0 ),
                    stationServices = stationData[ "services" ]?
                        .Select( t => StationService.FromName( t.ToString() ) )
                        .ToList() ?? new List<StationService>()
                };
                station.hasdocking = ( station.landingPads.Large + station.landingPads.Medium + station.landingPads.Small ) > 0;
                return station;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse quick station: {e.Message}", e );
                return null;
            }
        }
        
        private static Faction ParseQuickFaction ( JToken factionData )
        {
            // Spansh provides the faction name, influence, and state but all we'll gather are the names
            try
            {
                var faction = new Faction
                {
                    name = factionData[ "name" ]?.ToString()
                };
                return faction;
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse quick faction: {e.Message}", e );
                return null;
            }
        }
    }
}