using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEddpMonitor;
using EddiEvents;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using Utilities;

namespace EddiEddpMonitor
{
    /// <summary>
    /// An EDDI monitor to watch the EDDP feed for changes to the state of systems and stations
    /// </summary>
    public class EddpMonitor : EDDIMonitor
    {
        private bool running = false;
        private bool reloading = false;

        private EddpConfiguration configuration;

        /// <summary>
        /// The name of the monitor; shows up in EDDI's configuration window
        /// </summary>
        public string MonitorName()
        {
            return "EDDP monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.EddpResources.name;
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
            return Properties.EddpResources.desc;
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
            configuration = EddpConfiguration.FromFile();
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
            // Reload the configuration and let the monitor know that we have done so
            configuration = EddpConfiguration.FromFile();
            reloading = true;
        }

        /// <summary>
        /// This method returns a user control with configuration controls.
        /// It is attached the the monitor's configuration tab in EDDI.
        /// </summary>
        public System.Windows.Controls.UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        private void monitor()
        {
            while (running)
            {
                while (!reloading)
                {
                    try
                    {
                        // We only listen for updates if the user has selected anything to listen to
                        if (configuration.watches != null && configuration.watches.Count > 0)
                        {
                            using (var subscriber = new SubscriberSocket())
                            {
                                subscriber.Connect("tcp://api.eddp.co:5556");
                                subscriber.Subscribe("eddp.delta.system");
                                while (running && !reloading)
                                {
                                    string topic = null;
                                    if (subscriber.TryReceiveFrameString(new TimeSpan(0, 0, 1), out topic))
                                    {
                                        string message = subscriber.ReceiveFrameString();
                                        Logging.Debug("Message is " + message);
                                        JObject json = JObject.Parse(message);
                                        if (topic == "eddp.delta.system")
                                        {
                                            handleSystemDelta(json);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Error("Caught exception", ex);
                    }
                    finally
                    {
                        Thread.Sleep(1000);
                    }
                }
                Thread.Sleep(1000);
                reloading = false;
            }
        }

        private void handleSystemDelta(JObject json)
        {
            // Fetch guaranteed information
            string systemname = (string)json["systemname"];
            decimal x = (decimal)(double)json["x"];
            decimal y = (decimal)(double)json["y"];
            decimal z = (decimal)(double)json["z"];

            // Fetch delta information
            string oldfaction = (string)json["oldfaction"];
            string newfaction = (string)json["newfaction"];

            SystemState oldstate = SystemState.FromName((string)json["oldstate"]);
            SystemState newstate = SystemState.FromName((string)json["newstate"]);

            string oldallegiance = (string)json["oldallegiance"];
            string newallegiance = (string)json["newallegiance"];

            string oldgovernment = (string)json["oldgovernment"];
            string newgovernment = (string)json["newgovernment"];

            string oldeconomy = (string)json["oldeconomy"];
            string neweconomy = (string)json["neweconomy"];

            string oldsecurity = (string)json["oldsecurity"];
            string newsecurity = (string)json["newsecurity"];

            // See if this matches our parameters
            string matchname = match(systemname, null, x, y, z, oldfaction, newfaction, oldstate, newstate);
            if (matchname != null)
            {
                // Fetch the system from our local repository (but don't create it if it doesn't exist)
                StarSystem system = StarSystemSqLiteRepository.Instance.GetStarSystem(systemname);
                if (system != null)
                {
                    // Update our local copy of the system
                    if (newfaction != null)
                    {
                        system.faction = newfaction;
                    }
                    if (newstate != null)
                    {
                        system.systemState = newstate;
                    }
                    if (newallegiance != null)
                    {
                        system.allegiance = newallegiance;
                    }
                    if (newgovernment != null)
                    {
                        system.government = newgovernment;
                    }
                    if (neweconomy != null)
                    {
                        system.primaryeconomy = neweconomy;
                    }
                    if (newsecurity != null)
                    {
                        system.security = newsecurity;
                    }
                    system.lastupdated = DateTime.Now;
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(system);
                }

                // Send an appropriate event
                Event @event = null;
                if (newfaction != null)
                {
                    @event = new SystemFactionChangedEvent(DateTime.Now, matchname, systemname, oldfaction, newfaction);
                }
                else if (newstate != null)
                {
                    @event = new SystemStateChangedEvent(DateTime.Now, matchname, systemname, oldstate, newstate);
                }
                if (@event != null)
                {
                    EDDI.Instance.eventHandler(@event);
                }
            }
        }


        /// <summary>
        /// Find a matching watch for a given set of parameters
        /// </summary>
        private string match(string systemname, string stationname, decimal x, decimal y, decimal z, string oldfaction, string newfaction, SystemState oldstate, SystemState newstate)
        {
            foreach (Watch watch in configuration.watches)
            {
                if (watch.System != null && watch.System != systemname)
                {
                    continue;
                }

                if (watch.Station != null && watch.Station != stationname)
                {
                    continue;
                }

                if (watch.Faction != null && watch.Faction != oldfaction && watch.Faction != newfaction)
                {
                    continue;
                }

                if (watch.State != null && watch.State != oldstate && watch.State != newstate)
                {
                    continue;
                }

                if (EDDI.Instance.CurrentStarSystem != null)
                {

                    if (watch.MaxDistanceFromShip != null)
                    {
                        // Calculate the distance of the system from the ship
                        decimal distance = (decimal)Math.Sqrt(Math.Pow((double)(EDDI.Instance.CurrentStarSystem.x - x), 2)
                                                     + Math.Pow((double)(EDDI.Instance.CurrentStarSystem.y - y), 2)
                                                     + Math.Pow((double)(EDDI.Instance.CurrentStarSystem.z - z), 2));
                        if (distance > watch.MaxDistanceFromShip)
                        {
                            continue;
                        }
                    }

                    if (watch.MaxDistanceFromHome != null)
                    {
                        // Calculate the distance of the system from the home system
                        decimal distance = (decimal)Math.Sqrt(Math.Pow((double)(EDDI.Instance.HomeStarSystem.x - x), 2)
                                                     + Math.Pow((double)(EDDI.Instance.HomeStarSystem.y - y), 2)
                                                     + Math.Pow((double)(EDDI.Instance.HomeStarSystem.z - z), 2));
                        if (distance > watch.MaxDistanceFromHome)
                        {
                            continue;
                        }
                    }
                }

                // Passed all tests
                Logging.Debug("Message matched watch " + watch.Name);
                return watch.Name;
            }

            // No match
            Logging.Debug("Message did not match any watch; ignoring");
            return null;
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
    }
}
