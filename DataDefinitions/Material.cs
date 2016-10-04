using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// Material definitions
    /// </summary>
    public class Material
    {
        private static readonly List<Material> MATERIALS = new List<Material>();

        public string category { get; private set; }

        public string EDName { get; private set; }

        public Rarity rarity { get; private set; }

        public string name { get; private set; }

        // Only for elements
        public string symbol { get; private set; }

        public decimal? goodpctbody { get; private set; }

        public decimal? greatpctbody { get; private set; }

        private Material(string EDName, string category, string name, Rarity rarity, string symbol = null, decimal? goodpctbody = null, decimal? greatpctbody = null)
        {
            this.EDName = EDName;
            this.category = category;
            this.symbol= symbol;
            this.name = name;
            this.rarity = rarity;
            this.goodpctbody = goodpctbody;
            this.greatpctbody = greatpctbody;

            MATERIALS.Add(this);
        }

        public static readonly Material Carbon = new Material("carbon", "Element", "Carbon", Rarity.VeryCommon, "C", 17.5M, 22.5M);
        public static readonly Material Iron = new Material("iron", "Element", "Iron", Rarity.VeryCommon, "Fe", 20, 35);
        public static readonly Material Nickel = new Material("nickel", "Element", "Nickel", Rarity.VeryCommon, "Ni", 17.5M, 25);
        public static readonly Material Phosphorus = new Material("phosphorus", "Element", "Phosphorus", Rarity.VeryCommon, "P", 12.5M, 15);
        public static readonly Material Sulphur = new Material("sulphur", "Element", "Sulphur", Rarity.VeryCommon, "S", 22.5M, 27.5M);

        public static readonly Material Chromium = new Material("chromium", "Element", "Chromium", Rarity.Common, "Cr", 10, 15);
        public static readonly Material Germanium = new Material("germanium", "Element", "Germanium", Rarity.Common, "Ge", 5, 6);
        public static readonly Material Manganese = new Material("manganese", "Element", "Manganese", Rarity.Common, "Mn", 10, 15);
        public static readonly Material Vanadium = new Material("vanadium", "Element", "Vanadium", Rarity.Common, "V", 5, 9);
        public static readonly Material Zinc = new Material("zinc", "Z", "Zinc", Rarity.Common, "Element", 5, 9);

        public static readonly Material Arsenic = new Material("arsenic", "Element", "Arsenic", Rarity.Standard, "As", 1.8M, 2.4M);
        public static readonly Material Niobium = new Material("niobium", "Element", "Niobium", Rarity.Standard, "Nb", 1, 2);
        public static readonly Material Selenium = new Material("selenium", "Element", "Selenium", Rarity.Standard, "Se", 3.5M, 4.3M);
        public static readonly Material Tungsten = new Material("tungsten", "Element", "Tungsten", Rarity.Standard, "W", 1, 1.8M);
        public static readonly Material Zirconium = new Material("zirconium", "Element", "Zirconium", Rarity.Standard, "Zr", 2.5M, 4);

        public static readonly Material Cadmium = new Material("cadmium", "Element", "Cadmiun", Rarity.Rare, "Cd", 1.6M, 2);
        public static readonly Material Mercury = new Material("mercury", "Element", "Mercury", Rarity.Rare, "Hg", 1, 1.5M);
        public static readonly Material Molybdenum = new Material("molybdenum", "Element", "Molybdenum", Rarity.Rare, "Mo", 1.5M, 2);
        public static readonly Material Tin = new Material("tin", "Element", "Tin", Rarity.Rare, "Sn", 1.3M, 2);
        public static readonly Material Yttrium = new Material("yttrium", "Element", "Yttrium", Rarity.Rare, "Y", 1, 1.5M);

        public static readonly Material Antimony = new Material("antimony", "Element", "Antimony", Rarity.VeryRare, "Sb", 1, 1.3M);
        public static readonly Material Polonium = new Material("polonium", "Element", "Polonium", Rarity.VeryRare, "Po", 0.5M, 1);
        public static readonly Material Ruthenium = new Material("ruthenium", "Element", "Ruthenium", Rarity.VeryRare, "Ru", 1.4M, 2);
        public static readonly Material Technetium = new Material("technetium", "Element", "Technetium", Rarity.VeryRare, "Tc", 0.8M, 1);
        public static readonly Material Tellurium = new Material("tellurium", "Element", "Tellurium", Rarity.VeryRare, "Te", 1, 1.4M);


        public static Material FromName(string from)
        {
            Material result = MATERIALS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown material name " + from);
            }
            return result;
        }

        public static Material FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();
            Material result = MATERIALS.FirstOrDefault(v => v.EDName.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown material ED name " + from);
            }
            return result;
        }

        public static Material FromSymbol(string from)
        {
            Material result = MATERIALS.FirstOrDefault(v => v.symbol == from);
            if (result == null)
            {
                Logging.Report("Unknown material rank " + from);
            }
            return result;
        }
    }
}
