using System;
using System.Linq;

namespace EddiSpeechService.SampleProviders.ChorusHelpers
{
    internal class ChorusVoice
    {
        public float BaseDelayMs { get; }
        private float DepthMs { get; }
        private float LfoHz { get; }
        public float LfoPhase { get; set; }

        private readonly float[] _buffer;
        private int _writePos;
        private readonly float _baseDelaySamples;
        private readonly float _depthSamples;
        private readonly float _dynamicDetuneMix;
        private readonly float _lfoIncrement;
        private readonly float _lfoJitterScale;
        private readonly float _norm;
        private float _metaPhase;
        private readonly float _metaInc;

        public ChorusVoice ( int sampleRate, float baseDelayMs, float depthMs, float lfoHz, float fxLevel, int voiceIndex )
        {
            // Total max delay this voice will ever read
            var totalDelayMs = Math.Min( Constants.MaxVoiceDelayMs, baseDelayMs + depthMs ) +
                               Constants.BufferHeadroomMs;
            var bufLen = Math.Max( 64, (int)Math.Ceiling( sampleRate * totalDelayMs / 1000.0 ) ) + 2;

            BaseDelayMs = baseDelayMs;
            DepthMs = depthMs;
            LfoHz = lfoHz;

            _buffer = new float[ bufLen ];
            _writePos = 0;
            LfoPhase = 0f;
            _baseDelaySamples = BaseDelayMs * sampleRate / 1000f;
            _depthSamples = DepthMs * sampleRate / 1000f;
            _lfoIncrement = Constants.TwoPi * LfoHz / sampleRate;
            _norm = fxLevel / 100;
            var u = (voiceIndex + 1) * Constants.Phi % 1f;
            _metaPhase = Constants.TwoPi * u;
            var rateScale = 1.0f + ( ( u - 0.5f ) * Constants.MetaJitterVoiceRateSpread );
            _metaInc = Constants.TwoPi * ( Constants.MetaJitterRateHz * rateScale ) / sampleRate;

            _dynamicDetuneMix = Curve.DynamicDetuneMix( fxLevel );
            _lfoJitterScale = Curve.LfoJitterScale( fxLevel );
        }

