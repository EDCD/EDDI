using EddiSpeechService.Filters;
using EddiSpeechService.SampleProviders.ChorusHelpers;
using NAudio.Wave;

namespace EddiSpeechService.SampleProviders
{
    public sealed class BaselineEqSampleProvider : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _channels;

        private readonly BiquadPeakingEqFilter[] _boost125;
        private readonly BiquadPeakingEqFilter[] _boost250;
        private readonly BiquadPeakingEqFilter[] _boost500;
        private readonly BiquadPeakingEqFilter[] _boost1k;
        private readonly BiquadPeakingEqFilter[] _boost2k;
        private readonly BiquadPeakingEqFilter[] _boost4k;
        private readonly BiquadPeakingEqFilter[] _boost8k;
        private readonly BiquadHighShelfFilter[] _hiShelf;

        public WaveFormat WaveFormat => _source.WaveFormat;

        public BaselineEqSampleProvider ( ISampleProvider source, int sampleRate, int fxLevel )
        {
            _source = source;
            _channels = source.WaveFormat.Channels;

            var trim4kDb = SpeechFxFunctions
                .SmoothSplineClamped( Constants.Baseline4kTrimDbFx, Constants.Baseline4kTrimDbY, fxLevel );
            var trim8kDb = SpeechFxFunctions
                .SmoothSplineClamped( Constants.Baseline8kTrimDbFx, Constants.Baseline8kTrimDbY, fxLevel );
            var baseline4kDb = Constants.Baseline4kBoostDb + trim4kDb;
            var baseline8kDb = Constants.Baseline8kBoostDb + trim8kDb;

            _boost125 = new BiquadPeakingEqFilter[ _channels ];
            _boost250 = new BiquadPeakingEqFilter[ _channels ];
            _boost500 = new BiquadPeakingEqFilter[ _channels ];
            _boost1k = new BiquadPeakingEqFilter[ _channels ];
            _boost2k = new BiquadPeakingEqFilter[ _channels ];
            _boost4k = new BiquadPeakingEqFilter[ _channels ];
            _boost8k = new BiquadPeakingEqFilter[ _channels ];
            _hiShelf = new BiquadHighShelfFilter[ _channels ];

            for ( var ch = 0; ch < _channels; ch++ )
            {
                _boost125[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline125BoostHz, Constants.Baseline125BoostDb, Constants.Baseline125BoostQ, sampleRate );
                _boost250[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline250BoostHz, Constants.Baseline250BoostDb, Constants.Baseline250BoostQ, sampleRate );
                _boost500[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline500BoostHz, Constants.Baseline500BoostDb, Constants.Baseline500BoostQ, sampleRate );
                _boost1k[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline1kBoostHz, Constants.Baseline1kBoostDb, Constants.Baseline1kBoostQ, sampleRate );
                _boost2k[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline2kBoostHz, Constants.Baseline2kBoostDb, Constants.Baseline2kBoostQ, sampleRate );
                _boost4k[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline4kBoostHz, baseline4kDb, Constants.Baseline4kBoostQ, sampleRate );
                _boost8k[ ch ] = new BiquadPeakingEqFilter( Constants.Baseline8kBoostHz, baseline8kDb, Constants.Baseline8kBoostQ, sampleRate );
                _hiShelf[ ch ] = new BiquadHighShelfFilter( Constants.BaselineHiShelfHz, Constants.BaselineHiShelfDb, Constants.BaselineHiShelfQ, sampleRate );
            }
        }

        public int Read ( float[] buffer, int offset, int count )
        {
            var read = _source.Read(buffer, offset, count);

            for ( var n = 0; n < read; n += _channels )
            {
                for ( var ch = 0; ch < _channels; ch++ )
                {
                    var i = offset + n + ch;
                    var x = buffer[i];
                    x = _boost125[ ch ].Process( x );
                    x = _boost250[ ch ].Process( x );
                    x = _boost500[ ch ].Process( x );
                    x = _boost1k[ ch ].Process( x );
                    x = _boost2k[ ch ].Process( x );
                    x = _boost4k[ ch ].Process( x );
                    x = _boost8k[ ch ].Process( x );
                    x = _hiShelf[ ch ].Process( x );
                    buffer[ i ] = x;
                }
            }
            return read;
        }
    }
}