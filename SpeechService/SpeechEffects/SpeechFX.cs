using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace EddiSpeechService.SpeechEffects
{
    public static class SpeechFx
    {
        public static IWaveProvider addEffectsToSource ( Stream stream, int targetVolume, int fxLevel,
            int distortionLevel, int echoDelay, bool radio )
        {
            float[] allSamples;
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
            }

            // Wrap in a memory‑backed provider
            ISampleProvider sampleProvider = new BufferedSampleProvider(allSamples, sampleRate);

            // Stage 1: Resample if needed
            var targetSampleRate = 44100;
            if ( sampleRate != targetSampleRate )
            {
                sampleProvider = new WdlResamplingSampleProvider( sampleProvider, targetSampleRate );
                sampleRate = targetSampleRate;
            }

            // Stage 2: Set our base volume
            sampleProvider = new VolumeSampleProvider( sampleProvider ) { Volume = 2.0f * targetSampleRate / sampleRate };

            // Stage 3: Effects
            var damageAdjustedFxLevel = DamageAdjustedFxLevel(distortionLevel, fxLevel);
            Logging.Debug( $"Effects level is {damageAdjustedFxLevel}, echo delay is {echoDelay}" );
            sampleProvider = new ChorusSampleProvider( sampleProvider, sampleRate, fxLevel, damageAdjustedFxLevel );
            sampleProvider = new ReverbSampleProvider( sampleProvider, sampleRate, fxLevel, damageAdjustedFxLevel );
            sampleProvider = new DistortionSampleProvider( sampleProvider, distortionLevel );
            sampleProvider = radio
                ? new RadioSampleProvider( sampleProvider, sampleRate )
                : new EchoSampleProvider( sampleProvider, sampleRate, fxLevel, echoDelay ) as ISampleProvider;

            // Stage 4: Normalize and Limit
            sampleProvider = new NormalizeSampleProvider( sampleProvider, targetVolume );
            sampleProvider = new LimiterSampleProvider( sampleProvider, thresholdDb: -0.5f );

            // Stage 5: Convert to 16‑bit PCM and mono
            var waveProvider = sampleProvider.ToWaveProvider16();

            return waveProvider;
        }
        
        private static int DamageAdjustedFxLevel ( decimal distortionLevel, int configFxLevel )
        {
            // Effects level can be increased, e.g. by damage if distortion is enabled
            var bonusFX = 0;
            if ( distortionLevel > 0 )
            {
                bonusFX = (int)decimal.Round( distortionLevel / 100M * ( 100M - configFxLevel ) );
            }

            return configFxLevel + bonusFX;
        }
    }
}
