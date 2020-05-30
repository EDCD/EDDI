using Eddi;
using EddiDataDefinitions;
using EddiEvents;
using EddiMissionMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Threading;
using Utilities;

namespace EddiNavigationMonitor
{
    public class NavigationMonitor : EDDIMonitor
    {
        // Observable collection for us to handle changes
        public ObservableCollection<Bookmark> bookmarks { get; private set; }

        private DateTime updateDat;

        private static readonly object bookmarksLock = new object();
        public event EventHandler BookmarksUpdatedEvent;

        public string MonitorName()
        {
            return "Navigation monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.NavigationMonitor.navigation_monitor_name;
        }

        public string MonitorDescription()
        {
            return Properties.NavigationMonitor.navigation_monitor_desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public NavigationMonitor()
        {
            bookmarks = new ObservableCollection<Bookmark>();
            BindingOperations.CollectionRegistering += Bookmarks_CollectionRegistering;
            initializeNavigationMonitor();
        }

        public void initializeNavigationMonitor(NavigationMonitorConfiguration configuration = null)
        {
            readBookmarks(configuration);
            Logging.Info($"Initialized {MonitorName()}");
        }

        private void Bookmarks_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(bookmarks, bookmarksLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(bookmarks, bookmarksLock); });
            }
        }
        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return false;
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }

        public void Reload()
        {
            readBookmarks();
            Logging.Info($"Reloaded {MonitorName()}");
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(bookmarks, bookmarksLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(bookmarks); });
        }

        public void HandleProfile(JObject profile)
        {
        }

        public void PostHandle(Event @event)
        {
        }

        public void PreHandle(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));

            // Handle the events that we care about
            if (@event is RouteEvent)
            {
                handleRouteEvent((RouteEvent)@event);
            }

        }
        private void handleRouteEvent(RouteEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;

            }
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["bookmarks"] = new List<Bookmark>(bookmarks),
            };
            return variables;
        }

        public void writeBookmarks()
        {
            lock (bookmarksLock)
            {
                // Write cargo configuration with current inventory
                NavigationMonitorConfiguration configuration = new NavigationMonitorConfiguration()
                {
                    updatedat = updateDat,
                    bookmarks = bookmarks,

                };
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(BookmarksUpdatedEvent, bookmarks);
        }

        private void readBookmarks(NavigationMonitorConfiguration configuration = null)
        {
            lock (bookmarksLock)
            {
                // Obtain current cargo inventory from configuration
                configuration = configuration ?? NavigationMonitorConfiguration.FromFile();
                updateDat = configuration.updatedat;

                // Build a new bookmark list
                bookmarks.Clear();
                foreach (Bookmark bookmark in configuration.bookmarks)
                {
                    bookmarks.Add(bookmark);
                }
            }
        }

        private void RemoveBookmark(Bookmark bookmark)
        {
            _RemoveBookmark(bookmark);
        }

        public void _RemoveBookmark(Bookmark bookmark)
        {
            string system = bookmark.system.ToLowerInvariant();
            string body = bookmark.body?.ToLowerInvariant();
            string poi = bookmark.poi?.ToLowerInvariant();
            lock (bookmarksLock)
            {
                for (int i = 0; i < bookmarks.Count; i++)
                {
                    if (bookmarks[i].system.ToLowerInvariant() == system
                        && bookmarks[i].body?.ToLowerInvariant() == body
                        && bookmarks[i].poi?.ToLowerInvariant() == poi)
                    {
                        bookmarks.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        static void RaiseOnUIThread(EventHandler handler, object sender)
        {
            if (handler != null)
            {
                SynchronizationContext uiSyncContext = SynchronizationContext.Current ?? new SynchronizationContext();
                if (uiSyncContext == null)
                {
                    handler(sender, EventArgs.Empty);
                }
                else
                {
                    uiSyncContext.Send(delegate { handler(sender, EventArgs.Empty); }, null);
                }
            }
        }
    }
}
