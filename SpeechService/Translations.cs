using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Utilities;

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
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_archon") + "\">Archon</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_delaine") + "\">Delaine</phoneme>";
                case "Aisling Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_aisling") + "\">Aisling</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_duval") + "\">Duval</phoneme>";
                case "Arissa Lavigny-Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_arissa") + "\">Arissa</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_lavigny") +"\">Lavigny</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_duval") + "\">Duval</phoneme>";
                case "Denton Patreus":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_denton") + "\">Denton</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_patreus") + "\">Patreus</phoneme>";
                case "Edmund Mahon":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_edmund") + "\">Edmund</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_mahon") + "\">Mahon</phoneme>";
                case "Felicia Winters":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_felicia") + "\">Felicia</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_winters") + "\">Winters</phoneme>";
                case "Pranav Antal":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_pranav") + "\">Pranav</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_antal") + "\">Antal</phoneme>";
                case "Zachary Hudson":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_zachary") + "\">Zachary</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_hudson") + "\">Hudson</phoneme>";
                case "Zemina Torval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_zemina") + "\">Zemina</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_torval") + "\">Torval</phoneme>";
                case "Li Yong-Rui":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_li") + "\">Li</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_yong") + "\">Yong</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_rui") + "\">Rui</phoneme>";
                case "Yuri Grom":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_yuri") + "\">Yuri</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("phonetic_grom") + "\">Grom</phoneme>";
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
            { "SCORPIONS ORDER", "Scorpions Order" }, // Stop it being treated as a sector
            { "Federation Unite!", "Federation Unite"} // Stop pausing at the end of Unite!
        };

        private static Dictionary<string, string[]> STAR_SYSTEM_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Achenar", new string[] { I18N.GetString("phonetic_achenar") } },
            { "Acihault", new string[] { I18N.GetString("phonetic_acihault") } },
            { "Adan", new string[] { I18N.GetString("phonetic_adan") } },
            { "Alcyone", new string[] { I18N.GetString("phonetic_alcyone") } },
            { "Aldebaran", new string[] { I18N.GetString("phonetic_aldebaran") } },
            { "Anemoi", new string[] { I18N.GetString("phonetic_anemoi") } },
            { "Apoyota", new string[] { I18N.GetString("phonetic_apoyota") } },
            { "Arque", new string[] { I18N.GetString("phonetic_arque") } },
            { "Asterope", new string[] { I18N.GetString("phonetic_asterope") } },
            { "Atlas", new string[] { I18N.GetString("phonetic_atlas") } },
            { "Aulin", new string[] { I18N.GetString("phonetic_aulin") } },
            { "Bunda", new string[] { I18N.GetString("phonetic_bunda") } },
            { "Cayutorme", new string[] { I18N.GetString("phonetic_cayutorme") } },
            { "Celaeno", new string[] { I18N.GetString("phonetic_celaeno") } },
            { "Ceos", new string[] { I18N.GetString("phonetic_ceos") } },
            { "Cygnus", new string[] { I18N.GetString("phonetic_cygnus") } },
            { "Deciat", new string[] { I18N.GetString("phonetic_deciat") } },
            { "Diso ", new string[] { I18N.GetString("phonetic_diso") } },
            { "Djiwal", new string[] { I18N.GetString("phonetic_djiwal") } },
            { "Dvorsi", new string[] { I18N.GetString("phonetic_dvorsi") } },
            { "Electra", new string[] { I18N.GetString("phonetic_electra") } },
            { "Eravate" , new string[] { I18N.GetString("phonetic_eravate") } },
            { "Eranin" , new string[] { I18N.GetString("phonetic_eranin") } },
            { "Frigaha", new string[] { I18N.GetString("phonetic_frigaha") } },
            { "Grandmort" , new string[] { I18N.GetString("phonetic_grandmort") } },
            { "Hecate" , new string[] { I18N.GetString("phonetic_hecate") } },
            { "Hotas" , new string[] { I18N.GetString("phonetic_hotas") } },
            { "Isleta" , new string[] { I18N.GetString("phonetic_hotas") } },
            { "i Bootis" , new string[] { I18N.GetString("phonetic_ibootis_i"), I18N.GetString("phonetic_ibootis_bootis") } },
            { "Lave", new string[] { I18N.GetString("phonetic_lave") } },
            { "Kaia Bajaja", new string[] { I18N.GetString("phonetic_kaiabajaja_kaia"), I18N.GetString("phonetic_kaiabajaja_bajaja") } },
            { "Kigana", new string[] { I18N.GetString("phonetic_kigana") } },
            { "Kini", new string[] { I18N.GetString("phonetic_kini") } },
            { "Kremainn", new string[] { I18N.GetString("phonetic_kremainn") } },
            { "Laksak", new string[] { I18N.GetString("phonetic_laksak") } },
            { "Leesti", new string[] { I18N.GetString("phonetic_leesti") } },
            { "Leucos", new string[] { I18N.GetString("phonetic_leucos") } },
            { "Luyten's Star", new string[] { I18N.GetString("phonetic_luytens"), I18N.GetString("phonetic_star") } },
            { "Maia", new string[] { I18N.GetString("phonetic_maia") } },
            { "Mata", new string[] { I18N.GetString("phonetic_mata") } },
            { "Merope", new string[] { I18N.GetString("phonetic_merope") } },
            { "Mu Koji", new string[] { I18N.GetString("phonetic_mukoji_mu"), I18N.GetString("phonetic_mukoji_koji") } },
            { "Nuenets", new string[] { I18N.GetString("phonetic_nuenets") } },
            { "Okinura", new string[] { I18N.GetString("phonetic_okinura") } },
            { "Orrere", new string[] { I18N.GetString("phonetic_orrere") } },
            { "Pai Szu", new string[] { I18N.GetString("phonetic_paiszu_pai"), I18N.GetString("phonetic_paiszu_szu") } },
            { "Pleione", new string[] { I18N.GetString("phonetic_pleione") } },
            { "Procyon", new string[] { I18N.GetString("phonetic_procyon") } },
            { "Potriti", new string[] { I18N.GetString("phonetic_potriti") } },
            { "Reorte", new string[] { I18N.GetString("phonetic_reorte") } },
            { "Sakti", new string[] { I18N.GetString("phonetic_sakti") } },
            { "Shinrarta Dezhra", new string[] { I18N.GetString("phonetic_shinrartadezhra_shinrarta"), I18N.GetString("phonetic_shinrartadezhra_dezhra") } },
            { "Surya", new string[] { I18N.GetString("phonetic_surya") } },
            { "Taygeta", new string[] { I18N.GetString("phonetic_taygeta") } },
            { "Tse", new string[] { I18N.GetString("phonetic_tse") } },
            { "Xihe", new string[] { I18N.GetString("phonetic_xihe") } },
            { "Xinca", new string[] { I18N.GetString("phonetic_xinca") } },
            { "Yakabugai", new string[] { I18N.GetString("phonetic_yakabugai") } },
            { "Zaonce", new string[] { I18N.GetString("phonetic_zaonce") } },
            { "Zhang Fei", new string[] { I18N.GetString("phonetic_zhangfei_zhang"), I18N.GetString("phonetic_zhangfei_fei") } },
        };

        private static Dictionary<string, string[]> CONSTELLATION_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Alrai" , new string[] { I18N.GetString("phonetic_Alrai") } },
            { "Antliae" , new string[] { I18N.GetString("phonetic_Antliae") } },
            { "Aquarii" , new string[] { I18N.GetString("phonetic_Aquarii") } },
            { "Arietis" , new string[] { I18N.GetString("phonetic_Arietis") } },
            { "Bei Dou" , new string[] { I18N.GetString("phonetic_Bei"), I18N.GetString("phonetic_Dou") } },
            { "Blanco" , new string[] { I18N.GetString("phonetic_Blanco") } },
            { "Bleae Thaa" , new string[] { I18N.GetString("phonetic_BleaeThaa_Bleae"), I18N.GetString("phonetic_BleaeThaa_Thaa") } },
            { "Bleae Thua" , new string[] { I18N.GetString("phonetic_BleaeThaa_Bleae"), I18N.GetString("phonetic_phonetic_BleaeThua_Thua") } },
            { "Capricorni" , new string[] { I18N.GetString("phonetic_Capricorni") } },
            { "Cepheus" , new string[] { I18N.GetString("phonetic_Cepheus") } },
            { "Cephei" , new string[] { I18N.GetString("phonetic_Cephei") } },
            { "Ceti" , new string[] { I18N.GetString("phonetic_Ceti") } },
            { "Chi Persei" , new string[] { I18N.GetString("phonetic_ChiPersei_Chi"), I18N.GetString("phonetic_ChiPersei_Persei") } },
            { "Crucis" , new string[] { I18N.GetString("phonetic_Crucis") } },
            { "Cygni" , new string[] { I18N.GetString("phonetic_Cygni") } },
            { "Eta Carina" , new string[] { I18N.GetString("phonetic_EtaCarina_Eta"), I18N.GetString("phonetic_EtaCarina_Carina") } },
            { "Fornacis" , new string[] { I18N.GetString("phonetic_Fornacis") } },
            { "Herculis" , new string[] { I18N.GetString("phonetic_Herculis") } },
            { "Hyades" , new string[] { I18N.GetString("phonetic_Hyades") } },
            { "Hydrae" , new string[] { I18N.GetString("phonetic_Hydrae") } },
            { "Lupus" , new string[] { I18N.GetString("phonetic_Lupus") } },
            { "Lyncis" , new string[] { I18N.GetString("phonetic_Lyncis") } },
            { "Omega" , new string[] { I18N.GetString("phonetic_Omega") } },
            { "Ophiuchus" , new string[] { I18N.GetString("phonetic_Ophiuchus") } },
            { "Pegasi" , new string[] { I18N.GetString("phonetic_Pegasi") } },
            { "Persei" , new string[] { I18N.GetString("phonetic_Persei") } },
            { "Piscium" , new string[] { I18N.GetString("phonetic_Piscium") } },
            { "Pleiades" , new string[] { I18N.GetString("phonetic_Pleiades") } },
            { "Puppis" , new string[] { I18N.GetString("phonetic_Puppis") } },
            { "Pru Euq" , new string[] { I18N.GetString("phonetic_PruEuq_Pru"), I18N.GetString("phonetic_PruEuq_Euq") } },
            { "Rho Ophiuchi" , new string[] { I18N.GetString("phonetic_RhoOphiuchi_Rho"), I18N.GetString("phonetic_RhoOphiuchi_Ophiuchi") } },
            { "Sagittarius", new string[] { I18N.GetString("phonetic_Sagittarius") } },
            { "Scorpii", new string[] { I18N.GetString("phonetic_Scorpii") } },
            { "Shui Wei", new string[] { I18N.GetString("phonetic_ShuiWei_Shui"), I18N.GetString("phonetic_ShuiWei_Wei") } },
            { "Tau Ceti" , new string[] { I18N.GetString("phonetic_TauCeti_Tau"), I18N.GetString("phonetic_TauCeti_Ceti") } },
            { "Tascheter", new string[] { I18N.GetString("phonetic_Tascheter") } },
            { "Trianguli", new string[] { I18N.GetString("phonetic_Trianguli") } },
            { "Trifid", new string[] { I18N.GetString("phonetic_Trifid")} },
            { "Tucanae", new string[] { I18N.GetString("phonetic_Tucanae") } },
            { "Wredguia", new string[] { I18N.GetString("phonetic_Wredguia") } },
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
						Console.WriteLine("Results[" + i + "][" + j + "] is *" + match.Groups[i].Captures[j].Value.Trim()+ "*");
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
        public static string StarSystem(string starSystem, bool useICAO=false)
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
                starSystem = starSystem.Replace("-", I18N.GetString("trans_dash"));
            }
            else if (starSystem.StartsWith("Gliese "))
            {
                starSystem = starSystem.Replace(".", I18N.GetString("trans_point"));
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

                string secondPiece = Match.Groups[2].Value;
                Console.WriteLine("1) Second piece is " + secondPiece);
                if (useICAO)
                {
                    secondPiece = ICAO(secondPiece, true);
                }
                Console.WriteLine("2) Second piece is " + secondPiece);
                secondPiece = secondPiece.Replace("-", " dash ");
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
                                       .Replace("+", I18N.GetString("trans_plus"))
                                       .Replace("-", I18N.GetString("trans_minus"))
                                       .Replace(".", I18N.GetString("trans_point"));
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

        public static string ICAO(string callsign, bool passDash=false)
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
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_A") + "\">alpha</phoneme>");
                        break;
                    case 'B':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_B") + "\">bravo</phoneme>");
                        break;
                    case 'C':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_C") + "\">charlie</phoneme>");
                        break;
                    case 'D':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_D") + "\">delta</phoneme>");
                        break;
                    case 'E':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_E") + "\">echo</phoneme>");
                        break;
                    case 'F':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_F") + "\">foxtrot</phoneme>");
                        break;
                    case 'G':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_G") + "\">golf</phoneme>");
                        break;
                    case 'H':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_H") + "\">hotel</phoneme>");
                        break;
                    case 'I':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_I") + "\">india</phoneme>");
                        break;
                    case 'J':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_J") + "\">juliet</phoneme>");
                        break;
                    case 'K':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_K") + "\">kilo</phoneme>");
                        break;
                    case 'L':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_L") + "\">lima</phoneme>");
                        break;
                    case 'M':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_M") + "\">mike</phoneme>");
                        break;
                    case 'N':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_N") + "\">november</phoneme>");
                        break;
                    case 'O':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_O") + "\">oscar</phoneme>");
                        break;
                    case 'P':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_P") + "\">papa</phoneme>");
                        break;
                    case 'Q':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_Q") + "\">quebec</phoneme>");
                        break;
                    case 'R':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_R") + "\">romeo</phoneme>");
                        break;
                    case 'S':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_S") + "\">sierra</phoneme>");
                        break;
                    case 'T':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_T") + "\">tango</phoneme>");
                        break;
                    case 'U':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_U") + "\">uniform</phoneme>");
                        break;
                    case 'V':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_V") + "\">victor</phoneme>");
                        break;
                    case 'W':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_W") + "\">whiskey</phoneme>");
                        break;
                    case 'X':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_X") + "\">x-ray</phoneme>");
                        break;
                    case 'Y':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_Y") + "\">yankee</phoneme>");
                        break;
                    case 'Z':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_Z") + "\">zulu</phoneme>");
                        break;
                    case '0':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_0") + "\">zero</phoneme>");
                        break;
                    case '1':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_1") + "\">one</phoneme>");
                        break;
                    case '2':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_2") + "\">two</phoneme>");
                        break;
                    case '3':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_3") + "\">tree</phoneme>");
                        break;
                    case '4':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_4") + "\">fawer</phoneme>");
                        break;
                    case '5':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_5") + "\">fife</phoneme>");
                        break;
                    case '6':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_6") + "\">six</phoneme>");
                        break;
                    case '7':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_7") + "\">seven</phoneme>");
                        break;
                    case '8':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_8") + "\">eight</phoneme>");
                        break;
                    case '9':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"" + I18N.GetString("ICAO_9") + "\">niner</phoneme>");
                        break;
                    case '-':
                        if (passDash) elements.Add("-");
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
                return I18N.GetString("trans_zero");
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
                order = I18N.GetString("trans_thousand");
                nextDigit = (((int)value) - (number * 1000)) / 100;
            }
            else if (digits < 9)
            {
                // Millions
                number = (int)(value / 1000000);
                order = I18N.GetString("trans_million");
                nextDigit = (((int)value) - (number * 1000000)) / 100000;
            }
            else if (digits < 12)
            {
                // Billions
                number = (int)(value / 1000000000);
                order = I18N.GetString("trans_billion");
                nextDigit = (int)(((long)value) - ((long)number * 1000000000)) / 100000000;
            }
            else if (digits < 15)
            {
                // Trillions
                number = (int)(value / 1000000000000);
                order = I18N.GetString("trans_trillion");
                nextDigit = (int)(((long)value) - (int)((number * 1000000000000)) / 100000000000);
            }
            else
            {
                // Quadrillions
                number = (int)(value / 1000000000000000);
                order = I18N.GetString("trans_quadrillion");
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
                        return I18N.GetString("trans_over") + number + " " + order;
                    }
                    else
                    {
                        return I18N.GetString("trans_nearly") + (number + 1) + " " + order;
                    }
                }
                switch (nextDigit)
                {
                    case 0:
                        return I18N.GetString("trans_justover") + number + " " + order;
                    case 1:
                        return I18N.GetString("trans_over") + number + " " + order;
                    case 2:
                        return I18N.GetString("trans_wellover") + number + " " + order;
                    case 3:
                        return I18N.GetString("trans_onthewayto") + number + I18N.GetString("trans_andahalf") + order;
                    case 4:
                        return I18N.GetString("trans_nearly") + number + I18N.GetString("trans_andahalf") + order;
                    case 5:
                        return I18N.GetString("trans_around") + number + I18N.GetString("trans_andahalf") + order;
                    case 6:
                        return I18N.GetString("trans_justover") + number + I18N.GetString("trans_andahalf") + order;
                    case 7:
                        return I18N.GetString("trans_wellover") + number + I18N.GetString("trans_andahalf") + order;
                    case 8:
                        return I18N.GetString("trans_onthewayto") + (number + 1) + " " + order;
                    case 9:
                        return I18N.GetString("trans_nearly") + (number + 1) + " " + order;
                    default:
                        return I18N.GetString("trans_around") + number + " " + order;
                }
            }

        }
    }
}
