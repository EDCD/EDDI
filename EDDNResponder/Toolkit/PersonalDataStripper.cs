using System.Collections.Generic;
using System.Linq;

namespace EddiEddnResponder.Toolkit
{
    public class PersonalDataStripper
    {
        // We will strip these personal keys (plus any localized properties) before sending data to EDDN
        private static readonly string[] personalKeys =
        {
            "ActiveFine",
            "BoostUsed",
            "CockpitBreach",
            "HappiestSystem",
            "HomeSystem",
            "FuelLevel",
            "FuelUsed",
            "JumpDist",
            "Latitude",
            "Longitude",
            "MyReputation",
            "SquadronFaction",
            "Wanted"
        };

        protected internal IDictionary<string, object> Strip(IDictionary<string, object> data)
        {
            // Need to strip a number of personal entries
            foreach (var personalKey in personalKeys) { data.Remove(personalKey); }

            // Need to remove any keys ending with _Localised
            data = data.Where(x => !x.Key.EndsWith("_Localised") && !x.Key.Equals("locName"))
                .ToDictionary(x => x.Key, x => x.Value);

            // Need to remove personal data from any Dictionary or List type child objects
            IDictionary<string, object> fixedData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value is Dictionary<string, object> dict)
                {
                    fixedData.Add(item.Key, Strip(dict));
                    continue;
                }
                if (item.Value is List<object> list)
                {
                    List<object> newList = new List<object>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is Dictionary<string, object> listDict)
                        {
                            newList.Add(Strip(listDict));
                            continue;
                        }
                        newList.Add(list[i]);
                    }
                    fixedData.Add(item.Key, newList);
                    continue;
                }
                fixedData.Add(item);
            }

            return fixedData;
        }
    }
}