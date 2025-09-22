using NAudio.Dsp;
using NAudio.Wave;

namespace EddiSpeechService.SpeechEffects
{
    public class RadioSampleProvider : EffectSampleProvider
    {
        private readonly BiQuadFilter _highpass;
        private readonly NWaves.Effects.DistortionEffect _distortion;

        public RadioSampleProvider ( ISampleProvider source, int sampleRate )
            : base( source )
        {
            // High‑pass filter at ~1 kHz, Q=1
            _highpass = BiQuadFilter.HighPassFilter( sampleRate, 1015, 1 );

            // Distortion with fixed input/output gains
            var inputGain = 15;
            var outputGain = -10;
            _distortion = new NWaves.Effects.DistortionEffect(
                NWaves.Effects.DistortionMode.HardClipping,
                inputGain,
                outputGain );
            _distortion.WetDryMix( 0.9f );
        }

        protected override float ProcessSample ( float input )
        {
            // Step 1: high‑pass filter
            var hp = _highpass.Transform(input);

            // Step 2: distortion
            return _distortion.Process( hp );
        }
    }
}