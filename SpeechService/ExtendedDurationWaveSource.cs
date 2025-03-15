using CSCore;
using System;
using Utilities;

namespace EddiSpeechService
{
    public class ExtendedDurationWaveSource : WaveAggregatorBase
    {
        private int bytesToExtend;
        public override long Length { get; }

        public ExtendedDurationWaveSource(IWaveSource waveSource, int milliSecondsToExtend) : base(waveSource)
        {
            try
            {
                bytesToExtend = (int)waveSource.GetRawElements( milliSecondsToExtend );
            }
            catch ( Exception ex )
            {
                Logging.Warn( "Unable to extend wave source duration", ex );
            }
            Length = base.Length + bytesToExtend;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = BaseSource.Read(buffer, offset, count);
            if (read < count)
            {
                int diff = count - read;

                diff = Math.Min(diff, bytesToExtend);
                if (diff > 0)
                {
                    Array.Clear(buffer, offset, diff);
                }

                bytesToExtend -= diff;

                return diff;
            }
            return read;
        }
    }
}
