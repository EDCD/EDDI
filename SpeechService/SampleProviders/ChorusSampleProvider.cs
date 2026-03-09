using EddiSpeechService.Filters;
using EddiSpeechService.SampleProviders.ChorusHelpers;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using Constants = EddiSpeechService.SampleProviders.ChorusHelpers.Constants;

namespace EddiSpeechService.SampleProviders
{
    internal class ChorusSampleProvider : EffectSampleProvider
    {
        private readonly List<(ChorusVoice voice, float weight)> _voices = [ ];

        private readonly int _fxLevel; 
        private readonly float _dryGain;
        private readonly float _wetGain;
        private readonly float _makeupGain;
        private float _bodyGainDb;
        private readonly float _depthTargetMs;
        private readonly float _fbDrive;
        private float _fbState;
        private readonly float _feedbackAmount;
        private readonly bool _isBypassed;
        private float _lfoRateScale;
        private float _microCombScale;
        private readonly Curve.ModStruct _mod;
        private float _mudCutDb;
        private float _presenceCutDb;
        private float _wetHpfHz;
        private float _wetLpfHz;
        private float _wetNorm;

        private BiquadPeakingEqFilter _bodyEq;
        private TwoPoleHighPassFilter _dryHpf;
        private readonly TwoPoleHighPassFilter _feedbackHpf;
        private readonly OnePoleLowPassFilter _feedbackLpf;
        private readonly OnePoleHighShelf _feedbackShelf;
        private BiquadPeakingEqFilter _mudCutEq;
        private BiquadPeakingEqFilter _presenceCutEq;
        private AllPassFilter _resonanceAllPass;
        private TwoPoleHighPassFilter _wetHpf;
        private OnePoleLowPassFilter _wetLpfShelf;

        public ChorusSampleProvider ( ISampleProvider source, int sampleRate, int fxLevel ) : base( source )
        {
            // Our input fxLevel should always be an integer value between 0 and 100. 
            _fxLevel = fxLevel;
            _isBypassed = _fxLevel < Constants.TrueDryThreshold;

            _dryGain = 0.0f;
            _wetGain = 0.0f;
            _fbState = 0f;
            _voices.Clear();

            if ( !_isBypassed )
            {
                // Define the master curve we'll fit our effects around
                var master = SpeechFxFunctions.AsymSigmoid( _fxLevel / 100f, Constants.SigmoidCenter, Constants.SigmoidSteepL,
                    Constants.SigmoidSteepR );

                // Wet/dry gain
                _dryGain = Curve.DryGain( _fxLevel );
                _wetGain = Curve.WetGain( _fxLevel );
                _makeupGain = SpeechFxFunctions.DecibalsToLinear( Curve.MixMakeupDb( _fxLevel ) );

                // Modulation Profile
                _mod = Curve.ModulationProfile( _fxLevel );

                // Feedback
                _feedbackLpf = new OnePoleLowPassFilter();
                _feedbackLpf.Set( Curve.FeedbackLpfCutoff( master ), sampleRate );
                _feedbackShelf = new OnePoleHighShelf( Constants.FeedbackShelfCutoffHz, Curve.FeedbackGainDb( master ),
                    sampleRate );
                _feedbackAmount = Curve.Feedback( master, _fxLevel );
                _fbDrive = SpeechFxFunctions.LinearInterpolate( Constants.FeedbackSoftLimitDriveMax, 1.0f, _fxLevel / 100f );
                var feedbackHpfHz = Curve.FeedbackHpfHz(_fxLevel);
                _feedbackHpf = new TwoPoleHighPassFilter();
                _feedbackHpf.Set( feedbackHpfHz, sampleRate );

                // Hybrid delay spread: linear core + nonlinear outer bias
                var voiceBaseDelaysMs = new float[ Constants.VoiceCount ];
                var centerIdx = ( Constants.VoiceCount - 1 ) / 2f;

                // Step scales with intensity in [StepMinMs..StepMaxMs]
                var baseStep = SpeechFxFunctions.LinearInterpolate(
                    Constants.StepMinMs,
                    Constants.StepMaxMs,
                    SpeechFxFunctions.SmoothStep( Constants.StepEdge0, Constants.StepEdge1, master )
                );

                // Curved emphasis
                var stepCurve = Curve.StepSpline.Evaluate( master );
                var stepMs = baseStep * stepCurve;

                // Delay
                var centerDelayMs = Curve.CenterDelayMs( _fxLevel );
                for ( var v = 0; v < Constants.VoiceCount; v++ )
                {
                    // Pure linear spread around center index
                    var baseDelayMs = centerDelayMs + ( ( v - centerIdx ) * stepMs );
                    voiceBaseDelaysMs[ v ] = Math.Max( Constants.MinVoiceDelayMs,
                        Math.Min( baseDelayMs, Constants.MaxVoiceDelayMs ) );
                }

                // Depth
                _depthTargetMs = Curve.ChorusDepth( _fxLevel );

                InitializeChorusFilters( sampleRate );
                InitializeChorusVoices( sampleRate, Constants.VoiceCount, voiceBaseDelaysMs );
                CalculateVoiceWeightSum();
            }
        }

