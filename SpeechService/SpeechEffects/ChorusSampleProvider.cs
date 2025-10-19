using EddiSpeechService.SpeechEffects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

internal class ChorusSampleProvider : EffectSampleProvider
{
    // ===== Chorus tuning and safety bounds =====
    internal static class Constants
    {
        // Define the master backbone for effects
        public const float SigmoidCenter = 0.44f;
        public const float SigmoidSteepL = 1.20f;
        public const float SigmoidSteepR = 1.60f;

        // Wet/Dry Mixing
        public const float TrueDryThreshold = 0.02f; // below this, force dry
        public const float DryAnchorMin = 0.05f; // prevent dry loss from falling below this value
        public const float WetCurveAlpha = 1.00f; // wet curve 
        public const float DryFloorStart = 0.05f; // start of dry floor ramp
        public const float DryFloorEnd = 0.25f; // end of dry floor ramp

        // Delay spread
        public const float CenterDelayMs = 6.50f; // perceptual “body” region
        public const float StepMinMs = 1.20f; // tight ensemble at low fx
        public const float StepMaxMs = 2.20f; // max ensemble breadth at high fx

        // Depth shaping
        // Shallow (2–4 ms):  Subtle thickening, gentle detune. Warmth without obvious motion.
        // Moderate (5–8 ms): Classic chorus effect. Noticeable motion, but not extreme.
        // Deep (9–12 ms):    Warbly effect. Strong motion, metallic timbre.
        public const float DepthFloor = 0.00f; // minimum depth
        public const float DepthMax = 3.30f; // target nominal max depth at 100% before caps
        public const float DepthPosMin = 0.60f; // inner voices
        public const float DepthPosMax = 1.00f; // outer voices
        public const float MaxDepthFractionOfBase = 0.55f;
        public const float DepthEasePower = 0.75f;

        // Detune
        public const float DetuneSpanMin = 0.16f;
        public const float DetuneSpanMax = 0.28f;
        public const float DetuneEasePower = 1.20f;

        // Feedback
        public const float FeedbackMax = 0.18f;
        public const float FeedbackMin = 0.02f;
        public const float FeedbackCenter = 0.08f;
        public const float FeedbackWidth = 0.15f;
        public const float FeedbackLateStart = 0.90f;
        public const float FeedbackLateEnd = 1.00f;
        public const float FeedbackLateMinFactor = 0.93f;

        // Modulation Signal Shaping (per voice)
        public const float LfoMinHz = 0.35f; // the minimum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
        public const float LfoMaxHz = 0.90f; // the maximum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
        private const float LfoPercent1 = 0.82f; // The percent of the LFO from the first signal curve
        public const float LfoPercent2 = 1.00f - LfoPercent1; // The percent of the LFO from the second signal curve
        public const float LfoRamp = 0.65f;
        public const float OuterLfoBias = 1.05f; // outer voices have slightly faster LFO for more motion
        public const float MicroCombOffset = 0.06f; // In milliseconds
        public const float MicroCombRandMs = 0.15f; // ± randomization to avoid uniform combs
        public const float PhaseJitterRadians = 0.35f; // small random jitter around staggered phases
        public const float LfoJitterDepth = 0.005f;
        public const float LfoJitterRateHz = 0.30f;
        public const float LfoHarmonic2 = 2f;

        // Normalization
        public const float MakeUpSoftLimitDb = 3.5f;
        public static readonly float[] MakeUpGainSplineIncrements_10FX = { 0.0f, -0.4f, -0.7f, -0.9f, -0.3f, +1.1f, +1.5f, +2.2f, +3.3f, +3.0f, +3.0f };

        // Voice Shaping
        public const int VoiceCount = 6;
        public const float VoiceBaseWeight = 1.05f;
        public const float VoiceCorrelationBias = 0.85f;
        public const float VoiceOuterPhase0 = 0f;
        public const float VoiceOuterPhasePi = (float)Math.PI;

        // Body Shelf
        public const float BodyShelfCutoffHz = 140f;
        public const float BodyShelfQ = 0.70f;

        // Feedback Shelf
        public const float FeedbackShelfCutoffHz = 8000f;
        public const float FeedbackShelfGainEarlyDb = -1.5f;
        public const float FeedbackShelfGainLateDb = -3.0f;
        public const float FeedbackShelfStart = 0.15f;
        public const float FeedbackShelfEnd = 0.90f;
        public const float FeedbackLpfLow = 8000f;
        public const float FeedbackLpfHigh = 11500f;

