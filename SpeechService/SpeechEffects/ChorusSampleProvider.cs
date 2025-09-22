using EddiSpeechService.SpeechEffects;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;

internal class ChorusSampleProvider : EffectSampleProvider
{
    // ===== Chorus tuning and safety bounds =====

    // Mix
    private const double DryAnchorMin           = 0.05;  // prevent dry loss from falling below this value
    private const double WetCurveAlpha          = 0.50;  // concave wet curve (quickly grows to near full power at low fx); 
    private const double MaxMakeupGainDb        = 15.0;  // up to +15 dB to compensate for chorus energy loss

    // Delay spread
    private const float  CenterDelayMs          = 16f;   // perceptual “body” region
    private const float  StepMinMs              = 4.0f;  // tight ensemble at low fx
    private const float  StepMaxMs              = 8.0f;  // max ensemble bread at high fx
    private const float  OuterBiasK             = 0.30f; // defines the power with which outer voices are spread out in the ensemble
    private const float  OuterBiasPow           = 0.80f; // concave curve spreads outer voices in the ensemble more quickly at low fx.

    // Modulation Signal Shaping (per voice)
    private const float  LfoMinHz               = 0.90f; // the minimum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
    private const float  LfoMaxHz               = 2.00f; // the maximum amplitude of the Low Frequency Oscillator (LFO) signal controlling delay time.
    private const float  LfoPercent1            = 0.82f; // The percent of the LFO from the first signal curve
    private const float  LfoPercent2            = 1.0f - LfoPercent1; // The percent of the LFO from the second signal curve
    private const float  LfoRamp                = 0.8f;
    private const float  DetuneSpan             = 0.20f; // ±20% across voices, adds variation to the LFO rate and helps "shimmer" emerge earlier

    // Depth shaping
    // Shallow (2–4 ms):  Subtle thickening, gentle detune. Warmth without obvious motion.
    // Moderate (5–8 ms): Classic chorus effect. Noticeable motion, but not extreme.
    // Deep (9–12 ms):    Warbly effect. Strong motion, metallic timbre.
    private const float  DepthFloor             = 0.20f; // minimum depth
    private const float  DepthMaxMsAt100        = 12f;   // target nominal max depth at 100% before caps
    private const float  DepthPosMin            = 0.6f;  // inner voices
    private const float  DepthPosMax            = 1.0f;  // outer voices
    private const float  DepthPow               = 0.70f; // concave growth; more audible at mid fx
    private const float  MaxDepthFractionOfBase = 0.60f; // depth <= 60% of base delay

    // Safety: absolute delay and buffer sizing
    private const float  MaxVoiceDelayMs        = 90f;   // hard cap base+depth per voice
    private const float  BufferHeadroomMs       = 10f;   // extra beyond (base+depth)

    // Feedback
    private const float  Feedback               = 0.23f; // base feedback. Scales dynamically in code. Defines how much of the wet signal is recirculated into the chorus delay lines.
    private const float  FeedbackFloorPercent   = 0.70f;

    // LFO phase decorrelation
    private const float  PhaseJitterRadians     = 0.35f; // small random jitter around staggered phases

    private readonly List<(SimpleChorusVoice voice, float weight)> _voices = new List<(SimpleChorusVoice, float)>();

    private readonly float _norm;
    private readonly float _normSquareRoot;
    private readonly double _dryGain;
    private readonly double _wetGain;
    private float _fbState;

    public ChorusSampleProvider ( ISampleProvider source, int sampleRate, int fxLevel, int damageAdjustedFxLevel ) : base( source )
    {
        if ( damageAdjustedFxLevel == 0 )
        {
            _dryGain = 1.0;
            _wetGain = 0.0;
            return;
        }
        
        _norm = damageAdjustedFxLevel / 100f;
        _normSquareRoot = (float)Math.Sqrt(Math.Max(0f, Math.Min(1f, _norm)));

        // Scale number of voices with fxLevel 
        var voiceCount = fxLevel < 60 ? 4 : fxLevel < 85 ? 5 : 6;
        
        // Wet/dry Mixing (concave, constant power)
        var w = Math.Pow(_norm, WetCurveAlpha);
        _dryGain = Math.Sqrt( Math.Max( 0.0, 1.0 - ( w * w ) ) );
        _wetGain = w / Math.Sqrt( voiceCount );

        // Boost gain to compensate for energy loss in the chorus ensemble
        var compDb = MaxMakeupGainDb * _norm;
        var comp   = Math.Pow(10, compDb / 20.0);
        _dryGain *= comp;
        _wetGain *= comp;

        // Hybrid delay spread: linear core + nonlinear outer bias
        var baseDelaysMs = new float[voiceCount];
        var centerIdx   = (voiceCount - 1) / 2f;

        // Step scales with intensity in [StepMinMs..StepMaxMs]
        var step = StepMinMs + ( ( StepMaxMs - StepMinMs ) * _norm );

        for ( var v = 0; v < voiceCount; v++ )
        {
            // Linear spread around center index
            var baseDelay = CenterDelayMs + ((v - centerIdx) * step);

            // Nonlinear outer bias (use distance from center index, not CenterDelayMs)
            var dist   = Math.Abs(v - centerIdx);
            var bias = 1f + ( OuterBiasK * (float)Math.Pow( dist, OuterBiasPow ) );
            var voiced = baseDelay * bias;

            // Safety cap on base delay (keeps us chorus-like)
            baseDelaysMs[ v ] = Math.Min( voiced, MaxVoiceDelayMs );
        }

        InitializeChorusVoices( sampleRate, voiceCount, baseDelaysMs );
    }

