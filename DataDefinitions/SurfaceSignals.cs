﻿using System;
using System.Collections.Generic;
using Utilities;

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
            public IDictionary<string, Geology> list;

            public int reportedTotal;

            public int? numTotal => list.Count;

            public Geo()
            {
                list = new Dictionary<string, Geology>();
                reportedTotal = 0;
            }
        }

        [PublicAPI]
        public Geo geo;

        public class Bio
        {
            public Dictionary<string, Exobiology> list;

            public int reportedTotal;  // The number of biologicals reported by FSS/SAA

            public int? numTotal => list.Count;

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
                            _listRemaining.Add( item.genus.localizedName );
                        }
                    }
                    return _listRemaining;
                }
                set
                {
                    _listRemaining = value;
                }
            }

            private int? _numComplete;

            [PublicAPI]
            public int? numComplete
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

            private int? _numRemaining;

            [PublicAPI]
            public int? numRemaining
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
                reportedTotal = 0;
            }

        };

        [PublicAPI]
        public Bio bio;

        // Are the current biologicals predicted
        [PublicAPI]
        public bool predicted;

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
                geo.list.Add( edname, Geology.LookupByName( edname ) );
            }
        }

        public List<string> GetBios ()
        {
            List<string> list = new List<string>();

            if ( bio.list != null )
            {
                foreach ( string key in bio.list.Keys )
                {
                    list.Add( bio.list[ key ].genus.localizedName );
                }
            }

            return list;
        }

        public List<string> GetGeos ()
        {
            List<string> list = new List<string>();

            if ( bio.list != null )
            {
                foreach ( string key in geo.list.Keys )
                {
                    list.Add( geo.list[ key ].localizedName );
                }
            }

            return list;
        }
    }
}
