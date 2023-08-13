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
            public Status status;           // 0=Inside Radius, 1=Outside Radius
            public Status lastStatus;       // diff between this and status determines when to trigger update events
        }

        public bool prediction;             // Was this added as a prediction?
        public int samples;                 // 0=none, 1=Log, 2=Sample 1, 3=Sample 2, 4=Analyse
        public bool complete;               // Sampling of this biological is complete
        public int value
        {
            get {
                int val = 1;
                try
                {
                    val = 2;
                    if ( data != null )
                    {
                        val = (int)data.value;
                    }
                }
                catch
                {
                    val = 99;
                }
                return val;
            }

        }
        public Coordinates[] coords;        // coordinates of scan [n-1]. Only Log and Sample are stored.

        //public Exobiology ( bool prediction=false) : base ()
        //{
        //    this.prediction = prediction;
        //    this.samples = 0;
        //    coords = new Coordinates [ 2 ];
        //    for ( int i = 0; i < 2; i++ )
        //    {
        //        coords[ i ] = new Coordinates();
        //    }
        //}

        public Exobiology ( string edname_genus, bool prediction = false ) : base()
        {
            if ( edname_genus != null )
            {
                this.prediction = prediction;
                this.samples = 0;
                coords = new Coordinates[ 2 ];
                for ( int i = 0; i < 2; i++ )
                {
                    coords[ i ] = new Coordinates();
                }

                this.genus = OrganicInfo.SetGenus( edname_genus );
            }
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
            this.data = item.data;
        }

        public List<string> PredictBios ( Body body )
        {
            // Create temporary list of ALL species possible
            List<string> list = new List<string>();
            foreach ( string species in OrganicInfo.speciesData.Keys )
            {
                list.Add( species );
            }

            // Create an empty list for species that do not meet conditions
            List<string> purge = new List<string>();

            // Iterate though species
            foreach ( string species in list )
            {
                //System.Diagnostics.Debug.WriteLine( $" ========[ Predicting Bio: {species} ]========" );

                // Iterate through conditions
                //foreach ( OrganicInfo.SpeciesData data in OrganicInfo.speciesData.Values )
                OrganicInfo.SpeciesData data = OrganicInfo.speciesData[species];
                {
                    // Check if body meets max gravity requirements
                    // maxG: Maximum gravity
                    if ( data.maxG != null )
                    {
                        //System.Diagnostics.Debug.WriteLine( $"   - Gravity: {body.gravity} > {data.maxG}" );
                        if ( body.gravity > data.maxG )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Gravity: PURGE {species}" );
                            purge.Add( species );
                            //break;
                            goto Skip_To_End;
                        }
                    }

                    // Check if body meets temperature (K) requirements
                    //  - data.kRange: 'None'=No K requirements; 'Min'=K must be greater than minK; 'Max'=K must be less than maxK; 'MinMax'=K must be between minK and maxK
                    //  - data.minK: Minimum temperature
                    //  - data.maxK: Maximum temperature
                    if ( data.kRange != "" && data.kRange != "None" )
                    {
                        //System.Diagnostics.Debug.WriteLine( $"   - Temperature: PURGE {species}" );

                        if ( data.kRange == "Min" )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Temperature: {body.temperature} <= {data.minK}" );
                            if ( body.temperature <= data.minK )
                            {
                                //System.Diagnostics.Debug.WriteLine( $"   - Temperature: PURGE {species}" );
                                purge.Add( species );
                                //break;
                                goto Skip_To_End;
                            }
                        }
                        else if ( data.kRange == "Max" )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Temperature: {body.temperature} >= {data.maxK}" );
                            if ( body.temperature >= data.maxK )
                            {
                                //System.Diagnostics.Debug.WriteLine( $"   - Temperature: PURGE {species}" );
                                purge.Add( species );
                                //break;
                                goto Skip_To_End;
                            }
                        }
                        else if ( data.kRange == "MinMax" )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Temperature: {body.temperature} < {data.minK} || {body.temperature} > {data.maxK}" );
                            if ( body.temperature < data.minK || body.temperature > data.maxK )
                            {
                                //System.Diagnostics.Debug.WriteLine( $"   - Temperature: PURGE {species}" );
                                purge.Add( species );
                                //break;
                                goto Skip_To_End;
                            }
                        }
                    }

                    // Check if body has appropriate parent star
                    //data.parentStar;
                    if ( data.parentStar != null )
                    {
                        // TODO:#2212........[Need to figure out how to find the parent star of the body.]
                        //data.parentStar;
                        //body.parents
                        foreach ( IDictionary<string, object> parent in body.parents )
                        {
                            //int c=0;
                            //foreach ( string key in parent.Keys )
                            //{
                            //    //System.Diagnostics.Debug.WriteLine( $"   - Detect Parent:[{c}] = {key}" );
                            //    c++;
                            //}
                        }
                    }

                    // Check if body has appropriate class
                    //data.planetClass;
                    bool found = false;
                    if ( data.planetClass != "" )
                    {
                        string[] classParts = data.planetClass.Split(',');
                        foreach ( string part in classParts )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Planet Class: {part} == {body.planetClass.edname}?" );
                            if ( part == body.planetClass.edname )
                            {
                                found = true;
                            }
                        }

                        if ( !found )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Planet Class: PURGE {species}" );
                            purge.Add( species );
                            //break;
                            goto Skip_To_End;
                        }
                    }

                    // Check if body has appropriate astmosphere
                    //data.atmosphereClass;
                    found = false;
                    if ( data.atmosphereClass != "" )
                    {
                        string[] atmosParts = data.atmosphereClass.Split(',');
                        foreach ( string part in atmosParts )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Atmosphere Class: {part} == {body.atmosphereclass.edname}?" );
                            if ( part == body.atmosphereclass.edname )
                            {
                                found = true;
                            }
                        }

                        if ( !found )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Atmosphere Class: PURGE {species}" );
                            purge.Add( species );
                            //break;
                            goto Skip_To_End;
                        }
                    }

                    // Check if body has appropriate volcanism
                    //data.volcanism;
                    found = false;
                    if ( data.volcanism != "" )
                    {
                        if ( body.volcanism != null )
                        {
                            string[] volcanismParts = data.volcanism.Split(',');
                            foreach ( string part in volcanismParts )
                            {
                                //System.Diagnostics.Debug.WriteLine( $"   - Volcanism: {part} == {body.volcanism.invariantComposition}?" );
                                if ( part == body.volcanism.invariantComposition )
                                {
                                    found = true;
                                }
                            }
                        }
                        else
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Volcanism: NULL" );
                            found = false;
                        }

                        if ( !found )
                        {
                            //System.Diagnostics.Debug.WriteLine( $"   - Volcanism: PURGE {species}" );
                            purge.Add( species );
                            //break;
                            goto Skip_To_End;
                        }
                    }
                }
            Skip_To_End:
                ;
            }

            // Remove species that don't meet conditions from temporary list
            foreach ( string species in purge )
            {
                list.Remove( species );
            }

            // Create a list of only the unique genus' found
            List<string> genus = new List<string>();
            foreach ( string species in list )
            {
                if ( !genus.Contains( OrganicInfo.speciesData[ species ].genus ) )
                {
                    genus.Add( OrganicInfo.speciesData[ species ].genus );
                }
            }

            return genus;
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

            // Check for sample type and update sample numbers
            if ( scanType == "Log" )
            {
                try
                {
                    samples = 1;

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
                catch(System.Exception e )
                {
                    Logging.Error( $"Exobiology: Log Failed [{e}]" );
                }
            }
            else if ( scanType == "Sample" && samples==1 )
            {
                try
                {
                    samples = 2;

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
                catch ( System.Exception e )
                {
                    Logging.Error( $"Exobiology: Sample 1 Failed [{e}]" );
                }
            }
            else if ( scanType == "Sample" && samples == 2 )
            {
                try
                {
                    samples = 3;
                }
                catch ( System.Exception e )
                {
                    Logging.Error( $"Exobiology: Sample 2 Failed [{e}]" );
                }
            }
            else if ( scanType == "Analyse" )
            {
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
