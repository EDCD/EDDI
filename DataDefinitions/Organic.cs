using Utilities;

namespace EddiDataDefinitions
{
    public class Organic
    {
        public bool exists;   // This item exists and has been populated with information
        public OrganicGenus genus;
        public OrganicSpecies species;
        public OrganicVariant variant;

        /// <summary>
        /// Static constructor, we only need to initialize this data once.
        /// </summary>
        public Organic ()
        {
            this.exists = false;
            this.variant = new OrganicVariant();
            this.genus = new OrganicGenus();
            this.species = new OrganicSpecies();
        }

        public Organic (string genus)
        {
            this.genus = OrganicGenus.Lookup( genus );
            this.variant = new OrganicVariant();
            this.species = new OrganicSpecies();

            //if ( this.genus != null )
            //{
            //    this.exists = true;
            //}
        }

        [PublicAPI]
        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        public static Organic Lookup ( long entryid, string variant )
        {
            Organic item = new Organic();

            item.variant = OrganicVariant.Lookup( entryid, variant );
            item.species = OrganicSpecies.Lookup( item.variant.species );
            item.genus = OrganicGenus.Lookup( item.variant.genus );
            item.exists = true;

            return item;
        }

        [PublicAPI]
        /// <summary>Get all the biological data, this should be done at the first sample</summary>
        public void SetData ( string variant )
        {
            this.variant = OrganicVariant.LookupByVariant( variant );
            this.species = OrganicSpecies.Lookup( this.variant.species );
            this.genus = OrganicGenus.Lookup( this.variant.genus );
            this.exists = true;
        }
    }
}
