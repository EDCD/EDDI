using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Materials
    /// </summary>
    public class Material
    {
        private static readonly List<Material> MATERIALS = new List<Material>();

        public string edname { get; private set; }

        public string symbol { get; private set; }

        public Rarity rarity { get; private set; }

        public string name { get; private set; }

        public decimal goodpctbody { get; private set; }

        public decimal greatpctbody { get; private set; }

        private Material(string edname, string symbol, string name, Rarity rarity, decimal goodpctbody, decimal greatpctbody)
        {
            this.edname = edname;
            this.symbol= symbol;
            this.name = name;
            this.rarity = rarity;
            this.goodpctbody = goodpctbody;
            this.greatpctbody = greatpctbody;

            MATERIALS.Add(this);
        }

        public static readonly Material Carbon = new Material("carbon", "C", "Carbon", Rarity.VeryCommon, 17.5M, 22.5M);
        public static readonly Material Iron = new Material("iron", "Fe", "Iron", Rarity.VeryCommon, 20, 35);
        public static readonly Material Nickel = new Material("nickel", "Ni", "Nickel", Rarity.VeryCommon, 17.5M, 25);
        public static readonly Material Phosphorus = new Material("phosphorus", "P", "Phosphorus", Rarity.VeryCommon, 12.5M, 15);
        public static readonly Material Sulphur = new Material("sulphur", "S", "Sulphur", Rarity.VeryCommon, 22.5M, 27.5M);

        public static readonly Material Chromium = new Material("chromium", "Cr", "Chromium", Rarity.Common, 10, 15);
        public static readonly Material Germanium = new Material("germanium", "Ge", "Germanium", Rarity.Common, 5, 6);
        public static readonly Material Manganese = new Material("manganese", "Mn", "Manganese", Rarity.Common, 10, 15);
        public static readonly Material Vanadium = new Material("vanadium", "V", "Vanadium", Rarity.Common, 5, 9);
        public static readonly Material Zinc = new Material("zinc", "Z", "Zinc", Rarity.Common, 5, 9);

        public static readonly Material Arsenic = new Material("arsenic", "As", "Arsenic", Rarity.Standard, 1.8M, 2.4M);
        public static readonly Material Niobium = new Material("niobium", "Nb", "Niobium", Rarity.Standard, 1, 2);
        public static readonly Material Selenium = new Material("selenium", "Se", "Selenium", Rarity.Standard, 3.5M, 4.3M);
        public static readonly Material Tungsten= new Material("tungsten", "W", "Tungsten", Rarity.Standard, 1, 1.8M);
        public static readonly Material Zirconium = new Material("zirconium", "Zr", "Zirconium", Rarity.Standard, 2.5M, 4);

        public static readonly Material Cadmium = new Material("cadmium", "Cd", "Cadmiun", Rarity.Rare, 1.6M, 2);
        public static readonly Material Mercury = new Material("mercury", "Hg", "Mercury", Rarity.Rare, 1, 1.5M);
        public static readonly Material Molybdenum = new Material("molybdenum", "Mo", "Molybdenum", Rarity.Rare, 1.5M, 2);
        public static readonly Material Tin = new Material("tin", "Sn", "Tin", Rarity.Rare, 1.3M, 2);
        public static readonly Material Yttrium = new Material("yttrium", "Y", "Yttrium", Rarity.Rare, 1, 1.5M);

        public static readonly Material Antimony= new Material("antimony", "Sb", "Antimony", Rarity.VeryRare, 1, 1.3M);
        public static readonly Material Polonium = new Material("polonium", "Po", "Polonium", Rarity.VeryRare, 0.5M, 1);
        public static readonly Material Ruthenium = new Material("ruthenium", "Ru", "Ruthenium", Rarity.VeryRare, 1.4M, 2);
        public static readonly Material Technetium = new Material("technetium", "Tc", "Technetium", Rarity.VeryRare, 0.8M, 1);
        public static readonly Material Tellurium = new Material("tellurium", "Te", "Tellurium", Rarity.VeryRare, 1, 1.4M);


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
            Material result = MATERIALS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
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
