using CSCore;
using CSCore.DSP;
using CSCore.Streams.Effects;
using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiSpeechService
{
    public partial class SpeechService
    {
        private IWaveSource addEffectsToSource(IWaveSource source, int chorusLevel, int reverbLevel, int echoDelay, int distortionLevel, bool radio)
        {
            // Effects level is increased by damage if distortion is enabled
            int effectsLevel = fxLevel(distortionLevel);

            // Add various effects...
            Logging.Debug("Effects level is " + effectsLevel + ", chorus level is " + chorusLevel + ", reverb level is " + reverbLevel + ", echo delay is " + echoDelay);

            // We need to extend the duration of the wave source if we have any effects going on
            if (chorusLevel != 0 || reverbLevel != 0 || echoDelay != 0)
            {
                // Add a base of 500ms plus 10ms per effect level over 50
                Logging.Debug("Extending duration by " + 500 + Math.Max(0, (effectsLevel - 50) * 10) + "ms");
                source = source.AppendSource(x => new ExtendedDurationWaveSource(x, 500 + Math.Max(0, (effectsLevel - 50) * 10)));
            }

            // We always have chorus
            if (chorusLevel != 0)
            {
                source = source.AppendSource(x => new DmoChorusEffect(x) { Depth = chorusLevel, WetDryMix = Math.Min(100, (int)(180 * (effectsLevel) / ((decimal)100))), Delay = 16, Frequency = (effectsLevel / 10), Feedback = 25 });
            }

            // We only have reverb and echo if we're not transmitting or receiving
            if (!radio)
            {
                if (reverbLevel != 0)
                {
                    source = source.AppendSource(x => new DmoWavesReverbEffect(x) { ReverbTime = (int)(1 + 999 * (effectsLevel) / ((decimal)100)), ReverbMix = Math.Max(-96, -96 + (96 * reverbLevel / 100)) });
                }

                if (echoDelay != 0)
                {
                    source = source.AppendSource(x => new DmoEchoEffect(x) { LeftDelay = echoDelay, RightDelay = echoDelay, WetDryMix = Math.Max(5, (int)(10 * (effectsLevel) / ((decimal)100))), Feedback = 0 });
                }
            }
            // Apply a high pass filter for a radio effect
            else
            {
                var sampleSource = source.ToSampleSource().AppendSource(x => new BiQuadFilterSource(x));
                sampleSource.Filter = new HighpassFilter(source.WaveFormat.SampleRate, 1015);
                source = sampleSource.ToWaveSource();
            }

            // Adjust gain
            if (effectsLevel != 0 && chorusLevel != 0)
            {
                int radioGain = radio ? 7 : 0;
                source = source.AppendSource(x => new DmoCompressorEffect(x) { Gain = effectsLevel / 15 + radioGain });
            }

            return source;
        }

        public EddiSpeech GetSpeechFX(EddiSpeech speech)
        {
            Ship ship = speech.ship;
            speech.echoDelay = echoDelayForShip(ship);
            speech.chorusLevel = chorusLevelForShip(ship);
            speech.reverbLevel = reverbLevelForShip(ship);
            speech.distortionLevel = distortionLevelForShip(ship);
            speech.compressionLevel = 0;
            return speech;
        }

        private int echoDelayForShip(Ship ship)
        {
            // this is affected by ship size
            int echoDelay = 50; // Default
            if (ship != null)
            {
                if (ship.size == "Small")
                {
                    echoDelay = 50;
                }
                else if (ship.size == "Medium")
                {
                    echoDelay = 100;
                }
                else if (ship.size == "Large")
                {
                    echoDelay = 200;
                }
                else if (ship.size == "Huge")
                {
                    echoDelay = 400;
                }
            }
            return echoDelay;
        }

        private int chorusLevelForShip(Ship ship)
        {
            // This may be affected by ship parameters
            return (int)(60 * (Math.Max(fxLevel(distortionLevelForShip(ship)), (decimal)configuration.EffectsLevel) / (decimal)100));
        }

        private int reverbLevelForShip(Ship ship)
        {
            // This is not affected by ship parameters
            return (int)(80 * ((decimal)configuration.EffectsLevel) / ((decimal)100));
        }

        private int distortionLevelForShip(Ship ship)
        {
            // This is affected by ship health
            int distortionLevel = 0;
            if (ship != null && configuration.DistortOnDamage)
            {
                distortionLevel = (100 - (int)ship.health);
            }
            return distortionLevel;
        }

        private int fxLevel(decimal distortionLevel)
        {
            // Effects level is increased by damage if distortion is enabled
            int distortionFX = 0;
            if (distortionLevel > 0)
            {
                distortionFX = (int)Decimal.Round(((decimal)distortionLevel / 100) * (100 - configuration.EffectsLevel));
                Logging.Debug("Calculating effect of distortion on speech effects: +" + distortionFX);
            }
            return configuration.EffectsLevel + distortionFX;
        }
    }

    public class BiQuadFilterSource : SampleAggregatorBase
    {
        private readonly object _lockObject = new object();
        private BiQuad _biquad;

        public BiQuad Filter
        {
            get { return _biquad; }
            set
            {
                lock (_lockObject)
                {
                    _biquad = value;
                }
            }
        }

        public BiQuadFilterSource(ISampleSource source) : base(source)
        {
        }

        public override int Read(float[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            lock (_lockObject)
            {
                if (Filter != null)
                {
                    for (int i = 0; i < read; i++)
                    {
                        buffer[i + offset] = Filter.Process(buffer[i + offset]);
                    }
                }
            }

            return read;
        }
    }
}
