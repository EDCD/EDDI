using Newtonsoft.Json.Linq;
using System.Net;

namespace EliteDangerousDataProviderAppService
{
    public class DataProviderApp
    {
        public static StarSystem GetSystemData(string system)
        {
            var client = new WebClient();
            var response = client.DownloadString("http://api.eddp.co:16161/systems/" + system);
            return ParseSystemData(response);
        }

        public static StarSystem ParseSystemData(string data)
        {
            return StarSystem.FromEDDP(JObject.Parse(data));
        }
    }
}
