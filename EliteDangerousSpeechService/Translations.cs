using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EliteDangerousSpeechService
{
    /// <summary>Translations for Elite items for text-to-speech</summary>
    public class Translations
    {
        /// <summary>Fix up ship models</summary>
        public static string ShipModel(string model)
        {
            if (model == null)
            {
                return null;
            }

            switch (model)
            {
                case "Cobra Mk. III":
                    return "Cobra Mark 3";
                case "Cobra Mk. IV":
                    return "Cobra Mark 4";
                case "Viper Mk. III":
                    return "Viper Mark 3";
                case "Viper Mk. IV":
                    return "Viper Mark 4";
                default:
                    return model;
            }
        }

        /// <summary>Fix up power names</summary>
        public static string Power(string power)
        {
            if (power == null)
            {
                return null;
            }

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

        private static Dictionary<string, string[]> ASTRONOMY_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            // Star systems
            { "Achenar", new string[] { "ˈakɜːnɑ" } },
            { "Alcyone", new string[] { "ælˈsaɪəniː" } },
            { "Arque", new string[] { "ɑːrk" } },
            { "Asterope", new string[] { "əˈstɛroʊpiː" } },
            { "Atlas", new string[] {  "ˈætləs" } },
            { "Celaeno", new string[] {  "sᵻˈliːnoʊ" }  },
            { "Cygnus", new string[] { "ˈsɪɡnəs" }  },
            { "Diso ", new string[] { "ˈdiːsəʊ" } },
            { "Electra", new string[] { "ᵻˈlɛktrə" } },
            { "Lave", new string[] { "leɪv" } },
            { "Leesti", new string[] { "ˈliːstiː" } },
            { "Maia", new string[] { "ˈmaɪ.ə" } },
            { "Merope", new string[] { "ˈmɛrəpiː" } },
            { "Nuenets", new string[] { "ˈnjuːənɛts" } },
            { "Orrere", new string[] { "ɒrˈɪər" } },
            { "Pleione", new string[] { "ˈplaɪəniː" } },
            { "Reorte", new string[] { "ˌriːˈɔːt" } },
            { "Shinrarta Dezhra", new string[] { "ʃɪnˈrɑːrtə", "ˈdezɦrə" } },
            { "Taygeta", new string[] { "teɪˈɪdʒᵻtə" } },
            { "Zaonce", new string[] { "ˈzeɪɒns" } },

            // Sectors
            { "Pleiades" , new string[] { "ˈplaɪədiːz" } },
            { "Sagittarius", new string[] { "ˌsædʒˈtɛəriəs" } },
        };

        private static Regex DIGITS = new Regex(@"\d{3,}");
        private static Regex DECIMAL_DIGITS = new Regex(@"( point )(\d{2,})");
        // Regular expression to locate generated star systems
        private static Regex SECTOR = new Regex("(.*) ([A-Za-z][A-Za-z]-[A-Za-z] .*)");
        /// <summary>Fix up star system names</summary>
        public static string StarSystem(string starSystem)
        {
            if (starSystem == null)
            {
                return null;
            }

            // Specific translations
            if (ASTRONOMY_PRONUNCIATIONS.ContainsKey(starSystem))
            {
                string[] fragmentTranslations = ASTRONOMY_PRONUNCIATIONS[starSystem];

                // Need to break down in to individual words
                StringBuilder sb = new StringBuilder();
                int i = 0;
                foreach (string starSystemFragment in starSystem.Split(' '))
                {
                    if (i > 0)
                    {
                        sb.Append(" ");
                    }
                    sb.Append("<phoneme alphabet=\"ipa\" ph=\"");
                    sb.Append(fragmentTranslations[i++]);
                    sb.Append("\">");
                    sb.Append(starSystemFragment);
                    sb.Append("</phoneme>");
                }
                return sb.ToString();
            }

            // Common star catalogues
            if (starSystem.StartsWith("HIP "))
            {
                starSystem = starSystem.Replace("HIP ", "Hip ");
            }
            if (starSystem.StartsWith("L ")
                || starSystem.StartsWith("LFT ")
                || starSystem.StartsWith("LHS ")
                || starSystem.StartsWith("LP ")
                || starSystem.StartsWith("LTT ")
                || starSystem.StartsWith("NLTT ")
                || starSystem.StartsWith("LPM ")
                || starSystem.StartsWith("PPM ")
                || starSystem.StartsWith("ADS ")
                || starSystem.StartsWith("HR ")
                || starSystem.StartsWith("HD ")
                )
            {
                starSystem = starSystem.Replace("-", "dash ");
            }
            if (starSystem.StartsWith("Gliese "))
            {
                starSystem = starSystem.Replace(".", " point ");
            }

            // Generated star systems
            if (SECTOR.IsMatch(starSystem))
            {
                // Need to handle the pieces before and after the sector marker separately
                Match Match = SECTOR.Match(starSystem);

                // Fix common names
                string firstPiece = Match.Groups[1].Value
                    .Replace("Col ", "Coll ")
                    .Replace("R CrA ", "R CRA ")
                    .Replace("Tr ", "TR ")
                    .Replace("Skull and Crossbones Neb. ", "Skull and Crossbones ")
                    .Replace("(", "").Replace(")", "");

                string secondPiece = Match.Groups[2].Value.Replace("-", " dash ");

                starSystem = firstPiece + " " + secondPiece;
            }

            // Star systems with +/- (and sometimes .)
            if (starSystem.StartsWith("2MASS ")
                || starSystem.StartsWith("AC ")
                || starSystem.StartsWith("AG") // Note no space
                || starSystem.StartsWith("BD")
                || starSystem.StartsWith("CFBDSIR ")
                || starSystem.StartsWith("CXOONC ")
                || starSystem.StartsWith("CXOU ")
                || starSystem.StartsWith("CSI") // Note no space
                || starSystem.StartsWith("Csi") // Note no space
                || starSystem.StartsWith("IDS ")
                || starSystem.StartsWith("LF ")
                || starSystem.StartsWith("MJD95 ")
                || starSystem.StartsWith("SDSS ")
                || starSystem.StartsWith("UGCS ")
                || starSystem.StartsWith("WISE ")
                || starSystem.StartsWith("XTE ")
                )
            {
                starSystem = starSystem.Replace("Csi ", "CSI ")
                                       .Replace("WISE ", "Wise ")
                                       .Replace("2MASS ", "2 mass ")
                                       .Replace("+", " plus ")
                                       .Replace("-", " minus ")
                                       .Replace(".", " point ");
            }

            // Fix up digit strings.  
            // Any digits after a decimal point are broken in to individual digits
            starSystem = DECIMAL_DIGITS.Replace(starSystem, match => match.Groups[1].Value + string.Join<char>(" ", match.Groups[2].Value));
            // Any string of more than two digits is broken up in to individual digits
            starSystem = DIGITS.Replace(starSystem, match => string.Join<char>(" ", match.Value));

            return starSystem;
        }

        public static string CallSign(string callsign)
        {
            if (callsign == null)
            {
                return null;
            }

            List<string> elements = new List<string>();
            foreach (char c in callsign)
            {
                switch (c)
                {
                    case 'A':
                        elements.Add("alpha");
                        break;
                    case 'B':
                        elements.Add("bravo");
                        break;
                    case 'C':
                        elements.Add("charlie");
                        break;
                    case 'D':
                        elements.Add("delta");
                        break;
                    case 'E':
                        elements.Add("echo");
                        break;
                    case 'F':
                        elements.Add("foxtrot");
                        break;
                    case 'G':
                        elements.Add("golf");
                        break;
                    case 'H':
                        elements.Add("hotel");
                        break;
                    case 'I':
                        elements.Add("india");
                        break;
                    case 'J':
                        elements.Add("juliette");
                        break;
                    case 'K':
                        elements.Add("kilo");
                        break;
                    case 'L':
                        elements.Add("lima");
                        break;
                    case 'M':
                        elements.Add("mike");
                        break;
                    case 'N':
                        elements.Add("november");
                        break;
                    case 'O':
                        elements.Add("oscar");
                        break;
                    case 'P':
                        elements.Add("Pappa");
                        break;
                    case 'Q':
                        elements.Add("quebec");
                        break;
                    case 'R':
                        elements.Add("romeo");
                        break;
                    case 'S':
                        elements.Add("sierra");
                        break;
                    case 'T':
                        elements.Add("tango");
                        break;
                    case 'U':
                        elements.Add("uniform");
                        break;
                    case 'V':
                        elements.Add("victor");
                        break;
                    case 'W':
                        elements.Add("whisky");
                        break;
                    case 'X':
                        elements.Add("x-ray");
                        break;
                    case 'Y':
                        elements.Add("yankee");
                        break;
                    case 'Z':
                        elements.Add("zulu");
                        break;
                    case '0':
                        elements.Add("zero");
                        break;
                    case '1':
                        elements.Add("one");
                        break;
                    case '2':
                        elements.Add("two");
                        break;
                    case '3':
                        elements.Add("tree");
                        break;
                    case '4':
                        elements.Add("fawer");
                        break;
                    case '5':
                        elements.Add("fife");
                        break;
                    case '6':
                        elements.Add("six");
                        break;
                    case '7':
                        elements.Add("seven");
                        break;
                    case '8':
                        elements.Add("eight");
                        break;
                    case '9':
                        elements.Add("niner");
                        break;
                }
            }

            return elements.Aggregate((i, j) => i + " " + j);
        }
    }
}
