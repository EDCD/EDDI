using System;

namespace EddiSpeechService.Filters
{
    public class OnePoleHighPassFilter
    {
        private float _a, _b, _z;
        private bool _enabled;

        public void Set ( float fc, float fs )
        {
            if ( fc <= 0f )
            {
                _enabled = false;
                _a = 1f;
                _b = 0f;
                return;
            }

            _enabled = true;
            fc = Clamp( fc, 20f, 20000f );
            var x = (float)Math.Exp( -2.0 * Math.PI * fc / fs );
            _a = 1f - x;
            _b = x;
        }

        public float Process ( float x )
        {
            if ( !_enabled )
            { return x; }

            // Lowpass state
            _z = ( _a * x ) + ( _b * _z );

            // Highpass = input - lowpassed
            return x - _z;
        }

        private static float Clamp ( float value, float min, float max )
        {
            if ( value < min )
            { return min; }
            return value > max ? max : value;
        }
    }
}