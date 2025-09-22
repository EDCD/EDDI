using NAudio.Wave;

namespace EddiSpeechService.SpeechEffects
{
    public abstract class EffectSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private bool _sourceEnded;

        protected EffectSampleProvider ( ISampleProvider source )
        {
            _source = source;
        }

        public WaveFormat WaveFormat => _source.WaveFormat;

        public int Read ( float[] buffer, int offset, int count )
        {
            var temp = new float[count];
            var samplesRead = _source.Read(temp, 0, count);

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
                        buffer[ offset + n ] = ProcessSample( 0f );
                    }

                    return count;
                }

                // Tail is gone, now we’re truly finished
                return 0;
            }

            for ( var n = 0; n < samplesRead; n++ )
            {
                buffer[ offset + n ] = ProcessSample( temp[ n ] );
            }

            return samplesRead;
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