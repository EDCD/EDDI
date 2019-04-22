using Eddi;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiStarMapService;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Threading;
using Utilities;

namespace EddiCrimeMonitor
{
    /**
     * Monitor claims, fines, and bounties for the current ship
     */
    public class CrimeMonitor : EDDIMonitor
    {
        private bool running;

        // Observable collection for us to handle changes
        public ObservableCollection<FactionRecord> criminalrecord { get; private set; }
        public long claims;
        public long fines;
        public long bounties;
        public int? profitShare;
        public Dictionary<string, string> homeSystems;
        private DateTime updateDat;

        private static readonly object recordLock = new object();
        public event EventHandler RecordUpdatedEvent;

        public string MonitorName()
        {
            return "Crime monitor";
        }

        public string LocalizedMonitorName()
        {
            return Properties.CrimeMonitor.crime_monitor_name;
        }

        public string MonitorVersion()
        {
            return "1.0.0";
        }

        public string MonitorDescription()
        {
            return Properties.CrimeMonitor.crime_monitor_desc;
        }

        public bool IsRequired()
        {
            return true;
        }

        public CrimeMonitor()
        {
            criminalrecord = new ObservableCollection<FactionRecord>();
            homeSystems = new Dictionary<string, string>();
            BindingOperations.CollectionRegistering += Record_CollectionRegistering;
            initializeCrimeMonitor();
        }

        public void initializeCrimeMonitor(CrimeMonitorConfiguration configuration = null)
        {
            readRecord(configuration);
            Logging.Info("Initialised " + MonitorName() + " " + MonitorVersion());
        }

