using EddiDataDefinitions;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace EddiStarMapService
{
    public partial class StarMapService
    {
        public async Task<Traffic> GetStarMapHostilityAsync(string systemName, long? edsmId = null)
        {
            if ( !TrySetEdsmCredentials() ) { return null; }

            var traffic = await GetStarMapTrafficAsync(systemName, edsmId);
            var deaths = await GetStarMapDeathsAsync(systemName, edsmId);

            if (traffic != null && deaths != null)
            {
                var hostility = new Traffic(
                    decimal.Divide((long)deaths.total, (long)traffic.total) * 100,
                    decimal.Divide((long)deaths.week, (long)traffic.week) * 100,
                    decimal.Divide((long)deaths.day, (long)traffic.day) * 100
                );
                return hostility;
            }
            return null;
        }

        public async Task<Traffic> GetStarMapTrafficAsync ( string systemName, long? edsmId = null )
        {
            if ( systemName == null ) { return null; }
            if ( !TrySetEdsmCredentials() ) { return new Traffic(); }

            var url = $"{baseUrl}api-system-v1/traffic";
            var parameters = new Dictionary<string, string> { { "systemName", systemName } };
            if ( edsmId != null ) { parameters.Add( "systemId", edsmId.ToString() ); }

            using ( var httpContent = new FormUrlEncodedContent( parameters ) )
            {
                var responseJson = await edsmHttpClient.PostAsync( url, httpContent );
                var token = responseJson is null ? null : JToken.Parse( responseJson );
                if ( token is JObject response )
                {
                    return ParseStarMapTraffic( response );
                }
            }

            return null;
        }

        public Traffic ParseStarMapTraffic(JObject response)
        {
            if ( response.IsNullOrEmpty() || !response.TryGetValue( "traffic", out var trafficToken ) )
            {
                return new Traffic();
            }

            var traffic = trafficToken.ToObject<Traffic>() ?? new Traffic();
            return traffic;
        }

        public async Task<Traffic> GetStarMapDeathsAsync ( string systemName, long? edsmId = null )
        {
            if ( systemName == null ) { return null; }
            if ( !TrySetEdsmCredentials() ) { return new Traffic(); }

            var url = $"{baseUrl}api-system-v1/deaths";
            var parameters = new Dictionary<string, string> { { "systemName", systemName } };
            if ( edsmId != null ) { parameters.Add( "systemId", edsmId.ToString() ); }

            using ( var httpContent = new FormUrlEncodedContent( parameters ) )
            {
                var responseJson = await edsmHttpClient.PostAsync( url, httpContent );
                var token = responseJson is null ? null : JToken.Parse( responseJson );
                if ( token is JObject response )
                {
                    return ParseStarMapDeaths( response );
                }
            }

            return null;
        }

        public Traffic ParseStarMapDeaths ( JObject response )
        {
            if ( response.IsNullOrEmpty() || !response.TryGetValue( "deaths", out var deathsToken ) )
            {
                return new Traffic();
            }

            var deaths = deathsToken.ToObject<Traffic>() ?? new Traffic();
            return deaths;
        }
    }
}
