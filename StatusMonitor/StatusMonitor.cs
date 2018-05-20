using Eddi;
using EddiEvents;
using EddiShipMonitor;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;
using EddiDataDefinitions;
using System.Text;

namespace EddiStatusMonitor
{
    public class StatusMonitor : EDDIMonitor
    {
        // What we are monitoring and what to do with it
        private static Regex Filter = new Regex(@"^Status\.json$");
        private static Regex JsonRegex = new Regex(@"^{.*}$");
        private string Directory = GetSavedGamesDir();
        public static Status currentStatus { get; set; } = new Status();
        public static Status lastStatus { get; set; } = new Status();

        // Keep track of status
        private bool running;

        public StatusMonitor()
        {
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        public string MonitorName()
        {
            return "Status monitor";
        }

        public string LocalizedMonitorName()
        {
            return "Status monitor";
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return "Monitor Elite: Dangerous' Status.json for current status.  This should not be disabled unless you are sure you know what you are doing, as it will result in many functions inside EDDI no longer working";
        }

        public bool IsRequired()
        {
            return true;
        }

        public bool NeedsStart()
        {
            return true;
        }

        public void Start()
        {
            start();
        }

        /// <summary>Monitor Status.json for changes</summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        public void start()
        {
            if (Directory == null || Directory.Trim() == "")
            {
                return;
            }

            running = true;

            // Start off by moving to the end of the file
            string lastStatus = string.Empty;
            FileInfo fileInfo = null;
            try
            {
                fileInfo = FindStatusFile(Directory, Filter);
            }
            catch (NotSupportedException nsex)
            {
                Logging.Error("Directory " + Directory + " not supported: ", nsex);
            }
            if (fileInfo != null)
            {
                try
                {
                    using (FileStream fs = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (StreamReader reader = new StreamReader(fs, Encoding.UTF8))
                    {
                        lastStatus = reader.ReadLine() ?? string.Empty;

                        // Main loop
                        while (running)
                        {
                            if (fileInfo == null)
                            {
                                // Status.json could not be found. Sleep until a Status.json file is found.
                                Logging.Info("Error locating Elite Dangerous Status.json. Status monitor is not active. Have you installed and run Elite Dangerous previously? ");
                                while (fileInfo == null)
                                {
                                    Thread.Sleep(5000);
                                    fileInfo = FindStatusFile(Directory, Filter);
                                }
                                Logging.Info("Elite Dangerous Status.json found. Status monitor activated.");
                                return;
                            }
                            else
                            {
                                string thisStatus = string.Empty;
                                try
                                {
                                    fs.Seek(0, SeekOrigin.Begin);
                                    thisStatus = reader.ReadLine() ?? string.Empty;
                                }
                                catch (Exception)
                                {
                                    // file open elsewhere or being written, just wait for the next pass
                                }
                                if (lastStatus != thisStatus && !string.IsNullOrWhiteSpace(thisStatus))
                                {
                                    Status status = ParseStatusEntry(thisStatus);

                                    // Spin off a thread to pass status entry updates in the background
                                    Thread updateThread = new Thread(() => handleStatus(status));
                                    updateThread.IsBackground = true;
                                    updateThread.Start();
                                }
                                lastStatus = thisStatus;
                            }
                            Thread.Sleep(500);
                        }
                    }
                }
                catch (Exception)
                {
                    // file open elsewhere or being written, just wait for the next pass
                }
            }
        }

        public void Stop()
        {
            running = false;
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return null;
        }

        public static Status ParseStatusEntry(string line)
        {
            Status status = new Status();
            try
            {
                Match match = JsonRegex.Match(line);
                if (match.Success)
                {
                    IDictionary<string, object> data = Deserializtion.DeserializeData(line);

                    // Every status event has a timestamp field
                    if (data.ContainsKey("timestamp"))
                    {
                        if (data["timestamp"] is DateTime)
                        {
                            status.timestamp = ((DateTime)data["timestamp"]).ToUniversalTime();
                        }
                        else
                        {
                            status.timestamp = DateTime.Parse(JsonParsing.getString(data, "timestamp")).ToUniversalTime();
                        }
                    }
                    else
                    {
                        Logging.Warn("Status event without timestamp; using current time");
                    }

                    status.flags = (Status.Flags)(JsonParsing.getOptionalLong(data, "Flags") ?? 0);
                    if (status.flags == Status.Flags.None)
                    {
                        // No flags are set. We aren't in game.
                        return status;
                    }

                    object val;
                    data.TryGetValue("Pips", out val);
                    List<long> pips = ((List<object>)val)?.Cast<long>()?.ToList(); // The 'TryGetValue' function returns these values as type 'object<long>'
                    status.pips_sys = pips != null ? ((decimal?)pips[0] / 2) : null; // Set system pips (converting from half pips)
                    status.pips_eng = pips != null ? ((decimal?)pips[1] / 2) : null; // Set engine pips (converting from half pips)
                    status.pips_wea = pips != null ? ((decimal?)pips[2] / 2) : null; // Set weapon pips (converting from half pips)

                    status.firegroup = JsonParsing.getOptionalInt(data, "FireGroup");
                    int? gui_focus = JsonParsing.getOptionalInt(data, "GuiFocus");
                    switch (gui_focus)
                    {
                        case 0: // No focus
                            {
                                status.gui_focus = "none";
                                break;
                            }
                        case 1: // InternalPanel (right hand side)
                            {
                                status.gui_focus = "internal panel";
                                break;
                            }
                        case 2: // ExternalPanel (left hand side)
                            {
                                status.gui_focus = "external panel";
                                break;
                            }
                        case 3: // CommsPanel (top)
                            {
                                status.gui_focus = "communications panel";
                                break;
                            }
                        case 4: // RolePanel (bottom)
                            {
                                status.gui_focus = "role panel";
                                break;
                            }
                        case 5: // StationServices
                            {
                                status.gui_focus = "station services";
                                break;
                            }
                        case 6: // GalaxyMap
                            {
                                status.gui_focus = "galaxy map";
                                break;
                            }
                        case 7: // SystemMap
                            {
                                status.gui_focus = "system map";
                                break;
                            }
                    }
                    status.latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                    status.longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                    status.altitude = JsonParsing.getOptionalDecimal(data, "Altitude");
                    status.heading = JsonParsing.getOptionalDecimal(data, "Heading");

                    return status;
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to parse Status.json line: " + ex.ToString());
                Logging.Error(ex);
            }
            return status = null;
        }

        public static void handleStatus(Status thisStatus)
        {
            if (thisStatus == null)
            {
                return;
            }

            if (currentStatus != thisStatus)
            {
                // Save our last status for reference and update our current status
                lastStatus = currentStatus;
                currentStatus = thisStatus;

                // Post a status event to share the new status with other monitors and responders 
                EDDI.Instance.eventHandler(new StatusEvent(thisStatus.timestamp, thisStatus));

                // Trigger events for changed status, as applicable
                if (thisStatus.srv_turret_deployed != lastStatus.srv_turret_deployed)
                {
                    EDDI.Instance.eventHandler(new SRVTurretEvent(thisStatus.timestamp, thisStatus.srv_turret_deployed));
                }
                if (thisStatus.silent_running != lastStatus.silent_running)
                {
                    EDDI.Instance.eventHandler(new SilentRunningEvent(thisStatus.timestamp, thisStatus.silent_running));
                }
                if (thisStatus.srv_under_ship != lastStatus.srv_under_ship)
                {
                    // If the turret is deployable then we are not under our ship. And vice versa. 
                    bool deployable = !thisStatus.srv_under_ship;
                    EDDI.Instance.eventHandler(new SRVTurretDeployableEvent(thisStatus.timestamp, deployable));
                }
                if (thisStatus.fsd_status != lastStatus.fsd_status 
                    && thisStatus.vehicle == Constants.VEHICLE_SHIP 
                    && !thisStatus.docked)
                {
                    if (thisStatus.fsd_status == "ready")
                    {
                        switch(lastStatus.fsd_status)
                        {
                            case "charging":
                                EDDI.Instance.eventHandler(new ShipFsdEvent(thisStatus.timestamp, "charging complete"));
                                break;
                            case "cooldown":
                                EDDI.Instance.eventHandler(new ShipFsdEvent(thisStatus.timestamp, "cooldown complete"));
                                break;
                            case "masslock":
                                EDDI.Instance.eventHandler(new ShipFsdEvent(thisStatus.timestamp, "masslock cleared"));
                                break;
                        }
                    }
                    else
                    {
                        EDDI.Instance.eventHandler(new ShipFsdEvent(thisStatus.timestamp, thisStatus.fsd_status));
                    }
                }
                if (thisStatus.low_fuel != lastStatus.low_fuel)
                {
                    // Don't trigger 'low fuel' event when fuel exceeds 25% or when we're not in our ship
                    if (thisStatus.low_fuel 
                        && thisStatus.vehicle == Constants.VEHICLE_SHIP) 
                    {
                        EDDI.Instance.eventHandler(new ShipLowFuelEvent(thisStatus.timestamp));
                    }
                }
            }
        }

        private static string GetSavedGamesDir()
        {
            IntPtr path;
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out path);
            if (result >= 0)
            {
                return Marshal.PtrToStringUni(path) + @"\Frontier Developments\Elite Dangerous";
            }
            else
            {
                throw new ExternalException("Failed to find the saved games directory.", result);
            }
        }

        internal class NativeMethods
        {
            [DllImport("Shell32.dll")]
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)]Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
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

        /// <summary>Find the latest file in a given directory matching a given expression, or null if no such file exists</summary>
        private static FileInfo FindStatusFile(string path, Regex filter = null)
        {
            if (path == null)
            {
                // Configuration can be changed underneath us so we do have to check each time...
                return null;
            }

            var directory = new DirectoryInfo(path);
            if (directory != null)
            {
                try
                {
                    FileInfo info = directory.GetFiles().Where(f => filter == null || filter.IsMatch(f.Name)).FirstOrDefault();
                    if (info != null)
                    {
                        // This info can be cached so force a refresh
                        info.Refresh();
                    }
                    return info;
                }
                catch { }
            }
            return null;
        }
    }
}