        // Metallic Shelf
        public const float MetallicShelfHz = 2300f;
        public const float MetallicQ = 0.80f;
        public static readonly float[] MetallicSplineIncrements_10FX = { 3.0f, 1.5f, 0.0f, -0.6f, -0.3f, 0.5f, 1.8f, 3.0f, 3.8f, 4.2f, 4.6f };

        // High Shelf: Brightness and Shimmer
        public const float TiltShelfCutoffHz = 10000f;
        public const float TiltShelfGainDb = 0.6f;
        public const float TiltShelfPower = 0.68f;
        public const float TiltRampQ = 0.75f;
        public const float TiltFloorGainDb = 0.9f;
        public const float TiltPower = 0.60f;        

        public const float ShimmerShelfHz = 6500f;
        public const float ShimmerShelfQ = 0.60f;
        public static readonly float[] ShimmerSplineIncrements_10FX = { 4.00f, 4.40f, 4.25f, 5.10f, 5.20f, 4.60f, 4.00f, 4.10f, 4.30f, 4.60f, 4.80f };

        // Spline Control Points (by fx level)
        public static readonly float[] ControlPtIncrements_10FX = { 0f, 10f, 20f, 30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f };

        // Safety: absolute delay and buffer sizing
        public const float MaxVoiceDelayMs = 90.0f; // hard cap base+depth per voice
        public const float BufferHeadroomMs = 10.0f; // extra beyond (base+depth)
    }

    private static class Curve
    {
        public static float Depth ( float master )
        {
            // Ease-in power ramp, capped by Constants
            var depthTarget = LinearInterpolate( Constants.DepthFloor, Constants.DepthMax,
                EaseInPow( master, Constants.DepthEasePower ) );
            return depthTarget;
        }

        public static float DetuneSpan ( float master )
        {
            return LinearInterpolate( Constants.DetuneSpanMin, Constants.DetuneSpanMax,
                EaseInPow( master, Constants.DetuneEasePower ) );
        }

        public static float DryFloor ( float master )
        {
            var t = LinearRescale( master, Constants.DryFloorStart, Constants.DryFloorEnd );
            return LinearInterpolate( Constants.DryAnchorMin, 0f, t );
        }

        public static float Feedback ( float master )
        {
            // Early fade gives slight taper below 0.1 master; constant thereafter.
            var t = SoftStep( master, Constants.FeedbackCenter, Constants.FeedbackWidth );
            var fb = LinearInterpolate( Constants.FeedbackMin, Constants.FeedbackMax, t );
            // Taper to reduce late-range cancellation
            var late = LinearRescale( master, Constants.FeedbackLateStart, Constants.FeedbackLateEnd );
            var lateAtten = LinearInterpolate( 1.00f, Constants.FeedbackLateMinFactor, late );
            return fb * lateAtten;
        }

        public static float BodyGainDb ( float master )
        {
            float[] x = { 0f, 30f, 50f, 70f, 100f }; // fx level
            float[] y = { 0f, 0.28f, 0.45f, 0.58f, 0.55f };
            var fx = Clamp( master, 0f, 1f ) * 100f;
            return SmoothSpline( x, y, fx );
        }

        public static float FeedbackGainDb ( float master )
        {
            var shelfNorm = LinearRescale( master, Constants.FeedbackShelfStart, Constants.FeedbackShelfEnd );
            var baseCut = LinearInterpolate( Constants.FeedbackShelfGainEarlyDb, Constants.FeedbackShelfGainLateDb, shelfNorm );
            // As feedback rises, open the filter slightly to avoid "buzz" and let resonance bloom
            return baseCut * ( 1f + ( 0.15f * master ) );
        }

        public static float FeedbackLpfCutoff ( float master )
        {
            return LinearInterpolate( Constants.FeedbackLpfLow, Constants.FeedbackLpfHigh, master );
        }

        public static float MetallicGainDb ( float master )
        {
            // Earlier “metal” in the 2–4 kHz band, but cap late to avoid glare
            var fx = Clamp( master, 0f, 1f ) * 100f;
            var baseDb = SmoothSpline( Constants.ControlPtIncrements_10FX, Constants.MetallicSplineIncrements_10FX, fx );
            // Gentle resonant bias: +0.4 dB bump near 3–4 kHz equivalent
            return baseDb - ( 0.6f * master );
        }

