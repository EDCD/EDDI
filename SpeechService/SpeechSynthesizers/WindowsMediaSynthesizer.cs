using EddiConfigService.Configurations;
using EddiSpeechService.SpeechPreparation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Windows.Media.SpeechSynthesis;

namespace EddiSpeechService.SpeechSynthesizers
{
    [SupportedOSPlatform( "windows10.0.17763.0" )]
    public sealed class WindowsMediaSynthesizer : IDisposable
    {
        private readonly SpeechSynthesizer synth;

        private WindowsMediaSynthesizer ( SpeechSynthesizer synth )
        {
            this.synth = synth ?? throw new ArgumentNullException( nameof( synth ) );
        }

        internal string currentVoice => synth.Voice.DisplayName;

        public static async Task<WindowsMediaSynthesizer> CreateAsync (
            HashSet<VoiceDetails> voiceStore,
            CancellationToken ct = default )
        {
            SpeechSynthesizer synth;
            try
            {
                synth = new SpeechSynthesizer();
            }
            catch ( Exception ex ) when ( ex is ArgumentException || ex is InvalidOperationException ||
                                          ex is NotImplementedException || ex is COMException ||
                                          ex.GetType().FullName?.Contains( "WinRT" ) == true )
            {
                Logging.Warn(
                    $"Windows.Media.SpeechSynthesis.SpeechSynthesizer is not activatable on this system. " +
                    $"Windows Media voices will be unavailable. {RuntimeInformation.OSDescription}",
                    ex );
                return null;
            }

            var synthesizer = new WindowsMediaSynthesizer( synth );
            await synthesizer.InitializeAsync( voiceStore, ct ).ConfigureAwait( false );
            return synthesizer;
        }

        private async Task InitializeAsync ( HashSet<VoiceDetails> voiceStore, CancellationToken ct = default )
        {
            var allVoices = SpeechSynthesizer.AllVoices.ToList();
            var voices = new List<VoiceDetails>();

            foreach ( var voice in allVoices )
            {
                try
                {
                    Logging.Debug( $"Found OneCore voice: {voice.DisplayName}", voice );

                    var voiceDetails = new VoiceDetails(
                        voice.DisplayName,
                        voice.Gender.ToString(),
                        CultureInfo.GetCultureInfo( voice.Language ),
                        nameof( Windows.Media )
                    );

                    if ( !TryOneCoreVoiceRegistry( voiceDetails ) )
                    {
                        continue;
                    }

                    if ( !await TryOneCoreVoiceSpeechAsync( voiceDetails, ct ).ConfigureAwait( false ) )
                    {
                        continue;
                    }

                    voices.Add( voiceDetails );
                    Logging.Debug( $"Loaded OneCore voice: {voice.DisplayName}", voiceDetails );
                }
                catch ( Exception ex )
                {
                    Logging.Warn( $"Failed to process OneCore voice: {voice.DisplayName}", ex );
                }
            }

            foreach ( var voice in voices )
            {
                voiceStore.Add( voice );
            }
        }

        private async Task<bool> TryOneCoreVoiceSpeechAsync ( VoiceDetails voiceDetails, CancellationToken ct = default )
        {
            try
            {
                var matchingVoice = SpeechSynthesizer.AllVoices
                    .FirstOrDefault( v => v.DisplayName == voiceDetails.name );

                if ( matchingVoice == null )
                {
                    Logging.Warn( $"{voiceDetails.name} was not found in SpeechSynthesizer.AllVoices, skipping." );
                    return false;
                }

                synth.Voice = matchingVoice;

                // Use a short non-empty phrase. Empty-string synthesis is not a good validation case.
                using var stream = await synth
                    .SynthesizeTextToStreamAsync( "test" )
                    .AsTask( ct )
                    .ConfigureAwait( false );

                return stream != null;
            }
            catch ( Exception ex )
            {
                Logging.Warn( $"{voiceDetails.name} failed a OneCore speech test, skipping.", ex );
                return false;
            }
        }

