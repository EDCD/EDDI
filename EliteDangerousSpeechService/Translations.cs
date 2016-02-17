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

        private static Dictionary<string, string[]> STAR_SYSTEM_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Achenar", new string[] { "ˈakɜːnɑ" } },
            { "Acihault", new string[] { "əˈsɪhɔːt" } },
            { "Alcyone", new string[] { "ælˈsaɪəniː" } },
            { "Aldebaran", new string[] { "alˈdɛbəɹən" } },
            { "Arque", new string[] { "ɑːrk" } },
            { "Asterope", new string[] { "əˈstɛroʊpiː" } },
            { "Atlas", new string[] {  "ˈætləs" } },
            { "Aulin", new string[] { "ˈɔːlɪn" } },
            { "Celaeno", new string[] {  "sᵻˈliːnoʊ" }  },
            { "Cygnus", new string[] { "ˈsɪɡnəs" }  },
            { "Diso ", new string[] { "ˈdiːsəʊ" } },
            { "Electra", new string[] { "ᵻˈlɛktrə" } },
            { "Eravate" , new string[] { "ɛrəˈvɑːtˌeɪ" } },
            { "Eranin" , new string[] { "ˈɛrənin" } },
            { "i Bootis" , new string[] { "aɪ", "bəʊˈəʊtɪs" } },
            { "Lave", new string[] { "leɪv" } },
            { "Leesti", new string[] { "ˈliːstiː" } },
            { "Maia", new string[] { "ˈmaɪ.ə" } },
            { "Merope", new string[] { "ˈmɛrəpiː" } },
            { "Nuenets", new string[] { "ˈnjuːənɛts" } },
            { "Okinura", new string[] { "ɒkɪˈnjʊrə" } },
            { "Orrere", new string[] { "ɒrˈɪər" } },
            { "Pleione", new string[] { "ˈplaɪəniː" } },
            { "Reorte", new string[] { "ˌriːˈɔːt" } },
            { "Shinrarta Dezhra", new string[] { "ʃɪnˈrɑːrtə", "ˈdezɦrə" } },
            { "Taygeta", new string[] { "teɪˈɪdʒᵻtə" } },
            { "Xihe", new string[] { "ʃiː.hər" } },
            { "Zaonce", new string[] { "ˈzeɪɒns" } },
        };

        private static Dictionary<string, string[]> CONSTELLATION_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Alrai" , new string[] { "ˈalraɪ" } },
            { "Antliae" , new string[] { "ˈæntlɪˌiː" } },
            { "Aquarii" , new string[] { "əˈkwɛərɪˌaɪ" } },
            { "Arietis" , new string[] { "əˈraɪɪtɪs" } },
            { "Bei Dou" , new string[] { "beɪ", "ˈduː" } },
            { "Blanco" , new string[] { "blæŋkˌəʊ" } },
            { "Capricorni" , new string[] { "ˌkæprɪˈkɔːnaɪ" } },
            { "Cepheus" , new string[] { "ˈsiːfjuːs" } },
            { "Cephei" , new string[] { "ˈsiːfɪˌaɪ" } },
            { "Ceti" , new string[] { "ˈsiːtaɪ" } },
            { "Chi Persei" , new string[] { "kaɪ", "ˈpɜːsɪˌaɪ" } },
            { "Crucis" , new string[] { "ˈkruːsɪs" } },
            { "Cygni" , new string[] { "ˈsɪɡnaɪ" } },
            { "Eta Carina" , new string[] { "ˈiːtə", "kəˈriːnə" } },
            { "Herculis" , new string[] { "hɜːkjʊˈlɪs" } },
            { "Hyades" , new string[] { "ˈhaɪəˌdiːz" } },
            { "Hydrae" , new string[] { "ˈhaɪdriː" } },
            { "Lupus" , new string[] { "ˈluːpəs" } },
            { "Lyncis" , new string[] { "ˈlɪnsɪs" } },
            { "Omega" , new string[] { "ˈəʊmɪɡə" } },
            { "Ophiuchus" , new string[] { "ɒˈfjuːkəs" } },
            { "Pegasi" , new string[] { "ˈpɛɡəˌsaɪ" } },
            { "Persei" , new string[] { "ˈpɜːsɪˌaɪ" } },
            { "Piscium" , new string[] { "ˈpaɪsɪəm" } },
            { "Pleiades" , new string[] { "ˈplaɪədiːz" } },
            { "Puppis" , new string[] { "ˈpʌpɪs" } },
            { "Rho Ophiuchi" , new string[] { "rəʊ", "ɒˈfjuːkaɪ" } },
            { "Sagittarius", new string[] { "ˌsædʒˈtɛəriəs" } },
            { "Scorpii", new string[] { "ˈskɔːpɪˌaɪ" } },
            { "Shui Wei", new string[] { "ˈʃuːi", "weɪ" } },
            { "Tascheter", new string[] { "ˈtɑːʃətɜː" } },
            { "Trianguli", new string[] { "traɪˈæŋˌɡjʊˌlaɪ" } },
            { "Trifid", new string[] { "ˈtraɪfɪd" } },
            { "Tucanae", new string[] { "tuːˈkɑːniː" } },
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
            if (STAR_SYSTEM_PRONUNCIATIONS.ContainsKey(starSystem))
            {
                return replaceWithPronunciation(starSystem, STAR_SYSTEM_PRONUNCIATIONS[starSystem]);
            }

            // Common star catalogues
            if (starSystem.StartsWith("HIP "))
            {
                starSystem = starSystem.Replace("HIP ", "Hip ");
            }
            else if (starSystem.StartsWith("L ")
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
            else if (starSystem.StartsWith("Gliese "))
            {
                starSystem = starSystem.Replace(".", " point ");
            }
            else if (SECTOR.IsMatch(starSystem))
            {
                // Generated star systems
                // Need to handle the pieces before and after the sector marker separately
                Match Match = SECTOR.Match(starSystem);

                // Fix common names
                string firstPiece = Match.Groups[1].Value
                    .Replace("Col ", "Coll ")
                    .Replace("R CrA ", "R CRA ")
                    .Replace("Tr ", "TR ")
                    .Replace("Skull and Crossbones Neb. ", "Skull and Crossbones ")
                    .Replace("(", "").Replace(")", "");

                // Various items between the sector name and 'Sector' need to be removed to allow us to find the base pronunciation
                string subPiece = "";
                if (firstPiece.EndsWith(" Dark Region B Sector"))
                {
                    firstPiece = firstPiece.Replace(" Dark Region B Sector", "");
                    subPiece = " Dark Region B Sector";
                }
                else if (firstPiece.EndsWith(" Sector"))
                {
                    firstPiece = firstPiece.Replace(" Sector", "");
                    subPiece = " Sector";
                }

                // Might be a name that we need to translate
                if (CONSTELLATION_PRONUNCIATIONS.ContainsKey(firstPiece))
                {
                    firstPiece = replaceWithPronunciation(firstPiece, CONSTELLATION_PRONUNCIATIONS[firstPiece]);
                }

                // TODO need to break apart any letter combinations to stop voices from guessing pronunciation incorrectly

                string secondPiece = Match.Groups[2].Value.Replace("-", " dash ");

                starSystem = firstPiece + subPiece + " " + secondPiece;
            }
            else if (starSystem.StartsWith("2MASS ")
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
                // Star systems with +/- (and sometimes .)
                starSystem = starSystem.Replace("Csi ", "CSI ")
                                       .Replace("WISE ", "Wise ")
                                       .Replace("2MASS ", "2 mass ")
                                       .Replace("+", " plus ")
                                       .Replace("-", " minus ")
                                       .Replace(".", " point ");
            }
            else
            {
                // It's possible that the name contains a constellation, in which case translate it
                string[] pieces = starSystem.Split(' ');
                for (int i = 0; i < pieces.Length; i++)
                {
                    if (CONSTELLATION_PRONUNCIATIONS.ContainsKey(pieces[i]))
                    {
                        pieces[i] = replaceWithPronunciation(pieces[i], CONSTELLATION_PRONUNCIATIONS[pieces[i]]);
                    }
                }
                starSystem = string.Join(" ", pieces);
            }

            // Fix up digit strings.  
            // Any digits after a decimal point are broken in to individual digits
            starSystem = DECIMAL_DIGITS.Replace(starSystem, match => match.Groups[1].Value + string.Join<char>(" ", match.Groups[2].Value));
            // Any string of more than two digits is broken up in to individual digits
            starSystem = DIGITS.Replace(starSystem, match => string.Join<char>(" ", match.Value));

            return starSystem;
        }

        private static string replaceWithPronunciation(string sourcePhrase, string[] pronunciation)
        {
            StringBuilder sb = new StringBuilder();
            int i = 0;
            foreach (string source in sourcePhrase.Split(' '))
            {
                if (i > 0)
                {
                    sb.Append(" ");
                }
                sb.Append("<phoneme alphabet=\"ipa\" ph=\"");
                sb.Append(pronunciation[i++]);
                sb.Append("\">");
                sb.Append(source);
                sb.Append("</phoneme>");
            }
            return sb.ToString();
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
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈælfə\">alpha</phoneme>");
                        break;
                    case 'B':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈbrɑːˈvo\">bravo</phoneme>");
                        break;
                    case 'C':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈtʃɑːli\">charlie</phoneme>");
                        break;
                    case 'D':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈdeltɑ\">delta</phoneme>");
                        break;
                    case 'E':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈeko\">echo</phoneme>");
                        break;
                    case 'F':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈfɒkstrɒt\">foxtrot</phoneme>");
                        break;
                    case 'G':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ɡɒlf\">golf</phoneme>");
                        break;
                    case 'H':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"hoːˈtel\">hotel</phoneme>");
                        break;
                    case 'I':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈindiˑɑ\">india</phoneme>");
                        break;
                    case 'J':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈdʒuːliˑˈet\">juliet</phoneme>");
                        break;
                    case 'K':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈkiːlo\">kilo</phoneme>");
                        break;
                    case 'L':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈliːmɑ\">lima</phoneme>");
                        break;
                    case 'M':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"maɪk\">mike</phoneme>");
                        break;
                    case 'N':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"noˈvembə\">november</phoneme>");
                        break;
                    case 'O':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈɒskə\">oscar</phoneme>");
                        break;
                    case 'P':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"pəˈpɑ\">papa</phoneme>");
                        break;
                    case 'Q':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"keˈbek\">quebec</phoneme>");
                        break;
                    case 'R':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈroːmiˑo\">romeo</phoneme>");
                        break;
                    case 'S':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"siˈerə\">sierra</phoneme>");
                        break;
                    case 'T':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈtænɡo\">tango</phoneme>");
                        break;
                    case 'U':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈjuːnifɔːm\">uniform</phoneme>");
                        break;
                    case 'V':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈvɪktə\">victor</phoneme>");
                        break;
                    case 'W':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈwiski\">whiskey</phoneme>");
                        break;
                    case 'X':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈeksˈrei\">x-ray</phoneme>");
                        break;
                    case 'Y':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈjænki\">yankee</phoneme>");
                        break;
                    case 'Z':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈzuːluː\">zulu</phoneme>");
                        break;
                    case '0':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈzɪərəʊ\">zero</phoneme>");
                        break;
                    case '1':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈwʌn\">one</phoneme>");
                        break;
                    case '2':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈtuː\">two</phoneme>");
                        break;
                    case '3':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈtriː\">tree</phoneme>");
                        break;
                    case '4':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈfoʊ.ər\">fawer</phoneme>");
                        break;
                    case '5':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈfaɪf\">fife</phoneme>");
                        break;
                    case '6':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈsɪks\">six</phoneme>");
                        break;
                    case '7':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈsɛvɛn\">seven</phoneme>");
                        break;
                    case '8':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈeɪt\">eight</phoneme>");
                        break;
                    case '9':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈnaɪnər\">niner</phoneme>");
                        break;
                }
            }

            return elements.Aggregate((i, j) => i + " " + j);
        }
    }
}
