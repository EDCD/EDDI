﻿using EddiSpeechService.SpeechPreparation;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading;
using Utilities;

namespace EddiSpeechService.SpeechSynthesizers
{
    public class SystemSpeechSynthesizer : IDisposable
    {
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();

        private static readonly object synthLock = new object();

        internal string voice 
        {
            get
            {
                lock (synthLock)
                {
                    return synth.Voice.Name;
                }
            }
        }

        public SystemSpeechSynthesizer(ref HashSet<VoiceDetails> voiceStore)
        {
            lock (synthLock)
            {
                var systemSpeechVoices = synth
                    .GetInstalledVoices()
                    .Where(v => v.Enabled &&
                                !v.VoiceInfo.Name.Contains("Microsoft Server Speech Text to Speech Voice"))
                    .ToList();
                foreach (var voice in systemSpeechVoices)
                {
                    try
                    {
                        Logging.Debug($"Found voice: ", voice.VoiceInfo);

                        var voiceDetails = new VoiceDetails(voice.VoiceInfo.Name, voice.VoiceInfo.Gender.ToString(),
                            voice.VoiceInfo.Culture ?? CultureInfo.InvariantCulture, nameof(System));

                        // Skip duplicates of voices already added from Windows.Media.SpeechSynthesis
                        // (for example, if OneCore voices have been added to System.Speech with a registry edit)
                        if (voiceStore.Any(v => v.name == voiceDetails.name))
                        {
                            continue;
                        }

                        // Suppress voices "Desktop" variant voices from System.Speech.Synthesis
                        // where we already have a (newer) OneCore version (without disabling manual invocation of those voices)
                        if (voiceStore.Any(v => v.name + " Desktop" == voiceDetails.name))
                        {
                            voiceDetails.hideVoice = true;
                        }

                        // Skip Amazon Polly neural voices - these tend to throw an internal error (cause unknown) with the system speech synthesizer and are not currently reliable.
                        if (!string.IsNullOrEmpty(voiceDetails.name) && voiceDetails.name.StartsWith("Amazon Polly") && voiceDetails.name.EndsWith("Neural"))
                        {
                            continue;
                        }

                        voiceStore.Add(voiceDetails);
                        Logging.Debug($"Loaded voice: ", voiceDetails);
                    }
                    catch (Exception e)
                    {
                        if (voice.VoiceInfo.Culture is null)
                        {
                            Logging.Warn($"Failed to load {voice.VoiceInfo.Name}, voice culture is not set.", e);
                        }
                        else
                        {
                            Logging.Error($"Failed to load {voice.VoiceInfo.Name}", e);
                        }
                    }
                }
            }
        }

        internal Stream Speak(VoiceDetails voiceDetails, string speech, SpeechServiceConfiguration Configuration)
        {
            Logging.Debug($"Selecting {nameof(System)} synthesizer");
            return SystemSpeechSynthesis(voiceDetails, speech, Configuration);
        }

        private MemoryStream SystemSpeechSynthesis(VoiceDetails voice, string speech, SpeechServiceConfiguration Configuration)
        {
            if (voice is null || speech is null) { return null; }

            // Speak using the system's native speech synthesizer (System.Speech.Synthesis). 
            var stream = new MemoryStream();
            var synthThread = new Thread(() =>
            {
                try
                {
                    lock (synthLock)
                    {
                        if (!voice.name.Equals(synth.Voice.Name))
                        {
                            Logging.Debug("Selecting voice " + voice);
                            synth.SelectVoice(voice.name);
                        }

                        synth.Rate = Configuration.Rate;
                        synth.Volume = Configuration.Volume;
                        synth.SetOutputToWaveStream(stream);
                        Logging.Debug("Speech configuration is: ", Configuration);
                        SpeechFormatter.PrepareSpeech(voice, ref speech, out var useSSML);
                        if (useSSML)
                        {
                            try
                            {
                                Logging.Debug("Feeding SSML to synthesizer: " + speech);
                                synth.SpeakSsml(speech);
                            }
                            catch (Exception ex)
                            {
                                var badSpeech = new Dictionary<string, object>
                                {
                                        {"voice", voice},
                                        {"speech", speech},
                                        {"exception", ex}
                                    };
                                Logging.Warn("Speech failed. Stripping IPA tags and re-trying.", badSpeech);
                                synth.SpeakSsml(SpeechFormatter.DisableIPA(speech));
                            }
                        }
                        else
                        {
                            Logging.Debug("Feeding normal text to synthesizer: " + speech);
                            synth.Speak(speech);
                        }
                    }

                    stream.Position = 0;
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Speech failed: ", ex);
                    var badSpeech = new Dictionary<string, object>
                    {
                            {"voice", voice},
                            {"speech", speech},
                        };
                    string badSpeechJSON = JsonConvert.SerializeObject(badSpeech);
                    Logging.Info("Speech failed", badSpeechJSON);
                }
            });
            synthThread.Start();
            synthThread.Join();
            return stream;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // another false positive from CA2213
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "<synth>k__BackingField")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (synthLock)
                {
                    synth?.Dispose();
                }
            }
        }
    }
}
