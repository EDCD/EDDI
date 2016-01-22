using Newtonsoft.Json.Linq;
using System.Net;

namespace EliteDangerousDataProviderAppService
{
    public class DataProviderApp
    {
        public static dynamic GetSystemData(string system)
        {
            var client = new WebClient();
            var response = client.DownloadString("http://api.eddp.co:16161/systems/" + system);
            return JObject.Parse(response);
        }
    }
}
