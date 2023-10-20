using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Eventing.Reader;
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

        [PublicAPI( "The maximum expected credit value for biological signals on this body" )]
        public long exobiologyValue => bioSignals.Select(s => s.value).Sum();

        [PublicAPI ( "The number of biologicals reported by FSS/SAA" )]
        public int reportedBiologicalCount { get; set; }

        [ PublicAPI( "True if the current biologicals are predicted (but not confirmed) " ) ]
        public bool hasPredictedBios => bioSignals.Any( s => s.scanState == Exobiology.State.Predicted );

        public HashSet<Exobiology> bioSignalsRemaining =>
            bioSignals.Where( e => e.scanState < Exobiology.State.SampleComplete ).ToHashSet();

        [ PublicAPI( "True if the current biologicals are predicted (but not confirmed) " ) ]
        public bool predicted => bioSignals.Any( s => s.scanState == Exobiology.State.Predicted );

        [PublicAPI( "The maximum expected credit value for biological signals that have not been fully scanned on this body" )]
        public long remainingExobiologyValue => bioSignalsRemaining.Select( s => s.value ).Sum();

        [PublicAPI( "The predicted total minimum value limited to the number of reported biologicals." )]
        public long predictedMinimumTotalValue {
            get {
                long value = 0;

                if(reportedBiologicalCount==1) {
                    value = bioSignals.First().value;
                }
                else if(reportedBiologicalCount>1) {
                    SortedSet<long> values = new SortedSet<long>();
                    foreach(Exobiology t_bio in bioSignals) {
                        values.Add( t_bio.predictedMinimumValue );
                    }

                    int iMin = Math.Min(values.Count(), reportedBiologicalCount);
                    for(int i=0; i<iMin; i++) {
                        value += values.ElementAt(i);
                    }
                }

                return value;
            }
        }

        [PublicAPI( "The predicted total maximum value limited to the number of reported biologicals." )]
        public long predictedMaximumTotalValue {
            get {
                long value = 0;

                if(reportedBiologicalCount==1) {
                    value = bioSignals.First().value;
                }
                else if(reportedBiologicalCount>1) {
                    SortedSet<long> values = new SortedSet<long>();
                    foreach(Exobiology t_bio in bioSignals) {
                        values.Add( t_bio.predictedMaximumValue );
                    }

                    int iMax = Math.Max(0, values.Count()-reportedBiologicalCount);
                    for( int i = values.Count()-1; i>iMax; i-- ) {
                        value += values.ElementAt(i);
                    }
                }

                return value;
            }
        }

        public bool TryGetBio ( Organic organic, out Exobiology bio )
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

        //public Exobiology AddBioFromPrediction ( OrganicGenus genus, long value, bool prediction = false )
        //{
        //    var bio = new Exobiology( genus, value, prediction );
        //    bioSignals.Add( bio );
        //    return bio;
        //}

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
