using EddiDataDefinitions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using Utilities;

namespace EddiNavigationService
{
    public static class EDAstro
    {
        private const string GalacticPOI_URI = "https://edastro.com/poi/json/combined";
        public static ObservableCollection<NavBookmark> GetPOIs()
        {
            var galacticPOIs = new ObservableCollection<NavBookmark>();

            var jsonString = Net.DownloadString(GalacticPOI_URI);
            if (!string.IsNullOrEmpty(jsonString))
            {
                var jArray = JArray.Parse(jsonString);

                // Iterate in reverse order to remove any duplicate entries which appear on both GEC and GMP lists
                var unique = jArray
                    .OrderBy(y => y["source"]) // Order alphabetically by source ("GEC" before "GMP")
                    .GroupBy(x => x["galMapSearch"]) // Group POIs by system name
                    .Select(x => x.First()) // Select only the GEC POI if both GEC and GMP POIs exist within the same group
                    .ToList(); 
                for (var i = jArray.Count - 1; i >= 0; i--)
                {
                    var token = jArray[i];
                    if (!unique.Contains(token))
                    {
                        token.Remove();
                    }
                }

                foreach (JToken jToken in jArray)
                {
                    try
                    {
                        var obj = jToken.ToObject<JObject>();

                        var systemName = obj["galMapSearch"].ToString();
                        var systemAddress = obj["id64"]?.ToObject<ulong?>();
                        var poiName = WebUtility.HtmlDecode(obj["name"].ToString());

                        // Skip any items without a system name
                        if (string.IsNullOrEmpty(systemName))
                        {
                            continue;
                        }

                        // Coordinates
                        var coordinates = obj["coordinates"].ToObject<JArray>();
                        var x = coordinates[0]?.ToObject<decimal>();
                        var y = coordinates[1]?.ToObject<decimal>();
                        var z = coordinates[2]?.ToObject<decimal>();

                        var poiBookmark = new NavBookmark(systemName, systemAddress, x, y, z, "", poiName, false, null,
                            null, false);
                        poiBookmark.descriptionMarkdown = obj["descriptionMardown"].ToString();

                        if (obj["source"].ToString() == "GEC")
                        {
                            poiBookmark.comment = obj["summary"].ToString();
                            poiBookmark.url = obj["poiUrl"].ToString();
                        }
                        else if (obj["source"].ToString() == "GMP")
                        {
                            poiBookmark.url = obj["galMapUrl"].ToString();
                        }

                        galacticPOIs.Add(poiBookmark);
                    }
                    catch (Exception e)
                    {
                        Logging.Error("Failed to parse Galactic POI: " + JsonConvert.SerializeObject(jToken), e);
                    }
                }
            }

            return galacticPOIs;
        }
    }
}