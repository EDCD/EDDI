using EddiSpeechService.SampleProviders;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using Utilities;

namespace EddiSpeechService
{
    public static class SpeechFx
    {
        private const float GlobalOutputGainMultiplier = 1.463f;
        private const float LimiterThreshholdDb = -0.5f;
        private const float LimiterReleaseMs = 75f;
        private static readonly float[] NormalizedDbSplineX =
        {
            0f, 10f, 20f, 25f, 28f, 30f, 32f, 35f, 37f, 40f, 42f, 45f, 50f, 60f, 70f, 80f, 90f, 100f
        };

        private static readonly float[] NormalizedDbSplineY =
        {
            0f, 0.183f, 0.447f, -0.13f, 0.043f, -1.379f, -1.042f, -0.382f, -0.529f, -0.422f,
            0.009f, -0.442f, -1.639f, -1.044f, -0.657f, -0.986f, -0.993f, -1.018f
        };

        // Optional debugging outputs
        private static bool TapsEnabled => Environment.GetEnvironmentVariable( "EnableSpeechFxTaps" ) != null;
        private static string TapRootDir =>
            Environment.GetEnvironmentVariable( "EDDI_SPEECH_TAPS_DIR" )
            ?? Path.Combine( Path.GetTempPath(), "EddiSpeechTaps" );
        private static string MakeTapPath ( string sessionId, int fxLevel, int damageAdjustedFxLevel, string stage, string ext = "wav" )
        {
            var folder = Path.Combine(TapRootDir, $"{sessionId}_fx{fxLevel}_adj{damageAdjustedFxLevel}");
            return Path.Combine( folder, $"{stage}.{ext}" );
        }

        public static IWaveProvider addEffectsToSource ( Stream stream, int fxLevel,
            int distortionLevel, int echoDelay, bool radio )
        {
            float[] allSamples;
            int channels;
            int sampleRate;
            fxLevel = SpeechFxFunctions.Clamp( fxLevel, 0, 100 );
            var tapSessionId = TapsEnabled ? DateTime.Now.ToString( "yyyyMMdd_HHmmss_fff" ) : null;

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

            // Stage 2: Effects
            var damageAdjustedFxLevel = DamageAdjustedFxLevel(distortionLevel, fxLevel);
            var GlobalGainDb = PostFxNormalizeDb(fxLevel);
            Logging.Debug( $"FxLevel: {fxLevel}, DamageAdjustedFxLevel: {damageAdjustedFxLevel}, EchoDelay: {echoDelay}, GlobalGainDB: {GlobalGainDb}, GlobalOutputGainMultiplier: {GlobalOutputGainMultiplier}" );

            if ( radio )
            {
                sampleProvider = new RadioSampleProvider( sampleProvider, sampleRate );
            }
            else
            {
                // Apply effects
                sampleProvider = new ChorusSampleProvider( sampleProvider, sampleRate, damageAdjustedFxLevel );
                if ( TapsEnabled )
                {
                    sampleProvider = new TapSampleProvider(
                        sampleProvider,
                        MakeTapPath( tapSessionId, fxLevel, damageAdjustedFxLevel, "postChorus_preEcho" ) );
                }

                sampleProvider = new DistortionSampleProvider( sampleProvider, distortionLevel );
                sampleProvider = new EchoSampleProvider( sampleProvider, sampleRate, fxLevel, echoDelay );

                if ( TapsEnabled )
                {
                    sampleProvider = new TapSampleProvider(
                        sampleProvider,
                        MakeTapPath( tapSessionId, fxLevel, damageAdjustedFxLevel, "postEcho_preBaseline" ) );
                }

                // Apply baseline EQ
                sampleProvider = new BaselineEqSampleProvider( sampleProvider, sampleRate, fxLevel );

                if ( TapsEnabled )
                {
                    sampleProvider = new TapSampleProvider(
                        sampleProvider,
                        MakeTapPath( tapSessionId, fxLevel, damageAdjustedFxLevel, "postBaseline_preGain" ) );
                }
            }

            // Stage 3: Adjust from our base volume and apply a limiter
            sampleProvider = new VolumeSampleProvider( sampleProvider ) { Volume = SpeechFxFunctions.DecibalsToLinear( GlobalGainDb ) * GlobalOutputGainMultiplier };

            if ( TapsEnabled )
            {
                sampleProvider = new TapSampleProvider(
                    sampleProvider,
                    MakeTapPath( tapSessionId, fxLevel, damageAdjustedFxLevel, "postGain_preLimiter" ) );
            }

            sampleProvider = new LimiterSampleProvider( sampleProvider, LimiterThreshholdDb, LimiterReleaseMs );

            // Stage 4: Convert to 16‑bit PCM and mono
            var waveProvider = sampleProvider.ToWaveProvider16();

            if ( TapsEnabled )
            {
                waveProvider = new TapWaveProvider(
                    waveProvider,
                    MakeTapPath( tapSessionId, fxLevel, damageAdjustedFxLevel, "final16" ) );
            }

            return waveProvider;
        }

        private static float PostFxNormalizeDb ( float fxLevel )
        {
            return SpeechFxFunctions.SmoothSplineClamped( NormalizedDbSplineX, NormalizedDbSplineY, fxLevel );
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

    #region Debugging Outputs

    internal sealed class TapSampleProvider : ISampleProvider, IDisposable
    {
        private readonly ISampleProvider _source;
        private readonly WaveFileWriter _writer;
        private bool _disposed;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public TapSampleProvider ( ISampleProvider source, string filePath )
        {
            _source = source ?? throw new ArgumentNullException( nameof( source ) );

            var dir = Path.GetDirectoryName(filePath);
            if ( !string.IsNullOrWhiteSpace( dir ) )
            {
                Directory.CreateDirectory( dir );
            }

            _writer = new WaveFileWriter( filePath, _source.WaveFormat );
        }

        public int Read ( float[] buffer, int offset, int count )
        {
            var read = _source.Read(buffer, offset, count);
            if ( read > 0 )
            {
                _writer.WriteSamples( buffer, offset, read );
            }
            else
            {
                Dispose();
            }
            return read;
        }

        public void Dispose ()
        {
            if ( _disposed )
                return;
            _disposed = true;
            _writer?.Dispose();
        }
    }

    internal sealed class TapWaveProvider : IWaveProvider, IDisposable
    {
        private readonly IWaveProvider _source;
        private readonly WaveFileWriter _writer;
        private bool _disposed;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public TapWaveProvider ( IWaveProvider source, string filePath )
        {
            _source = source ?? throw new ArgumentNullException( nameof( source ) );

            var dir = Path.GetDirectoryName(filePath);
            if ( !string.IsNullOrWhiteSpace( dir ) )
            {
                Directory.CreateDirectory( dir );
            }

            _writer = new WaveFileWriter( filePath, _source.WaveFormat );
        }

        public int Read ( byte[] buffer, int offset, int count )
        {
            var read = _source.Read(buffer, offset, count);
            if ( read > 0 )
            {
                _writer.Write( buffer, offset, read );
            }
            else
            {
                Dispose();
            }
            return read;
        }

        public void Dispose ()
        {
            if ( _disposed )
                return;
            _disposed = true;
            _writer?.Dispose();
        }
    }

    #endregion
}
