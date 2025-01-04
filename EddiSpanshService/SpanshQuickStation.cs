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
        // Uses the Spansh station quick API (brief station data), e.g. https://spansh.co.uk/api/station/3707582976 
        // Useful for quickly obtaining sparse system stations.
        public NavWaypoint GetQuickStation (long marketId)
        {
            if ( marketId == 0 ) { return null; }
            var request = new RestRequest($"station/{marketId}");
            if (TryGetQuickStation(request, out var quickStation))
            {
                return quickStation;
            }
            return null;
        }

        public IList<NavWaypoint> GetQuickStations ( long[] marketIds )
        {
            return marketIds.AsParallel()
                .Select( GetQuickStation )
                .ToList();
        }

        private bool TryGetQuickStation ( IRestRequest request, out NavWaypoint quickStation )
        {
            var clientResponse = spanshRestClient.Get(request);
            quickStation = null;
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
                    quickStation = ParseQuickStation( jResponse[ "record" ] );
                }
                catch ( Exception e )
                {
                    Logging.Error( "Failed to parse Spansh response", e );
                }
            }
            else
            {
                Logging.Debug( "Spansh responded with: " + clientResponse.ErrorMessage, clientResponse.ErrorException );
            }

            return quickStation != null;
        }

        public NavWaypoint ParseQuickStation ( JToken stationData )
        {
            if ( stationData is null )
            { return null; }

            var systemName = stationData[ "system_name" ]?.ToString();
            var systemAddress = stationData[ "system_id64" ]?.ToObject<ulong>() ?? 0;
            var systemX = stationData[ "system_x" ]?.ToObject<decimal>() ?? 0;
            var systemY = stationData[ "system_y" ]?.ToObject<decimal>() ?? 0;
            var systemZ = stationData[ "system_z" ]?.ToObject<decimal>() ?? 0;

            return new NavWaypoint( systemName, systemAddress, systemX, systemY, systemZ )
            {
                stationName = stationData[ "name" ]?.ToString(),
                marketID = stationData[ "market_id" ]?.ToObject<long>()
            };
        }
    }
}