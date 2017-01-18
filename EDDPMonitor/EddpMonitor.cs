using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utilities;

namespace EddiEddpMonitor
{
    /// <summary>
    /// An EDDI monitor to watch the EDDP feed for changes to the state of systems and stations
    /// </summary>
    public class EddpMonitor : EDDIMonitor
    {
        private bool running = false;

        private EddpConfiguration configuration;

        /// <summary>
        /// The name of the monitor; shows up in EDDI's configuration window
        /// </summary>
        public string MonitorName()
        {
            return "System monitor";
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
            return @"Monitor EDDP for changes in system control and state, and generate events when changes occur";
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

        public void Reload() { }

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
                try
                {
                    // We only listen for updates if the user has selected anything to listen to
                    if (true) // (configuration.watches != null && configuration.watches.Count > 0)
                    {
                        using (var subscriber = new SubscriberSocket())
                        {
                            subscriber.Connect("tcp://api.eddp.co:5556");
                            subscriber.Subscribe("eddp.delta.system");
                            while (running)
                            {
                                string topic = subscriber.ReceiveFrameString();
                                string message = subscriber.ReceiveFrameString();
                                Logging.Warn("Message is " + message);
                                JObject json = JObject.Parse(message);
                                if (topic == "eddp.delta.system")
                                {
                                    handleSystemDelta(json);
                                }
                                //string data;
                                //byte[] compressed = subscriber.ReceiveFrameBytes();
                                //using (var stream = new MemoryStream(compressed, 2, compressed.Length - 2))
                                //using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
                                //using (var streamReader = new StreamReader(inflater))
                                //{
                                //    data = streamReader.ReadToEnd();
                                //}
                                //JObject json = JObject.Parse(data);

                                //Logging.Debug("received message");
                                //// We care about journal entries for 'Docked' and 'FSDJump'
                                //if (json["$schemaRef"] != null && (string)json["$schemaRef"] == "http://schemas.elite-markets.net/eddn/journal/1")
                                //{
                                //    if (json["message"] != null)
                                //    {
                                //        string eventName = (string)json["message"]["event"];
                                //        if (eventName == "FSDJump")
                                //        {
                                //            handleFSDJumpEvent(json);
                                //        }
                                //    }
                                //}
                            }
                        }
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Logging.Error("Caught exception", ex);
                }
            }
        }

        private void handleSystemDelta(JObject json)
        {
            // Fetch guaranteed information
            string systemname = (string)json["systemname"];
            Logging.Warn("System name is " + systemname);
            decimal x = (decimal)(double)json["x"];
            decimal y = (decimal)(double)json["y"];
            decimal z = (decimal)(double)json["z"];

            // Fetch delta information
            string oldfaction = (string)json["oldfaction"];
            string newfaction = (string)json["newfaction"];

            string oldstate = (string)json["oldstate"];
            string newstate = (string)json["newstate"];

            string oldallegiance = (string)json["oldallegiance"];
            string newallegiance = (string)json["newallegiance"];

            string oldgovernment = (string)json["oldgovernment"];
            string newgovernment = (string)json["newgovernment"];

            string oldeconomy = (string)json["oldeconomy"];
            string neweconomy= (string)json["neweconomy"];

            string oldsecurity = (string)json["oldsecurity"];
            string newsecurity = (string)json["newsecurity"];

            // See if this matches our parameters
            string matchname = match(systemname, x, y, z, oldfaction, newfaction, oldstate, newstate);
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
                        system.state = newstate;
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
                } else if (newstate != null)
                {
                    @event = new SystemStateChangedEvent(DateTime.Now, matchname, systemname, oldstate, newstate);
                }
                if (@event != null)
                {
                    EDDI.Instance.eventHandler(@event);
                }
            }