        private void InitializeChorusVoices ( int sampleRate, int voiceCount, float[] baseDelaysMs )
        {
            var centerIdx = ( voiceCount - 1 ) / 2f;
            var ghost = Curve.GhostProfile( _fxLevel );

            var ghostWeightFactor = ghost.GhostWeight;
            var ghostDepthFactor = ghost.GhostDepthFactor;

            for ( var v = 0; v < voiceCount; v++ )
            {
                // --- Detune spread ±DetuneSpan across voices ---
                // Force outer voices to be symmetrical for added metallic character.
                // Detune spread grows with fx: ~0.16 at low fx to ~0.28 at high fx
                var detuneSpan = Curve.DetuneSpan( _fxLevel );
                var pos = ( v - centerIdx ) / Math.Max( 1f, centerIdx ); // -1..+1
                var detune = 1f + ( detuneSpan * pos );

                // Use a symmetric key so left/right voice pairs share the same jitter magnitude.
                var vPair = Math.Min( v, voiceCount - 1 - v );

                // --- Add per-voice detune jitter for ensemble width ---
                if ( Math.Abs( pos ) > Constants.VoiceOuterThreshold )
                {
                    detune *= Constants.VoiceOuterDetuneBoost;
                }

                var detuneJitterRnd = SpeechFxFunctions.Hash0To1( [ vPair, 1 ] );
                detune *= 1.0f + ( ( detuneJitterRnd - 0.5f ) * _mod.DetuneJitter );

                // --- LFO frequency ---
                var outerBias = v == 0 || v == ( voiceCount - 1 )
                    ? Constants.OuterLfoBias // outer voices
                    : 1.0f; // inner voices
                var freqBase = SpeechFxFunctions.LinearScale( _fxLevel, 0f, 100f, Constants.LfoMinHz, Constants.LfoMaxHz );
                var freqHz = freqBase * _lfoRateScale * detune * outerBias;

                // --- Depth scaling by position (outer is deeper) ---
                var posScale = Constants.DepthPosMin +
                               ( ( Constants.DepthPosMax - Constants.DepthPosMin ) * Math.Abs( pos ) );
                var depthMs = _depthTargetMs * posScale; // base target

                // --- Randomize depth per voice for richer beating ---
                var depthRnd = SpeechFxFunctions.Hash0To1( [ vPair, 2 ] );
                depthMs *= 1.0f + ( ( depthRnd - 0.5f ) * Constants.DepthJitterPct );
                if ( Math.Abs( pos ) > Constants.VoiceOuterThreshold )
                {
                    depthMs *= Constants.VoiceOuterDepthBoost;
                }

                // --- Micro-comb seeding offsets ---
                // Add tiny, fixed offsets to base delay per voice to seed consistent micro‑comb features that read as “metallic”
                var bias = (float)Math.Pow( Math.Abs( pos ), Constants.MicroCombPower ) * Math.Sign( pos );
                var microOffsetMs = Constants.MicroCombOffset * _microCombScale * bias;

                // --- Base delay jitter ---
                var delayRnd = SpeechFxFunctions.Hash0To1( [ vPair, 3 ] );
                var delayJitter = 1.0f + ( ( delayRnd - 0.5f ) * 2.0f * Constants.DelayJitterPct );

                var baseMs = SpeechFxFunctions.Clamp(
                    ( baseDelaysMs[ v ] * delayJitter ) + microOffsetMs,
                    Constants.MinVoiceDelayMs,
                    Constants.MaxVoiceDelayMs
                );

                // --- Cap depth ---
                var maxDepthByBase = baseDelaysMs[ v ] * Constants.MaxDepthFractionOfBase;
                var maxDepthByAbs = Math.Max( 0f, Constants.MaxVoiceDelayMs - baseMs );
                var maxDepth = Math.Min( maxDepthByBase, maxDepthByAbs );
                depthMs = Math.Max( Math.Min( depthMs, maxDepth ), Constants.DepthFloor );

                // --- Construct voice ---
                var voice = new ChorusVoice( sampleRate, baseMs, depthMs, freqHz, _fxLevel, v );

                // --- Non-linear phase staggering --- denser mid, loose outer
                var phaseNorm = (float)v / ( voiceCount - 1 );
                var asym = (float)Math.Pow( phaseNorm, Constants.LfoPhaseSpreadPower );
                var stagger = asym * Constants.TwoPi;

                // --- Phase jitter / symmetry ---
                // Evenly distributed phases + small jitter to decorrelate; force outer voices symmetrical.
                var phaseJitterRnd = SpeechFxFunctions.Hash0To1( [ vPair, 4 ] );
                var jitter = ( phaseJitterRnd - 0.5f ) * _mod.PhaseJitterRad;
                if ( v == 0 ) // outer voice
                {
                    voice.LfoPhase = Constants.Zero + jitter;
                }
                else if ( v == ( voiceCount - 1 ) ) // outer voice
                {
                    voice.LfoPhase = Constants.Pi - jitter; // mirrored
                }
                else // inner voices
                {
                    voice.LfoPhase = stagger + jitter;
                }

                // --- Add slight phase drift bias for adjacent voices ---
                if ( v > 0 )
                {
                    var lfoPhaseRnd = SpeechFxFunctions.Hash0To1( [ vPair, 5 ] );
                    voice.LfoPhase += ( lfoPhaseRnd - 0.5f ) * Constants.LfoPhaseDrift;
                }

                var weight = GetVoiceWeight( v, _fxLevel );

                // --- Ghost modulation ---
                if ( ghostWeightFactor > 0.0001f && ( v % 2 ) == 0 && v < ( Constants.VoiceCount - 1 ) )
                {
                    // Add subtle random depth variance for richer comb interaction
                    var ghostDepthMs = depthMs * ghostDepthFactor;
                    var ghostFreq = freqHz * Constants.GhostVoiceFreqBias; // slight freq offset
                    var ghostVoice =
                        new ChorusVoice( sampleRate, baseMs + Constants.GhostBaseDelayOffsetMs, ghostDepthMs, ghostFreq,
                            _fxLevel, v ) { LfoPhase = stagger + Constants.GhostVoicePhaseOffset };
                    var ghostVoiceWeight = weight * ghostWeightFactor;
                    _voices.Add( ( ghostVoice, ghostVoiceWeight ) );
                }

                _voices.Add( ( voice, weight ) );
            }
        }

