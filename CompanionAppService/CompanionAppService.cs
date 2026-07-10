using EddiCompanionAppService.Exceptions;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCompanionAppService
{
    public class CompanionAppService : IDisposable, INotifyPropertyChanged, ICompanionAppServiceInitializer
    {
        // Implementation instructions from Frontier: https://hosting.zaonce.net/docs/oauth2/instructions.html
        private const string LIVE_SERVER = "https://companion.orerve.net";
        private const string BETA_SERVER = "https://pts-companion.orerve.net";
        private const string AUTH_SERVER = "https://auth.frontierstore.net";
        private static readonly string CALLBACK_URL = $"{Constants.EDDI_URL_PROTOCOL}://auth/";
        private const string AUTH_URL = "/auth";
        private const string DECODE_URL = "/decode";
        private const string TOKEN_URL = "/token";
        private const string AUDIENCE = "audience=all";
        private const string SCOPE = "scope=capi auth";

        private readonly HttpClient httpClient;
        private CustomURLResponder URLResponder;
        private string verifier;
        private string authSessionID;
        private CompanionAppCredentials Credentials;
        private readonly SemaphoreSlim refreshLock = new(1, 1);
        private readonly Func<string, CompanionAppCredentials> loadCredentials;
        private readonly string credentialsFilePath;
        
        public bool gameIsBeta { get; set; }
        public bool unitTesting;

        #region State Variables

        public enum State
        {
            LoggedOut,
            AwaitingCallback,
            Authorized,
            ConnectionLost,
            NoClientIDConfigured,
            TokenRefresh,
        };
        private State _currentState;
        public State CurrentState
        {
            get => _currentState;
            private set
            {
                if (_currentState == value) { return; }
                var oldState = _currentState;
                _currentState = value;
                StateChanged?.Invoke(oldState, _currentState);
                OnPropertyChanged();
            }
        }
        public delegate void StateChangeHandler(State oldState, State newState);

        public event StateChangeHandler StateChanged;
        public bool active => CurrentState == State.Authorized;

        #endregion

        #region Instance

        private static CompanionAppService instance;
        private readonly string clientID; // we are not allowed to check the client ID into version control or publish it to 3rd parties

        private static readonly object instanceLock = new();
        public static CompanionAppService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No companion API instance: creating one");
                            instance = new CompanionAppService();
                        }
                    }
                }
                return instance;
            }
        }

        #endregion

        #region Endpoints

        public readonly Endpoints.FleetCarrierEndpoint FleetCarrierEndpoint = new();
        public readonly Endpoints.ProfileEndpoint ProfileEndpoint = new();
        public readonly Endpoints.CombinedStationEndpoints CombinedStationEndpoints = new();
        public readonly Endpoints.SquadronEndpoint SquadronEndpoint = new();

        #endregion

        private CompanionAppService ()
            : this(
                new HttpClient(),
                CompanionAppCredentials.Load,
                ClientId.ID,
                runStartupRefresh: true )
        { }

        internal CompanionAppService (
            HttpClient httpClient,
            Func<string, CompanionAppCredentials> loadCredentials,
            string clientID,
            bool runStartupRefresh = false,
            string credentialsFilePath = null )
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException( nameof(httpClient) );
            this.loadCredentials = loadCredentials ?? throw new ArgumentNullException( nameof(loadCredentials) );
            this.clientID = clientID;
            this.credentialsFilePath = credentialsFilePath;

            Credentials = this.loadCredentials( this.credentialsFilePath );

            this.httpClient.DefaultRequestHeaders.UserAgent.ParseAdd( $"{Constants.EDDI_NAME}/{Constants.EDDI_VERSION}" );
            this.httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue( "application/json" ) );

            if ( clientID == null )
            {
                CurrentState = State.NoClientIDConfigured;
                return;
            }

            if ( !runStartupRefresh )
            {
                CurrentState = State.Authorized;
                return;
            }

            // Our access token may have expired. Use our refresh token to obtain a new access token.
            TryRefreshTokenAsync().SafeFireAndForget( ex =>
            {
                Logging.Warn( "Initial companion API token refresh failed.", ex );
                CurrentState = string.IsNullOrEmpty( Credentials.refreshToken )
                    ? State.LoggedOut
                    : State.ConnectionLost;
            } );
        }

        /// <summary>Initialize a custom URL responder for OAuth callbacks. This responder uses DDE and should only be called if the UI dispatcher is available.</summary>
        public void InitializeOAuthCallback()
        {
            static void logger ( string message ) => Logging.Error( message );
            var appPath = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            URLResponder = new CustomURLResponder(Constants.EDDI_URL_PROTOCOL, handleCallbackUrlAsync, logger, appPath);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                URLResponder?.Dispose();
            }
            // dispose unmanaged resources
        }

        protected internal string ServerURL()
        {
            return gameIsBeta 
                ? BETA_SERVER 
                : LIVE_SERVER;
        }

        ///<summary>Log in. Throws an exception if it fails</summary>
        public void Login()
        {
            Logging.Debug("Request initiated");
            if (CurrentState != State.LoggedOut)
            {
                // Shouldn't be here
                throw new EliteDangerousCompanionAppIllegalStateException("Service in incorrect state to login (" + CurrentState + ")");
            }

            if (clientID == null)
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Client ID is not configured");
            }

            CurrentState = State.AwaitingCallback;
            var codeChallenge = createAndRememberChallenge();
            var webURL = $"{AUTH_SERVER}{AUTH_URL}" + $"?response_type=code&{AUDIENCE}&{SCOPE}&client_id={clientID}&code_challenge={codeChallenge}&code_challenge_method=S256&state={authSessionID}&redirect_uri={Uri.EscapeDataString(CALLBACK_URL)}";
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = webURL,
                    UseShellExecute = true
                };
                Process.Start( psi );
                Logging.Debug( "Awaiting callback" );
            }
            catch ( Win32Exception win32Exception )
            {
                Logging.Warn("Unable to login: " + win32Exception.Message, win32Exception );
                Logout();
            }
        }

        private string createAndRememberChallenge()
        {
            var rawVerifier = new byte[32];
            using ( var rng = RandomNumberGenerator.Create() )
            {
                rng.GetBytes( rawVerifier );
                verifier = base64UrlEncode( rawVerifier );

                var rawAuthSessionID = new byte[8];
                rng.GetBytes( rawAuthSessionID );

                authSessionID = base64UrlEncode( rawAuthSessionID );
            }

            var byteVerifier = Encoding.ASCII.GetBytes(verifier);
            var hash = SHA256.HashData(byteVerifier);
            var codeChallenge = base64UrlEncode(hash);
            return codeChallenge;
        }

        private static string base64UrlEncode(byte[] blob)
        {
            var base64 = Convert.ToBase64String(blob, Base64FormattingOptions.None);
            return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        private async Task handleCallbackUrlAsync ( string url )
        {
            // NB any user can send an arbitrary URL from the Windows Run dialog, so it must be treated as untrusted
            try
            {
                Logging.Debug( "Received callback" );
                var code = codeFromCallback( url );

                using ( var request = new HttpRequestMessage( HttpMethod.Post, AUTH_SERVER + TOKEN_URL ) )
                {
                    request.Content = new StringContent(
                        $"grant_type=authorization_code&client_id={clientID}&code_verifier={verifier}&code={code}&redirect_uri={Uri.EscapeDataString( CALLBACK_URL )}",
                        Encoding.UTF8, "application/x-www-form-urlencoded" );

                    using ( var response = await httpClient.SendAsync( request ).ConfigureAwait( false ) )
                    {
                        response.EnsureSuccessStatusCode();

                        var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait( false );
                        var json = JObject.Parse( responseData );

                        var accessToken = json[ "access_token" ]?.ToString();
                        var refreshToken = json[ "refresh_token" ]?.ToString();
                        var expiresInSec = json[ "expires_in" ]?.Value<long?>();

                        if ( string.IsNullOrEmpty( accessToken ) ||
                             string.IsNullOrEmpty( refreshToken ) ||
                             expiresInSec is null )
                        {
                            throw new EliteDangerousCompanionAppAuthenticationException(
                                "Response is missing expected fields" );
                        }

                        Credentials.accessToken = accessToken;
                        Credentials.refreshToken = refreshToken;
                        Credentials.tokenExpiry = DateTime.UtcNow + TimeSpan.FromSeconds( expiresInSec.Value );
                        Credentials.Save();

                        if ( Credentials.accessToken == null )
                        {
                            throw new EliteDangerousCompanionAppAuthenticationException( "Access token not found" );
                        }

                        CurrentState = State.Authorized;
                    }
                }
            }
            catch ( Exception ex)
            {
                Logging.Warn( ex.Message, ex );
                CurrentState = State.LoggedOut;
            }
        }

        private string codeFromCallback(string url)
        {
            if (!(url.StartsWith(CALLBACK_URL) && url.Contains('?')))
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Malformed callback URL from Frontier");
            }

            var paramsDict = ParseQueryString(url);
            if (authSessionID == null || !paramsDict.TryGetValue("state", out var state ) || state != authSessionID)
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Unexpected callback URL from Frontier");
            }

            if (!paramsDict.TryGetValue("code", out var callback))
            {
                if (!paramsDict.TryGetValue("error_description", out var desc))
                {
                    paramsDict.TryGetValue("error", out desc);
                }
                desc ??= "no error description";
                throw new EliteDangerousCompanionAppAuthenticationException($"Negative response from Frontier: {desc}");
            }
            return callback;
        }

        private static Dictionary<string, string> ParseQueryString(string url)
        {
            // Sadly System.Web.HttpUtility.ParseQueryString() is not available to us
            // https://stackoverflow.com/questions/659887/get-url-parameters-from-a-string-in-net
            var myUri = new Uri(url);
            var query = myUri.Query.TrimStart('?');
            var queryParams = query.Split( [ '&' ], StringSplitOptions.RemoveEmptyEntries);
            var paramValuePairs = queryParams.Select(parameter => parameter.Split( [ '=' ], StringSplitOptions.RemoveEmptyEntries));
            var sanitizedValuePairs = paramValuePairs.GroupBy(
                parts => parts[0],
                parts => parts.Length > 2 ? string.Join("=", parts, 1, parts.Length - 1) : parts.Length > 1 ? parts[1] : "");
            var paramsDict = sanitizedValuePairs.ToDictionary(
                grouping => grouping.Key,
                grouping => string.Join(",", grouping));
            return paramsDict;
        }

