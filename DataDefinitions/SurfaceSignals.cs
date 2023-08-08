using JetBrains.Annotations;
using System.Collections.Generic;

namespace EddiDataDefinitions
{
    public class SurfaceSignals
    {
        /// <summary>
        /// Create a simple geology list, same as a codex entry
        /// Create an Exobiology list, which contains additional structures for tracking
        /// Both are keyed to their edname because the EntryID is not available for the ScanOrganic event.
        /// While we could probably use a List here, the IDictionary inherently prevents duplicate entries from being added.
        /// </summary>
        public IDictionary<string, Exobiology> bioList;
        public IDictionary<string, GeologyItem> geoList;

        string currentGenus;


        public SurfaceSignals ()
        {
            bioList = new Dictionary<string, Exobiology> ();
            geoList = new Dictionary<string, GeologyItem>();
        }

        public void SetGenus ( string edname_genus )
        {
            currentGenus = edname_genus;
        }

        public Exobiology GetBio ()
        {
            return bioList[ currentGenus ];
        }

        public Exobiology GetBio ( string edname_genus )
        {
            currentGenus = edname_genus;
            if ( bioList.ContainsKey( edname_genus ) )
            {
                return bioList[ edname_genus ];
            }
            return new Exobiology();
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
            currentGenus = edname_genus;
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
            currentGenus = edname_genus;
        }

        public void Predict ( Body body )
        {
            // TODO:#2212........[Use body data to predict what genus' might be present.]
        }

        public void AddGeo ( string edname )
        {
            if ( !geoList.ContainsKey( edname ) )
            {
                geoList.Add( edname, new GeologyItem() );
            }
        }

        public List<string> GetBios ()
        {
            List<string> list = new List<string>();

            if ( bioList != null )
            {
                foreach ( string key in bioList.Keys )
                {
                    list.Add( bioList[ key ].genus.name );
                }
            }

            return list;
        }
    }
}
