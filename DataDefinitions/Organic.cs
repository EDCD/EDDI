using Utilities;

namespace EddiDataDefinitions
{
    public class Organic
    {
        public OrganicGenus genus;
        public OrganicSpecies species;
        public OrganicVariant variant;

        /// <summary>
        /// Static constructor, we only need to initialize this data once.
        /// </summary>
        public Organic ()
        { }

        public Organic ( OrganicGenus genus, OrganicSpecies species = null, OrganicVariant variant = null )
        {
            this.genus = genus;
            this.species = species;
            this.variant = variant;
        }

        public Organic ( string genusEDName )
        {
            this.genus = OrganicGenus.FromEDName( genusEDName );
        }

        [PublicAPI]
        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        public static Organic Lookup ( long entryid, string variant )
        {
            var organicVariant = OrganicVariant.Lookup( entryid, variant );
            return new Organic( organicVariant.genus, organicVariant.species, organicVariant );
        }

        [PublicAPI]
        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        public void SetVariantData ( OrganicVariant thisVariant )
        {
            this.variant = thisVariant;
            this.species = this.variant.species;
            this.genus = this.variant.genus;
        }
    }
}