        public static float ShimmerGainDb ( float master )
        {
            // Start shimmer sooner; strongest shaping across 30–60 fx; flatten near 100 to avoid fizziness
            var fx = Clamp( master, 0f, 1f ) * 100f;
            return SmoothSpline( Constants.ControlPtIncrements_10FX, Constants.ShimmerSplineIncrements_10FX, fx );
        }

        public static float TiltGainDb ( float master )
        {
            var shimmerAmt = (float)Math.Pow( master, Constants.TiltPower );
            var tiltAmt = (float)Math.Pow( master, Constants.TiltShelfPower );
            return ( Constants.TiltFloorGainDb * shimmerAmt ) + ( Constants.TiltShelfGainDb * tiltAmt );
        }

        public static class MakeUpGainHelper
        {
            // ---- Cached spline data ----
            private static readonly float[] X;
            private static readonly float[] Y;
            private static readonly float[] H;
            private static readonly float[] M;

            static MakeUpGainHelper ()
            {
                // Lookup Table Definitions
                var fx = Constants.ControlPtIncrements_10FX;
                var db = Constants.MakeUpGainSplineIncrements_10FX;
                var n = fx.Length;

                // Allocate arrays
                X = new float[ n ];
                Y = new float[ n ];
                H = new float[ n - 1 ];
                M = new float[ n ];

                // Copy and monotonize Y (running max)
                X[ 0 ] = fx[ 0 ];
                Y[ 0 ] = db[ 0 ];
                for ( var i = 1; i < n; i++ )
                {
                    X[ i ] = fx[ i ];
                    Y[ i ] = db[ i ];
                }

                // Compute step sizes and secant slopes
                var d = new float[ n - 1 ];
                for ( var i = 0; i < ( n - 1 ); i++ )
                {
                    H[ i ] = X[ i + 1 ] - X[ i ];
                    d[ i ] = ( Y[ i + 1 ] - Y[ i ] ) / H[ i ];
                }

                // Compute derivatives (PCHIP)
                if ( n == 2 )
                {
                    M[ 0 ] = d[ 0 ];
                    M[ 1 ] = d[ 0 ];
                    return;
                }

                for ( var i = 1; i <= ( n - 2 ); i++ )
                {
                    if ( d[ i - 1 ] == 0f || d[ i ] == 0f )
                    {
                        M[ i ] = 0f;
                    }
                    else
                    {
                        var w1 = ( 2f * H[ i ] ) + H[ i - 1 ];
                        var w2 = H[ i ] + ( 2f * H[ i - 1 ] );
                        M[ i ] = ( w1 + w2 ) / ( ( w1 / d[ i - 1 ] ) + ( w2 / d[ i ] ) );
                    }
                }

                // Endpoint slopes (Fritsch–Carlson)
                M[ 0 ] = ( ( ( ( 2f * H[ 0 ] ) + H[ 1 ] ) * d[ 0 ] ) - ( H[ 0 ] * d[ 1 ] ) ) / ( H[ 0 ] + H[ 1 ] );
                if ( M[ 0 ] < 0f || d[ 0 ] == 0f )
                {
                    M[ 0 ] = 0f;
                }
                else if ( M[ 0 ] > ( 3f * d[ 0 ] ) )
                {
                    M[ 0 ] = 3f * d[ 0 ];
                }

                M[ n - 1 ] = ( ( ( ( 2f * H[ n - 2 ] ) + H[ n - 3 ] ) * d[ n - 2 ] ) - ( H[ n - 2 ] * d[ n - 3 ] ) ) /
                             ( H[ n - 2 ] + H[ n - 3 ] );
                if ( M[ n - 1 ] < 0f || d[ n - 2 ] == 0f )
                {
                    M[ n - 1 ] = 0f;
                }
                else if ( M[ n - 1 ] > ( 3f * d[ n - 2 ] ) )
                {
                    M[ n - 1 ] = 3f * d[ n - 2 ];
                }
            }

            // ---- Public API ----
            public static float MakeUpGainDb ( float master )
            {
                var fxInput = Clamp( master, 0f, 1f ) * 100f;
                var db = EvalPchip( fxInput );

                // Soft limiter
                if ( db > Constants.MakeUpSoftLimitDb )
                {
                    db = Constants.MakeUpSoftLimitDb + ( ( db - Constants.MakeUpSoftLimitDb ) * 0.25f );
                }

                return db;
            }

