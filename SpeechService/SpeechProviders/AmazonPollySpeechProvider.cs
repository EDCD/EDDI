using Amazon;
using Amazon.Polly;
using Amazon.Polly.Model;
using Amazon.Runtime;
using EddiConfigService.Configurations;
using EddiDataDefinitions;
using EddiSpeechService.SpeechPreparation;
using NAudio.Wave;
using NWaves.Operations;
using NWaves.Operations.Tsm;
using NWaves.Signals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Utilities;

namespace EddiSpeechService.SpeechProviders
{
    internal sealed class AmazonPollySpeechProvider : IWebSpeechProvider
    {
        internal const string ProviderTypeId = "AmazonPolly";
        internal const string StandardEngine = "standard";
        internal const string NeuralEngine = "neural";
        internal const string Mp3OutputFormat = "mp3";
        private const string ProviderDisplayName = "Amazon Polly";
        private const string RegionSetting = "region";
        private const string AccessKeyIdSetting = "accessKeyId";
        private const string SecretAccessKeySetting = "secretAccessKey";
        private const string SetupUrl = "https://github.com/EDCD/EDDI/wiki/Amazon-Polly";
        private const string AccountUrl = "https://console.aws.amazon.com/";
        private const int Mp3SampleRate = 24000;
        private const short PcmBitsPerSample = 16;
        private const double DefaultTempoStretchFactor = 2; // Lower is faster
        private static readonly IReadOnlyList<string> Engines = [ StandardEngine, NeuralEngine ];
        private readonly Func<WebSpeechProvider, IAmazonPollyClient> createClient;
        private readonly Func<Stream, Stream> decodeAudioToWave;
        private readonly double tempoStretchFactor;

        public AmazonPollySpeechProvider()
            : this( profile => new AmazonPollySdkClient( profile ) )
        { }

        internal AmazonPollySpeechProvider (
            Func<WebSpeechProvider, IAmazonPollyClient> createClient,
            Func<Stream, Stream> decodeAudioToWave = null,
            double tempoStretchFactor = DefaultTempoStretchFactor )
        {
            this.createClient = createClient ?? throw new ArgumentNullException( nameof( createClient ) );
            this.decodeAudioToWave = decodeAudioToWave ?? DecodeMp3ToWave;
            this.tempoStretchFactor = Math.Max( 1.0, tempoStretchFactor );
        }

        public string ProviderType => ProviderTypeId;

        public string DisplayName => ProviderDisplayName;

