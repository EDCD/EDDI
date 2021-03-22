using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace EddiSpeechService
{
    public partial class Translations
    {
        // Fixes to avoid issues with some of the more strangely-named systems
        private static readonly Dictionary<string, string> STAR_SYSTEM_FIXES = new Dictionary<string, string>()
        {
            { "VESPER-M4", "Vesper M 4" }, // Stop Vesper being treated as a sector
            { "Sagittarius A*", "Sagittarius " + sayAsLettersOrNumbers("A") + " Star" }, // Allow the * to be parsed out
            { "Summerland", "Summer Land" }, // Separate summer from land 
        };

        private static readonly Dictionary<string, string[]> STAR_SYSTEM_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Achenar", new string[] { Properties.Phonetics.achenar } },
            { "Acihault", new string[] { Properties.Phonetics.acihault } },
            { "Adan", new string[] { Properties.Phonetics.adan } },
            { "Alcyone", new string[] { Properties.Phonetics.alcyone } },
            { "Aldebaran", new string[] { Properties.Phonetics.aldebaran } },
            { "Anemoi", new string[] { Properties.Phonetics.anemoi } },
            { "Apoyota", new string[] { Properties.Phonetics.apoyota } },
            { "Arque", new string[] { Properties.Phonetics.arque } },
            { "Asterope", new string[] { Properties.Phonetics.asterope } },
            { "Atlas", new string[] { Properties.Phonetics.atlas } },
            { "Aulin", new string[] { Properties.Phonetics.aulin } },
            { "Bunda", new string[] { Properties.Phonetics.bunda } },
            { "Cayutorme", new string[] { Properties.Phonetics.cayutorme } },
            { "Celaeno", new string[] { Properties.Phonetics.celaeno } },
            { "Ceos", new string[] { Properties.Phonetics.ceos } },
            { "Conn", new string[] { Properties.Phonetics.Conn } },
            { "Cygnus", new string[] { Properties.Phonetics.cygnus } },
            { "Deciat", new string[] { Properties.Phonetics.deciat } },
            { "Diso", new string[] { Properties.Phonetics.diso } },
            { "Eta Draconis", new string[] {Properties.Phonetics.Eta, Properties.Phonetics.Draconis } },
            { "Djiwal", new string[] { Properties.Phonetics.djiwal } },
            { "Dvorsi", new string[] { Properties.Phonetics.dvorsi } },
            { "Electra", new string[] { Properties.Phonetics.electra } },
            { "Eravate" , new string[] { Properties.Phonetics.eravate } },
            { "Eranin" , new string[] { Properties.Phonetics.eranin } },
            { "Frigaha", new string[] { Properties.Phonetics.frigaha } },
            { "Grandmort" , new string[] { Properties.Phonetics.grandmort } },
            { "Hecate" , new string[] { Properties.Phonetics.hecate } },
            { "Hotas" , new string[] { Properties.Phonetics.hotas } },
            { "Isleta" , new string[] { Properties.Phonetics.hotas } },
            { "i Bootis" , new string[] { Properties.Phonetics.ibootis_i, Properties.Phonetics.ibootis_bootis } },
            { "Lave", new string[] { Properties.Phonetics.lave } },
            { "Kaia Bajaja", new string[] { Properties.Phonetics.kaiabajaja_kaia, Properties.Phonetics.kaiabajaja_bajaja } },
            { "Kigana", new string[] { Properties.Phonetics.kigana } },
            { "Kini", new string[] { Properties.Phonetics.kini } },
            { "Kremainn", new string[] { Properties.Phonetics.kremainn } },
            { "Laksak", new string[] { Properties.Phonetics.laksak } },
            { "Leesti", new string[] { Properties.Phonetics.leesti } },
            { "Leucos", new string[] { Properties.Phonetics.leucos } },
            { "Luyten's Star", new string[] { Properties.Phonetics.luytens, Properties.Phonetics.star } },
            { "Maia", new string[] { Properties.Phonetics.maia } },
            { "Mata", new string[] { Properties.Phonetics.mata } },
            { "Merope", new string[] { Properties.Phonetics.merope } },
            { "Mu Koji", new string[] { Properties.Phonetics.mukoji_mu, Properties.Phonetics.mukoji_koji } },
            { "Nuenets", new string[] { Properties.Phonetics.nuenets } },
            { "Okinura", new string[] { Properties.Phonetics.okinura } },
            { "Orrere", new string[] { Properties.Phonetics.orrere } },
            { "Pai Szu", new string[] { Properties.Phonetics.paiszu_pai, Properties.Phonetics.paiszu_szu } },
            { "Pleione", new string[] { Properties.Phonetics.pleione } },
            { "Procyon", new string[] { Properties.Phonetics.procyon } },
            { "Potriti", new string[] { Properties.Phonetics.potriti } },
            { "Reorte", new string[] { Properties.Phonetics.reorte } },
            { "Sagittarius A Star", new string[] {Properties.Phonetics.Sagittarius, Properties.Phonetics.Sagittarius_A, Properties.Phonetics.Sagittarius_A_star} },
            { "Sakti", new string[] { Properties.Phonetics.sakti } },
            { "Shinrarta Dezhra", new string[] { Properties.Phonetics.shinrartadezhra_shinrarta, Properties.Phonetics.shinrartadezhra_dezhra } },
            { "Sterope", new string[] {Properties.Phonetics.Sterope} },
            { "Surya", new string[] { Properties.Phonetics.surya } },
            { "Taygeta", new string[] { Properties.Phonetics.taygeta } },
            { "Tse", new string[] { Properties.Phonetics.tse } },
            { "Xihe", new string[] { Properties.Phonetics.xihe } },
            { "Xinca", new string[] { Properties.Phonetics.xinca } },
            { "Yakabugai", new string[] { Properties.Phonetics.yakabugai } },
            { "Zaonce", new string[] { Properties.Phonetics.zaonce } },
            { "Zhang Fei", new string[] { Properties.Phonetics.zhangfei_zhang, Properties.Phonetics.zhangfei_fei } },
        };

        private static readonly Dictionary<string, string[]> CONSTELLATION_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Alrai" , new string[] { Properties.Phonetics.Alrai } },
            { "Antliae" , new string[] { Properties.Phonetics.Antliae } },
            { "Aquarii" , new string[] { Properties.Phonetics.Aquarii } },
            { "Arietis" , new string[] { Properties.Phonetics.Arietis } },
            { "Bei Dou" , new string[] { Properties.Phonetics.Bei, Properties.Phonetics.Dou } },
            { "Blanco" , new string[] { Properties.Phonetics.Blanco } },
            { "Bleae Thaa" , new string[] { Properties.Phonetics.BleaeThaa_Bleae, Properties.Phonetics.BleaeThaa_Thaa } },
            { "Capricorni" , new string[] { Properties.Phonetics.Capricorni } },
            { "Cepheus" , new string[] { Properties.Phonetics.Cepheus } },
            { "Cephei" , new string[] { Properties.Phonetics.Cephei } },
            { "Ceti" , new string[] { Properties.Phonetics.Ceti } },
            { "Chi Persei" , new string[] { Properties.Phonetics.ChiPersei_Chi, Properties.Phonetics.ChiPersei_Persei } },
            { "Crucis" , new string[] { Properties.Phonetics.Crucis } },
            { "Cygni" , new string[] { Properties.Phonetics.Cygni } },
            { "Eta Carina" , new string[] { Properties.Phonetics.EtaCarina_Eta, Properties.Phonetics.EtaCarina_Carina } },
            { "Fornacis" , new string[] { Properties.Phonetics.Fornacis } },
            { "Herculis" , new string[] { Properties.Phonetics.Herculis } },
            { "Hyades" , new string[] { Properties.Phonetics.Hyades } },
            { "Hydrae" , new string[] { Properties.Phonetics.Hydrae } },
            { "Lupus" , new string[] { Properties.Phonetics.Lupus } },
            { "Lyncis" , new string[] { Properties.Phonetics.Lyncis } },
            { "Omega" , new string[] { Properties.Phonetics.Omega } },
            { "Ophiuchus" , new string[] { Properties.Phonetics.Ophiuchus } },
            { "Pegasi" , new string[] { Properties.Phonetics.Pegasi } },
            { "Persei" , new string[] { Properties.Phonetics.Persei } },
            { "Piscium" , new string[] { Properties.Phonetics.Piscium } },
            { "Pleiades" , new string[] { Properties.Phonetics.Pleiades } },
            { "Puppis" , new string[] { Properties.Phonetics.Puppis } },
            { "Pru Euq" , new string[] { Properties.Phonetics.PruEuq_Pru, Properties.Phonetics.PruEuq_Euq } },
            { "Rho Ophiuchi" , new string[] { Properties.Phonetics.RhoOphiuchi_Rho, Properties.Phonetics.RhoOphiuchi_Ophiuchi } },
            { "Sagittarius", new string[] { Properties.Phonetics.Sagittarius } },
            { "Scorpii", new string[] { Properties.Phonetics.Scorpii } },
            { "Shui Wei", new string[] { Properties.Phonetics.ShuiWei_Shui, Properties.Phonetics.ShuiWei_Wei } },
            { "Tau Ceti" , new string[] { Properties.Phonetics.TauCeti_Tau, Properties.Phonetics.TauCeti_Ceti } },
            { "Tascheter", new string[] { Properties.Phonetics.Tascheter } },
            { "Trianguli", new string[] { Properties.Phonetics.Trianguli } },
            { "Trifid", new string[] { Properties.Phonetics.Trifid} },
            { "Tucanae", new string[] { Properties.Phonetics.Tucanae } },
            { "Wredguia", new string[] { Properties.Phonetics.Wredguia } },
        };

        /// <summary>Fix up star system names</summary>
        public static string getPhoneticStarSystem(string starSystem, bool useICAO = false)
        {
            if (starSystem == null)
            {
                return null;
            }

            // Specific fixing of names to avoid later confusion
            if (STAR_SYSTEM_FIXES.ContainsKey(starSystem))
            {
                return STAR_SYSTEM_FIXES[starSystem];
            }

            // Specific translations
            if (STAR_SYSTEM_PRONUNCIATIONS.ContainsKey(starSystem))
            {
                return replaceWithPronunciation(starSystem, STAR_SYSTEM_PRONUNCIATIONS[starSystem]);
            }

            // Common star catalogues
            if (starSystem.StartsWith("HIP"))
            {
                starSystem = starSystem.Replace("HIP", "Hip");
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
                || starSystem.StartsWith("Luyten ")
            )
            {
                starSystem = starSystem.Replace("-", " " + Properties.Phrases.dash + " ");
            }
            else if (starSystem.StartsWith("Gliese "))
            {
                starSystem = starSystem.Replace(".", " " + Properties.Phrases.point + " ");
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
                if (useICAO)
                {
                    secondPiece = ICAO(secondPiece, true);
                }
                secondPiece = secondPiece.Replace("-", " " + Properties.Phrases.dash + " ");
                starSystem = firstPiece + subPiece + " " + secondPiece;
            }
            else if (starSystem.StartsWith("2MASS ")
                || starSystem.StartsWith("AC ")
                || starSystem.StartsWith("AG") // Note no space
                || starSystem.StartsWith("BD") // Note no space
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
                                       .Replace("+", " " + Properties.Phrases.plus + " ")
                                       .Replace("-", " " + Properties.Phrases.minus + " ")
                                       .Replace(".", " " + Properties.Phrases.point + " ");
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
    }
}
