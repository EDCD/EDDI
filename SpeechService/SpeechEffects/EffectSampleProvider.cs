using NAudio.Wave;

namespace EddiSpeechService.SpeechEffects
{
    public abstract class EffectSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private bool _sourceEnded;

        // Add a small ramp (fade-in) for effects
        private const double rampMs = .010; // 10 ms
        private readonly int _rampSamples;
        private int _rampPos;
        private float[] _temp; // reuse to avoid per-call allocation

        protected EffectSampleProvider ( ISampleProvider source )
        {
            _source = source;
            var sr = source.WaveFormat.SampleRate;
            _rampSamples = (int)( rampMs * sr ); // 10 ms
            _rampPos = 0;
            _temp = new float[ 4096 ];
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read ( float[] buffer, int offset, int count )
        {
            // Reuse temp buffer; resize if needed
            if ( _temp.Length < count )
            {
                _temp = new float[ count ];
            }

            var samplesRead = _source.Read(_temp, 0, count);

            if ( samplesRead == 0 && !_sourceEnded )
            {
                // Upstream ended, but effect may still have tail
                _sourceEnded = true;
            }

            if ( _sourceEnded )
            {
                if ( EffectStillActive() )
                {
                    // Keep clocking effect with silence
                    for ( var n = 0; n < count; n++ )
                    {
                        buffer[ offset + n ] = ApplyStartRamp( ProcessSample( 0f ) );
                    }

                    return count;
                }

                // Tail is gone, now we’re truly finished
                return 0;
            }

            for ( var n = 0; n < samplesRead; n++ )
            {
                buffer[ offset + n ] = ApplyStartRamp( ProcessSample( _temp[ n ] ) );
            }

            return samplesRead;
        }

        private float ApplyStartRamp ( float y )
        {
            if ( _rampPos < _rampSamples )
            {
                var t = (float)_rampPos / _rampSamples; // 0→1
                _rampPos++;
                return y * t;
            }
            return y;
        }

        /// <summary>
        /// Override this in derived classes to implement per‑sample DSP.
        /// </summary>
        protected abstract float ProcessSample ( float input );

        /// <summary>
        /// Override this in a derived class to indicate whether the effect is still active
        /// </summary>
        protected virtual bool EffectStillActive () => false;
    }
}