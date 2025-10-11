namespace EddiSpeechService.SpeechEffects
{
    using NAudio.Wave;
    using System;

    public class LimiterSampleProvider : EffectSampleProvider
    {
        private readonly float _threshold; // linear threshold (e.g. 0.95f)
        private readonly float _release; // release factor for smoothing
        private float _gain;

        public LimiterSampleProvider ( ISampleProvider source, float thresholdDb = -0.5f, float releaseMs = 10f )
            : base( source )
        {
            // Convert dB threshold to linear
            _threshold = (float)Math.Pow( 10.0, thresholdDb / 20.0 );

            // Release smoothing coefficient
            _release = (float)Math.Exp( -1.0 / ( source.WaveFormat.SampleRate * ( releaseMs / 1000.0 ) ) );

            _gain = 1.0f;
        }

        protected override float ProcessSample ( float input )
        {
            var abs = Math.Abs( input );

            // If above threshold, reduce gain
            if ( abs > _threshold )
            {
                var targetGain = _threshold / abs;
                _gain = Math.Min( _gain, targetGain );
            }
            else
            {
                // Recover gain gradually (release)
                _gain = ( _gain * _release ) + ( ( 1 - _release ) * 1.0f );
            }

            var sample = input * _gain;

            // Clamp just in case
            if ( sample > 1f )
            {
                sample = 1f;
            }

            if ( sample < -1f )
            {
                sample = -1f;
            }

            return sample;
        }
    }
}