        public WebSpeechProviderDescriptor Descriptor { get; } = new(
            ProviderTypeId,
            ProviderDisplayName,
            [
                new WebSpeechProviderProfileField( RegionSetting, "Region" ),
                new WebSpeechProviderProfileField( AccessKeyIdSetting, "Access key ID", true ),
                new WebSpeechProviderProfileField( SecretAccessKeySetting, "Secret access key", true )
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
        { }

        internal static string GetAccessKeyId ( WebSpeechProvider profile ) => profile?.GetSetting( AccessKeyIdSetting );

        internal static void SetAccessKeyId ( WebSpeechProvider profile, string accessKeyId ) =>
            profile?.SetSetting( AccessKeyIdSetting, accessKeyId?.Trim() );

        internal static string GetSecretAccessKey ( WebSpeechProvider profile ) => profile?.GetSetting( SecretAccessKeySetting );

        internal static void SetSecretAccessKey ( WebSpeechProvider profile, string secretAccessKey ) =>
            profile?.SetSetting( SecretAccessKeySetting, secretAccessKey?.Trim() );

        internal static string GetRegion ( WebSpeechProvider profile ) => profile?.GetSetting( RegionSetting );

        internal static void SetRegion ( WebSpeechProvider profile, string region ) => profile?.SetSetting( RegionSetting, region?.Trim() );

        public bool IsConfigured ( WebSpeechProvider profile )
        {
            return profile != null &&
                   profile.ProviderType == ProviderType &&
                   !string.IsNullOrWhiteSpace( GetRegion( profile ) ) &&
                   !string.IsNullOrWhiteSpace( GetAccessKeyId( profile ) ) &&
                   !string.IsNullOrWhiteSpace( GetSecretAccessKey( profile ) );
        }

        public async Task<IReadOnlyList<VoiceDetails>> GetVoicesAsync ( WebSpeechProvider profile, CancellationToken ct )
        {
            if ( !IsConfigured( profile ) )
            {
                return [];
            }

            try
            {
                using var client = createClient( profile );
                var voices = await client.DescribeVoicesAsync( ct ).ConfigureAwait( false );
                return voices
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
                Logging.Warn( $"Failed to query Amazon Polly voices for profile '{profile.DisplayName}'.", ex );
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
                throw new InvalidOperationException( $"Amazon Polly profile '{profile?.DisplayName}' is not configured." );
            }

            var voiceSelection = ParseVoiceSelection( voice );
            var languageCode = SelectSynthesisLanguageCode( profile, voice, voiceSelection.LanguageCode );
            var preparedSpeech = speech;
            SpeechFormatter.PrepareSpeech( voice, ref preparedSpeech, out _ );
            var effectiveConfiguration = configuration ?? new SpeechServiceConfiguration();
            var normalizedRate = NormalizeRate( effectiveConfiguration.Rate );
            var normalizedVolume = NormalizeVolume( effectiveConfiguration.Volume );
            var pollySpeech = PreparePollySsml( preparedSpeech, languageCode, normalizedRate, normalizedVolume );
            var request = new AmazonPollySynthesisRequest(
                voiceSelection.VoiceId,
                voiceSelection.Engine,
                pollySpeech,
                "ssml",
                Mp3OutputFormat,
                Mp3SampleRate.ToString( CultureInfo.InvariantCulture ),
                languageCode );

            Logging.Info(
                $"Amazon Polly synthesis request voice='{voice?.name}', voiceId='{request.VoiceId}', engine='{request.Engine}', language='{request.LanguageCode}', outputFormat='{request.OutputFormat}', requestedSampleRate='{request.SampleRate}', configuredRate={effectiveConfiguration.Rate}, normalizedRate='{normalizedRate}', configuredVolume={effectiveConfiguration.Volume}, normalizedVolume='{normalizedVolume}', ssmlLength={pollySpeech.Length}.");

            try
            {
                using var client = createClient( profile );
                await using var audioStream = await client.SynthesizeSpeechAsync( request, ct ).ConfigureAwait( false ) 
                    ?? throw new InvalidOperationException( $"Amazon Polly did not return audio for voice '{voice?.name}'." );

                using var encodedStream = new MemoryStream();
                await audioStream.CopyToAsync( encodedStream, ct ).ConfigureAwait( false );
                if ( encodedStream.Length == 0 )
                {
                    throw new InvalidOperationException( $"Amazon Polly returned empty audio for voice '{voice?.name}'." );
                }

                encodedStream.Position = 0;
                using var decodedStream = decodeAudioToWave( encodedStream );
                var outputStream = ApplyTempoStretch( decodedStream, tempoStretchFactor );
                var (SampleRate, BitsPerSample, Channels, Duration) = GetWaveMetadata( outputStream );
                Logging.Info(
                    $"Amazon Polly synthesis response voice='{voice?.name}', encodedBytes={encodedStream.Length}, outputFormat='{request.OutputFormat}', requestedSampleRate={request.SampleRate}, tempoStretchFactor={tempoStretchFactor:0.###}, outputSampleRate={SampleRate}, outputBits={BitsPerSample}, outputChannels={Channels}, outputBytes={outputStream.Length}, outputDurationMs={Duration.TotalMilliseconds:0}." );
                outputStream.Position = 0;
                return outputStream;
            }
            catch ( OperationCanceledException )
            {
                throw;
            }
            catch ( Exception ex ) when ( ex is not InvalidOperationException )
            {
                throw new InvalidOperationException( $"Amazon Polly synthesis failed for voice '{voice?.name}'.", ex );
            }
        }

        public async Task ValidateAsync ( WebSpeechProvider profile, CancellationToken ct )
        {
            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource( ct );
            timeoutSource.CancelAfter( TimeSpan.FromSeconds( 15 ) );
            var voices = await GetVoicesAsync( profile, timeoutSource.Token ).ConfigureAwait( false );
            if ( voices.Count == 0 )
            {
                Logging.Warn( $"Amazon Polly profile '{profile.DisplayName}' did not return any voices. Check the access key, secret access key, region, and locale filters." );
                throw new InvalidOperationException();
            }
        }

        private static VoiceDetails CreateVoiceDetails ( WebSpeechProvider profile, AmazonPollyVoice voice )
        {
            var voiceId = string.IsNullOrWhiteSpace( voice.VoiceId ) ? voice.Name : voice.VoiceId;
            var languageCodes = GetVoiceLanguageCodes( voice );
            var primaryLanguageCode = languageCodes.FirstOrDefault() ?? voice.LanguageCode;
            var culture = TryGetCulture( primaryLanguageCode );
            var displayName = CreateVoiceDisplayName( voiceId, voice.Engine );
            return new VoiceDetails(
                displayName,
                voice.Gender,
                culture,
                ProviderTypeId,
                profile.Id,
                profile.DisplayName,
                isMultilingual: languageCodes.Count > 1,
                supportedLocales: languageCodes,
                providerVoiceId: CreateVoiceKey( voiceId, voice.Engine ),
                friendlyName: CreateFriendlyVoiceName( displayName, culture, profile.DisplayName ) );
        }

        private static string CreateVoiceDisplayName ( string voiceId, string engine )
        {
            return $"{voiceId} ({ToDisplayEngine( engine )})";
        }

        private static string CreateFriendlyVoiceName (
            string displayName,
            CultureInfo culture,
            string providerDisplayName )
        {
            return $"{culture.EnglishName} {displayName} [{providerDisplayName}]";
        }

        private static string CreateVoiceKey ( string voiceId, string engine )
        {
            return $"{voiceId}:{engine}";
        }

        private static (string VoiceId, string Engine, string LanguageCode) ParseVoiceSelection ( VoiceDetails voice )
        {
            var voiceKeyParts = voice?.providerVoiceId?.Split( ':' );
            if ( voiceKeyParts?.Length == 2 )
            {
                return (voiceKeyParts[0], voiceKeyParts[1], voice?.culturecode);
            }

            var name = voice?.name ?? string.Empty;
            foreach ( var engine in Engines )
            {
                var suffix = $" ({ToDisplayEngine( engine )})";
                if ( name.EndsWith( suffix, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    return (name.Substring( 0, name.Length - suffix.Length ), engine, voice?.culturecode);
                }
            }

            return (name, StandardEngine, voice?.culturecode);
        }

        private static string SelectSynthesisLanguageCode (
            WebSpeechProvider profile,
            VoiceDetails voice,
            string defaultLanguageCode )
        {
            var filters = profile?.LocaleFilters?
                .Where( filter => !string.IsNullOrWhiteSpace( filter ) )
                .Select( filter => filter.Trim() )
                .ToList() ?? [];
            var supportedLocales = voice?.supportedLocales?.Count > 0
                ? voice.supportedLocales
                : [ defaultLanguageCode ];

            return filters
                       .Select( filter => supportedLocales.FirstOrDefault( locale => LocaleMatches( locale, filter ) ) )
                       .FirstOrDefault( locale => !string.IsNullOrWhiteSpace( locale ) ) ??
                   defaultLanguageCode;
        }

        private static bool LocaleMatches ( string locale, string filter )
        {
            if ( string.IsNullOrWhiteSpace( locale ) || string.IsNullOrWhiteSpace( filter ) )
            {
                return false;
            }

            return locale.Equals( filter, StringComparison.InvariantCultureIgnoreCase ) ||
                   locale.StartsWith( $"{filter}-", StringComparison.InvariantCultureIgnoreCase );
        }

        private static List<string> GetVoiceLanguageCodes ( AmazonPollyVoice voice )
        {
            return new[] { voice.LanguageCode }
                .Concat( voice.AdditionalLanguageCodes ?? [] )
                .Where( languageCode => !string.IsNullOrWhiteSpace( languageCode ) )
                .Distinct( StringComparer.InvariantCultureIgnoreCase )
                .ToList();
        }

        private static string ToDisplayEngine ( string engine )
        {
            return string.Equals( engine, NeuralEngine, StringComparison.InvariantCultureIgnoreCase ) ? "Neural" : "Standard";
        }

        private static bool IsSpeakDocument ( string speech )
        {
            return speech?.IndexOf( "<speak", StringComparison.InvariantCultureIgnoreCase ) >= 0;
        }

        private static string PreparePollySsml (
            string preparedSpeech,
            string languageCode,
            string rate,
            string volume )
        {
            if ( IsSpeakDocument( preparedSpeech ) )
            {
                try
                {
                    var document = XDocument.Parse( preparedSpeech, LoadOptions.PreserveWhitespace );
                    if ( document.Root is { } speak &&
                         speak.Name.LocalName.Equals( "speak", StringComparison.InvariantCultureIgnoreCase ) )
                    {
                        var speechNamespace = speak.GetDefaultNamespace();
                        var bodyNodes = speak.Nodes().ToList();
                        speak.SetAttributeValue( XNamespace.Xml + "lang", languageCode ?? "en-US" );
                        speak.RemoveNodes();
                        speak.Add(
                            new XElement(
                                speechNamespace + "prosody",
                                new XAttribute( "volume", volume ),
                                new XAttribute( "rate", rate ),
                                bodyNodes ) );
                        return SerializeDocument( document );
                    }
                }
                catch ( XmlException )
                {
                    // Fall back to escaping and wrapping the input as plain text.
                }
            }

            var xmlEscapedSpeech = SecurityElement.Escape( preparedSpeech ?? string.Empty );
            return $"<speak version=\"1.0\" xmlns=\"http://www.w3.org/2001/10/synthesis\" xml:lang=\"{languageCode ?? "en-US"}\">" +
                   $"<prosody volume=\"{volume}\" rate=\"{rate}\">" +
                   $"{xmlEscapedSpeech}" +
                   "</prosody></speak>";
        }

        private static string SerializeDocument ( XDocument document )
        {
            var body = document.ToString( SaveOptions.DisableFormatting );
            return document.Declaration == null ? body : $"{document.Declaration}{body}";
        }

        private static string NormalizeRate ( int rate )
        {
            var normalizedRate = Math.Clamp( rate, -10, 10 );
            var ratePercent = 100 + (normalizedRate * 5);
            return $"{Math.Clamp( ratePercent, 20, 200 )}%";
        }

        private static string NormalizeVolume ( int volume )
        {
            var normalizedVolume = Math.Clamp( volume, 0, 100 );
            if ( normalizedVolume == 0 )
            {
                return "silent";
            }

            var decibels = 20 * Math.Log10( normalizedVolume / 100.0 );
            var formattedDecibels = Math.Round( decibels, 1 )
                .ToString( "0.#", CultureInfo.InvariantCulture );
            return decibels >= 0
                ? $"+{formattedDecibels}dB"
                : $"{formattedDecibels}dB";
        }

        private static CultureInfo TryGetCulture ( string languageCode )
        {
            try
            {
                return CultureInfo.GetCultureInfo( languageCode );
            }
            catch ( ArgumentException )
            {
                return CultureInfo.InvariantCulture;
            }
        }

        private static MemoryStream DecodeMp3ToWave ( Stream mp3Stream )
        {
            using var reader = new Mp3FileReader( mp3Stream );
            var stream = new MemoryStream();
            using ( var writer = new WaveFileWriter( stream, new WaveFormat( reader.WaveFormat.SampleRate, PcmBitsPerSample, reader.WaveFormat.Channels ) ) )
            {
                var buffer = new float[ reader.WaveFormat.SampleRate * reader.WaveFormat.Channels ];
                var sampleProvider = reader.ToSampleProvider();
                int read;
                while ( ( read = sampleProvider.Read( buffer, 0, buffer.Length ) ) > 0 )
                {
                    writer.WriteSamples( buffer, 0, read );
                }
            }

            return new MemoryStream( stream.ToArray() );
        }

        private static MemoryStream ApplyTempoStretch ( Stream waveStream, double stretchFactor )
        {
            if ( stretchFactor <= 1.0 )
            {
                return CopyToMemoryStream( waveStream );
            }

            using var reader = new WaveFileReader( waveStream );
            if ( reader.WaveFormat.Encoding != WaveFormatEncoding.Pcm ||
                 reader.WaveFormat.BitsPerSample != PcmBitsPerSample )
            {
                return CopyToMemoryStream( waveStream );
            }

            var sampleRate = reader.WaveFormat.SampleRate;
            var channels = reader.WaveFormat.Channels;
            var samplesByChannel = ReadPcm16SamplesByChannel( reader );
            var stretchedByChannel = samplesByChannel
                .Select( samples => Operation
                    .TimeStretch( new DiscreteSignal( sampleRate, samples, true ), stretchFactor, TsmAlgorithm.Wsola )
                    .Samples )
                .ToList();
            var frameCount = stretchedByChannel.Min( samples => samples.Length );
            var outputStream = new MemoryStream();
            using ( var writer = new WaveFileWriter( outputStream, new WaveFormat( sampleRate, PcmBitsPerSample, channels ) ) )
            {
                var buffer = new byte[ channels * sizeof( short ) ];
                for ( var frame = 0; frame < frameCount; frame++ )
                {
                    for ( var channel = 0; channel < channels; channel++ )
                    {
                        var sample = Math.Clamp( stretchedByChannel[ channel ][ frame ], -1.0f, 1.0f );
                        var pcm = (short)Math.Round( sample * short.MaxValue );
                        BitConverter.GetBytes( pcm ).CopyTo( buffer, channel * sizeof( short ) );
                    }
                    writer.Write( buffer, 0, buffer.Length );
                }
            }

            return new MemoryStream( outputStream.ToArray() );
        }

        private static List<float[]> ReadPcm16SamplesByChannel ( WaveFileReader reader )
        {
            var channels = reader.WaveFormat.Channels;
            var samplesByChannel = Enumerable.Range( 0, channels )
                .Select( _ => new List<float>() )
                .ToList();
            var buffer = new byte[ reader.WaveFormat.BlockAlign * reader.WaveFormat.SampleRate ];
            int bytesRead;
            while ( ( bytesRead = reader.Read( buffer, 0, buffer.Length ) ) > 0 )
            {
                for ( var offset = 0; offset + reader.WaveFormat.BlockAlign <= bytesRead; offset += reader.WaveFormat.BlockAlign )
                {
                    for ( var channel = 0; channel < channels; channel++ )
                    {
                        var pcm = BitConverter.ToInt16( buffer, offset + ( channel * sizeof( short ) ) );
                        samplesByChannel[ channel ].Add( pcm / (float)short.MaxValue );
                    }
                }
            }

            return samplesByChannel.Select( samples => samples.ToArray() ).ToList();
        }

        private static MemoryStream CopyToMemoryStream ( Stream stream )
        {
            var copy = new MemoryStream();
            stream.Position = 0;
            stream.CopyTo( copy );
            copy.Position = 0;
            return copy;
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

    internal interface IAmazonPollyClient : IDisposable
    {
        Task<IReadOnlyList<AmazonPollyVoice>> DescribeVoicesAsync ( CancellationToken ct );

        Task<Stream> SynthesizeSpeechAsync ( AmazonPollySynthesisRequest request, CancellationToken ct );
    }

    internal sealed record AmazonPollyVoice (
        string VoiceId,
        string Name,
        string Gender,
        string LanguageCode,
        string Engine,
        IReadOnlyList<string> AdditionalLanguageCodes = null );

    internal sealed record AmazonPollySynthesisRequest (
        string VoiceId,
        string Engine,
        string Text,
        string TextType,
        string OutputFormat,
        string SampleRate,
        string LanguageCode );

    internal sealed class AmazonPollySdkClient : IAmazonPollyClient
    {
        private readonly AmazonPollyClient client;
        private readonly string region;

        public AmazonPollySdkClient ( WebSpeechProvider profile )
        {
            region = AmazonPollySpeechProvider.GetRegion( profile );
            client = new AmazonPollyClient(
                new BasicAWSCredentials(
                    AmazonPollySpeechProvider.GetAccessKeyId( profile ),
                    AmazonPollySpeechProvider.GetSecretAccessKey( profile ) ),
                RegionEndpoint.GetBySystemName( region ) );
        }

        public async Task<IReadOnlyList<AmazonPollyVoice>> DescribeVoicesAsync ( CancellationToken ct )
        {
            return await DescribeVoicesByEngineAsync(
                new[] { Engine.Standard, Engine.Neural },
                async ( engine, nextToken, cancellationToken ) => await client.DescribeVoicesAsync(
                    new DescribeVoicesRequest
                    {
                        Engine = engine,
                        IncludeAdditionalLanguageCodes = true,
                        NextToken = nextToken
                    },
                    cancellationToken ).ConfigureAwait( false ),
                region,
                ct ).ConfigureAwait( false );
        }

        public async Task<Stream> SynthesizeSpeechAsync ( AmazonPollySynthesisRequest request, CancellationToken ct )
        {
            var response = await client.SynthesizeSpeechAsync(
                new SynthesizeSpeechRequest
                {
                    Engine = Engine.FindValue( request.Engine ),
                    OutputFormat = OutputFormat.FindValue( request.OutputFormat ),
                    SampleRate = request.SampleRate,
                    Text = request.Text,
                    TextType = TextType.FindValue( request.TextType ),
                    VoiceId = VoiceId.FindValue( request.VoiceId ),
                    LanguageCode = LanguageCode.FindValue( request.LanguageCode )
                },
                ct ).ConfigureAwait( false );

            return response.AudioStream;
        }

        public void Dispose()
        {
            client.Dispose();
        }

        internal static async Task<IReadOnlyList<AmazonPollyVoice>> DescribeVoicesByEngineAsync (
            IEnumerable<Engine> engines,
            Func<Engine, string, CancellationToken, Task<DescribeVoicesResponse>> describeVoicesAsync,
            string region,
            CancellationToken ct )
        {
            var voices = new List<AmazonPollyVoice>();
            foreach ( var engine in engines )
            {
                try
                {
                    string nextToken = null;
                    do
                    {
                        var response = await describeVoicesAsync( engine, nextToken, ct ).ConfigureAwait( false );
                        var voiceVariants = response.Voices.SelectMany( voice => CreateVoiceVariants( voice, engine ) ).ToList();
                        Logging.Info(
                            $"Amazon Polly DescribeVoices region='{region}', requestedEngine='{ConstantValue( engine )}', returnedVoices={response.Voices.Count}, mappedVoices={voiceVariants.Count}, hasNextPage={!string.IsNullOrWhiteSpace( response.NextToken )}." );
                        voices.AddRange( voiceVariants );
                        nextToken = response.NextToken;
                    } while ( !string.IsNullOrWhiteSpace( nextToken ) );
                }
                catch ( OperationCanceledException )
                {
                    throw;
                }
                catch ( Exception ex )
                {
                    Logging.Warn(
                        $"Amazon Polly DescribeVoices failed for region='{region}', requestedEngine='{ConstantValue( engine )}'. Continuing with voices returned by other engines.",
                        ex );
                }
            }

            var uniqueVoices = voices
                .GroupBy( voice => (voice.VoiceId, voice.Engine), StringTupleComparer.Instance )
                .Select( group => group.First() )
                .ToList();
            Logging.Info(
                $"Amazon Polly returned {uniqueVoices.Count} voices ({uniqueVoices.Count( voice => string.Equals( voice.Engine, AmazonPollySpeechProvider.StandardEngine, StringComparison.InvariantCultureIgnoreCase ) )} standard, {uniqueVoices.Count( voice => string.Equals( voice.Engine, AmazonPollySpeechProvider.NeuralEngine, StringComparison.InvariantCultureIgnoreCase ) )} neural) for region '{region}'." );
            return uniqueVoices;
        }

        private static string ConstantValue ( object value ) => value?.ToString() ?? string.Empty;

        internal static IEnumerable<AmazonPollyVoice> CreateVoiceVariants ( Voice voice, Engine requestedEngine )
        {
            foreach ( var engine in GetSelectableEngines( voice, requestedEngine ) )
            {
                yield return new AmazonPollyVoice(
                    ConstantValue( voice.Id ),
                    voice.Name,
                    ConstantValue( voice.Gender ),
                    ConstantValue( voice.LanguageCode ),
                    ConstantValue( engine ),
                    voice.AdditionalLanguageCodes?.Select( ConstantValue ).ToList() ?? [] );
            }
        }

        private static List<string> GetSelectableEngines ( Voice voice, Engine requestedEngine )
        {
            var supportedEngines = voice.SupportedEngines?
                .Where( engine => string.Equals( engine, AmazonPollySpeechProvider.StandardEngine, StringComparison.InvariantCultureIgnoreCase ) ||
                                  string.Equals( engine, AmazonPollySpeechProvider.NeuralEngine, StringComparison.InvariantCultureIgnoreCase ) )
                .Distinct( StringComparer.InvariantCultureIgnoreCase )
                .ToList();
            return supportedEngines?.Count > 0 ? supportedEngines : [ ConstantValue( requestedEngine ) ];
        }

        private sealed class StringTupleComparer : IEqualityComparer<(string First, string Second)>
        {
            public static readonly StringTupleComparer Instance = new();

            public bool Equals ( (string First, string Second) x, (string First, string Second) y )
            {
                return string.Equals( x.First, y.First, StringComparison.InvariantCultureIgnoreCase ) &&
                       string.Equals( x.Second, y.Second, StringComparison.InvariantCultureIgnoreCase );
            }

            public int GetHashCode ( (string First, string Second) obj )
            {
                return HashCode.Combine(
                    StringComparer.InvariantCultureIgnoreCase.GetHashCode( obj.First ?? string.Empty ),
                    StringComparer.InvariantCultureIgnoreCase.GetHashCode( obj.Second ?? string.Empty ) );
            }
        }
    }
}
