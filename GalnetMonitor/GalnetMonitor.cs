using Eddi;
using SimpleFeedReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Utilities;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System.Windows.Controls;

namespace GalnetMonitor
{
    /// <summary>
    /// A sample EDDI monitor to watch The Elite: Dangerous RSS feed and generate an event for new items
    /// </summary>
    public class GalnetMonitor : EDDIMonitor
    {
        private readonly string SOURCE = "https://community.elitedangerous.com/";
        private readonly string RESOURCE = "/galnet-rss";
        private static Dictionary<string, string> locales = new Dictionary<string, string>() { { "English", "en" }, { "Français", "fr" }, { "Deutsch", "de" } };
        private GalnetConfiguration configuration = new GalnetConfiguration();

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
            return @"Monitor Galnet for new news items and generate a ""Galnet news published"" event when new items are posted";
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
                        string locale = "en";
                        locales.TryGetValue(configuration.language, out locale);
                        string url = SOURCE + locale + RESOURCE;
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
                                    break;
                                }

                                if (isInteresting(item.Title))
                                {
                                    News newsItem = new News(item.Id, categoryFromTitle(item.Title), item.Title, item.GetContent(), item.PublishDate.DateTime, false);
                                    newsItems.Add(newsItem);
                                    GalnetSqLiteRepository.Instance.SaveNews(newsItem);
                                }
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
                                EDDI.Instance.eventHandler(new GalnetNewsPublishedEvent(DateTime.Now, newsItems));
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

        private static bool isInteresting(string title)
        {
            return title != "Powerplay: Incoming Update" && title != "Luttes d'influence galactiques" && title != "Machtspiele: Neues Update";
        }

        /// <summary>
        /// Pick a category for the news item given its title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string categoryFromTitle(string title)
        {
            if (configuration.language == "English")
            {
                if (title.StartsWith("Galactic News: Weekly "))
                {
                    return title.Replace("Galactic News: Weekly ", "");
                }

                if (title.StartsWith("Community Goal: "))
                {
                    return "Community Goal";
                }

                if (title == "Galactic News: Starport Status Update")
                {
                    return "Starport Status Update";
                }

                return "Article";
            }
            else if (configuration.language == "Français")
            {
                if (title.StartsWith("Actualité galactique : Rapport hebdomadaire - "))
                {
                    string subtitle = title.Replace("Actualité galactique : Rapport hebdomadaire - ", "");
                    if (subtitle == "Démocratie")
                    {
                        return "Democracy Report";
                    }
                    else if (subtitle == "Conflits")
                    {
                        return "Conflict Report";
                    }
                    else if (subtitle == "Santé")
                    {
                        return "Health Report";
                    }
                    else if (subtitle == "Économie")
                    {
                        return "Economic Report";
                    }
                    else if (subtitle == "Sécurité")
                    {
                        return "Security Report";
                    }
                    else if (subtitle == "Expansions")
                    {
                        return "Expansion Report";
                    }
                }

                if (title.StartsWith("Opération communautaire"))
                {
                    return "Community Goal";
                }

                if (title == "Actualité galactique : Mise à jour - État des spatioports")
                {
                    return "Starport Status Update";
                }

                return "Article";
            }
            else if (configuration.language == "Deutsch")
            {
                if (title.StartsWith("Galaktische News: Wöchentlicher "))
                {
                    string subtitle = title.Replace("Galaktische News: Wöchentlicher ", "");
                    if (subtitle == "Demokratiereport")
                    {
                        return "Democracy Report";
                    }
                    else if (subtitle == "Konfliktreport")
                    {
                        return "Conflict Report";
                    }
                    else if (subtitle == "Gesundheitsreport")
                    {
                        return "Health Report";
                    }
                    else if (subtitle == "Wirtschaftsreport")
                    {
                        return "Economic Report";
                    }
                    else if (subtitle == "Sicherheitsreport")
                    {
                        return "Security Report";
                    }
                    else if (subtitle == "Expansionsreport")
                    {
                        return "Expansion Report";
                    }
                }
                if (title.StartsWith("Community-Ziel"))
                {
                    return "Community Goal";
                }

                if (title == "Galaktische News: Sternenhafen-Status-Update")
                {
                    return "Starport Status Update";
                }

                return "Article";
            }
            else
            {
                return "Article";
            }
        }
    }
}
