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

        [PublicAPI]
        public decimal value { get; set; } // Credit value (when sold in the `OrganicDataSold` event)

        [PublicAPI]
        public decimal bonus { get; set; } // Bonus credit value (when sold in the `OrganicDataSold` event)

        /// <summary>
        /// Populate the organic from variant data. Most preferred.
        /// </summary>
        public Organic ( OrganicVariant variant )
        {
            if (variant is null) { return; }
            this.variant = variant;
            this.species = variant.species;
            this.genus = variant.genus;
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
            this.variant = thisVariant;
            this.species = this.variant.species;
            this.genus = this.variant.genus;
        }
    }
}