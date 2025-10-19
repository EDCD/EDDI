using System;

namespace EddiSpeechService.SpeechEffects
{
    public class OnePoleLowPassFilter
    {
        private float a, b, z;

        public void Set ( float fc, float fs )
        {
            fc = Clamp( fc, 20f, 20000f );
            var x = (float)Math.Exp( -2.0 * Math.PI * fc / fs );
            a = 1f - x;
            b = x;
        }

        public float Process ( float x )
        {
            z = ( a * x ) + ( b * z );
            return z;
        }

        private static float Clamp ( float value, float min, float max )
        {
            if ( value < min ) { return min; }

            return value > max ? max : value;
        }
    }
}