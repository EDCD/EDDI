using System.Collections.Generic;

namespace EddiSpeechService
{
    public partial class Translations
    {
        // Fixes to avoid issues with pronunciation of station model names
        private static readonly Dictionary<string, string> STATION_MODEL_FIXES = new Dictionary<string, string>()
        {
            { "Orbis Starport", "Or-bis Starport" }, // Stop "Or-bis" from sometimes being pronounced as "Or-bise"
            { "Megaship", "Mega-ship" } // Stop "Mega-Ship" from sometimes being pronounced as "Meg-AH-ship"
        };

        private static readonly Dictionary<string, string[]> STATION_PRONUNCIATIONS = new Dictionary<string, string[]>()
        {
            { "Aachen Town", new string[] { Properties.Phonetics.Aachen, Properties.Phonetics.Town } },
            { "Slough Orbital", new string[] { Properties.Phonetics.Slough, Properties.Phonetics.Orbital } },
        };

        /// <summary>Fix up station related pronunciations </summary>
        private static string getPhoneticStation(string station)
        {
            // Specific translations
            if (STATION_PRONUNCIATIONS.ContainsKey(station))
            {
                return replaceWithPronunciation(station, STATION_PRONUNCIATIONS[station]);
            }

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
    }
}
