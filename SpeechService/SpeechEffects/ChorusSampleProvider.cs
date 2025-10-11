using EddiSpeechService.SpeechEffects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

internal class ChorusSampleProvider : EffectSampleProvider
{
    // ===== Chorus tuning and safety bounds =====

    // Wet/Dry Mixing
    private const float TrueDryThreshold        = 0.05f; // below this, force dry
    private const float DryAnchorMin            = 0.05f;  // prevent dry loss from falling below this value
    private const float WetCurveAlpha           = 2.00f;  // wet curve (quickly grows to near full power at low fx); 
    private const float WetRampStart            = 0.05f;
    private const float WetRampEnd              = 0.40f;

    // Delay spread
    private const float CenterDelayMs           = 16.0f; // perceptual “body” region
    private const float StepMinMs               = 3.50f; // tight ensemble at low fx
    private const float StepMaxMs               = 7.00f; // max ensemble bread at high fx

    // Depth shaping
    // Shallow (2–4 ms):  Subtle thickening, gentle detune. Warmth without obvious motion.
    // Moderate (5–8 ms): Classic chorus effect. Noticeable motion, but not extreme.
    // Deep (9–12 ms):    Warbly effect. Strong motion, metallic timbre.
    private const float DepthFloor              = 0.35f; // minimum depth
    private const float DepthMax                = 12.0f; // target nominal max depth at 100% before caps
    private const float DepthPosMin             = 0.60f; // inner voices
    private const float DepthPosMax             = 1.00f; // outer voices
    private const float MaxDepthFractionOfBase  = 0.30f; // depth <= 30% of base delay
    private const float DepthEasePower          = 1.45f;

    // Detune
    private const float DetuneSpanMin           = 0.18f;
    private const float DetuneSpanMax           = 0.37f;
    private const float DetuneEasePower         = 1.50f;

    // Feedback
    private const float Feedback                = 0.36f; // base feedback. Scales dynamically in code. Defines how much of the wet signal is recirculated into the chorus delay lines.
    private const float FeedbackRampPow         = 0.925f;
    private const float FeedbackFloorPercent    = 0.75f;
    private const float FeedbackShelfCutoffHz   = 2800f;
    private const float FeedbackShelfGainMinDb  = 0.00f;
    private const float FeedbackShelfGainMaxDb  = 3.00f;
    private const float FeedbackShelfGainEarlyDb= 1.50f;
    private const float FeedbackShelfGainLateDb = 4.50f;
    private const float FeedbackShelfStart      = 0.02f;
    private const float FeedbackShelfEnd        = 0.42f;
    private const float FeedbackMaxDb           = 0.36f;
    private const float FeedbackKneeDb          = 0.35f;
    private const float FeedbackMinNorm         = 0.20f;

    // Modulation Signal Shaping (per voice)
    private const float LfoMinHz                = 0.90f; // the minimum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
    private const float LfoMaxHz                = 2.30f; // the maximum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
    private const float LfoPercent1             = 0.82f; // The percent of the LFO from the first signal curve
    private const float LfoPercent2             = 1.00f - LfoPercent1; // The percent of the LFO from the second signal curve
    private const float LfoRamp                 = 0.80f;
    private const float OuterLfoBias            = 1.05f; // outer voices have slightly faster LFO for more motion
    private const float MicroCombOffset         = 0.10f; // In milliseconds
    private const float PhaseJitterRadians      = 0.35f; // small random jitter around staggered phases
    private const float LfoJitterDepth          = 0.005f;
    private const float LfoJitterRateHz         = 0.30f;

    // Outer Voice Shaping
    private const float OuterVoiceWeightMin     = 0.20f;
    private const float OuterVoiceWeightMax     = 0.35f;
    private const float OuterVoiceSpanMin       = 0.28f;
    private const float OuterVoiceSpanMax       = 0.58f;

    // Voice Count
    private const int VoiceThresholdMid         = 50;
    private const int VoiceThresholdHigh        = 85;
    private const float VoiceBlendStart         = 0.05f;
    private const float VoiceBlendEnd           = 0.20f;

