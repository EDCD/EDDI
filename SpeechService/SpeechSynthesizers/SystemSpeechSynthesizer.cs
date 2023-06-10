using EddiSpeechService.SpeechPreparation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using Utilities;

namespace EddiSpeechService.SpeechSynthesizers
{
    public sealed class SystemSpeechSynthesizer : IDisposable
    {
        private readonly SpeechSynthesizer synth = new SpeechSynthesizer();

        private static readonly object synthLock = new object();

        internal string currentVoice 
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
            lock ( synthLock )
            {
                bool TrySystemVoice ( VoiceDetails voiceDetails )
                {
                    // System.Speech.Synthesis.SpeechSynthesizer.GetInstalledVoices() can pick up voices which
                    // cannot be selected successfully (e.g. if the user has modified their registry).
                    // Test each voice to make sure it is selectable.
                    try
                    {
                        synth.SelectVoice( voiceDetails.name );
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }

                var selectedVoice = synth.Voice;
                var systemSpeechVoices = synth
                    .GetInstalledVoices()
                    .Where( v => v.Enabled && 
                                 !v.VoiceInfo.Name.Contains( "Microsoft Server Speech Text to Speech Voice" ) )
                    .ToList();

                foreach ( var voice in systemSpeechVoices )
                {
                    try
                    {
                        Logging.Debug( $"Found voice: {voice.VoiceInfo.Name}", voice.VoiceInfo );

                        var voiceDetails = new VoiceDetails( voice.VoiceInfo.Name, voice.VoiceInfo.Gender.ToString(),
                            voice.VoiceInfo.Culture ?? CultureInfo.InvariantCulture, nameof(System) );

                        // Skip duplicates of voices already added from Windows.Media.SpeechSynthesis
                        // (for example, if OneCore voices have been added to System.Speech with a registry edit)
                        if ( voiceStore.Any( v => v.name == voiceDetails.name ) )
                        {
                            Logging.Debug(
                                $"{voice.VoiceInfo.Name} has already been added to the voice list, skipping." );
                            continue;
                        }

                        // Skip voices which are not selectable
                        if ( !TrySystemVoice( voiceDetails ) )
                        {
                            Logging.Debug( $"{voice.VoiceInfo.Name} is not selectable, skipping." );
                            continue;
                        }

                        // Suppress voices "Desktop" variant voices from System.Speech.Synthesis
                        // where we already have a (newer) OneCore version (without disabling manual invocation of those voices)
                        if ( voiceStore.Any( v => $"{v.name} Desktop" == voiceDetails.name ) )
                        {
                            voiceDetails.hideVoice = true;
                        }

                        // Skip Amazon Polly voices - these tend to throw various internal errors (cause unknown)
                        // and are not currently reliable, particularly in VoiceAttack.
                        if ( !string.IsNullOrEmpty( voiceDetails.name ) &&
                             voiceDetails.name.StartsWith( "Amazon Polly" ) )
                        {
                            continue;
                        }

                        voiceStore.Add( voiceDetails );
                        Logging.Debug( $"Loaded voice: {voice.VoiceInfo.Name}", voiceDetails );
                    }
                    catch ( Exception e )
                    {
                        if ( voice.VoiceInfo.Culture is null )
                        {
                            Logging.Warn( $"Failed to load {voice.VoiceInfo.Name}, voice culture is not set.", e );
                        }
                        else
                        {
                            Logging.Error( $"Failed to load {voice.VoiceInfo.Name}", e );
                        }
                    }
                }
                synth.SelectVoice( selectedVoice.Name );
            }
        }

        internal Stream Speak(VoiceDetails voiceDetails, string speech, SpeechServiceConfiguration Configuration)
        {
            Logging.Debug($"Selecting {nameof(System)} synthesizer");
            return SystemSpeechSynthesis(voiceDetails, speech, Configuration);
        }

        private MemoryStream SystemSpeechSynthesis(VoiceDetails voice, string speech,
            SpeechServiceConfiguration Configuration)
        {
            if (voice is null || speech is null)
            {
                return null;
            }

            // Speak using the system's native speech synthesizer (System.Speech.Synthesis). 
            var stream = new MemoryStream();
            var synthTask = Task.Run(() =>
            {
                lock ( synthLock )
                {
                    if ( !voice.name.Equals( synth.Voice.Name ) )
                    {
                        Logging.Debug( "Selecting voice " + voice );
                        synth.SelectVoice( voice.name );
                    }

                    synth.Rate = Configuration.Rate;
                    synth.Volume = Configuration.Volume;
                    synth.SetOutputToWaveStream( stream );

                    Logging.Debug( "Speech configuration is: ", Configuration );
                    SpeechFormatter.PrepareSpeech( voice, ref speech, out var useSSML );
                    if ( useSSML )
                    {
                        try
                        {
                            Logging.Debug( "Feeding SSML to synthesizer: " + speech );
                            synth.SpeakSsml( speech );
                        }
                        catch ( Exception ex )
                        {
                            var badSpeech = new Dictionary<string, object>
                            {
                                { "voice", voice }, { "speech", speech }, { "exception", ex }
                            };
                            Logging.Warn( "Speech failed. Stripping IPA tags and re-trying.", badSpeech );
                            synth.SpeakSsml( SpeechFormatter.DisableIPA( speech ) );
                        }
                    }
                    else
                    {
                        Logging.Debug( "Feeding normal text to synthesizer: " + speech );
                        synth.Speak( speech );
                    }
                }

                stream.Position = 0;
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
