using System;

namespace EddiSpeechService.SpeechEffects
{
    public class OnePoleHighCut
    {
        private readonly float _a0, _b1;
        private float _z1;

        public OnePoleHighCut ( float cutoffHz, int sampleRate )
        {
            var x = (float)Math.Exp(-2.0 * Math.PI * cutoffHz / sampleRate);
            _a0 = 1 - x;
            _b1 = x;
        }

        public float Process ( float input )
        {
            _z1 = ( input * _a0 ) + ( _z1 * _b1 );
            return _z1;
        }
    }
}