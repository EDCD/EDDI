using NAudio.Wave;
using System;
using System.Linq;

namespace EddiSpeechService.SpeechEffects
{
    public class EchoSampleProvider : EffectSampleProvider
    {
        private const float EchoWetMax = 0.10f;
        
        private readonly float _feedback;
        private readonly float[] _delayBuffer;
        private int _pos;
        private readonly float _wet;
        private readonly float _dry;

        public EchoSampleProvider ( ISampleProvider source, int sampleRate, int fxLevel, int echoDelayMs, float feedback = 0.05f ) : base( source )
        {
            if ( fxLevel <= 0 )
            {
                _delayBuffer = Array.Empty<float>();
                _wet = 0f;
                _dry = 1f;
                _feedback = 0f;
                return;
            }

            var norm = fxLevel / 100f;
            var delaySamples = (int)( sampleRate * ( echoDelayMs / 1000.0 ) );
            _delayBuffer = new float[ delaySamples ];

            _wet = Math.Min( norm, EchoWetMax );
            _dry = 1.0f - _wet;
            _feedback = feedback;
        }

        protected override float ProcessSample ( float input )
        {
            if ( _delayBuffer.Length == 0 )
            {
                return input;
            }

            var delayed = _delayBuffer[ _pos ];
            var output = ( _dry * input ) + ( _wet * delayed );

            _delayBuffer[ _pos ] = input + ( delayed * _feedback );

            if ( ++_pos >= _delayBuffer.Length )
            {
                _pos = 0;
            }

            return output;
        }

        protected override bool EffectStillActive ()
        {
            // Check if delay buffer still has energy
            return _delayBuffer.Any( s => Math.Abs( s ) > 1e-4f );
        }
    }
}