    // Normalization
    private const float NormCompThreshold       = 0.60f;
    private const float NormCompBoost           = 1.08f;
    private const float NormCompUnity           = 1.00f;
    private const float NormKnee                = 57.5f;
    private const float MaxMakeupGainDb         = 5.00f;  // How much to compensate for chorus energy loss in decibels
    private const double MakeupGainPow          = 1.00;

    // Body Shelf
    private const float BodyShelfCutoffHz       = 120f;
    private const float BodyShelfGainDb         = 1.80f;
    private const float BodyBlendStart          = 0.00f;
    private const float BodyBlendEnd            = 0.60f;
    private const float BodyShelfEnd            = 1.00f;
    private const float BodyShelfQ              = 0.70f;
    
    // Metallic Shelf
    private const float MetallicShelfCutoffHz   = 3200f;
    private const float MetallicShelfMinGainDb  = -0.5f;
    private const float MetallicShelfGainDb     = -1.5f;
    private const float MetallicBlendStart      = 0.08f;
    private const float MetallicBlendEnd        = 1.00f;
    private const float MetallicQ               = 0.85f;
    
    // High Shelf: Brightness and Shimmer
    private const float TiltShelfCutoffHz       = 4900f;
    private const float TiltShelfGainDb         = 0.60f;
    private const float TiltBlendStart          = 0.00f;
    private const float TiltBlendEnd            = 0.50f;
    private const float TiltRampEnd             = 1.00f;
    private const float TiltRampPow             = 2.50f;
    private const float TiltRampQ               = 0.70f;
    private const float ShimmerFloorGainDb      = 0.90f;
    private const float ShimmerBlendMin         = 0.00f;
    private const float ShimmerBlendMax         = 0.50f;

    // Safety: absolute delay and buffer sizing
    private const float MaxVoiceDelayMs         = 90.0f; // hard cap base+depth per voice
    private const float BufferHeadroomMs        = 10.0f; // extra beyond (base+depth)

    private readonly List<(ChorusVoice voice, float weight)> _voices = new List<(ChorusVoice, float)>();

    private float _fbState;
    private readonly double _levelComp; // loudness normalization
    private readonly float _norm; // Normalized [0..1] intensity
    private readonly double _dryGain;
    private readonly double _wetGain;
    private readonly OnePoleHighShelf _feedbackShelf; // high-shelf filter for feedback coloration
    private readonly BiquadHighShelf _highShelf; 
    private readonly BiquadHighShelf _metallicAttenuator;
    private readonly BiquadLowShelf _bodyShelf;

