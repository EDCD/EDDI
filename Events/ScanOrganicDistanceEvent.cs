using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScanOrganicDistanceEvent : Event
    {
        public const string NAME = "Scan organic distance";
        public const string DESCRIPTION = "Triggered when entering and exiting organic sampling ranges";
        public static ScanOrganicDistanceEvent SAMPLE = new ScanOrganicDistanceEvent( DateTime.UtcNow, new Organic( OrganicVariant.Clypeus_02_A ), true );

        [PublicAPI( "An object holding data about the organism currently being sampled" )]
        public Organic organic { get; set; }

        [PublicAPI( "The minimum distance that you must travel from your prior sample location(s), in meters, before you can collect a fresh sample" )]
        public int minimumdistance { get; private set; }

        [PublicAPI( "True if you have traveled sufficiently far from your prior sample(s), false if you have re-entered a prior scan radius" )]
        public bool scanready { get; private set; }

        // Not intended to be user facing

        public ScanOrganicDistanceEvent ( DateTime timestamp, Organic bio, bool isInsideRadii ) : base( timestamp, NAME )
        {
            organic = bio;
            minimumdistance = bio.genus.minimumDistanceMeters;
            scanready = !isInsideRadii;
        }
    }
}