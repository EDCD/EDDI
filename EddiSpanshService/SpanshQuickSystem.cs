using EddiDataDefinitions;
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
        // Uses the Spansh star system quick API (brief star system data), e.g. https://spansh.co.uk/api/system/3932277478106
        // Useful for quickly obtaining sparse system stations.
        public StarSystem GetQuickStarSystem(ulong systemAddress)
        {
            if ( systemAddress == 0 ) { return null; }
            var request = new RestRequest($"system/{systemAddress}");
            if (TryGetQuickSystem(request, out var quickStarSystem))
            {
                return quickStarSystem;
            }
            return null;
        }

        public IList<StarSystem> GetQuickStarSystems ( ulong[] systemAddresses )
        {
            return systemAddresses.AsParallel().Select( GetQuickStarSystem ).RemoveNulls().ToList();
        }

        private bool TryGetQuickSystem ( IRestRequest request, out StarSystem quickStarSystem )
        {
            var clientResponse = spanshRestClient.Get(request);
            quickStarSystem = null;
            if (clientResponse.IsSuccessful)
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
                        Logging.Debug( "Spansh responded with: " + jResponse["error"] );
                    }
                    quickStarSystem = ParseQuickSystem( jResponse[ "record" ] );
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

            return quickStarSystem != null;
        }

        private static StarSystem ParseQuickSystem ( JToken data )
        {
            try
            {
                var starSystem = new StarSystem
                {
                    systemname = data[ "name" ].ToString(),
                    systemAddress = data[ "id64" ].ToObject<ulong>(),
                    x = data[ "x" ]?.ToObject<decimal>(),
                    y = data[ "y" ]?.ToObject<decimal>(),
                    z = data[ "z" ]?.ToObject<decimal>(),
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

                // Spansh does not assign on-foot surface settlements a station type so we have to assign these ourselves.
                starSystem.AddOrUpdateStations( data[ "stations" ]?.AsParallel().Select( s =>
                {
                    var station = ParseQuickStation( s );
                    station.systemname = starSystem.systemname;
                    station.systemAddress = starSystem.systemAddress;
                    return station;
                } ).RemoveNulls().ToList() ?? new List<Station>() );

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
            try
            {
                var station = new Station
                {
                    name = stationData[ "name" ]?.ToString(),
                    marketId = stationData[ "market_id" ]?.ToObject<long?>(),
                    Model = FromSpanshStationModel( stationData[ "type" ]?.ToString() ) ?? StationModel.OnFootSettlement,
                    landingPads = new StationLandingPads(
                        stationData[ "small_pads" ]?.ToObject<int?>() ?? 0,
                        stationData[ "medium_pads" ]?.ToObject<int?>() ?? 0,
                        stationData[ "large_pads" ]?.ToObject<int?>() ?? 0 )
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
    }
}