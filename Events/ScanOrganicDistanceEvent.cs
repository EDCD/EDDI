using EddiDataDefinitions;
using MathNet.Numerics.Random;
using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class ScanOrganicDistanceEvent : Event
    {
        public const string NAME = "Scan organic distance event";
        public const string DESCRIPTION = "Triggered when entering and exiting sample ranges.";
        public static ScanOrganicDistanceEvent SAMPLE = new ScanOrganicDistanceEvent(DateTime.UtcNow, new Exobiology( OrganicGenus.FromEDName("Clypeus") )
        {
            nearPriorSample = new Random().NextBoolean(), species = OrganicSpecies.ClypeusMargaritus, scanState = Exobiology.State.SampleStarted
        });

        [PublicAPI( "An object holding all the data about the organism currently being sampled" )]
        public Exobiology bio { get; set; }

        [PublicAPI( "The minimum distance that you must travel from your last sample location, in meters, before you can collect a fresh sample" )]
        public int minimumdistance { get; private set; }

        [PublicAPI( "True if you have traveled sufficiently far from your prior sample(s), false if you have not" )]
        public bool scanready { get; private set; }

        // Not intended to be user facing

        public ScanOrganicDistanceEvent ( DateTime timestamp, Exobiology bio ) : base( timestamp, NAME )
        {
            this.bio = bio;
            this.minimumdistance = bio.genus.minimumDistanceMeters;
            this.scanready = !bio.nearPriorSample;
        }
    }
}
