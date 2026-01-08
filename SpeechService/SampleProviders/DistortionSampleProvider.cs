using NAudio.Wave;

namespace EddiSpeechService.SampleProviders
{
    // Adds "fuzz" to the output based on the distortion level
    public class DistortionSampleProvider : EffectSampleProvider
    {
        private readonly NWaves.Effects.DistortionEffect _distortion;

        public DistortionSampleProvider ( ISampleProvider source, int distortionLevel )
            : base( source )
        {
            if ( distortionLevel != 0 )
            {
                var inputGain = distortionLevel / 100f * 30;
                var outputGain = distortionLevel / 100f * -25;
                var distortionMode = NWaves.Effects.DistortionMode.HardClipping;
                _distortion = new NWaves.Effects.DistortionEffect( distortionMode, inputGain, outputGain );
                _distortion.WetDryMix( 0.9f );
            }
        }

        protected override float ProcessSample ( float input )
        {
            if ( _distortion == null )
            {
                return input;
            }

            return _distortion.Process( input );
        }
    }
}