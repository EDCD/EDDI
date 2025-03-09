using CommanderMonitor;
using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using Utilities;

[assembly: InternalsVisibleTo( "Tests" )]
namespace EddiCommanderMonitor
{
    [UsedImplicitly]
    public class CommanderMonitor : IEddiMonitor, INotifyPropertyChanged
    {
        internal Commander Cmdr => EDDI.Instance.Cmdr;
        private static readonly object commanderLock = new object();

        [CanBeNull]
        public StarSystem SquadronStarSystem // May be null when the commander hasn't set a squadron star system
        {
            get => squadronStarSystem;
            private set
            {
                void childPropertyChangedHandler ( object sender, PropertyChangedEventArgs e )
                {
                    OnPropertyChanged();
                }
                if ( squadronStarSystem != null ) { squadronStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if ( value != null ) { value.PropertyChanged += childPropertyChangedHandler; }
                squadronStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem squadronStarSystem;

        private DateTime JournalTimeStamp { get; set; } = DateTime.MinValue;

        private DateTime updatedAt { get; set; } = DateTime.MinValue;

        public string MonitorName () => "Commander monitor";

        public string LocalizedMonitorName () => Properties.Resources.MonitorName;

        public string MonitorDescription () => Properties.Resources.MonitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start () => Reload();

        public void Stop ()
        {
            WriteCommander();
        }

        public void Reload ()
        {
            ReadCommander();
            Logging.Info( $"Reloaded {MonitorName()}" );
        }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return new Dictionary<string, Tuple<Type, object>>()
            {
                { "squadronsystem", new Tuple<Type, object>( typeof(StarSystem), SquadronStarSystem ) }
            };
        }

        public UserControl ConfigurationTabItem () => ConfigurationWindow.Instance;

        public void PreHandle ( Event @event )
        {
            if ( @event is CarrierBankTransferEvent carrierBankTransferEvent )
            {
                handleCarrierBankTransferEvent( carrierBankTransferEvent );
            }
            else if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is CommanderLoadingEvent commanderLoadingEvent )
            {
                handleCommanderLoadingEvent( commanderLoadingEvent );
            }
            else if ( @event is CommanderPromotionEvent commanderPromotionEvent )
            {
                handleCommanderPromotionEvent( commanderPromotionEvent );
            }
            else if ( @event is CommanderRatingsEvent commanderRatingsEvent )
            {
                handleCommanderRatingsEvent( commanderRatingsEvent );
            }
            else if ( @event is FileHeaderEvent )
            {
                handleFileHeaderEvent();
            }
            else if ( @event is JumpedEvent jumpedEvent )
            {
                handleJumpedEvent( jumpedEvent );
            }
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
            }
            else if ( @event is PowerJoinedEvent powerJoinedEvent )
            {
                handlePowerJoinedEvent( powerJoinedEvent );
            }
            else if ( @event is PowerLeftEvent powerLeftEvent )
            {
                handlePowerLeftEvent( powerLeftEvent );
            }
            else if ( @event is PowerplayEvent powerplayEvent )
            {
                handlePowerPlayEvent( powerplayEvent );
            }
            else if ( @event is PowerVoucherReceivedEvent powerVoucherReceivedEvent )
            {
                handlePowerVoucherReceivedEvent( powerVoucherReceivedEvent );
            }
            else if ( @event is SquadronRankEvent squadronRankEvent )
            {
                handleSquadronRankEvent( squadronRankEvent );
            }
            else if ( @event is SquadronStartupEvent squadronStartupEvent )
            {
                handleSquadronStartupEvent( squadronStartupEvent );
            }
            else if ( @event is SquadronStatusEvent squadronStatusEvent )
            {
                handleSquadronStatusEvent( squadronStatusEvent );
            }

            JournalTimeStamp = @event.timestamp;
        }

