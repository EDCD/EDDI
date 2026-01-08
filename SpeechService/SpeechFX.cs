using EddiSpeechService.SampleProviders;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace EddiSpeechService
{
    public static class SpeechFx
    {
        private const float GlobalOutputGainMultiplier = 3.00f;
        private const float LimiterThreshholdDb = -0.5f;
        private const float LimiterReleaseMs = 75f;

        public static IWaveProvider addEffectsToSource ( Stream stream, int fxLevel,
            int distortionLevel, int echoDelay, bool radio )
        {
            float[] allSamples;
            int channels;
            int sampleRate;

            // Step 1: Read all samples into memory
            using ( var reader = new WaveFileReader( stream ) )
            {
                var sp = reader.ToSampleProvider();
                sampleRate = reader.WaveFormat.SampleRate;

                var buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];
                var samples = new List<float>();
                int read;
                while ( ( read = sp.Read( buffer, 0, buffer.Length ) ) > 0 )
                {
                    for ( var i = 0; i < read; i++ )
                    {
                        samples.Add( buffer[ i ] );
                    }
                }

                allSamples = samples.ToArray();
                channels = reader.WaveFormat.Channels;
            }

            // Wrap in a memory‑backed provider
            ISampleProvider sampleProvider = new BufferedSampleProvider(allSamples, sampleRate, channels);

            // Stage 1: Resample if needed
            var targetSampleRate = 44100;
            if ( sampleRate != targetSampleRate )
            {
                sampleProvider = new WdlResamplingSampleProvider( sampleProvider, targetSampleRate );
                sampleRate = targetSampleRate;
            }

            // Stage 2: Set our base volume
            sampleProvider = new VolumeSampleProvider( sampleProvider ) { Volume = 1.0f };

            // Stage 3: Effects
            var damageAdjustedFxLevel = DamageAdjustedFxLevel(distortionLevel, fxLevel);
            Logging.Debug( $"Effects level is {damageAdjustedFxLevel}, echo delay is {echoDelay}" );
            if ( radio )
            {
                sampleProvider = new RadioSampleProvider( sampleProvider, sampleRate );
            }
            else
            {
                sampleProvider = new DistortionSampleProvider( sampleProvider, distortionLevel );
                sampleProvider = new ChorusSampleProvider( sampleProvider, sampleRate, damageAdjustedFxLevel );
                sampleProvider = new EchoSampleProvider( sampleProvider, sampleRate, fxLevel, echoDelay );
            }

            // Stage 4: Adjust from our base volume and apply a limiter
            sampleProvider = new VolumeSampleProvider( sampleProvider ) { Volume = GlobalOutputGainMultiplier };
            sampleProvider = new LimiterSampleProvider( sampleProvider, LimiterThreshholdDb, LimiterReleaseMs );

            // Stage 5: Convert to 16‑bit PCM and mono
            var waveProvider = sampleProvider.ToWaveProvider16();

            return waveProvider;
        }
        
        private static int DamageAdjustedFxLevel ( decimal distortionLevel, int configFxLevel )
        {
            // Effects level can be increased, e.g. by damage if distortion is enabled
            var bonusFx = 0;
            if ( distortionLevel > 0 )
            {
                bonusFx = (int)decimal.Round( distortionLevel / 100M * ( 100M - configFxLevel ) );
            }

            return configFxLevel + bonusFx;
        }
    }
}