            // ---- Helper Methods ----
            private static float EvalPchip ( float x )
            {
                var n = X.Length;
                if ( x <= X[ 0 ] )
                {
                    return Y[ 0 ];
                }

                if ( x >= X[ n - 1 ] )
                {
                    return Y[ n - 1 ];
                }

                // Binary search would be faster for large LUTs, but linear scan is fine here
                var i = 0;
                for ( ; i < ( n - 1 ); i++ )
                {
                    if ( x <= X[ i + 1 ] )
                    {
                        break;
                    }
                }

                var h = H[ i ];
                var t = ( x - X[ i ] ) / h;
                var t2 = t * t;
                var t3 = t2 * t;

                var y0 = Y[ i ];
                var y1 = Y[ i + 1 ];
                var m0 = M[ i ];
                var m1 = M[ i + 1 ];

                var h00 = ( 2f * t3 ) - ( 3f * t2 ) + 1f;
                var h10 = t3 - ( 2f * t2 ) + t;
                var h01 = ( -2f * t3 ) + ( 3f * t2 );
                var h11 = t3 - t2;

                return ( h00 * y0 ) + ( h * h10 * m0 ) + ( h01 * y1 ) + ( h * h11 * m1 );
            }
        }
    }

    private readonly List<(ChorusVoice voice, float weight)> _voices = new List<(ChorusVoice, float)>();

    private float _fbState;
    private readonly float _norm; // Normalized [0..1] intensity
    private readonly float _master;
    private readonly double _dryGain;
    private readonly double _wetGain;
    private readonly BiquadLowShelf _bodyShelf;
    private readonly OnePoleLowPassFilter _feedbackLpf; // low pass feedback filter
    private readonly OnePoleHighShelf _feedbackShelf; // high-shelf filter for feedback coloration
    private readonly BiquadHighShelf _metallicShelf;
    private readonly AllPassFilter _resonanceAllPass;
    private readonly BiquadHighShelf _shimmerShelf;
    private readonly BiquadHighShelf _tiltShelf;

    public ChorusSampleProvider ( ISampleProvider source, int sampleRate, int damageAdjustedFxLevel ) : base( source )
    {
        // Scaled normalization:
        _norm = damageAdjustedFxLevel / 100f;

        if ( damageAdjustedFxLevel == 0 || _norm <= Constants.TrueDryThreshold )
        {
            _norm = 0f;
            _dryGain = 1.0;
            _wetGain = 0.0;
            _voices.Clear();

            // Do not instantiate shelves when dry
            _tiltShelf = null;
            _metallicShelf = null;
            _bodyShelf = null;
            _feedbackShelf = null;

            return;
        }

        // Define the master curve we'll fit our effects around
        _master = AsymSigmoid( _norm, Constants.SigmoidCenter, Constants.SigmoidSteepL, Constants.SigmoidSteepR );

        // Define an early-onset ease-out shimmer curve
        var shimmer = 1.0f - (float)Math.Pow( 1.0f - _master, 0.6f );

        // Wet/dry mixing
        var w = Math.Pow( _master, Constants.WetCurveAlpha ); // concave/convex tweak
        _dryGain = Math.Sqrt( Math.Max( 0.0, 1.0 - ( w * w ) ) );
        _wetGain = w;

        // Low‑mid body shelf
        _bodyShelf = new BiquadLowShelf( Constants.BodyShelfCutoffHz, Curve.BodyGainDb( _master ), Constants.BodyShelfQ, sampleRate );

        // Feedback Filter
        _feedbackLpf = new OnePoleLowPassFilter();
        _feedbackLpf.Set( Curve.FeedbackLpfCutoff( _master ), sampleRate );
        
        // Feedback coloration
        _feedbackShelf = new OnePoleHighShelf( Constants.FeedbackShelfCutoffHz, Curve.FeedbackGainDb( _master ), sampleRate );

        // Metallic shimmer
        _metallicShelf = new BiquadHighShelf( Constants.MetallicShelfHz, Curve.MetallicGainDb( _master ), Constants.MetallicQ, sampleRate );

        // Resonance Filter
        var resLen = Math.Max(2, (int)(0.00020f * sampleRate)); // ~0.20 ms
        _resonanceAllPass = new AllPassFilter( resLen, 0.40f );

        // Shimmer Shelf
        _shimmerShelf = new BiquadHighShelf( Constants.ShimmerShelfHz, Curve.ShimmerGainDb( _master ), Constants.ShimmerShelfQ, sampleRate );

        // High band shelf
        _tiltShelf = new BiquadHighShelf( Constants.TiltShelfCutoffHz, Curve.TiltGainDb( shimmer ), Constants.TiltRampQ, sampleRate );

        // Hybrid delay spread: linear core + nonlinear outer bias
        var baseDelaysMs = new float[ Constants.VoiceCount ];
        var centerIdx = ( Constants.VoiceCount - 1 ) / 2f;

        // Step scales with intensity in [StepMinMs..StepMaxMs]
        var step = LinearInterpolate( Constants.StepMinMs, Constants.StepMaxMs, SmoothStep( 0.05f, 0.85f, _master ) );

        for ( var v = 0; v < Constants.VoiceCount; v++ )
        {
            // Pure linear spread around center index
            var baseDelay = Constants.CenterDelayMs + ( ( v - centerIdx ) * step );
            baseDelaysMs[ v ] = Math.Min( baseDelay, Constants.MaxVoiceDelayMs );
        }

        InitializeChorusVoices( sampleRate, Constants.VoiceCount, baseDelaysMs );
    }

