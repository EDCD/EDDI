using Utilities;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class Organic
    {
        [PublicAPI]
        public OrganicGenus genus { get; set; }

        [PublicAPI]
        public OrganicSpecies species { get; set; }

        [PublicAPI]
        public OrganicVariant variant { get; set; }

        [JsonProperty]
        internal long? genusPredictedMinimumValue = null;
        public void SetPredictedMinimumValue ( long? minimum )
        {
            genusPredictedMinimumValue = minimum;
        }

        [JsonProperty]
        internal long? genusPredictedMaximumValue = null;
        public void SetPredictedMaximumValue ( long? maximum )
        {
            genusPredictedMaximumValue = maximum;
        }

        [JsonIgnore, PublicAPI( "The minimum value from all predictions of this genus." )]
        public long predictedMinimumValue => valueOverride ??
                                             genusPredictedMinimumValue ??
                                             0;

        [JsonIgnore, PublicAPI( "The maximum value from all predictions of this genus." )]
        public long predictedMaximumValue => valueOverride ??
                                             genusPredictedMaximumValue ??
                                             0;

        [JsonIgnore, PublicAPI( "The minimum distance that you must travel before you can collect a fresh sample of this genus (if known)" )]
        public int? minimumDistanceMeters => genus?.minimumDistanceMeters;

        /// <summary>
        /// Overrides the credit values from definitions when an actual value is indicated (as by the `OrganicDataSold` event)
        /// </summary>
        public long? valueOverride { get; set; } = null;

        /// <summary>
        /// Sets the value from predictions, this could be the minimum value from several predicted species of the same genus.
        /// </summary>
        //public long? valuePredicted { get; set; }

        [PublicAPI( "The bonus credit value, as awarded when selling organic data" )]
        public decimal bonus { get; set; }

        public Organic ()
        {
        }

        /// <summary>
        /// Populate the organic from variant data. Most preferred.
        /// </summary>
        public Organic ( OrganicVariant variant )
        {
            if ( variant is null ) { return; }
            this.variant = variant;
            this.species = variant.species;
            this.genus = variant.species?.genus;
        }

        /// <summary>
        /// Populate the organic from species data. Supplement using the {SetVariantData} method when variant data is available.
        /// </summary>
        public Organic ( OrganicSpecies species )
        {
            if ( species is null ) { return; }
            this.species = species;
            this.genus = species.genus;
        }

        /// <summary>
        /// Populate the organic from genus data. Least preferred. Supplement using the {SetVariantData} method when variant data is available.
        /// </summary>
        public Organic ( OrganicGenus genus )
        {
            if ( genus is null ) { return; }
            this.genus = genus;
        }

        /// <summary> Get all the biological data, this should be done at the first sample </summary>
        [PublicAPI]
        public static Organic Lookup ( long entryid, string variant )
        {
            var organicVariant = OrganicVariant.Lookup( entryid, variant );
            return new Organic( organicVariant );
        }

        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        [PublicAPI]
        public void SetVariantData ( OrganicVariant thisVariant )
        {
            if ( thisVariant is null ) { return; }
            this.variant = thisVariant;
            this.species = this.variant?.species;
            this.genus = this.variant?.species?.genus;
        }
    }
}