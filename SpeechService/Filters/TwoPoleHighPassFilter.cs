namespace EddiSpeechService.Filters
{
    public sealed class TwoPoleHighPassFilter
    {
        private readonly OnePoleHighPassFilter _hp1 = new OnePoleHighPassFilter();
        private readonly OnePoleHighPassFilter _hp2 = new OnePoleHighPassFilter();

        public void Set ( float fc, float fs )
        {
            _hp1.Set( fc, fs );
            _hp2.Set( fc, fs );
        }

        public float Process ( float x )
        {
            return _hp2.Process( _hp1.Process( x ) );
        }
    }
}