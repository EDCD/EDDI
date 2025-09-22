using System;

namespace EddiSpeechService.SpeechEffects
{
    public class PeakingEQ
    {
        private readonly float _a0, _a1, _a2, _b1, _b2;
        private float _z1, _z2;

        public PeakingEQ ( float centerFreqHz, float q, float gainDb, int sampleRate )
        {
            var A = (float)Math.Pow(10, gainDb / 40.0); // linear gain
            var w0 = 2.0f * (float)Math.PI * centerFreqHz / sampleRate;
            var alpha = (float)Math.Sin(w0) / (2.0f * q);
            var cosw0 = (float)Math.Cos(w0);

            var b0 = 1 + (alpha * A);
            var b1 = -2 * cosw0;
            var b2 = 1 - (alpha * A);
            var a0 = 1 + (alpha / A);
            var a1 = -2 * cosw0;
            var a2 = 1 - (alpha / A);

            _a0 = b0 / a0;
            _a1 = b1 / a0;
            _a2 = b2 / a0;
            _b1 = a1 / a0;
            _b2 = a2 / a0;
        }

        public float Process ( float input )
        {
            var output = (_a0 * input) + (_a1 * _z1) + (_a2 * _z2) - (_b1 * _z1) - (_b2 * _z2);
            _z2 = _z1;
            _z1 = output;
            return output;
        }
    }
}