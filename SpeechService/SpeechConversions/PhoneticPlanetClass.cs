using EddiDataDefinitions;

namespace EddiSpeechService.SpeechConversions
{
    public static partial class SpeechConversions
    {
        public static string getPhoneticPlanetClass(string val)
        {
            if (val == null)
            {
                return null;
            }

            // Properly handle roman numerals in planet classes
            foreach (var planetClass in PlanetClass.AllOfThem)
            {
                if (val.Contains(planetClass.localizedName))
                {
                    var numeralToNumber = planetClass.localizedName
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
    }
}
