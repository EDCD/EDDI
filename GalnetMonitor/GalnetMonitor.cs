﻿using Eddi;
using EddiEvents;
using Newtonsoft.Json.Linq;
using SimpleFeedReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Resources;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace GalnetMonitor
{

    /// <summary>
    /// A sample EDDI monitor to watch The Elite: Dangerous RSS feed and generate an event for new items
    /// </summary>
    public class GalnetMonitor : EDDIMonitor
    {
        private static Dictionary<string, string> locales = new Dictionary<string, string>();
        protected static string locale;
        private GalnetConfiguration configuration = new GalnetConfiguration();
        protected static ResourceManager resourceManager = EddiGalnetMonitor.Properties.GalnetMonitor.ResourceManager;

        private bool running = false;

        public static bool altURL { get; private set; }

        public GalnetMonitor()
        {
            // Remove the old configuration file if it still exists
            if (File.Exists(Constants.DATA_DIR + @"\galnet"))
            {
                try
                {
                    File.Delete(Constants.DATA_DIR + @"\galnet");
                }
                catch { }
            }

            configuration = GalnetConfiguration.FromFile();
        }

        /// <summary>
        /// The name of the monitor; shows up in EDDI's configuration window
        /// </summary>
        public string MonitorName()
        {
            return "Galnet monitor";
        }

        public string LocalizedMonitorName()
        {
            return EddiGalnetMonitor.Properties.GalnetMonitor.name;
        }

        /// <summary>
        /// The version of the monitor; shows up in EDDI's logs
        /// </summary>
        public string MonitorVersion()
        {
            return "1.0.0";
        }

        /// <summary>
        /// The description of the monitor; shows up in EDDI's configuration window
        /// </summary>
        public string MonitorDescription()
        {
            return EddiGalnetMonitor.Properties.GalnetMonitor.desc;
        }

        public bool IsRequired()
        {
            return false;
        }

        public bool NeedsStart()
        {
            return true;
        }

        /// <summary>
        /// This method is run when the monitor is requested to start
        /// </summary>
        public void Start()
        {
            running = true;
            locales = GetGalnetLocales();
            monitor();
        }

        public void Stop()
        /// <summary>
        /// This method is run when the monitor is requested to stop
        /// </summary>
        {
            running = false;
        }

        public void Reload()
        {
            configuration = GalnetConfiguration.FromFile();
        }

        /// <summary>
        /// This method returns a user control with configuration controls.
        /// It is attached the the monitor's configuration tab in EDDI.
        /// </summary>
        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void OnClosingConfigurationTabItem()
        {
        }

        private void monitor()
        {
            const int inGameOnlyStartDelayMilliSecs = 5 * 60 * 1000; // 5 mins
            const int alwaysOnIntervalMilliSecs = 2 * 60 * 1000; // 2 mins
            const int inGameOnlyIntervalMilliSecs = 30 * 1000; // 30 secs

            if (!configuration.galnetAlwaysOn)
            {
                // Wait at least 5 minutes after starting before polling for new articles, but only if galnetAlwaysOn is false
                Thread.Sleep(inGameOnlyStartDelayMilliSecs);
            }

            while (running)
            {
                if (configuration.galnetAlwaysOn)
                {
                    monitorGalnet();
                    Thread.Sleep(alwaysOnIntervalMilliSecs);
                }
                else
                {
                    // We'll update the Galnet Monitor only if a journal event has taken place within the specified number of minutes
                    if ((DateTime.UtcNow - EDDI.Instance.JournalTimeStamp).TotalMinutes < 10)
                    {
                        monitorGalnet();
                    }
                    else
                    {
                        Logging.Debug("No in-game activity detected, skipping galnet feed update");
                    }
                    Thread.Sleep(inGameOnlyIntervalMilliSecs);
                }

                void monitorGalnet()
                {
                    List<News> newsItems = new List<News>();
                    string firstUid = null;

                    locales.TryGetValue(configuration.language, out locale);
                    string url = GetGalnetResource("sourceURL");
                    altURL = false;

                    Logging.Debug("Fetching Galnet articles from " + url);
                    FeedReader feedReader = new FeedReader(new GalnetFeedItemNormalizer(), true);
                    IEnumerable<FeedItem> items = null;
                    try
                    {
                        items = feedReader.RetrieveFeed(url);
                    }
                    catch (WebException wex)
                    {
                        Logging.Warn("Exception contacting primary Galnet feed: ", wex);
                        url = GetGalnetResource("alternateURL");
                        altURL = true;
                        Logging.Warn("Trying alternate Galnet feed " + url);
                        try
                        {
                            items = feedReader.RetrieveFeed(url);
                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Galnet feed exception (alternate url unsuccessful): ", ex);
                        }
                    }
                    catch (System.Xml.XmlException xex)
                    {
                        Logging.Error("Exception attempting to obtain galnet feed: ", xex);
                    }

                    if (items != null)
                    {
                        try
                        {
                            foreach (GalnetFeedItemNormalizer.ExtendedFeedItem item in items)
                            {
                                try
                                {

                                    if (firstUid == null)
                                    {
                                        // Obtain the ID of the first item that we read as a marker
                                        firstUid = item.Id;
                                    }

                                    if (item.Id == configuration.lastuuid)
                                    {
                                        // Reached the first item we have already seen - go no further
                                        break;
                                    }

                                    if (item.Title is null || item.GetContent() is null)
                                    {
                                        // Skip items which do not contain useful content.
                                        continue;
                                    }

                                    News newsItem = new News(item.Id, assignCategory(item.Title, item.GetContent()), item.Title, item.GetContent(), item.PublishDate.DateTime, false);
                                    newsItems.Add(newsItem);
                                    GalnetSqLiteRepository.Instance.SaveNews(newsItem);
                                }
                                catch (Exception ex)
                                {
                                    Dictionary<string, object> data = new Dictionary<string, object>()
                                    {
                                        { "item", item },
                                        { "exception", ex}
                                    };
                                    Logging.Error("Exception handling Galnet news item.", data);
                                }
                            }

                            if (firstUid != null && firstUid != configuration.lastuuid)
                            {
                                Logging.Debug("Updated latest UID to " + firstUid);
                                configuration.lastuuid = firstUid;
                                configuration.ToFile();
                            }

                            if (newsItems.Count > 0)
                            {
                                // Spin out event in to a different thread to stop blocking
                                Thread thread = new Thread(() =>
                                {
                                    try
                                    {
                                        EDDI.Instance.enqueueEvent(new GalnetNewsPublishedEvent(DateTime.UtcNow, newsItems));
                                    }
                                    catch (ThreadAbortException)
                                    {
                                        Logging.Debug("Thread aborted");
                                    }
                                })
                                {
                                    IsBackground = true
                                };
                                thread.Start();
                            }

                        }
                        catch (Exception ex)
                        {
                            Logging.Error("Exception attempting to handle galnet feed: ", ex);
                        }
                    }
                }
            }
        }

        public void PreHandle(Event @event)
        {
        }

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
        }

        public IDictionary<string, object> GetVariables()
        {
            return null;
        }

        /// <summary>
        /// Pick a category for the news item given its title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string assignCategory(string title, string content)
        {
            try
            {
                if (title.StartsWith(GetGalnetResource("titleFilterPowerplay")))
                {
                    return GetGalnetResource("categoryPowerplay");
                }

                if (title.StartsWith(GetGalnetResource("titleFilterStarportStatus")))
                {
                    return GetGalnetResource("categoryStarportStatus");
                }

                if (title.StartsWith(GetGalnetResource("titleFilterWeekInReview")))
                {
                    return GetGalnetResource("categoryWeekInReview");
                }
                if (title.StartsWith(GetGalnetResource("titleFilterCg")) ||
                    Regex.IsMatch(content, GetGalnetResource("contentFilterCgRegex")))
                {
                    return GetGalnetResource("categoryCG");
                }
            }
            catch (Exception ex)
            {
                Dictionary<string, object> data = new Dictionary<string, object>()
                {
                    { "title", title },
                    { "content", content },
                    { "exception", ex }
                };
                Logging.Error("Exception categorizing Galnet article.", data);
            }

            return GetGalnetResource("categoryArticle");
        }

        private string GetGalnetResource(string basename)
        {
            try
            {
                CultureInfo ci = locale != null ? CultureInfo.GetCultureInfo(locale) : CultureInfo.InvariantCulture;
                string res = resourceManager.GetString(basename, ci);
                if (string.IsNullOrEmpty(res))
                {
                    // Fallback to our invariant culture if the local language returns an empty result
                    res = resourceManager.GetString(basename, CultureInfo.InvariantCulture);
                }
                return res;
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to obtain Galnet resource for " + basename, ex);
                return null;
            }
        }

        public Dictionary<string, string> GetGalnetLocales()
        {
            Dictionary<string, string> locales = new Dictionary<string, string>();

            locales.Add("English", "en"); // Add our "neutral" language "en".

            // Add our satellite resource language folders to the list. Since these are stored according to folder name, we can interate through folder names to identify supported resources
            Dictionary<string, string> satelliteLocales = new Dictionary<string, string>();
            DirectoryInfo rootInfo = new DirectoryInfo(new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName);
            DirectoryInfo[] subDirs = rootInfo.GetDirectories();
            foreach (DirectoryInfo dir in subDirs)
            {
                string name = dir.Name;
                if (name == "x86" || name == "x64")
                {
                    continue;
                }
                if (dir.GetFiles().Count() == 0)
                {
                    continue;
                }
                try
                {
                    CultureInfo cInfo = new CultureInfo(name);
                    ResourceSet resourceSet = resourceManager.GetResourceSet(cInfo, true, true);
                    if (!string.IsNullOrEmpty(resourceSet.GetString("sourceURL")))
                    {
                        satelliteLocales.Add(cInfo.DisplayName, name);
                    }
                }
                catch
                { }
            }

            // Sort satellite locales prior to adding them to our list
            var list = satelliteLocales.Keys.ToList();
            list.Sort();
            foreach (var key in list)
            {
                locales.Add(key, satelliteLocales[key]);
            }

            return locales;
        }
    }
}