    public ChorusSampleProvider ( ISampleProvider source, int sampleRate, int fxLevel, int damageAdjustedFxLevel ) : base( source )
    {
        _norm = damageAdjustedFxLevel / 100f;

        // Scaled normalization:
        _levelComp = Lerp( NormCompBoost, NormCompUnity, Remap01( _norm, 0f, NormCompThreshold ) );

        // At very low fx, softly fade in chorus instead of hard cut
        // This ensures 0 fx is essentially dry, but shimmer/body ramp in smoothly.
        if ( _norm <= 0f )
        {
            _dryGain = 1.0;
            _wetGain = 0.0;
            _voices.Clear();
            return;
        }
        // Softly ramp in wet to avoid a hard cut
        var rampNorm = Remap01(_norm, WetRampStart, WetRampEnd);

        // Initialize feedback coloration filter
        var fbGainDb = Lerp(FeedbackShelfGainMinDb, FeedbackShelfGainMaxDb, _norm);
        _feedbackShelf = new OnePoleHighShelf( FeedbackShelfCutoffHz, fbGainDb, sampleRate );
        
        // High band shelf
        var shimmerBlend = Remap01(_norm, ShimmerBlendMin, ShimmerBlendMax); // earlier shimmer ramp
        var tiltBlend    = Remap01(_norm, TiltBlendStart, TiltBlendEnd);
        var tiltScale    = Remap01(_norm, TiltBlendEnd, TiltRampEnd);
        var hiGainDb = (ShimmerFloorGainDb * shimmerBlend) + (TiltShelfGainDb * EaseInPow(tiltBlend * tiltScale, TiltRampPow));
        _highShelf = new BiquadHighShelf( TiltShelfCutoffHz, hiGainDb, TiltRampQ, sampleRate );

        // Metallic shimmer 
        var metallicBlend = Remap01(_norm, MetallicBlendStart, MetallicBlendEnd); // fade-in
        var metallicGainDb = Lerp(MetallicShelfMinGainDb, MetallicShelfGainDb, metallicBlend);  // continue scaling
        _metallicAttenuator = new BiquadHighShelf( MetallicShelfCutoffHz, metallicGainDb, MetallicQ, sampleRate );

        // Low‑mid body restoration 
        var bodyBlend = Remap01(_norm, BodyBlendStart, BodyBlendEnd); // fade-in
        var bodyScale = Remap01(_norm, BodyBlendEnd, BodyShelfEnd);  // continue scaling
        var bodyShelfGainDb = Lerp(0f, BodyShelfGainDb, bodyBlend * bodyScale);
        _bodyShelf = new BiquadLowShelf( BodyShelfCutoffHz, bodyShelfGainDb, BodyShelfQ, sampleRate );

        // Scale number of voices with fxLevel 
        var voiceCount = GetVoiceCount(fxLevel);

        // Wet/dry Mixing (concave, constant power)
        var w = Math.Pow(rampNorm, WetCurveAlpha);
        _dryGain = Math.Sqrt( Math.Max( 0.0, 1.0 - ( w * w ) ) );
        _wetGain = w / Math.Sqrt( voiceCount );

        // Boost gain to compensate for energy loss in the chorus ensemble
        // Bias compensation to mid fx levels
        var t = Clamp(fxLevel / NormKnee, 0f, 1f);
        var compDb = Math.Pow(t, MakeupGainPow) * MaxMakeupGainDb; // exponent <1 = convex, rises faster in the first half
        var comp   = Math.Pow(10, compDb / 20.0);
        _dryGain *= comp;
        _wetGain *= comp;

        // Hybrid delay spread: linear core + nonlinear outer bias
        var baseDelaysMs = new float[voiceCount];
        var centerIdx   = (voiceCount - 1) / 2f;

        // Step scales with intensity in [StepMinMs..StepMaxMs]
        var step = ScaleLinear(fxLevel, 0, 100, StepMinMs, StepMaxMs);

        for ( var v = 0; v < voiceCount; v++ )
        {
            // Pure linear spread around center index
            var baseDelay = CenterDelayMs + ( ( v - centerIdx ) * step );
            baseDelaysMs[ v ] = Math.Min( baseDelay, MaxVoiceDelayMs );
        }

        InitializeChorusVoices( sampleRate, voiceCount, baseDelaysMs );
    }

    private static int GetVoiceCount ( int fxLevel ) =>
        fxLevel < VoiceThresholdMid ? 4 :
        fxLevel < VoiceThresholdHigh ? 5 : 6;

