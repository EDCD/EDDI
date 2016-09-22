using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousDataDefinitions
{
    /// <summary>
    /// Material definitions
    /// </summary>
    public class MaterialDefinition
    {
        private static readonly List<MaterialDefinition> MATERIALS = new List<MaterialDefinition>();

        public string edname { get; private set; }

        public string symbol { get; private set; }

        public Rarity rarity { get; private set; }

        public string name { get; private set; }

        public decimal goodpctbody { get; private set; }

        public decimal greatpctbody { get; private set; }

        private MaterialDefinition(string edname, string symbol, string name, Rarity rarity, decimal goodpctbody, decimal greatpctbody)
        {
            this.edname = edname;
            this.symbol= symbol;
            this.name = name;
            this.rarity = rarity;
            this.goodpctbody = goodpctbody;
            this.greatpctbody = greatpctbody;

            MATERIALS.Add(this);
        }

        public static readonly MaterialDefinition Carbon = new MaterialDefinition("carbon", "C", "Carbon", Rarity.VeryCommon, 17.5M, 22.5M);
        public static readonly MaterialDefinition Iron = new MaterialDefinition("iron", "Fe", "Iron", Rarity.VeryCommon, 20, 35);
        public static readonly MaterialDefinition Nickel = new MaterialDefinition("nickel", "Ni", "Nickel", Rarity.VeryCommon, 17.5M, 25);
        public static readonly MaterialDefinition Phosphorus = new MaterialDefinition("phosphorus", "P", "Phosphorus", Rarity.VeryCommon, 12.5M, 15);
        public static readonly MaterialDefinition Sulphur = new MaterialDefinition("sulphur", "S", "Sulphur", Rarity.VeryCommon, 22.5M, 27.5M);

        public static readonly MaterialDefinition Chromium = new MaterialDefinition("chromium", "Cr", "Chromium", Rarity.Common, 10, 15);
        public static readonly MaterialDefinition Germanium = new MaterialDefinition("germanium", "Ge", "Germanium", Rarity.Common, 5, 6);
        public static readonly MaterialDefinition Manganese = new MaterialDefinition("manganese", "Mn", "Manganese", Rarity.Common, 10, 15);
        public static readonly MaterialDefinition Vanadium = new MaterialDefinition("vanadium", "V", "Vanadium", Rarity.Common, 5, 9);
        public static readonly MaterialDefinition Zinc = new MaterialDefinition("zinc", "Z", "Zinc", Rarity.Common, 5, 9);

        public static readonly MaterialDefinition Arsenic = new MaterialDefinition("arsenic", "As", "Arsenic", Rarity.Standard, 1.8M, 2.4M);
        public static readonly MaterialDefinition Niobium = new MaterialDefinition("niobium", "Nb", "Niobium", Rarity.Standard, 1, 2);
        public static readonly MaterialDefinition Selenium = new MaterialDefinition("selenium", "Se", "Selenium", Rarity.Standard, 3.5M, 4.3M);
        public static readonly MaterialDefinition Tungsten= new MaterialDefinition("tungsten", "W", "Tungsten", Rarity.Standard, 1, 1.8M);
        public static readonly MaterialDefinition Zirconium = new MaterialDefinition("zirconium", "Zr", "Zirconium", Rarity.Standard, 2.5M, 4);

        public static readonly MaterialDefinition Cadmium = new MaterialDefinition("cadmium", "Cd", "Cadmiun", Rarity.Rare, 1.6M, 2);
        public static readonly MaterialDefinition Mercury = new MaterialDefinition("mercury", "Hg", "Mercury", Rarity.Rare, 1, 1.5M);
        public static readonly MaterialDefinition Molybdenum = new MaterialDefinition("molybdenum", "Mo", "Molybdenum", Rarity.Rare, 1.5M, 2);
        public static readonly MaterialDefinition Tin = new MaterialDefinition("tin", "Sn", "Tin", Rarity.Rare, 1.3M, 2);
        public static readonly MaterialDefinition Yttrium = new MaterialDefinition("yttrium", "Y", "Yttrium", Rarity.Rare, 1, 1.5M);

        public static readonly MaterialDefinition Antimony= new MaterialDefinition("antimony", "Sb", "Antimony", Rarity.VeryRare, 1, 1.3M);
        public static readonly MaterialDefinition Polonium = new MaterialDefinition("polonium", "Po", "Polonium", Rarity.VeryRare, 0.5M, 1);
        public static readonly MaterialDefinition Ruthenium = new MaterialDefinition("ruthenium", "Ru", "Ruthenium", Rarity.VeryRare, 1.4M, 2);
        public static readonly MaterialDefinition Technetium = new MaterialDefinition("technetium", "Tc", "Technetium", Rarity.VeryRare, 0.8M, 1);
        public static readonly MaterialDefinition Tellurium = new MaterialDefinition("tellurium", "Te", "Tellurium", Rarity.VeryRare, 1, 1.4M);


        public static MaterialDefinition FromName(string from)
        {
            MaterialDefinition result = MATERIALS.FirstOrDefault(v => v.name == from);
            if (result == null)
            {
                Logging.Report("Unknown material name " + from);
            }
            return result;
        }

        public static MaterialDefinition FromEDName(string from)
        {
            string tidiedFrom = from == null ? null : from.ToLowerInvariant();
            MaterialDefinition result = MATERIALS.FirstOrDefault(v => v.edname.ToLowerInvariant() == tidiedFrom);
            if (result == null)
            {
                Logging.Report("Unknown material ED name " + from);
            }
            return result;
        }

        public static MaterialDefinition FromSymbol(string from)
        {
            MaterialDefinition result = MATERIALS.FirstOrDefault(v => v.symbol == from);
            if (result == null)
            {
                Logging.Report("Unknown material rank " + from);
            }
            return result;
        }
    }
}
