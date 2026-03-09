using System.Collections.Generic;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        // Fixes to avoid issues with some of the more strangely-named factions
        private static readonly Dictionary<string, string> FACTION_FIXES = new()
        {
            { "SCORPIONS ORDER", "Scorpions Order" }, // Stop it being treated as a sector
            { "Federation Unite!", "Federation Unite"}, // Stop pausing at the end of Unite!
            { "Minutemen", "Minute men" }, // Prevent pronunciation like "Minnuh-tea-men"
            { "The Fatherhood", "The Father hood" }, // Prevent garbling of "Fatherhood"
            { "C.O.N.T.R.A.I.L", "CON TRAIL" }, // Pronounce "C.O.N.T.R.A.I.L" phonetically rather than spelling it out
        };

        /// <summary>Fix up faction names</summary>
        public static string getPhoneticFaction(string faction, bool useICAO = false)
        {
            if (faction == null)
            {
                return null;
            }

            // Specific fixing of names to avoid later confusion
            if (FACTION_FIXES.TryGetValue(faction, out var value))
            {
                faction = value;
            }

            // Faction names can contain system names; hunt them down and change them
            foreach (var pronunciation in STAR_SYSTEM_FIXES)
            {
                if (faction.Contains(pronunciation.Key))
                {
                    return faction.Replace(pronunciation.Key, pronunciation.Value);
                }
            }
            foreach (var pronunciation in STAR_SYSTEM_PRONUNCIATIONS)
            {
                if (faction.Contains(pronunciation.Key))
                {
                    var replacement = replaceWithPronunciation(pronunciation.Key, pronunciation.Value);
                    return faction.Replace(pronunciation.Key, replacement);
                }
            }

            // It's possible that the name contains a constellation or catalog abbreviation, in which case translate it
            var pieces = faction.Split(' ');
            for (var i = 0; i < pieces.Length; i++)
            {
                if (CONSTELLATION_PRONUNCIATIONS.TryGetValue(pieces[i], out var pronunciations))
                {
                    pieces[i] = replaceWithPronunciation(pieces[i], pronunciations );
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
