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
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiCompanionAppService
{
    public class CompanionAppService : IDisposable, INotifyPropertyChanged
    {
        // Implementation instructions from Frontier: https://hosting.zaonce.net/docs/oauth2/instructions.html
        private static readonly string LIVE_SERVER = "https://companion.orerve.net";
        private static readonly string BETA_SERVER = "https://pts-companion.orerve.net";
        private static readonly string AUTH_SERVER = "https://auth.frontierstore.net";
        private static readonly string CALLBACK_URL = $"{Constants.EDDI_URL_PROTOCOL}://auth/";
        private static readonly string AUTH_URL = "/auth";
        private static readonly string DECODE_URL = "/decode";
        private static readonly string TOKEN_URL = "/token";
        private static readonly string AUDIENCE = "audience=all";
        private static readonly string SCOPE = "scope=capi auth";

        private static readonly HttpClient httpClient = new HttpClient();
        private readonly CustomURLResponder URLResponder;
        private string verifier;
        private string authSessionID;
        public CompanionAppCredentials Credentials;

        public bool gameIsBeta { get; set; } = false;
        public static bool unitTesting;

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
            protected internal set
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

        private static readonly object instanceLock = new object();
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

        public readonly Endpoints.FleetCarrierEndpoint FleetCarrierEndpoint = new Endpoints.FleetCarrierEndpoint();
        public readonly Endpoints.ProfileEndpoint ProfileEndpoint = new Endpoints.ProfileEndpoint();
        public readonly Endpoints.CombinedStationEndpoints CombinedStationEndpoints = new Endpoints.CombinedStationEndpoints();

        #endregion

        private CompanionAppService()
        {
            Credentials = CompanionAppCredentials.Load();
            var appPath = System.Reflection.Assembly.GetEntryAssembly()?.Location;
            void logger(string message) => Logging.Error(message);
            URLResponder = new CustomURLResponder(Constants.EDDI_URL_PROTOCOL, handleCallbackUrl, logger, appPath);
            clientID = ClientId.ID;
            if (clientID == null)
            {
                CurrentState = State.NoClientIDConfigured;
                return;
            }
            if (unitTesting)
            {
                CurrentState = State.Authorized;
                return;
            }

            Task.Run( async () =>
            {
                try
                {
                    // Our access token may have expired. Use our refresh token to obtain a new access token.
                    await RefreshTokenAsync();
                }
                catch ( Exception )
                {
                    if ( Credentials.refreshToken != null )
                    {
                        CurrentState = State.ConnectionLost;
                    }

                    CurrentState = State.LoggedOut;
                }
            } ).ConfigureAwait( false );
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                URLResponder.Dispose();
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
                Process.Start( webURL );
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
            var hash = SHA256.Create().ComputeHash(byteVerifier);
            var codeChallenge = base64UrlEncode(hash);
            return codeChallenge;
        }

        private string base64UrlEncode(byte[] blob)
        {
            var base64 = Convert.ToBase64String(blob, Base64FormattingOptions.None);
            return base64.Replace('+', '-').Replace('/', '_').Replace("=", "");
        }

        private void handleCallbackUrl ( string url )
        {
            Logging.Debug( "Received callback" );
            // NB any user can send an arbitrary URL from the Windows Run dialog, so it must be treated as untrusted
            try
            {
                string code = codeFromCallback(url);

                var request = new HttpRequestMessage(HttpMethod.Post, AUTH_SERVER + TOKEN_URL);
                request.Content = new StringContent( $"grant_type=authorization_code&client_id={clientID}&code_verifier={verifier}&code={code}&redirect_uri={Uri.EscapeDataString( CALLBACK_URL )}", Encoding.UTF8, "application/x-www-form-urlencoded" );

                Task.Run( async () =>
                {
                    using ( var response = await httpClient.SendAsync( request ) )
                    {
                        if ( response?.StatusCode == null )
                        {
                            throw new EliteDangerousCompanionAppAuthenticationException(
                                "Failed to contact authorization server" );
                        }

                        if ( response.StatusCode == HttpStatusCode.OK )
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            var json = JObject.Parse( responseData );
                            Credentials.refreshToken = (string)json[ "refresh_token" ];
                            Credentials.accessToken = (string)json[ "access_token" ];
                            Credentials.tokenExpiry = DateTime.UtcNow.AddSeconds( (double)json[ "expires_in" ] );
                            Credentials.Save();
                            if ( Credentials.accessToken == null )
                            {
                                throw new EliteDangerousCompanionAppAuthenticationException( "Access token not found" );
                            }

                            CurrentState = State.Authorized;
                        }
                        else
                        {
                            throw new EliteDangerousCompanionAppAuthenticationException(
                                "Invalid refresh token from authorization server" );
                        }
                    }
                } ).ConfigureAwait( true );
            }
            catch ( Exception )
            {
                CurrentState = State.LoggedOut;
            }
        }

        private string codeFromCallback(string url)
        {
            if (!(url.StartsWith(CALLBACK_URL) && url.Contains("?")))
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Malformed callback URL from Frontier");
            }

            var paramsDict = ParseQueryString(url);
            if (authSessionID == null || !paramsDict.ContainsKey("state") || paramsDict["state"] != authSessionID)
            {
                throw new EliteDangerousCompanionAppAuthenticationException("Unexpected callback URL from Frontier");
            }

            if (!paramsDict.TryGetValue("code", out var callback))
            {
                if (!paramsDict.TryGetValue("error_description", out var desc))
                {
                    paramsDict.TryGetValue("error", out desc);
                }
                desc = desc ?? "no error description";
                throw new EliteDangerousCompanionAppAuthenticationException($"Negative response from Frontier: {desc}");
            }
            return callback;
        }

        private Dictionary<string, string> ParseQueryString(string url)
        {
            // Sadly System.Web.HttpUtility.ParseQueryString() is not available to us
            // https://stackoverflow.com/questions/659887/get-url-parameters-from-a-string-in-net
            var myUri = new Uri(url);
            var query = myUri.Query.TrimStart('?');
            var queryParams = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            var paramValuePairs = queryParams.Select(parameter => parameter.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries));
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

            using (var response = await httpClient.SendAsync( request ) )
            {
                if (response == null)
                {
                    Logging.Debug("Failed to contact API server");
                    throw new EliteDangerousCompanionAppException("Failed to contact API server");
                }

                if (response.StatusCode == HttpStatusCode.Found)
                {
                    return null;
                }
                return JObject.Parse(await response.Content.ReadAsStringAsync());
            }
        }

        private async Task RefreshTokenAsync ()
        {
            if ( clientID == null )
            {
                throw new EliteDangerousCompanionAppAuthenticationException( "Client ID is not configured" );
            }
            if ( Credentials.refreshToken == null )
            {
                throw new EliteDangerousCompanionAppAuthenticationException( "Refresh token not found, need full login" );
            }

            CurrentState = State.TokenRefresh;
            var request = new HttpRequestMessage(HttpMethod.Post, AUTH_SERVER + TOKEN_URL);
            request.Content = new StringContent( $"grant_type=refresh_token&client_id={clientID}&refresh_token={Credentials.refreshToken}", Encoding.UTF8, "application/x-www-form-urlencoded" );

            try
            {
                using ( var response = await httpClient.SendAsync( request ) )
                {
                    if ( response == null )
                    {
                        throw new EliteDangerousCompanionAppException( "Failed to contact API server" );
                    }
                    if ( response.StatusCode == HttpStatusCode.OK )
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(responseData);
                        Credentials.refreshToken = (string)json[ "refresh_token" ];
                        Credentials.accessToken = (string)json[ "access_token" ];
                        Credentials.tokenExpiry = DateTime.UtcNow.AddSeconds( (double)json[ "expires_in" ] );
                        Credentials.Save();
                        if ( Credentials.accessToken == null )
                        {
                            CurrentState = State.ConnectionLost;
                            CurrentState = State.LoggedOut;
                            throw new EliteDangerousCompanionAppAuthenticationException( "Access token not found" );
                        }
                        CurrentState = State.Authorized;
                    }
                    else
                    {
                        CurrentState = State.ConnectionLost;
                        CurrentState = State.LoggedOut;
                        throw new EliteDangerousCompanionAppAuthenticationException( "Invalid refresh token" );
                    }
                }
            }
            catch ( HttpRequestException ex )
            {
                Logging.Warn( ex.Message, ex );
                throw new EliteDangerousCompanionAppAuthenticationException( "Request failed", ex );
            }
        }

        /// <summary>Log out of the companion API and remove local credentials</summary>
        public void Logout()
        {
            // Remove everything other than the local email address
            Credentials = CompanionAppCredentials.Load();
            Credentials.Clear();
            Credentials.Save();
            CurrentState = State.LoggedOut;

            Logging.Debug( "Credentials cleared" );
        }

        protected internal async Task<Tuple<string, DateTime>> obtainDataAsync ( string url )
        {
            var expiry = Credentials.tokenExpiry.AddSeconds(-60);
            if ( DateTime.UtcNow > expiry )
            {
                // Our access token either has expired or shall expire within the next minute.
                // Use our refresh token to obtain a new access token.
                await RefreshTokenAsync();
            }
            if ( CurrentState != State.Authorized )
            {
                // Happens if there is a problem with the API.  Logging in again might clear this...
                CurrentState = State.ConnectionLost;
                relogin();
                if ( CurrentState != State.Authorized )
                {
                    // No luck; give up
                    Logout();
                    return null;
                }
            }

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add( "Authorization", $"Bearer {Credentials.accessToken}" );

                using ( var response = await httpClient.SendAsync( request ) )
                {
                    if ( response == null )
                    {
                        Logging.Debug( "Failed to contact API server" );
                        throw new EliteDangerousCompanionAppException( "Failed to contact API server" );
                    }
                    if ( response.StatusCode == HttpStatusCode.OK )
                    {
                        var timestamp = DateTime.Parse(response.Headers.GetValues("date").FirstOrDefault() ?? string.Empty).ToUniversalTime();
                        var responseData = await response.Content.ReadAsStringAsync();
                        return new Tuple<string, DateTime>( responseData, timestamp );
                    }
                }
            }
            catch ( HttpRequestException ex )
            {
                Logging.Warn( ex.Message, ex );
                throw new EliteDangerousCompanionAppErrorException( ex.Message, ex );
            }
            return null;
        }

        /**
         * Try to relogin if there is some issue that requires it.
         * Throws an exception if it failed to log in.
         */
        private void relogin()
        {
            // Need to log in again.
            if (clientID == null) { return; }
            Logging.Debug("Renewing login.");
            Logout();
            Login();
            if (CurrentState != State.Authorized)
            {
                Logging.Debug("Service in incorrect state to provide profile (" + CurrentState + ")");
                throw new EliteDangerousCompanionAppIllegalStateException("Service in incorrect state to provide profile (" + CurrentState + ")");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
        { 
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}
