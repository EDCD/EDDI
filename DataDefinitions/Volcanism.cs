using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Volcanism
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Volcanism ( string type, string composition, string amountEdName )
    {
        static Volcanism()
        {
            resourceManager = Properties.Volcanism.ResourceManager;
            resourceManager.IgnoreCase = true;

            COMPOSITIONS.Add("ammonia", "Ammonia");
            COMPOSITIONS.Add("carbon dioxide", "Carbon dioxide");
            COMPOSITIONS.Add("metallic", "Iron");
            COMPOSITIONS.Add("methane", "Methane");
            COMPOSITIONS.Add("nitrogen", "Nitrogen");
            COMPOSITIONS.Add("rocky", "Silicate"); // "Rocky" isn't listed by the player journal manual but is reported by the player journal
            COMPOSITIONS.Add("silicate", "Silicate");
            COMPOSITIONS.Add("silicate vapour", "Silicate vapour");
            COMPOSITIONS.Add("water", "Water");
        }
        
        [PublicAPI( "the localized type of volcanism: either \"Geysers\" or \"Magma\"" ), JsonIgnore, Obsolete("Please use localizedType")]
        public string type => localizedType;

        [PublicAPI( "the invariant type of volcanism: either \"Geysers\" or \"Magma\"" ), JsonIgnore]
        public string invariantType => GetInvariantString( edType );

        [PublicAPI( "the localized composition of the volcanism (Iron, Carbon dioxide, Nitrogen etc.)" ), JsonIgnore, Obsolete("Please use localizedComposition")]
        public string composition => localizedComposition;

        [PublicAPI( "the invariant composition of the volcanism (Iron, Carbon dioxide, Nitrogen etc.)" ), JsonIgnore]
        public string invariantComposition => GetInvariantString( edComposition );

        [PublicAPI( "the localized amount of volcanism (\"Major\", \"Minor\" or nothing)" ), JsonIgnore, Obsolete("Please use localizedAmount")]
        public string amount => localizedAmount;

        [PublicAPI( "the invariant amount of volcanism (\"Major\", \"Minor\" or nothing)" ), JsonIgnore]
        public string invariantAmount => GetInvariantString( edAmount );

        // Not intended to be user facing

        public static readonly ResourceManager resourceManager;

        [JsonProperty("type")]
        public string edType { get; set; } = type; // Geysers/Magma
        public string localizedType => GetLocalizedString(edType);

        [JsonProperty("composition")]
        public string edComposition { get; set; } = composition; // Iron, Silicate, etc.
        public string localizedComposition => GetLocalizedString(edComposition);

        [JsonProperty("amount")]
        public string edAmount { get; set; } = amountEdName; // Minor, Major, null (for normal)
        public string localizedAmount => GetLocalizedString(edAmount);

        // Translation of composition of volcanism 
        private static readonly Dictionary<string, string> COMPOSITIONS = [];

        private static string GetInvariantString (string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name, CultureInfo.InvariantCulture);
        }

        private static string GetLocalizedString (string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name);
        }

        /// <summary>
        /// Create volcanism from component parts
        /// </summary>
        public static Volcanism FromName(string from)
        {
            from = from?.ToLowerInvariant();

            if (from is null or "" or "no volcanism")
            {
                return null;
            }

            // Volcanism commonly has ' volcanism' attached to the end of it; remove it here
            if (from.EndsWith(" volcanism"))
            {
                from = from.Replace(" volcanism", "");
            }

            // Volcanism can have either 'major ' or 'minor ' prepended
            string amount = null;
            if (from.StartsWith("major "))
            {
                amount = "Major";
                from = from.Replace("major ", "");
            }
            else if (from.StartsWith("minor "))
            {
                amount = "Minor";
                from = from.Replace("minor ", "");
            }

            // Volcanism can be either magma or geysers
            string type = null;
            if (from.EndsWith(" geysers"))
            {
                type = "Geysers";
                from = from.Replace(" geysers", "");
            }
            else if (from.EndsWith(" magma"))
            {
                type = "Magma";
                from = from.Replace(" magma", "");
            }

            // Remaining is composition
            var composition = from;
            if (COMPOSITIONS.TryGetValue(composition, out var value))
            {
                composition = value;
            }

            return new Volcanism(type, composition, amount);
        }

        public override string ToString()
        {
            if (localizedAmount == null)
            {
                return $"{localizedComposition} {localizedType}";
            }
            else
            {
                return $"{localizedAmount} {localizedComposition} {localizedType}";
            }
        }
    }
}
