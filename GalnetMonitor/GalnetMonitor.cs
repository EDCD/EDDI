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

namespace GalnetMonitor
{
    /// <summary>
    /// A sample EDDI monitor to watch The Elite: Dangerous RSS feed and generate an event for new items
    /// </summary>
    public class GalnetMonitor : EDDIMonitor
    {
        private readonly string SOURCE = "https://community.elitedangerous.com/galnet-rss";

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

        public void Reload() {}

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
            string lastUid;
            try
            {
                lastUid = File.ReadAllText(Constants.DATA_DIR + @"\galnet");
            }
            catch
            {
                lastUid = null;
            }

            while (running)
            {
                List<News> newsItems = new List<News>();
                string firstUid = null;
                try
                {
                    foreach (FeedItem item in new FeedReader(new GalnetFeedItemNormalizer(), true).RetrieveFeed(SOURCE))
                    {
                        if (firstUid == null)
                        {
                            // Obtain the ID of the first item that we read
                            firstUid = item.Id;
                            if (lastUid == null)
                            {
                                // We don't have any ID yet; use this as the marker
                                break;
                            }
                        }

                        if (item.Id == lastUid)
                        {
                            // Reached the first item we have already seen - go no further
                            break;
                        }

                        News newsItem = new News(item.PublishDate.DateTime, item.Title, item.GetContent());
                        newsItems.Add(newsItem);
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

                if (firstUid != null && firstUid != lastUid)
                {
                    Logging.Debug("Updated latest UID to " + firstUid);
                    File.WriteAllText(Constants.DATA_DIR + @"\galnet", firstUid);
                    lastUid = firstUid;
                }
                Thread.Sleep(120000);
            }
        }
    }
}
