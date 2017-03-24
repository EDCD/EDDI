using Eddi;
using SimpleFeedReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Utilities;
using EddiEvents;
using EddiCompanionAppService;
using Newtonsoft.Json.Linq;

namespace GalnetMonitor
{
    /// <summary>
    /// A sample EDDI monitor to watch The Elite: Dangerous RSS feed and generate an event for new items
    /// </summary>
    public class GalnetMonitor : EDDIMonitor
    {
        private readonly string SOURCE = "https://community.elitedangerous.com/";
        private readonly string RESOURCE = "/galnet-rss";

        private GalnetConfiguration configuration = new GalnetConfiguration();

        private bool running = false;

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
        public System.Windows.Controls.UserControl ConfigurationTabItem()
        {
            return null;
        }

        private void monitor()
        {
            while (running)
            {
                List<News> newsItems = new List<News>();
                string firstUid = null;
                try
                {
                    foreach (FeedItem item in new FeedReader(new GalnetFeedItemNormalizer(), true).RetrieveFeed(SOURCE + configuration.language + RESOURCE))
                    {
                        if (firstUid == null)
                        {
                            // Obtain the ID of the first item that we read
                            firstUid = item.Id;
                            if (configuration.lastuuid == null)
                            {
                                // We don't have any ID yet; use this as the marker
                                break;
                            }
                        }

                        if (item.Id == configuration.lastuuid)
                        {
                            // Reached the first item we have already seen - go no further
                            break;
                        }

                        if (isInteresting(item.Title))
                        {
                            News newsItem = new News(item.Id, item.PublishDate.DateTime, categoryFromTitle(item.Title), item.Title, item.GetContent());
                            newsItems.Add(newsItem);
                            GalnetSqLiteRepository.Instance.SaveNews(newsItem);
                        }
                    }
                }
                catch (WebException wex)
                {
                    Logging.Error("Exception attempting to obtain galnet feed: ", wex);
                }

                if (newsItems.Count > 0)
                {
                    EDDI.Instance.eventHandler(new GalnetNewsPublishedEvent(DateTime.Now, newsItems));
                }

                if (firstUid != null && firstUid != configuration.lastuuid)
                {
                    Logging.Debug("Updated latest UID to " + firstUid);
                    configuration.lastuuid = firstUid;
                    configuration.ToFile();
                }
                Thread.Sleep(120000);
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
            return (title != "Powerplay: Incoming Update" && title != "Luttes d'influence galactiques";
        }

        /// <summary>
        /// Pick a category for the news item given its title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string categoryFromTitle(string title)
        {
            if (configuration.language == "en")
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
            else if (configuration.language == "fr")
            {
                if (title.StartsWith("Actualité galactique : Rapport hebdomadaire - "))
                {
                    string subtitle = title.Replace("Actualité galactique : Rapport hebdomadaire -", "");
                    if (subtitle == "Démocratie")
                    {
                        return "Democracy Report";
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
                }

                if (title.StartsWith("Opération communautaire : "))
                {
                    return "Community Goal";
                }

                if (title == "Actualité galactique : Mise à jour - État des spatioports")
                {
                    return "Starport Status Update";
                }

                return "Article";
            }
            else if (configuration.language == "de")
            {

            }
            else
            {
                return "Article";
            }
        }
}
