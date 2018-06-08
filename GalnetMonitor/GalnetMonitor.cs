using Eddi;
using EddiEvents;
using Newtonsoft.Json.Linq;
using SimpleFeedReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Resources;
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
        private static Dictionary<string, string> locales = new Dictionary<string, string>() { { "English", "en" }, { "Français", "fr" }, { "Deutsch", "de" } };
        protected static string locale;
        private GalnetConfiguration configuration = new GalnetConfiguration();
        protected static ResourceManager resourceManager = EddiGalnetMonitor.Properties.GalnetMonitor.ResourceManager;

        private bool running = false;

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
                    try
                    {
                        locales.TryGetValue(configuration.language, out locale);
                        string url = GetGalnetResource("galnetSourceURL");

                        Logging.Debug("Fetching Galnet articles from " + url);
                        IEnumerable<FeedItem> items = new FeedReader(new GalnetFeedItemNormalizer(), true).RetrieveFeed(url);
                        if (items != null)
                        {
                            foreach (GalnetFeedItemNormalizer.ExtendedFeedItem item in items)
                            {
                                if (firstUid == null)
                                {
                                    // Obtain the ID of the first item that we read as a marker
                                    firstUid = item.Id;
                                }

                                if (item.Id == configuration.lastuuid)
                                {
                                    // Reached the first item we have already seen - go no further
                                    // break;
                                }

                                News newsItem = new News(item.Id, assignCategory(item.Title), item.Title, item.GetContent(), item.PublishDate.DateTime, false);
                                newsItems.Add(newsItem);
                                GalnetSqLiteRepository.Instance.SaveNews(newsItem);
                            }
                        }
                    }
                    catch (WebException wex)
                    {
                        Logging.Debug("Exception attempting to obtain galnet feed: ", wex);
                    }

                    if (firstUid != configuration.lastuuid)
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
                                EDDI.Instance.eventHandler(new GalnetNewsPublishedEvent(DateTime.UtcNow, newsItems));
                            }
                            catch (ThreadAbortException)
                            {
                                Logging.Debug("Thread aborted");
                            }
                        });
                        thread.IsBackground = true;
                        thread.Start();
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
        private string assignCategory(string title)
        {
            if (title.StartsWith(GetGalnetResource("galnetCategoryPowerplay")))
            {
                return "Powerplay";
            }

            if (title.StartsWith(GetGalnetResource("galnetCategoryCommunityGoal")))
            {
                return "Community Goal";
            }

            if (title.StartsWith(GetGalnetResource("galnetCategoryStarportStatus")))
            {
                return "Starport Status Update";
            }

            if (title.StartsWith(GetGalnetResource("galnetCategoryWeekInReview")))
            {
                return "Week in Review";
            }

            return "Article";
        }

        private static string GetGalnetResource(string basename)
        {
            CultureInfo ci = locale != null ? CultureInfo.GetCultureInfo(locale) : CultureInfo.InvariantCulture;
            return resourceManager.GetString(basename, ci) ?? null;
        }
    }
}
