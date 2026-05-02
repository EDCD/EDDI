using JetBrains.Annotations;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace EddiDataDefinitions
{
    public class Organic
    {
        [ Utilities.PublicAPI ("Genus details" ) ]
        public OrganicGenus genus { get; set; }

        [Utilities.PublicAPI ("Species details" ) ]
        public OrganicSpecies species
        {
            get => _species;
            set
            {
                _species = value;
                genus = _species?.genus;
            }
        }
        private OrganicSpecies _species;

        [ Utilities.PublicAPI ("Variant details" ) ]
        public OrganicVariant variant
        {
            get => _variant;
            set
            {
                _variant = value;
                _species = value?.species;
                genus = value?.genus;
            }
        }
        private OrganicVariant _variant;

        [ Utilities.PublicAPI("The invariant name of the organic")] 
        public string invariantName => ConsolidatedName( genus?.organicGroup, _variant?.invariantName, _species?.invariantName, genus?.invariantName );

        [Utilities.PublicAPI("The localized name of the organic")]
        public string localizedName => ConsolidatedName( genus?.organicGroup, _variant?.localizedName, _species?.localizedName, genus?.localizedName );

        [Utilities.PublicAPI( "The minimum distance that you must travel before you can collect a fresh sample of this genus (if known)" ), JsonIgnore]
        public int? minimumDistanceMeters => genus?.minimumDistanceMeters;

        [Utilities.PublicAPI( "The base credit value, as awarded when selling organic data" )]
        public long? value => valueOverride ?? species?.value;

        /// <summary>
        /// If true, apply a predicted bonus to the value of this organic (as presumably no other commander has sold this organic before).
        /// </summary>
        public bool firstFootfallRegistered { get; set; }

        /// <summary>
        /// Overrides the credit values from definitions when an actual value is indicated (as by the `OrganicDataSold` event)
        /// </summary>
        public long? valueOverride { get; set; } = null;

        [Utilities.PublicAPI( "The bonus credit value, as awarded when selling organic data. The bonus value is assumed to apply when a first footfall has been registered." )]
        public long? bonus => bonusOverride ?? (firstFootfallRegistered ? value * 4 : 0); 

        /// <summary>
        /// Overrides the bonus credit values from definitions when an actual value is indicated (as by the `OrganicDataSold` event)
        /// </summary>
        public long? bonusOverride { get; set; } = null;

        /// <summary>
        /// Populate the organic from variant data. Most preferred.
        /// </summary>
        public Organic ( [NotNull] OrganicVariant variant )
        {
            this.variant = variant;
        }

        /// <summary>
        /// Populate the organic from species data. Supplement using the {SetVariantData} method when variant data is available.
        /// </summary>
        public Organic ( [NotNull] OrganicSpecies species )
        {
            this.species = species;
        }

        /// <summary>
        /// Populate the organic from genus data. Least preferred. Supplement using the {SetVariantData} method when variant data is available.
        /// </summary>
        public Organic ( [NotNull] OrganicGenus genus )
        {
            this.genus = genus;
        }

        /// <summary>
        /// Creates a joined organism name without redundant elements
        /// </summary>
        private string ConsolidatedName ( OrganicGenus.OrganicGroup? organicGroup, string variantName,
            string speciesName, string genusName )
        {
            if ( organicGroup is OrganicGenus.OrganicGroup.Horizons )
            {
                return string.Join( " ", new[]
                    {
                        variantName,
                        _species != null && variantName.Contains( speciesName, StringComparison.OrdinalIgnoreCase ) ? null : speciesName,
                        genus != null && speciesName.Contains( genusName, StringComparison.OrdinalIgnoreCase ) ? null : genusName
                    }
                    .Where( n => n != null ) );
            }

            return string.Join( " ", new[]
                {
                    variantName,
                    genus != null && speciesName.Contains( genusName, StringComparison.OrdinalIgnoreCase ) ? null : genusName,
                    _species != null && variantName.Contains( speciesName, StringComparison.OrdinalIgnoreCase ) ? null : speciesName
                }
                .Where( n => n != null ) );
        }

        /// <summary> Get all the biological data, this should be done at the first sample </summary>
        [Utilities.PublicAPI]
        public static Organic Lookup ( long entryid, string variant )
        {
            var organicVariant = OrganicVariant.Lookup( entryid, variant );
            return organicVariant is null ? null : new Organic( organicVariant );
        }
    }
}