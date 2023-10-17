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
        public HashSet<Exobiology> bioSignals { get; set; } = new HashSet<Exobiology>();

        [PublicAPI( "The genus list of remaining biologicals" )]
        public List<string> biosignalsRemainingGenuslist {
            get
            {
                List<string> _list = new List<string>();

                HashSet<Exobiology> incomplete = biosignals.Where( e => e.scanState < Exobiology.State.SampleComplete ).ToHashSet();

                foreach(Exobiology t_bio in incomplete) {
                    _list.Add(t_bio.genus.localizedName);
                }

                return _list;
            }
        }

        [PublicAPI ( "The number of biologicals reported by FSS/SAA" )]
        public int reportedBiologicalCount { get; set; }

        [ PublicAPI( "True if the current biologicals are predicted (but not confirmed) " ) ]
        public bool hasPredictedBios => bioSignals.Any( s => s.scanState == Exobiology.State.Predicted );

        [PublicAPI( "The number of remaining bio signals on the body" )]
        public int biosignalsRemainingCount => biosignalsremaining().Count();

        [PublicAPI( "The number of complete bio signals on the body" )]
        public int biosignalsCompleteCount => biosignals.Where( e => e.scanState >= Exobiology.State.SampleComplete ).Count();

        [ PublicAPI( "True if the current biologicals are predicted (but not confirmed) " ) ]
        public bool predicted => biosignals.Any( s => s.scanState == Exobiology.State.Predicted );

        [PublicAPI( "The genus list of biologicals" )]
        public List<string> genuslist {
            get
            {
            List<string> _list = new List<string>();

            foreach(Exobiology t_bio in biosignals) {
                _list.Add(t_bio.genus.localizedName);
            }

            return _list;
            }
        }

        public bool TryGetBio ( string genusEDName, out Exobiology bio )
        {
            bio = bioSignals.FirstOrDefault( b => b.variant == organic.variant ) ?? 
                  bioSignals.FirstOrDefault( b => b.species == organic.species ) ?? 
                  bioSignals.FirstOrDefault( b => b.genus == organic.genus );
            return bio != null;
        }

        public bool TryGetBio ( OrganicVariant variant, OrganicSpecies species, OrganicGenus genus, out Exobiology bio )
        {
            bio = bioSignals.FirstOrDefault( b => b.variant == variant ) ?? 
                  bioSignals.FirstOrDefault( b => b.species == species ) ?? 
                  bioSignals.FirstOrDefault( b => b.genus == genus );
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
                bioSignals.Add( bio );
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
            bioSignals.Add( bio );
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

        [PublicAPI ( "The number of geologicals reported by FSS/SAA" )]
        public int reportedGeologicalCount { get; set; }

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
