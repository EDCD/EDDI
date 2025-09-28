using EddiConfigService.Configurations;
using EddiSpeechService.SpeechPreparation;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Utilities;
using Windows.Media.SpeechSynthesis;

namespace EddiSpeechService.SpeechSynthesizers
{
    public sealed class WindowsMediaSynthesizer : IDisposable
    {
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();

        internal string currentVoice => synth.Voice.DisplayName;
        
        public static async Task<WindowsMediaSynthesizer> CreateAsync ( HashSet<VoiceDetails> voiceStore )
        {
            var synthesizer = new WindowsMediaSynthesizer();
            await synthesizer.InitializeAsync( voiceStore ).ConfigureAwait( false );
            return synthesizer;
        }

        private async Task InitializeAsync ( HashSet<VoiceDetails> voiceStore )
        {
            // Cache the voices we're about to examine
            var allVoices = SpeechSynthesizer.AllVoices.ToList();
            
            var voices = new ConcurrentBag<VoiceDetails>();
            await Task.WhenAll( allVoices.Select( async voice =>
            {
                try
                {
                    Logging.Debug( $"Found voice: {voice.DisplayName}", voice );
                    var voiceDetails = new VoiceDetails(
                        voice.DisplayName,
                        voice.Gender.ToString(),
                        CultureInfo.GetCultureInfo( voice.Language ),
                        nameof(Windows.Media)
                    );
                    // Skip voices which are not fully registered or fail speech tests
                    if ( !await TryOneCoreVoiceRegistryAsync( voiceDetails ).ConfigureAwait( false ) ||
                         !await TryOneCoreVoiceSpeechAsync( voiceDetails ).ConfigureAwait( false ) )
                    {
                        return;
                    }

                    voices.Add( voiceDetails );
                    Logging.Debug( $"Loaded voice: {voice.DisplayName}", voiceDetails );
                }
                catch ( Exception e )
                {
                    Logging.Error( $"Failed to process voice: {voice.DisplayName}", e );
                }
            } ) ).ConfigureAwait( false );
            
            foreach ( var voice in voices )
            {
                voiceStore.Add( voice );
            }

            return;

            Task<bool> TryOneCoreVoiceRegistryAsync ( VoiceDetails voiceDetails )
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
                                    return Task.FromResult( true );
                                }
                            }
                        }
                    }
                }

                Logging.Warn( $"{voiceDetails.name} is missing registry keys (may have been uninstalled?), skipping." );
                return Task.FromResult( false );
            }

            async Task<bool> TryOneCoreVoiceSpeechAsync ( VoiceDetails voiceDetails )
            {
                try
                {
                    synth.Voice = SpeechSynthesizer.AllVoices.FirstOrDefault( v => v.DisplayName == voiceDetails.name );
                    _ = await synth.SynthesizeTextToStreamAsync( "" ).AsTask().ConfigureAwait( false );
                    return true;
                }
                catch ( Exception e )
                {
                    Logging.Warn( $"{voiceDetails.name} failed a speech test, skipping.", e );
                    return false;
                }
            }
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
                try
                {
                    double ConvertSpeakingRate(int rate)
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
                    synth.Options.AudioVolume = (double)Configuration.Volume / 100;         // Colume is on a 0 - 1 scale
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
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
            });

            try
            {
                Task.WaitAll(synthTask);
            }
            catch (AggregateException ae)
            {
                foreach (var ex in ae.InnerExceptions)
                {
                    throw ex;
                }
            }

            return stream;
        }

        public void Dispose()
        {
            synth?.Dispose();
        }
    }
}
