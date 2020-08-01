﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace EddiSpeechService
{
    /// <summary>Translations for Elite items for text-to-speech</summary>
    public class Translations
    {
        public static string GetTranslation(string val, bool useICAO = false)
        {
            // Translations from fixed dictionaries
            string translation = val;
            if (translation == val)
            {
                translation = Power(val);
            }
            if (translation == val)
            {
                translation = StellarClass(val);
            }
            if (translation == val)
            {
                translation = PlanetClass(val);
            }
            if (translation == val)
            {
                Ship ship = ShipDefinitions.FromModel(val);
                if (ship != null && ship.EDID > 0)
                {
                    translation = ship.SpokenModel();
                }
            }
            if (translation == val)
            {
                string phoneticManufacturer = ShipDefinitions.SpokenManufacturer(val);
                if (phoneticManufacturer != null)
                {
                    translation = phoneticManufacturer;
                }
            }
            if (translation == val)
            {
                translation = Body(val, useICAO);
            }
            if (translation == val)
            {
                translation = StarSystem(val, useICAO);
            }
            if (translation == val)
            {
                translation = Station(val);
            }
            if (translation == val)
            {
                translation = Faction(val);
            }
            return translation.Trim();
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
                case "Archon Delaine":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.archon + "\">Archon</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.delaine + "\">Delaine</phoneme>";
                case "Aisling Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.aisling + "\">Aisling</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.duval + "\">Duval</phoneme>";
                case "Arissa Lavigny-Duval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.arissa + "\">Arissa</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.lavigny + "\">Lavigny</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.duval + "\">Duval</phoneme>";
                case "Denton Patreus":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.denton + "\">Denton</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.patreus + "\">Patreus</phoneme>";
                case "Edmund Mahon":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.edmund + "\">Edmund</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.mahon + "\">Mahon</phoneme>";
                case "Felicia Winters":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.felicia + "\">Felicia</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.winters + "\">Winters</phoneme>";
                case "Pranav Antal":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.pranav + "\">Pranav</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.antal + "\">Antal</phoneme>";
                case "Zachary Hudson":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.zachary + "\">Zachary</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.hudson + "\">Hudson</phoneme>";
                case "Zemina Torval":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.zemina + "\">Zemina</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.torval + "\">Torval</phoneme>";
                case "Li Yong-Rui":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.li + "\">Li</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.yong + "\">Yong</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.rui + "\">Rui</phoneme>";
                case "Yuri Grom":
                    return "<phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.yuri + "\">Yuri</phoneme> <phoneme alphabet=\"ipa\" ph=\"" + Properties.Phonetics.grom + "\">Grom</phoneme>";
                default:
                    return power;
            }
        }

        // Fixes to avoid issues with some of the more strangely-named systems
        private static readonly Dictionary<string, string> STAR_SYSTEM_FIXES = new Dictionary<string, string>()
        {
            { "VESPER-M4", "Vesper M 4" }, // Stop Vesper being treated as a sector
            { "Sagittarius A*", "Sagittarius " + sayAsLettersOrNumbers("A") + " Star" }, // Allow the * to be parsed out
        };

        // Fixes to avoid issues with pronunciation of station model names
        private static readonly Dictionary<string, string> STATION_MODEL_FIXES = new Dictionary<string, string>()
        {
            { "Orbis Starport", "Or-bis Starport" }, // Stop "Or-bis" from sometimes being pronounced as "Or-bise"
            { "Megaship", "Mega-ship" } // Stop "Mega-Ship" from sometimes being pronounced as "Meg-AH-ship"
        };

        // Fixes to avoid issues with some of the more strangely-named factions
        private static readonly Dictionary<string, string> FACTION_FIXES = new Dictionary<string, string>()
        {
            { "SCORPIONS ORDER", "Scorpions Order" }, // Stop it being treated as a sector
            { "Federation Unite!", "Federation Unite"} // Stop pausing at the end of Unite!
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
            { "Cygnus", new string[] { Properties.Phonetics.cygnus } },
            { "Deciat", new string[] { Properties.Phonetics.deciat } },
            { "Diso ", new string[] { Properties.Phonetics.diso } },
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
            { "Surya", new string[] { Properties.Phonetics.surya } },
            { "Taygeta", new string[] { Properties.Phonetics.taygeta } },
            { "Tse", new string[] { Properties.Phonetics.tse } },
            { "Xihe", new string[] { Properties.Phonetics.xihe } },
            { "Xinca", new string[] { Properties.Phonetics.xinca } },
            { "Yakabugai", new string[] { Properties.Phonetics.yakabugai } },
            { "Zaonce", new string[] { Properties.Phonetics.zaonce } },
            { "Zhang Fei", new string[] { Properties.Phonetics.zhangfei_zhang, Properties.Phonetics.zhangfei_fei } },
        };

        public static string StellarClass(string val)
        {
            if (val == null)
            {
                return null;
            }

            // Some test to speech voices replace "TTS" with "text-to-speech". Fix that here.
            return val != "TTS" ? val : val.Replace("TTS", "T T S");
        }

        public static string PlanetClass(string val)
        {
            if (val == null)
            {
                return null;
            }

            // Properly handle roman numerals in planet classes
            foreach (PlanetClass planetClass in EddiDataDefinitions.PlanetClass.AllOfThem)
            {
                if (val.Contains(planetClass.localizedName))
                {
                    string numeralToNumber = planetClass.localizedName
                        .Replace(" I ", " 1 ")
                        .Replace(" II ", " 2 ")
                        .Replace(" III ", " 3 ")
                        .Replace(" IV ", " 4 ")
                        .Replace(" V ", " 5 ")
                        .Replace(" VI ", " 6 ");
                    val = val.Replace(planetClass.localizedName, numeralToNumber);
                }
            }
            return val;
        }

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

        // Various handy regexes so we don't keep recreating them
        private static readonly Regex ALPHA_THEN_NUMERIC = new Regex(@"[A-Za-z][0-9]");
        private static readonly Regex UPPERCASE = new Regex(@"([A-Z]{2,})|(?:([A-Z])(?:\s|$))");
        private static readonly Regex TEXT = new Regex(@"([A-Za-z]{1,3}(?:\s|$))");
        private static readonly Regex DIGIT = new Regex(@"\d+(?:\s|$)");
        private static readonly Regex THREE_OR_MORE_DIGITS = new Regex(@"\d{3,}");
        private static readonly Regex DECIMAL_DIGITS = new Regex(@"( point )(\d{2,})");
        private static readonly Regex SECTOR = new Regex("(.*) ([A-Za-z][A-Za-z]-[A-Za-z] .*)");
        private static readonly Regex PLANET = new Regex(@"^[A-Za-z]$");
        private static readonly Regex SUBSTARS = new Regex(@"^A[BCDE]?[CDE]?[DE]?[E]?|B[CDE]?[DE]?[E]?|C[DE]?[E]?|D[E]?$");
        private static readonly Regex BODY = new Regex(@"^(.*?) ([A-E]+ ){0,2}(Belt(?:\s|$)|Cluster(?:\s|$)|Ring|\d{1,2}(?:\s|$)|[A-Za-z](?:\s|$)){1,12}$", RegexOptions.IgnoreCase);

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
        private static string Body(string body, bool useICAO = false)
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
                        var part = match.Groups[i].Captures[j].Value.Trim();
                        var lastPart = j > 0 ? match.Groups[i].Captures[j - 1].Value.Trim()
                            : i > 2 && match.Groups[i - 1].Captures.Count > 0 ? match.Groups[i - 1].Captures[match.Groups[i - 1].Captures.Count - 1].Value.Trim()
                            : null;
                        var nextPart = j < match.Groups[i].Captures.Count - 1 ? match.Groups[i].Captures[j + 1].Value.Trim()
                            : i < match.Groups.Count - 1 ? match.Groups[i + 1].Captures[0].Value.Trim()
                            : null;

                        if (DIGIT.IsMatch(part))
                        {
                            // The part is a number; turn it in to ICAO if required
                            results.Add(useICAO ? ICAO(part, true) : part);
                        }
                        else if (PLANET.IsMatch(part) || lastPart == "Cluster" || nextPart == "Ring" || nextPart == "Belt")
                        {
                            // The part represents a body, possibly part of the name of a moon, ring, (stellar) belt, or belt cluster; 
                            // e.g. "Pru Aescs NC-M d7-192 A A Belt", "Prai Flyou JQ-F b30-3 B Belt Cluster 9", "Oopailks NV-X c17-1 AB 6 A Ring"

                            // turn it in to ICAO if required
                            if (useICAO)
                            {
                                results.Add(ICAO(part, true));
                            }
                            else
                            {
                                results.Add(sayAsLettersOrNumbers(part));
                            }
                        }
                        else if (part == "Belt" || part == "Cluster" || part == "Ring")
                        {
                            // Pass as-is
                            results.Add(part);
                        }
                        else if (SUBSTARS.IsMatch(part))
                        {
                            // The part is uppercase; turn it in to ICAO if required
                            results.Add(UPPERCASE.Replace(part, m => useICAO ? ICAO(m.Value, true) : string.Join<char>(" ", m.Value)));
                        }
                        else if (TEXT.IsMatch(part))
                        {
                            // turn it in to ICAO if required
                            if (useICAO)
                            {
                                results.Add(ICAO(part));
                            }
                            else
                            {
                                results.Add(sayAsLettersOrNumbers(part));
                            }
                        }
                        else
                        {
                            // Pass it as-is
                            results.Add(part);
                        }
                    }
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

        /// <summary>Fix up station related pronunciations </summary>
        private static string Station(string station)
        {
            // Specific fixing of station model pronunciations
            if (STATION_MODEL_FIXES.ContainsKey(station))
            {
                station = STATION_MODEL_FIXES[station];
            }
            // Strip plus signs and spaces from station name suffixes
            char[] charsToTrim = { '+', ' ' };
            station = station.TrimEnd(charsToTrim);
            return station;
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
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈælfə\">alpha</phoneme>");
                        break;
                    case 'B':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈbrɑːˈvo\">bravo</phoneme>");
                        break;
                    case 'C':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈtʃɑːli\">charlie</phoneme>");
                        break;
                    case 'D':
                        elements.Add("<phoneme alphabet=\"ipa\" ph=\"ˈdɛltə\">delta</phoneme>");
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
                    case '-':
                        if (passDash)
                        {
                            elements.Add("-");
                        }

                        break;
                }
            }

            return String.Join(" ", elements).Trim();
        }

        public static string sayAsLettersOrNumbers(string part)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var c in part)
            {
                var s = c.ToString();
                if (new Regex(@"\d").IsMatch(s))
                {
                    sb.Append(@"<say-as interpret-as=""number"">" + s + @"</say-as>");
                }
                else if (new Regex(@"\w").IsMatch(s))
                {
                    sb.Append(@"<say-as interpret-as=""characters"">" + s + @"</say-as>");
                }
                else
                {
                    sb.Append(s);
                }
            }
            return sb.ToString();
        }

        private static (int number, int nextDigit) Normalize(decimal? value, decimal order)
        {
            return (
                number: (int)(value / order),
                nextDigit: (int)((value % order) / (order / 10))
            );
        }

        public static string Humanize(decimal? value, bool recursed = false)
        {
            if (value == null)
            {
                return null;
            }

            string maybeMinus = "";
            if (value < 0)
            {
                maybeMinus = Properties.Phrases.minus + " ";
                value = -value;
            }

            if (value == 0)
            {
                return Properties.Phrases.zero;
            }

            if (value < 10)
            {
                // Work out how many 0s to begin with
                int numzeros = -1;
                while (value < 1)
                {
                    value *= 10;
                    numzeros++;
                }
                // Now round it to 2sf
                return maybeMinus + (Math.Round((double)value * 10) / (Math.Pow(10, numzeros + 2))).ToString();
            }

            int number;
            int nextDigit;
            string order;
            int digits = (int)Math.Truncate(Math.Log10((double)value));
            if (digits < 2)
            {
                // Units
                number = (int)value;
                order = "";
                nextDigit = (int)((value - number) * 10);
            }
            else if (digits < 3)
            {
                // Hundreds
                (number, nextDigit) = Normalize(value, 100);
                order = Properties.Phrases.hundred;
            }
            else if (digits < 6)
            {
                // Thousands
                (number, nextDigit) = Normalize(value, 1E3M);
                order = Properties.Phrases.thousand;
            }
            else if (digits < 9)
            {
                // Millions
                (number, nextDigit) = Normalize(value, 1E6M);
                order = Properties.Phrases.million;
            }
            else if (digits < 12)
            {
                // Billions
                (number, nextDigit) = Normalize(value, 1E9M);
                order = Properties.Phrases.billion;
            }
            else if (digits < 15)
            {
                // Trillions
                (number, nextDigit) = Normalize(value, 1E12M);
                order = Properties.Phrases.trillion;
            }
            else
            {
                // Quadrillions
                (number, nextDigit) = Normalize(value, 1E15M);
                order = Properties.Phrases.quadrillion;
            }

            // emit exact value
            if (order == "")
            {
                // we recurse only if we know that there _is_ a nested order (i. e. stuff like "51 hundred thousand")
                // otherwise we won't be able to round correctly
                Debug.Assert(recursed == false);
                return maybeMinus + number;
            }

            // recurse to handle nested orders (i. e. "50 hundred thousand")
            else if (number >= 100)
            {
                return maybeMinus + Humanize(number, true) + " " + order;
            }

            // shorten (round) the normalized value to 1 or 2 significant digits so that it can be pronounced with a single word
            // (i. e. say 1..19 as is, but round 21 to "twenty")
            else
            {
                if (recursed)
                {
                    Debug.Assert(number > 0 && number < 10);
                }
                else
                {
                    Debug.Assert(number > 0 && number < 100);
                }

                if (number <= 20)
                {
                    // we can say @number precisely with a single word.
                    // round to .5
                    if (nextDigit == 0)
                    {
                        // the figure we are saying is round enough already
                        return maybeMinus + number + " " + order;
                    }
                    else if (nextDigit < 3)
                    {
                        // round @number down
                        return Properties.Phrases.over + " " + maybeMinus + number + " " + order;
                    }
                    else if (nextDigit < 5)
                    {
                        // round @number up to .5
                        return Properties.Phrases.under + " " + maybeMinus + number + " " + Properties.Phrases.andahalf + " " + order;
                    }
                    else if (nextDigit == 5)
                    {
                        // the figure we are saying is round enough already
                        return maybeMinus + number + " " + Properties.Phrases.andahalf + " " + order;
                    }
                    else if (nextDigit < 8)
                    {
                        // round @number down to .5
                        return Properties.Phrases.over + " " + maybeMinus + number + " " + Properties.Phrases.andahalf + " " + order;
                    }
                    else
                    {
                        // round @number up
                        return Properties.Phrases.under + " " + maybeMinus + (number + 1) + " " + order;
                    }
                }
                else
                {
                    // otherwise round @number even more so that it can be pronounced with a single word.
                    // that is, round to 5 -- same plan as above
                    nextDigit = number % 10;
                    number -= nextDigit;

                    if (nextDigit == 0)
                    {
                        // the figure we are saying is round enough already
                        return maybeMinus + number + " " + order;
                    }
                    else if (nextDigit < 3)
                    {
                        // round @number down
                        return Properties.Phrases.over + " " + maybeMinus + number + " " + order;
                    }
                    else if (nextDigit < 5)
                    {
                        // round @number up 5
                        return Properties.Phrases.under + " " + maybeMinus + (number + 5) + " " + order;
                    }
                    else if (nextDigit == 5)
                    {
                        // the figure we are saying is round enough already
                        return maybeMinus + (number + 5) + " " + order;
                    }
                    else if (nextDigit < 8)
                    {
                        // round @number down to 5
                        return Properties.Phrases.over + " " + maybeMinus + (number + 5) + " " + order;
                    }
                    else
                    {
                        // round @number up
                        return Properties.Phrases.under + " " + maybeMinus + (number + 10) + " " + order;
                    }
                }
            }
        }
    }
}
