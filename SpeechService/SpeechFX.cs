﻿using CSCore;
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
                int extMs = 500 + Math.Max(0, (effectsLevel - 50) * 10);
                Logging.Debug("Extending duration by " + extMs + "ms");
                source = source.AppendSource(x => new ExtendedDurationWaveSource(x, extMs));
            }

            // We always apply chorus effects.
            if (chorusLevel != 0)
            {
                // The "wetDryMix" mix is the percent of added chorus, with 0 indicating no added chorus.
                const int delay = 16;
                const int feedback = 25;
                float wetDryMix = Math.Min(100, (int)(180 * effectsLevel / (decimal)100));
                float frequency = (effectsLevel / 10);
                source = source.AppendSource(x => new DmoChorusEffect(x) { Depth = chorusLevel, WetDryMix = wetDryMix, Delay = delay, Frequency = frequency, Feedback = feedback });
            }

            // Apply a high pass filter for a radio effect
            if (radio)
            {
                var sampleSource = source.ToSampleSource().AppendSource(x => new BiQuadFilterSource(x));
                sampleSource.Filter = new HighpassFilter(source.WaveFormat.SampleRate, 1015);
                source = sampleSource.ToWaveSource();
            }
            // We only have reverb and echo if we're not transmitting or receiving
            else
            {
                if (reverbLevel != 0 && effectsLevel != 0)
                {
                    float reverbTime = (int)(1 + (999 * effectsLevel / (decimal)100));
                    float reverbMix = Math.Max(-96, -96 + (96 * reverbLevel / 100));
                    source = source.AppendSource(x => new DmoWavesReverbEffect(x) { ReverbTime = reverbTime, ReverbMix = reverbMix });
                }

                if (echoDelay != 0 && effectsLevel != 0)
                {
                    // The "wetDryMix" mix is the percent of added echo, with 0 indicating no added echo.
                    const int feedback = 0;
                    float wetDryMix = Math.Max(5, (int)(10 * effectsLevel / (decimal)100));
                    source = source.AppendSource(x => new DmoEchoEffect(x) { LeftDelay = echoDelay, RightDelay = echoDelay, WetDryMix = wetDryMix, Feedback = feedback });
                }
            }

            // Adjust gain
            const int standardGain = 10;
            int radioGain = radio ? 7 : 0;
            source = source.AppendSource(x => new DmoCompressorEffect(x) { Gain = (effectsLevel / 15) + radioGain + standardGain });

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
            int echoDelay = 0; // Default
            if (ship != null)
            {
                if (ship.Size == LandingPadSize.Small)
                {
                    echoDelay = 50;
                }
                else if (ship.Size == LandingPadSize.Medium)
                {
                    echoDelay = 100;
                }
                else if (ship.Size == LandingPadSize.Large)
                {
                    echoDelay = 200;
                }
            }
            return echoDelay;
        }

        private int chorusLevelForShip(Ship ship)
        {
            // This may be affected by ship parameters
            return (int)(60 * (Math.Max(fxLevel(distortionLevelForShip(ship)), (decimal)Configuration.EffectsLevel) / 100M));
        }

        private int reverbLevelForShip(Ship ship)
        {
            // This is not affected by ship parameters
            return (int)(80 * ((decimal)Configuration.EffectsLevel) / 100M);
        }

        private int distortionLevelForShip(Ship ship)
        {
            // This is affected by ship health
            int distortionLevel = 0;
            if (ship != null && Configuration.DistortOnDamage)
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
                distortionFX = (int)Decimal.Round((distortionLevel / 100M) * (100M - Configuration.EffectsLevel));
                Logging.Debug("Calculating effect of distortion on speech effects: +" + distortionFX);
            }
            return Configuration.EffectsLevel + distortionFX;
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
