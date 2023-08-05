using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Utilities;

namespace EddiDataDefinitions
{
    public class SurfaceSignals
    {
        /// <summary>
        /// Create a simple geology list, same as a codex entry
        /// Create an Exobiology list, which contains additional structures for tracking
        /// Both are keyed to their edname
        /// While we could probably use a List here, the IDictionary inherently prevents duplicate entries from being added.
        /// </summary>
        public IDictionary<string, Exobiology> bioList;
        public IDictionary<string, GeologyItem> geoList;
        //public List<Exobiology> exobiologyList;
        //public List<GeologyItem> geologyList;

        public SurfaceSignals ()
        {
        }

        /// <summary>
        /// Add a biological Exobiology object
        /// </summary>
        /// <param name="edname">i.e. name=Codex_Ent_Stratum_02_F_Name, edname=Stratum_02_F  </param>
        public void AddBio ( string edname_genus )
        {
            if ( !bioList.ContainsKey( edname_genus ) )
            {
                bioList.Add( edname_genus, new Exobiology() );
            }
        }

        public void AddBio ( string edname_genus, Body body, bool prediction = false )
        {
            if ( !bioList.ContainsKey( edname_genus ) )
            {
                bioList.Add( edname_genus, new Exobiology() );

                if ( prediction )
                {
                    bioList[ edname_genus ].Predict( body );
                }
            }
        }

        public void Predict ( Body body )
        {
            // TODO:#2212........[Iterate through genus list and call predictions]
        }

        public void AddGeo ( string edname )
        {
            if ( !geoList.ContainsKey( edname ) )
            {
                geoList.Add( edname, new GeologyItem() );
            }
        }


        //public void Add ( string genus )
        //{
        //    // If the key exists don't add but set to current genus
        //    if ( !bioItems.ContainsKey( genus ) )
        //    {
        //        bioItems.Add( genus, new BioItem() );
        //    }
        //    currentGenus = genus;
        //}
    }
}