        private void handleCarrierBankTransferEvent ( CarrierBankTransferEvent @event )
        {
            if ( @event.timestamp >= updatedAt  )
            {
                Cmdr.credits = @event.cmdrBalance;
                WriteCommander();
            }
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            SetCommanderTitle( @event.controllingsystemfaction.Allegiance );
            if ( ( @event.docked || @event.onFoot ) && @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt && 
                     TryUpdateSquadronHomeSystem( @event.factions, @event.systemAddress ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handleCommanderLoadingEvent ( CommanderLoadingEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                Cmdr.name = @event.name;
                Cmdr.EDID = @event.frontierID;

                WriteCommander();
                
                if ( ConfigService.Instance.commanderConfiguration.commanderName != @event.name )
                {
                    EDDI.Instance.ObtainResponder( "EDSM Responder" ).Reload();
                }
            }
        }

        private void handleCommanderPromotionEvent ( CommanderPromotionEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                // Capture commander ratings and add them to the commander object
                if ( @event.ratingObject is CombatRating combatRating )
                {
                    // There is a bug with the journal where it reports superpower increases in rank as combat increases
                    // Hence we check to see if this is a real event by comparing our known combat rating to the promoted rating
                    if ( Cmdr.combatrating == null || @event.rank != Cmdr.combatrating.localizedName )
                    {
                        // Real event. 
                        Cmdr.combatrating = combatRating;
                    }
                    // False event
                }
                else if ( @event.ratingObject is CQCRating cqcRating )
                {
                    Cmdr.cqcrating = cqcRating;
                }
                else if ( @event.ratingObject is EmpireRating empireRating )
                {
                    Cmdr.empirerating = empireRating;
                }
                else if ( @event.ratingObject is ExplorationRating explorationRating )
                {
                    Cmdr.explorationrating = explorationRating;
                }
                else if ( @event.ratingObject is ExobiologistRating exobiologistRating )
                {
                    Cmdr.exobiologistrating = exobiologistRating;
                }
                else if ( @event.ratingObject is FederationRating federationRating )
                {
                    Cmdr.federationrating = federationRating;
                }
                else if ( @event.ratingObject is MercenaryRating mercenaryRating )
                {
                    Cmdr.mercenaryrating = mercenaryRating;
                }
                else if ( @event.ratingObject is TradeRating tradeRating )
                {
                    // Capture commander ratings and add them to the commander object
                    Cmdr.traderating = tradeRating;
                }

                WriteCommander();
            }
        }

        private void handleCommanderRatingsEvent ( CommanderRatingsEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                Cmdr.combatrating = @event.combat;
                Cmdr.traderating = @event.trade;
                Cmdr.explorationrating = @event.exploration;
                Cmdr.cqcrating = @event.cqc;
                Cmdr.empirerating = @event.empire;
                Cmdr.federationrating = @event.federation;

                WriteCommander();
            }
        }

        private void handleFileHeaderEvent ()
        {
            ReadCommander();

            var configuration = ConfigService.Instance.commanderConfiguration;

            setHomeSystem( configuration.homeSystemAddress );
            setHomeStation( configuration.homeStationMarketID );
            setSquadronSystem( configuration.squadronSystemAddress, configuration.squadronFaction );
        }

        private void handleJumpedEvent ( JumpedEvent @event )
        {
            SetCommanderTitle( @event.controllingfaction?.Allegiance );
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt &&
                     TryUpdateSquadronHomeSystem( @event.factions, @event.systemAddress ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            SetCommanderTitle( @event.controllingsystemfaction.Allegiance );
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                if ( @event.timestamp >= updatedAt &&
                     TryUpdateSquadronHomeSystem( @event.factions, @event.systemAddress ) )
                {
                    WriteCommander();
                }
            }
        }

        private void handlePowerJoinedEvent ( PowerJoinedEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                Cmdr.Power = @event.Power;
                Cmdr.powermerits = 0;
                Cmdr.powerrating = 0;
                WriteCommander();
            }
        }

