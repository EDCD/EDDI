using System;
using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    public class SurfaceSignals
    {
        #region Biological Signals

        [ PublicAPI ] 
        public HashSet<Exobiology> biosignals { get; set; } = new HashSet<Exobiology>();

        // The number of geologicals reported by FSS/SAA
        public int reportedBiologicalCount { get; set; }

        public HashSet<Exobiology> biosignalsremaining () =>
            biosignals.Where( e => e.scanState != Exobiology.State.SampleComplete ).ToHashSet();

        /// <summary>
        /// Create an Exobiology list, which contains additional structures for tracking
        /// Both are keyed to their edname because the EntryID is not available for the ScanOrganic event.
        /// While we could probably use a List here, the IDictionary inherently prevents duplicate entries from being added.
        /// </summary>

        // Are the current biologicals predicted
        [PublicAPI]
        public bool predicted;

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
        public void AddBioFromGenus ( OrganicGenus genus, bool prediction = false )
        {
            biosignals.Add( new Exobiology( genus, prediction ) );
        }

        #endregion

        #region Geology Signals

        [PublicAPI]
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
