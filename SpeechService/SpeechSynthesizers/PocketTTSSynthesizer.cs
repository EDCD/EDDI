using EddiConfigService;
using EddiConfigService.Configurations;
using EddiSpeechService.SpeechPreparation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechService.SpeechSynthesizers
{
    /// <summary>
    /// Synthesizer that communicates with a locally running Pocket TTS server (https://github.com/kyutai-labs/pocket-tts).
    /// The server must be started externally with `pocket-tts serve` before EDDI is launched.
    /// Audio is returned as 24kHz mono 16-bit PCM WAV.
    /// </summary>
    public sealed class PocketTTSSynthesizer : IDisposable
    {
        private readonly HttpClient httpClient;
        private bool serverAvailable;

        /// <summary>The synthType identifier used for routing in SpeechManager.</summary>
        internal const string SynthTypeName = "PocketTTS";

        public PocketTTSSynthesizer ( ref HashSet<VoiceDetails> voiceStore )
        {
            httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds( 120 )
            };

            var config = ConfigService.Instance.speechServiceConfiguration;
            if ( !config.PocketTtsEnabled )
            {
                Logging.Info( "Pocket TTS is disabled in configuration." );
                return;
            }

            var serverUrl = GetServerUrl( config );
            try
            {
                var response = httpClient.GetAsync( $"{serverUrl}/health" ).GetAwaiter().GetResult();
                if ( response.IsSuccessStatusCode )
                {
                    serverAvailable = true;

                    var voiceDetails = new VoiceDetails(
                        "Pocket TTS",
                        "NotSet",
                        CultureInfo.GetCultureInfo( "en" ),
                        SynthTypeName
                    );
                    voiceStore.Add( voiceDetails );
                    Logging.Info( $"Pocket TTS server is available at {serverUrl}. Voice registered." );
                }
                else
                {
                    Logging.Warn( $"Pocket TTS server at {serverUrl} returned status {response.StatusCode}." );
                }
            }
            catch ( Exception e )
            {
                Logging.Warn( "Pocket TTS server is not reachable. Pocket TTS voices will not be available.", e );
            }
        }

        internal Stream Speak ( VoiceDetails voiceDetails, string speech, SpeechServiceConfiguration configuration )
        {
            Logging.Debug( $"Selecting {SynthTypeName} synthesizer" );

            if ( !serverAvailable )
            {
                Logging.Warn( "Pocket TTS server is not available." );
                return null;
            }

            return PocketTtsSynthesis( speech, configuration );
        }

        private MemoryStream PocketTtsSynthesis ( string speech, SpeechServiceConfiguration configuration )
        {
            if ( string.IsNullOrEmpty( speech ) )
            {
                return null;
            }

            // Strip SSML tags since Pocket TTS does not support SSML
            speech = SpeechFormatter.StripSSML( speech );
            if ( string.IsNullOrWhiteSpace( speech ) )
            {
                return null;
            }

            var serverUrl = GetServerUrl( configuration );
            var stream = new MemoryStream();
            var synthTask = Task.Run( async () =>
            {
                using ( var formContent = new MultipartFormDataContent() )
                {
                    formContent.Add( new StringContent( speech ), "text" );

                    Logging.Debug( $"Sending speech to Pocket TTS server: \"{speech}\"" );
                    var response = await httpClient
                        .PostAsync( $"{serverUrl}/tts", formContent )
                        .ConfigureAwait( false );
                    response.EnsureSuccessStatusCode();

                    await response.Content.CopyToAsync( stream ).ConfigureAwait( false );
                    stream.Position = 0;
                }
            } );

            try
            {
                Task.WaitAll( synthTask );
            }
            catch ( AggregateException ae )
            {
                foreach ( var ex in ae.InnerExceptions )
                {
                    throw ex;
                }
            }

            return stream;
        }

        private static string GetServerUrl ( SpeechServiceConfiguration config )
        {
            var url = string.IsNullOrWhiteSpace( config.PocketTtsServerUrl )
                ? "http://localhost:8000"
                : config.PocketTtsServerUrl;
            return url.TrimEnd( '/' );
        }

        public void Dispose ()
        {
            httpClient?.Dispose();
        }
    }
}
