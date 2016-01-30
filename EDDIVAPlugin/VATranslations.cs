using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EDDIVAPlugin
{
    /// <summary>Translations for VoiceAttack</summary>
    public class VATranslations
    {
        /// <summary>Fix up power names</summary>
        public static string Power(string power)
        {
            switch (power)
            {
                case "Aisling Duval":
                    return "Ashling Du-val";
                case "Arissa Lavigny-Duval":
                    return "Arissa Lavigny Du-val";
                case "Denton Patreus":
                    return "Denton Patreyus";
                case "Edmund Mahon":
                    return "Edmund Mahonn";
                case "Zemina Torval":
                    return "Zemeena Torvalll";
                default:
                    return power;


            }
        }

        // DIGIT matches a digit with a following digit, plus or minus sign, or period
        private static Regex DIGIT = new Regex("([0-9])(?=[0-9+-\\.])");
        /// <summary>Fix up stars ystem names</summary>
        public static string StarSystem(string starSystem)
        {
            if (starSystem.StartsWith("HIP "))
            {
                return DIGIT.Replace(starSystem.Replace("HIP ", "Hip "), "$1 ");
            }
            if (starSystem.StartsWith("L ") ||
                starSystem.StartsWith("LFT ") ||
                starSystem.StartsWith("LHS ") ||
                starSystem.StartsWith("LTT ") ||
                starSystem.StartsWith("NLTT ") ||
                starSystem.StartsWith("LPM ") ||
                starSystem.StartsWith("PPM ") ||
                starSystem.StartsWith("ADS ")                 )
            {
                return DIGIT.Replace(starSystem, "$1 ").Replace("-", "dash ");
            }
            if (starSystem.StartsWith("Gliese "))
            {
                return DIGIT.Replace(starSystem, "$1 ").Replace(".", "point ");
            }
            if (starSystem.StartsWith("WISE "))
            {
                return DIGIT.Replace(starSystem.Replace("WISE ", "Wise "), "$1 ");
            }
            if (starSystem.StartsWith("2MASS J"))
            {
                return DIGIT.Replace(starSystem.Replace("2MASS J", "2 mass J "), "$1 ").Replace("+", "plus ").Replace("-", "minus ");
            }
            if (starSystem.StartsWith("2MASS"))
            {
                return DIGIT.Replace(starSystem.Replace("2MASS", "2 mass"), "$1 ").Replace("+", "plus ").Replace("-", "minus ");
            }
            if (starSystem.StartsWith("CXOU J"))
            {
                return DIGIT.Replace(starSystem.Replace("CXOU J", "CXOU J "), "$1 ").Replace("+", "plus ").Replace("-", "minus ").Replace(".", "point ");
            }
            if (starSystem.StartsWith("SDSS J"))
            {
                return DIGIT.Replace(starSystem.Replace("SDSS J", "SDSS J "), "$1 ").Replace("+", "plus ").Replace("-", "minus ");
            }
            if (starSystem.StartsWith("UGCS J"))
            {
                return DIGIT.Replace(starSystem.Replace("UGCS J", "UGCS J "), "$1 ").Replace("+", "plus ").Replace("-", "minus ").Replace(".", "point ");
            }
            if (starSystem.StartsWith("XTE J"))
            {
                return DIGIT.Replace(starSystem.Replace("XTE J", "XTE J "), "$1 ").Replace("+", "plus ").Replace("-", "minus ").Replace(".", "point ");
            }
            if (starSystem.StartsWith("XTE PSR J"))
            {
                return DIGIT.Replace(starSystem.Replace("XTE PSR J", "XTE PSR J "), "$1 ").Replace("+", "plus ").Replace("-", "minus ").Replace(".", "point ");
            }
            if (starSystem.StartsWith("BD"))
            {
                return DIGIT.Replace(starSystem.Replace("BD+", "BD plus ").Replace("BD-", "BD minus "), "$1 ").Replace("+", "plus ").Replace("-", "minus ");
            }
            if (starSystem.StartsWith("LHS ") ||
                starSystem.StartsWith("HR ") ||
                starSystem.StartsWith("HD ") ||
                starSystem.StartsWith("AC +") ||
                starSystem.StartsWith("AG+") ||
                starSystem.StartsWith("CFBDSIR") ||
                starSystem.StartsWith("CSI") ||
                starSystem.ToUpper().StartsWith("CSI"))
            {
                // Star catalogues that use + and - as plus and minus
                return DIGIT.Replace(starSystem.ToUpper(), "$1 ").Replace("+", "plus ").Replace("-", "minus ");
            }
            if (Regex.IsMatch(starSystem, "^V[0-9]"))
            {
                return DIGIT.Replace(starSystem.Replace("V", "V "), "$1 ").Trim();
            }
            return starSystem;
        }
    }
}
