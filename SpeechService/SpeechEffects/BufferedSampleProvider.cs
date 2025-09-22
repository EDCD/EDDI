using NAudio.Wave;
using System;

namespace EddiSpeechService.SpeechEffects
{
    public class BufferedSampleProvider : ISampleProvider
    {
        private readonly float[] _samples;
        private int _position;

        public WaveFormat WaveFormat { get; }

        public BufferedSampleProvider ( float[] samples, int sampleRate, int channels = 1 )
        {
            _samples = samples ?? throw new ArgumentNullException( nameof( samples ) );
            _position = 0;
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat( sampleRate, channels );
        }

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