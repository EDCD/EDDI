using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Newtonsoft.Json;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Volcanism
    /// </summary>
    public class Volcanism
    {
        // Translation of composition of volcanism 
        private static readonly IDictionary<string, string> COMPOSITIONS = new Dictionary<string, string>();

        public string type { get; set; } // Geysers/Magma

        public string composition { get; set; } // Iron, Silicate, etc.

        /// <summary>The Local-ized Volcanism composition</summary>
        [JsonIgnore]
        public string LocalComposition
        {
            get
            {
                return I18N.GetString(composition) ?? composition;
            }
        }

        public string amount { get; set; } // Minor, Major, null (for normal)

        static Volcanism()
        {
            COMPOSITIONS.Add("ammonia", "Ammonia");
            COMPOSITIONS.Add("carbon dioxide", "Carbon dioxide");
            COMPOSITIONS.Add("metallic", "Iron");
            COMPOSITIONS.Add("methane", "Methane");
            COMPOSITIONS.Add("nitrogen", "Nitrogen");
            COMPOSITIONS.Add("rocky", "Silicate");
            COMPOSITIONS.Add("silicate vapour", "Silicate vapour");
            COMPOSITIONS.Add("water", "Water");
        }

        public Volcanism(string type, string composition, string amount)
        {
            this.type = I18N.GetString(type);           //type;
            this.composition = I18N.GetString(composition);    //composition;
            this.amount = I18N.GetString(amount);         //amount;
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

        public string toString()
        {
            if (amount == null)
            {
                return composition + " " + type;
            }
            else
            {
                return amount + " " + composition + " " + type;
            }
        }
    }
}
