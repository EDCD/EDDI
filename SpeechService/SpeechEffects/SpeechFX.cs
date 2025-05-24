using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NWaves.Effects;
using NWaves.Signals;
using System;
using System.IO;
using Utilities;
using WaveFormat = NAudio.Wave.WaveFormat;

namespace EddiSpeechService.SpeechEffects
{
    public static class SpeechFX
    {
        private static SpeechServiceConfiguration Configuration => SpeechService.Instance.Configuration;

        public static IWaveProvider addEffectsToSource ( Stream stream, int chorusLevel, int reverbLevel, int echoDelay, int distortionLevel, bool radio )
        {
            using ( var source = new WaveFileReader( stream ) )
            {
                // Read all samples from the source
                var sampleProvider = source.ToSampleProvider();
                var sampleCount = (int)( source.Length / ( source.WaveFormat.BitsPerSample / 8 ) );
                var buffer = new float[ sampleCount ];
                var samplesRead = sampleProvider.Read( buffer, 0, buffer.Length );
                var trimmedBuffer = buffer;
                if ( samplesRead < buffer.Length )
                {
                    trimmedBuffer = new float[ samplesRead ];
                    Array.Copy( buffer, trimmedBuffer, samplesRead );
                }

                var signal = new DiscreteSignal( source.WaveFormat.SampleRate, trimmedBuffer );
                var sampleRate = source.WaveFormat.SampleRate;
                var damageAdjustedFxLevel = FxParameters.DamageAdjustedFxLevel( distortionLevel, Configuration.EffectsLevel );

                Logging.Debug( $"Effects level is {damageAdjustedFxLevel}, chorus level is {chorusLevel}, reverb level is {reverbLevel}, echo delay is {echoDelay}" );

                // Chorus - We always apply chorus effects.
                signal = ApplyChorus(signal, chorusLevel, damageAdjustedFxLevel, sampleRate);

                // Radio (highpass)
                if ( radio )
                {
                    signal = ApplyRadio(signal, sampleRate);
                }
                // We only have distortion, echo, and reverb if we're not transmitting or receiving
                else
                {
                    // Echo
                    signal = ApplyEcho( signal, echoDelay, sampleRate );

                    // Reverb
                    signal = ApplyReverb( signal, reverbLevel, damageAdjustedFxLevel, sampleRate );

                    // Distortion (apply last)
                    signal = ApplyDamageDistortion( signal, distortionLevel );
                }

                signal = new DiscreteSignal( signal.SamplingRate, NormalizeSamples( signal ) );

                // Generate the processed signal in mono
                var processedSampleProvider = new DiscreteSignalSampleProvider( signal ).ToMono();

                // Resample to 44100Hz
                if ( processedSampleProvider.WaveFormat.SampleRate != 44100 )
                {
                    processedSampleProvider = new WdlResamplingSampleProvider( processedSampleProvider, 44100 );
                }

                // Convert to 16-bit PCM (typical format for standard audio)
                IWaveProvider waveProvider = new SampleToWaveProvider16( processedSampleProvider );

                if ( chorusLevel != 0 || reverbLevel != 0 || echoDelay != 0 )
                {
                    var extMs = Convert.ToInt32( 500 + Math.Max( 0, ( damageAdjustedFxLevel - 50 ) * 10 ) );
                    Logging.Debug( "Extending duration by " + extMs + "ms" );
                    waveProvider = new ExtendedDurationWaveProvider( waveProvider, extMs );
                }

                return waveProvider;
            }
        }

        private static DiscreteSignal ApplyChorus ( DiscreteSignal signal, int chorusLevel, int damageAdjustedFxLevel, int sampleRate )
        {
            if ( chorusLevel != 0 )
            {
                var rate = Math.Max( 0.1f, chorusLevel / 20f ); // e.g., 1–5 Hz Frequency scaling
                var width = Math.Max(0.005f, damageAdjustedFxLevel / 10f * .001f); // 5-10 ms delay

                var chorus = new ChorusEffect( sampleRate, new[] { rate }, new[] { width } );
                chorus.WetDryMix( damageAdjustedFxLevel / 100f );
                signal = chorus.ApplyTo( signal );
            }

            return signal;
        }

        private static DiscreteSignal ApplyDamageDistortion ( DiscreteSignal signal, int distortionLevel )
        {
            if ( distortionLevel != 0 )
            {
                var inputGain = distortionLevel / 100f * 30;
                var outputGain = distortionLevel / 100f * -25;
                var distortion = new DistortionEffect( DistortionMode.HardClipping, inputGain, outputGain );
                distortion.WetDryMix( 0.9f );
                signal = distortion.ApplyTo( signal );
            }

            return signal;
        }

