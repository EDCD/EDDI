using System.Collections.Generic;

namespace EddiSpeechService
{
    public partial class Translations
    {
        // Fixes to avoid issues with some of the more strangely-named factions
        private static readonly Dictionary<string, string> FACTION_FIXES = new Dictionary<string, string>()
        {
            { "SCORPIONS ORDER", "Scorpions Order" }, // Stop it being treated as a sector
            { "Federation Unite!", "Federation Unite"}, // Stop pausing at the end of Unite!
            { "Minutemen", "Minute men" } // Prevent pronunciation like "Minnuh-tea-men"
        };

        /// <summary>Fix up faction names</summary>
        public static string getPhoneticFaction(string faction, bool useICAO = false)
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

            // It's possible that the name contains a constellation or catalog abbreviation, in which case translate it
            string[] pieces = faction.Split(' ');
            for (int i = 0; i < pieces.Length; i++)
            {
                if (CONSTELLATION_PRONUNCIATIONS.ContainsKey(pieces[i]))
                {
                    pieces[i] = replaceWithPronunciation(pieces[i], CONSTELLATION_PRONUNCIATIONS[pieces[i]]);
                }
                else if (ALPHA_THEN_NUMERIC.IsMatch(pieces[i]))
                {
                    pieces[i] = sayAsLettersOrNumbers(pieces[i], false, useICAO);
                }
                else if (ALPHA_DOT.IsMatch(pieces[i]))
                {
                    pieces[i] = sayAsLettersOrNumbers(pieces[i].Replace(".", ""), false, useICAO);
                }
                else if (DIGIT.IsMatch(pieces[i]))
                {
                    pieces[i] = sayAsLettersOrNumbers(pieces[i], !THREE_OR_MORE_DIGITS.IsMatch(pieces[i]), useICAO);
                }
            }
            return string.Join(" ", pieces);
        }
    }
}
