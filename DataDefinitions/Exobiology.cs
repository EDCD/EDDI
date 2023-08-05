using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Utilities;

namespace EddiDataDefinitions
{
    public class Exobiology : OrganicItem
    {

        // The genus is the dictionary key for this item
        //public bool prediction;                       // Is this a prediction? Should be set to false after proven to exist; object deleted if proven not to exist
        public IDictionary<string, string> predictions; // List of predicted variants. <edname_variant, localised_name>
        public int samples;                             // 0=none, 1=Log, 2=Sample 1, 3=Sample 2, 4=Analyse
        public decimal?[] latitude;                     // [n]=latitude of scan n-1 (only Log and Sample 1 matter)
        public decimal?[] longitude;                    // [n]=longitude of scan n-1 (only Log and Sample 1 matter)

        public Exobiology () : base ()
        {
            predictions = null;
            this.samples = 0;
            this.latitude = new decimal?[ 2 ];
            this.longitude = new decimal?[ 2 ];
        }

        public Exobiology ( string edname_genus, bool predict=false ) : base()
        {
            this.samples = 0;
            this.latitude = new decimal?[ 2 ];
            this.longitude = new decimal?[ 2 ];

            this.genus = OrganicInfo.SetGenus( edname_genus );

            if ( predict )
            {
            }
            else
            {
                predictions = null;
            }
        }

        [PublicAPI]
        public void Predict ( Body body )
        {
            predictions = OrganicInfo.GetPrediction( body );
        }

        [PublicAPI]
        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        private void SetData ( string edname_variant )
        {
            OrganicItem item = OrganicInfo.LookupByVariant( edname_variant );

            this.exists = item.exists;
            this.genus = item.genus;
            this.species = item.species;
            this.variant = item.variant;
            this.variantData = item.variantData;
        }

        /// <summary>Increase the sample count, set the coordinates, and return the number of scans complete.</summary>
        public int Sample ( string edname_variant, decimal? latitude, decimal? longitude )
        {
            if ( samples == 0 )
            {
                SetData( edname_variant );
            }
            samples++;
            this.latitude[ samples ] = latitude;
            this.longitude[ samples ] = longitude;
            return samples;
        }

        [PublicAPI]
        /// <summary>Is sampling of this biological complete?</summary>
        public bool IsComplete ()
        {
            return ( samples >= 4);
        }

        [PublicAPI]
        /// <summary>Get the number of samples remaining</summary>
        public int Remaining ()
        {
            return 3 - samples;
        }
    }
}
