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

        [PublicAPI ( "The number of biologicals reported by FSS/SAA" )]
        public int reportedBiologicalCount { get; set; }

        public HashSet<Exobiology> biosignalsremaining () =>
            biosignals.Where( e => e.scanState < Exobiology.State.SampleComplete ).ToHashSet();


        [ PublicAPI( "True if the current biologicals are predicted (but not confirmed) " ) ]
        public bool predicted => biosignals.Any( s => s.scanState == Exobiology.State.Predicted );

        public bool TryGetBio ( string genusEDName, out Exobiology bio )
        {
            bio = biosignals.FirstOrDefault( b => b.genus.edname == genusEDName );
            return bio != null;
        }

        public bool TryGetBio ( OrganicGenus genus, out Exobiology bio )
        {
            bio = biosignals.FirstOrDefault( b => b.genus.edname == genus.edname );
            return bio != null;
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

        [PublicAPI( "Geological signal data" )]
        public HashSet<Geology> geosignals { get; set; } = new HashSet<Geology>();

        // The number of geologicals reported by FSS/SAA
        public int reportedGeologicalCount { get; set; }

        public void AddGeo ( string edname )
        {
            geosignals.Add( Geology.FromEDName( edname ) );
        }

        public bool TryGetGeo ( string edname, out Geology geo )
        {
            geo = geosignals.FirstOrDefault( g => g.edname == edname );
            return geo != null;
        }
        
        #endregion

        public DateTime lastUpdated { get; set; }
    }
}
