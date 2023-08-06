using System.Collections.Generic;
using Utilities;
using System.Threading;

namespace EddiDataDefinitions
{
    public class Exobiology : OrganicItem
    {
        public enum Status
        {
            InsideSampleRange = 0,
            OutsideSampleRange = 1
        }

        public class Coordinates
        {
            public decimal? latitude;
            public decimal? longitude;
            public Status status;             // 0=Inside Radius, 1=Outside Radius
            public Status lastStatus;         // diff between this and status determines when to trigger update events
        }

        // The genus is the dictionary key for this item
        //public bool prediction;                       // Is this a prediction? Should be set to false after proven to exist; object deleted if proven not to exist
        public IDictionary<string, string> predictions; // List of predicted variants. <edname_variant, localised_name>
        public int samples;                             // 0=none, 1=Log, 2=Sample 1, 3=Sample 2, 4=Analyse
        public bool complete;                           // Sampling of this biological is complete
        //public decimal?[] latitude;                     // [n]=latitude of scan n-1 (only Log and Sample 1 matter)
        //public decimal?[] longitude;                    // [n]=longitude of scan n-1 (only Log and Sample 1 matter)
        public Coordinates[] coords;                    // coordinates of scan [n-1]. Only Log and Sample are stored.

        public Exobiology () : base ()
        {
            predictions = null;
            this.samples = 0;
            //this.latitude = new decimal?[ 2 ];      // TODO:#2212........[deprecate]
            //this.longitude = new decimal?[ 2 ];     // TODO:#2212........[deprecate]
            coords = new Coordinates [ 2 ];
            for ( int i = 0; i < 2; i++ )
            {
                coords[ i ] = new Coordinates();
            }
        }

        public Exobiology ( string edname_genus, bool predict=false ) : base()
        {
            this.samples = 0;
            //this.latitude = new decimal?[ 2 ];      // TODO:#2212........[deprecate]
            //this.longitude = new decimal?[ 2 ];     // TODO:#2212........[deprecate]
            coords = new Coordinates[ 2 ];
            for ( int i = 0; i < 2; i++ )
            {
                coords[ i ] = new Coordinates();
            }

            this.genus = OrganicInfo.SetGenus( edname_genus );


            predictions = null;
        }

        [PublicAPI]
        public void Predict ( Body body )
        {
            predictions = OrganicInfo.GetPrediction( body );
        }

        [PublicAPI]
        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        public void SetData ( string edname_variant )
        {
            OrganicItem item = OrganicInfo.LookupByVariant( edname_variant );

            this.exists = item.exists;
            this.genus = item.genus;
            this.species = item.species;
            this.variant = item.variant;
            this.variantData = item.variantData;
        }

        /// <summary>Increase the sample count, set the coordinates, and return the number of scans complete.</summary>
        public int Sample ( string scanType, string edname_variant, decimal? latitude, decimal? longitude )
        {
            // Never scanned before? Update data.
            if ( samples == 0 )
            {
                SetData( edname_variant );
                complete = false;
            }

            //new Thread( () => System.Windows.Forms.MessageBox.Show( $"Data Set" ) ).Start();

            // Check for sample type and update sample numbers
            if ( scanType == "Log" )
            {
                try
                {
                    //new Thread( () => System.Windows.Forms.MessageBox.Show( $"Log" ) ).Start();
                    samples = 1;
                    //this.latitude[ samples - 1 ] = latitude;        // TODO:#2212........[deprecate]
                    //this.longitude[ samples - 1 ] = longitude;      // TODO:#2212........[deprecate]

                    if ( coords[ samples - 1 ].latitude == null )
                    {
                        coords[ samples - 1 ].latitude = new decimal( 0 );
                    }
                    if ( coords[ samples - 1 ].longitude == null )
                    {
                        coords[ samples - 1 ].longitude = new decimal( 0 );
                    }

                    coords[ samples - 1 ].latitude = latitude;
                    coords[ samples - 1 ].longitude = longitude;

                    coords[ samples - 1 ].status = Status.InsideSampleRange;
                    coords[ samples - 1 ].lastStatus = Status.InsideSampleRange;

                    complete = false;
                }
                catch
                {
                    new Thread( () => System.Windows.Forms.MessageBox.Show( $"Log Failed" ) ).Start();
                }
            }
            else if ( scanType == "Sample" && samples==1 )
            {
                try
                {
                    //new Thread( () => System.Windows.Forms.MessageBox.Show( $"Sample 1" ) ).Start();
                    samples = 2;
                    //this.latitude[ samples - 1 ] = latitude;        // TODO:#2212........[deprecate]
                    //this.longitude[ samples - 1 ] = longitude;      // TODO:#2212........[deprecate]

                    if ( coords[ samples - 1 ].latitude == null )
                    {
                        coords[ samples - 1 ].latitude = new decimal( 0 );
                    }
                    if ( coords[ samples - 1 ].longitude == null )
                    {
                        coords[ samples - 1 ].longitude = new decimal( 0 );
                    }

                    coords[ samples - 1 ].latitude = latitude;
                    coords[ samples - 1 ].longitude = longitude;
                }
                catch
                {
                    new Thread( () => System.Windows.Forms.MessageBox.Show( $"Sample 1 Failed" ) ).Start();
                }
            }
            else if ( scanType == "Sample" && samples == 2 )
            {
                try
                {
                    //new Thread( () => System.Windows.Forms.MessageBox.Show( $"Sample 2" ) ).Start();
                    samples = 3;
                }
                catch
                {
                    new Thread( () => System.Windows.Forms.MessageBox.Show( $"Sample 2 Failed" ) ).Start();
                }
            }
            else if ( scanType == "Analyse" )
            {
                //new Thread( () => System.Windows.Forms.MessageBox.Show( $"Analyse" ) ).Start();
                complete = true;
                samples = 4;
            }
            
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
