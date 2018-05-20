using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Volcanism
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Volcanism
    {
        static Volcanism()
        {
            resourceManager = Properties.Volcanism.ResourceManager;
            resourceManager.IgnoreCase = false;

            COMPOSITIONS.Add("ammonia", "Ammonia");
            COMPOSITIONS.Add("carbon dioxide", "Carbon dioxide");
            COMPOSITIONS.Add("metallic", "Iron");
            COMPOSITIONS.Add("methane", "Methane");
            COMPOSITIONS.Add("nitrogen", "Nitrogen");
            COMPOSITIONS.Add("rocky", "Silicate");
            COMPOSITIONS.Add("silicate vapour", "Silicate vapour");
            COMPOSITIONS.Add("water", "Water");
        }

        public static readonly ResourceManager resourceManager;

        // Translation of composition of volcanism 
        private static readonly IDictionary<string, string> COMPOSITIONS = new Dictionary<string, string>();

        [JsonProperty("type")]
        public string edType { get; set; } // Geysers/Magma
        public string invariantType => GetInvariantString(edType);
        public string localizedType => GetLocalizedString(edType);
        [JsonIgnore, Obsolete("Please use localizedType or invariantType")]
        public string type => localizedType;

        [JsonProperty("composition")]
        public string edComposition { get; set; } // Iron, Silicate, etc.
        public string invariantComposition => GetInvariantString(edComposition);
        public string localizedComposition => GetLocalizedString(edComposition);
        [JsonIgnore, Obsolete("Please use localizedComposition or invariantComposition")]
        public string composition => localizedComposition;

        [JsonProperty("amount")]
        public string edAmount { get; set; } // Minor, Major, null (for normal)
        public string invariantAmount => GetInvariantString(edAmount);
        public string localizedAmount => GetLocalizedString(edAmount);
        [JsonIgnore, Obsolete("Please use localizedAmount or invariantAmount")]
        public string amount => localizedAmount;

        private string GetInvariantString(string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name, CultureInfo.InvariantCulture);
        }

        private string GetLocalizedString(string name)
        {
            if (name == null) { return null; }
            name = name.Replace(" ", "_");
            return resourceManager.GetString(name);
        }

        public Volcanism(string type, string composition, string amountEDNAme)
        {
            this.edType = type;
            this.edComposition = composition;
            this.edAmount = amountEDNAme;
        }

        /// <summary>
        /// Create volcanism from component parts
        /// </summary>
        public static Volcanism FromName(string from)
        {
            if (from == null || from == "" || from == "No volcanism")
            {
                return null;
            }

            from = from.ToLowerInvariant();

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
            } else if (from.StartsWith("minor "))
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
            } else if (from.EndsWith(" magma"))
            {
                type = "Magma";
                from = from.Replace(" magma", "");
            }

            // Remaining is composition
            string composition = from;
            if (COMPOSITIONS.ContainsKey(composition))
            {
                composition = COMPOSITIONS[composition];
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