#pragma warning disable IDE0051 // Remove unused private members - Preserve unusued method as it may be useful for future debug testing
        // ReSharper disable once UnusedMember.Local
        private async Task<JObject> DecodeTokenAsync()
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (Credentials.accessToken == null) { return null; }

            var request = new HttpRequestMessage(HttpMethod.Get, AUTH_SERVER + DECODE_URL);
            request.Headers.Add( "Authorization", $"Bearer {Credentials.accessToken}" );

            using (var response = await httpClient.SendAsync( request ).ConfigureAwait(false) )
            {
                if (response.StatusCode == HttpStatusCode.Found)
                {
                    return null;
                }
                return JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false));
            }
        }

        internal async Task<bool> TryRefreshTokenAsync ( bool force = false )
        {
            await refreshLock.WaitAsync().ConfigureAwait( false );

            // Reload in case another refresh completed and saved a new token.
            Credentials = loadCredentials( credentialsFilePath );
            
            try
            {
                if ( clientID == null )
                {
                    CurrentState = State.NoClientIDConfigured;
                    return false;
                }

                if ( !force &&
                     !string.IsNullOrEmpty( Credentials.accessToken ) &&
                     DateTime.UtcNow <= Credentials.tokenExpiry.AddSeconds( -60 ) )
                {
                    CurrentState = State.Authorized;
                    return true;
                }

                if ( string.IsNullOrEmpty( Credentials.refreshToken ) )
                {
                    // No refresh token. Can't refresh. Need to log in again.
                    CurrentState = State.LoggedOut;
                    return false;
                }

                CurrentState = State.TokenRefresh;

                using ( var request = new HttpRequestMessage( HttpMethod.Post, AUTH_SERVER + TOKEN_URL ) )
                {
                    request.Content = new FormUrlEncodedContent( new Dictionary<string, string>
                    {
                        [ "grant_type" ] = "refresh_token",
                        [ "client_id" ] = clientID,
                        [ "refresh_token" ] = Credentials.refreshToken
                    } );

                    using ( var response = await httpClient.SendAsync( request ).ConfigureAwait( false ) )
                    {
                        if ( response.StatusCode == HttpStatusCode.Unauthorized )
                        {
                            CurrentState = State.LoggedOut;
                            return false;
                        }

                        if ( !response.IsSuccessStatusCode )
                        {
                            CurrentState = State.ConnectionLost;
                            return false;
                        }

                        var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                        var json = JObject.Parse(responseData);

                        var accessToken = json["access_token"]?.ToString();
                        var refreshToken = json["refresh_token"]?.ToString();
                        var expiresInSec = json["expires_in"]?.Value<double?>();

                        if ( string.IsNullOrEmpty( accessToken ) ||
                             string.IsNullOrEmpty( refreshToken ) ||
                             expiresInSec is null )
                        {
                            CurrentState = State.ConnectionLost;
                            return false;
                        }

                        Credentials.accessToken = accessToken;
                        Credentials.refreshToken = refreshToken;
                        Credentials.tokenExpiry = DateTime.UtcNow.AddSeconds( expiresInSec.Value );
                        Credentials.Save();

                        CurrentState = State.Authorized;
                        return true;
                    }
                }
            }
            catch ( EliteDangerousCompanionAppAuthenticationException ex )
            {
                CurrentState = State.LoggedOut;
                Logging.Warn( ex.Message, ex );
                return false;
            }
            catch ( Exception ex ) when ( ex is HttpRequestException or TaskCanceledException or Newtonsoft.Json.JsonException or InvalidOperationException )
            {
                CurrentState = State.ConnectionLost;
                Logging.Warn( "Companion API token refresh failed.", ex );
                return false;
            }
            finally
            {
                refreshLock.Release();
            }
        }

        /// <summary>Log out of the companion API and remove local credentials</summary>
        public void Logout()
        {
            Credentials = loadCredentials( credentialsFilePath );
            Credentials.Clear();
            Credentials.Save();
            CurrentState = State.LoggedOut;

            Logging.Debug( "Credentials cleared" );
        }

        protected internal async Task<Tuple<string, DateTime>> obtainDataAsync ( string url )
        {
            var expiry = Credentials.tokenExpiry.AddSeconds( -60 );
            if ( DateTime.UtcNow > expiry )
            {
                // Our access token either has expired or shall expire within the next minute.
                // Use our refresh token to obtain a new access token.
                var refreshed = await TryRefreshTokenAsync().ConfigureAwait(false);
                if ( !refreshed )
                {
                    return null;
                }
            }

            if ( CurrentState == State.ConnectionLost &&
                 !string.IsNullOrEmpty( Credentials.accessToken ) &&
                 DateTime.UtcNow <= Credentials.tokenExpiry.AddSeconds( -60 ) )
            {
                // We still have a usable access token. Allow the request to retry.
                CurrentState = State.Authorized;
            }

            if ( CurrentState != State.Authorized )
            {
                return null;
            }

            var maxRetries = 3;
            var delay = 1000; // Initial delay in milliseconds
            for ( var retry = 0; retry < maxRetries; retry++ )
            {
                try
                {
                    var request = new HttpRequestMessage( HttpMethod.Get, url );
                    request.Headers.Add( "Authorization", $"Bearer {Credentials.accessToken}" );

                    using ( var response = await httpClient.SendAsync( request ).ConfigureAwait(false) )
                    {
                        if ( response.StatusCode == HttpStatusCode.OK )
                        {
                            var timestamp = DateTime
                                .Parse( response.Headers.GetValues( "date" ).FirstOrDefault() ?? string.Empty )
                                .ToUniversalTime();
                            var responseData = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                            return new Tuple<string, DateTime>( responseData, timestamp );
                        }
                        if ( response.StatusCode == HttpStatusCode.Unauthorized )
                        {
                            var refreshed = await TryRefreshTokenAsync(force: true).ConfigureAwait(false);
                            if ( !refreshed )
                            {
                                return null;
                            }

                            continue; // retry with new access token
                        }
                    }
                }
                catch ( TaskCanceledException )
                {
                    // Task cancelled, nothing to do except return.
                }
                catch ( HttpRequestException ex )
                {
                    Logging.Warn( $"Attempt {retry + 1} failed: {ex.Message}", ex );
                    if ( retry == (maxRetries - 1) )
                    {
                        throw new EliteDangerousCompanionAppErrorException( ex.Message, ex );
                    }
                }

                await Task.Delay( delay ).ConfigureAwait(false);
                delay *= 2; // Exponential backoff
            }

            return null;
        }

        internal void SetStateForTesting ( State state )
        {
            CurrentState = state;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}