    private void InitializeChorusVoices ( int sampleRate, int voiceCount, float[] baseDelaysMs )
    {
        var rnd = new Random();
        var centerIdx = (voiceCount - 1) / 2f;

        for ( var v = 0; v < voiceCount; v++ )
        {
            // Detune spread ±DetuneSpan across voices
            var pos = ( v - centerIdx ) / Math.Max( 1f, centerIdx ); // -1..+1
            var detune = 1.0f + ( pos * DetuneSpan );

            // LFO frequency with clamp
            var baseRate = Lerp( LfoMinHz, LfoMaxHz, _normSquareRoot );
            var convex = Lerp( LfoMinHz, LfoMaxHz, _normSquareRoot * _normSquareRoot );

            var freqHz   = (0.5f * baseRate) + (0.5f * convex); // ~1.0–1.2 Hz at 30–40; ~1.7–2.0 Hz by 100
            freqHz = Clamp( freqHz * detune, LfoMinHz, LfoMaxHz );

            // Depth scaling by position (outer is deeper)
            var posScale = DepthPosMin + ( ( DepthPosMax - DepthPosMin ) * Math.Abs( pos ) );
            var depthMs = DepthMaxMsAt100 * (float)Math.Pow( _norm, DepthPow ) * posScale;

            // Cap depth against base delay fraction and absolute max voice delay
            var baseMs = baseDelaysMs[ v ];
            var maxDepthByBase = baseMs * MaxDepthFractionOfBase;
            var maxDepthByAbs = Math.Max( 0f, MaxVoiceDelayMs - baseMs );
            var maxDepth = Math.Min( maxDepthByBase, maxDepthByAbs );
            depthMs = Math.Max( Math.Min( depthMs, maxDepth ), DepthFloor );

            // Construct voice (voice will allocate buffer sized to base+depth+headroom)
            var voice = new SimpleChorusVoice( sampleRate, baseMs, depthMs, freqHz, _normSquareRoot );

            // Evenly distributed phases + small jitter to decorrelate
            var stagger = (float)( v * ( 2.0 * Math.PI / voiceCount ) );
            var jitter = (float)( ( ( rnd.NextDouble() * 2 ) - 1 ) * PhaseJitterRadians );
            voice.SetPhase( stagger + jitter );

            // Weight outer voices more
            var weight = 0.8f + ( 0.4f * Math.Abs( pos ) );
            _voices.Add( ( voice, weight ) );
        }
    }

    protected override float ProcessSample ( float x )
    {
        if ( _voices.Count == 0 )
        {
            // Pure dry passthrough
            return (float)( _dryGain * x );
        }

        // Inputs: Feedback, FeedbackFloorPercent, _norm in [0,1]
        var fb = Lerp(Feedback * FeedbackFloorPercent, Feedback, _normSquareRoot);
        var inWithFb = x + (fb * _fbState);

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

        return (float)( ( dry * x ) + ( wet * wetMix ) );
    }

    private static float Clamp ( float value, float min, float max )
    {
        return value < min 
            ? min 
            : value > max 
                ? max 
                : value;
    }

    private static float Lerp ( float a, float b, float t )
    {
        return a + ( ( b - a ) * t );
    }

    private class SimpleChorusVoice
    {
        private readonly float[] _buffer;
        private int _writePos;
        private readonly float _baseDelaySamples;
        private readonly float _depthSamples;
        private readonly float _lfoIncrement;
        private float _lfoPhase;
        private readonly float _normSquareRoot;

        public SimpleChorusVoice ( int sampleRate, float baseDelayMs, float depthMs, float lfoHz, float normSquareRoot )
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
            _normSquareRoot = normSquareRoot;
        }

        public float Process ( float input )
        {
            _buffer[ _writePos ] = input;

            // LFO

            var ss = _normSquareRoot * (2f - _normSquareRoot);
            var h2 = Math.Min( 0.25f, LfoPercent2 + ( LfoRamp * ss ) );
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
            var delaySamples = _baseDelaySamples + (lfo * _depthSamples);
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
}