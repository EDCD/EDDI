using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Utilities;

namespace EddiDataDefinitions
{
    public class Exobiology /*: ResourceBasedLocalizedEDName<Exobiology>*/
    {
        /// <summary>Information for a single biological populated after mapping a planet</summary>
        public class BioItem
        {
            // The genus is the dictionary key for this item
            public bool prediction;         // Is this a prediction? Should be set to false after proven to exist; object deleted if proven not to exist
            public string species;          // i.e. Flabellum
            public string variant;          // i.e. Green
            public int scans;               // 0=none, 1=Log, 2=Sample 1, 3=Sample 2, 4=Analyse
            public decimal?[] latitude;     // [n]=latitude of scan n-1 (only Log and Sample 1 matter)
            public decimal?[] longitude;    // [n]=longitude of scan n-1 (only Log and Sample 1 matter)

            public BioItem ()
            {
                this.prediction = false;
                this.species = "";
                this.variant = "";
                this.scans = 0;
                this.latitude = new decimal?[ 2 ];
                this.longitude = new decimal?[ 2 ];
            }

            public BioItem ( string species, string variant, bool prediction=true )
            {
                this.prediction = prediction;
                this.species = species;
                this.variant = variant;
                this.scans = 0;
                this.latitude = new decimal?[ 2 ];
                this.longitude = new decimal?[ 2 ];
            }
        }

        [PublicAPI]
        /// <summary>
        /// List of biologicals:
        ///     - We only have the genus initially when a planet is mapped
        ///     - The genus here is the edname (i.e. Codex_Ent_Bacterial_Genus_Name)
        ///     - Only organics scannable with genetic sampler should be in here
        /// </summary>
        public IDictionary<string, BioItem> bioItems;

        /// <summary>This stores the last used or specifically set genus</summary>
        public string currentGenus;

        public Exobiology () /*: base ("", "")*/
        {
            //bioItems = new IDictionary<int, BioItem>();
            //bioItems.Add( 0, new BioItem() );
        }

        [PublicAPI]
        /// <summary>
        /// Add a biological to the list:
        ///     - This is called when a body is first mapped and all we know is the genus
        /// </summary>
        public void Add ( string genus )
        {
            // If the key exists don't add but set to current genus
            if ( !bioItems.ContainsKey( genus ) )
            {
                bioItems.Add( genus, new BioItem() );
            }
            currentGenus = genus;

        }

        [PublicAPI]
        /// <summary>
        /// Add a biological to the list:
        ///     - If we know all the data...which we likely never will but just in case.
        /// </summary>
        public void Add ( string genus, string species, string variant )
        {
            bioItems.Add( genus, new BioItem( species, variant ) );
            currentGenus = genus;
        }

        [PublicAPI]
        /// <summary>
        /// Sets the current genus for convenience of other functions
        /// </summary>
        public void SetGenus ( string genus )
        {
            currentGenus = genus;
        }

        /// <summary>
        /// Set the species name, after we have made a first scan
        /// </summary>
        private void SetSpecies ( string genus, string species )
        {
            SetGenus( genus );
            SetSpecies( species );
        }

        private void SetSpecies ( string species )
        {
            if ( currentGenus != null )
            {
                bioItems[ currentGenus ].species = species;
            }
        }

        [PublicAPI]
        /// <summary>
        /// Set the variant name, after we have made a first scan
        /// </summary>
        private void SetVariant ( string genus, string variant )
        {
            SetGenus( genus );
            SetVariant( variant );
        }

        private void SetVariant ( string variant )
        {
            if ( currentGenus != null )
            {
                bioItems[ currentGenus ].variant = variant;
            }
        }

        [PublicAPI]
        public BioItem GetBio ( string genus )
        {
            SetGenus( genus );
            return bioItems[ genus ];
        }

        [PublicAPI]
        public void SetPrediction ( string genus, bool prediction )
        {
            SetGenus( genus );
            bioItems[ genus ].prediction = prediction;
        }

        [PublicAPI]
        /// <summary>
        /// Increase the scan count and return the result
        /// </summary>
        public int Scan ( string genus, string species, string variant, decimal? latitude, decimal? longitude )
        {
            SetGenus( genus );
            SetSpecies( species );
            SetVariant( variant );

            // TODO:#2212........[Check and update predictions]

            return Scan( latitude, longitude );
        }

        private int Scan ( string genus, decimal? latitude, decimal? longitude )
        {
            SetGenus( genus );
            return Scan( latitude, longitude);
        }

        private int Scan ( decimal? latitude, decimal? longitude )
        {
            bioItems[ currentGenus ].scans++;
            bioItems[ currentGenus ].latitude[ bioItems[ currentGenus ].scans ] = latitude;
            bioItems[ currentGenus ].longitude[ bioItems[ currentGenus ].scans ] = longitude;
            return bioItems[ currentGenus ].scans;
        }

        [PublicAPI]
        /// <summary>Get the total number of biologicals</summary>
        public int Total ()
        {
            return bioItems.Count;
        }

        [PublicAPI]
        /// <summary>Get the number of scanned biologicals</summary>
        public int Complete ()
        {
            int numComplete = 0;

            foreach ( BioItem item in bioItems.Values )
            {
                if ( item.scans >= 4 )
                {
                    numComplete++;
                }
            }

            return numComplete;
        }

        [PublicAPI]
        /// <summary>Get the number of unscanned biologicals</summary>
        public int Remaining ()
        {
            return Total() - Complete();
        }
    }
}