    private void InitializeChorusVoices ( int sampleRate, int voiceCount, float[] baseDelaysMs )
    {
        var rnd = new Random();
        var centerIdx = ( voiceCount - 1 ) / 2f;

        for ( var v = 0; v < voiceCount; v++ )
        {
            // Detune spread ±DetuneSpan across voices
            // Force outer voices to be symmetrical for added metallic character.
            // Detune spread grows with fx: ~0.16 at low fx to ~0.28 at high fx
            var detuneSpan = Curve.DetuneSpan( _master );
            var pos = ( v - centerIdx ) / Math.Max( 1f, centerIdx ); // -1..+1
            var detune = 1.0f;
            if ( v == 0 )
            {
                detune -= detuneSpan;
            }
            else if ( v == ( voiceCount - 1 ) )
            {
                detune += detuneSpan;
            }
            else
            {
                detune += pos * detuneSpan;
            }

            // LFO frequency
            var outerBias = v == 0 || v == ( voiceCount - 1 ) ? Constants.OuterLfoBias : 1.0f;
            var freqHz = LinearScale( _norm, 0f, 1f, Constants.LfoMinHz, Constants.LfoMaxHz ) * detune * outerBias;

            // Depth scaling by position (outer is deeper)
            var posScale = Constants.DepthPosMin +
                           ( ( Constants.DepthPosMax - Constants.DepthPosMin ) * Math.Abs( pos ) );
            var depthTargetMs = Curve.Depth( _master );
            var depthMs = depthTargetMs * posScale;

            // Add tiny, fixed offsets to base delay per voice to seed consistent micro‑comb features that read as “metallic”
            float microOffsetMs;
            if ( v == 0 )
            {
                microOffsetMs = -Constants.MicroCombOffset;
            }
            else if ( v == ( voiceCount - 1 ) )
            {
                microOffsetMs = +Constants.MicroCombOffset;
            }
            else
            {
                microOffsetMs = pos > 0 ? +Constants.MicroCombOffset / 2f : -Constants.MicroCombOffset / 2f;
            }
            // inject small random perturbation to break exact symmetry
            microOffsetMs += (float)( ( (rnd.NextDouble() * 2.0) - 1.0 ) * Constants.MicroCombRandMs );
            
            var baseMs = baseDelaysMs[ v ] + microOffsetMs;

            // Cap depth against base delay fraction and absolute max voice delay
            var maxDepthByBase = baseMs * Constants.MaxDepthFractionOfBase;
            var maxDepthByAbs = Math.Max( 0f, Constants.MaxVoiceDelayMs - baseMs );
            var maxDepth = Math.Min( maxDepthByBase, maxDepthByAbs );
            depthMs = Math.Max( Math.Min( depthMs, maxDepth ), Constants.DepthFloor );

            // Construct voice (voice will allocate buffer sized to base+depth+headroom)
            var voice = new ChorusVoice( sampleRate, baseMs, depthMs, freqHz, _norm );

            // Evenly distributed phases + small jitter to decorrelate,
            // except force outer voices to be symmetrical
            // Taper jitter with FX to avoid high-FX "fuzz"
            var stagger = (float)( v * ( 2.0 * Math.PI / voiceCount ) );
            var jitter = (float)( ( ( rnd.NextDouble() * 2 ) - 1 ) * Constants.PhaseJitterRadians );
            if ( v == 0 )
            {
                voice.SetPhase( Constants.VoiceOuterPhase0 );
            }
            else if ( v == ( voiceCount - 1 ) )
            {
                voice.SetPhase( Constants.VoiceOuterPhasePi );
            }
            else
            {
                voice.SetPhase( stagger + jitter );
            }

            var weight = GetVoiceWeight( v, _master );

            _voices.Add( (voice, weight) );
        }
    }

