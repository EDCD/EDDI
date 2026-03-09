using System.Collections.Generic;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        // Fixes to avoid issues with pronunciation of station model names
        private static readonly Dictionary<string, string> STATION_MODEL_FIXES = new()
        {
            { "Orbis Starport", "Or-bis Starport" }, // Stop "Or-bis" from sometimes being pronounced as "Or-bise"
            { "Megaship", "Mega-ship" } // Stop "Mega-Ship" from sometimes being pronounced as "Meg-AH-ship"
        };

        private static readonly Dictionary<string, string[]> STATION_PRONUNCIATIONS = new()
        {
            { "Aachen Town", [ Properties.Phonetics.Aachen, Properties.Phonetics.Town ] },
            { "Slough Orbital", [ Properties.Phonetics.Slough, Properties.Phonetics.Orbital ] },
        };

        /// <summary>Fix up station related pronunciations </summary>
        private static string getPhoneticStation(string station)
        {
            // Specific translations
            if (STATION_PRONUNCIATIONS.TryGetValue(station, out var stationPronunciation))
            {
                return replaceWithPronunciation(station, stationPronunciation);
            }

            // Specific fixing of station model pronunciations
            if (STATION_MODEL_FIXES.TryGetValue(station, out var value))
            {
                station = value;
            }
            // Strip plus signs and spaces from station name suffixes
            if (station.EndsWith('+'))
            {
                char[] charsToTrim = [ '+', ' ' ];
                station = station.TrimEnd(charsToTrim);
            }
            return station;
        }
    }
}