        private void handlePowerLeftEvent ( PowerLeftEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                Cmdr.Power = Power.None;
                Cmdr.powermerits = null;
                Cmdr.powerrating = 0;
                WriteCommander();
            }
        }

        private void handlePowerPlayEvent ( PowerplayEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                Cmdr.Power = @event.Power;
                Cmdr.powerrating = @event.rank;
                Cmdr.powermerits = @event.merits;
                WriteCommander();
            }
        }

        private void handlePowerVoucherReceivedEvent ( PowerVoucherReceivedEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                Cmdr.Power = @event.Power;
                WriteCommander();
            }
        }

        private void handleSquadronRankEvent ( SquadronRankEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                var rank = SquadronRank.FromRank( @event.newrank + 1 );

                // Update the commander object, if it exists
                if ( Cmdr != null )
                {
                    Cmdr.squadronname = @event.name;
                    Cmdr.squadronrank = rank;
                    WriteCommander();
                }

                // Update the squadron UI data
                Application.Current?.Dispatcher?.InvokeAsync( () =>
                {
                    if ( Application.Current?.MainWindow != null )
                    {
                        ConfigurationWindow.Instance.eddiSquadronNameText.Text = @event.name;
                        ConfigurationWindow.Instance.squadronRankDropDown.SelectedItem = rank;
                    }
                } );
            }
        }

        private void handleSquadronStartupEvent ( SquadronStartupEvent @event )
        {
            if ( @event.timestamp >= updatedAt )
            {
                var rank = SquadronRank.FromRank( @event.rank + 1 );

                // Update the commander object, if it exists
                if ( Cmdr != null )
                {
                    Cmdr.squadronname = @event.name;
                    Cmdr.squadronrank = rank;
                    WriteCommander();
                }

                // Update the squadron UI data
                Application.Current?.Dispatcher?.InvokeAsync( () =>
                {
                    if ( Application.Current?.MainWindow != null )
                    {
                        ConfigurationWindow.Instance.eddiSquadronNameText.Text = @event.name;
                        ConfigurationWindow.Instance.squadronRankDropDown.SelectedItem = rank;
                    }
                } );
            }
        }

        private void handleSquadronStatusEvent ( SquadronStatusEvent @event )
        {
            var configuration = ConfigService.Instance.commanderConfiguration;

            switch ( @event.status )
            {
                case "created":
                    {
                        var rank = SquadronRank.FromRank(1);

                        // Update the configuration file
                        configuration.squadronName = @event.name;
                        configuration.SquadronRank = rank;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ConfigurationWindow.Instance.eddiSquadronNameText.Text = @event.name;
                                ConfigurationWindow.Instance.squadronRankDropDown.SelectedItem = rank;
                                configuration = ConfigurationWindow.Instance.resetSquadronRank( configuration );
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( Cmdr != null )
                        {
                            Cmdr.squadronname = @event.name;
                            Cmdr.squadronrank = rank;
                        }
                        break;
                    }
                case "joined":
                    {
                        // Update the configuration file
                        configuration.squadronName = @event.name;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ConfigurationWindow.Instance.eddiSquadronNameText.Text = @event.name;
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( Cmdr != null )
                        {
                            Cmdr.squadronname = @event.name;
                        }
                        break;
                    }
                case "disbanded":
                case "kicked":
                case "left":
                    {
                        // Update the configuration file
                        configuration.squadronName = null;
                        configuration.squadronID = null;

                        // Update the squadron UI data
                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ConfigurationWindow.Instance.eddiSquadronNameText.Text = string.Empty;
                                ConfigurationWindow.Instance.eddiSquadronIDText.Text = string.Empty;
                                configuration = ConfigurationWindow.Instance.resetSquadronRank( configuration );
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( Cmdr != null )
                        {
                            Cmdr.squadronname = null;
                        }
                        break;
                    }
            }
            ConfigService.Instance.commanderConfiguration = configuration;
        }

        public void setHomeSystem ( ulong? newSystemAddress )
        {
            StarSystem newSystem = null;
            if ( newSystemAddress != null )
            {
                newSystem = EDDI.Instance.DataProvider.GetOrFetchStarSystem( (ulong)newSystemAddress );
            }

            //Ignore null & empty systems
            if ( newSystem?.bodies?.Count > 0 )
            {
                if ( newSystem.systemAddress != EDDI.Instance.HomeStarSystem?.systemAddress )
                {
                    EDDI.Instance.HomeStarSystem = newSystem;
                    Logging.Debug( "Home star system is " + EDDI.Instance.HomeStarSystem.systemname );

                    var configuration = ConfigService.Instance.commanderConfiguration;
                    if ( newSystem.systemAddress != configuration.homeSystemAddress )
                    {
                        configuration.homeSystemName = newSystem.systemname;
                        configuration.homeSystemAddress = newSystem.systemAddress;
                        configuration.homeStationName = null;
                        configuration.homeStationMarketID = null;
                        ConfigService.Instance.commanderConfiguration = configuration;
                    }
                }
            }
            else
            {
                EDDI.Instance.HomeStarSystem = null;
            }

            Application.Current?.Dispatcher?.Invoke( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ConfigurationWindow.Instance.ConfigureHomeSystemOptions( newSystem?.systemname );
                }
            } );
        }

        public void setHomeStation ( long? newMarketId )
        {
            if ( newMarketId != null && EDDI.Instance.HomeStarSystem?.stations != null )
            {
                foreach ( var station in EDDI.Instance.HomeStarSystem.stations )
                {
                    if ( station.marketId == newMarketId )
                    {
                        EDDI.Instance.HomeStation = station;
                        
                        var configuration = ConfigService.Instance.commanderConfiguration;
                        if ( newMarketId != configuration.homeStationMarketID )
                        {
                            configuration.homeStationName = station.name;
                            configuration.homeStationMarketID = station.marketId;
                            ConfigService.Instance.commanderConfiguration = configuration;
                        }

                        Logging.Debug( "Home station is " + EDDI.Instance.HomeStation.name );

                        Application.Current?.Dispatcher?.Invoke( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ConfigurationWindow.Instance.ConfigureHomeStationOptions();
                            }
                        } );
                    }
                }
            }
        }

        private bool TryUpdateSquadronHomeSystem ( List<Faction> factions, ulong currentSystemAddress )
        {
            bool update = false;

            // Check if current system is inhabited by or HQ for squadron faction
            var squadronFaction = factions.FirstOrDefault( f =>
            {
                return f.squadronfaction ||
                       ( f.presences.FirstOrDefault( p => p.systemAddress == currentSystemAddress )?.squadronhomesystem ??
                         false );
            } );

            if ( squadronFaction != null )
            {
                //Update the squadron faction, if changed
                setSquadronFaction( squadronFaction );

                // Update system, allegiance, & power when in squadron home system
                if ( EDDI.Instance.CurrentStarSystem?.systemAddress == currentSystemAddress )
                {
                    // Update the squadron system data, if changed
                    setSquadronSystem( EDDI.Instance.CurrentStarSystem?.systemAddress, squadronFaction.name );

                    // Update the squadron allegiance according to the faction info from the journal
                    setSquadronAllegiance( EDDI.Instance.CurrentStarSystem?.Faction?.Allegiance ?? Superpower.None );

                    // Update the squadron power to match the HQ system's controlling power if it has not been previously set
                    setSquadronPower( EDDI.Instance.CurrentStarSystem?.Power ?? Power.None );
                }

                update = true;
            }

            return update;
        }

        private void setSquadronFaction(Faction squadronFaction)
        {
            if ( string.IsNullOrEmpty( Cmdr.squadronfaction ) || Cmdr.squadronfaction != squadronFaction.name )
            {
                Cmdr.squadronfaction = squadronFaction.name;

                Application.Current?.Dispatcher?.InvokeAsync( () =>
                {
                    if ( Application.Current?.MainWindow != null )
                    {
                        ConfigurationWindow.Instance.squadronFactionDropDown.SelectedItem = squadronFaction;
                    }
                } );
            }
        }

        public void setSquadronSystem ( ulong? newSystemAddress, string squadronFactionName = null )
        {
            StarSystem newSystem = null;
            if ( newSystemAddress != null )
            {
                newSystem = EDDI.Instance.DataProvider.GetOrFetchStarSystem( (ulong)newSystemAddress );
            }

            //Ignore null & empty systems
            if ( newSystem?.bodies.Count > 0 )
            {
                if ( newSystem.systemAddress != SquadronStarSystem?.systemAddress )
                {
                    // Update the SquadronStarSystem object
                    SquadronStarSystem = newSystem;

                    Logging.Debug( $"Squadron star system set to: {newSystem.systemname} ({newSystem.systemAddress})" );

                    var configuration = ConfigService.Instance.commanderConfiguration;
                    configuration.squadronSystemName = newSystem.systemname;
                    configuration.squadronSystemAddress = newSystem.systemAddress;
                    ConfigService.Instance.commanderConfiguration = configuration;

                    // Update the UI
                    Application.Current.Dispatcher?.InvokeAsync( () =>
                    {
                        if ( Application.Current?.MainWindow != null )
                        {
                            ConfigurationWindow.Instance.squadronSystemDropDown.Text = newSystem.systemname;
                        }
                    } );
                }
            }
            else
            {
                SquadronStarSystem = null;
            }

            // Update the UI
            Application.Current.Dispatcher?.InvokeAsync( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ConfigurationWindow.Instance.ConfigureSquadronSystemOptions( newSystem?.systemname );
                    ConfigurationWindow.Instance.ConfigureSquadronFactionOptions( newSystem?.factions, squadronFactionName );
                }
            } );
        }

        private void setSquadronAllegiance(Superpower allegiance = null)
        {
            var configuration = ConfigService.Instance.commanderConfiguration;

            if ( configuration.SquadronAllegiance != allegiance )
            {
                configuration.SquadronAllegiance = allegiance;
                ConfigService.Instance.commanderConfiguration = configuration;

                if ( Cmdr != null )
                {
                    Cmdr.squadronallegiance = allegiance;
                }
            }
        }

        private void setSquadronPower(Power power = null)
        {
            var configuration = ConfigService.Instance.commanderConfiguration;

            if ( configuration.SquadronPower != power )
            {
                configuration.SquadronPower = power;
                ConfigService.Instance.commanderConfiguration = configuration;

                Application.Current?.Dispatcher?.InvokeAsync( () =>
                {
                    if ( Application.Current?.MainWindow != null )
                    {
                        ConfigurationWindow.Instance.ConfigureSquadronPowerOptions(
                            configuration.SquadronAllegiance, configuration.SquadronPower );
                    }
                } );
            }
        }

        public void PostHandle ( Event @event )
        {
            if ( @event is SquadronStartupEvent )
            {
                postHandleSquadronStartupEvent();
            }
        }

        private void postHandleSquadronStartupEvent ()
        {
            var configuration = ConfigService.Instance.commanderConfiguration;
            setSquadronSystem( configuration.squadronSystemAddress, configuration.squadronFaction );
        }

        public void HandleStatus ( Status status )
        {
            lock ( commanderLock )
            {
                Cmdr.credits = status.credit_balance;
            }
        }

        public void HandleProfile ( JObject profile )
        {
            // Update our commander object
            var frontierApiProfile = FrontierApiProfile.FromJson( profile );
            var updatedCmdr = Commander.FromFrontierApiCmdr(Cmdr, frontierApiProfile.Cmdr, frontierApiProfile.timestamp, JournalTimeStamp, out bool cmdrMatches);

            // Stop if the commander returned from the profile does not match our expected commander name
            if ( !cmdrMatches )
            {
                Logging.Debug( "Skipping profile update - Frontier API commander information doesn't match journal information" );
                return;
            }

            Logging.Debug( "Commander information updated from Frontier API; updating local copy" );
            lock ( commanderLock )
            {
                EDDI.Instance.Cmdr = updatedCmdr;
            }
        }

        /// <summary>Work out the title for the commander in the current system</summary>
        private const int minEmpireRankForTitle = 3;
        private const int minFederationRankForTitle = 1;
        private void SetCommanderTitle ( Superpower controllingFactionAllegiance )
        {
            Cmdr.title = EddiCore.Properties.Resources.Commander;
            if ( controllingFactionAllegiance != null )
            {
                if ( controllingFactionAllegiance.invariantName == Superpower.Federation.invariantName &&
                     Cmdr.federationrating != null && Cmdr.federationrating.rank > minFederationRankForTitle )
                {
                    Cmdr.title = Cmdr.federationrating.localizedName;
                }
                else if ( controllingFactionAllegiance.invariantName == Superpower.Empire.invariantName &&
                          Cmdr.empirerating != null && Cmdr.empirerating.rank > minEmpireRankForTitle )
                {
                    Cmdr.title = Cmdr.empirerating.maleRank.localizedName;
                }
            }
        }

        private void ReadCommander ( CommanderConfiguration configuration = null )
        {
            // Obtain current commander from our configuration
            configuration = configuration ?? ConfigService.Instance.commanderConfiguration;

            lock ( commanderLock )
            {
                EDDI.Instance.Cmdr = new Commander
                {
                    name = configuration.commanderName,
                    credits = configuration.credits,
                    friends = configuration.friends,
                    EDID = configuration.frontierID,
                    gender = configuration.gender,
                    phoneticName = configuration.phoneticName,

                    // Power information
                    Power = configuration.Power,
                    powermerits = configuration.powerMerits,
                    powerrating = configuration.powerRank,

                    // Squadron information
                    squadronname = configuration.squadronName,
                    squadronid = configuration.squadronID,
                    squadronrank = configuration.SquadronRank,
                    squadronfaction = configuration.squadronFaction,
                    squadronpower = configuration.SquadronPower,
                    squadronallegiance = configuration.SquadronAllegiance
                };

                updatedAt = configuration.updatedat;
            }
        }

        public void WriteCommander ()
        {
            lock ( commanderLock )
            {
                // Write our current commander configuration
                var configuration = ConfigService.Instance.commanderConfiguration;

                if ( Cmdr != null )
                {
                    configuration.commanderName = Cmdr.name;
                    configuration.credits = Cmdr.credits;
                    configuration.friends = Cmdr.friends;
                    configuration.frontierID = Cmdr.EDID;
                    configuration.gender = Cmdr.gender;
                    configuration.phoneticName = Cmdr.phoneticName;

                    // Write power information
                    configuration.Power = Cmdr.Power;
                    configuration.powerMerits = Cmdr.powermerits;
                    configuration.powerRank = Cmdr.powerrating;

                    // Write squadron information
                    configuration.squadronName = Cmdr.squadronname;
                    configuration.squadronID = Cmdr.squadronid;
                    configuration.SquadronRank = Cmdr.squadronrank;
                    configuration.squadronFaction = Cmdr.squadronfaction;
                    configuration.SquadronPower = Cmdr.squadronpower;
                }

                // Write home system information
                configuration.homeSystemName = EDDI.Instance.HomeStarSystem?.systemname;
                configuration.homeSystemAddress = EDDI.Instance.HomeStarSystem?.systemAddress;
                configuration.homeStationName = EDDI.Instance.HomeStation?.name;
                configuration.homeStationMarketID = EDDI.Instance.HomeStation?.marketId;

                // Write squadron star system information
                configuration.squadronSystemName = SquadronStarSystem?.systemname;
                configuration.squadronSystemAddress = SquadronStarSystem?.systemAddress;

                configuration.updatedat = updatedAt;
                ConfigService.Instance.commanderConfiguration = configuration;
            }
        }
        
        #region Implement INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }        

        #endregion
    }
}
