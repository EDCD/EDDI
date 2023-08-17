using System.Collections.Generic;
using System.ComponentModel;
using System.Web.UI;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A dictionary of barycenter ID's tied to BodyIds.
    /// </summary>
    public class BaryCentre : Dictionary<long, List<long>>
    {
        /// <summary>
        /// Placeholder, in case we need a nested BaryCentre
        /// </summary>
        //public BaryCentre child = null;

        public BaryCentre ( )
        { }

        /// <summary>
        /// Create a new empty BaryCentre list of bodies
        /// </summary>
        public void AddBaryCentre ( long baryId )
        {
            if ( !ContainsKey( baryId ) )
            {
                Add( baryId, new List<long>() );
            }
        }

        /// <summary>
        /// Add a body to a BaryCentre list, create the list if it doesn't exist
        /// </summary>
        public void AddBody ( long baryId, long bodyId )
        {
            // If barycenter ID doesn't exist, create it
            AddBaryCentre( baryId );

            // Add the BodyId to the barycenter list if it doesn't exist already
            if ( !this[ baryId ].Contains( bodyId ) )
            {
                this[ baryId ].Add( bodyId );
            }
        }

        /// <summary>
        /// Get the list of bodies that belong to a BaryId
        /// </summary>
        public List<long> GetBaryCentres ( long baryId )
        {
            if ( ContainsKey( baryId ) )
            {
                return this[ baryId ];
            }
            return new List<long>();
        }
    }
}
