using NAudio.Wave;
using System;

namespace EddiSpeechService.SpeechEffects
{
    public class ExtendedDurationWaveProvider : IWaveProvider
    {
        private readonly IWaveProvider source;
        private int extensionBytesRemaining;
        private bool sourceDepleted;

        public ExtendedDurationWaveProvider ( IWaveProvider source, int extensionMilliseconds )
        {
            this.source = source;
            this.WaveFormat = source.WaveFormat;
            var extensionBytes1 = (int)( WaveFormat.AverageBytesPerSecond * extensionMilliseconds / 1000.0 );
            this.extensionBytesRemaining = extensionBytes1;
            this.sourceDepleted = false;
        }

        public WaveFormat WaveFormat { get; }

        public int Read ( byte[] buffer, int offset, int count )
        {
            if ( !sourceDepleted )
            {
                var read = source.Read(buffer, offset, count);
                if ( read < count )
                {
                    sourceDepleted = true;
                    extensionBytesRemaining -= ( count - read );
                    // Fill the rest with silence
                    Array.Clear( buffer, offset + read, count - read );
                    return count;
                }
                return read;
            }

            if ( extensionBytesRemaining > 0 )
            {
                var toWrite = Math.Min(count, extensionBytesRemaining);
                Array.Clear( buffer, offset, toWrite );
                extensionBytesRemaining -= toWrite;
                return toWrite;
            }

            return 0;
        }
    }
}