        private void InitializeChorusFilters ( int sampleRate )
        {
            // Body Peaking EQ Filter
            _bodyGainDb = Curve.BodyGainDb( _fxLevel );
            _bodyEq = new BiquadPeakingEqFilter( Constants.BodyShelfHz, _bodyGainDb, Constants.BodyShelfQ, sampleRate );

            // Dry High Pass Filter
            var dryHpfHz = Curve.DryHpfHz(_fxLevel);
            _dryHpf = new TwoPoleHighPassFilter();
            _dryHpf.Set( dryHpfHz, sampleRate );

            // Mud Cut 
            _mudCutDb = Curve.MudCutGainDb( _fxLevel );
            _mudCutEq = new BiquadPeakingEqFilter( Constants.MudCutHz, _mudCutDb, Constants.MudCutQ, sampleRate );

            // Presence cut
            _presenceCutDb = Curve.PresenceCutGainDb( _fxLevel );
            _presenceCutEq = new BiquadPeakingEqFilter(
                Constants.PresenceCutHz,
                _presenceCutDb, 
                Constants.PresenceCutQ, sampleRate );

            // Resonance Filter
            var resLen = Math.Max( 2, (int)( Constants.ResonanceAllPassLenMs * .001f * sampleRate ) );
            var resonanceGain = Curve.ResonanceAllPassGain( _fxLevel );
            _resonanceAllPass = new AllPassFilter( resLen, Constants.ResonanceAllPassGain );
            _resonanceAllPass.SetGain( resonanceGain );
            _lfoRateScale = Curve.LfoRateScale( _fxLevel );
            _microCombScale = Curve.MicroCombScale( _fxLevel );

            // Wet Low Pass Filter
            _wetLpfHz = Curve.WetLpfCutoffHz( _fxLevel );
            _wetLpfShelf = new OnePoleLowPassFilter();
            _wetLpfShelf.Set( _wetLpfHz, sampleRate );

            // Wet High Pass Filter
            _wetHpfHz = Curve.WetHpfHz( _fxLevel );
            _wetHpf = new TwoPoleHighPassFilter();
            _wetHpf.Set( _wetHpfHz, sampleRate );
        }

