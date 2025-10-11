using System;

namespace EddiSpeechService.SpeechEffects
{
    public class OnePoleHighShelf
    {
        private readonly float _a0, _b1;
        private float _z1;
        private float _gain;

        public OnePoleHighShelf ( float cutoffHz, float gainDb, int sampleRate )
        {
            var x = (float)Math.Exp(-2.0 * Math.PI * cutoffHz / sampleRate);
            _a0 = 1 - x;
            _b1 = x;
            _gain = (float)Math.Pow( 10, gainDb / 20.0 ); // linear gain
        }

        public float Process ( float input )
        {
            // Apply shelf: boost difference between input and smoothed signal
            var smoothed = (_a0 * input) + (_b1 * _z1);
            _z1 = smoothed;
            return input + ( ( input - smoothed ) * ( _gain - 1 ) );
        }

        public void SetGainDb ( float gainDb )
        {
            _gain = (float)Math.Pow( 10, gainDb / 20.0 );
        }
    }
}