        public float Process ( float input )
        {
            if ( _norm <= 0.0f )
            {
                // True dry: no delay mod, no wet mix, no make-up, no shelf.
                return input;
            }

            _buffer[ _writePos ] = input;

            // LFO (low frequency oscillation) composition
            var ss = _norm * ( Constants.LfoNormShapePower - _norm );
            var h2 = Math.Min( Constants.LfoH2Cap,
                Constants.LfoPercent + ( Constants.LfoRamp * ss * Constants.LfoH2RampScale ) );
            var s1w = 1f - h2;
            var s1 = Math.Sin( LfoPhase );
            var s2 = Math.Sin( Constants.LfoHarmonic2 * LfoPhase );
            var h3 = Math.Min( Constants.LfoH3Cap, Constants.LfoPercent * ss * Constants.LfoH3BaseScale );
            var s3 = Math.Sin( Constants.LfoHarmonic3 * LfoPhase );
            var lfo = (float)( s1w * s1 ) + (float)( h2 * s2 ) + (float)( h3 * s3 );

            // Increase dwell near extremes (more shimmer partial build-up)
            lfo *= 1f + ( Constants.LfoSkew * lfo * lfo ); // sin³-style skew

            // Ultra-slow rate “wobble” for metallic beating — advance meta-phase ONCE
            _metaPhase += _metaInc;
            if ( _metaPhase >= Constants.TwoPi )
            {
                _metaPhase -= Constants.TwoPi;
            }

            var metaWobble = 1f + ( Constants.MetaJitterDepth * (float)Math.Sin( _metaPhase ) );

            // --- Add dynamic detune drift (per-voice independent) ---
            var dynamicDrift = (float)Math.Sin( _metaPhase * Constants.DynamicDetuneRateHz );

            // Apply ultra-slow rate “wobble” and detune drift for metallic beating
            var instInc = _lfoIncrement * metaWobble *
                          ( 1f + ( Constants.DynamicDetuneDepth * _dynamicDetuneMix * dynamicDrift ) );
            LfoPhase += instInc;

            // Final per-sample LFO increment
            if ( LfoPhase >= Constants.TwoPi )
            {
                LfoPhase -= Constants.TwoPi;
            }

            // Fractional read index
            var jitterDepth = Constants.LfoJitterDepth * _lfoJitterScale;
            var jitterSamples = jitterDepth * (float)Math.Sin( _metaPhase ) * _depthSamples;
            var delaySamples = _baseDelaySamples + ( lfo * _depthSamples ) + jitterSamples;
            var readIndex = _writePos - delaySamples;

            // Wrap safely into [0..len]
            var len = _buffer.Length;
            // handle negatives and large positives robustly
            readIndex %= len;
            if ( readIndex < 0f )
            {
                readIndex += len;
            }

            // --- 3rd-order Lagrange fractional delay ---
            // index floor
            var i1 = (int)Math.Floor( readIndex );
            var mu = readIndex - i1; // 0..1
            // sample indices: i0,i1,i2,i3 with wrap
            var i0 = ( i1 - 1 + len ) % len;
            var i2 = ( i1 + 1 ) % len;
            var i3 = ( i1 + 2 ) % len;
            var x0 = _buffer[ i0 ];
            var x1 = _buffer[ ( i1 + len ) % len ];
            var x2 = _buffer[ i2 ];
            var x3 = _buffer[ i3 ];

            // Detect wrap or aggressive modulation
            var nearWrap = i1 < 1 || i1 > (len - 3);

            var (c0, c1, c2, c3) = Lagrange3( mu );
            var delayedCubic = ( c0 * x0 ) + ( c1 * x1 ) + ( c2 * x2 ) + ( c3 * x3 );
            var delayedLinear = x1 + ( mu * ( x2 - x1 ) );
            var blend = nearWrap ? 1.0f : 0f;
            var delayed = ( blend * delayedLinear ) + ( ( 1f - blend ) * delayedCubic );

            // Advance write pointer
            _writePos++;
            if ( _writePos >= len )
            {
                _writePos = 0;
            }

            // Soft limiter for safety – reduces crackle
            var limited = SoftCeiling( delayed );

            // Still enforce absolute cap as a last resort
            if ( limited > Constants.VoiceSafetyLimiter )
            {
                limited = Constants.VoiceSafetyLimiter;
            }
            else if ( limited < -Constants.VoiceSafetyLimiter )
            {
                limited = -Constants.VoiceSafetyLimiter;
            }

            return limited;
        }

        private static (float c0, float c1, float c2, float c3) Lagrange3 ( float mu )
        {
            // 4-point, 3rd-order Lagrange interpolation: x[-1], x[0], x[+1], x[+2]
            // mu in [0,1], mu=0 -> x[0], mu=1 -> x[+1]
            var c0 = -mu * ( mu - 1f ) * ( mu - 2f ) / 6f;
            var c1 = ( mu + 1f ) * ( mu - 1f ) * ( mu - 2f ) / 2f;
            var c2 = -( mu + 1f ) * mu * ( mu - 2f ) / 2f;
            var c3 = ( mu + 1f ) * mu * ( mu - 1f ) / 6f;
            return ( c0, c1, c2, c3 );
        }

        private static float SoftCeiling ( float x, float threshold = 0.93f, float ceiling = 0.97f )
        {
            var ax = Math.Abs( x );
            if ( ax <= threshold )
            {
                return x;
            }

            float sign = Math.Sign( x );
            if ( ax >= ceiling )
            {
                return sign * ceiling;
            }

            var t = ( ax - threshold ) / ( ceiling - threshold ); // 0..1
            var smooth = t * t * ( 3f - ( 2f * t ) ); // smoothstep
            var y = threshold + ( ( ceiling - threshold ) * smooth );
            return sign * y;
        }

        public bool IsStillActive ( float threshold = 1e-4f )
        {
            return _buffer.Any( t => Math.Abs( t ) > threshold );
        }
    }
}