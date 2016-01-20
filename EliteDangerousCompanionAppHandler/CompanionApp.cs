using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;

namespace EliteDangerousDataProvider
{
    public class CompanionApp
    {
        private static string serverRoot = "https://companion.orerve.net";

        private CookieWebClient client;

        public CompanionApp(Credentials credentials)
        {
            var cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, credentials);
            AddMachineIdCookie(cookieContainer, credentials);
            AddMachineTokenCookie(cookieContainer, credentials);

            client = new CookieWebClient(cookieContainer);
            client.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257");
        }

        //<summary>
        //Log in.  Returns credentials (null if the login is unsuccessful)
        //</summary>
        public static Credentials Login(string username, string password)
        {
            Credentials credentials = null;
            string location = serverRoot + "/user/login";
            //bool complete = false;
            //CookieContainer cookies = new CookieContainer();
            //while (!complete)
            //{
                // Send the request.
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location);
                request.AllowAutoRedirect = false;  // Don't redirect or we lose the cookies
                //request.CookieContainer = cookies;
                request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";
                request.ContentType = "application/x-www-form-urlencoded";
                request.Method = "POST";
                string encodedUsername = WebUtility.UrlEncode(username);
                string encodedPassword = WebUtility.UrlEncode(password);
                byte[] data = Encoding.UTF8.GetBytes("email=" + encodedUsername + "&password=" + encodedPassword);
                request.ContentLength = data.Length;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(data, 0, data.Length);
                dataStream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // A good response is a redirect status code
            if ((int)response.StatusCode < 300 || (int)response.StatusCode > 399)
            {
                throw new Exception();
            }

            // Obtain the cookies from the raw information available to us
            String cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
                if (cookieHeader != null)
                {
                    Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                    if (companionAppMatch.Success)
                    {
                        if (credentials == null) { credentials = new Credentials(); }
                        credentials.appId = companionAppMatch.Groups[1].Value;
                        //AddCompanionAppCookie(cookies, credentials);
                    }
                    Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                    if (machineIdMatch.Success)
                    {
                        if (credentials == null) { credentials = new Credentials(); }
                        credentials.machineId = machineIdMatch.Groups[1].Value;
                        //AddMachineIdCookie(cookies, credentials);
                    }
                    Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                    if (machineTokenMatch.Success)
                    {
                        if (credentials == null) { credentials = new Credentials(); }
                        credentials.machineToken = machineTokenMatch.Groups[1].Value;
                        //AddMachineTokenCookie(cookies, credentials);
                    }
                }

                //foreach (Cookie cookie in response.Cookies)
                //{
                //    if (cookie.Name == "CompanionApp")
                //    {
                //        if (credentials == null) { credentials = new Credentials(); }
                //        credentials.appId = cookie.Value;
                //    }
                //    if (cookie.Name == "mid")
                //    {
                //        if (credentials == null) { credentials = new Credentials(); }
                //        credentials.machineId = cookie.Value;
                //    }
                //}

                // Handle the response
                //if ((int)response.StatusCode >= 300 && (int)response.StatusCode <= 399)
                //{
                //    location = serverRoot + response.Headers["Location"];
                //}
                //else
                //{
                //    complete = true;
                //}

                // We need to break out the cookie header ourselves manually
                //String cookieHeader = response.Headers[HttpResponseHeader.SetCookie];

            //}
            return credentials;
        }

        public static Credentials Confirm(Credentials credentials, string code)
        {
            var cookieContainer = new CookieContainer();
            AddCompanionAppCookie(cookieContainer, credentials);
            AddMachineIdCookie(cookieContainer, credentials);
            AddMachineTokenCookie(cookieContainer, credentials);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(serverRoot + "/user/confirm");
            request.AllowAutoRedirect = false;
            request.CookieContainer = cookieContainer;
            request.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_1_2 like Mac OS X) AppleWebKit/537.51.2 (KHTML, like Gecko) Mobile/11D257";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";
            string encodedCode = WebUtility.UrlEncode(code);
            byte[] data = Encoding.UTF8.GetBytes("code=" + encodedCode);
            request.ContentLength = data.Length;
            Stream dataStream = request.GetRequestStream();
            dataStream.Write(data, 0, data.Length);
            dataStream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            // A good response is a found
            if ((int)response.StatusCode < 200 || (int)response.StatusCode > 399)
            {
                throw new Exception();
            }

            // Refresh the cookies from the raw information available to us
            String cookieHeader = response.Headers[HttpResponseHeader.SetCookie];
            if (cookieHeader != null)
            {
                Match companionAppMatch = Regex.Match(cookieHeader, @"CompanionApp=([^;]+)");
                if (companionAppMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.appId = companionAppMatch.Groups[1].Value;
                    //AddCompanionAppCookie(cookies, credentials);
                }
                Match machineIdMatch = Regex.Match(cookieHeader, @"mid=([^;]+)");
                if (machineIdMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineId = machineIdMatch.Groups[1].Value;
                    //AddMachineIdCookie(cookies, credentials);
                }
                Match machineTokenMatch = Regex.Match(cookieHeader, @"mtk=([^;]+)");
                if (machineTokenMatch.Success)
                {
                    if (credentials == null) { credentials = new Credentials(); }
                    credentials.machineToken = machineTokenMatch.Groups[1].Value;
                    //AddMachineTokenCookie(cookies, credentials);
                }
            }

            return credentials;
        }

        public dynamic Profile()
        {
            // Obtain and parse our response
            return JObject.Parse(client.DownloadString("https://companion.orerve.net/profile"));
        }

        private static void AddCompanionAppCookie(CookieContainer cookies, Credentials credentials)
        {
            var appCookie = new Cookie();
            appCookie.Domain = "companion.orerve.net";
            appCookie.Path = "/";
            appCookie.Name = "CompanionApp";
            appCookie.Value = credentials.appId;
            cookies.Add(appCookie);
        }

        private static void AddMachineIdCookie(CookieContainer cookies, Credentials credentials)
        {
            var machineIdCookie = new Cookie();
            machineIdCookie.Domain = ".companion.orerve.net";
            machineIdCookie.Path = "/";
            machineIdCookie.Name = "mid";
            machineIdCookie.Value = credentials.machineId;
            cookies.Add(machineIdCookie);
        }

        private static void AddMachineTokenCookie(CookieContainer cookies, Credentials credentials)
        {
            var machineTokenCookie = new Cookie();
            machineTokenCookie.Domain = ".companion.orerve.net";
            machineTokenCookie.Path = "/";
            machineTokenCookie.Name = "mtk";
            machineTokenCookie.Value = credentials.machineToken;
            cookies.Add(machineTokenCookie);
        }
    }
}
