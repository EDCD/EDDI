using Microsoft.Win32;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Net
    {
        public static string DownloadString(string uri)
        {
            try
            {
                HttpWebRequest request = GetRequest(uri);
                request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
                using ( HttpWebResponse response = GetResponse( request ) )
                {
                    if ( response == null ) // Means that the system was not found
                    {
                        return null;
                    }

                    // Obtain and parse our response
                    var encoding = string.IsNullOrEmpty(response.CharacterSet)
                        ? Encoding.UTF8
                        : Encoding.GetEncoding(response.CharacterSet);

                    Logging.Debug( "Reading response from " + uri );
                    return ReadResponseString( response, encoding );
                }
            }
            catch ( Exception e )
            {
                Logging.Error($"Error obtaining string response from {uri}: {e.Message}", e);
                return null;
            }
        }

        private static string ReadResponseString(HttpWebResponse response, Encoding encoding)
        {
            string data = null;
            int attempts = 0;
            Exception ex = null;

            while (data is null && attempts < 10)
            {
                try
                {
                    using (var stream = response.GetResponseStream())
                    {
                        if (stream != null)
                        {
                            var reader = new StreamReader(stream, encoding);
                            data = reader.ReadToEnd();
                            return data;
                        }
                        return null;
                    }
                }
                catch (Exception e)
                {
                    attempts++;
                    Thread.Sleep(50);
                    ex = e;
                }
            }

            if (attempts >= 10 && ex != null)
            {
                Logging.Warn(ex.Message);
            }

            return data;
        }

        public static async Task<string> DownloadFileAsync(string uri, string name)
        {
            try
            {
                var fileName = Path.GetTempPath() + @"\" + name;
                var response = await new HttpClient().GetAsync(uri);
                using (var fs = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    await response.Content.CopyToAsync(fs);
                }
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
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
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
                if (!(wex.Response is HttpWebResponse errorResponse))
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
            Logging.Debug("Response is: ", response);
            return response;
        }

        public static string GetDefaultBrowserPath()
        {
            string urlAssociation = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http";
            string browserPathKey = @"$BROWSER$\shell\open\command";

            try
            {
                // Read default browser path from userChoiceLKey
                var userChoiceKey = Registry.CurrentUser.OpenSubKey(urlAssociation + @"\UserChoice", false);

                // If user choice was not found, try machine default
                if (userChoiceKey == null)
                {
                    // Read default browser path from Win XP registry key
                    // If browser path wasn’t found, try Win Vista (and newer) registry key
                    var browserKey = Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false) ?? 
                                     Registry.CurrentUser.OpenSubKey(
                        urlAssociation, false);

                    var path = CleanifyBrowserPath(browserKey?.GetValue(null) as string);
                    browserKey?.Close();
                    Logging.Debug("Browser path (1) is " + path);
                    return path;
                }
                else
                {
                    // user defined browser choice was found
                    string progId = (userChoiceKey.GetValue("ProgId")?.ToString());
                    userChoiceKey.Close();

                    // now look up the path of the executable
                    string concreteBrowserKey = browserPathKey.Replace("$BROWSER$", progId);
                    var kp = Registry.ClassesRoot.OpenSubKey(concreteBrowserKey, false);
                    var browserPath = CleanifyBrowserPath(kp?.GetValue(null) as string);
                    kp?.Close();
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
