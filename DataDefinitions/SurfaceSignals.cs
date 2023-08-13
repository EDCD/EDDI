using JetBrains.Annotations;
using System.Collections.Generic;
using Utilities;
using System.Threading;
using System;
using System.Security.Cryptography.X509Certificates;

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

        public class Geo
        {
            public DateTime timestamp;

            public IDictionary<string, GeologyItem> list;

            private int _numTotal;
            public int numTotal   // TODO:#2212........The number of geologicals reported from logs, should match geoList length. 
            {
                get
                {
                    return _numTotal;
                }

                set
                {
                    timestamp = DateTime.Now;
                    _numTotal = value;
                }
            }

            public Geo()
            {
                list = new Dictionary<string, GeologyItem>();
            }
        }
        public Geo geo;

        public class Bio
        {
            public DateTime timestamp;
            public IDictionary<string, Exobiology> list;

            private int _numTotal;
            public int numTotal   // TODO:#2212........The number of biologicals reported from logs, may not match bioList length due to predictions.
            {
                get
                {
                    return _numTotal;
                }

                set
                {
                    timestamp = DateTime.Now;
                    _numTotal = value;
                }
            }
            //public int complete;
            //public int remaining;

            private List<string> _listRemaining;
            public List<string> listRemaining
            {
                get
                {
                    if ( _listRemaining == null )
                    {
                        _listRemaining = new List<string>();
                    }
                    else
                    {
                        _listRemaining.Clear();
                    }

                    foreach ( Exobiology item in list.Values )
                    {
                        if ( !item.complete )
                        {
                            _listRemaining.Add( item.genus.name );
                        }
                    }
                    return _listRemaining;
                }
                set
                {
                    _listRemaining = value;
                }
            }

            private int _numComplete;
            public int numComplete
            {
                get
                {
                    _numComplete = 0;
                    foreach ( Exobiology item in list.Values )
                    {
                        if ( item.complete )
                        {
                            _numComplete++;
                        }
                    }
                    return _numComplete;
                }
                set
                {
                    _numComplete = value;
                }
            }

            private int _numRemaining;
            public int numRemaining
            {
                get
                {
                    _numRemaining = numTotal - _numComplete;
                    return _numRemaining;
                }
                set
                {
                    _numRemaining = value;
                }
            }

            public Bio ()
            {
                list = new Dictionary<string, Exobiology>();
            }

        };
        public Bio bio;

        public SurfaceSignals ()
        {
            bio = new Bio();
            geo = new Geo();
        }

        public Exobiology GetBio ( string edname_genus )
        {
            if ( bio.list.ContainsKey( edname_genus ) )
            {
                return bio.list[ edname_genus ];
            }
            return new Exobiology( edname_genus );
        }

        /// <summary>
        /// Add a biological Exobiology object
        /// </summary>
        /// <param name="edname">i.e. name=Codex_Ent_Stratum_02_F_Name, edname=Stratum_02_F  </param>
        public void AddBio ( string edname_genus )
        {
            if ( !bio.list.ContainsKey( edname_genus ) )
            {
                bio.list.Add( edname_genus, new Exobiology( edname_genus ) );
            }
        }

        public void AddBio ( string edname_genus, bool prediction )
        {
            if ( !bio.list.ContainsKey( edname_genus ) )
            {
                bio.list.Add( edname_genus, new Exobiology( edname_genus, prediction ) );
            }
        }

        public void AddGeo ( string edname )
        {
            if ( !geo.list.ContainsKey( edname ) )
            {
                geo.list.Add( edname, new GeologyItem() );
            }
        }

        public List<string> GetBios ()
        {
            List<string> list = new List<string>();

            if ( bio.list != null )
            {
                int c = 0;
                foreach ( string key in bio.list.Keys )
                {
                    Logging.Debug( $"[SurfaceSignals] GetBios() -> [#{c}] {key}" );
                    Thread.Sleep( 10 );
                    c++;

                    list.Add( bio.list[ key ].genus.name );
                }
            }

            return list;
        }
    }
}