    private void InitializeChorusVoices ( int sampleRate, int voiceCount, float[] baseDelaysMs )
    {
        var rnd = new Random();
        var centerIdx = (voiceCount - 1) / 2f;

        for ( var v = 0; v < voiceCount; v++ )
        {
            // Detune spread ±DetuneSpan across voices
            // Force outer voices to be symmetrical for added metallic character.
            // Detune spread grows with fx: ~0.16 at low fx to ~0.28 at high fx
            var detuneSpanNow = Lerp(DetuneSpanMin, DetuneSpanMax, EaseInPow(_norm, DetuneEasePower));
            var pos = ( v - centerIdx ) / Math.Max( 1f, centerIdx ); // -1..+1
            var detune = 1.0f;
            if ( v == 0 )
            {
                detune -= detuneSpanNow;
            }
            else if ( v == (voiceCount - 1) )
            {
                detune += detuneSpanNow;
            }
            else
            {
                detune += pos * detuneSpanNow;
            }

            // LFO frequency
            var outerBias = v == 0 || v == (voiceCount - 1) ? OuterLfoBias : 1.0f;
            var freqHz = ScaleLinear(_norm, 0f, 1f, LfoMinHz, LfoMaxHz) * detune * outerBias;

            // Depth scaling by position (outer is deeper)
            var posScale = DepthPosMin + ((DepthPosMax - DepthPosMin) * Math.Abs(pos));
            var depthTargetMs = DepthFloor + ( ( DepthMax - DepthFloor ) * EaseInPow( _norm,DepthEasePower ) );
            var depthMs = depthTargetMs * posScale;

            // Add tiny, fixed offsets to base delay per voice to seed consistent micro‑comb features that read as “metallic”
            float microOffsetMs;
            if ( v == 0 )
            {
                microOffsetMs = -MicroCombOffset;
            }
            else if ( v == ( voiceCount - 1 ) )
            {
                microOffsetMs = +MicroCombOffset;
            }
            else
            {
                microOffsetMs = pos > 0 ? +MicroCombOffset / 2f : -MicroCombOffset / 2f;
            }
            var baseMs = baseDelaysMs[v] + microOffsetMs;

            // Cap depth against base delay fraction and absolute max voice delay
            var maxDepthByBase = baseMs * MaxDepthFractionOfBase;
            var maxDepthByAbs = Math.Max( 0f, MaxVoiceDelayMs - baseMs );
            var maxDepth = Math.Min( maxDepthByBase, maxDepthByAbs );
            depthMs = Math.Max( Math.Min( depthMs, maxDepth ), DepthFloor );

            // Construct voice (voice will allocate buffer sized to base+depth+headroom)
            var voice = new ChorusVoice( sampleRate, baseMs, depthMs, freqHz, _norm );

            // Evenly distributed phases + small jitter to decorrelate,
            // except force outer voices to be symmetrical
            var stagger = (float)( v * ( 2.0 * Math.PI / voiceCount ) );
            var jitter = (float)( ( ( rnd.NextDouble() * 2 ) - 1 ) * PhaseJitterRadians );
            if ( v == 0 )
            {
                voice.SetPhase( 0f );
            }
            else if ( v == (voiceCount - 1) )
            {
                voice.SetPhase( (float)Math.PI );
            }
            else
            {
                voice.SetPhase( stagger + jitter );
            }

            // Weight outer voices more strongly for sharper metallic beating
            // Blend weighting slope based on norm
            var slope = Lerp(OuterVoiceWeightMin, OuterVoiceWeightMax, SmoothStep(OuterVoiceSpanMin, OuterVoiceSpanMax, _norm));
            var voiceBlend = SmoothStep(VoiceBlendStart, VoiceBlendEnd, _norm);
            var baseWeight = 1.05f;
            var weight = (baseWeight + (slope * Math.Abs(pos))) * voiceBlend;

            _voices.Add( ( voice, weight ) );
        }
    }

