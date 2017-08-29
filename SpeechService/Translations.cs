using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EddiSpeechService
{
    /// <summary>Translations for Elite items for text-to-speech</summary>
    public class Translations
    {
        /// <summary>Fix up power names</summary>
        public static string Power(string power)
        {
            if (power == null)
            {
                return null;
            }

            switch (power)
            {
                case "Archon Delaine":
                    return "<phoneme alphabet=\"ipa\" ph=\"aʁkɔ̃ \">Archon</phoneme> <phoneme alphabet=\"ipa\" ph=\"delɛn\">Delaine</phoneme>";
                case "Aisling Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"ɛsliŋ\">Aisling</phoneme> <phoneme alphabet=\"ipa\" ph=\"duːˈvæl\">dyval</phoneme>";
                case "Arissa Lavigny-Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"aʁisa\">Arissa</phoneme> <phoneme alphabet=\"ipa\" ph=\"laviɲi\">Lavigny</phoneme> <phoneme alphabet=\"ipa\" ph=\"duːˈvæl\">dyval</phoneme>";
                case "Denton Patreus":
                    return "<phoneme alphabet=\"ipa\" ph=\"ˈdɛntən\">Denton</phoneme> <phoneme alphabet=\"ipa\" ph=\"patʁeys\">Patreus</phoneme>";
                case "Edmund Mahon":
                    return "<phoneme alphabet=\"ipa\" ph=\"ɛdmund\">Edmund</phoneme> <phoneme alphabet=\"ipa\" ph=\"maɔ̃\">Mahon</phoneme>";
                case "Felicia Winters":
                    return "<phoneme alphabet=\"ipa\" ph=\"felisja\">Felicia</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈwɪntəs\">Winters</phoneme>";
                case "Pranav Antal":
                    return "<phoneme alphabet=\"ipa\" ph=\"pʁanav\">Pranav</phoneme> <phoneme alphabet=\"ipa\" ph=\"ɑ̃tal\">Antal</phoneme>";
                case "Zachary Hudson":
                    return "<phoneme alphabet=\"ipa\" ph=\"zakaʁi\">Zachary</phoneme> <phoneme alphabet=\"ipa\" ph=\"ydsɔ̃\">Hudson</phoneme>";
                case "Zemina Torval":
                    return "<phoneme alphabet=\"ipa\" ph=\"zemina\">Zemina</phoneme> <phoneme alphabet=\"ipa\" ph=\"tɔʁval\">Torval</phoneme>";
                case "Li Yong-Rui":
                    return "<phoneme alphabet=\"ipa\" ph=\"li\">Li</phoneme> <phoneme alphabet=\"ipa\" ph=\"jɔ̃ɡ\">Yong</phoneme> <phoneme alphabet=\"ipa\" ph=\"ʁɥi\">Rui</phoneme>";
                case "Yuri Grom":
                    return "<phoneme alphabet=\"ipa\" ph=\"juʁi\">Yuri</phoneme> <phoneme alphabet=\"ipa\" ph=\"ɡʁɔm\">Grom</phoneme>";
                default:
                    return power;
            }
        }

        // Fixes to avoid issues with some of the more strangely-named systems
        private static Dictionary<string, string> STAR_SYSTEM_FIXES = new Dictionary<string, string>()
        {
            { "VESPER-M4", "Vesper M 4" } // Stop Vesper being treated as a sector
        };

        // Fixes to avoid issues with some of the more strangely-named factions
        private static Dictionary<string, string> FACTION_FIXES = new Dictionary<string, string>()
        {
            { "SCORPIONS ORDER", "Scorpions Order" } // Stop it being treated as a sector
        };

        private static Dictionary<string, string[]> STAR_SYSTEM_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Achenar", new string[] { "akenaʁ" } },
            { "Acihault", new string[] { "asiol" } },
            { "Adan", new string[] { "adɑ̃" } },
            { "Alcyone", new string[] { "alsjɔn" } },
            { "Aldebaran", new string[] { "aldebaʁɑ̃" } },
            { "Anemoi", new string[] { "æˈniːmɔɪ" } },
            { "Apoyota", new string[] { "apwajɔta" } },
            { "Arque", new string[] { "aʁk" } },
            { "Asterope", new string[] { "asteʁɔp" } },
            { "Atlas", new string[] {  "atlas" } },
            { "Aulin", new string[] { "olɛ̃" } },
            { "Bunda", new string[] { "bynda" } },
            { "Cayutorme", new string[] { "kɛjytɔʁm" }  },
            { "Celaeno", new string[] {  "səlaeno" }  },
            { "Ceos", new string[] { "seo" }  },
            { "Cygnus", new string[] { "siɲy" }  },
            { "Deciat", new string[] { "desja" } },
            { "Diso ", new string[] { "dizo" } },
            { "Djiwal", new string[] { "dʒiwal" } },
            { "Dvorsi", new string[] { "dvɔʁsi" } },
            { "Electra", new string[] { "elɛktʁa" } },
            { "Eravate" , new string[] { "eʁavat" } },
            { "Eranin" , new string[] { "eʁanɛ̃" } },
            { "Frigaha", new string[] { "fʁiɡaa" } },
            { "Grandmort" , new string[] { "ɡʁɑ̃dmɔʁ" } },
            { "Hecate" , new string[] { "ekat" } },
            { "Hotas" , new string[] { "ɔtas" } },
            { "Isleta" , new string[] { "isleta" } },
            { "i Bootis" , new string[] { "i", "butis" } },
            { "Lave", new string[] { "lav" } },
            { "Kaia Bajaja", new string[] { "kɛa", "baʒaʒa" } },
            { "Kigana", new string[] { "kiɡana" } },
            { "Kini", new string[] { "kini" } },
            { "Kremainn", new string[] { "kʁemɛ̃n" } },
            { "Laksak", new string[] { "laksak" } },
            { "Leesti", new string[] { "listi" } },
            { "Leucos", new string[] { "løko" } },
            { "Maia", new string[] { "Maïa" } },
            { "Mata", new string[] { "mata" } },
            { "Merope", new string[] { "meʁɔp" } },
            { "Mu Koji", new string[] { "my", "kɔʒi" } },
            { "Nuenets", new string[] { "nɥenɛ" } },
            { "Okinura", new string[] { "ɔkinuʁa" } },
            { "Orrere", new string[] { "ɔʁeʁe" } },
            { "Pai Szu", new string[] { "pɛ", "szu" } },
            { "Pleione", new string[] { "plejɔn" } },
            { "Procyon", new string[] { "pʁɔsjɔ̃" } },
            { "Potriti", new string[] { "pɔtʁiti" } },
            { "Reorte", new string[] { "ʁeɔʁt" } },
            { "Sakti", new string[] { "sakti" } },
            { "Shinrarta Dezhra", new string[] { "ʃinʁaʁta", "dɛzʁa" } },
            { "Surya", new string[] { "syʁja" } },
            { "Taygeta", new string[] { "tɛʒta" } },
            { "Tse", new string[] { "tse" } },
            { "Xihe", new string[] { "ksie" } },
            { "Xinca", new string[] { "ksinka" } },
            { "Yakabugai", new string[] { "jakabuɡɛj" } },
            { "Zaonce", new string[] { "zaɔ̃s" } },
            { "Zhang Fei", new string[] { "ʈʂáŋ", "feɪ" } },
        };

        private static Dictionary<string, string[]> CONSTELLATION_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Alrai" , new string[] { "alʁai" } },
            { "Antliae" , new string[] { "ɑ̃tlijae" } },
            { "Aquarii" , new string[] { "vɛʁso" } },
            { "Arietis" , new string[] { "belje" } },
            { "Bei Dou" , new string[] { "bɛ", "du" } },
            { "Blanco" , new string[] { "blɑ̃ko" } },
            { "Bleae Thaa" , new string[] { "bləa", "taa" } },
            { "Bleae Thua" , new string[] { "bləa", "tɥa" } },
            { "Capricorni" , new string[] { "kapʁikɔʁn" } },
            { "Cepheus" , new string[] { "seføs" } },
            { "Cephei" , new string[] { "sefəi" } },
            { "Ceti" , new string[] { "seti" } },
            { "Chi Persei" , new string[] { "ʃi", "pɛʁsei" } },
            { "Crucis" , new string[] { "kʁwa", "dy", "syd" } },
            { "Cygni" , new string[] { "siɲ" } },
            { "Eta Carina" , new string[] { "eta", "kaʁina" } },
            { "Fornacis" , new string[] { "fɔʁnasist" } },
            { "Herculis" , new string[] { "ɛʁkyl" } },
            { "Hyades" , new string[] { "jad" } },
            { "Hydrae" , new string[] { "idʁ" } },
            { "Lupus" , new string[] { "lu" } },
            { "Lyncis" , new string[] { "lɛ̃ks" } },
            { "Omega" , new string[] { "ɔmeɡa" } },
            { "Ophiuchus" , new string[] { "ɒˈfjuːkəs" } },
            { "Pegasi" , new string[] { "peɡaz" } },
            { "Persei" , new string[] { "pɛʁse" } },
            { "Piscium" , new string[] { "pwasɔ̃" } },
            { "Pleiades" , new string[] { "plejad" } },
            { "Puppis" , new string[] { "pup" } },
            { "Pru Euq" , new string[] { "pʁy", "øk"} },
            { "Rho Ophiuchi" , new string[] { "ʁo", "ɔfjyʃy" } },
            { "Sagittarius", new string[] { "saʒitɛʁ" } },
            { "Scorpii", new string[] { "skɔʁpjɔ̃" } },
            { "Shui Wei", new string[] { "ˈʃuːi", "weɪ" } },
            { "Tau Ceti" , new string[] { "to", "seti" } },
            { "Tascheter", new string[] { "ˈtɑːʃətɜː" } },
            { "Trianguli", new string[] { "tʁijɑ̃ɡl" } },
            { "Trifid", new string[] { "ˈtraɪfɪd" } },
            { "Tucanae", new string[] { "tykana" } },
            { "Wredguia", new string[] { "wʁedɡja" } },
        };

        // Various handy regexes so we don't keep recreating them
        private static Regex ALPHA_THEN_NUMERIC = new Regex(@"[A-Za-z][0-9]");
        private static Regex UPPERCASE = new Regex(@"([A-Z]{2,})|(?:([A-Z])(?:\s|$))");
        private static Regex TEXT = new Regex(@"([A-Za-z]{1,3}(?:\s|$))");
        private static Regex DIGIT = new Regex(@"\d+(?:\s|$)");
        private static Regex THREE_OR_MORE_DIGITS = new Regex(@"\d{3,}");
        private static Regex DECIMAL_DIGITS = new Regex(@"( point )(\d{2,})");
        private static Regex SECTOR = new Regex("(.*) ([A-Za-z][A-Za-z]-[A-Za-z] .*)");
        private static Regex PLANET = new Regex(@"[A-Za-z](?=[^A-Za-z])");
        private static Regex SUBSTARS = new Regex(@"^A[BCDE]?[CDE]?[DE]?[E]?|B[CDE]?[DE]?[E]?|C[DE]?[E]?|D[E]?$");
        private static Regex BODY = new Regex(@"^(.*?) ([A-E]+ )?(Belt(?:\s|$)|Cluster(?:\s|$)|\d{1,2}(?:\s|$)|[A-Za-z](?:\s|$)){1,12}$", RegexOptions.IgnoreCase);

        /// <summary>Fix up faction names</summary>
        public static string Faction(string faction)
        {
            if (faction == null)
            {
                return null;
            }

            // Specific fixing of names to avoid later confusion
            if (FACTION_FIXES.ContainsKey(faction))
            {
                faction = FACTION_FIXES[faction];
            }

            // Faction names can contain system names; hunt them down and change them
            foreach (var pronunciation in STAR_SYSTEM_PRONUNCIATIONS)
            {
                if (faction.Contains(pronunciation.Key))
                {
                    var replacement = replaceWithPronunciation(pronunciation.Key, pronunciation.Value);
                    return faction.Replace(pronunciation.Key, replacement);
                }
            }

            return faction;
        }

        /// <summary>Fix up body names</summary>
        public static string Body(string body, bool useICAO = false)
        {
            if (body == null)
            {
                return null;
            }

            List<string> results = new List<string>();

            // Use a regex to break apart the body from the system
            Match match = BODY.Match(body);
            if (!match.Success)
            {
                // There was no match so we pass this as-is
                return body;
            }
            else
            {
                // Parse the starsystem
                results.Add(StarSystem(match.Groups[1].Value.Trim(), useICAO));
                // Parse the body
                for (int i = 2; i < match.Groups.Count; i++)
                {
                    for (int j = 0; j < match.Groups[i].Captures.Count; j++)
                    {
                        Console.WriteLine("Results[" + i + "][" + j + "] is *" + match.Groups[i].Captures[j].Value.Trim() + "*");
                        string part = match.Groups[i].Captures[j].Value.Trim();

                        if (DIGIT.IsMatch(part))
                        {
                            Console.WriteLine("Part " + part + " is digit");
                            // The part is a number; turn it in to ICAO if required
                            results.Add(useICAO ? ICAO(part, true) : part);
                        }
                        else if (PLANET.IsMatch(part))
                        {
                            Console.WriteLine("Part " + part + " is planet");
                            // The part is a planet; turn it in to ICAO if required
                            results.Add(useICAO ? ICAO(part, true) : part);
                        }
                        else if (part == "Belt" || part == "Cluster")
                        {
                            // Pass as-is
                            results.Add(part);
                        }
                        else if (SUBSTARS.IsMatch(part))
                        {
                            Console.WriteLine("Part " + part + " is substars");
                            // The part is uppercase; turn it in to ICAO if required
                            results.Add(UPPERCASE.Replace(part, m => useICAO ? ICAO(m.Value, true) : string.Join<char>(" ", m.Value)));
                        }
                        else if (TEXT.IsMatch(part))
                        {
                            Console.WriteLine("Part " + part + " is text");
                            // The part is uppercase; turn it in to ICAO if required
                            results.Add(useICAO ? ICAO(part) : part);
                        }
                        else
                        {
                            // Pass it as-is
                            results.Add(part);
                        }
                    }
                    Console.WriteLine("Results is " + Regex.Replace(string.Join(" ", results), @"\s+", " "));
                }
            }

            return Regex.Replace(string.Join(" ", results), @"\s+", " ");
        }

        /// <summary>Fix up star system names</summary>
        public static string StarSystem(string starSystem, bool useICAO = false)
        {
            if (starSystem == null)
            {
                return null;
            }

            // Specific fixing of names to avoid later confusion
            if (STAR_SYSTEM_FIXES.ContainsKey(starSystem))
            {
                starSystem = STAR_SYSTEM_FIXES[starSystem];
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
                starSystem = starSystem.Replace("-", " tiré ");
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
                    subPiece = " Dark Region Secteur B";
                }
                else if (firstPiece.EndsWith(" Sector"))
                {
                    firstPiece = firstPiece.Replace(" Sector", "");
                    subPiece = " Secteur";
                }

                // Might be a name that we need to translate
                if (CONSTELLATION_PRONUNCIATIONS.ContainsKey(firstPiece))
                {
                    firstPiece = replaceWithPronunciation(firstPiece, CONSTELLATION_PRONUNCIATIONS[firstPiece]);
                }

                string secondPiece = Match.Groups[2].Value;
                Console.WriteLine("1) Second piece is " + secondPiece);
                if (useICAO)
                {
                    secondPiece = ICAO(secondPiece, true);
                }
                Console.WriteLine("2) Second piece is " + secondPiece);
                secondPiece = secondPiece.Replace("-", " tiré ");
                Console.WriteLine("3) Second piece is " + secondPiece);

                starSystem = firstPiece + subPiece + " " + secondPiece;
            }
            else if (starSystem.StartsWith("2MASS ")
                || starSystem.StartsWith("AC ")
                || starSystem.StartsWith("AG") // Note no space
                || starSystem.StartsWith("BD")
                || starSystem.StartsWith("CFBDSIR ")
                || starSystem.StartsWith("CXOONC ")
                || starSystem.StartsWith("CXOU ")
                || starSystem.StartsWith("CPD") // Note no space
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
                                       .Replace("-", " moins ")
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

            // Any string of an alpha followd by a numeric is broken up
            starSystem = ALPHA_THEN_NUMERIC.Replace(starSystem, match => useICAO ? ICAO(match.Value, true) : string.Join<char>(" ", match.Value));

            // Fix up digit strings
            // Any digits after a decimal point are broken in to individual digits
            starSystem = DECIMAL_DIGITS.Replace(starSystem, match => match.Groups[1].Value + string.Join<char>(" ", useICAO ? ICAO(match.Groups[2].Value, true) : match.Groups[2].Value));
            // Any string of more than two digits is broken up in to individual digits
            starSystem = THREE_OR_MORE_DIGITS.Replace(starSystem, match => useICAO ? ICAO(match.Value, true) : string.Join<char>(" ", match.Value));

            // Any string of upper-case letters is broken up to avoid issues such as 'DR' being pronounced as 'Doctor'
            starSystem = UPPERCASE.Replace(starSystem, match => useICAO ? ICAO(match.Value, true) : string.Join<char>(" ", match.Value));

            return Regex.Replace(starSystem, @"\s+", " ");
        }

        public static string replaceWithPronunciation(string sourcePhrase, string[] pronunciation)
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

        public static string ICAO(string callsign, bool passDash = false)
        {
            if (callsign == null)
            {
                return null;
            }

            List<string> elements = new List<string>();
            foreach (char c in callsign.ToUpperInvariant())
            {
                switch (c)
                {
                    case 'A':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"alfa\">alpha</phoneme>");
                        break;
                    case 'B':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"bʁavo\">bravo</phoneme>");
                        break;
                    case 'C':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ʃaʁli\">charlie</phoneme>");
                        break;
                    case 'D':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"dɛlta\">delta</phoneme>");
                        break;
                    case 'E':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"eko\">echo</phoneme>");
                        break;
                    case 'F':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"fɔkstʁɔt\">foxtrot</phoneme>");
                        break;
                    case 'G':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ɡɔlf\">golf</phoneme>");
                        break;
                    case 'H':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ɔtɛl\">hotel</phoneme>");
                        break;
                    case 'I':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"indja\">india</phoneme>");
                        break;
                    case 'J':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ʒyljɛt\">juliet</phoneme>");
                        break;
                    case 'K':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"kilo\">kilo</phoneme>");
                        break;
                    case 'L':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"lima\">lima</phoneme>");
                        break;
                    case 'M':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"maɪk\">mike</phoneme>");
                        break;
                    case 'N':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"nɔvɑ̃bʁ\">november</phoneme>");
                        break;
                    case 'O':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ɔskaʁ\">oscar</phoneme>");
                        break;
                    case 'P':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"papa\">papa</phoneme>");
                        break;
                    case 'Q':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"kebɛk\">quebec</phoneme>");
                        break;
                    case 'R':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ʁɔmeo\">romeo</phoneme>");
                        break;
                    case 'S':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"sjeʁa\">sierra</phoneme>");
                        break;
                    case 'T':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"tɑ̃ɡo\">tango</phoneme>");
                        break;
                    case 'U':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ynifɔʁm\">uniform</phoneme>");
                        break;
                    case 'V':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"viktɔʁ\">victor</phoneme>");
                        break;
                    case 'W':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"wiski\">whiskey</phoneme>");
                        break;
                    case 'X':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ksʁɛ\">x-ray</phoneme>");
                        break;
                    case 'Y':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"jɑ̃ki\">yankee</phoneme>");
                        break;
                    case 'Z':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"zulu\">zulu</phoneme>");
                        break;
                    case '0':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"zəʁo\">zero</phoneme>");
                        break;
                    case '1':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ɛ̃\">un</phoneme>");
                        break;
                    case '2':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"dø\">deux</phoneme>");
                        break;
                    case '3':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"tʁwa\">trois</phoneme>");
                        break;
                    case '4':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"katʁ\">quatre</phoneme>");
                        break;
                    case '5':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"sɛ̃k\">cinq</phoneme>");
                        break;
                    case '6':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"sis\">six</phoneme>");
                        break;
                    case '7':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"sɛt\">sept</phoneme>");
                        break;
                    case '8':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ɥit\">huit</phoneme>");
                        break;
                    case '9':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"nœf\">neuf</phoneme>");
                        break;
                }
            }

            return elements.Aggregate((i, j) => i + " " + j);
        }

        public static string Humanize(decimal? value)
        {
            if (value == null)
            {
                return null;
            }

            if (value < 0)
            {
                // We don't handle negatives at the moment
                return "" + value;
            }

            if (value == 0)
            {
                return "zero";
            }

            if (value < 10)
            {
                // Work out how many 0s to begin with
                int numzeros = -1;
                decimal testval = (decimal)value;
                while (value < 1)
                {
                    value *= 10;
                    numzeros++;
                }
                // Now round it to 2sf
                return (Math.Round((double)value * 10) / (Math.Pow(10, numzeros + 2))).ToString();
            }

            int number;
            int nextDigit;
            string order;
            int digits = (int)Math.Log10((double)value);
            if (digits < 3)
            {
                // Units
                number = (int)value;
                order = "";
                nextDigit = 0;
            }
            else if (digits < 6)
            {
                // Thousands
                number = (int)(value / 1000);
                order = "Millier";
                nextDigit = (((int)value) - (number * 1000)) / 100;
            }
            else if (digits < 9)
            {
                // Millions
                number = (int)(value / 1000000);
                order = "million";
                nextDigit = (((int)value) - (number * 1000000)) / 100000;
            }
            else if (digits < 12)
            {
                // Billions
                number = (int)(value / 1000000000);
                order = "miliard";
                nextDigit = (int)(((long)value) - ((long)number * 1000000000)) / 100000000;
            }
            else if (digits < 15)
            {
                // Trillions
                number = (int)(value / 1000000000000);
                order = "trillion";
                nextDigit = (int)(((long)value) - (int)((number * 1000000000000)) / 100000000000);
            }
            else
            {
                // Quadrillions
                number = (int)(value / 1000000000000000);
                order = "quadrillion";
                nextDigit = (int)(((long)value) - (int)((number * 1000000000000000)) / 100000000000000);
            }

            if (order == "")
            {
                return "" + number;
            }
            else
            {
                // See if we have an exact match
                if (((long)(((decimal)value) / (decimal)Math.Pow(10, digits - 1))) * (decimal)(Math.Pow(10, digits - 1)) == value)
                {
                    return "" + number + " " + order;
                }
                if (number > 60)
                {
                    if (nextDigit < 6)
                    {
                        return "plus de " + number + " " + order;
                    }
                    else
                    {
                        return "proche de " + (number + 1) + " " + order;
                    }
                }
                switch (nextDigit)
                {
                    case 0:
                        return "juste au dessus de" + number + " " + order;
                    case 1:
                        return "au dessus de" + number + " " + order;
                    case 2:
                        return "bien au dessus de " + number + " " + order;
                    case 3:
                        return "vers les " + number + " et demi " + order;
                    case 4:
                        return "proche des " + number + " et demi " + order;
                    case 5:
                        return "autour des " + number + " et demi " + order;
                    case 6:
                        return "juste au dela des " + number + " et demi " + order;
                    case 7:
                        return "bien au dessus des " + number + " et demi " + order;
                    case 8:
                        return "vers les " + (number + 1) + " " + order;
                    case 9:
                        return "prôche de" + (number + 1) + " " + order;
                    default:
                        return "environ " + number + " " + order;
                }
            }

        }
    }
}
