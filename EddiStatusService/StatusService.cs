using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Utilities;

namespace EddiStatusService
{
    public class StatusService
    {
        // Declare our constants
        private const int pollingIntervalActiveMs = 500;
        private const int pollingIntervalRelaxedMs = 5000;
        private static readonly Regex JsonRegex = new Regex(@"^{.*}$");
        private static readonly string Directory = GetSavedGamesDir();

        // Public Read Variables
        public Status CurrentStatus { get; private set; } = new Status();
        public Status LastStatus { get; private set; } = new Status();
        public static event EventHandler StatusUpdatedEvent;

        // Public Write variables (set elsewhere to assist with various calculations)
        public Ship CurrentShip;
        public List<KeyValuePair<DateTime, decimal?>> fuelLog;
        public EnteredNormalSpaceEvent lastEnteredNormalSpaceEvent;

        // Other variables used by this service
        private static StatusService instance;
        private static readonly object instanceLock = new object();
        public readonly object statusLock = new object();
        private bool running;

        public static StatusService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            Logging.Debug("No status service instance: creating one");
                            instance = new StatusService();
                        }
                    }
                }
                return instance;
            }
        }

        public void Start()
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
                        string LastStatusJson = reader.ReadLine() ?? string.Empty;

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
                                    LastStatusJson = ReadStatus(LastStatusJson, fs, reader);
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

        public void Stop()
        {
            running = false;
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

        private string ReadStatus(string LastStatusJson, FileStream fs, StreamReader reader)
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
            if (LastStatusJson != thisStatusJson && !string.IsNullOrWhiteSpace(thisStatusJson))
            {
                Status status = ParseStatusEntry(thisStatusJson);

                // Spin off a thread to pass status entry updates in the background
                Thread updateThread = new Thread(() => handleStatus(status))
                {
                    IsBackground = true
                };
                updateThread.Start();
            }
            LastStatusJson = thisStatusJson;
            return LastStatusJson;
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
                    status.credit_balance = JsonParsing.getOptionalULong(data, "Balance");

                    // When on foot
                    status.oxygen = JsonParsing.getOptionalDecimal(data, "Oxygen") * 100; // Convert Oxygen to a 0-100 percent scale
                    status.health = JsonParsing.getOptionalDecimal(data, "Health") * 100; // Convert Health to a 0-100 percent scale
                    status.temperature = JsonParsing.getOptionalDecimal(data, "Temperature"); // In Kelvin
                    status.selected_weapon = JsonParsing.getString(data, "SelectedWeapon_Localised")
                        ?? JsonParsing.getString(data, "SelectedWeapon"); // The name of the selected weapon
                    status.gravity = JsonParsing.getOptionalDecimal(data, "Gravity"); // Gravity, relative to 1G

                    // When not on foot
                    if (data.TryGetValue("Destination", out object destinationData))
                    {
                        if (destinationData is IDictionary<string, object> destinationInfo)
                        {
                            status.destinationSystemAddress = JsonParsing.getOptionalULong(destinationInfo, "System");
                            status.destinationBodyId = JsonParsing.getOptionalInt(destinationInfo, "Body");
                            status.destination_name = JsonParsing.getString(destinationInfo, "Name");
                            status.destination_localized_name = JsonParsing.getString(destinationInfo, "Name_Localised") ?? string.Empty;

                            // Destination might be a fleet carrier with name and carrier id in a single string. If so, we break them apart
                            var fleetCarrierRegex = new Regex("^(.+)(?> )([A-Za-z0-9]{3}-[A-Za-z0-9]{3})$");
                            if (string.IsNullOrEmpty(status.destination_localized_name) && fleetCarrierRegex.IsMatch(status.destination_name))
                            {
                                // Fleet carrier names include both the carrier name and carrier ID, we need to separate them
                                var fleetCarrierParts = fleetCarrierRegex.Matches(status.destination_name)[0].Groups;
                                if (fleetCarrierParts.Count == 3)
                                {
                                    status.destination_name = fleetCarrierParts[2].Value;
                                    status.destination_localized_name = fleetCarrierParts[1].Value;
                                }
                            }
                        }
                    }

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

            lock ( statusLock )
            {
                if ( CurrentStatus != thisStatus )
                {
                    // Save our last status for reference and update our current status
                    LastStatus = CurrentStatus;
                    CurrentStatus = thisStatus;

                    // Pass the change in status to all subscribed processes
                    OnStatus( StatusUpdatedEvent, CurrentStatus );
                }
            }
        }

        private void OnStatus(EventHandler statusUpdatedEvent, Status status)
        {
            statusUpdatedEvent?.Invoke(status, EventArgs.Empty);
        }

        private void SetFuelExtras(Status status)
        {
            decimal? fuel_rate = FuelConsumptionPerSecond(status.timestamp, status.fuel);
            FuelPercentAndTime(status.vehicle, status.fuel, fuel_rate, out decimal? fuel_percent, out int? fuel_seconds);
            status.fuel_percent = fuel_percent;
            status.fuel_seconds = fuel_seconds;
        }

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
                fuelLog.RemoveAll(log => (DateTime.UtcNow - log.Key).TotalMinutes > trackingMinutes);
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

        private void FuelPercentAndTime(string vehicle, decimal? fuelRemaining, decimal? fuelPerSecond, out decimal? fuel_percent, out int? fuel_seconds)
        {
            fuel_percent = null;
            fuel_seconds = null;

            if (fuelRemaining is null)
            {
                return;
            }

            if (vehicle == Constants.VEHICLE_SHIP && CurrentShip != null)
            {
                if (CurrentShip?.fueltanktotalcapacity > 0)
                {
                    // Fuel recorded in Status.json includes the fuel carried in the Active Fuel Reservoir
                    decimal percent = (decimal)(fuelRemaining / (CurrentShip.fueltanktotalcapacity + CurrentShip.activeFuelReservoirCapacity) * 100);
                    fuel_percent = percent > 10 ? Math.Round(percent, 0) : Math.Round(percent, 1);
                    fuel_seconds = fuelPerSecond > 0 ? (int?)((CurrentShip.fueltanktotalcapacity + CurrentShip.activeFuelReservoirCapacity) / fuelPerSecond) : null;
                }
            }
            else if (vehicle == Constants.VEHICLE_SRV)
            {
                const decimal srvFuelTankCapacity = 0.45M;
                decimal percent = (decimal)(fuelRemaining / srvFuelTankCapacity * 100);
                fuel_percent = percent > 10 ? Math.Round(percent, 0) : Math.Round(percent, 1);
                fuel_seconds = fuelPerSecond > 0 ? (int?)(srvFuelTankCapacity / fuelPerSecond) : null;
            }
            return; // At present, fighters do not appear to consume fuel.
        }

        ///<summary> Our calculated slope may be inaccurate if we are in normal space and thrusting
        /// in a direction other than the direction we are oriented (e.g. if pointing down towards a
        /// planet and applying reverse thrust to raise our altitude) </summary> 
        private void SetSlope(Status status)
        {
            status.slope = null;
            if (LastStatus?.planetradius != null && LastStatus?.altitude != null && LastStatus?.latitude != null && LastStatus?.longitude != null)
            {
                if (status.planetradius != null && status.altitude != null && status.latitude != null && status.longitude != null)
                {
                    double square(double x) => x * x;

                    var radiusKm = (double)status.planetradius / 1000;
                    var deltaAltKm = (double)(status.altitude - LastStatus.altitude) / 1000;

                    // Convert latitude & longitude to radians
                    var currentLat = (double)status.latitude * Math.PI / 180;
                    var lastLat = (double)LastStatus.latitude * Math.PI / 180;
                    var deltaLat = currentLat - lastLat;
                    var deltaLong = (double)(status.longitude - LastStatus.longitude) * Math.PI / 180;

                    // Calculate distance traveled using Law of Haversines
                    var a = square(Math.Sin(deltaLat / 2)) + (Math.Cos(currentLat) * Math.Cos(lastLat) * square(Math.Sin(deltaLong / 2)));
                    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
                    var distanceKm = c * radiusKm;

                    // Calculate the slope angle
                    var slopeRadians = Math.Atan2(deltaAltKm, distanceKm);
                    var slopeDegrees = slopeRadians * 180 / Math.PI;
                    status.slope = Math.Round((decimal)slopeDegrees, 1);
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
            internal static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);
        }
    }
}