    protected override float ProcessSample ( float input )
    {
        if ( _voices.Count == 0 || _norm < Constants.TrueDryThreshold )
        {
            // Pure dry passthrough
            return input;
        }

        // Feedback
        var feedbackAmount = Curve.Feedback( _master );
        var fbColored = _feedbackShelf.Process( _fbState );
        fbColored = _resonanceAllPass.Process( fbColored ); // diffuse before LPF/feedback injection
        var fbFiltered = _norm > 0.05f ? _feedbackLpf.Process( fbColored ) : fbColored; // Skip the filter at very low fx to save CPU
        var chorusInput = input + ( feedbackAmount * fbFiltered );

        // Add gentle low-frequency reinforcement to restore body resonance
        var bodyGain = 0.15f * (1f - _norm);      // stronger at lower fx
        var lowBoost = _bodyShelf?.Process(_fbState) ?? _fbState;
        chorusInput += bodyGain * lowBoost;
        
        // Sum all chorus voices
        var wetSum = 0f;
        var weightSum = 0f;
        foreach ( var (voice, weight) in _voices )
        {
            var wetSample = voice.Process( chorusInput );
            wetSum += weight * wetSample;
            weightSum += weight * weight;
        }

        weightSum *= Constants.VoiceCorrelationBias;

        // Update feedback state with normalized wet
        _fbState = weightSum > 0 ? wetSum / (float)Math.Sqrt( weightSum ) : 0f;

        // --- Mix with dry ---
        var dry = (float)Math.Max( Curve.DryFloor( _master ), _dryGain );
        var wetMix = weightSum > 0 ? wetSum / (float)Math.Sqrt( weightSum ) : 0f;
        wetMix *= (float)_wetGain;

        // Apply shelves
        if ( _norm > 0 )
        {
            wetMix = _shimmerShelf?.Process( wetMix ) ?? wetMix;
            wetMix = _bodyShelf?.Process( wetMix ) ?? wetMix;
            wetMix = _metallicShelf?.Process( wetMix ) ?? wetMix;
            wetMix = _tiltShelf?.Process( wetMix ) ?? wetMix;
        }

        // Apply make-up gain for energy lost in the chorus
        var makeUp = dB( Curve.MakeUpGainHelper.MakeUpGainDb( _master ) );

        var outSample = (dry * input) + (wetMix * makeUp);
        return outSample;
    }

    private class ChorusVoice
    {
        private readonly float[] _buffer;
        private int _writePos;
        private readonly float _baseDelaySamples;
        private readonly float _depthSamples;
        private readonly float _lfoIncrement;
        private float _lfoPhase;
        private readonly float _norm;

        public ChorusVoice ( int sampleRate, float baseDelayMs, float depthMs, float lfoHz, float norm )
        {
            // Total max delay this voice will ever read
            var totalDelayMs = Math.Min( Constants.MaxVoiceDelayMs, baseDelayMs + depthMs ) +
                               Constants.BufferHeadroomMs;
            var bufLen = Math.Max( 64, (int)Math.Ceiling( sampleRate * totalDelayMs / 1000.0 ) ) + 2;

            _buffer = new float[ bufLen ];
            _writePos = 0;
            _lfoPhase = 0f;
            _baseDelaySamples = baseDelayMs * sampleRate / 1000f;
            _depthSamples = depthMs * sampleRate / 1000f;
            _lfoIncrement = (float)( 2 * Math.PI * lfoHz / sampleRate );
            _norm = norm;
        }

