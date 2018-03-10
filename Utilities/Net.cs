using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace Utilities
{
    public class Net
    {
        static Net()
        {
            // .NET 4.7 forces ServicePointManager.SecurityProtocol to SecurityProtocolType.SystemDefault, which is what we want.
            // Unfortunately when running under Voice Attack we are linked to an older version of .NET (confirmed with VA devs to be 4.5 as at 2018.03.10) which doesn't do this.
            // This means that we try to call the update server with a deprecated version of TLS which it rejects.
            // Thus we explicity set the security protocol here.
            // Ref: https://stackoverflow.com/questions/26389899/how-do-i-disable-ssl-fallback-and-use-only-tls-for-outbound-connections-in-net/26392698#26392698
            //
            // TODO: yank this when VoiceAttack updates to .NET 4.7 or later.
            ServicePointManager.SecurityProtocol = 0; // 0 is SecurityProtocolType.SystemDefault
            foreach (SecurityProtocolType protocol in SecurityProtocolType.GetValues(typeof(SecurityProtocolType)))
            {
                switch (protocol)
                {
                    case SecurityProtocolType.Ssl3:
                    case SecurityProtocolType.Tls:
                    case SecurityProtocolType.Tls11:
                        // these are deprecated
                        break;
                    default:
                        // we bitwise OR all the non-deprecated protocols
                        ServicePointManager.SecurityProtocol |= protocol;
                        break;
                }
            }
        }

        public static string DownloadString(string uri)
        {
            HttpWebRequest request = GetRequest(uri);
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            using (HttpWebResponse response = GetResponse(request))
            {
                if (response == null) // Means that the system was not found
                {
                    return null;
                }

                // Obtain and parse our response
                var encoding = response.CharacterSet == ""
                        ? Encoding.UTF8
                        : Encoding.GetEncoding(response.CharacterSet);

                Logging.Debug("Reading response from " + uri);
                using (var stream = response.GetResponseStream())
                {
                    var reader = new StreamReader(stream, encoding);
                    string data = reader.ReadToEnd();
                    return data;
                }
            }
        }

        public static string DownloadFile(string uri, string name)
        {
            try
            {
                WebClient client = new WebClient();
                string fileName = Path.GetTempPath() + @"\" + name;
                client.DownloadFile(new Uri(uri), fileName);
                return fileName;
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to download file " + uri, ex);
                return null;
            }
        }

        // Set up a request with the correct parameters for talking to the companion app
        private static HttpWebRequest GetRequest(string url)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Timeout = 10000;
            request.ReadWriteTimeout = 10000;
            return request;
        }

        // Obtain a response, ensuring that we obtain the response's cookies
        private static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            Logging.Debug("Requesting " + request.RequestUri);

            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException wex)
            {
                HttpWebResponse errorResponse = wex.Response as HttpWebResponse;
                if (errorResponse == null)
                {
                    // No error response
                    Logging.Warn("Failed to obtain response, error code " + wex.Status);
                    return null;
                }
                else if (errorResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    // Not found is usual
                    return null;
                }
                else
                {
                    Logging.Warn("Bad response, error code " + wex.Status);
                    throw;
                }
            }
            Logging.Debug("Response is " + JsonConvert.SerializeObject(response));
            return response;
        }

        // Async send a string
        public static void UploadString(string uri, string data)
        {
            Thread thread = new Thread(() =>
            {
                try
                {
                    using (var client = new WebClient())
                    {
                        client.UploadString(uri, data);
                    }
                }
                catch (ThreadAbortException)
                {
                    Logging.Debug("Thread aborted");
                }
                catch (Exception ex)
                {
                    Logging.Warn("Failed to send string to " + uri, ex);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static string GetDefaultBrowserPath()
        {
            string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            string browserPathKey = @"$BROWSER$\shell\open\command";

            RegistryKey userChoiceKey = null;
            string browserPath = "";

            try
            {
                // Read default browser path from userChoiceLKey
                userChoiceKey = Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);

                // If user choice was not found, try machine default
                if (userChoiceKey == null)
                {
                    // Read default browser path from Win XP registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                    // If browser path wasn’t found, try Win Vista (and newer) registry key
                    if (browserKey == null)
                    {
                        browserKey =
                        Registry.CurrentUser.OpenSubKey(
                        urlAssociation, false);
                    }
                    var path = CleanifyBrowserPath(browserKey.GetValue(null) as string);
                    browserKey.Close();
                    Logging.Debug("Browser path (1) is " + path);
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    string progId = (userChoiceKey.GetValue("ProgId").ToString());
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    string concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
                    var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    browserPath = CleanifyBrowserPath(kp.GetValue(null) as string);
                    kp.Close();
                    Logging.Debug("Browser path (2) is " + browserPath);
                    return browserPath;
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to find default browser: ", ex);
                return "explorer.exe";
            }
        }

        private static string CleanifyBrowserPath(string p)
        {
            string[] url = p.Split('"');
            string clean = url[1];
            return clean;
        }
    }
}