    protected override float ProcessSample ( float x )
    {
        if ( _voices.Count == 0 || _norm < TrueDryThreshold )
        {
            // Pure dry passthrough
            return (float)( _dryGain * x );
        }

        // Feedback ramp with convex ease-in
        var fbNorm = (float)Math.Pow(_norm, FeedbackRampPow); // convex: more feedback earlier
        var fbTarget = ScaleLinear(fbNorm, FeedbackMinNorm, 1.0f, Feedback * FeedbackFloorPercent, Feedback);

        // Soft‑knee clamp near top fx
        var kneeBlend = Remap01(_norm, 0.85f, 1.0f);
        var kneeMax = Lerp(FeedbackMaxDb, FeedbackKneeDb, kneeBlend);
        var fb = Math.Min(fbTarget, kneeMax);

        // Apply HF shelf to feedback signal
        var shelfNorm = Remap01(_norm, FeedbackShelfStart, FeedbackShelfEnd);
        _feedbackShelf.SetGainDb( Lerp( FeedbackShelfGainEarlyDb, FeedbackShelfGainLateDb, shelfNorm ) );
        var fbColored = _feedbackShelf.Process(_fbState);
        var inWithFb = x + (fb * fbColored);

        var wetSum = 0f;
        var weightSum = 0f;

        // Sum all chorus voices
        foreach ( var (voice, weight) in _voices )
        {
            var wetSample = voice.Process( inWithFb );
            wetSum += weight * wetSample;
            weightSum += weight;
        }

        // Feedback uses normalized wet
        _fbState = weightSum > 0 ? wetSum / weightSum : 0f;

        // Normalize wet energy
        var wetMix = weightSum > 0 ? wetSum / (float)Math.Sqrt(weightSum) : 0f;

        // --- Mix with dry ---
        var dry = Math.Max( DryAnchorMin, _dryGain );
        var wet = _wetGain;

        if ( _norm > 0 )
        {
            wetMix = _highShelf.Process( wetMix );
            wetMix = _metallicAttenuator.Process( wetMix );
            wetMix = _bodyShelf.Process( wetMix );
        }

        return (float)( ( ( dry * x ) + ( wet * wetMix ) ) * _levelComp );
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
            var totalDelayMs = Math.Min(MaxVoiceDelayMs, baseDelayMs + depthMs) + BufferHeadroomMs;
            var bufLen         = Math.Max(64, (int)Math.Ceiling(sampleRate * totalDelayMs / 1000.0)) + 2;

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
            _buffer[ _writePos ] = input;

            // LFO
            var ss = _norm * (2f - _norm);
            var h2 = Math.Min( 0.22f, LfoPercent2 + ( LfoRamp * ss * 0.9f ) );
            var s1w = 1f - h2;

            var s1 = Math.Sin(_lfoPhase);
            var s2 = Math.Sin(2f * _lfoPhase);
            var lfo = (float)(s1w * s1) + (float)(h2 * s2);

            _lfoPhase += _lfoIncrement;
            if ( _lfoPhase >= (2 * Math.PI) )
            {
                _lfoPhase -= (float)( 2 * Math.PI );
            }

            // Fractional read index
            var jitter = 1.0f + ( LfoJitterDepth * (float)Math.Sin( _lfoPhase * LfoJitterRateHz ) );
            var delaySamples = _baseDelaySamples + (lfo * _depthSamples * jitter);
            var readIndex = _writePos - delaySamples;

            // Wrap safely into [0..len)
            var len = _buffer.Length;
            // handle negatives and large positives robustly
            readIndex %= len;
            if ( readIndex < 0 )
            {
                readIndex += len;
            }
            
            var i0 = (int)readIndex;
            var i1 = (i0 + 1) % len;
            var frac = readIndex - i0;

            // Ensure indices are valid by wrapping them within bounds
            i0 = ( i0 + len ) % len;
            i1 = ( i1 + len ) % len;

            var delayed = (_buffer[i0] * (1 - frac)) + (_buffer[i1] * frac);

            // Advance write pointer
            _writePos++;
            if ( _writePos >= len )
            {
                _writePos = 0;
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

    private static float Clamp ( float value, float min, float max )
    {
        return value < min
            ? min
            : value > max
                ? max
                : value;
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
    private static float Lerp ( float a, float b, float t )
    {
        return a + ( ( b - a ) * t );
    }
    
    private static float Remap01 ( float x, float inMin, float inMax )
    {
        return Clamp( ( x - inMin ) / ( inMax - inMin ), 0f, 1f );
    }

    /// <summary>
    /// Linear scaling helper for fxCount‑based parameters.
    /// </summary>
    private static float ScaleLinear ( float fxLevel, float minFx, float maxFx, float minVal, float maxVal )
    {
        var t = Clamp( ( fxLevel - minFx ) / (maxFx - minFx), 0f, 1f );
        return minVal + ( ( maxVal - minVal ) * t );
    }

    /// <summary>
    /// Similar to the `ScaleLinear` method except it applies a cubic Hermite polynomial (e.g. "t^2*(3-2t)") to ease in/out, so the slope is zero at the edges
    /// </summary>
    /// <param name="edge0"></param>
    /// <param name="edge1"></param>
    /// <param name="x"></param>
    /// <returns></returns>
    private static float SmoothStep ( float edge0, float edge1, float x )
    {
        var t = Clamp((x - edge0) / (edge1 - edge0), 0f, 1f);
        return t * t * ( 3f - (2f * t) );
    }
}