        private void CalculateVoiceWeightSum ()
        {
            var weightSqSum = 0f;
            for ( var i = 0; i < _voices.Count; i++ )
            {
                var w = _voices[ i ].weight;
                weightSqSum += w * w;
            }

            var denom = (float)Math.Sqrt( ( weightSqSum * Constants.VoiceCorrelationBias ) + 1e-12f );
            _wetNorm = _wetGain / denom;
        }

        protected override float ProcessSample ( float input )
        {
            if ( _voices.Count == 0 || _isBypassed )
            {
                // Pure dry passthrough
                return input;
            }

            // Color / diffuse feedback
            var fbColored = _feedbackShelf.Process( _fbState );
            fbColored = _resonanceAllPass.Process( fbColored );
            fbColored = _feedbackHpf.Process( fbColored );

            // Optional LPF on feedback path
            var fbFiltered = _fxLevel > Constants.FeedbackBypassFxThresh
                ? _feedbackLpf.Process( fbColored )
                : fbColored;

            // Soft-limit the feedback state itself to prevent narrow spikes
            var fbLimited = (float)Math.Tanh( fbFiltered * _fbDrive ) / _fbDrive;

            // Absolute safety ceiling in the feedback loop
            if ( fbLimited > Constants.FeedbackSoftLimitCeiling )
            {
                fbLimited = Constants.FeedbackSoftLimitCeiling;
            }
            else if ( fbLimited < -Constants.FeedbackSoftLimitCeiling )
            {
                fbLimited = -Constants.FeedbackSoftLimitCeiling;
            }

            // Mildly band-limit the dry/input path so deep bass doesn’t “leak around” the wet HPF.
            var dryFiltered = _dryHpf.Process(input);

            // Inject limited feedback into chorus input
            var chorusInput = dryFiltered + (_feedbackAmount * fbLimited);

            // Sum all chorus voices
            var wetSum = 0f;
            foreach ( var (voice, weight) in _voices )
            {
                var wetSample = voice.Process( chorusInput );
                wetSum += weight * wetSample;
            }
            var wetMix = wetSum * _wetNorm;

            // Apply wet shelves
            if ( _fxLevel > 0 )
            {
                // First, enforce the wet low-cut.
                wetMix = _wetHpf.Process( wetMix );

                // Feed the feedback loop from the *audible* wet band (prevents hidden LF in the loop).
                _fbState = wetMix;

                // Then restore “body” and do tone shaping.
                wetMix = _bodyEq.Process( wetMix );
                wetMix = _mudCutEq.Process( wetMix );
                wetMix = _presenceCutEq.Process( wetMix );
                wetMix = _wetLpfShelf.Process( wetMix );
            }
            else
            {
                _fbState = wetMix;
            }

            // Mix dry and wet
            var mixed = (_dryGain * dryFiltered) + wetMix;
            mixed *= _makeupGain;

            // --- soft clip ---
            mixed = SpeechFxFunctions.SoftClipCeiling( mixed, Constants.MixSoftClipCeiling );

            return mixed;
        }

        protected override bool EffectStillActive ()
        {
            // Check if delay buffer is still active
            return _voices.Any( v => v.voice.IsStillActive() );
        }

        private static float GetVoiceWeight ( int voiceIndex, float fxLevel )
        {
            // Spline-based activation curves (smooth, monotonic; no abrupt cuts)
            float act;
            switch ( voiceIndex )
            {
                case 0:
                    act = Curve.Voice0ActSpline.Evaluate( fxLevel );
                    break;
                case 1:
                    act = Curve.Voice1ActSpline.Evaluate( fxLevel );
                    break;
                case 2:
                    act = Curve.Voice2ActSpline.Evaluate( fxLevel );
                    break;
                case 3:
                    act = Curve.Voice3ActSpline.Evaluate( fxLevel );
                    break;
                case 4:
                    act = Curve.Voice4ActSpline.Evaluate( fxLevel );
                    break;
                default:
                    act = Curve.Voice5ActSpline.Evaluate( fxLevel );
                    break;
            }

            return Constants.VoiceBaseWeight * act;
        }
    }
}