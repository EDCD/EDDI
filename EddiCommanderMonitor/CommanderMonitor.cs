using CommanderMonitor;
using EddiConfigService;
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

        [CanBeNull]
        public StarSystem SquadronStarSystem // May be null when the commander hasn't set a squadron star system
        {
            get => squadronStarSystem;
            set
            {
                void childPropertyChangedHandler ( object sender, PropertyChangedEventArgs e )
                {
                    OnPropertyChanged();
                }
                if ( squadronStarSystem != null )
                { squadronStarSystem.PropertyChanged -= childPropertyChangedHandler; }
                if ( value != null )
                { value.PropertyChanged += childPropertyChangedHandler; }
                squadronStarSystem = value;
                OnPropertyChanged();
            }
        }
        private StarSystem squadronStarSystem;

        public string MonitorName () => "Commander monitor";

        public string LocalizedMonitorName () => Properties.Resources.MonitorName;

        public string MonitorDescription () => Properties.Resources.MonitorDescription;

        public bool IsRequired () => true;

        public bool NeedsStart () => false;

        public void Start () => Reload();

        public void Stop ()
        { }

        public void Reload ()
        { }

        public IDictionary<string, Tuple<Type, object>> GetVariables ()
        {
            return new Dictionary<string, Tuple<Type, object>>()
            {
                { "squadronsystem", new Tuple<Type, object>( typeof(StarSystem), SquadronStarSystem ) }
            };
        }

        public UserControl ConfigurationTabItem () => new ConfigurationWindow();

        public void PreHandle ( Event @event )
        {
            if ( @event is CarrierJumpedEvent carrierJumpedEvent )
            {
                handleCarrierJumpedEvent( carrierJumpedEvent );
            }
            else if ( @event is JumpedEvent jumpedEvent )
            {
                handleJumpedEvent( jumpedEvent );
            }
            else if ( @event is LocationEvent locationEvent )
            {
                handleLocationEvent( locationEvent );
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
        }

        private void handleCarrierJumpedEvent ( CarrierJumpedEvent @event )
        {
            if ( ( @event.docked || @event.onFoot ) && @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                // Check if current system is inhabited by or HQ for squadron faction
                var squadronFaction = @event.factions.FirstOrDefault(f =>
                {
                    var squadronhomesystem = f.presences
                        .FirstOrDefault(p => p.systemAddress == EDDI.Instance.CurrentStarSystem.systemAddress)?.squadronhomesystem;
                    return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                });
                if ( squadronFaction != null )
                {
                    updateSquadronData( squadronFaction, EDDI.Instance.CurrentStarSystem.systemAddress );
                }
            }
        }

        private void handleJumpedEvent ( JumpedEvent @event )
        {
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                // Check if current system is inhabited by or HQ for squadron faction
                var squadronFaction = @event.factions.FirstOrDefault(f =>
                {
                    var squadronhomesystem = f.presences
                        .FirstOrDefault(p => p.systemAddress == EDDI.Instance.CurrentStarSystem.systemAddress)?.squadronhomesystem;
                    return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                });
                if ( squadronFaction != null )
                {
                    updateSquadronData( squadronFaction, EDDI.Instance.CurrentStarSystem.systemAddress );
                }
            }
        }

        private void handleLocationEvent ( LocationEvent @event )
        {
            if ( @event.factions.Any() && EDDI.Instance.CurrentStarSystem != null )
            {
                // Check if current system is inhabited by or HQ for squadron faction
                var squadronFaction = @event.factions.FirstOrDefault(f =>
                {
                    var squadronhomesystem = f.presences
                        .FirstOrDefault(p => p.systemAddress == EDDI.Instance.CurrentStarSystem.systemAddress)?.squadronhomesystem;
                    return squadronhomesystem != null && ((bool)squadronhomesystem || f.squadronfaction);
                });
                if ( squadronFaction != null )
                {
                    updateSquadronData( squadronFaction, EDDI.Instance.CurrentStarSystem.systemAddress );
                }
            }
        }

        private bool handleSquadronRankEvent ( SquadronRankEvent @event )
        {
            var rank = SquadronRank.FromRank(@event.newrank + 1);

            // Update the configuration file
            var configuration = ConfigService.Instance.commanderConfiguration;
            configuration.squadronName = @event.name;
            configuration.SquadronRank = rank;
            ConfigService.Instance.commanderConfiguration = configuration;

            // Update the squadron UI data
            Application.Current?.Dispatcher?.InvokeAsync( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                    ( (ConfigurationWindow)ConfigurationTabItem() ).squadronRankDropDown.SelectedItem = rank;
                }
            } );

            // Update the commander object, if it exists
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.squadronname = @event.name;
                EDDI.Instance.Cmdr.squadronrank = rank;
            }
            return true;
        }

        private void handleSquadronStartupEvent ( SquadronStartupEvent @event )
        {
            var rank = SquadronRank.FromRank(@event.rank + 1);

            // Update the configuration file
            var configuration = ConfigService.Instance.commanderConfiguration;
            configuration.squadronName = @event.name;
            configuration.SquadronRank = rank;
            ConfigService.Instance.commanderConfiguration = configuration;

            // Update the squadron UI data
            Application.Current?.Dispatcher?.InvokeAsync( () =>
            {
                if ( Application.Current?.MainWindow != null )
                {
                    ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                    ( (ConfigurationWindow)ConfigurationTabItem() ).squadronRankDropDown.SelectedItem = rank;
                }
            } );

            // Update the commander object, if it exists
            if ( EDDI.Instance.Cmdr != null )
            {
                EDDI.Instance.Cmdr.squadronname = @event.name;
                EDDI.Instance.Cmdr.squadronrank = rank;
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
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                                ( (ConfigurationWindow)ConfigurationTabItem() ).squadronRankDropDown.SelectedItem = rank;
                                configuration = ( (ConfigurationWindow)ConfigurationTabItem() ).resetSquadronRank( configuration );
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronname = @event.name;
                            EDDI.Instance.Cmdr.squadronrank = rank;
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
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = @event.name;
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronname = @event.name;
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
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronNameText.Text = string.Empty;
                                ( (ConfigurationWindow)ConfigurationTabItem() ).eddiSquadronIDText.Text = string.Empty;
                                configuration = ( (ConfigurationWindow)ConfigurationTabItem() ).resetSquadronRank( configuration );
                            }
                        } );

                        // Update the commander object, if it exists
                        if ( EDDI.Instance.Cmdr != null )
                        {
                            EDDI.Instance.Cmdr.squadronname = null;
                        }
                        break;
                    }
            }
            ConfigService.Instance.commanderConfiguration = configuration;
        }

        public void setSquadronSystem ( ulong? newSystemAddress )
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
                    SquadronStarSystem = newSystem;
                    Logging.Debug( $"Squadron star system set to: {newSystemAddress} ({SquadronStarSystem?.systemname})" );

                    var configuration = ConfigService.Instance.commanderConfiguration;
                    configuration.squadronSystemName = newSystem.systemname;
                    configuration.squadronSystemAddress = newSystem.systemAddress;
                    ConfigService.Instance.commanderConfiguration = configuration;
                }
            }
            else
            {
                SquadronStarSystem = null;
            }
        }

        public void updateSquadronData ( Faction faction, ulong systemAddress )
        {
            if ( faction != null )
            {
                var configuration = ConfigService.Instance.commanderConfiguration;

                //Update the squadron faction, if changed
                if ( configuration.squadronFaction == null || configuration.squadronFaction != faction.name )
                {
                    configuration.squadronFaction = faction.name;

                    Application.Current?.Dispatcher?.InvokeAsync( () =>
                    {
                        if ( Application.Current?.MainWindow != null )
                        {
                            ( (ConfigurationWindow)ConfigurationTabItem() ).squadronFactionDropDown.SelectedItem = faction.name;
                        }
                    } );

                    if ( EDDI.Instance.Cmdr != null )
                    {
                        EDDI.Instance.Cmdr.squadronfaction = faction.name;
                    }
                }

                // Update system, allegiance, & power when in squadron home system
                if ( ( faction.presences.FirstOrDefault( p => p.systemAddress == systemAddress )?.squadronhomesystem ?? false ) )
                {
                    // Update the squadron system data, if changed
                    if ( configuration.squadronSystemAddress == null || configuration.squadronSystemAddress != EDDI.Instance.CurrentStarSystem?.systemAddress )
                    {
                        configuration.squadronSystemAddress = EDDI.Instance.CurrentStarSystem?.systemAddress;

                        Application.Current?.Dispatcher?.InvokeAsync( () =>
                        {
                            if ( Application.Current?.MainWindow != null )
                            {
                                ( (ConfigurationWindow)ConfigurationTabItem() ).squadronSystemDropDown.Text = EDDI.Instance.CurrentStarSystem?.systemname;
                                ( (ConfigurationWindow)ConfigurationTabItem() ).ConfigureSquadronFactionOptions();
                            }
                        } );

                        setSquadronSystem( configuration.squadronSystemAddress );
                    }

                    //Update the squadron allegiance, if changed
                    Superpower allegiance = EDDI.Instance.CurrentStarSystem?.Faction?.Allegiance ?? Superpower.None;

                    //Prioritize UI entry if squadron system allegiance not specified
                    if ( allegiance != Superpower.None )
                    {
                        if ( configuration.SquadronAllegiance == Superpower.None || configuration.SquadronAllegiance != allegiance )
                        {
                            configuration.SquadronAllegiance = allegiance;
                            if ( EDDI.Instance.Cmdr != null )
                            {
                                EDDI.Instance.Cmdr.squadronallegiance = allegiance;
                            }
                        }
                    }

                    // Update the squadron power, if changed
                    Power power = Power.FromName(EDDI.Instance.CurrentStarSystem?.power) ?? Power.None;

                    //Prioritize UI entry if squadron system power not specified
                    if ( power != Power.None )
                    {
                        if ( configuration.SquadronPower == Power.None && configuration.SquadronPower != power )
                        {
                            configuration.SquadronPower = power;

                            Application.Current?.Dispatcher?.InvokeAsync( () =>
                            {
                                if ( Application.Current?.MainWindow != null )
                                {
                                    ( (ConfigurationWindow)ConfigurationTabItem() ).squadronPowerDropDown.SelectedItem = power.localizedName;
                                    ( (ConfigurationWindow)ConfigurationTabItem() ).ConfigureSquadronPowerOptions( configuration );
                                }
                            } );
                        }
                    }
                }
                ConfigService.Instance.commanderConfiguration = configuration;
            }
        }

        public void PostHandle ( Event @event )
        { }

        public void HandleStatus ( Status status )
        { }

        public void HandleProfile ( JObject profile )
        { }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged ( [CallerMemberName] string propertyName = null )
        {
            PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected bool SetField<T> ( ref T field, T value, [CallerMemberName] string propertyName = null )
        {
            if ( EqualityComparer<T>.Default.Equals( field, value ) )
                return false;
            field = value;
            OnPropertyChanged( propertyName );
            return true;
        }
    }
}
