using System;

namespace EddiSpeechService.SpeechEffects
{
    public class BiquadLowShelf
    {
        private readonly float a0, a1, a2, b1, b2;
        private float x1, x2, y1, y2;

        public BiquadLowShelf ( float cutoffHz, float gainDb, float q, int sampleRate )
        {
            var A = (float)Math.Pow( 10, gainDb / 40.0 );
            var w0 = 2f * (float)Math.PI * cutoffHz / sampleRate;
            var cosw0 = (float)Math.Cos( w0 );
            var sinw0 = (float)Math.Sin( w0 );
            var alpha = sinw0 / ( 2f * q );
            var sqrtA = (float)Math.Sqrt( A );

            var b0 = A * ( A + 1f - ( ( A - 1f ) * cosw0 ) + ( 2f * sqrtA * alpha ) );
            var b1n = 2f * A * ( A - 1f - ( ( A + 1f ) * cosw0 ) );
            var b2n = A * ( A + 1f - ( ( A - 1f ) * cosw0 ) - ( 2f * sqrtA * alpha ) );
            var a0n = A + 1f + ( ( A - 1f ) * cosw0 ) + ( 2f * sqrtA * alpha );
            var a1n = -2f * ( A - 1f + ( ( A + 1f ) * cosw0 ) );
            var a2n = A + 1f + ( ( A - 1f ) * cosw0 ) - ( 2f * sqrtA * alpha );

            a0 = b0 / a0n;
            a1 = b1n / a0n;
            a2 = b2n / a0n;
            b1 = a1n / a0n;
            b2 = a2n / a0n;
        }

        public float Process ( float x )
        {
            var y = ( a0 * x ) + ( a1 * x1 ) + ( a2 * x2 ) - ( b1 * y1 ) - ( b2 * y2 );
            x2 = x1;
            x1 = x;
            y2 = y1;
            y1 = y;
            return y;
        }
    }
}