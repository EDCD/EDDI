using System;
using System.Collections.Generic;
using Utilities;

namespace EddiDataDefinitions
{
    public class Exobiology : Organic
    {
        public enum State
        {
            Predicted,
            Confirmed,
            SampleStarted,    // Logged (1st sample collected)
            SampleInProgress, // Sampled (2nd sample collected)
            SampleComplete,   // Sampled (3rd sample collected)
            SampleAnalyzed    // Analyzed - this comes shortly after the final sample is collected
        }

        public State scanState { get; set; }

        [PublicAPI]
        public string state => scanState.ToString();

        // coordinates of scan [n-1]. Only Log and Sample are stored.
        [ PublicAPI ]
        public List<Tuple<decimal?, decimal?>> sampleCoords = new List<Tuple<decimal?, decimal?>>(); 
        
        [PublicAPI] public bool nearPriorSample { get; set; }

        [PublicAPI]
        public int samples => sampleCoords.Count;

        public Exobiology ( OrganicGenus genus, bool isPrediction = false ) : base ( genus )
        {
            this.genus = genus;

            if ( isPrediction )
            {
                this.scanState = State.Predicted;
            }
            else
            {
                this.scanState = State.Confirmed;
            }
        }

        /// <summary>Increase the sample count, set the coordinates, and return the number of scans complete.</summary>
        public void Sample ( string scanType, OrganicVariant sampleVariant, decimal? latitude, decimal? longitude )
        {
            if ( variant is null )
            {
                SetVariantData( sampleVariant );
            }

            // Check for sample type and update sample coordinates
            if ( scanType == "Log" )
            {
                scanState = State.SampleStarted;
                sampleCoords.Add( new Tuple<decimal?, decimal?>( latitude, longitude ) );
            }
            else if ( scanType == "Sample" && samples < 2 )
            {
                scanState = State.SampleInProgress;
                sampleCoords.Add( new Tuple<decimal?, decimal?>( latitude, longitude ) );
            }
            else if ( scanType == "Sample" && samples == 2 )
            {
                scanState = State.SampleComplete;
            }
            else if ( scanType == "Analyse" )
            {
                scanState = State.SampleAnalyzed;
            }

            nearPriorSample = true;
        }
    }
}
