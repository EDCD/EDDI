using System;

namespace EddiSpeechService.SpeechEffects
{
    public class BiquadLowShelf
    {
        private readonly float _a0, _a1, _a2, _b1, _b2;
        private float _z1, _z2;

        public BiquadLowShelf ( float cutoffHz, float gainDb, float q, int sampleRate )
        {
            var A = (float)Math.Pow( 10, gainDb / 40.0 );
            var w0 = 2.0f * (float)Math.PI * cutoffHz / sampleRate;
            var cosw0 = (float)Math.Cos( w0 );
            var sinw0 = (float)Math.Sin( w0 );
            var alpha = sinw0 / ( 2.0f * q );
            var sqrtA = (float)Math.Sqrt( A );

            // Low-shelf coefficients
            var b0 = A * ( A + 1f - ( ( A - 1f ) * cosw0 ) + ( 2f * sqrtA * alpha ) );
            var b1 = 2f * A * ( A - 1f - ( ( A + 1f ) * cosw0 ) );
            var b2 = A * ( A + 1f - ( ( A - 1f ) * cosw0 ) - ( 2f * sqrtA * alpha ) );
            var a0 = A + 1f + ( ( A - 1f ) * cosw0 ) + ( 2f * sqrtA * alpha );
            var a1 = -2f * ( A - 1f + ( ( A + 1f ) * cosw0 ) );
            var a2 = A + 1f + ( ( A - 1f ) * cosw0 ) - ( 2f * sqrtA * alpha );

            _a0 = b0 / a0;
            _a1 = b1 / a0;
            _a2 = b2 / a0;
            _b1 = a1 / a0;
            _b2 = a2 / a0;
        }

        public float Process ( float input )
        {
            var output = ( _a0 * input ) + ( _a1 * _z1 ) + ( _a2 * _z2 ) - ( _b1 * _z1 ) - ( _b2 * _z2 );
            _z2 = _z1;
            _z1 = output;
            return output;
        }
    }
}