        public float Process ( float input )
        {
            if ( _norm <= 0.0f )
            {
                // True dry: no delay mod, no wet mix, no make-up, no shelf.
                return input;
            }
            
            _buffer[ _writePos ] = input;

            // LFO
            var ss = _norm * ( 2f - _norm );
            var h2 = Math.Min( 0.22f, Constants.LfoPercent2 + ( Constants.LfoRamp * ss * 0.9f ) );
            var s1w = 1f - h2;

            var s1 = Math.Sin( _lfoPhase );
            var s2 = Math.Sin( Constants.LfoHarmonic2 * _lfoPhase );
            var lfo = (float)( s1w * s1 ) + (float)( h2 * s2 );

            // Add gentle curvature to increase the effective dwell time at extreme delay offsets, enhancing resonant partial build-up.
            lfo *= 1f + (0.15f * lfo * lfo); // sin³-style skew
            
            _lfoPhase += _lfoIncrement;
            if ( _lfoPhase >= ( 2 * Math.PI ) )
            {
                _lfoPhase -= (float)( 2 * Math.PI );
            }

            // Fractional read index
            var jitterDepth = Constants.LfoJitterDepth * (1.0f - (_norm * _norm));
            var jitter = 1.0f + ( jitterDepth * (float)Math.Sin( _lfoPhase * Constants.LfoJitterRateHz ) );
            var delaySamples = _baseDelaySamples + ( lfo * _depthSamples * jitter );
            var readIndex = _writePos - delaySamples;

            // Wrap safely into [0..len)
            var len = _buffer.Length;
            // handle negatives and large positives robustly
            readIndex %= len;
            if ( readIndex < 0 )
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
            // Lagrange coefficients
            var c0 = (-0.1666667f * mu) + (0.5f * mu * mu) - (0.3333333f * mu * mu * mu);
            var c1 = 1.0f - (1.8333333f * mu * mu) + (0.8333333f * mu * mu * mu);
            var c2 = (0.1666667f * mu) + (1.3333333f * mu * mu) - (0.8333333f * mu * mu * mu);
            var c3 = (-0.5f * mu * mu) + (0.3333333f * mu * mu * mu);
            var delayedCubic = ( c0 * x0 ) + ( c1 * x1 ) + ( c2 * x2 ) + ( c3 * x3 );
            // Linear fallback (safer, less prone to overshoot)
            var delayedLinear = x1 + (mu * (x2 - x1));
            // Detect wrap or aggressive modulation
            var nearWrap = i1 < 3 || i3 > (len - 3);
            var blend = nearWrap ? 1.0f : Math.Min(1.0f, _depthSamples / 8.0f); // stronger blend when depth large
            // Smooth transition
            var delayed = (blend * delayedLinear) + ((1f - blend) * delayedCubic);
            
            // Advance write pointer
            _writePos++;
            if ( _writePos >= len )
            {
                _writePos = 0;
            }

            // Mild limiter for safety
            if ( delayed > 1.0f )
            {
                delayed = 1.0f;
            }
            else if ( delayed < -1.0f )
            {
                delayed = -1.0f;
            }
            
            return delayed;
        }

        public void SetPhase ( float phase )
        {
            _lfoPhase = phase;
        }

        public bool IsStillActive ( float threshold = 1e-4f )
        {
            return _buffer.Any( t => Math.Abs( t ) > threshold );
        }
    }

    protected override bool EffectStillActive ()
    {
        // Check if delay buffer is still active
        return _voices.Any( v => v.voice.IsStillActive() );
    }

    private static float GetVoiceWeight ( int voiceIndex, float master )
    {
        var activation = 1.0f;
        if ( voiceIndex == 4 )
        {
            // Voice 4 fades in at midrange
            activation = SmoothStep( 0.30f, 0.40f, master );
        }

        if ( voiceIndex == 5 )
        {
            // Voice 5 fades in at near the beginning of the upper range
            activation = SmoothStep( 0.60f, 0.70f, master );
        }

        return Constants.VoiceBaseWeight * activation;
    }

    /// <summary>
    /// Asymmetric sigmoid function
    /// </summary>
    /// <param name="x"></param>
    /// <param name="center"></param>
    /// <param name="steepL"></param>
    /// <param name="steepR"></param>
    /// <returns></returns>
    private static float AsymSigmoid ( float x, float center, float steepL, float steepR )
    {
        if ( x < center )
        {
            return 0.5f * (float)Math.Pow( x / center, steepL );
        }

        return 1f - ( 0.5f * (float)Math.Pow( ( 1f - x ) / ( 1f - center ), steepR ) );
    }

