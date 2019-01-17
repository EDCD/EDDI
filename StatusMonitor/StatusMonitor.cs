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
        private static readonly Regex JsonRegex = new Regex(@"^{.*}$");
        private string Directory = GetSavedGamesDir();
        public static Status currentStatus { get; set; } = new Status();
        public static Status lastStatus { get; set; } = new Status();

        private static bool gliding;
        private static bool jumping;

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
                fileInfo = Files.FileInfo(Directory, "Status.json");
            }
            catch (NotSupportedException nsex)
            {
                Logging.Error("Directory " + Directory + " not supported: ", nsex);
            }
            if (fileInfo.Exists)
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
                                    fileInfo = Files.FileInfo(Directory, "Status.json");
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
                                    Thread updateThread = new Thread(() => handleStatus(status))
                                    {
                                        IsBackground = true
                                    };
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

                    data.TryGetValue("Pips", out object val);
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
                        case 8: // Orrery
                            {
                                status.gui_focus = "orrery";
                                break;
                            }
                        case 9: // FSS mode
                            {
                                status.gui_focus = "fss mode";
                                break;
                            }
                        case 10: // SAA mode
                            {
                                status.gui_focus = "saa mode";
                                break;
                            }
                        case 11: // Codex
                            {
                                status.gui_focus = "codex";
                                break;
                            }
                    }
                    status.latitude = JsonParsing.getOptionalDecimal(data, "Latitude");
                    status.longitude = JsonParsing.getOptionalDecimal(data, "Longitude");
                    status.altitude = JsonParsing.getOptionalDecimal(data, "Altitude");
                    status.heading = JsonParsing.getOptionalDecimal(data, "Heading");
                    if (data.TryGetValue("Fuel", out object fuelData))
                    {
                        if (fuelData is IDictionary<string, object> fuelInfo)
                        {
                            decimal? mainFuel = JsonParsing.getOptionalDecimal(fuelInfo, "FuelMain");
                            decimal? reserveFuel = JsonParsing.getOptionalDecimal(fuelInfo, "FuelReservoir");
                            status.fuel = mainFuel + reserveFuel;
                        }
                    }
                    status.cargo_carried = (int?)JsonParsing.getOptionalDecimal(data, "Cargo");

                    // Calculated data
                    SetFuelExtras(status);

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
                                if (!jumping && thisStatus.supercruise == lastStatus.supercruise)
                                {
                                    EDDI.Instance.eventHandler(new ShipFsdEvent(thisStatus.timestamp, "charging cancelled"));
                                }
                                jumping = false;
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
                if (thisStatus.landing_gear_down != lastStatus.landing_gear_down)
                {
                    EDDI.Instance.eventHandler(new ShipLandingGearEvent(thisStatus.timestamp, thisStatus.landing_gear_down));
                }
                if (thisStatus.cargo_scoop_deployed != lastStatus.cargo_scoop_deployed)
                {
                    EDDI.Instance.eventHandler(new ShipCargoScoopEvent(thisStatus.timestamp, thisStatus.cargo_scoop_deployed));
                }
                if (thisStatus.lights_on != lastStatus.lights_on)
                {
                    EDDI.Instance.eventHandler(new ShipLightsEvent(thisStatus.timestamp, thisStatus.lights_on));
                }
                if (gliding && thisStatus.fsd_status == "cooldown")
                {
                    gliding = false;
                    EDDI.Instance.eventHandler(new GlideEvent(currentStatus.timestamp, gliding, EDDI.Instance.CurrentStellarBody.systemname, EDDI.Instance.CurrentStellarBody.systemAddress, EDDI.Instance.CurrentStellarBody.name, EDDI.Instance.CurrentStellarBody.Type));
                }

                // Reset our fuel log if we change vehicles or refuel
                if (thisStatus.vehicle != lastStatus.vehicle || thisStatus.fuel > lastStatus.fuel)
                {
                    fuelLog = null;
                }
            }
        }

        private static string GetSavedGamesDir()
        {
            int result = NativeMethods.SHGetKnownFolderPath(new Guid("4C5C32FF-BB9D-43B0-B5B4-2D72E54EAAA4"), 0, new IntPtr(0), out IntPtr path);
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
            // Some events can be derived from our status during a given event
            if (@event is EnteredNormalSpaceEvent)
            {
                handleEnteredNormalSpaceEvent(@event);
            }
            else if (@event is FSDEngagedEvent)
            {
                handleFSDEngagedEvent(@event);
            }
        }

        private void handleEnteredNormalSpaceEvent(Event @event)
        {
            // We can derive a "Glide" event from the context in our status
            if (currentStatus.near_surface && currentStatus.fsd_status == "masslock")
            {
                gliding = true;
                EnteredNormalSpaceEvent theEvent = (EnteredNormalSpaceEvent)@event;
                EDDI.Instance.eventHandler(new GlideEvent(DateTime.UtcNow, gliding, theEvent.system, theEvent.systemAddress, theEvent.body, theEvent.bodyType) { raw = @event.raw, fromLoad = @event.fromLoad });
            }
        }

        private void handleFSDEngagedEvent(Event @event)
        {
            if (((FSDEngagedEvent)@event).target == "Hyperspace")
            {
                jumping = true;
            }
            EDDI.Instance.eventHandler(new ShipFsdEvent(DateTime.UtcNow, "charging complete") { raw = @event.raw, fromLoad = @event.fromLoad });
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

        public Status GetStatus()
        {
            return currentStatus;
        }

        private static void SetFuelExtras(Status status)
        {
            decimal? fuel_rate = FuelConsumptionPerSecond(status.timestamp, status.fuel);
            FuelPercentAndTime(status.fuel, fuel_rate, out decimal? fuel_percent, out int? fuel_seconds);
            status.fuel_percent = fuel_percent;
            status.fuel_seconds = fuel_seconds;
        }

        private static List<KeyValuePair<DateTime, decimal?>> fuelLog;
        private static decimal? FuelConsumptionPerSecond(DateTime timestamp, decimal? fuel, int trackingMinutes = 5)
        {
            if (fuel is null)
            {
                return null;
            }

            if (fuelLog is null)
            {
                fuelLog = new List<KeyValuePair<DateTime, decimal?>>();
            }
            else
            {
                fuelLog?.RemoveAll(log => (DateTime.UtcNow - log.Key).TotalMinutes > trackingMinutes);
            }
            fuelLog.Add(new KeyValuePair<DateTime, decimal?>(timestamp, fuel));

            decimal? fuelConsumed = fuelLog.First().Value - fuelLog.Last().Value;
            TimeSpan timespan = fuelLog.Last().Key - fuelLog.First().Key;

            return timespan.Seconds == 0 ? null : fuelConsumed / timespan.Seconds; // Return tons of fuel consumed per second
        }

        private static void FuelPercentAndTime(decimal? fuelRemaining, decimal? fuelPerSecond, out decimal? fuel_percent, out int? fuel_seconds)
        {
            fuel_percent = null;
            fuel_seconds = null;

            if (fuelRemaining is null)
            {
                return;
            }

            if (currentStatus.vehicle == Constants.VEHICLE_SHIP)
            {
                Ship ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
                if (ship?.fueltanktotalcapacity != null && fuelRemaining != null)
                {
                    // Fuel recorded in Status.json includes the fuel carried in the Active Fuel Reservoir
                    decimal percent = (decimal)(fuelRemaining / (ship.fueltanktotalcapacity + ship.activeFuelReservoirCapacity) * 100);
                    fuel_percent = percent > 10 ? Math.Round(percent, 0) : Math.Round(percent, 1);
                    fuel_seconds = (fuelPerSecond is null || fuelPerSecond == 0) ? null : (int?)((ship.fueltanktotalcapacity + ship.activeFuelReservoirCapacity) / fuelPerSecond);
                }
            }
            else if (currentStatus.vehicle == Constants.VEHICLE_SRV)
            {
                const decimal srvFuelTankCapacity = 0.45M;
                decimal percent = (decimal)(fuelRemaining / srvFuelTankCapacity * 100);
                fuel_percent = percent > 10 ? Math.Round(percent, 0) : Math.Round(percent, 1);
                fuel_seconds = (fuelPerSecond is null || fuelPerSecond == 0) ? null : (int?)(srvFuelTankCapacity / fuelPerSecond);
            }
            return; // At present, fighters do not appear to consume fuel.
        }
    }
}
