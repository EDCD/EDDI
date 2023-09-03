using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScanOrganicDistanceEvent : Event
    {
        public const string NAME = "Scan organic distance event";
        public const string DESCRIPTION = "Triggered by Discover Monitor when entering and exiting sample ranges.";
        public const string SAMPLE = "";

        [PublicAPI("The current sample distance")]
        public int sample_distance { get; private set; }

        [PublicAPI( "Player has moved inside the sample distance for sample 1" )]
        public bool sample1_inside { get; private set; }

        [PublicAPI( "Player has moved outside the sample distance for sample 1" )]
        public bool sample1_outside { get; private set; }

        [PublicAPI( "Player has moved inside the sample distance for sample 2" )]
        public bool sample2_inside { get; private set; }

        [PublicAPI( "Player has moved outside the sample distance for sample 2" )]
        public bool sample2_outside { get; private set; }

        // Not intended to be user facing

        public ScanOrganicDistanceEvent ( DateTime timestamp, int distance, int state1, int state2 ) : base( timestamp, NAME )
        {
            this.sample_distance = distance;

            if ( state1 == 1 )          // Transitioned to inside sample distance radius for Sample 1
            {
                this.sample1_inside = true;
            }
            else if ( state1 == 2 )     // Transitioned to outside sample distance radius for Sample 1
            {
                this.sample1_outside = true;
            }

            if ( state2 == 1 )          // Transitioned to inside sample distance radius for Sample 2
            {
                this.sample2_inside = true;
            }
            else if ( state2 == 2 )     // Transitioned to outside sample distance radius for Sample 2
            {
                this.sample2_outside = true;
            }
        }
    }
}