    private static float Clamp ( float value, float min, float max )
    {
        if ( value < min )
        { return min; }

        if ( value > max )
        { return max; }

        return value;
    }

    static float dB ( float dbValue )
    {
        return (float)Math.Pow( 10.0, dbValue / 20.0 );
    }

    private static float EaseInPow ( float t, float p )
    {
        // Ease-in curve t^p on [0..1], clamped for safety
        return (float)Math.Pow( Clamp( t, 0f, 1f ), p );
    }

    /// <summary>
    /// Linear interpolation between two values based on a given weight
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    private static float LinearInterpolate ( float a, float b, float t )
    {
        return a + ( ( b - a ) * t );
    }

    private static float LinearRescale ( float x, float inMin, float inMax )
    {
        return Clamp( ( x - inMin ) / ( inMax - inMin ), 0f, 1f );
    }

    /// <summary>
    /// Linear scaling helper for fxCount‑based parameters.
    /// </summary>
    private static float LinearScale ( float fxLevel, float minFx, float maxFx, float minVal, float maxVal )
    {
        var t = Clamp( ( fxLevel - minFx ) / ( maxFx - minFx ), 0f, 1f );
        return minVal + ( ( maxVal - minVal ) * t );
    }
    
    /// <summary>
    /// Easier to tune than a true sigmoid.
    /// Applies a Windowed cubic Hermite polynomial to create a smooth threshold between points defined using a center and width.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="center"></param>
    /// <param name="width"></param>
    /// <returns></returns>
    private static float SoftStep ( float x, float center, float width )
    {
        // width controls how wide the transition band is
        var t = Clamp( ( x - ( center - ( width * 0.5f ) ) ) / width, 0f, 1f );
        return t * t * ( 3f - ( 2f * t ) ); // cubic Hermite
    }

    /// <summary>
    /// Applies a piecewise Cubic Hermite Interpolating Polynomial to create a smooth spline between data points.
    /// Local control: Y[i] only affects its two neighboring segments.
    /// Monotonic - it avoids overshoot.
    /// Natural “ease-in/ease-out” transitions between control points.
    /// </summary>
    /// <param name="X"></param>
    /// <param name="Y"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private static float SmoothSpline ( float[] X, float[] Y, float x )
    {
        // Piecewise Cubic Hermite Interpolating Polynomial
        var n = X.Length;
        if ( x <= X[ 0 ] )
        {
            return Y[ 0 ];
        }

        if ( x >= X[ n - 1 ] )
        {
            return Y[ n - 1 ];
        }

        var i = 0;
        while ( i < ( n - 1 ) && x > X[ i + 1 ] )
            i++;

        var h = X[i + 1] - X[i];
        var t = (x - X[i]) / h;
        var t2 = t * t;
        var t3 = t2 * t;

        var m0 = i == 0 ? (Y[1] - Y[0]) / (X[1] - X[0]) : (Y[i + 1] - Y[i - 1]) / (X[i + 1] - X[i - 1]);
        var m1 = i == (n - 2) ? (Y[n - 1] - Y[n - 2]) / (X[n - 1] - X[n - 2]) : (Y[i + 2] - Y[i]) / (X[i + 2] - X[i]);

        var h00 = (2f * t3) - (3f * t2) + 1f;
        var h10 = t3 - (2f * t2) + t;
        var h01 = (-2f * t3) + (3f * t2);
        var h11 = t3 - t2;

        return ( h00 * Y[ i ] ) + ( h * h10 * m0 ) + ( h01 * Y[ i + 1 ] ) + ( h * h11 * m1 );
    }

    /// <summary>
    /// Applies a Single cubic Hermite polynomial to create an S shaped curve with zero slope at aither edge and max slope at the center.
    /// Can be used for "fade in/out" or "ease" transitions.
    /// </summary>
    /// <param name="edge0"></param>
    /// <param name="edge1"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private static float SmoothStep ( float edge0, float edge1, float x )
    {
        var t = Clamp( ( x - edge0 ) / ( edge1 - edge0 ), 0f, 1f );
        return t * t * ( 3f - ( 2f * t ) );
    }
}