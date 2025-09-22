using NAudio.Wave;
using NWaves.Filters.BiQuad;
using System;
using System.Linq;

namespace EddiSpeechService.SpeechEffects
{
    public class ReverbSampleProvider : EffectSampleProvider
    {
        private readonly int _reverbTimeMs;
        private readonly double _dryGain;
        private readonly double _wetGain;
        
        private readonly FeedbackComb[] _combs;
        private readonly AllPassFilter[] _allpasses;
        private readonly OnePoleHighCut _onePoleHighCut;
        private readonly PeakingEQ _midShelf;

        public ReverbSampleProvider ( ISampleProvider source, int sampleRate, int fxLevel, int damageAdjustedFxLevel ) : base( source )
        {
            // Map damageAdjustedFxLevel to the decay time (0–1000 ms)
            _reverbTimeMs = 10 * damageAdjustedFxLevel;

            // Map fxLevel to the wet/dry mix
            var theta = fxLevel / 100.0 * (Math.PI / 2);
            var wetGain = Math.Sin(theta) * 2;
            var dryGain = Math.Cos(theta) / 2;

            // Normalize so dry+wet = 1.0
            var norm = 1.0 / ( wetGain + dryGain );
            _wetGain = wetGain * norm;
            _dryGain = dryGain * norm;

            // Feedback coefficient tied to decay time
            var reverbTimeSec = _reverbTimeMs / 1000.0;
            var feedback = Math.Exp( -3.0 / reverbTimeSec );
            feedback = Math.Min( feedback, 0.88 ); // clamp for stability

            // Bandpass filter to boost the mids
            var bandpass = new BandPassFilter(1000f, 0.8f, sampleRate);

            // Fixed comb delays (ms)
            int[] combDelaysMs = { 29, 31, 37, 41, 67 };
            _combs = combDelaysMs
                .Select( d => new FeedbackComb( (int)( sampleRate * d / 1000.0 ), (float)feedback, bandpass ) )
                .ToArray();

            // Diffusion all‑passes (ms)
            int[] allPassDelaysMs = { 5, 7, 11 };
            _allpasses = allPassDelaysMs
                .Select( d => new AllPassFilter( (int)( sampleRate * d / 1000.0 ), 0.5f ) )
                .ToArray();

            _onePoleHighCut = new OnePoleHighCut( 7000, sampleRate );

            _midShelf = new PeakingEQ( 1000f, 0.7f, 3.0f, sampleRate );
            // center 1 kHz, Q=0.7, +3 dB boost
        }

        protected override float ProcessSample ( float input )
        {
            if ( _reverbTimeMs == 0 )
            {
                return input;
            }
            
            // Parallel combs
            var wet = 0f;
            foreach ( var comb in _combs )
            {
                wet += comb.Process( input );
            }
            wet /= _combs.Length;

            // Diffusion
            foreach ( var ap in _allpasses )
            {
                wet = ap.Process( wet );
            }

            // Remove any high-end "fizz" from the output
            wet = _onePoleHighCut.Process( wet );
            
            // Post-EQ mid boost (shelving filter, e.g. +3 dB @ 1 kHz)
            wet = _midShelf.Process( wet );

            return (float)( ( _dryGain * input ) + ( _wetGain * wet ) );
        }

        // --- Internal DSP building blocks ---

        private class FeedbackComb
        {
            private readonly float[] _buffer;
            private int _pos;
            private readonly float _feedback;
            private readonly BandPassFilter _bpf;

            public FeedbackComb ( int delaySamples, float feedback, BandPassFilter bpf )
            {
                _buffer = new float[ delaySamples ];
                _feedback = feedback;
                _bpf = bpf;
            }

            public float Process ( float input )
            {
                var y = _buffer[ _pos ];
                var fb = _bpf.Process( y * _feedback );
                var output = y + input;

                _buffer[ _pos ] = input + fb;

                if ( ++_pos >= _buffer.Length )
                {
                    _pos = 0;
                }

                return output;
            }

            public bool IsStillActive ( float threshold = 1e-4f )
            {
                return _buffer.Any( t => Math.Abs( t ) > threshold );
            }
        }

        private class BandPassFilter
        {
            private readonly float _a0, _a1, _a2, _b1, _b2;
            private float _z1, _z2;

            public BandPassFilter ( float centerFreqHz, float q, int sampleRate )
            {
                var w0 = 2.0f * (float)Math.PI * centerFreqHz / sampleRate;
                var alpha = (float)Math.Sin(w0) / (2.0f * q);

                var cosw0 = (float)Math.Cos(w0);

                _a0 = alpha;
                _a1 = 0;
                _a2 = -alpha;
                var a0Norm = 1 + alpha;

                _b1 = -2 * cosw0 / a0Norm;
                _b2 = ( 1 - alpha ) / a0Norm;

                _a0 /= a0Norm;
                _a1 /= a0Norm;
                _a2 /= a0Norm;
            }

            public float Process ( float input )
            {
                var output = (_a0 * input) + (_a1 * _z1) + (_a2 * _z2) - (_b1 * _z1) - (_b2 * _z2);
                _z2 = _z1;
                _z1 = output;
                return output;
            }
        }

        protected override bool EffectStillActive ()
        {
            // Check if delay buffer is still active
            return _combs.Any( c => c.IsStillActive() );
        }
    }
}