        private bool TryOneCoreVoiceRegistry(VoiceDetails voiceDetails)
        {
            // Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices can pick up voices we've previously uninstalled,
            // so we test the registry entries for each voice to see if it is really fully registered.
                
            var oneCoreVoicesRegistryDir = @"SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens";
            using ( var voiceKeys = Registry.LocalMachine.OpenSubKey( oneCoreVoicesRegistryDir, false ) )
            {
                if ( voiceKeys != null )
                {
                    foreach ( var subKeyName in voiceKeys.GetSubKeyNames() )
                    {
                        using ( var voiceKey =
                               Registry.LocalMachine.OpenSubKey( $@"{oneCoreVoicesRegistryDir}\{subKeyName}" ) )
                        {
                            var voiceName = voiceKey?.GetValue( "" )?.ToString();
                            if ( voiceName?.Contains( voiceDetails.name ) ?? false )
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            Logging.Warn( $"{voiceDetails.name} is missing registry keys (may have been uninstalled?), skipping." );
            return false;
        }

        internal Stream Speak(VoiceDetails voiceDetails, string speech, SpeechServiceConfiguration Configuration)
        {
            Logging.Debug($"Selecting {nameof(Windows.Media)} synthesizer");
            return WindowsMediaSpeechSynthesis(voiceDetails, speech, Configuration)?.AsStreamForRead();
        }

        private SpeechSynthesisStream WindowsMediaSpeechSynthesis(VoiceDetails voice, string speech, SpeechServiceConfiguration Configuration)
        {
            if (voice is null || speech is null) { return null; }

            // Speak using the Windows.Media.SpeechSynthesis speech synthesizer. 
            SpeechSynthesisStream stream = null;
            var synthTask = Task.Run( async () =>
            {
                static double ConvertSpeakingRate(int rate)
                {
                    // Convert from rate from -10 to 10 (with 0 being normal speed) to rate from 0.5X to 3X (with 1.0 being normal speed)
                    var result = 1.0;
                    if (rate < 0)
                    {
                        result += rate * 0.05;
                    }
                    else if (rate > 0)
                    {
                        result += rate * 0.2;
                    }

                    return result;
                }

                if (!voice.name.Equals(synth.Voice.DisplayName))
                {
                    Logging.Debug("Selecting voice " + voice.name);
                    synth.Voice =
                        SpeechSynthesizer.AllVoices.FirstOrDefault(v =>
                            v.DisplayName == voice.name);
                }

                synth.Options.SpeakingRate = ConvertSpeakingRate(Configuration.Rate);
                synth.Options.AudioVolume = (double)Configuration.Volume / 100;         // Volume is on a 0 - 1 scale
                Logging.Debug("Configuration is: ", Configuration);

                SpeechFormatter.PrepareSpeech(voice, ref speech, out var useSSML);
                if (useSSML)
                {
                    try
                    {
                        Logging.Debug("Feeding SSML to synthesizer: " + speech);
                        stream = await synth.SynthesizeSsmlToStreamAsync(speech);
                    }
                    catch (Exception ex)
                    {
                        var badSpeech = new Dictionary<string, object>
                        {
                            { "voice", voice },
                            { "speech", speech },
                            { "exception", ex }
                        };
                        if ( speech.Contains("<phoneme") )
                        {
                            Logging.Warn("Speech failed. Stripping IPA tags and re-trying.", badSpeech);
                            stream = await synth.SynthesizeSsmlToStreamAsync(SpeechFormatter.DisableIPA(speech));
                        }
                        else
                        {
                            Logging.Warn("Speech failed. Stripping all SSML tags and re-trying.", badSpeech);
                            speech = SpeechFormatter.StripSSML( speech );
                            stream = await synth.SynthesizeTextToStreamAsync(speech);
                        }
                    }
                }
                else
                {
                    Logging.Debug("Feeding normal text to synthesizer: " + speech);
                    stream = await synth.SynthesizeTextToStreamAsync(speech);
                }

                stream.Seek(0);
            });

            try
            {
                synthTask.Wait();
            }
            catch (AggregateException ae)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ae.InnerExceptions[0]).Throw();
            }

            return stream;
        }

        public void Dispose()
        {
            synth?.Dispose();
        }
    }
}
