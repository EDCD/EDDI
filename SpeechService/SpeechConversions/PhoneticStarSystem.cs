using EddiSpeechService.Properties;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilities;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        // Fixes to avoid issues with some of the more strangely-named systems
        private static readonly Dictionary<string, string> STAR_SYSTEM_FIXES = new()
        {
            { "VESPER-M4", "Vesper M 4" }, // Stop Vesper being treated as a sector
            {
                "Sagittarius A*", "Sagittarius " + sayAsLettersOrNumbers( "A" ) + " Star"
            }, // Allow the * to be parsed out
            { "Summerland", "Summer Land" }, // Separate summer from land 
        };

        private static readonly Dictionary<string, string[]> STAR_SYSTEM_PRONUNCIATIONS = new()
        {
            { "Achenar", [ Phonetics.achenar ] },
            { "Acihault", [ Phonetics.acihault ] },
            { "Adan", [ Phonetics.adan ] },
            { "Alcyone", [ Phonetics.alcyone ] },
            { "Aldebaran", [ Phonetics.aldebaran ] },
            { "Anemoi", [ Phonetics.anemoi ] },
            { "Apoyota", [ Phonetics.apoyota ] },
            { "Arque", [ Phonetics.arque ] },
            { "Asterope", [ Phonetics.asterope ] },
            { "Atlas", [ Phonetics.atlas ] },
            { "Aulin", [ Phonetics.aulin ] },
            { "Bunda", [ Phonetics.bunda ] },
            { "Cayutorme", [ Phonetics.cayutorme ] },
            { "Celaeno", [ Phonetics.celaeno ] },
            { "Ceos", [ Phonetics.ceos ] },
            { "Conn", [ Phonetics.Conn ] },
            { "Cygnus", [ Phonetics.cygnus ] },
            { "Deciat", [ Phonetics.deciat ] },
            { "Diso", [ Phonetics.diso ] },
            { "Eta Draconis", [ Phonetics.Eta, Phonetics.Draconis ] },
            { "Djiwal", [ Phonetics.djiwal ] },
            { "Dvorsi", [ Phonetics.dvorsi ] },
            { "Electra", [ Phonetics.electra ] },
            { "Eravate", [ Phonetics.eravate ] },
            { "Eranin", [ Phonetics.eranin ] },
            { "Frigaha", [ Phonetics.frigaha ] },
            { "Grandmort", [ Phonetics.grandmort ] },
            { "Hecate", [ Phonetics.hecate ] },
            { "Hotas", [ Phonetics.hotas ] },
            { "Isleta", [ Phonetics.hotas ] },
            { "i Bootis", [ Phonetics.ibootis_i, Phonetics.ibootis_bootis ] },
            { "Lave", [ Phonetics.lave ] },
            { "Kaia Bajaja", [ Phonetics.kaiabajaja_kaia, Phonetics.kaiabajaja_bajaja ] },
            { "Kigana", [ Phonetics.kigana ] },
            { "Kini", [ Phonetics.kini ] },
            { "Kremainn", [ Phonetics.kremainn ] },
            { "Laksak", [ Phonetics.laksak ] },
            { "Leesti", [ Phonetics.leesti ] },
            { "Leucos", [ Phonetics.leucos ] },
            { "Luyten's Star", [ Phonetics.luytens, Phonetics.star ] },
            { "Maia", [ Phonetics.maia ] },
            { "Mata", [ Phonetics.mata ] },
            { "Merope", [ Phonetics.merope ] },
            { "Mu Koji", [ Phonetics.mukoji_mu, Phonetics.mukoji_koji ] },
            { "Nuenets", [ Phonetics.nuenets ] },
            { "Okinura", [ Phonetics.okinura ] },
            { "Orrere", [ Phonetics.orrere ] },
            { "Pai Szu", [ Phonetics.paiszu_pai, Phonetics.paiszu_szu ] },
            { "Pleione", [ Phonetics.pleione ] },
            { "Procyon", [ Phonetics.procyon ] },
            { "Potriti", [ Phonetics.potriti ] },
            { "Reorte", [ Phonetics.reorte ] },
            { "Sagittarius A Star", [ Phonetics.Sagittarius, Phonetics.Sagittarius_A, Phonetics.Sagittarius_A_star ] },
            { "Sakti", [ Phonetics.sakti ] },
            { "Shinrarta Dezhra", [ Phonetics.shinrartadezhra_shinrarta, Phonetics.shinrartadezhra_dezhra ] },
            { "Sterope", [ Phonetics.Sterope ] },
            { "Surya", [ Phonetics.surya ] },
            { "Taygeta", [ Phonetics.taygeta ] },
            { "Tse", [ Phonetics.tse ] },
            { "Xihe", [ Phonetics.xihe ] },
            { "Xinca", [ Phonetics.xinca ] },
            { "Yakabugai", [ Phonetics.yakabugai ] },
            { "Zaonce", [ Phonetics.zaonce ] },
            { "Zhang Fei", [ Phonetics.zhangfei_zhang, Phonetics.zhangfei_fei ] },
        };

        private static readonly Dictionary<string, string[]> CONSTELLATION_PRONUNCIATIONS = new()
        {
            { "Alrai", [ Phonetics.Alrai ] },
            { "Antliae", [ Phonetics.Antliae ] },
            { "Aquarii", [ Phonetics.Aquarii ] },
            { "Arietis", [ Phonetics.Arietis ] },
            { "Bei Dou", [ Phonetics.Bei, Phonetics.Dou ] },
            { "Blanco", [ Phonetics.Blanco ] },
            { "Bleae Thaa", [ Phonetics.BleaeThaa_Bleae, Phonetics.BleaeThaa_Thaa ] },
            { "Bleae Thua", [ Phonetics.BleaeThaa_Bleae, Phonetics.BleaeThua_Thua ] },
            { "Capricorni", [ Phonetics.Capricorni ] },
            { "Carinae", [ Phonetics.Carinae ] },
            { "Cepheus", [ Phonetics.Cepheus ] },
            { "Cephei", [ Phonetics.Cephei ] },
            { "Ceti", [ Phonetics.Ceti ] },
            { "Chi Persei", [ Phonetics.ChiPersei_Chi, Phonetics.ChiPersei_Persei ] },
            { "Crucis", [ Phonetics.Crucis ] },
            { "Cygni", [ Phonetics.Cygni ] },
            { "Eta Carina", [ Phonetics.EtaCarina_Eta, Phonetics.EtaCarina_Carina ] },
            { "Fornacis", [ Phonetics.Fornacis ] },
            { "Herculis", [ Phonetics.Herculis ] },
            { "Hyades", [ Phonetics.Hyades ] },
            { "Hydrae", [ Phonetics.Hydrae ] },
            { "Lupus", [ Phonetics.Lupus ] },
            { "Lyncis", [ Phonetics.Lyncis ] },
            { "Omega", [ Phonetics.Omega ] },
            { "Ophiuchus", [ Phonetics.Ophiuchus ] },
            { "Pegasi", [ Phonetics.Pegasi ] },
            { "Persei", [ Phonetics.Persei ] },
            { "Piscium", [ Phonetics.Piscium ] },
            { "Pleiades", [ Phonetics.Pleiades ] },
            { "Puppis", [ Phonetics.Puppis ] },
            { "Pru Euq", [ Phonetics.PruEuq_Pru, Phonetics.PruEuq_Euq ] },
            { "Rho Ophiuchi", [ Phonetics.RhoOphiuchi_Rho, Phonetics.RhoOphiuchi_Ophiuchi ] },
            { "Sagittarius", [ Phonetics.Sagittarius ] },
            { "Scorpii", [ Phonetics.Scorpii ] },
            { "Shui Wei", [ Phonetics.ShuiWei_Shui, Phonetics.ShuiWei_Wei ] },
            { "Tau Ceti", [ Phonetics.TauCeti_Tau, Phonetics.TauCeti_Ceti ] },
            { "Tascheter", [ Phonetics.Tascheter ] },
            { "Trianguli", [ Phonetics.Trianguli ] },
            { "Trifid", [ Phonetics.Trifid ] },
            { "Tucanae", [ Phonetics.Tucanae ] },
            { "Wredguia", [ Phonetics.Wredguia ] },
        };

        private static readonly Dictionary<string, string[]> CATALOG_FIXUPS = new()
        {
            {
                "AC ",
                [ @"<say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">C</say-as> " ]
            },
            {
                "Gl ", [ $@"<phoneme alphabet=""ipa"" ph=""{Phonetics.Gliese}"">Gliese</phoneme> " ]
            }, // Abbreviation for "Gliese"
            {
                "Gliese", [ $@"<phoneme alphabet=""ipa"" ph=""{Phonetics.Gliese}"">Gliese</phoneme>" ]
            }, // Gliese Catalogue of Nearby Stars
            {
                "Lalande", [ $@"<phoneme alphabet=""ipa"" ph=""{Phonetics.Lalande}"">Lalande</phoneme>" ]
            }, // Histoire Céleste Française, e.g. "Lalande 1234"
            {
                "Luyten", [ $@"<phoneme alphabet=""ipa"" ph=""{Phonetics.Luyten}"">Luyten</phoneme>" ]
            }, // Luyten, e.g. "Luyten 143-23"
            {
                "2MASS", [ "2", "mass" ]
            }, // Two Micron All-Sky Survey, e.g. "2MASS J07464256+2000321 A or 2MASS 1503+2525"
        };

        /// <summary>Fix up star system names</summary>
        public static string getPhoneticStarSystem ( string starSystem, bool useICAO = false )
        {
            if ( starSystem == null )
            {
                return null;
            }

            // Specific fixing of names to avoid later confusion
            if ( STAR_SYSTEM_FIXES.TryGetValue( starSystem, out var phoneticStarSystem ) )
            {
                return phoneticStarSystem;
            }

            // Specific translations
            if ( STAR_SYSTEM_PRONUNCIATIONS.TryGetValue( starSystem, out var starSystemPronunciation ) )
            {
                return replaceWithPronunciation( starSystem, starSystemPronunciation );
            }

            // Match procedurally generated star systems, breaking apart systems and bodies wherever we can
            if ( GeneratedRegex.PROC_GEN_SYSTEM_BODY().IsMatch( starSystem ) )
            {
                var match = GeneratedRegex.PROC_GEN_SYSTEM_BODY().Match( starSystem );

                // Deal with the sector name first (e.g. Col 107 Sector)

                // We need to handle the pieces before and after the sector marker separately.
                // Fix common names
                var sectorNamePart1 = match.Groups[ "SECTOR" ].Value
                    .Replace( "Col ", "Coll " )
                    .Replace( "R CrA ", @"<say-as interpret-as=""characters"">R</say-as> Corona Australis " )
                    .Replace( "Tr ",
                        @"<say-as interpret-as=""characters"">T</say-as> <say-as interpret-as=""characters"">R</say-as> " )
                    .Replace( "Skull and Crossbones Neb. ", "Skull and Crossbones " )
                    .Replace( "(", "" ).Replace( ")", "" );

                // Various items between the sector name and 'Sector' need to be removed to allow us to find the base pronunciation
                var sectorNamePart2 = "";
                if ( sectorNamePart1.EndsWith( " Dark Region B Sector" ) )
                {
                    sectorNamePart1 = sectorNamePart1.Replace( " Dark Region B Sector", "" );
                    sectorNamePart2 = " Dark Region B Sector";
                }
                else if ( sectorNamePart1.EndsWith( " Sector" ) )
                {
                    sectorNamePart1 = sectorNamePart1.Replace( " Sector", "" );
                    sectorNamePart2 = " Sector";
                }

                // There might be a constellation name that we need to translate
                if ( CONSTELLATION_PRONUNCIATIONS.TryGetValue( sectorNamePart1, out var value ) )
                {
                    sectorNamePart1 = replaceWithPronunciation( sectorNamePart1, value );
                }

                // The sector name might include digits. Break up any group of three or more.
                var sectorNamePart1Words = new List<string>();
                foreach ( var word in sectorNamePart1.Split( ' ' ) )
                {
                    if ( GeneratedRegex.THREE_OR_MORE_DIGITS().IsMatch( word ) )
                    {
                        sectorNamePart1Words.Add( sayAsLettersOrNumbers( word, false, useICAO ) );
                    }
                    else
                    {
                        sectorNamePart1Words.Add( word );
                    }
                }

                sectorNamePart1 = string.Join( " ", sectorNamePart1Words ).Trim();

                // Now we spell out the procedurally generated sector coordinates (e.g. AI-H b40-6)
                var sectorCoordinates = sayAsLettersOrNumbers( match.Groups[ "COORDINATES" ].Value, true, useICAO );

                // Name might contain a body name - handle that here.
                var body = match.Groups[ "BODY" ]?.Value;

                return string.IsNullOrEmpty( body )
                    ? $"{sectorNamePart1 + sectorNamePart2} {sectorCoordinates}"
                    : $"{sectorNamePart1 + sectorNamePart2} {sectorCoordinates} {getPhoneticBody( body, useICAO )}";
            }

            // Common star catalogs
            if ( phoneticStarCatalogSystem( starSystem, out var result, useICAO ) )
            {
                return result;
            }

            else
            {
                // It's possible that the name contains a constellation or catalog abbreviation, in which case translate it
                var pieces = starSystem.Split( ' ' );
                for ( var i = 0; i < pieces.Length; i++ )
                {
                    if ( CONSTELLATION_PRONUNCIATIONS.TryGetValue( pieces[ i ], out var pronunciations ) )
                    {
                        pieces[ i ] = replaceWithPronunciation( pieces[ i ], pronunciations );
                    }
                    else if ( GeneratedRegex.ALPHA_THEN_NUMERIC().IsMatch( pieces[ i ] ) )
                    {
                        pieces[ i ] = sayAsLettersOrNumbers( pieces[ i ], false, useICAO );
                    }
                    else if ( GeneratedRegex.ALPHA_DOT().IsMatch( pieces[ i ] ) )
                    {
                        pieces[ i ] = sayAsLettersOrNumbers( pieces[ i ].Replace( ".", "" ), false, useICAO );
                    }
                    else if ( GeneratedRegex.DIGIT().IsMatch( pieces[ i ] ) )
                    {
                        pieces[ i ] = sayAsLettersOrNumbers( pieces[ i ], false, useICAO );
                    }
                    else if ( GeneratedRegex.UPPERCASE().IsMatch( pieces[ i ] ) )
                    {
                        pieces[ i ] = sayAsLettersOrNumbers( pieces[ i ], false, useICAO );
                    }
                }

                return string.Join( " ", pieces );
            }
        }

        // Match star catalogs, breaking apart systems and bodies wherever we can
        private static bool phoneticStarCatalogSystem ( string starSystem, out string result, bool useICAO = false )
        {
            // If we have a replacement for the first element in the string (e.g. a phonetic translation of a star catalog name), replace the first element with the simple phonetic name
            foreach ( var abbr in CATALOG_FIXUPS )
            {
                if ( starSystem.StartsWith( abbr.Key ) )
                {
                    var elements = new List<string>();
                    var parts = starSystem.Split( ' ' );
                    for ( var i = 0; i < parts.Length; i++ )
                    {
                        if ( i == 0 )
                        {
                            // Apply our transformation to our first element
                            if ( abbr.Key == "2MASS" )
                            {
                                // Apply ICAO for "2MASS" catalog star systems
                                elements.Add( sayAsLettersOrNumbers( abbr.Value[ 0 ], false, useICAO ) );
                                elements.Add( abbr.Value[ 1 ] );
                            }
                            else
                            {
                                elements.Add( string.Join( " ", abbr.Value ).Trim() );
                            }
                        }
                        else
                        {
                            elements.Add( sayAsLettersOrNumbers( parts[ i ],
                                !GeneratedRegex.THREE_OR_MORE_DIGITS().IsMatch( parts[ i ] ), useICAO ) );
                        }
                    }

                    result = string.Join( " ", elements ).Trim();
                    return result != starSystem;
                }
            }

            // First word proper-cased and spoken, remainder said as letters, numbers, and simple symbols
            if (
                starSystem.StartsWith( "GAT" ) || // Unknown (?), e.g. "GAT 118"
                starSystem.StartsWith( "HIP" ) || // Hipparcos Catalog, e.g. "HIP 1234", e.g. "HIP 1234"
                starSystem.StartsWith( "LAWD" ) || // Luyten Atlas of White Dwarfs, e.g. "LAWD 1234"
                starSystem.StartsWith( "Ross" ) || // Ross Catalogs (multiple by Frank Elmore Ross), e.g. "Ross 1478"
                starSystem.StartsWith( "SAO" ) || // Smithsonian Astrophysical Observatory Star Catalog, e.g. "SAO 1478"
                starSystem.StartsWith( "Smethells" ) || // Unknown (?), e.g. "Smethells 118"
                starSystem.StartsWith(
                    "WISE" ) || // The WISE Catalog of Galactic HII Regions V2.2, e.g. "WISE 1503+2525 or WISE A123"
                starSystem.StartsWith( "Wolf" ) // Wolf Catalogs (multiple by Max Wolf), e.g. "Wolf 1478"
            )
            {
                var elements = new List<string>();
                var parts = starSystem.Split( ' ' );
                for ( var i = 0; i < parts.Length; i++ )
                {
                    if ( i == 0 )
                    {
                        // Proper case our first word only
                        elements.Add( char.ToUpper( parts[ i ][ 0 ] ) + parts[ i ].Substring( 1 ).ToLower() );
                    }
                    else
                    {
                        elements.Add( sayAsLettersOrNumbers( parts[ i ],
                            !GeneratedRegex.THREE_OR_MORE_DIGITS().IsMatch( parts[ i ] ), useICAO ) );
                    }
                }

                result = string.Join( " ", elements ).Trim();
            }

            else if (
                starSystem.StartsWith( "AC " ) || // Astrographic Catalog, e.g. "AC +24 44"
                starSystem.StartsWith( "ADS " ) || // Unknown (?), e.g. "ADS 4229 ABC"
                starSystem.StartsWith( "AG+" ) || // Unknown (?), e.g. "AG+01 1940"
                starSystem.StartsWith( "BD-" ) || // Bonner Durchmusterung, e.g. "BD-12 1234"
                starSystem.StartsWith( "BD+" ) || // Bonner Durchmusterung, e.g. "BD+01 1234"
                starSystem.StartsWith( "BPM " ) || // Unknown (?), e.g. "BPM 1234"
                starSystem.StartsWith( "CD-" ) || // Cordoba Durchmusterung, e.g. "CD-12 123, CD-24 13832D"
                starSystem.StartsWith( "CDP-" ) || // Unknown (?), e.g. "CDP-76 796"
                starSystem.StartsWith( "CFBDSIR " ) || // Canada-France Brown Dwarfs Survey, e.g. "CFBDSIR 1458+10"
                starSystem.StartsWith( "CPC " ) || // Cape obs., Photographic Catalog, e.g. "CPC 19 205"
                starSystem.StartsWith( "CPD-" ) || // Cape Photographic Durchmusterung, e.g. "CPD-20 2376"
                starSystem.StartsWith( "CSI-" ) || // Catalog of Stellar Identifications, e.g. "CSI-06-19031"
                starSystem.StartsWith( "Csi+" ) || // Catalog of Stellar Identifications, e.g. "Csi+09-19289"
                starSystem.StartsWith( "CXOONC " ) || // Unknown (?), e.g. "CXOONC J053517.5-052145"
                starSystem.StartsWith( "CXOU " ) || // Unknown (?), e.g. "CXOU J061705.3+222127"
                starSystem.StartsWith( "GCRV " ) || // General Catalog of Stellar Radial Velocities, e.g. "GCRV 1234"
                starSystem.StartsWith( "GD " ) || // Unknown (?), e.g. "GD 118"
                starSystem.StartsWith( "HR " ) || // Bright Star Catalog (Harvard Revised Catalogue), e.g. "HR 1234"
                starSystem.StartsWith( "HD " ) || // Henry Draper Catalog, e.g. "HD 1234"
                starSystem.StartsWith( "IDS " ) || // Index Catalogue of Visual Double Stars, e.g. IDS 02156+5638 AB
                starSystem.StartsWith( "L " ) || // Bruce Proper Motion Survey (Luyten), e.g. "L 10-7"
                starSystem.StartsWith( "LDS " ) || // Luyten Double Star catalogue, e.g. "LDS 118"
                starSystem.StartsWith( "LF " ) || // Unknown (?), e.g. "LF 8 +16 41"
                starSystem.StartsWith( "LFT " ) || // Luyten Five-Tenths Catalog, e.g. "LFT 1234"
                starSystem.StartsWith( "LHS " ) || // Luyten Half-Second Catalog, e.g. "LHS 1234"
                starSystem.StartsWith( "LP " ) || // Luyten Proper-Motion Catalog, e.g. "LP 1-52"
                starSystem.StartsWith( "LPM " ) || // Luyten Proper-Motion Catalogue, e.g. "LPM 118"
                starSystem.StartsWith( "LTT " ) || // Luyten Two-Tenths Catalog, e.g. "LTT 1234"
                starSystem.StartsWith( "MCC " ) || // McCormick obs., e.g. "MCC 123"
                starSystem.StartsWith( "MJD95 " ) || // Unknown (?), e.g. "MJD95 J023308.65+610144.3"
                starSystem.StartsWith( "NLTT " ) || // New Luyten Two-Tenths Catalog, e.g. "NLTT 1234"
                starSystem.StartsWith( "PPM " ) || // Positions and Proper Motions Star Catalogue, e.g. "PPM 41187"
                starSystem.StartsWith( "PW2010 " ) || // Unknown (?), e.g. "PW2010 118"
                starSystem.StartsWith( "SDSS " ) || // Sloan Digital Sky Survey, e.g. "SDSS J1416+1348"
                starSystem.StartsWith( "S171 " ) || // Sharpless Catalog-171 (NGC 7822 Nebula), e.g. "S171 32"
                starSystem.StartsWith( "UGCS " ) || // Unknown (?), e.g. "UGCS J083151.13+212922.7"
                starSystem.StartsWith( "WD " ) || // White Dwarf Catalog, e.g. "WD 1207-032"
                starSystem.StartsWith(
                    "XTE " ) // discovered using the All-Sky Monitor on the Rossi X-Ray Timing Explorer satellite, e.g. "XTE J1748-288"
            )
            {
                var elements = new List<string>();
                foreach ( var m in GeneratedRegex.StellarBodyRegex().Matches( starSystem ).OfType<Match>() )
                {
                    var useLongNumbers = !GeneratedRegex.THREE_OR_MORE_DIGITS().IsMatch( m.ToString() );
                    elements.Add( sayAsLettersOrNumbers( m.ToString(), useLongNumbers, useICAO ) );
                }

                result = string.Join( " ", elements ).Trim();
            }

            // No match
            else
            {
                result = starSystem;
            }

            return result != starSystem;
        }
    }
}