            //try
            //{
            //    // Send faction changed event
            //    SystemFactionChangedEvent @event = new SystemFactionChangedEvent(DateTime.Now, systemname, oldfactionname, factionname);
            //    EDDI.Instance.eventHandler(@event);
            //}
            //catch (Exception ex)
            //{
            //    // Could be lots of reasons, just error it and move on
            //    Logging.Error("Failed to handle EDDP message: ", ex);
            //}
        }

        //private void handleFSDJumpEvent(JObject json)
        //{
        //    // Pull the relevant bits from the JSON
        //    try
        //    {
        //        string systemname = (string)json["message"]["StarSystem"];
        //        string factionname = (string)json["message"]["SystemFaction"];
        //        string factionstate = (string)json["message"]["FactionState"];
        //        // Faction state is the ED version, so change it to a sane version here
        //        if (factionstate != null)
        //        {
        //            factionstate = State.FromEDName(factionstate).name;
        //        }
        //        string allegiance = (string)json["message"]["SystemAllegiance"];
        //        // Allegiance is the ED version, so change it to a sane version here
        //        if (allegiance != null)
        //        {
        //            Superpower superpower = Superpower.FromEDName(allegiance);
        //            if (superpower != null)
        //            {
        //                allegiance = superpower.name;
        //            }
        //        }
        //        decimal x = (decimal)(double)json["message"]["StarPos"][0];
        //        decimal y = (decimal)(double)json["message"]["StarPos"][1];
        //        decimal z = (decimal)(double)json["message"]["StarPos"][2];
        //        decimal? distance = null;
        //        if (EDDI.Instance.CurrentStarSystem != null)
        //        {
        //            distance = (decimal)Math.Round(Math.Sqrt(Math.Pow((double)(x - EDDI.Instance.CurrentStarSystem.x), 2)
        //                                                   + Math.Pow((double)(y - EDDI.Instance.CurrentStarSystem.y), 2)
        //                                                   + Math.Pow((double)(z - EDDI.Instance.CurrentStarSystem.z), 2)), 2);
        //        }

        //        if (match(systemname, factionname, factionstate, allegiance, distance))
        //        {
        //            Logging.Debug("Watch match for " + systemname + "/" + factionname + "/" + factionstate + "/" + allegiance + "/" + distance);

        //            // The user is potentially interested in this event

        //            // Fetch the relevant starsystem
        //            StarSystem system = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(systemname);

        //            if (system != null)
        //            {
        //                if (factionname != system.faction)
        //                {
        //                    Logging.Debug("System faction has changed");
        //                    string oldfactionname = system.faction;

        //                    // Update the system information so that it's up-to-date
        //                    system.state = factionstate;
        //                    system.faction = factionname;
        //                    system.allegiance = allegiance;
        //                    StarSystemSqLiteRepository.Instance.SaveStarSystem(system);

        //                    // Send faction changed event
        //                    SystemFactionChangedEvent @event = new SystemFactionChangedEvent(DateTime.Now, systemname, oldfactionname, factionname);
        //                    EDDI.Instance.eventHandler(@event);
        //                }
        //                else if (factionstate != system.state)
        //                {
        //                    Logging.Debug("System state has changed");
        //                    string oldstate = system.state;

        //                    // Update the system information so that it's up-to-date
        //                    system.state = factionstate;
        //                    system.allegiance = allegiance;
        //                    StarSystemSqLiteRepository.Instance.SaveStarSystem(system);

        //                    // Send state changed event
        //                    SystemStateChangedEvent @event = new SystemStateChangedEvent(DateTime.Now, systemname, factionname, oldstate, factionstate);
        //                    EDDI.Instance.eventHandler(@event);
        //                }

        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Could be lots of reasons, just error it and move on
        //        Logging.Error("Failed to handle EDDN message: ", ex);
        //    }
        //}

        /// <summary>
        /// Find a matching watch for a given set of parameters
        /// </summary>
        private static string match(string systemname, decimal x, decimal y, decimal z, string oldfaction, string newfaction, string oldstate, string newstate)
        {
            return "local news";
        }
    }
}
