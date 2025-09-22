using NAudio.Wave;
using System;

namespace EddiSpeechService.SpeechEffects
{
    public class NormalizeSampleProvider : EffectSampleProvider
    {
        private readonly float _volumeScale;
        private readonly float _normFactor;

        public NormalizeSampleProvider ( ISampleProvider source, int targetVolume ) : base( source )
        {
            // Pre‑scan to find peak
            var max = 0f;

            // If source is a WaveStream, we can scan and rewind
            if ( source is WaveStream ws && ws.CanSeek )
            {
                var pos = ws.Position;
                var scanBuffer = new float[4096];
                int read;
                var sp = ws.ToSampleProvider();

                do
                {
                    read = sp.Read( scanBuffer, 0, scanBuffer.Length );
                    for ( var i = 0; i < read; i++ )
                    {
                        var abs = Math.Abs(scanBuffer[i]);
                        if ( abs > max )
                            max = abs;
                    }
                } while ( read > 0 );

                ws.Position = pos; // rewind
            }
            else
            {
                // Non‑seekable: skip normalization, just volume scale
                max = 1f;
            }

            // Compute normalization factor
            _normFactor = max > 0 ? 0.95f / max : 1f;

            // Volume scaling (0–100%)
            _volumeScale = Math.Max( 0, Math.Min( 100, targetVolume ) ) / 100f;
        }

        protected override float ProcessSample ( float input )
        {
            var sample = input * _normFactor * _volumeScale;

            if ( float.IsNaN( sample ) || float.IsInfinity( sample ) )
                sample = 0f;

            // Clamp
            if ( sample > 1f )
                sample = 1f;
            if ( sample < -1f )
                sample = -1f;

            return sample;
        }
    }
}