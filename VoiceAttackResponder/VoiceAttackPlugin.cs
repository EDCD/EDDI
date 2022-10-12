﻿using Eddi;
using EddiCargoMonitor;
using EddiCompanionAppService;
using EddiCore;
using EddiDataDefinitions;
using EddiDataProviderService;
using EddiEvents;
using EddiNavigationService;
using EddiShipMonitor;
using EddiSpeechResponder;
using EddiSpeechService;
using EddiStarMapService;
using EddiStatusService;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace EddiVoiceAttackResponder
{
    public class VoiceAttackPlugin : VoiceAttackVariables
    {
        public static string VA_DisplayName()
        {
            return Constants.EDDI_NAME + " " + Constants.EDDI_VERSION;
        }

        public static string VA_DisplayInfo()
        {
            return Constants.EDDI_NAME + "\r\nVersion " + Constants.EDDI_VERSION;
        }

        public static Guid VA_Id()
        {
            return new Guid("{4AD8E3A4-CEFA-4558-B503-1CC9B99A07C1}");
        }

        private static readonly Random random = new Random();

        public static readonly ConcurrentDictionary<string, BlockingCollection<Event>> eventQueues = new ConcurrentDictionary<string, BlockingCollection<Event>>();
        private static readonly ConcurrentBag<Task> consumerTasks = new ConcurrentBag<Task>();
        private static readonly CancellationTokenSource consumerCancellationTS = new CancellationTokenSource(); // This must be static so that it is visible to child threads and tasks

        // We'll maintain a referenceable list of variables that we've set from events
        private static List<VoiceAttackVariable> currentVariables = new List<VoiceAttackVariable>();

        private static System.Version vaVersion;
        private static readonly object vaProxyLock = new object();

        private static Status currentStatus;

        public static void VA_Init1(dynamic vaProxy)
        {
            // Initialize and launch an EDDI instance without opening the main window
            // VoiceAttack commands will be used to manipulate the window state.
            App.vaProxy = vaProxy;
            if (App.AlreadyRunning()) { return; }

            Thread appThread = new Thread(App.Main);
            appThread.SetApartmentState(ApartmentState.STA);
            appThread.Start();

            try
            {
                int timeout = 0;
                while (Application.Current == null)
                {
                    if (timeout < 200)
                    {
                        Thread.Sleep(50);
                        timeout++;
                    }
                    else
                    {
                        throw new TimeoutException("EDDI VoiceAttack plugin initialisation has timed out");
                    }
                }

                Logging.Info("Initialising EDDI VoiceAttack plugin");

                // Set up our event responder.
                VoiceAttackResponder.RaiseEvent += (s, theEvent) =>
                {
                    if (theEvent is null) { return; }
                    if (eventQueues.ContainsKey(theEvent.type))
                    {
                        // Add our event to an existing blocking collection for that event type.
                        eventQueues[theEvent.type].Add(theEvent);
                    }
                    else
                    {
                        // Add our event to a new blocking collection for that event type and start a consumer task for that collection
                        eventQueues[theEvent.type] = new BlockingCollection<Event> { theEvent };
                        var consumerTask = Task.Run(() =>
                        {
                            // ReSharper disable once AccessToModifiedClosure - OK to use vaProxy in this context.
                            dequeueEvents(eventQueues[theEvent.type], ref vaProxy);
                        });
                        consumerTasks.Add(consumerTask);
                    }
                };

                // Add notifiers for changes in variables we want to react to 
                // (we can only use event handlers with classes which are always constructed - nullable objects will be updated via responder events)
                EDDI.Instance.PropertyChanged += (s, e) => updateStandardValues(e);
                EDDI.Instance.State.CollectionChanged += (s, e) =>
                {
                    setDictionaryValues(EDDI.Instance.State, "state", ref vaProxy);
                };
                SpeechService.Instance.PropertyChanged += (s, e) =>
                {
                    setSpeechState(e);
                };
                CompanionAppService.Instance.StateChanged += (oldState, newState) =>
                {
                    setCAPIState(newState == CompanionAppService.State.Authorized, ref vaProxy);
                };
                StatusService.StatusUpdatedEvent += OnStatusUpdated;

                CargoMonitor cargoMonitor = (CargoMonitor)EDDI.Instance.ObtainMonitor("Cargo monitor");
                cargoMonitor.InventoryUpdatedEvent += (s, e) =>
                {
                    lock (vaProxyLock)
                    {
                        setCargo(cargoMonitor, ref vaProxy);
                    }
                };

                ShipMonitor shipMonitor = (ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor");
                if (shipMonitor != null)
                {
                    shipMonitor.ShipyardUpdatedEvent += (s, e) =>
                    {
                        lock (vaProxyLock)
                        {
                            setShipValues(shipMonitor.GetCurrentShip(), "Ship", ref vaProxy);
                            Task.Run(() => setShipyardValues(shipMonitor.shipyard?.Copy().ToList(), ref vaProxy));
                        }
                    };
                }

                StatusService.StatusUpdatedEvent += (s, e) =>
                {
                    if (s is Status status)
                    {
                        lock (vaProxyLock)
                        {
                            setStatusValues(status, "Status", ref vaProxy);
                        }
                    }
                };

                // Set initial values for standard variables
                initializeStandardValues();

                // Display instance information if available
                if (EddiUpgrader.UpgradeRequired)
                {
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "red");
                    string msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.Say(null, msg, 0);
                }
                else if (EddiUpgrader.UpgradeAvailable)
                {
                    vaProxy.WriteToLog("Please shut down VoiceAttack and run EDDI standalone to upgrade", "orange");
                    string msg = Properties.VoiceAttack.run_eddi_standalone;
                    SpeechService.Instance.Say(null, msg, 0);
                }

                if (EddiUpgrader.Motd != null)
                {
                    vaProxy.WriteToLog("Message from EDDI: " + EddiUpgrader.Motd, "black");
                    string msg = String.Format(Eddi.Properties.EddiResources.msg_from_eddi, EddiUpgrader.Motd);
                    SpeechService.Instance.Say(null, msg, 0);
                }

                vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                setStatus(ref vaProxy, "Operational");

                // Fire an event once the VA plugin is initialized
                EDDI.Instance.enqueueEvent(new VAInitializedEvent(DateTime.UtcNow));

                // Set a variable indicating the version of VoiceAttack in use
                vaVersion = vaProxy.VAVersion;
                EDDI.Instance.vaVersion = vaVersion.ToString();

                Logging.Info("EDDI VoiceAttack plugin initialization complete");
            }
            catch (Exception e)
            {
                Logging.Error("Failed to initialize VoiceAttack plugin", e);
                vaProxy.WriteToLog("Unable to fully initialize EDDI. Some functions may not work.", "red");
            }
        }

        private static void OnStatusUpdated(object sender, EventArgs e)
        {
            if (sender is Status status)
            {
                LockManager.GetLock(nameof(currentStatus), () =>
                {
                    currentStatus = status;
                });
            }
        }

        private static void dequeueEvents(BlockingCollection<Event> eventQueue, ref dynamic vaProxy)
        {
            try
            {
                foreach (var @event in eventQueue.GetConsumingEnumerable(consumerCancellationTS.Token))
                {
                    try
                    {
                        if (@event?.type != null)
                        {
                            lock (vaProxyLock)
                            {
                                updateValuesOnEvent(@event, ref vaProxy);
                                triggerVACommands(@event, ref vaProxy);
                            }
                            // We need to wait until each event is no longer active before moving to the next from the same
                            // queue / event type so that variables aren't overwritten before VoiceAttack can respond.
                            // Other queues / event types will be able to continue processing events while we wait.
                            var active = true;
                            while (active)
                            {
                                Thread.Sleep(50);
                                if (vaVersion.CompareTo(new System.Version(1, 7, 4)) > 0) // If running VoiceAttack version 1.7.4 or later
                                {
                                    active = vaProxy.Command.Active("((EDDI " + @event.type.ToLowerInvariant() + "))");
                                }
                                else // Legacy command invocation for versions of VoiceAttack prior to 1.7.4
                                {
                                    active = vaProxy.CommandActive("((EDDI " + @event.type.ToLowerInvariant() + "))");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.Data.Add("event", JsonConvert.SerializeObject(@event));
                        Logging.Error("VoiceAttack failed to handle event.", ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Task canceled. Mark this collection as not accepting any new items.
                eventQueue.CompleteAdding();
            }
        }

        public static void updateValuesOnEvent(Event @event, ref dynamic vaProxy)
        {
            try
            {
                Logging.Debug($"Processing EDDI event {@event.type}:", @event);
                var startTime = DateTime.UtcNow;
                vaProxy.SetText("EDDI event", @event.type);

                // Retrieve and clear variables from prior iterations of the same event
                clearPriorEventValues(ref vaProxy, @event.type, currentVariables);
                currentVariables = currentVariables.Where(v => v.eventType != @event.type).ToList();

                // Prepare and update this event's variable values
                var eventVariables = new MetaVariables(@event.GetType(), @event)
                    .Results
                    .AsVoiceAttackVariables("EDDI", @event.type);
                foreach (var @var in eventVariables) { @var.Set(vaProxy); }

                // Save the updated state of our event variables
                currentVariables.AddRange(eventVariables);

                Logging.Debug($"Processed EDDI event {@event.type} in {(DateTime.UtcNow - startTime).Milliseconds} milliseconds:", @event);
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to set event variables in VoiceAttack", ex);
            }
        }

        public static void clearPriorEventValues(ref dynamic vaProxy, string eventType, List<VoiceAttackVariable> eventVariables)
        {
            try
            {
                // We set all values in our list from a prior version of the same event to null
                foreach (var variable in eventVariables
                    .Where(v => v.eventType == eventType && v.value != null))
                {
                    variable.value = null;
                }
                // We clear variable values by swapping the values to null and then instructing VA to set them again
                foreach (var @var in eventVariables) { @var.Set(vaProxy); }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to clear event variables in VoiceAttack", ex);
            }
        }

        private static void triggerVACommands(Event @event, ref dynamic vaProxy)
        {
            string commandName = "((EDDI " + @event.type.ToLowerInvariant() + "))";
            try
            {
                // Fire local command if present  
                Logging.Debug("Searching for command " + commandName);
                if (vaVersion.CompareTo(new System.Version(1,7,4)) > 0) // If running VoiceAttack version 1.7.4 or later
                {
                    if (vaProxy.Command.Exists(commandName))
                    {
                        Logging.Debug("Found command " + commandName);
                        vaProxy.Command.Execute(commandName);
                        Logging.Info("Executed command " + commandName);
                    }
                }
                else // Legacy command invocation for versions of VoiceAttack prior to 1.7.4
                {
                    if (vaProxy.CommandExists(commandName))
                    {
                        Logging.Debug("Found command " + commandName);
                        vaProxy.ExecuteCommand(commandName);
                        Logging.Info("Executed command " + commandName);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to trigger local VoiceAttack command " + commandName, ex);
            }
        }

        // ReSharper disable once UnusedMember.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "VA API")]
        public static void VA_Exit1(dynamic vaProxy)
        {
            Logging.Info("EDDI VoiceAttack plugin exiting");

            // Cancel the updater threads and wait for them to complete
            consumerCancellationTS?.Cancel();
            var timeout = Task.Delay(2000);
            Task.WhenAny(Task.WhenAll(consumerTasks), timeout);

            if (Application.Current?.Dispatcher != null)
            {
                try
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Application.Current.Exit += OnExit;
                        Application.Current.MainWindow?.Close();
                        Application.Current.Shutdown();
                    });
                }
                catch (Exception ex)
                {
                    Logging.Debug("EDDI configuration UI close from VA failed." + ex + ".");
                }
            }
        }

        private static void OnExit(object sender, ExitEventArgs e)
        {
            if (!App.eddiMutex.SafeWaitHandle.IsClosed)
            {
                App.eddiMutex.ReleaseMutex();
            }
        }

        public static void VA_StopCommand()
        {
        }

        public static void VA_Invoke1(dynamic vaProxy)
        {
            Logging.Debug("Invoked with context " + (string)vaProxy.Context);

            try
            {
                switch ((string)vaProxy.Context)
                {
                    case "coriolis":
                        InvokeCoriolis(ref vaProxy);
                        break;
                    case "eddbsystem":
                        InvokeEDDBSystem(ref vaProxy);
                        break;
                    case "eddbstation":
                        InvokeEDDBStation(ref vaProxy);
                        break;
                    case "edshipyard":
                        InvokeEDShipyard(ref vaProxy);
                        break;
                    case "profile":
                        InvokeUpdateProfile(ref vaProxy);
                        break;
                    case "say":
                        InvokeSay(ref vaProxy);
                        break;
                    case "speech":
                        InvokeSpeech(ref vaProxy);
                        break;
                    case "system comment":
                        InvokeStarMapSystemComment(ref vaProxy);
                        break;
                    case "initialize eddi":
                        if (App.FromVA && Application.Current != null)
                        {
                            vaProxy.WriteToLog("The EDDI plugin is fully operational.", "green");
                        }
                        else
                        {
                            VA_Init1(vaProxy);  // Attempt initialization again to see if it works this time...
                        }
                        break;
                    case "configuration":
                    case "configurationminimize":
                    case "configurationmaximize":
                    case "configurationrestore":
                    case "configurationclose":
                        // Ignore any attempt to access the EDDI UI if VA
                        // doesn't own the EDDI instance.
                        if (App.FromVA && Application.Current != null)
                        {
                            InvokeConfiguration(ref vaProxy);
                        }
                        else
                        {
                            vaProxy.WriteToLog("The EDDI plugin is not fully initialized.", "red");
                        }
                        break;
                    case "shutup":
                        InvokeShutUp(ref vaProxy);
                        break;
                    case "setstate":
                        InvokeSetState(ref vaProxy);
                        break;
                    case "disablespeechresponder":
                        InvokeDisableSpeechResponder(ref vaProxy);
                        break;
                    case "enablespeechresponder":
                        InvokeEnableSpeechResponder(ref vaProxy);
                        break;
                    case "setspeechresponderpersonality":
                        InvokeSetSpeechResponderPersonality(ref vaProxy);
                        break;
                    case "jumpdetails":
                        InvokeJumpDetails(ref vaProxy);
                        break;
                    case "transmit":
                        InvokeTransmit(ref vaProxy);
                        break;
                    case "missionsroute":
                    case "route":
                        InvokeRouteDetails(ref vaProxy);
                        break;
                    case "inara":
                        InvokeInaraProfileDetails(ref vaProxy);
                        break;
                    case "volume":
                        InvokeVolume(ref vaProxy);
                        break;
                }
            }
            catch (Exception e)
            {
                Logging.Error("Failed to invoke context " + vaProxy.Context, e);
                vaProxy.WriteToLog("Failed to invoke context " + vaProxy.Context, "red");
            }
        }

        private static void InvokeVolume(ref dynamic vaProxy)
        {
            int? volumeInt = vaProxy.GetInt("Volume");

            if (SpeechService.Instance.Configuration == null) { return; }

            // Fix any inputs outside of the expected range
            if (volumeInt == null) { volumeInt = new SpeechServiceConfiguration().Volume; } // Default volume
            else if (volumeInt < 0) { volumeInt = 0; } // Must be zero or greater
            else if (volumeInt > 100) { volumeInt = 100; } // Must be 100 or less

            SpeechService.Instance.Configuration.Volume = (int)volumeInt;
            SpeechService.Instance.Configuration.ToFile();
            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow)?.ConfigureTTS();
            });
        }

        private static void InvokeInaraProfileDetails(ref dynamic vaProxy)
        {
            string commanderName = vaProxy.GetText("Name");
            if (commanderName == null)
            {
                return;
            }
            try
            {
                EddiInaraService.IInaraService inaraService = new EddiInaraService.InaraService();
                var result = inaraService.GetCommanderProfile(commanderName);
                if (result != null)
                {
                    OpenOrStoreURI(ref vaProxy, result.url);
                }
                else
                {
                    Logging.Debug("No information on commander " + commanderName);
                }
            }
            catch (Exception ex)
            {
                Logging.Warn("Failed to obtain Inara details on commander " + commanderName, ex);
            }
        }

        private static void InvokeConfiguration(ref dynamic vaProxy)
        {
            string config = (string)vaProxy.Context;

            if (Application.Current?.Dispatcher != null
                && (bool)Application.Current?.Dispatcher?.Invoke(() => Application.Current.MainWindow == null)
                && config != "configuration")
            {
                vaProxy.WriteToLog("The EDDI configuration window is not open.", "orange");
                return;
            }

            switch (config)
            {
                case "configuration":
                    if (Application.Current?.Dispatcher != null)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            try
                            {
                                if (Application.Current?.MainWindow?.Visibility == Visibility.Collapsed
                                    || Application.Current?.MainWindow?.Visibility == Visibility.Hidden)
                                {
                                    Application.Current.MainWindow?.Show();
                                }
                                else
                                {
                                    // Tell the configuration UI to restore its window if minimized
                                    setWindowState(ref App.vaProxy, WindowState.Minimized, true, false);
                                    App.vaProxy.WriteToLog("The EDDI configuration window is already open.", "orange");
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Warn("Show configuration window failed", ex);
                            }
                        });
                    }
                    break;
                case "configurationminimize":
                    setWindowState(ref vaProxy, WindowState.Minimized);
                    break;
                case "configurationmaximize":
                    setWindowState(ref vaProxy, WindowState.Maximized);
                    break;
                case "configurationrestore":
                    setWindowState(ref vaProxy, WindowState.Normal);
                    break;
                case "configurationclose":
                    Application.Current?.Dispatcher?.Invoke(() => Application.Current?.MainWindow?.Hide());
                    break;
                default:
                    vaProxy.WriteToLog("Plugin context \"" + (string)vaProxy.Context + "\" not recognized.", "orange");
                    break;
            }
        }

        // Set main window minimize, maximize and normal states. Ignore and warn
        // if the main window is blocked waiting for a modal dialog to close.
        private static void setWindowState(ref dynamic vaProxy, WindowState newState, bool minimizeCheck = false, bool warn = true)
        {
            if (EDDI.Instance.SpeechResponderModalWait && warn)
            {
                System.Media.SystemSounds.Beep.Play();
                vaProxy.WriteToLog("The EDDI window state cannot be changed at this time.", "orange");
            }
            else
            {
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    MainWindow mainwindow = (MainWindow)Application.Current?.MainWindow;
                    mainwindow?.Dispatcher?.Invoke(mainwindow.VaWindowStateChange, newState, minimizeCheck);
                });
            }
        }

        /// <summary>Force-update EDDI's information</summary>
        private static void InvokeUpdateProfile(ref dynamic vaProxy)
        {
            EDDI.Instance.refreshProfile(true);
        }

        private static void OpenOrStoreURI(ref dynamic vaProxy, string systemUri)
        {
            if (vaProxy.GetBoolean("EDDI open uri in browser") != false)
            {
                Logging.Debug("Starting process with uri " + systemUri);
                HandleUri(ref vaProxy, systemUri);
            }
            Logging.Debug("Writing URI to `{TXT:EDDI uri}`: " + systemUri);
            vaProxy.SetText("EDDI uri", systemUri);
        }

        public static void InvokeEDDBSystem(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (EDDI.Instance.CurrentStarSystem == null)
                {
                    Logging.Debug("No information on current system");
                    return;
                }
                string systemUri = "https://eddb.io/system/ed-address/" + EDDI.Instance.CurrentStarSystem.systemAddress;
                OpenOrStoreURI(ref vaProxy, systemUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send system data to EDDB", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeEDDBStation(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (EDDI.Instance.CurrentStarSystem == null)
                {
                    Logging.Debug("No information on current station");
                    return;
                }
                Station thisStation = EDDI.Instance.CurrentStarSystem.stations.SingleOrDefault(s => s.name == (EDDI.Instance.CurrentStation?.name));
                if (thisStation == null)
                {
                    // Missing current star system information
                    Logging.Debug("No information on current station");
                    return;
                }
                string stationUri = "https://eddb.io/station/market-id/" + thisStation.marketId;
                OpenOrStoreURI(ref vaProxy, stationUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send station data to EDDB", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeCoriolis(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip() == null)
                {
                    Logging.Debug("No information on ship");
                    return;
                }

                string shipUri = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip().CoriolisUri();
                OpenOrStoreURI(ref vaProxy, shipUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send ship data to coriolis", e);
            }
            Logging.Debug("Leaving");
        }

        public static void InvokeEDShipyard(ref dynamic vaProxy)
        {
            Logging.Debug("Entered");
            try
            {
                if (((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip() == null)
                {
                    Logging.Debug("No information on ship");
                    return;
                }

                string shipUri = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip().EDShipyardUri();
                OpenOrStoreURI(ref vaProxy, shipUri);
                setStatus(ref vaProxy, "Operational");
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to send ship data to coriolis", e);
            }
            Logging.Debug("Leaving");
        }

        /// <summary>
        /// Handle a URI, either sending it to the default web browser or putting it on the clipboard
        /// </summary>
        private static void HandleUri(ref dynamic vaProxy, string uri)
        {
            bool? useClipboard = vaProxy.GetBoolean("EDDI use clipboard");
            if (useClipboard != null && useClipboard == true)
            {
                Thread thread = new Thread(() => Clipboard.SetText(uri));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }
            else
            {
                ProcessStartInfo proc = new ProcessStartInfo(Net.GetDefaultBrowserPath(), "\"" + uri + "\"")
                {
                    UseShellExecute = false
                };
                Process.Start(proc);
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSay(ref dynamic vaProxy)
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if (script == null)
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority") ?? 3;

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                Ship ship = null;
                if(EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                { 
                    ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
                }

                SpeechService.Instance.Say(ship, speech, (int)priority, voice, false, null, true);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to run EDDI's internal speech system (say)", e);
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary> 
        public static void InvokeTransmit(ref dynamic vaProxy)
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if (script == null)
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority") ?? 3;

                string voice = vaProxy.GetText("Voice");

                string speech = SpeechFromScript(script);

                Ship ship = null;
                if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                {
                    ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
                }

                SpeechService.Instance.Say(ship, speech, (int)priority, voice, true, null, true);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to run EDDI's internal speech system (transmit)", e);
            }
        }

        /// <summary>
        /// Stop talking
        /// </summary>
        public static void InvokeShutUp(ref dynamic vaProxy)
        {
            try
            {
                SpeechService.Instance.ShutUp();
                SpeechService.Instance.StopAudio();
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to shut up", e);
            }
        }

        /// <summary>Say something inside the cockpit with text-to-speech</summary>
        public static void InvokeSpeech(ref dynamic vaProxy)
        {
            try
            {
                string script = vaProxy.GetText("Script");
                if (script == null)
                {
                    return;
                }

                int? priority = vaProxy.GetInt("Priority");

                string voice = vaProxy.GetText("Voice");

                SpeechResponder speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                if (speechResponder == null)
                {
                    Logging.Warn("Unable to find speech responder");
                }

                Ship ship = null;
                if (EDDI.Instance.Vehicle == Constants.VEHICLE_SHIP)
                {
                    ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
                }

                // sayOutLoud must be true to match the behavior described by the wiki for the `disablespeechresponder` command
                // i.e. "not talk unless specifically asked for information"
                speechResponder?.Say(ship, script, null, priority, voice, true, true);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to run internal speech system", e);
            }
        }

        public static void InvokeDisableSpeechResponder(ref dynamic vaProxy)
        {
            try
            {
                EDDI.Instance.State["speechresponder_quiet"] = true;
                SpeechService.Instance.speechQueue.DequeueAllSpeech();
                SpeechService.Instance.StopCurrentSpeech();
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to disable speech responder", e);
            }
        }

        public static void InvokeEnableSpeechResponder(ref dynamic vaProxy)
        {
            try
            {
                EDDI.Instance.State["speechresponder_quiet"] = false;
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to enable speech responder", e);
            }
        }

        public static void InvokeSetSpeechResponderPersonality(ref dynamic vaProxy)
        {
            string personality = vaProxy.GetText("Personality");
            try
            {
                SpeechResponder speechResponder = (SpeechResponder)EDDI.Instance.ObtainResponder("Speech responder");
                speechResponder?.SetPersonality(personality);
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set speech responder personality", e);
            }
        }

        public static void InvokeSetState(ref dynamic vaProxy)
        {
            try
            {
                string name = vaProxy.GetText("State variable");
                if (string.IsNullOrEmpty(name))
                {
                    Logging.Info("No value in the VoiceAttack text variable 'State variable'; nothing to set");
                    return;
                }

                // State variable names are lower-case
                string stateVariableName = name.ToLowerInvariant().Replace(" ", "_");

                string strValue = vaProxy.GetText(name);
                if (strValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = strValue;
                    return;
                }

                int? intValue = vaProxy.GetInt(name);
                if (intValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = intValue;
                    return;
                }

                bool? boolValue = vaProxy.GetBoolean(name);
                if (boolValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = boolValue;
                    return;
                }

                decimal? decValue = vaProxy.GetDecimal(name);
                if (decValue != null)
                {
                    EDDI.Instance.State[stateVariableName] = decValue;
                    return;
                }

                // Nothing above, so set the item to null
                EDDI.Instance.State[stateVariableName] = null;
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to set state", e);
            }
        }

        public static string SpeechFromScript(string script)
        {
            if (script == null) { return null; }

            // Variable replacement
            Ship ship = ((ShipMonitor)EDDI.Instance.ObtainMonitor("Ship monitor")).GetCurrentShip();
            if (ship != null)
            {
                script = script.Replace("$=", ship.phoneticname);
            }

            string cmdrScript;
            if (string.IsNullOrEmpty(EDDI.Instance.Cmdr?.name))
            {
                cmdrScript = Eddi.Properties.EddiResources.Commander;
            }
            else
            {
                cmdrScript = EDDI.Instance.Cmdr.phoneticname;
            }
            script = script.Replace("$-", cmdrScript);

            // Multiple choice selection
            StringBuilder sb = new StringBuilder();

            // Step 1 - resolve any options in square brackets
            Match matchResult = Regex.Match(script, @"\[[^\]]*\]|[^\[\]]+");
            while (matchResult.Success)
            {
                if (matchResult.Value.StartsWith("["))
                {
                    // Remove the brackets and pick one of the options
                    string result = matchResult.Value.Substring(1, matchResult.Value.Length - 2);
                    string[] options = result.Split(';');
                    sb.Append(options[random.Next(0, options.Length)]);
                }
                else
                {
                    // Pass it right along
                    sb.Append(matchResult.Groups[0].Value);
                }
                matchResult = matchResult.NextMatch();
            }
            string res = sb.ToString();

            // Step 2 - resolve phrases separated by semicolons
            if (res.Contains(";"))
            {
                // Pick one of the options
                string[] options = res.Split(';');
                res = options[random.Next(0, options.Length)];
            }

            // Step 3 - pass it through the script resolver
            res = new EddiSpeechResponder.Service.ScriptResolver(null).resolveFromValue(res, true);

            return res ?? "";
        }

        /// <summary>
        /// Send a comment to the starmap service and store locally
        /// </summary>
        public static void InvokeStarMapSystemComment(ref dynamic vaProxy)
        {
            try
            {
                string comment = vaProxy.GetText("EDDI system comment");
                if (comment == null)
                {
                    return;
                }

                if (EDDI.Instance.CurrentStarSystem != null)
                {
                    // Store locally
                    string currentSystemName = EDDI.Instance.CurrentStarSystem.systemname;
                    StarSystem currentSystem = StarSystemSqLiteRepository.Instance.GetOrFetchStarSystem(currentSystemName);
                    currentSystem.comment = comment == "" ? null : comment;
                    StarSystemSqLiteRepository.Instance.SaveStarSystem(currentSystem);

                    // Store in EDSM
                    IEdsmService edsmService = new StarMapService();
                    edsmService.sendStarMapComment(currentSystemName, comment);
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to store system comment", e);
            }
        }

        public static void InvokeJumpDetails(ref dynamic vaProxy)
        {
            try
            {
                string type = vaProxy.GetText("Type variable");
                if (!string.IsNullOrEmpty(type))
                {
                    var detail = EDDI.Instance.CurrentShip?.JumpDetails(type);
                    vaProxy.SetDecimal("Ship jump detail distance", detail?.distance);
                    vaProxy.SetInt("Ship jump detail jumps", detail?.jumps);
                    vaProxy.SetText("Type variable", null);
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to get jump details", e);
            }
        }

        public static void InvokeRouteDetails(ref dynamic vaProxy)
        {
            try
            {
                string type = vaProxy.GetText("Type variable");
                string string0 = vaProxy.GetText("System variable");
                string string1 = vaProxy.GetText("System variable 2") ?? vaProxy.GetText("Station variable");
                decimal? numeric = vaProxy.GetDecimal("Numeric variable");

                vaProxy.SetText("Type variable", null);
                vaProxy.SetText("System variable", null);
                vaProxy.SetText("System variable 2", null);
                vaProxy.SetText("Station variable", null);
                vaProxy.SetDecimal("Numeric variable", null);

                if (Enum.TryParse(type, true, out QueryType result))
                {
                    var @event = NavigationService.Instance.NavQuery(result, string0, string1, numeric);
                    if (@event != null)
                    {
                        EDDI.Instance?.enqueueEvent(@event);
                    }
                }
                else
                {
                    Logging.Warn($"The search query '{type}' is unrecognized.");
                }
            }
            catch (Exception e)
            {
                setStatus(ref vaProxy, "Failed to get route", e);
            }
        }
    }
}