        private void Record_CollectionRegistering(object sender, CollectionRegisteringEventArgs e)
        {
            if (Application.Current != null)
            {
                // Synchronize this collection between threads
                BindingOperations.EnableCollectionSynchronization(criminalrecord, recordLock);
            }
            else
            {
                // If started from VoiceAttack, the dispatcher is on a different thread. Invoke synchronization there.
                Dispatcher.CurrentDispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(criminalrecord, recordLock); });
            }
        }
        public bool NeedsStart()
        {
            // We don't actively do anything, just listen to events
            return true;
        }

        public void Start()
        {
            _start();
        }

        public void Stop()
        {
            running = false;
        }

        public void Reload()
        {
            readRecord();
            Logging.Info("Reloaded " + MonitorName() + " " + MonitorVersion());

        }

        public void _start()
        {
            running = true;

            while (running)
            {
                List<FactionRecord> recordList;
                lock (recordLock)
                {
                    recordList = criminalrecord.ToList();
                }

                Thread.Sleep(5000);
            }
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void EnableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.EnableCollectionSynchronization(criminalrecord, recordLock); });
        }

        public void DisableConfigBinding(MainWindow configWindow)
        {
            configWindow.Dispatcher.Invoke(() => { BindingOperations.DisableCollectionSynchronization(criminalrecord); });
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
            if (@event is BondAwardedEvent)
            {
                handleBondAwardedEvent((BondAwardedEvent)@event);
            }
            else if (@event is BondRedeemedEvent)
            {
                handleBondRedeemedEvent((BondRedeemedEvent)@event);
            }
            else if (@event is BountyAwardedEvent)
            {
                handleBountyAwardedEvent((BountyAwardedEvent)@event);
            }
            else if (@event is BountyIncurredEvent)
            {
                handleBountyIncurredEvent((BountyIncurredEvent)@event);
            }
            else if (@event is BountyPaidEvent)
            {
                handleBountyPaidEvent((BountyPaidEvent)@event);
            }
            else if (@event is BountyRedeemedEvent)
            {
                handleBountyRedeemedEvent((BountyRedeemedEvent)@event);
            }
            else if (@event is FineIncurredEvent)
            {
                handleFineIncurredEvent((FineIncurredEvent)@event);
            }
            else if (@event is FinePaidEvent)
            {
                handleFinePaidEvent((FinePaidEvent)@event);
            }
            else if (@event is DiedEvent)
            {
                handleDiedEvent((DiedEvent)@event);
            }

        }

        private void handleBondAwardedEvent(BondAwardedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleBondAwardedEvent(@event);
                writeRecord();
            }
        }

        private void _handleBondAwardedEvent(BondAwardedEvent @event)
        {
            int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
            FactionReport report = new FactionReport(@event.timestamp, false, shipId, Crime.None, currentSystem, @event.reward)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.name,
                victim = @event.victimfaction
            };

            FactionRecord record = GetRecordWithFaction(@event.awardingfaction);
            if (record == null)
            {
                record = AddRecord(@event.awardingfaction);
            }
            record.factionReports.Add(report);
            record.claims += @event.reward;
        }

        private void handleBondRedeemedEvent(BondRedeemedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleBondRedeemedEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        private bool _handleBondRedeemedEvent(BondRedeemedEvent @event)
        {
            bool update = false;

            FactionRecord record = new FactionRecord();

            // Calculate amount, broker fees
            decimal percentage = (100 - (@event.brokerpercentage ?? 0)) / 100;
            long amount = Convert.ToInt64(Math.Ceiling(@event.rewards[0].amount / percentage));

            // Handle journal event from Interstellar Factors transaction (FDEV bug)
            if (string.IsNullOrEmpty(@event.rewards[0].faction))
            {
                List<string> systemFactions = EDDI.Instance.CurrentStarSystem?.factions.Select(f => f.name).ToList();

                // Get record which matches a system faction and the bond claims amount
                record = criminalrecord
                    .Where(r => systemFactions.Contains(r.faction))
                    .FirstOrDefault(r => r.bondClaims == amount);
            }
            else
            {
                record = GetRecordWithFaction(@event.rewards[0].faction);
            }

            if (record != null)
            {
                // Get all bond claims, excluding the discrepancy report
                List<FactionReport> reports = record.factionReports
                    .Where(r => !r.bounty && r.crimeDef == Crime.None).ToList();
                if (reports?.Any() ?? false)
                {
                    long total = reports.Sum(r => r.amount);

                    // Check for discrepancy in logged bond claims
                    if (total < amount)
                    {
                        // Adjust the discrepancy report & remove when zeroed out
                        FactionReport report = record.factionReports
                            .FirstOrDefault(r =>  r.crimeDef == Crime.Claim);
                        if (report != null)
                        {
                            report.amount -= Math.Min(amount - total, report.amount);
                            if (report.amount == 0) { reports.Add(report); }
                        }
                    }
                    // Remove associated bonds claims
                    record.factionReports = record.factionReports.Except(reports).ToList();
                }
                // Adjust the total claims
                record.claims -= Math.Min(amount, record.claims);

                RemoveRecord(record);
                update = true;
            }
            return update;
        }

        private void handleBountyAwardedEvent(BountyAwardedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleBountyAwardedEvent(@event);
                writeRecord();
            }
        }

        private void _handleBountyAwardedEvent(BountyAwardedEvent @event)
        {
            // 20% bonus for Arissa Lavigny-Duval 'controlled' and 'exploited' systems
            StarSystem currentSystem = EDDI.Instance?.CurrentStarSystem;
            if (currentSystem != null)
            {
                currentSystem = LegacyEddpService.SetLegacyData(currentSystem, true, false, false);
            }
            double bonus = currentSystem?.power == "Arissa Lavigny-Duval" ? 1.2 : 1.0;

            foreach (Reward reward in @event.rewards.ToList())
            {
                int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
                long amount = Convert.ToInt64(reward.amount * bonus);
                FactionReport report = new FactionReport(@event.timestamp, true, shipId, Crime.None, currentSystem.name, amount)
                {
                    station = EDDI.Instance?.CurrentStation?.name,
                    body = EDDI.Instance?.CurrentStellarBody?.name,
                    victim = @event.faction
                };

                FactionRecord record = GetRecordWithFaction(reward.faction);
                if (record == null)
                {
                    record = AddRecord(reward.faction);
                }
                record.factionReports.Add(report);
                record.claims += amount;
            }
        }

        private void handleBountyRedeemedEvent(BountyRedeemedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleBountyRedeemedEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        private bool _handleBountyRedeemedEvent(BountyRedeemedEvent @event)
        {
            bool update = false;

            foreach (Reward reward in @event.rewards.ToList())
            {
                FactionRecord record = new FactionRecord();

                // Calculate amount, before broker fees
                decimal percentage = (100 - (@event.brokerpercentage ?? 0)) / 100;
                long amount = Convert.ToInt64(Math.Ceiling(reward.amount / percentage));

                // Handle journal event from Interstellar Factors transaction (FDEV bug)
                if (string.IsNullOrEmpty(reward.faction))
                {
                    record = criminalrecord.FirstOrDefault(r => r.bountyClaims == amount);
                }
                else
                {
                    record = GetRecordWithFaction(reward.faction);
                }

                if (record != null)
                {
                    // Get all bounty claims, excluding the discrepancy report
                    List<FactionReport> reports = record.factionReports
                        .Where(r => r.bounty && r.crimeDef == Crime.None).ToList();
                    if (reports?.Any() ?? false)
                    {
                        long total = reports.Sum(r => r.amount);

                        // Check for discrepancy in logged bounty claims
                        if (total < amount)
                        {
                            // Adjust the discrepancy report & remove when zeroed out
                            FactionReport report = record.factionReports
                                .FirstOrDefault(r => r.crimeDef == Crime.Claim);
                            if (report != null)
                            {
                                report.amount -= Math.Min(amount - total, report.amount);
                                if (report.amount == 0) { reports.Add(report); }
                            }
                        }
                        // Remove associated bounty claims
                        record.factionReports = record.factionReports.Except(reports).ToList();
                    }
                    // Adjust the total claims
                    record.claims -= Math.Min(amount, record.claims);

                    RemoveRecord(record);
                    update = true;
                }
            }
            return update;
        }

        private void handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleBountyIncurredEvent(@event);
                writeRecord();
            }
        }

        private void _handleBountyIncurredEvent(BountyIncurredEvent @event)
        {
            int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
            Crime crime = Crime.FromEDName(@event.crimetype);
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
            FactionReport report = new FactionReport(@event.timestamp, true, shipId, crime, currentSystem, @event.bounty)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.name,
                victim = @event.victim
            };

            FactionRecord record = GetRecordWithFaction(@event.faction);
            if (record == null)
            {
                record = AddRecord(@event.faction);
            }
            record.factionReports.Add(report);
            record.bounties += report.amount;
        }

        private void handleBountyPaidEvent(BountyPaidEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleBountyPaidEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        private bool _handleBountyPaidEvent(BountyPaidEvent @event)
        {
            bool update = false;

            foreach (FactionRecord record in criminalrecord.ToList())
            {
                // If paid at 'Legal Facilities', bounties are grouped by superpower
                bool match = @event.brokerpercentage == null ? record.faction == @event.faction : record.Allegiance.invariantName == @event.faction;
                if (@event.allbounties || match)
                {
                    // Get all bounties incurred, excluing the discrepancy report
                    // Note that all bounties are assigned to the ship, not the commander
                    List<FactionReport> reports = record.factionReports
                        .Where(r => r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Bounty && r.shipId == @event.shipid)
                        .ToList();

                    if (reports?.Any() ?? false)
                    {
                        long total = reports.Sum(r => r.amount);

                        // Check for discrepancy in logged bounties incurred
                        if (total < @event.amount)
                        {
                            // Adjust the discrepancy report & remove when zeroed out
                            FactionReport report = record.factionReports
                                .FirstOrDefault(r => r.crimeDef == Crime.Bounty);
                            if (report != null)
                            {
                                report.amount -= Math.Min(@event.amount - total, report.amount);
                                if (report.amount == 0) { reports.Add(report); }
                            }
                        }
                        // Remove associated bounties
                        record.factionReports = record.factionReports.Except(reports).ToList();
                    }
                    // Adjust the total bounties incurred amount
                    record.bounties -= Math.Min(@event.amount, record.bounties);

                    RemoveRecord(record);
                    update = true;
                }
            }
            return update;
        }

        private void handleFineIncurredEvent(FineIncurredEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleFineIncurredEvent(@event);
                writeRecord();
            }
        }

        private void _handleFineIncurredEvent(FineIncurredEvent @event)
        {
            int shipId = EDDI.Instance?.CurrentShip?.LocalId ?? 0;
            Crime crime = Crime.FromEDName(@event.crimetype);
            string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
            FactionReport report = new FactionReport(@event.timestamp, false, shipId, crime, currentSystem, @event.fine)
            {
                station = EDDI.Instance?.CurrentStation?.name,
                body = EDDI.Instance?.CurrentStellarBody?.name,
                victim = @event.victim
            };

            FactionRecord record = GetRecordWithFaction(@event.faction);
            if (record == null)
            {
                record = AddRecord(@event.faction);
            }
            record.factionReports.Add(report);
            record.fines += report.amount;
        }

        private void handleFinePaidEvent(FinePaidEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                if (_handleFinePaidEvent(@event))
                {
                    writeRecord();
                }
            }
        }

        private bool _handleFinePaidEvent(FinePaidEvent @event)
        {
            bool update = false;

            foreach (FactionRecord record in criminalrecord.ToList())
            {
                // If paid at 'Legal Facilities', fines are grouped by superpower
                bool match = @event.brokerpercentage == null ? record.faction == @event.faction : record.Allegiance.invariantName == @event.faction;
                if (@event.allfines || match)
                {
                    // Get all fines incurred, excluing the discrepancy report
                    // Note that all fines are assigned to the ship, not the commander
                    List<FactionReport> reports = record.factionReports
                        .Where(r => !r.bounty && r.crimeDef != Crime.None && r.crimeDef != Crime.Fine && r.shipId == @event.shipid)
                        .ToList();

                    if (reports?.Any() ?? false)
                    {
                        long total = reports.Sum(r => r.amount);

                        // Check for discrepancy in logged fines incurred
                        if (total < @event.amount)
                        {
                            // Adjust the discrepancy report & remove when zeroed out
                            FactionReport report = record.factionReports
                                .FirstOrDefault(r => r.crimeDef == Crime.Fine);
                            if (report != null)
                            {
                                report.amount -= Math.Min(@event.amount - total, report.amount);
                                if (report.amount == 0) { reports.Add(report); }
                            }
                        }
                        // Remove associated fines
                        record.factionReports = record.factionReports.Except(reports).ToList();
                    }
                    // Adjust the total fines incurred amount
                    record.fines -= Math.Min(@event.amount, record.fines);

                    RemoveRecord(record);
                    update = true;
                }
            }
            return update;
        }

        private void handleDiedEvent(DiedEvent @event)
        {
            if (@event.timestamp > updateDat)
            {
                updateDat = @event.timestamp;
                _handleDiedEvent(@event);
                writeRecord();
            }
        }

        private void _handleDiedEvent(DiedEvent @event)
        {
            List<FactionReport> reports = new List<FactionReport>();

            // Remove all pending claims from criminal record
            foreach (FactionRecord record in criminalrecord.ToList())
            {
                reports = record.factionReports
                    .Where(r => r.crimeDef == Crime.None || r.crimeDef == Crime.Claim)
                    .ToList();
                if (reports != null)
                {
                    // Remove all pending claims from faction
                    record.factionReports = record.factionReports.Except(reports).ToList();
                }
                record.claims = 0;
            }
        }

        public IDictionary<string, object> GetVariables()
        {
            IDictionary<string, object> variables = new Dictionary<string, object>
            {
                ["criminalrecord"] = new List<FactionRecord>(criminalrecord),
                ["claims"] = claims,
                ["fines"] = fines,
                ["bounties"] = bounties
            };
            return variables;
        }

        public void writeRecord()
        {
            lock (recordLock)
            {
                // Write criminal configuration with current criminal record
                claims = criminalrecord.Sum(r => r.claims);
                fines = criminalrecord.Sum(r => r.fines);
                bounties = criminalrecord.Sum(r => r.bounties);
                CrimeMonitorConfiguration configuration = new CrimeMonitorConfiguration()
                {
                    criminalrecord = criminalrecord,
                    claims = claims,
                    fines = fines,
                    bounties = bounties,
                    homeSystems = homeSystems,
                    profitShare = profitShare,
                    updatedat = updateDat
                };
                configuration.ToFile();
            }
            // Make sure the UI is up to date
            RaiseOnUIThread(RecordUpdatedEvent, criminalrecord);
        }

        public void readRecord(CrimeMonitorConfiguration configuration = null)
        {
            lock (recordLock)
            {
                // Obtain current criminal record from configuration
                configuration = configuration ?? CrimeMonitorConfiguration.FromFile();
                claims = configuration.claims;
                fines = configuration.fines;
                bounties = configuration.bounties;
                profitShare = configuration.profitShare;
                homeSystems = configuration.homeSystems;
                updateDat = configuration.updatedat;

                // Build a new criminal record
                List<FactionRecord> records = configuration.criminalrecord.OrderBy(c => c.faction).ToList();
                criminalrecord.Clear();
                foreach (FactionRecord record in records)
                {
                    criminalrecord.Add(record);
                }
            }
        }

        private FactionRecord AddRecord(string faction)
        {
            if (faction == null) { return null; }

            FactionRecord record = new FactionRecord(faction);
            GetFactionData(record);

            lock (recordLock)
            {
                criminalrecord.Add(record);
            }
            return record;
        }

        private void RemoveRecord(FactionRecord record)
        {
            // Check if claims or crimes are pending
            if ((record.factionReports?.Any() ?? false)) { return; }
            {
                _RemoveRecord(record);
            }
        }

        public void _RemoveRecord(FactionRecord record)
        {
            string faction = record.faction.ToLowerInvariant();
            lock (recordLock)
            {
                for (int i = 0; i < criminalrecord.Count; i++)
                {
                    if (criminalrecord[i].faction.ToLowerInvariant() == faction)
                    {
                        criminalrecord.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        public FactionRecord GetRecordWithFaction(string faction)
        {
            if (faction == null)
            {
                return null;
            }
            return criminalrecord.FirstOrDefault(c => c.faction.ToLowerInvariant() == faction.ToLowerInvariant());
        }

        public StarSystem GetFactionSystem(string faction, int sphereLy = 20)
        {
            if (faction == null) { return null; }

            string currentSystem = EDDI.Instance?.CurrentStarSystem?.name;
            List<Dictionary<string, object>> sphereSystems = StarMapService.GetStarMapSystemsSphere(currentSystem, 0, sphereLy);

            SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
            foreach (Dictionary<string, object> dict in sphereSystems.ToList())
            {
                decimal? dist = dict["distance"] as decimal?;
                StarSystem system = dict["system"] as StarSystem;
                if (dist != null && system.Faction.name == faction)
                {
                    nearestList.Add(dist ?? 0, system.name);
                }
            }
            string nearestSystem = nearestList.Values.FirstOrDefault();
            StarSystem factionSystem = StarSystemSqLiteRepository.Instance.GetOrCreateStarSystem(nearestSystem, true);
            return factionSystem;
        }

        public void GetFactionData(FactionRecord record, string homeSystem = null)
        {
            if (record == null || record.faction == null || record.faction == Properties.CrimeMonitor.blank_faction) { return; }
            record.station = null;

            // Get the faction from Elite BGS and set faction record values
            Faction faction = DataProviderService.GetFactionByName(record.faction);
            if (faction.EDDBID == null)
            {
                record.faction = Properties.CrimeMonitor.blank_faction;
                record.system = null;
                return;
            }
            record.Allegiance = faction.Allegiance ?? Superpower.None;
            List<string> factionSystems = faction.presences.Select(p => p.systemName).ToList();
            record.factionSystems = factionSystems;

            // 'Home system' is not designated
            if (homeSystem == null)
            {
                // Look first in saved home systems
                if (homeSystems.TryGetValue(record.faction, out string result))
                {
                    homeSystem = result;
                }
                // Use sytem which is part of faction name. Otherwise, system with highest influence
                else
                {
                    List<FactionPresence> presences = faction.presences.Where(p => factionSystems.Contains(p.systemName)).ToList();
                    homeSystem = FindHomeSystem(record.faction, factionSystems)
                        ?? presences.OrderByDescending(p => p.influence).First().systemName;
                }
            }
            // If 'home system' is desiginated, check if system is part of faction presence
            else if (factionSystems.Contains(homeSystem))
            {
                if (FindHomeSystem(record.faction, factionSystems) == null && !homeSystems.ContainsKey(record.faction))
                {
                    // Save home system if not part of faction name and not previously saved
                    homeSystems.Add(record.faction, homeSystem);
                }
            }
            // System not found, exit.
            else { return; }

            StarSystem factionStarSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(homeSystem);
            record.system = homeSystem;

            if (factionStarSystem != null)
            {
                // Filter stations within the faction system which meet the game version and landing pad size requirements
                 string shipSize = EDDI.Instance?.CurrentShip?.size ?? "Large";
                List<Station> factionStations = EDDI.Instance.inHorizons ? factionStarSystem.stations : factionStarSystem.orbitalstations
                    .Where(s => s.LandingPadCheck(shipSize)).ToList();

                // Prioritize controlled stations
                List<Station> controlledStations = factionStations.Where(s => s.Faction.name == record.faction).ToList();
                if (controlledStations.Count > 0 && controlledStations.Min(s => s.distancefromstar) < 10000)
                {
                    factionStations = controlledStations;
                }

                // Build list to find the faction station nearest to the main star
                SortedList<decimal, string> nearestList = new SortedList<decimal, string>();
                foreach (Station station in factionStations)
                {
                    if (!nearestList.ContainsKey(station.distancefromstar ?? 0))
                    {
                        nearestList.Add(station.distancefromstar ?? 0, station.name);
                    }
                }

                // Faction station nearest to the main star
                string nearestStation = nearestList.Values.FirstOrDefault();
                record.station = nearestStation;
            }
        }

        private string FindHomeSystem(string faction, List<string> factionSystems)
        {
            // Look for system which is part of faction name
            foreach (string system in factionSystems)
            {
                string pattern = @"\b" + Regex.Escape(system) + @"\b";
                if (Regex.IsMatch(faction, pattern)) { return system; }
            }
            return null;
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
