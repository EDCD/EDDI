using EddiDataDefinitions;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScanOrganicDistanceEvent ( DateTime timestamp, Organic bio, bool isInsideRadii )
        : Event( timestamp, NAME )
    {
        public const string NAME = "Scan organic distance";
        public const string DESCRIPTION = "Triggered when entering and exiting organic sampling ranges";
        public static readonly ScanOrganicDistanceEvent SAMPLE = new( DateTime.UtcNow, new Organic( OrganicVariant.Clypeus_02_A ), true );

        [PublicAPI( "An object holding data about the organism currently being sampled" )]
        public Organic organic { get; set; } = bio;

        [PublicAPI( "The minimum distance that you must travel from your prior sample location(s), in meters, before you can collect a fresh sample" )]
        public int minimumdistance { get; private set; } = bio.genus.minimumDistanceMeters;

        [PublicAPI( "True if you have traveled sufficiently far from your prior sample(s), false if you have re-entered a prior scan radius" )]
        public bool scanready { get; private set; } = !isInsideRadii;

        // Not intended to be user facing
    }
}