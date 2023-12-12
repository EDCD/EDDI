using Utilities;

namespace EddiDataDefinitions
{
    public class Organic
    {
        [PublicAPI]
        public OrganicGenus genus;

        [PublicAPI]
        public OrganicSpecies species;

        [PublicAPI]
        public OrganicVariant variant;

        [PublicAPI ("The credit value for selling organic data for the species, or the predicted value, or else the minimum credit value for the genus if the species is not yet known" ) ]
        public long value => valueOverride ?? 
                             species?.value ?? 
                             genus?.minimumValue ?? 
                             0;

        [PublicAPI ("The minimum value from all predictions of this genus.")]
        public long predictedMinimumValue => valueOverride ??
                                             genus?.predictedMinimumValue ??
                                             0;

        [PublicAPI ("The maximum value from all predictions of this genus.")]
        public long predictedMaximumValue => valueOverride ??
                                             genus?.predictedMaximumValue ??
                                             0;

        [PublicAPI( "The minimum distance that you must travel before you can collect a fresh sample of this genus" )]
        public int minimumDistanceMeters => genus.minimumDistanceMeters;

        /// <summary>
        /// Overrides the credit values from definitions when an actual value is indicated (as by the `OrganicDataSold` event)
        /// </summary>
        public long? valueOverride { get; set; }

        /// <summary>
        /// Sets the value from predictions, this could be the minimum value from several predicted species of the same genus.
        /// </summary>
        //public long? valuePredicted { get; set; }

        [PublicAPI ( "The bonus credit value, as awarded when selling organic data" ) ]
        public decimal bonus { get; set; }

        /// <summary>
        /// Populate the organic from variant data. Most preferred.
        /// </summary>
        public Organic ( OrganicVariant variant )
        {
            if (variant is null) { return; }
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