using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Economy types
    /// </summary>
    public class Economy
    {
        private static readonly List<Economy> ECONOMIES = new List<Economy>();

        public string name { get; private set; }

        public string edname { get; private set; }

        private Economy(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            ECONOMIES.Add(this);
        }

        public static readonly Economy None = new Economy("$economy_None", "None");
        public static readonly Economy Agriculture = new Economy("$economy_Agri", "Agriculture");
        public static readonly Economy Colony = new Economy("$economy_Colony", "Colony");
        public static readonly Economy Damaged = new Economy("$economy_Damaged", "Damaged");
        public static readonly Economy Extraction = new Economy("$economy_Extraction", "Extraction");
        public static readonly Economy Refinery = new Economy("$economy_Refinery", "Refinery");
        public static readonly Economy Repair = new Economy("$economy_Repair", "Repairing");
        public static readonly Economy Industrial = new Economy("$economy_Industrial", "Industrial");
        public static readonly Economy Terraforming = new Economy("$economy_Terraforming", "Terraforming");
        public static readonly Economy HighTech = new Economy("$economy_HighTech", "High Tech");
        public static readonly Economy Service = new Economy("$economy_Service", "Service");
        public static readonly Economy Tourism = new Economy("$economy_Tourism", "Tourism");
        public static readonly Economy Military = new Economy("$economy_Military", "Military");

        public static Economy FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            Economy result = ECONOMIES.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown Economy name " + from);
            }
            return result;
        }

        public static Economy FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.Replace(";", "").ToLowerInvariant();

            Economy result;
            if (tidiedFrom == null || tidiedFrom == "")
            {
                result = null;
            }
            else
            {
                result = ECONOMIES.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            }
            if (result == null)
            {
                Logging.Report("Unknown Economy ED name " + from);
                result = new Economy(from, tidiedFrom);
            }
            return result;
        }
    }
}
