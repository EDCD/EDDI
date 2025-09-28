using EddiConfigService;
using JetBrains.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiStarMapService
{
    public interface IEdsmHttpClient
    {
        Task<string> GetAsync ( string url );
        Task<string> PostAsync ( string url, HttpContent content );
    }

    /// <summary> Talk to the Elite: Dangerous Star Map service </summary>
    public partial class StarMapService
    {
        // The maximum batch size we will use for syncing flight logs before we write systems to our sql database
        public const int syncBatchSize = 50;

        // The default timeout for requests to EDSM. Requests can override this by setting `RestRequest.Timeout`. Both are in milliseconds.
        private const int DefaultTimeoutMilliseconds = 10000;

        public static string inGameCommanderName { get; set; }
        private string commanderName { get; set; }
        private string apiKey { get; set; }

        private IEdsmHttpClient edsmHttpClient { get; }
        // For normal use, the EDSM API base URL is https://www.edsm.net/.
        // If you need to do some testing on EDSM's API, please use the https://beta.edsm.net/ endpoint for sending data.
        private const string baseUrl = "https://www.edsm.net/";

        private static readonly BlockingCollection<IDictionary<string, object>> queuedEvents = new BlockingCollection<IDictionary<string, object>>();
        private static CancellationTokenSource syncCancellationTS; // This must be static so that it is visible to child threads and tasks
        private static readonly ConcurrentDictionary<string, ResourceRateLimit> resourceRateLimits = new ConcurrentDictionary<string, ResourceRateLimit>();

        // This API only accepts and only returns data for the "live" galaxy, game version 4.0 or later.
        private static readonly System.Version minGameVersion = new System.Version(4, 0);
        private static System.Version currentGameVersion { get; set; }
        private static string gameVersion;
        private static string gameBuild;

        public class EdsmHttpClient : IEdsmHttpClient
        {
            private HttpClient HttpClient { get; }

            public EdsmHttpClient ( HttpClient httpClient = null, TimeSpan? defaultTimeOut = null )
            {
                HttpClient = httpClient ?? new HttpClient();
                HttpClient.Timeout = defaultTimeOut ?? HttpClient.Timeout;
                HttpClient.DefaultRequestHeaders.UserAgent
                    .ParseAdd( $"{Constants.EDDI_NAME}/{Constants.EDDI_VERSION}" );
                HttpClient.DefaultRequestHeaders.Accept
                    .Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );
            }

            [CanBeNull]
            public async Task<string> GetAsync ( string url )
            {
                try
                {
                    var response = await HttpClient.GetAsync( url ).ConfigureAwait(false);
                    await AwaitResourceAsync( url ).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    UpdateRateLimits( url, response );
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch ( HttpRequestException he )
                {
                    Logging.Warn( he.Message, he );
                    return null;
                }
                catch ( TaskCanceledException )
                {
                    // Task cancelled. Nothing to do here.
                    return null;
                }
            }

            [CanBeNull]
            public async Task<string> PostAsync ( string url, HttpContent content )
            {
                try
                {
                    var response = await HttpClient.PostAsync( url, content ).ConfigureAwait(false);
                    await AwaitResourceAsync( url ).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    UpdateRateLimits( url, response );
                    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                }
                catch ( HttpRequestException he )
                {
                    Logging.Warn( he.Message, he );
                    return null;
                }
                catch ( TaskCanceledException )
                {
                    // Task cancelled. Nothing to do here.
                    return null;
                }
            }
        }

        private static async Task AwaitResourceAsync ( string url )
        {
            if ( resourceRateLimits.TryGetValue( url, out var rateLimit ) )
            {
                if ( rateLimit.remaining < 5 && rateLimit.resetTime > DateTime.UtcNow )
                {
                    // We're close to our rate limit. Allow it to reset before we continue.
                    var waitSeconds = Math.Ceiling( ( rateLimit.resetTime - DateTime.UtcNow ).TotalSeconds + 1 );
                    Logging.Warn( $"EDSM rate limit {( rateLimit.remaining > 0 ? "almost " : "" )}exceeded." );
                    Logging.Warn( $"Waiting {waitSeconds} seconds for EDSM server cool-down." );
                    await Task.Delay( TimeSpan.FromSeconds( waitSeconds ) ).ConfigureAwait(false);
                }
            }
        }

        private static void UpdateRateLimits ( string url, HttpResponseMessage response )
        {
            // Update our rate limit data
            var requestsRemaining = TryParseIntHeader( response.Headers, "X-Rate-Limit-Remaining" );
            var resetSeconds = TryParseIntHeader( response.Headers, "X-Rate-Limit-Reset" );
            if ( requestsRemaining != null && resetSeconds != null )
            {
                resourceRateLimits[ url ] = new ResourceRateLimit
                {
                    remaining = (int)requestsRemaining,
                    resetTime = DateTime.UtcNow + TimeSpan.FromSeconds( (int)resetSeconds )
                };
            }
        }

        private static int? TryParseIntHeader ( HttpResponseHeaders headers, string key )
        {
            if ( headers.TryGetValues( key, out var values ) && int.TryParse( values.FirstOrDefault(), out var result ) )
            {
                return result;
            }
            return null;
        }

        public StarMapService (IEdsmHttpClient edsmHttpClient = null, bool needsCredentials = false)
        {
            this.edsmHttpClient = edsmHttpClient ??
                                  new EdsmHttpClient( null, TimeSpan.FromMilliseconds( DefaultTimeoutMilliseconds ) );
            this.edsmJournalHttpClient = edsmHttpClient ??
                                  new EdsmHttpClient( null, TimeSpan.FromMilliseconds( JournalTimeoutMilliseconds ) );

            if (needsCredentials)
            {
                // Set up EDSM API credentials
                TrySetEdsmCredentials();
            }
        }

        private bool TrySetEdsmCredentials()
        {
            var starMapCredentials = ConfigService.Instance.edsmConfiguration;
            if (!string.IsNullOrEmpty(starMapCredentials?.apiKey))
            {
                // Commander name might come from EDSM credentials or from the game and companion app
                string cmdrName = starMapCredentials.commanderName ?? inGameCommanderName;
                if (!string.IsNullOrEmpty(cmdrName))
                {
                    apiKey = starMapCredentials.apiKey?.Trim();
                    commanderName = cmdrName?.Trim();
                }
                else
                {
                    Logging.Warn("EDSM Responder not configured: Commander name not set.");
                }
            }
            else
            {
                Logging.Warn("EDSM Responder not configured: API key not set.");
            }

            return !string.IsNullOrEmpty( commanderName ) && !string.IsNullOrEmpty( apiKey );
        }

        public static void SetGameVersion(System.Version GameVersion, string gameversion, string gamebuild)
        {
            currentGameVersion = GameVersion;
            gameVersion = gameversion;
            gameBuild = gamebuild;

            if (currentGameVersion != null && currentGameVersion < minGameVersion)
            {
                Logging.Warn($"Service disabled. Game version is {currentGameVersion}, service may only send and receive data for version {minGameVersion} or later.");
            }
        }
    }

    // response from the Star Map log API
    [UsedImplicitly]
    class StarMapResponse
    {
        public string content { get; set; }
        public Dictionary<string, object> data { get; set; }
        public bool isSuccessful { get; set; }
        public string errorMessage { get; set; }
        public Exception errorException { get; set; }
    }

    [UsedImplicitly]
    class StarMapLogResponse
    {
        public int msgnum { get; set; }
        public string msg { get; set; }
        public string comment { get; set; }
        public DateTime? lastUpdate { get; set; }
        public List<StarMapResponseLogEntry> logs { get; set; }
    }

    [UsedImplicitly]
    public class StarMapResponseLogEntry
    {
        public string system { get; set; } // System name

        public long systemId { get; set; } // EDSM ID
        public ulong systemId64 { get; set; } // System address
        public DateTime date { get; set; }
    }

    // public consolidated version of star map log information
    [UsedImplicitly]
    public class StarMapInfo
    {
        public int Visits { get; set; }
        public DateTime? LastVisited { get; set; }
        public string Comment { get; set; }

        public StarMapInfo(int visits, DateTime? lastVisited, string comment)
        {
            Visits = visits;
            LastVisited = lastVisited;
            Comment = comment;
        }
    }

    [UsedImplicitly]
    public class ResourceRateLimit
    {
        // The number of requests remaining in the period prior to the rate reset
        public int remaining { get; set; }

        // The duration in seconds before the rate reset
        public DateTime resetTime { get; set; }
    }
}