        private static DiscreteSignal ApplyEcho ( DiscreteSignal signal, int echoDelayMs, int sampleRate )
        {
            if ( echoDelayMs != 0 )
            {
                // Define the echo sample rate and delay in seconds
                var delaySampleRate = Convert.ToInt32( sampleRate * 0.15 );
                var echo = new EchoEffect( delaySampleRate, echoDelayMs / 1000.0f );
                // The "wetDryMix" mix is the percent of added echo, with 0 indicating no added echo.
                echo.WetDryMix( 0.5f );
                signal = echo.ApplyTo( signal );
            }

            return signal;
        }

        private static DiscreteSignal ApplyRadio (DiscreteSignal signal, int sampleRate)
        {
            var highpass = BiQuadFilter.HighPassFilter( sampleRate, 1015, 1 );
            for ( var i = 0; i < ( signal.Samples.Length - 1 ); i++ )
            {
                signal.Samples[ i ] = highpass.Transform( signal.Samples[ i ] );
            }

            var inputGain = 15;
            var outputGain = -10;
            var distortion = new DistortionEffect( DistortionMode.HardClipping, inputGain, outputGain );
            distortion.WetDryMix( 0.9f );
            signal = distortion.ApplyTo( signal );

            return signal;
        }

        private static DiscreteSignal ApplyReverb ( DiscreteSignal signal, int reverbLevel, int damageAdjustedFxLevel, int sampleRate )
        {
            if ( reverbLevel != 0 )
            {
                // Map reverbLevel to number of echoes and decay
                var numEchoes = 1 + ( reverbLevel / 50 ); // 1 to 3 echoes
                var baseDecay = 0.1f + ( 0.2f * ( reverbLevel / 100f ) ); // 0.1 to 0.3
                var baseDelayMs = 30 + (int)( 120 * ( reverbLevel / 100f ) ); // 30ms to 150ms

                for ( var i = 0; i < numEchoes; i++ )
                {
                    var delayMs = baseDelayMs + ( i * 20 ); // spread echoes
                    var decay = baseDecay * (float)Math.Pow( 0.3, i ); // each echo decays more
                    var delaySamples = (int)( sampleRate * ( delayMs / 1000.0 ) );
                    var echo = new EchoEffect( delaySamples, decay );
                    echo.WetDryMix( damageAdjustedFxLevel / 100f );
                    signal = echo.ApplyTo( signal );
                }
            }

            return signal;
        }

        private static float[] NormalizeSamples ( DiscreteSignal signal )
        {
            var samples = signal.Samples;

            // Normalize to target peak prior to applying volume scaling
            var max = 0f;
            foreach (var t in samples)
            {
                var abs = Math.Abs(t);
                if ( abs > max )
                    max = abs;
            }

            if ( max > 0 )
            {
                var target = 0.95f;
                var norm = target / max;
                for ( var i = 0; i < samples.Length; i++ )
                {
                    samples[ i ] *= norm;
                }
            }

            var volumeScale = Math.Max(0, Math.Min(100, Configuration.Volume)) / 100f;
            for ( var i = 0; i < samples.Length; i++ )
            {
                // Apply configuration volume scaling (percent 0-100)
                samples[ i ] *= volumeScale;

                // Remove any out-of-range samples
                if ( float.IsNaN( samples[ i ] ) || float.IsInfinity( samples[ i ] ) )
                {
                    samples[ i ] = 0f;
                }

                // Clamp after normalization
                samples[ i ] = Math.Max( -1.0f, Math.Min( 1.0f, samples[ i ] ) );
            }

            return samples;
        }

        private class DiscreteSignalSampleProvider : ISampleProvider
        {
            private readonly float[] samples;
            private int position;
            public WaveFormat WaveFormat { get; }

            public DiscreteSignalSampleProvider ( DiscreteSignal signal )
            {
                samples = signal.Samples;
                position = 0;
                WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat( signal.SamplingRate, 1 );
            }

            public int Read ( float[] buffer, int offset, int count )
            {
                var available = samples.Length - position;
                var toCopy = Math.Min( available, count );
                Array.Copy( samples, position, buffer, offset, toCopy );
                position += toCopy;
                return toCopy;
            }
        }
    }
}
