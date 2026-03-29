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
        // Uses the Spansh station quick API (brief station data), e.g. https://spansh.co.uk/api/station/3707582976 
        // Useful for quickly obtaining sparse system stations.
        public async Task<NavWaypoint> GetQuickStationAsync ( long marketId, CancellationToken cancellationToken )
        {
            if ( marketId == 0 ) { return null; }

            try
            {
                var requestUri = $"station/{marketId}";
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
                        return ParseQuickStationWaypoint( jResponse[ "record" ] );
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
            }

            return null;
        }

        public async Task<IList<NavWaypoint>> GetQuickStationsAsync ( long[] marketIds, CancellationToken cancellationToken )
        {
            var stations = await Task
                .WhenAll( marketIds.AsParallel().Select( s => GetQuickStationAsync( s, cancellationToken ) ) )
                .ConfigureAwait( false );
            return stations.RemoveNulls();
        }

        public static NavWaypoint ParseQuickStationWaypoint ( JToken stationData )
        {
            try
            {
                if ( stationData is null ) { return null; }

                var systemName = stationData[ "system_name" ]?.ToString();
                var systemAddress = stationData[ "system_id64" ]?.Value<ulong>() ?? 0;
                var systemX = stationData[ "system_x" ]?.Value<decimal>() ?? 0;
                var systemY = stationData[ "system_y" ]?.Value<decimal>() ?? 0;
                var systemZ = stationData[ "system_z" ]?.Value<decimal>() ?? 0;

                return new NavWaypoint( systemName, systemAddress, systemX, systemY, systemZ )
                {
                    stationName = stationData[ "name" ]?.ToString(),
                    marketID = stationData[ "market_id" ]?.Value<long>()
                };
            }
            catch ( Exception e )
            {
                Logging.Error( $"Failed to parse quick station waypoint: {e.Message}", e );
                throw;
            }
        }
    }
}