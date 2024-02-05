using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using System.Xml;
using Utilities;
using Version = System.Version;

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
        protected static ResourceManager resourceManager = Properties.GalnetMonitor.ResourceManager;

        private bool running;
        private DateTime journalTimeStamp;

        // This monitor currently requires game version 4.0 or later.
        private static readonly Version minGameVersion = new Version(4, 0);

        public GalnetMonitor ()
        {
            configuration = ConfigService.Instance.galnetConfiguration;
        }

        /// <summary>
        /// The name of the monitor; shows up in EDDI's configuration window
        /// </summary>
        public string MonitorName ()
        {
            return "Galnet monitor";
        }

        public string LocalizedMonitorName ()
        {
            return Properties.GalnetMonitor.name;
        }

        /// <summary>
        /// The description of the monitor; shows up in EDDI's configuration window
        /// </summary>
        public string MonitorDescription ()
        {
            return Properties.GalnetMonitor.desc;
        }

        public bool IsRequired ()
        {
            return false;
        }

        public bool NeedsStart ()
        {
            return true;
        }

        /// <summary>
        /// This method is run when the monitor is requested to start
        /// </summary>
        public void Start ()
        {
            EDDI.Instance.GameVersionUpdated += OnGameVersionUpdated;
            running = true;
            locales = GetGalnetLocales();
            Monitor();
        }

        private void OnGameVersionUpdated ( object sender, EventArgs e )
        {
            if ( sender is Version currentGameVersion )
            {
                if ( currentGameVersion < minGameVersion )
                {
                    Logging.Warn( $"Monitor disabled. Game version is {currentGameVersion}, monitor may only receive data for version {minGameVersion} or later." );
                    Stop();
                }
                else
                {
                    if ( !running )
                    {
                        Start();
                    }
                }
            }
        }

        /// <summary>
        /// This method is run when the monitor is requested to stop
        /// </summary>
        public void Stop ()
        {
            running = false;
        }

        public void Reload ()
        {
            configuration = ConfigService.Instance.galnetConfiguration;
        }

        /// <summary>
        /// This method returns a user control with configuration controls.
        /// It is attached the the monitor's configuration tab in EDDI.
        /// </summary>
        public UserControl ConfigurationTabItem ()
        {
            return new ConfigurationWindow();
        }

        private void Monitor ()
        {
            var inGameOnlyStartDelayMilliSecs = new TimeSpan( 0, 5, 0 ); // 5 mins
            var passiveIntervalMilliSecs = new TimeSpan( 0, 15, 0 ); // 15 mins
            var activeIntervalMilliSecs = new TimeSpan( 0, 5, 0 ); // 5 mins

            bool firstRun = true;
            while ( running )
            {
                if ( configuration.galnetAlwaysOn )
                {
                    FetchGalnet();
                    Thread.Sleep( passiveIntervalMilliSecs );
                }
                else
                {
                    // We'll update the Galnet Monitor only if a journal event has taken place within the specified number of minutes
                    if ( ( DateTime.UtcNow - journalTimeStamp ) < passiveIntervalMilliSecs )
                    {
                        if ( firstRun )
                        {
                            // Wait at least 5 minutes after starting before polling for new articles
                            firstRun = false;
                            Thread.Sleep( inGameOnlyStartDelayMilliSecs );
                        }
                        FetchGalnet();
                        Thread.Sleep( activeIntervalMilliSecs );
                    }
                    else
                    {
                        Logging.Debug( "No in-game activity detected, skipping galnet feed update" );
                        Thread.Sleep( passiveIntervalMilliSecs );
                    }
                }
            }
        }

        private void FetchGalnet ()
        {
            List<News> newsItems = new List<News>();
            string firstUid = null;

            locales.TryGetValue( configuration.language, out locale );
            var url = GetGalnetResource("sourceURL");

            Logging.Debug( "Fetching Galnet articles from " + url );
            var items = GetFeedItems( url );
            if ( items != null )
            {
                try
                {
                    foreach ( var item in items )
                    {
                        if ( item is null )
                        { continue; }
                        try
                        {
                            if ( string.IsNullOrEmpty( firstUid ) )
                            {
                                // Obtain the ID of the first item that we read as a marker
                                firstUid = item?.Id;
                            }

                            if ( item.Id == configuration.lastuuid )
                            {
                                // Reached the first item we have already seen - go no further
                                break;
                            }

                            if ( item.Title is null || item.Content is null )
                            {
                                // Skip items which do not contain useful content.
                                continue;
                            }

                            Logging.Debug( "Handling Galnet feed item", item );
                            var newsItem = new News(item.Id, assignCategory(item.Title, item.Content), item.Title, item.Content, item.PublishDate.DateTime, false);
                            newsItems.Add( newsItem );
                            GalnetSqLiteRepository.Instance.SaveNews( newsItem );
                        }
                        catch ( Exception ex )
                        {
                            Logging.Error( $"Exception handling Galnet feed item: {item.Title}", ex );
                        }
                    }

                    if ( firstUid != null && firstUid != configuration.lastuuid )
                    {
                        Logging.Debug( "Updated latest UID to " + firstUid );
                        configuration.lastuuid = firstUid;
                        ConfigService.Instance.galnetConfiguration = configuration;
                    }

                    if ( newsItems.Count > 0 )
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
                catch ( Exception ex )
                {
                    Logging.Error( "Exception attempting to handle galnet feed: ", ex );
                }
            }
        }
        private List<FeedItem> GetFeedItems ( string url, bool fromAltUrl = false )
        {
            var items = new List<FeedItem>();
            try
            {
                using ( var reader = XmlReader.Create( url ) )
                {
                    var feed = SyndicationFeed.Load( reader );
                    var normalizer = new GalnetFeedItemNormalizer( fromAltUrl );
                    foreach ( var syndicationItem in feed.Items )
                    {
                        var feedItem = normalizer.Normalize( feed, syndicationItem );
                        if ( feedItem != null )
                        {
                            items.Add( feedItem );
                        }
                    }
                }
            }
            catch ( WebException wex )
            {
                // FDev has in the past made available an alternate Galnet feed. We'll try the alternate feed.
                // If the alternate feed fails, the page may not currently be available without an FDev login.
                if ( url == GetGalnetResource( "sourceURL" ) )
                {
                    Logging.Warn( "Exception contacting primary Galnet feed: ", wex );
                    url = GetGalnetResource( "alternateURL" );
                    Logging.Warn( "Trying alternate Galnet feed (may not work)" + url );
                    items = GetFeedItems( url, true );
                }
            }
            catch ( XmlException xex )
            {
                Logging.Error( "Exception attempting to obtain galnet feed: ", xex );
            }
            catch ( FileNotFoundException ex )
            {
                Logging.Error( "Exception attempting to obtain galnet feed: ", ex );
            }

            return items;
        }

        public void PreHandle ( Event @event )
        {
            if ( !@event.fromLoad )
            {
                journalTimeStamp = @event.timestamp;
            }
        }

        public void PostHandle ( Event @event )
        {
        }

        public void HandleProfile ( JObject profile )
        {
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return null;
        }

        /// <summary>
        /// Pick a category for the news item given its title
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string assignCategory ( string title, string content )
        {
            try
            {
                if ( title.StartsWith( GetGalnetResource( "titleFilterPowerplay" ) ) )
                {
                    return GetGalnetResource( "categoryPowerplay" );
                }

                if ( title.StartsWith( GetGalnetResource( "titleFilterStarportStatus" ) ) )
                {
                    return GetGalnetResource( "categoryStarportStatus" );
                }

                if ( title.StartsWith( GetGalnetResource( "titleFilterWeekInReview" ) ) )
                {
                    return GetGalnetResource( "categoryWeekInReview" );
                }
                if ( title.StartsWith( GetGalnetResource( "titleFilterCg" ) ) ||
                    Regex.IsMatch( content, GetGalnetResource( "contentFilterCgRegex" ) ) )
                {
                    return GetGalnetResource( "categoryCG" );
                }
            }
            catch ( Exception ex )
            {
                Logging.Error( $"Exception categorizing Galnet article {title}.", ex );
            }

            return GetGalnetResource( "categoryArticle" );
        }

        private string GetGalnetResource ( string basename )
        {
            try
            {
                var ci = locale != null ? CultureInfo.GetCultureInfo(locale) : CultureInfo.InvariantCulture;
                var res = resourceManager.GetString(basename, ci);
                if ( string.IsNullOrEmpty( res ) )
                {
                    // Fallback to our invariant culture if the local language returns an empty result
                    res = resourceManager.GetString( basename, CultureInfo.InvariantCulture );
                }
                return res;
            }
            catch ( Exception ex )
            {
                Logging.Error( "Failed to obtain Galnet resource for " + basename, ex );
                return null;
            }
        }

        public Dictionary<string, string> GetGalnetLocales ()
        {
            var galnetLocales = new Dictionary<string, string>
            {
                { "English", "en" } // Add our "neutral" language "en".
            };

            // Add our satellite resource language folders to the list. Since these are stored according to folder name, we can interate through folder names to identify supported resources
            var satelliteLocales = new Dictionary<string, string>();
            var fileInfo = new FileInfo(Assembly.GetExecutingAssembly().Location).DirectoryName;
            if ( fileInfo is null )
            { return galnetLocales; }
            var rootInfo = new DirectoryInfo(fileInfo);
            var subDirs = rootInfo.GetDirectories();
            foreach ( var dir in subDirs )
            {
                string name = dir.Name;
                if ( name == "x86" ||
                    name == "x64" ||
                    !dir.GetFiles().Any( f => f.Extension.Equals( ".dll" ) )
                    )
                {
                    continue;
                }
                if ( !dir.GetFiles().Any() )
                {
                    continue;
                }

                try
                {
                    var cInfo = new CultureInfo(name);
                    var resourceSet = resourceManager.GetResourceSet(cInfo, true, true);
                    if ( !string.IsNullOrEmpty( resourceSet?.GetString( "sourceURL" ) ) )
                    {
                        satelliteLocales.Add( cInfo.DisplayName, name );
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
            foreach ( var key in list )
            {
                galnetLocales.Add( key, satelliteLocales[ key ] );
            }

            return galnetLocales;
        }
    }
}
