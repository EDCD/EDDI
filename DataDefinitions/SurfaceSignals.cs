using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class SurfaceSignals
    {
        #region Biological Signals

        /// <summary>
        /// Create an Exobiology list, which contains additional structures for tracking
        /// </summary>
        [ PublicAPI ("Biological signal data") ] 
        public HashSet<Exobiology> biosignals { get; set; } = new HashSet<Exobiology>();

        [PublicAPI ( "The number of biological signals reported by FSS/SAA" )]
        public int reportedBiologicalCount { get; set; }

        [ PublicAPI( "True if the current biologicals are predicted (but not confirmed) " ) ]
        public bool hasPredictedBios => biosignals.Any( s => s.scanState == Exobiology.State.Predicted );

        [PublicAPI( "The biological signals that have not been fully scanned" )]
        public HashSet<Exobiology> biosignalsremaining =>
            biosignals.Where( e => e.scanState < Exobiology.State.SampleComplete ).ToHashSet();

        [PublicAPI( "The maximum expected credit value for biological signals on this body" )]
        public long exobiologyValue => biosignals.Select(s => s.value).Sum();

        public bool TryGetBio ( Organic organic, out Exobiology bio )
        {
            bio = biosignals.FirstOrDefault( b => b.variant == organic.variant ) ?? 
                  biosignals.FirstOrDefault( b => b.species == organic.species ) ?? 
                  biosignals.FirstOrDefault( b => b.genus == organic.genus );
            return bio != null;
        }

        public bool TryGetBio ( OrganicVariant variant, OrganicSpecies species, OrganicGenus genus, out Exobiology bio )
        {
            bio = biosignals.FirstOrDefault( b => b.variant == variant ) ?? 
                  biosignals.FirstOrDefault( b => b.species == species ) ?? 
                  biosignals.FirstOrDefault( b => b.genus == genus );
            return bio != null;
        }

        /// <summary>
        /// Add a biological object
        /// </summary>
        /// <param name="variant">The Organic Variant of the biological object</param>
        /// <param name="species">The Organic Species of the biological object</param>
        /// <param name="genus">The Organic Genus of the biological object</param>
        /// <param name="prediction">true if this is a prediction, false if confirmed</param>
        /// <returns>The Exobiological object which was added to the body's surface signals</returns>
        public Exobiology AddBio ( OrganicVariant variant, OrganicSpecies species, OrganicGenus genus, bool prediction = false )
        {
            var bio = variant != null ? new Exobiology( variant, prediction ) :
                species != null ? new Exobiology( species, prediction ) :
                genus != null ? new Exobiology( genus, prediction ) : null;
            if ( bio != null )
            {
                biosignals.Add( bio );
            }
            return bio;
        }

        /// <summary>
        /// Add a biological object
        /// </summary>
        /// <param name="genus">The OrganicGenus of the biological object</param>
        /// <param name="prediction">true if this is a prediction, false if confirmed</param>
        /// <returns>The Exobiological object which was added to the body's surface signals</returns>
        public Exobiology AddBioFromGenus ( OrganicGenus genus, bool prediction = false )
        {
            var bio = new Exobiology( genus, prediction );
            biosignals.Add( bio );
            return bio;
        }

        #endregion

        #region Geology Signals

        [PublicAPI( "The number of geological signals reported by FSS/SAA" )]
        public int reportedGeologicalCount { get; set; }

        #endregion

        #region Guardian Signals

        [PublicAPI( "The number of Guardian signals reported by FSS/SAA" )]
        public int reportedGuardianCount { get; set; }

        #endregion

        #region Human Signals

        [PublicAPI( "The number of Human signals reported by SAA" )]
        public int reportedHumanCount { get; set; }

        #endregion

        #region Thargoid Signals

        [PublicAPI( "The number of Thargoid signals reported by SAA" )]
        public int reportedThargoidCount { get; set; }

        #endregion

        #region Other Signals

        [PublicAPI( "The number of other signals reported by SAA" )]
        public int reportedOtherCount { get; set; }

        #endregion

        public DateTime lastUpdated { get; set; }
    }
}
