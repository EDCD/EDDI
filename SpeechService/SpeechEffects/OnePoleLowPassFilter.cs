using System;

namespace EddiSpeechService.SpeechEffects
{
    /// <summary>
    /// Simple one‑pole low‑pass filter.
    /// Use to smooth chorus/reverb tails and tame high‑frequency shimmer.
    /// </summary>
    public class OnePoleLowPass
    {
        private readonly float _a0;
        private readonly float _b1;
        private float _z1;

        /// <param name="cutoffHz">Cutoff frequency in Hz</param>
        /// <param name="sampleRate">Sample rate in Hz</param>
        public OnePoleLowPass ( float cutoffHz, int sampleRate )
        {
            // Compute exponential smoothing coefficient
            var x = (float)Math.Exp(-2.0 * Math.PI * cutoffHz / sampleRate);

            _a0 = 1.0f - x; // feed‑forward coefficient
            _b1 = x;        // feedback coefficient
            _z1 = 0f;       // filter state
        }

        /// <summary>
        /// Process one sample through the low‑pass filter.
        /// </summary>
        public float Process ( float input )
        {
            _z1 = ( input * _a0 ) + ( _z1 * _b1 );
            return _z1;
        }

        /// <summary>
        /// Reset filter state.
        /// </summary>
        public void Reset ()
        {
            _z1 = 0f;
        }
    }
}