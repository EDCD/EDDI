using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Controls;
using Utilities;

namespace EddiStatusMonitor
{
    public class StatusMonitor : EDDIMonitor
    {
        // What we are monitoring and what to do with it
        private static readonly Regex JsonRegex = new Regex(@"^{.*}$");
        private string Directory = GetSavedGamesDir();
        public Status currentStatus { get; private set; } = new Status();
        public Status lastStatus { get; private set; } = new Status();

        // Declare our constants
        private const int pollingIntervalActiveMs = 500;
        private const int pollingIntervalRelaxedMs = 5000;

        // Miscellaneous tracking
        private bool gliding;
        private bool jumping;
        private EnteredNormalSpaceEvent lastEnteredNormalSpaceEvent;

        // Keep track of status monitor 
        private bool running;

        public event EventHandler StatusUpdatedEvent;

        public StatusMonitor()
        {
            Logging.Info($"Initialized {MonitorName()}");
        }

        public string MonitorName()
        {
            return "Status monitor";
        }

        public string LocalizedMonitorName()
        {
            return "Status monitor";
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
            if (string.IsNullOrWhiteSpace(Directory))
            {
                return;
            }

            running = true;

            // Start off by moving to the end of the file
            FileInfo fileInfo = null;
            try
            {
                fileInfo = Files.FileInfo(Directory, "Status.json");
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
                        string lastStatusJson = reader.ReadLine() ?? string.Empty;

                        // Main loop
                        while (running)
                        {
                            if (Processes.IsEliteRunning())
                            {
                                if (!fileInfo.Exists)
                                {
                                    WaitForStatusFile(ref fileInfo);
                                }
                                else
                                {
                                    lastStatusJson = ReadStatus(lastStatusJson, fs, reader);
                                }
                                Thread.Sleep(pollingIntervalActiveMs);
                            }
                            else
                            {
                                Thread.Sleep(pollingIntervalRelaxedMs);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // file open elsewhere or being written, just wait for the next pass
                }
            }
        }

        private void WaitForStatusFile(ref FileInfo fileInfo)
        {
            // Status.json could not be found. Sleep until a Status.json file is found.
            Logging.Info("Error locating Elite Dangerous Status.json. Status monitor is not active. Have you installed and run Elite Dangerous previously? ");
            while (!fileInfo.Exists)
            {
                Thread.Sleep(pollingIntervalRelaxedMs);
                fileInfo = Files.FileInfo(Directory, "Status.json");
            }
            Logging.Info("Elite Dangerous Status.json found. Status monitor activated.");
        }

        private string ReadStatus(string lastStatusJson, FileStream fs, StreamReader reader)
        {
            string thisStatusJson = string.Empty;
            try
            {
                fs.Seek(0, SeekOrigin.Begin);
                thisStatusJson = reader.ReadLine() ?? string.Empty;
            }
            catch (Exception)
            {
                // file open elsewhere or being written, just wait for the next pass
            }
            if (lastStatusJson != thisStatusJson && !string.IsNullOrWhiteSpace(thisStatusJson))
            {
                Status status = ParseStatusEntry(thisStatusJson);

                // Spin off a thread to pass status entry updates in the background
                Thread updateThread = new Thread(() => handleStatus(status))
                {
                    IsBackground = true
                };
                updateThread.Start();
            }
            lastStatusJson = thisStatusJson;
            return lastStatusJson;
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

        public Status ParseStatusEntry(string line)
        {
            Status status = new Status() { raw = line };
            try
            {
                Match match = JsonRegex.Match(line);
                if (match.Success)
                {
                    IDictionary<string, object> data = Deserializtion.DeserializeData(line);

                    // Every status event has a timestamp field
                    status.timestamp = DateTime.UtcNow;
                    try
                    {
                        status.timestamp = JsonParsing.getDateTime("timestamp", data);
                    }
                    catch
                    {
                        Logging.Warn("Status without timestamp; using current time");
                    }

                    status.flags = (Status.Flags)(JsonParsing.getOptionalLong(data, "Flags") ?? 0);
                    status.flags2 = (Status.Flags2)(JsonParsing.getOptionalLong(data, "Flags2") ?? 0);
                    
                    if (status.flags == Status.Flags.None && status.flags2 == Status.Flags2.None)
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
                        case null:
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
                            status.fuelInTanks = JsonParsing.getOptionalDecimal(fuelInfo, "FuelMain");
                            status.fuelInReservoir = JsonParsing.getOptionalDecimal(fuelInfo, "FuelReservoir");
                        }
                    }
                    status.cargo_carried = (int?)JsonParsing.getOptionalDecimal(data, "Cargo");
                    status.legalStatus = LegalStatus.FromEDName(JsonParsing.getString(data, "LegalState")) ?? LegalStatus.Clean;
                    status.bodyname = JsonParsing.getString(data, "BodyName"); // Might be a station name if we're in an orbital station
                    status.planetradius = JsonParsing.getOptionalDecimal(data, "PlanetRadius");

                    // When on foot
                    status.oxygen = JsonParsing.getOptionalDecimal(data, "Oxygen") * 100; // Convert Oxygen to a 0-100 percent scale
                    status.health = JsonParsing.getOptionalDecimal(data, "Health") * 100; // Convert Health to a 0-100 percent scale
                    status.temperature = JsonParsing.getOptionalDecimal(data, "Temperature"); // In Kelvin
                    status.selected_weapon = JsonParsing.getString(data, "SelectedWeapon_Localised") 
                        ?? JsonParsing.getString(data, "SelectedWeapon"); // The name of the selected weapon
                    status.gravity = JsonParsing.getOptionalDecimal(data, "Gravity"); // Gravity, relative to 1G

                    // Calculated data
                    SetFuelExtras(status);
                    SetSlope(status);

                    return status;
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to parse Status.json line: " + ex.ToString());
                Logging.Error("", ex);
            }
            return null;
        }

        public void handleStatus(Status thisStatus)
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

                // Update glide status
                if (thisStatus.hyperspace || thisStatus.supercruise || thisStatus.docked || thisStatus.landed)
                {
                    gliding = false;
                }

                // Update vehicle information
                if (!string.IsNullOrEmpty(thisStatus.vehicle) && thisStatus.vehicle != lastStatus.vehicle && lastStatus.vehicle == EDDI.Instance.Vehicle)
                {
                    var statusSummary = new Dictionary<string, Status> { { "isStatus", thisStatus }, { "wasStatus", lastStatus } };
                    Logging.Debug($"Status changed vehicle from {lastStatus.vehicle} to {thisStatus.vehicle}", statusSummary);

                    EDDI.Instance.Vehicle = thisStatus.vehicle;
                }

                // Trigger events for changed status, as applicable
                if (thisStatus.shields_up != lastStatus.shields_up && thisStatus.vehicle == lastStatus.vehicle)
                {
                    // React to changes in shield state.
                    // We check the vehicle to make sure that events aren't generated when we switch vehicles, start the game, or stop the game.
                    if (thisStatus.shields_up)
                    {
                        EDDI.Instance.enqueueEvent(new ShieldsUpEvent(thisStatus.timestamp));
                    }
                    else
                    {
                        EDDI.Instance.enqueueEvent(new ShieldsDownEvent(thisStatus.timestamp));
                    }
                }
                if (thisStatus.srv_turret_deployed != lastStatus.srv_turret_deployed)
                {
                    EDDI.Instance.enqueueEvent(new SRVTurretEvent(thisStatus.timestamp, thisStatus.srv_turret_deployed));
                }
                if (thisStatus.silent_running != lastStatus.silent_running)
                {
                    EDDI.Instance.enqueueEvent(new SilentRunningEvent(thisStatus.timestamp, thisStatus.silent_running));
                }
                if (thisStatus.srv_under_ship != lastStatus.srv_under_ship && lastStatus.vehicle == Constants.VEHICLE_SRV)
                {
                    // If the turret is deployable then we are not under our ship. And vice versa. 
                    bool deployable = !thisStatus.srv_under_ship;
                    EDDI.Instance.enqueueEvent(new SRVTurretDeployableEvent(thisStatus.timestamp, deployable));
                }
                if (thisStatus.fsd_status != lastStatus.fsd_status
                    && thisStatus.vehicle == Constants.VEHICLE_SHIP
                    && !thisStatus.docked)
                {
                    if (thisStatus.fsd_status == "ready")
                    {
                        switch (lastStatus.fsd_status)
                        {
                            case "charging":
                                if (!jumping && thisStatus.supercruise == lastStatus.supercruise)
                                {
                                    EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, "charging cancelled"));
                                }
                                jumping = false;
                                break;
                            case "cooldown":
                                EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, "cooldown complete"));
                                break;
                            case "masslock":
                                EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, "masslock cleared"));
                                break;
                        }
                    }
                    else
                    {
                        EDDI.Instance.enqueueEvent(new ShipFsdEvent(thisStatus.timestamp, thisStatus.fsd_status));
                    }
                }
                if (thisStatus.low_fuel != lastStatus.low_fuel)
                {
                    // Don't trigger 'low fuel' event when fuel exceeds 25% or when we're not in our ship
                    if (thisStatus.low_fuel
                        && thisStatus.vehicle == Constants.VEHICLE_SHIP)
                    {
                        EDDI.Instance.enqueueEvent(new ShipLowFuelEvent(thisStatus.timestamp));
                    }
                }
                if (thisStatus.landing_gear_down != lastStatus.landing_gear_down 
                    && thisStatus.vehicle == Constants.VEHICLE_SHIP && lastStatus.vehicle == Constants.VEHICLE_SHIP)
                {
                    EDDI.Instance.enqueueEvent(new ShipLandingGearEvent(thisStatus.timestamp, thisStatus.landing_gear_down));
                }
                if (thisStatus.cargo_scoop_deployed != lastStatus.cargo_scoop_deployed)
                {
                    EDDI.Instance.enqueueEvent(new ShipCargoScoopEvent(thisStatus.timestamp, thisStatus.cargo_scoop_deployed));
                }
                if (thisStatus.lights_on != lastStatus.lights_on)
                {
                    EDDI.Instance.enqueueEvent(new ShipLightsEvent(thisStatus.timestamp, thisStatus.lights_on));
                }
                if (thisStatus.hardpoints_deployed != lastStatus.hardpoints_deployed)
                {
                    EDDI.Instance.enqueueEvent(new ShipHardpointsEvent(thisStatus.timestamp, thisStatus.hardpoints_deployed));
                }
                if (thisStatus.flight_assist_off != lastStatus.flight_assist_off)
                {
                    EDDI.Instance.enqueueEvent(new FlightAssistEvent(thisStatus.timestamp, thisStatus.flight_assist_off));
                }
                if (gliding && thisStatus.fsd_status == "cooldown")
                {
                    gliding = false;
                    EDDI.Instance.enqueueEvent(new GlideEvent(currentStatus.timestamp, gliding, EDDI.Instance.CurrentStellarBody?.systemname, EDDI.Instance.CurrentStellarBody?.systemAddress, EDDI.Instance.CurrentStellarBody?.bodyname, EDDI.Instance.CurrentStellarBody?.bodyType));
                }
                else if (!currentStatus.supercruise && lastStatus.supercruise)
                {
                    // We are exiting supercruise
                    if (!gliding && lastEnteredNormalSpaceEvent != null)
                    {
                        // We're not already gliding and we have data from a prior `EnteredNormalSpace` event
                        if (currentStatus.fsd_status == "ready"
                            && currentStatus.slope >= -60 && currentStatus.slope <= -5
                            && currentStatus.altitude < 100000
                            && currentStatus.altitude < lastStatus.altitude)
                        {
                            // The FSD status is `ready`, altitude is less than 100000 meters, and we are dropping
                            gliding = true;
                            EnteredNormalSpaceEvent theEvent = lastEnteredNormalSpaceEvent;
                            EDDI.Instance.enqueueEvent(new GlideEvent(DateTime.UtcNow, gliding, theEvent.systemname, theEvent.systemAddress, theEvent.bodyname, theEvent.bodyType) { fromLoad = theEvent.fromLoad });
                        }
                    }
                }
                // Reset our fuel log if we change vehicles or refuel
                if (thisStatus.vehicle != lastStatus.vehicle || thisStatus.fuel > lastStatus.fuel)
                {
                    fuelLog = null;
                }

                // Pass the change in status to all subscribed processes
                OnStatus(StatusUpdatedEvent, currentStatus);
            }
        }

        private void OnStatus(EventHandler statusUpdatedEvent, Status status)
        {
            statusUpdatedEvent?.Invoke(status, EventArgs.Empty);
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
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
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
            lastEnteredNormalSpaceEvent = (EnteredNormalSpaceEvent)@event;
        }

        private void handleFSDEngagedEvent(Event @event)
        {
            if (((FSDEngagedEvent)@event).target == "Hyperspace")
            {
                jumping = true;
            }
            EDDI.Instance.enqueueEvent(new ShipFsdEvent(DateTime.UtcNow, "charging complete") { fromLoad = @event.fromLoad });
        }

        public void PostHandle(Event @event)
        {
        }

        public void HandleProfile(JObject profile)
        {
        }

        public IDictionary<string, object> GetVariables()
        {
            Dictionary<string, object> variables = new Dictionary<string, object>
            {
                { "currentStatus", currentStatus },
                { "lastStatus", lastStatus }
            };
            return variables;
        }

        private void SetFuelExtras(Status status)
        {
            decimal? fuel_rate = FuelConsumptionPerSecond(status.timestamp, status.fuel);
            FuelPercentAndTime(status.fuel, fuel_rate, out decimal? fuel_percent, out int? fuel_seconds);
            status.fuel_percent = fuel_percent;
            status.fuel_seconds = fuel_seconds;
        }

        private List<KeyValuePair<DateTime, decimal?>> fuelLog;
        private decimal? FuelConsumptionPerSecond(DateTime timestamp, decimal? fuel, int trackingMinutes = 5)
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
            if (fuelLog.Count > 1)
            {
                decimal? fuelConsumed = fuelLog.FirstOrDefault().Value - fuelLog.LastOrDefault().Value;
                TimeSpan timespan = fuelLog.LastOrDefault().Key - fuelLog.FirstOrDefault().Key;

                return timespan.Seconds == 0 ? null : fuelConsumed / timespan.Seconds; // Return tons of fuel consumed per second
            }
            // Insufficient data, return 0.
            return 0;
        }

        private void FuelPercentAndTime(decimal? fuelRemaining, decimal? fuelPerSecond, out decimal? fuel_percent, out int? fuel_seconds)
        {
            fuel_percent = null;
            fuel_seconds = null;

            if (fuelRemaining is null)
            {
                return;
            }

            if (currentStatus.vehicle == Constants.VEHICLE_SHIP)
            {
                Ship ship = EDDI.Instance.CurrentShip;
                if (ship?.fueltanktotalcapacity > 0)
                {
                    // Fuel recorded in Status.json includes the fuel carried in the Active Fuel Reservoir
                    decimal percent = (decimal)(fuelRemaining / (ship.fueltanktotalcapacity + ship.activeFuelReservoirCapacity) * 100);
                    fuel_percent = percent > 10 ? Math.Round(percent, 0) : Math.Round(percent, 1);
                    fuel_seconds = fuelPerSecond > 0 ? (int?)((ship.fueltanktotalcapacity + ship.activeFuelReservoirCapacity) / fuelPerSecond) : null;
                }
            }
            else if (currentStatus.vehicle == Constants.VEHICLE_SRV)
            {
                const decimal srvFuelTankCapacity = 0.45M;
                decimal percent = (decimal)(fuelRemaining / srvFuelTankCapacity * 100);
                fuel_percent = percent > 10 ? Math.Round(percent, 0) : Math.Round(percent, 1);
                fuel_seconds = fuelPerSecond > 0 ? (int?)(srvFuelTankCapacity / fuelPerSecond) : null;
            }
            return; // At present, fighters do not appear to consume fuel.
        }

        private void SetSlope(Status status)
        {
            status.slope = null;
            if (lastStatus?.planetradius != null && lastStatus?.altitude != null && lastStatus?.latitude != null && lastStatus?.longitude != null)
            {
                if (status.planetradius != null && status.altitude != null && status.latitude != null && status.longitude != null)
                {
                    double square(double x) => x * x;

                    double radius = (double)status.planetradius / 1000;
                    double deltaAlt = (double)(status.altitude - lastStatus.altitude) / 1000;

                    // Convert latitude & longitude to radians
                    double currentLat = (double)status.latitude * Math.PI / 180;
                    double lastLat = (double)lastStatus.latitude * Math.PI / 180;
                    double deltaLat = currentLat - lastLat;
                    double deltaLong = (double)(status.longitude - lastStatus.longitude) * Math.PI / 180;

                    // Calculate distance traveled using Law of Haversines
                    double a = square(Math.Sin(deltaLat / 2)) + Math.Cos(currentLat) * Math.Cos(lastLat) * square(Math.Sin(deltaLong / 2));
                    double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    double distance = c * radius;

                    // Calculate the slope angle
                    double slope = Math.Atan2(deltaAlt, distance) * 180 / Math.PI;
                    status.slope = Math.Round((decimal)slope, 1);
                }
            }
        }
    }
}
