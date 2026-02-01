using System;
    
namespace EddiSpeechService.Filters
{
    public sealed class BiquadPeakingEqFilter
    {
        private float _b0, _b1, _b2, _a1, _a2;
        private float _z1, _z2;

        public BiquadPeakingEqFilter ( float centerHz, float gainDb, float q, int sampleRate )
        {
            Set( centerHz, q, gainDb, sampleRate );
        }

        public void Set ( float centerHz, float q, float gainDb, int sampleRate )
        {
            // RBJ Audio EQ Cookbook (peaking EQ)
            var A = Math.Pow( 10.0, gainDb / 40.0 );
            var w0 = 2.0 * Math.PI * centerHz / sampleRate;
            var cosw0 = Math.Cos( w0 );
            var sinw0 = Math.Sin( w0 );
            var alpha = sinw0 / ( 2.0 * q );

            var b0 = 1.0 + (alpha * A);
            var b1 = -2.0 * cosw0;
            var b2 = 1.0 - (alpha * A);
            var a0 = 1.0 + (alpha / A);
            var a1 = -2.0 * cosw0;
            var a2 = 1.0 - (alpha / A);

            // Normalize
            _b0 = (float)( b0 / a0 );
            _b1 = (float)( b1 / a0 );
            _b2 = (float)( b2 / a0 );
            _a1 = (float)( a1 / a0 );
            _a2 = (float)( a2 / a0 );
        }

        public float Process ( float x )
        {
            var y = ( _b0 * x ) + _z1;
            _z1 = ( _b1 * x ) - ( _a1 * y ) + _z2;
            _z2 = ( _b2 * x ) - ( _a2 * y );
            return y;
        }

        public void Reset ()
        {
            _z1 = 0f;
            _z2 = 0f;
        }
    }
}