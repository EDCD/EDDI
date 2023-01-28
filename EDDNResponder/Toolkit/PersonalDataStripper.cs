using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EddiEddnResponder.Toolkit
{
    public class PersonalDataStripper
    {
        // We will strip these personal keys (plus any localized properties) before sending data to EDDN

        // Personal keys disallowed in events are as follows:
        private static readonly Dictionary<string, string[]> disallowedKeysDictionary = new Dictionary<string, string[]>
        {
            { "ActiveFine", new []{ "Docked" }},
            { "BoostUsed", new []{ "FSDJump" }},
            { "CockpitBreach", new []{ "Docked" }},
            { "HappiestSystem", new []{ "CarrierJump", "FSDJump", "Location" }},
            { "HomeSystem", new []{ "CarrierJump", "FSDJump", "Location" }},
            { "FuelLevel", new []{ "CarrierJump", "FSDJump" }},
            { "FuelUsed", new []{ "CarrierJump", "FSDJump" }},
            { "IsNewEntry", new []{ "CodexEntry" }},
            { "JumpDist", new []{ "CarrierJump", "FSDJump" }},
            { "Latitude", new []{ "Location" } },
            { "Longitude", new []{ "Location" } },
            { "MyReputation", new []{ "CarrierJump", "FSDJump", "Location" }},
            { "NewTraitsDiscovered", new []{ "CodexEntry" }},
            { "SquadronFaction", new []{ "CarrierJump", "FSDJump", "Location" }}, 
            { "Wanted", new []{ "CarrierJump", "Docked", "FSDJump", "Location" }}
        };

        protected internal IDictionary<string, object> Strip(IDictionary<string, object> data, string edType = null)
        {
            // Need to strip a number of personal entries
            foreach (var kv in disallowedKeysDictionary)
            {
                if (string.IsNullOrEmpty(edType) || kv.Value.Contains(edType, StringComparer.InvariantCultureIgnoreCase))
                {
                    data.Remove(kv.Key);
                }
            }

            // Need to remove any keys ending with _Localised
            data = data.Where(x => !x.Key.EndsWith("_Localised") && !x.Key.Equals("locName"))
                .ToDictionary(x => x.Key, x => x.Value);

            // Need to remove personal data from any Dictionary or List type child objects
            IDictionary<string, object> fixedData = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> item in data)
            {
                if (item.Value is Dictionary<string, object> dict)
                {
                    fixedData.Add(item.Key, Strip(dict, edType));
                    continue;
                }
                if (item.Value is JObject jObject)
                {
                    fixedData.Add(item.Key, Strip(jObject.ToObject<Dictionary<string, object>>(), edType));
                    continue;
                }
                if (item.Value is List<object> list)
                {
                    var newList = new List<object>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i] is Dictionary<string, object> listDict)
                        {
                            newList.Add(Strip(listDict, edType));
                            continue;
                        }
                        newList.Add(list[i]);
                    }
                    fixedData.Add(item.Key, newList);
                    continue;
                }
                if (item.Value is JArray jArray)
                {
                    var newArray = new List<object>();
                    for (int i = 0; i < jArray.Count; i++)
                    {
                        if (jArray[i] is JObject listJObject)
                        {
                            newArray.Add(Strip(listJObject.ToObject<Dictionary<string, object>>(), edType));
                            continue;
                        }
                        newArray.Add(jArray[i]);
                    }
                    fixedData.Add(item.Key, newArray);
                    continue;
                }
                fixedData.Add(item);
            }

            return fixedData;
        }
    }
}