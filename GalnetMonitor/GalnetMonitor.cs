using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
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

namespace EddiGalnetMonitor
{

    /// <summary>
    /// A sample EDDI monitor to watch The Elite: Dangerous RSS feed and generate an event for new items
    /// </summary>
    public class GalnetMonitor : IEddiMonitor
    {
        private static Dictionary<string, string> locales = new Dictionary<string, string>();
        protected static string locale;
        private GalnetConfiguration configuration;
        protected static ResourceManager resourceManager = EddiGalnetMonitor.Properties.GalnetMonitor.ResourceManager;

        private bool running;
        private DateTime journalTimeStamp;

        public static bool altURL { get; private set; }

        // This monitor currently requires game version 4.0 or later.
        private static readonly System.Version minGameVersion = new System.Version(4, 0);

        public GalnetMonitor()
        {
            configuration = ConfigService.Instance.galnetConfiguration;
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
            EDDI.Instance.GameVersionUpdated += OnGameVersionUpdated;
            running = true;
            locales = GetGalnetLocales();
            monitor();
        }

        private void OnGameVersionUpdated(object sender, EventArgs e)
        {
            if (sender is System.Version currentGameVersion)
            {
                if (currentGameVersion < minGameVersion)
                {
                    Logging.Warn($"Monitor disabled. Game version is {currentGameVersion}, monitor may only receive data for version {minGameVersion} or later.");
                    Stop();
                }
                else
                {
                    if (!running)
                    {
                        Start();
                    }
                }
            }
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
            configuration = ConfigService.Instance.galnetConfiguration;
        }

        /// <summary>
        /// This method returns a user control with configuration controls.
        /// It is attached the the monitor's configuration tab in EDDI.
        /// </summary>
        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        private void monitor()
        {
            const int inGameOnlyStartDelayMilliSecs = 5 * 60 * 1000; // 5 mins
            const int passiveIntervalMilliSecs = 15 * 60 * 1000; // 15 mins
            const int activeIntervalMilliSecs = 5 * 60 * 1000; // 5 mins

            bool firstRun = true;
            while (running)
            {
                if (configuration.galnetAlwaysOn)
                {
                    monitorGalnet();
                    Thread.Sleep(passiveIntervalMilliSecs);
                }
                else
                {
                    // We'll update the Galnet Monitor only if a journal event has taken place within the specified number of minutes
                    if ((DateTime.UtcNow - journalTimeStamp).TotalMilliseconds < passiveIntervalMilliSecs)
                    {
                        if (firstRun)
                        {
                            // Wait at least 5 minutes after starting before polling for new articles
                            firstRun = false;
                            Thread.Sleep(inGameOnlyStartDelayMilliSecs);
                        }
                        monitorGalnet();
                        Thread.Sleep(activeIntervalMilliSecs);
                    }
                    else
                    {
                        Logging.Debug("No in-game activity detected, skipping galnet feed update");
                        Thread.Sleep(passiveIntervalMilliSecs);
                    }
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
                        // FDev has in the past made available an alternate Galnet feed. We'll try the alternate feed.
                        // If the alternate feed fails, the page may not currently be available without an FDev login.
                        Logging.Warn("Exception contacting primary Galnet feed: ", wex);
                        url = GetGalnetResource("alternateURL");
                        altURL = true;
                        Logging.Warn("Trying alternate Galnet feed (may not work)" + url);
                        try
                        {
                            items = feedReader.RetrieveFeed(url);
                        }
                        catch (Exception ex)
                        {
                            Logging.Warn("Galnet feed exception (alternate url unsuccessful): ", ex);
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
                            foreach (FeedItem item in items)
                            {
                                try
                                {
                                    if (string.IsNullOrEmpty(firstUid))
                                    {
                                        // Obtain the ID of the first item that we read as a marker
                                        firstUid = item?.Id;
                                    }

                                    if (item?.Id == configuration.lastuuid)
                                    {
                                        // Reached the first item we have already seen - go no further
                                        break;
                                    }

                                    if (item?.Title is null || item?.GetContent() is null)
                                    {
                                        // Skip items which do not contain useful content.
                                        continue;
                                    }

                                    Logging.Debug("Handling Galnet feed item", item);
                                    News newsItem = new News(item.Id, assignCategory(item.Title, item.GetContent()), item.Title, item.GetContent(), item.PublishDate.DateTime, false);
                                    newsItems.Add(newsItem);
                                    GalnetSqLiteRepository.Instance.SaveNews(newsItem);
                                }
                                catch (Exception ex)
                                {
                                    Logging.Error($"Exception handling Galnet feed item: {item?.Title}", ex);
                                }
                            }

                            if (firstUid != null && firstUid != configuration.lastuuid)
                            {
                                Logging.Debug("Updated latest UID to " + firstUid);
                                configuration.lastuuid = firstUid;
                                ConfigService.Instance.galnetConfiguration = configuration;
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
            if (!@event.fromLoad)
            {
                journalTimeStamp = @event.timestamp;
            }
        }

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables()
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
                Logging.Error($"Exception categorizing Galnet article {title}.", ex);
            }

            return GetGalnetResource("categoryArticle");
        }

        private string GetGalnetResource(string basename)
        {
            try
            {
                var ci = locale != null ? CultureInfo.GetCultureInfo(locale) : CultureInfo.InvariantCulture;
                var res = resourceManager.GetString(basename, ci);
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
            var galnetLocales = new Dictionary<string, string>
            {
                { "English", "en" } // Add our "neutral" language "en".
            };

            // Add our satellite resource language folders to the list. Since these are stored according to folder name, we can interate through folder names to identify supported resources
            var satelliteLocales = new Dictionary<string, string>();
            var fileInfo = new FileInfo(System.Reflection.Assembly.GetExecutingAssembly().Location).DirectoryName;
            if (fileInfo is null) { return galnetLocales; }
            var rootInfo = new DirectoryInfo(fileInfo);
            var subDirs = rootInfo.GetDirectories();
            foreach (var dir in subDirs)
            {
                string name = dir.Name;
                if (name == "x86" || 
                    name == "x64" ||
                    !dir.GetFiles().Any(f => f.Extension.Equals(".dll"))
                    )
                {
                    continue;
                }
                if (!dir.GetFiles().Any())
                {
                    continue;
                }

                try
                {
                    var cInfo = new CultureInfo(name);
                    var resourceSet = resourceManager.GetResourceSet(cInfo, true, true);
                    if (!string.IsNullOrEmpty(resourceSet?.GetString("sourceURL")))
                    {
                        satelliteLocales.Add(cInfo.DisplayName, name);
                    }
                }
                catch
                {
                    // Nothing to do here
                }
            }

            // Sort satellite locales prior to adding them to our list
            var list = satelliteLocales.Keys.ToList();
            list.Sort();
            foreach (var key in list)
            {
                galnetLocales.Add(key, satelliteLocales[key]);
            }

            return galnetLocales;
        }
    }
}
