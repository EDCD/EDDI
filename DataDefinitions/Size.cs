using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Sizes for ships, landing pads etc
    /// </summary>
    public class Size
    {
        private static readonly List<Size> SIZES = new List<Size>();

        public string id { get; private set; }

        public string name { get; private set; }

        private Size(string id, string name)
        {
            this.id = id;
            this.name = name;

            SIZES.Add(this);
        }

        public static readonly Size None = new Size("None", "None");
        public static readonly Size Small = new Size("S", "Small");
        public static readonly Size Medium = new Size("M", "Medium");
        public static readonly Size Large = new Size("L", "Large");
        public static readonly Size Huge = new Size("H", "Huge");

        public static Size FromName(string from)
        {
            Size result;
            if (from == null || from == "")
            {
                result = None;
            }
            else
            {
                result = SIZES.FirstOrDefault(v => v.name == from);
            }
            if (result == null)
            {
                Logging.Report("Unknown Size name " + from);
            }
            return result;
        }

        public static Size FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();

            Size result;
            if (from == null || from == "")
            {
                result = None;
            }
            else
            {
                result = SIZES.FirstOrDefault(v => v.id.ToLowerInvariant() == tidiedFrom);
            }
            if (result == null)
            {
                Logging.Report("Unknown Size ED name " + from);
                result = new Size(from, tidiedFrom);
            }
            return result;
        }
    }
}
