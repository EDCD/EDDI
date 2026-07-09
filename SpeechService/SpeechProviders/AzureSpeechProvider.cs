using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using Microsoft.CognitiveServices.Speech;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Utilities;

namespace EddiSpeechService.SpeechProviders
{
    internal sealed class AzureSpeechProvider : IWebSpeechProvider
    {
        internal const string ProviderTypeId = "AzureSpeechServices";
        private const string ProviderDisplayName = "Azure Speech Services";
        private const string ApiKeySetting = "apiKey";
        private const string RegionSetting = "region";
        private const string LegacyApiKey = "azureApiKey";
        private const string LegacyRegion = "azureRegion";
        private const string SetupUrl = "https://github.com/EDCD/EDDI/wiki/Azure-Speech-Services";
        private const string AccountUrl = "https://portal.azure.com/";
        private readonly ConcurrentDictionary<string, Lazy<CachedAzureSynthesizer>> synthesizerCache = [];

        public string ProviderType => ProviderTypeId;
        public string DisplayName => ProviderDisplayName;

        public WebSpeechProviderDescriptor Descriptor { get; } = new(
            ProviderTypeId,
            ProviderDisplayName,
            [
                new WebSpeechProviderProfileField( RegionSetting, "Region" ),
                new WebSpeechProviderProfileField( ApiKeySetting, "API key", true )
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
                Settings = []
            };
        }

        public void MigrateLegacyConfiguration ( SpeechServiceConfiguration configuration )
        {
            if ( configuration == null )
            {
                return;
            }

            var hasApiKey = configuration.TryGetAdditionalData<string>( LegacyApiKey, out var apiKey );
            var hasRegion = configuration.TryGetAdditionalData<string>( LegacyRegion, out var region );
            if ( !hasApiKey && !hasRegion )
            {
                return;
            }

            if ( configuration.SpeechProviderConfigurations.All( profile => profile.ProviderType != ProviderTypeId ) )
            {
                var profile = CreateProfile();
                SetApiKey( profile, apiKey );
                SetRegion( profile, region );
                configuration.SpeechProviderConfigurations =
                [
                    .. configuration.SpeechProviderConfigurations,
                    profile
                ];
            }

            configuration.RemoveAdditionalData( LegacyApiKey );
            configuration.RemoveAdditionalData( LegacyRegion );
        }

        internal static string GetApiKey ( WebSpeechProvider profile ) => profile?.GetSetting( ApiKeySetting );

        internal static void SetApiKey ( WebSpeechProvider profile, string apiKey ) => profile?.SetSetting( ApiKeySetting, apiKey?.Trim() );

        internal static string GetRegion ( WebSpeechProvider profile ) => profile?.GetSetting( RegionSetting );

        internal static void SetRegion ( WebSpeechProvider profile, string region ) => profile?.SetSetting( RegionSetting, region?.Trim() );

        internal static string GetSynthesizerCacheKey ( WebSpeechProvider profile, VoiceDetails voice )
        {
            var apiKeyHash = Convert.ToHexString( SHA256.HashData( Encoding.UTF8.GetBytes( GetApiKey( profile ) ?? string.Empty ) ) );
            return string.Join(
                "|",
                ProviderTypeId,
                profile?.Id ?? string.Empty,
                GetRegion( profile ) ?? string.Empty,
                apiKeyHash,
                voice?.name ?? string.Empty );
        }

        public bool IsConfigured ( WebSpeechProvider profile )
        {
            return profile != null &&
                   profile.ProviderType == ProviderType &&
                   !string.IsNullOrWhiteSpace( GetApiKey( profile ) ) &&
                   !string.IsNullOrWhiteSpace( GetRegion( profile ) );
        }

        public async Task<IReadOnlyList<VoiceDetails>> GetVoicesAsync ( WebSpeechProvider profile, CancellationToken ct )
        {
            if ( !IsConfigured( profile ) )
            {
                return [];
            }

            try
            {
                var speechConfig = SpeechConfig.FromSubscription( GetApiKey( profile ), GetRegion( profile ) );
                using var synthesizer = new SpeechSynthesizer( speechConfig, null );
                using var result = await synthesizer.GetVoicesAsync()
                    .WaitAsync( ct )
                    .ConfigureAwait( false );

                if ( result.Reason != ResultReason.VoicesListRetrieved )
                {
                    Logging.Warn( $"Failed to retrieve Azure Speech Services voices for profile '{profile.DisplayName}'. Reason: {result.Reason}" );
                    return [];
                }

                return result.Voices
                    .Select( voice => CreateVoiceDetails( profile, voice ) )
                    .Where( voice => WebSpeechProviderFilters.IsVoiceAllowed( voice, profile.LocaleFilters ) )
                    .ToList();
            }
            catch ( OperationCanceledException )
            {
                throw;
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"Failed to query Azure Speech Services voices for profile '{profile.DisplayName}'.", ex );
                return [];
            }
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
                throw new InvalidOperationException( $"Azure Speech Services profile '{profile?.DisplayName}' is not configured." );
            }

