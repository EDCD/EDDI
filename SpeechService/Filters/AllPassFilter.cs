using System;

namespace EddiSpeechService.Filters
{
    public class AllPassFilter
    {
        private readonly float[] _buf;
        private int _idx;
        private float _g;

        public AllPassFilter ( int samples, float g = 0.5f )
        {
            _buf = new float[ Math.Max( 2, samples ) ];
            _idx = 0;
            _g = Clamp( g, -0.7f, 0.7f );
        }

        public void SetGain ( float g ) => _g = Clamp( g, -0.7f, 0.7f );

        public float Process ( float x )
        {
            var y = ( -_g * x ) + _buf[ _idx ];
            _buf[ _idx ] = x + ( _g * y );
            if ( ++_idx >= _buf.Length ) _idx = 0;
            return y;
        }

        private static float Clamp ( float value, float min, float max )
        {
            if ( value < min )
            {
                return min;
            }

            return value > max ? max : value;
        }
    }
}