using JetBrains.Annotations;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    public class Organic
    {
        [ Utilities.PublicAPI ]
        public OrganicGenus genus { get; set; }

        [Utilities.PublicAPI ]
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

        [ Utilities.PublicAPI ]
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

        [JsonIgnore, Utilities.PublicAPI( "The minimum value from all predictions of this genus." )]
        public long predictedMinimumValue => valueOverride ?? genusPredictedMinimumValue ?? 0;

        [JsonIgnore, Utilities.PublicAPI( "The maximum value from all predictions of this genus." )]
        public long predictedMaximumValue => valueOverride ?? genusPredictedMaximumValue ?? 0;

        [JsonProperty]
        internal long? genusPredictedMinimumValue = null;

        [JsonProperty]
        internal long? genusPredictedMaximumValue = null;

        [JsonIgnore, Utilities.PublicAPI( "The minimum distance that you must travel before you can collect a fresh sample of this genus (if known)" )]
        public int? minimumDistanceMeters => genus?.minimumDistanceMeters;

        /// <summary>
        /// Overrides the credit values from definitions when an actual value is indicated (as by the `OrganicDataSold` event)
        /// </summary>
        public long? valueOverride { get; set; } = null;

        /// <summary>
        /// Sets the value from predictions, this could be the minimum value from several predicted species of the same genus.
        /// </summary>
        //public long? valuePredicted { get; set; }

        [Utilities.PublicAPI( "The bonus credit value, as awarded when selling organic data" )]
        public decimal bonus { get; set; }

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

        public void SetPredictedMinimumValue ( long? minimum )
        {
            genusPredictedMinimumValue = minimum;
        }

        public void SetPredictedMaximumValue ( long? maximum )
        {
            genusPredictedMaximumValue = maximum;
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