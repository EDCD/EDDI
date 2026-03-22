using NAudio.Wave;
using System;

namespace EddiSpeechService.SampleProviders
{
    public class BufferedSampleProvider ( float[] samples, int sampleRate, int channels = 1 ) : ISampleProvider
    {
        private readonly float[] _samples = samples ?? throw new ArgumentNullException( nameof( samples ) );
        private int _position = 0;

        public WaveFormat WaveFormat { get; } = WaveFormat.CreateIeeeFloatWaveFormat( sampleRate, channels );

        public int Read ( float[] buffer, int offset, int count )
        {
            var available = _samples.Length - _position;
            var toCopy = Math.Min(available, count);

            if ( toCopy > 0 )
            {
                Array.Copy( _samples, _position, buffer, offset, toCopy );
                _position += toCopy;
            }

            return toCopy;
        }
    }
}