            var cachedSynthesizer = GetCachedSynthesizer( profile, voice );

            var preparedSpeech = speech;
            SpeechFormatter.PrepareSpeech( voice, ref preparedSpeech, out var useSSML );
            var ssml = useSSML
                ? PrepareAzureSsml( preparedSpeech, voice, configuration )
                : BuildSsmlFromText( preparedSpeech, voice, configuration );

            await cachedSynthesizer.SynthesisLock.WaitAsync( ct ).ConfigureAwait( false );
            try
            {
                cachedSynthesizer.LastUsedUtc = DateTime.UtcNow;
                using var result = await cachedSynthesizer.Synthesizer.SpeakSsmlAsync( ssml )
                    .WaitAsync( ct )
                    .ConfigureAwait( false );

                if ( result.Reason == ResultReason.SynthesizingAudioCompleted )
                {
                    return new MemoryStream( result.AudioData );
                }

                if ( result.Reason == ResultReason.Canceled )
                {
                    var cancellation = SpeechSynthesisCancellationDetails.FromResult( result );
                    throw new InvalidOperationException(
                        $"Azure Speech Services synthesis was canceled. Reason: {cancellation.Reason}. Details: {cancellation.ErrorDetails}" );
                }

                throw new InvalidOperationException( $"Azure Speech Services synthesis failed. Reason: {result.Reason}" );
            }
            finally
            {
                cachedSynthesizer.SynthesisLock.Release();
            }
        }

        public async Task ValidateAsync ( WebSpeechProvider profile, CancellationToken ct )
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource( ct );
            timeoutSource.CancelAfter( TimeSpan.FromSeconds( 15 ) );
            var voices = await GetVoicesAsync( profile, timeoutSource.Token ).ConfigureAwait( false );
            if ( voices.Count == 0 )
            {
                Logging.Warn( $"Azure Speech Services profile '{profile.DisplayName}' did not return any voices. Check the key, region, and locale filters. If you have limited your speech resource to only be accessible from specific IP addresses, please verify that your public IP address has not changed." );
                throw new InvalidOperationException();
            }
        }

        private static VoiceDetails CreateVoiceDetails (
            WebSpeechProvider profile,
            VoiceInfo voice )
        {
            var culture = CultureInfo.GetCultureInfo( voice.Locale );
            var isMultilingual = voice.ShortName?.Contains( "Multilingual", StringComparison.OrdinalIgnoreCase ) ?? false;
            return new VoiceDetails(
                voice.ShortName,
                voice.Gender.ToString(),
                culture,
                ProviderTypeId,
                profile.Id,
                profile.DisplayName,
                isMultilingual,
                [ voice.Locale ],
                friendlyName: CreateFriendlyVoiceName( voice.ShortName, culture, profile.DisplayName ) );
        }

        private static string CreateFriendlyVoiceName (
            string voiceName,
            CultureInfo culture,
            string providerDisplayName )
        {
            var simpleName = voiceName ?? string.Empty;
            var lastDashIndex = voiceName?.LastIndexOf( '-' ) ?? -1;
            if ( lastDashIndex >= 0 && lastDashIndex < voiceName.Length - 1 )
            {
                simpleName = voiceName.Substring( lastDashIndex + 1 );
            }

            if ( simpleName.EndsWith( "MultilingualNeural", StringComparison.OrdinalIgnoreCase ) )
            {
                simpleName = string.Concat( simpleName.AsSpan( 0, simpleName.Length - "MultilingualNeural".Length ), " (Multilingual)" );
            }
            else if ( simpleName.EndsWith( "Neural", StringComparison.OrdinalIgnoreCase ) )
            {
                simpleName = simpleName.Substring( 0, simpleName.Length - "Neural".Length );
            }

            return $"{culture.EnglishName} {simpleName} - Neural [{providerDisplayName}]";
        }

        private static string PrepareAzureSsml (
            string preparedSpeech,
            VoiceDetails voice,
            SpeechServiceConfiguration configuration )
        {
            XDocument document;
            try
            {
                document = XDocument.Parse( preparedSpeech, LoadOptions.PreserveWhitespace );
            }
            catch ( XmlException )
            {
                return BuildSsmlFromText( preparedSpeech, voice, configuration );
            }

            if ( document.Root is not { } speak ||
                 !speak.Name.LocalName.Equals( "speak", StringComparison.InvariantCultureIgnoreCase ) )
            {
                return BuildSsmlFromText( preparedSpeech, voice, configuration );
            }

            var bodyNodes = speak.Nodes()
                .Where( node => !IsLexiconNode( node ) )
                .ToList();
            var speechNamespace = speak.GetDefaultNamespace();

            speak.RemoveNodes();
            speak.Add(
                new XElement(
                    speechNamespace + "voice",
                    new XAttribute( "name", voice.name ),
                    new XElement(
                        speechNamespace + "prosody",
                        new XAttribute( "volume", NormalizeVolume( configuration.Volume ) ),
                        new XAttribute( "rate", NormalizeRate( configuration.Rate ) ),
                        bodyNodes ) ) );

            return SerializeDocument( document );
        }

        private static bool IsLexiconNode ( XNode node )
        {
            return node is XElement element &&
                   element.Name.LocalName.Equals( "lexicon", StringComparison.InvariantCultureIgnoreCase );
        }

        private static string SerializeDocument ( XDocument document )
        {
            var body = document.ToString( SaveOptions.DisableFormatting );
            return document.Declaration == null ? body : $"{document.Declaration}{body}";
        }

        private static string BuildSsmlFromText (
            string speech,
            VoiceDetails voice,
            SpeechServiceConfiguration configuration )
        {
            var xmlEscapedSpeech = SecurityElement.Escape( speech );
            return $"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"{voice.culturecode ?? "en-US"}\">" +
                   $"<voice name=\"{voice.name}\">" +
                   $"<prosody volume=\"{NormalizeVolume( configuration.Volume )}\" rate=\"{NormalizeRate( configuration.Rate )}\">" +
                   $"{xmlEscapedSpeech}" +
                   "</prosody></voice></speak>";
        }

        private static string NormalizeRate ( int rate )
        {
            var normalizedRate = Math.Clamp( rate, -10, 10 );
            var ratePercent = normalizedRate < 0
                ? normalizedRate * 5
                : normalizedRate * 10;
            return ratePercent >= 0 ? $"+{ratePercent}%" : $"{ratePercent}%";
        }

        private static string NormalizeVolume ( int volume )
        {
            return $"{Math.Clamp( volume, 0, 100 )}";
        }

        private CachedAzureSynthesizer GetCachedSynthesizer (
            WebSpeechProvider profile,
            VoiceDetails voice )
        {
            var cacheKey = GetSynthesizerCacheKey( profile, voice );
            RemoveStaleSynthesizersForProfile( profile.Id, cacheKey );
            return synthesizerCache.GetOrAdd(
                cacheKey,
                key => new Lazy<CachedAzureSynthesizer>(
                    () => CreateCachedSynthesizer( key, profile, voice ),
                    LazyThreadSafetyMode.ExecutionAndPublication ) ).Value;
        }

        private static CachedAzureSynthesizer CreateCachedSynthesizer (
            string cacheKey,
            WebSpeechProvider profile,
            VoiceDetails voice )
        {
            var speechConfig = SpeechConfig.FromSubscription( GetApiKey( profile ), GetRegion( profile ) );
            speechConfig.SpeechSynthesisVoiceName = voice.name;
            speechConfig.SetSpeechSynthesisOutputFormat( SpeechSynthesisOutputFormat.Riff24Khz16BitMonoPcm );
            var synthesizer = new SpeechSynthesizer( speechConfig, null );
            TryPreconnect( synthesizer, profile.DisplayName );
            return new CachedAzureSynthesizer( cacheKey, synthesizer );
        }

        private void RemoveStaleSynthesizersForProfile (
            string profileId,
            string activeCacheKey )
        {
            if ( string.IsNullOrWhiteSpace( profileId ) )
            {
                return;
            }

            var profileCachePrefix = $"{ProviderTypeId}|{profileId}|";
            foreach ( var key in synthesizerCache.Keys )
            {
                if ( key == activeCacheKey ||
                     !key.StartsWith( profileCachePrefix, StringComparison.InvariantCulture ) )
                {
                    continue;
                }

                if ( synthesizerCache.TryRemove( key, out var staleSynthesizer ) &&
                     staleSynthesizer.IsValueCreated )
                {
                    staleSynthesizer.Value.Dispose();
                }
            }
        }

        private static void TryPreconnect (
            SpeechSynthesizer synthesizer,
            string profileName )
        {
            try
            {
                using var connection = Connection.FromSpeechSynthesizer( synthesizer );
                connection.Open( true );
            }
            catch ( Exception ex )
            {
                Logging.Debug( $"Unable to pre-connect Azure Speech Services profile '{profileName}'. The next synthesis request will connect on demand.", ex );
            }
        }

        public void Dispose()
        {
            foreach ( var cachedSynthesizer in synthesizerCache.Values.Where( lazy => lazy.IsValueCreated ) )
            {
                cachedSynthesizer.Value.Dispose();
            }
            synthesizerCache.Clear();
        }

        private sealed class CachedAzureSynthesizer (
            string cacheKey,
            SpeechSynthesizer synthesizer )
            : IDisposable
        {
            public string CacheKey { get; } = cacheKey;
            public SpeechSynthesizer Synthesizer { get; } = synthesizer;
            public SemaphoreSlim SynthesisLock { get; } = new( 1, 1 );
            public DateTime LastUsedUtc { get; set; } = DateTime.UtcNow;

            public void Dispose()
            {
                SynthesisLock.Dispose();
                Synthesizer.Dispose();
            }
        }
    }
}
