namespace EddiDataDefinitions
{
    public class StationLandingPads ( int small = 0, int medium = 0, int large = 0 )
    {
        public int Small { get; set; } = small;
        public int Medium { get; set; } = medium;
        public int Large { get; set; } = large;

        public LandingPadSize LargestPad ()
        {
            if ( Large > 0 )
            {
                return LandingPadSize.Large;
            }

            if ( Medium > 0 )
            {
                return LandingPadSize.Medium;
            }

            if ( Small > 0 )
            {
                return LandingPadSize.Small;
            }

            return LandingPadSize.None;
        }

        public bool LandingPadCheck(LandingPadSize shipSize)
        {
            return LargestPad().sizeIndex >= shipSize.sizeIndex;
        }
    }
}