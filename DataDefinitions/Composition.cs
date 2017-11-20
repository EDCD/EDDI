using System.Collections.Generic;
using System.Linq;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Ring compositions
    /// </summary>
    public class Composition
    {
        private static readonly List<Composition> COMPOSITIONS = new List<Composition>();

        public string edname { get; private set; }

        public string name { get; private set; }

        public Composition() { }

        private Composition(string edname, string name)
        {
            this.edname = edname;
            this.name = name;

            COMPOSITIONS.Add(this);
        }

        public static readonly Composition Icy= new Composition("eRingClass_Icy", "Icy");
        public static readonly Composition Rocky = new Composition("eRingClass_Rocky", "Rocky");
        public static readonly Composition Metallic = new Composition("eRingClass_Metalic", "Metallic");
        public static readonly Composition MetalRich = new Composition("eRingClass_MetalRich", "Metal-rich");

        public static Composition FromName(string from)
        {
            if (from == null)
            {
                return null;
            }

            Composition result = COMPOSITIONS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown composition name " + from);
            }
            return result;
        }

        public static Composition FromEDName(string from)
        {
            if (from == null)
            {
                return null;
            }

            string tidiedFrom = from.ToLowerInvariant();
            Composition result = COMPOSITIONS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown composition ED name " + from);
                result = new EddiDataDefinitions.Composition(from, tidiedFrom);
            }
            return result;
        }
    }
}
