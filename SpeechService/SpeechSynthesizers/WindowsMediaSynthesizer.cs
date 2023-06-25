using EddiSpeechService.SpeechPreparation;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Schema;
using Utilities;
using Windows.Media.SpeechSynthesis;

namespace EddiSpeechService.SpeechSynthesizers
{
    public sealed class WindowsMediaSynthesizer : IDisposable
    {
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();

        private static readonly object synthLock = new object();

        internal string currentVoice
        {
            get
            {
                lock (synthLock)
                {
                    return synth.Voice.DisplayName;
                }
            }
        }

        public WindowsMediaSynthesizer (ref HashSet<VoiceDetails> voiceStore, XmlSchemaSet lexiconSchemas)
        {
            bool TryOneCoreVoiceRegistry( VoiceDetails voiceDetails )
            {
                // Windows.Media.SpeechSynthesis.SpeechSynthesizer.AllVoices can pick up voices we've previously uninstalled,
                // so we test the registry entries for each voice to see if it is really fully registered.
                var oneCoreVoicesRegistryDir = @"SOFTWARE\Microsoft\Speech_OneCore\Voices\Tokens";
                var voiceKeys = Registry.LocalMachine.OpenSubKey(oneCoreVoicesRegistryDir, false);
                if (voiceKeys != null)
                {
                    foreach (var subKeyName in voiceKeys.GetSubKeyNames())
                    {
                        var voiceKey =
                            Registry.LocalMachine.OpenSubKey($@"{oneCoreVoicesRegistryDir}\{subKeyName}");
                        var voiceName = voiceKey?.GetValue("")?.ToString();
                        if (voiceName?.Contains(voiceDetails.name) ?? false)
                        {
                            return true;
                        }
                    }
                }

                Logging.Warn( $"{voiceDetails.name} is missing registry keys (may have been uninstalled?), skipping." );
                return false;
            }

            bool TryOneCoreVoiceSpeech ( VoiceDetails voiceDetails )
            {
                // Text that the voice can render a simple text string.
                try
                {
                    lock ( synthLock )
                    {
                        synth.Voice = SpeechSynthesizer.AllVoices.FirstOrDefault( v =>
                            v.DisplayName == voiceDetails.name );
                        _ = synth.SynthesizeTextToStreamAsync( "" ).AsTask().Result;
                    }
                    return true;
                }
                catch ( Exception e )
                {
                    Logging.Warn( $"{voiceDetails.name} failed a speech test, skipping.", e );
                    return false;
                }
            }

            // Get all available voices from Windows.Media.SpeechSynthesis
            foreach (var voice in SpeechSynthesizer.AllVoices)
            {
                try
                {
                    Logging.Debug($"Found voice: {voice.DisplayName}", voice);

                    var voiceDetails = new VoiceDetails(voice.DisplayName, voice.Gender.ToString(),
                        CultureInfo.GetCultureInfo(voice.Language), nameof(Windows.Media), lexiconSchemas);

                    // Skip voices which are not fully registered
                    if (!TryOneCoreVoiceRegistry(voiceDetails) || !TryOneCoreVoiceSpeech(voiceDetails))
                    {
                        continue;
                    }

                    voiceStore.Add(voiceDetails);
                    Logging.Debug($"Loaded voice: {voice.DisplayName}", voiceDetails);
                }
                catch (Exception e)
                {
                    Logging.Error($"Failed to load {voice.DisplayName}", e);
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
            var synthTask = Task.Run(() =>
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

                    lock (synthLock)
                    {
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
                                stream = synth.SynthesizeSsmlToStreamAsync(speech).AsTask().Result;
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
                                    lock (synthLock)
                                    {
                                        stream = synth.SynthesizeSsmlToStreamAsync(SpeechFormatter.DisableIPA(speech))
                                            .AsTask()
                                            .Result;
                                    }
                                }
                                else
                                {
                                    Logging.Warn("Speech failed. Stripping all SSML tags and re-trying.", badSpeech);
                                    speech = SpeechFormatter.StripSSML( speech );
                                    stream = synth.SynthesizeTextToStreamAsync(speech).AsTask().Result;
                                }
                            }
                        }
                        else
                        {
                            Logging.Debug("Feeding normal text to synthesizer: " + speech);
                            stream = synth.SynthesizeTextToStreamAsync(speech).AsTask().Result;
                        }
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
            lock ( synthLock )
            {
                synth?.Dispose();
            }
        }
    }
}
