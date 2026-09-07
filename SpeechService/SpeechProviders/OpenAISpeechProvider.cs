using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechService.SpeechProviders
{
    internal sealed class OpenAISpeechProvider : IWebSpeechProvider
    {
        internal const string ProviderTypeId = "OpenAI";
        private const string ProviderDisplayName = "OpenAI";
        private const string ApiKeySetting = "apiKey";
        private const string BaseUrlSetting = "baseUrl";
        private const string ModelSetting = "model";
        private const string SpeedSetting = "speed";
        private const string CustomVoicesSetting = "customVoices";
        private const string SetupUrl = "https://github.com/EDCD/EDDI/wiki/OpenAI";
        private const string AccountUrl = "https://platform.openai.com/";
        private const string DefaultBaseUrl = "https://api.openai.com";
        private const string DefaultApiKeyPlaceholder = "ENTER_API_KEY";
        private const string DefaultModel = "tts-1";
        private const double DefaultSpeed = 1.0;

        private static readonly HttpClient SharedHttpClient = new();
        private readonly Func<HttpClient> createHttpClient;

        public OpenAISpeechProvider()
            : this( () => SharedHttpClient )
        { }

        internal OpenAISpeechProvider( Func<HttpClient> createHttpClient )
        {
            this.createHttpClient = createHttpClient ?? throw new ArgumentNullException( nameof( createHttpClient ) );
        }

        public string ProviderType => ProviderTypeId;
        public string DisplayName => ProviderDisplayName;

        public WebSpeechProviderDescriptor Descriptor { get; } = new(
            ProviderTypeId,
            ProviderDisplayName,
            [
                new WebSpeechProviderProfileField( ApiKeySetting, "API key" ),
                new WebSpeechProviderProfileField( BaseUrlSetting, "Base URL" ),
                new WebSpeechProviderProfileField( ModelSetting, "Model" ),
                new WebSpeechProviderProfileField( SpeedSetting, "Speed (0.25-4.0)" ),
                new WebSpeechProviderProfileField( CustomVoicesSetting, "Custom voice IDs" )
            ],
            setupUrl: SetupUrl,
            accountUrl: AccountUrl );

        public WebSpeechProvider CreateProfile()
        {
            return new WebSpeechProvider
            {
                Id = $"{ProviderTypeId}-{Guid.NewGuid():N}",
                ProviderType = ProviderTypeId,
                DisplayName = ProviderDisplayName,
                Enabled = true,
                LocaleFilters = [],
                Settings = new Dictionary<string, string>
                {
                    { ApiKeySetting, DefaultApiKeyPlaceholder },
                    { CustomVoicesSetting, "alloy" }
                }
            };
        }

        public void MigrateLegacyConfiguration ( SpeechServiceConfiguration configuration )
        { }

        internal static string GetApiKey ( WebSpeechProvider profile ) => profile?.GetSetting( ApiKeySetting );

        internal static void SetApiKey ( WebSpeechProvider profile, string apiKey ) =>
            profile?.SetSetting( ApiKeySetting, apiKey?.Trim() );

        internal static string GetBaseUrl ( WebSpeechProvider profile ) =>
            profile?.GetSetting( BaseUrlSetting )?.Trim().TrimEnd( '/' );

        internal static void SetBaseUrl ( WebSpeechProvider profile, string baseUrl ) =>
            profile?.SetSetting( BaseUrlSetting, baseUrl?.Trim() );

        internal static string GetModel ( WebSpeechProvider profile ) =>
            !string.IsNullOrWhiteSpace( profile?.GetSetting( ModelSetting ) )
                ? profile.GetSetting( ModelSetting ).Trim()
                : DefaultModel;

        internal static void SetModel ( WebSpeechProvider profile, string model ) =>
            profile?.SetSetting( ModelSetting, model?.Trim() );

        internal static double GetSpeed ( WebSpeechProvider profile )
        {
            if ( double.TryParse( profile?.GetSetting( SpeedSetting ), NumberStyles.Float, CultureInfo.InvariantCulture, out var speed ) )
            {
                return Math.Clamp( speed, 0.25, 4.0 );
            }
            return DefaultSpeed;
        }

        internal static void SetSpeed ( WebSpeechProvider profile, double speed ) =>
            profile?.SetSetting( SpeedSetting, Math.Clamp( speed, 0.25, 4.0 ).ToString( "G", CultureInfo.InvariantCulture ) );

        internal static List<string> GetCustomVoices ( WebSpeechProvider profile )
        {
            var raw = profile?.GetSetting( CustomVoicesSetting );
            if ( string.IsNullOrWhiteSpace( raw ) )
            {
                return [];
            }
            return raw.Split( ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries )
                .Where( v => !string.IsNullOrWhiteSpace( v ) )
                .ToList();
        }

        internal static void SetCustomVoices ( WebSpeechProvider profile, IEnumerable<string> voices ) =>
            profile?.SetSetting( CustomVoicesSetting, voices != null ? string.Join( ", ", voices ) : null );

        public bool IsConfigured ( WebSpeechProvider profile )
        {
            return profile != null &&
                   profile.ProviderType == ProviderType;
        }

        public Task<IReadOnlyList<VoiceDetails>> GetVoicesAsync ( WebSpeechProvider profile, CancellationToken ct )
        {
            if ( !IsConfigured( profile ) )
            {
                return Task.FromResult<IReadOnlyList<VoiceDetails>>( [] );
            }

            var voices = new List<VoiceDetails>();

            foreach ( var customVoiceId in GetCustomVoices( profile ) )
            {
                voices.Add( CreateVoiceDetails( profile, customVoiceId, "Unknown" ) );
            }

            var filtered = voices
                .Where( voice => WebSpeechProviderFilters.IsVoiceAllowed( voice, profile.LocaleFilters ) )
                .ToList();

            return Task.FromResult<IReadOnlyList<VoiceDetails>>( filtered );
        }

        public async Task<Stream> SynthesizeAsync (
            WebSpeechProvider profile,
            VoiceDetails voice,
            string speech,
            SpeechServiceConfiguration configuration,
            CancellationToken ct )
        {
            if ( !IsConfigured( profile ) )
            {
                throw new InvalidOperationException( $"OpenAI profile '{profile?.DisplayName}' is not configured." );
            }

            var preparedSpeech = speech;
            SpeechFormatter.PrepareSpeech( voice, ref preparedSpeech, out _ );
            // Strip SSML tags since OpenAI TTS expects plain text
            preparedSpeech = GeneratedRegex.SsmlTagRegex().Replace( preparedSpeech, string.Empty );

            var baseUrl = GetBaseUrl( profile ) ?? DefaultBaseUrl;
            var model = GetModel( profile );
            var speed = GetSpeed( profile );
            var voiceId = !string.IsNullOrWhiteSpace( voice.providerVoiceId ) ? voice.providerVoiceId : voice.name;

            var requestBody = new
            {
                model,
                voice = voiceId,
                input = preparedSpeech,
                response_format = "wav",
                speed
            };

            var json = JsonConvert.SerializeObject( requestBody );
            Logging.Info(
                $"OpenAI synthesis request voice='{voiceId}', model='{model}', speed={speed:0.##}, baseUrl='{baseUrl}', textLength={preparedSpeech.Length}." );

            using var request = new HttpRequestMessage( HttpMethod.Post, $"{baseUrl}/v1/audio/speech" )
            {
                Content = new StringContent( json, Encoding.UTF8, "application/json" )
            };
            var apiKey = GetApiKey( profile );
            if ( !string.IsNullOrWhiteSpace( apiKey ) &&
                 !string.Equals( apiKey, DefaultApiKeyPlaceholder, StringComparison.OrdinalIgnoreCase ) )
            {
                request.Headers.Authorization = new AuthenticationHeaderValue( "Bearer", apiKey );
            }

            var client = createHttpClient();
            using var response = await client.SendAsync( request, HttpCompletionOption.ResponseHeadersRead, ct )
                .ConfigureAwait( false );

            if ( !response.IsSuccessStatusCode )
            {
                var errorBody = await response.Content.ReadAsStringAsync( ct ).ConfigureAwait( false );
                throw new InvalidOperationException(
                    $"OpenAI TTS request failed with status {(int)response.StatusCode}: {errorBody}" );
            }

            await using var responseStream = await response.Content.ReadAsStreamAsync( ct ).ConfigureAwait( false );
            var wavStream = new MemoryStream();
            await responseStream.CopyToAsync( wavStream, ct ).ConfigureAwait( false );

            if ( wavStream.Length == 0 )
            {
                throw new InvalidOperationException( $"OpenAI returned empty audio for voice '{voiceId}'." );
            }

            var (SampleRate, BitsPerSample, Channels, Duration) = GetWaveMetadata( wavStream );
            Logging.Info(
                $"OpenAI synthesis response voice='{voiceId}', outputSampleRate={SampleRate}, outputBits={BitsPerSample}, outputChannels={Channels}, outputBytes={wavStream.Length}, outputDurationMs={Duration.TotalMilliseconds:0}." );
            wavStream.Position = 0;
            return wavStream;
        }

        public async Task ValidateAsync ( WebSpeechProvider profile, CancellationToken ct )
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource( ct );
            timeoutSource.CancelAfter( TimeSpan.FromSeconds( 15 ) );
            var voices = await GetVoicesAsync( profile, timeoutSource.Token ).ConfigureAwait( false );
            if ( voices.Count == 0 )
            {
                Logging.Warn( $"OpenAI profile '{profile.DisplayName}' did not return any voices. Check the API key and locale filters." );
                throw new InvalidOperationException();
            }
        }

        private static VoiceDetails CreateVoiceDetails ( WebSpeechProvider profile, string voiceId, string gender )
        {
            // OpenAI voices are multilingual; default to English
            var culture = CultureInfo.GetCultureInfo( "en-US" );
            var displayName = voiceId;
            var friendlyName = $"{voiceId} [{profile.DisplayName}]";
            return new VoiceDetails(
                displayName,
                gender,
                culture,
                ProviderTypeId,
                profile.Id,
                profile.DisplayName,
                isMultilingual: true,
                supportedLocales: [ "en" ],
                providerVoiceId: voiceId,
                friendlyName: friendlyName );
        }

        private static (int SampleRate, int BitsPerSample, int Channels, TimeSpan Duration) GetWaveMetadata ( MemoryStream stream )
        {
            var originalPosition = stream.Position;
            stream.Position = 0;
            using var reader = new WaveFileReader( stream );
            var metadata = (
                reader.WaveFormat.SampleRate,
                reader.WaveFormat.BitsPerSample,
                reader.WaveFormat.Channels,
                reader.TotalTime );
            stream.Position = originalPosition;
            return metadata;
        }
    }
}
