using EddiCore;
using EddiDataDefinitions;
using EddiEvents;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Eddi
{
    public class MainWindowResponder: EDDIResponder
    {
        private MainWindow _mainWindow = null;

        private MainWindow MainWindow
        {
            get
            {
                if (_mainWindow == null)
                {
                    if (Application.Current?.MainWindow != null)
                    {
                        _mainWindow = ((MainWindow) Application.Current.MainWindow);
                    }
                }

                return _mainWindow;
            }
        }

        public string ResponderName() => "MainWindow";

        public string LocalizedResponderName() => "MainWindow";

        public string ResponderDescription() => "Main Window Description";

        public bool Start() => true;

        public void Stop() { }

        public void Reload() { }
        public System.Windows.Controls.UserControl ConfigurationTabItem() => null;

        public void Handle(Event theEvent)
        {
            if (MainWindow == null)
                return;

            switch (theEvent)
            {
                case SquadronStatusEvent squadronStatus:
                    eventSquadronStatus(squadronStatus);
                    break;
                case SquadronRankEvent squadronRank:
                    eventSquadronRank(squadronRank);
                    break;
                case CarrierJumpedEvent carrierJumped:
                    updateSquadronFactionDropDown(carrierJumped.factions);
                    break;
                case LocationEvent location:
                    updateSquadronFactionDropDown(location.factions);
                    break;
                case JumpedEvent jumped:
                    updateSquadronFactionDropDown(jumped.factions);
                    break;
                default:
                    break;
            }
        }

        private void updateSquadronFactionDropDown(IEnumerable<Faction> factions)
        {
            //Update the squadron faction
            var configuration = EDDIConfiguration.FromFile();
            MainWindow.squadronFactionDropDown.SelectedItem = configuration.SquadronFaction;

            // Update system, allegiance, & power when in squadron home system
            var currentSystem = EDDI.Instance.CurrentStarSystem;

            var squadronFaction = factions.FirstOrDefault(f =>
                (bool)f.presences.
                    FirstOrDefault(p => p.systemName == currentSystem.systemname)?.squadronhomesystem || f.squadronfaction);

            if (squadronFaction != null)
            {
                MainWindow.squadronSystemDropDown.Text = currentSystem.systemname;
                MainWindow.ConfigureSquadronFactionOptions(configuration);
            }

            // Update the squadron power, if changed
            var power = Power.FromName(currentSystem?.power);
            if (power == null || power == configuration.SquadronPower)
            {
                return;
            }

            MainWindow.squadronPowerDropDown.SelectedItem = power.localizedName;
            MainWindow.ConfigureSquadronPowerOptions(configuration);
        }

        //private void updateSquadronFactionDropDown()
        //{
        //    EDDIConfiguration configuration = EDDIConfiguration.FromFile();
        //    MainWindow.squadronFactionDropDown.SelectedItem = configuration.SquadronFaction;

        //    var currentSystem = EDDI.Instance.CurrentStarSystem;

        //    // Check if current system is inhabited by or HQ for squadron faction
        //    var squadronFaction = factions.FirstOrDefault(f =>
        //        (bool)f.presences.
        //            FirstOrDefault(p => p.systemName == currentSystem.systemname)?.squadronhomesystem || f.squadronfaction);
        //    if (squadronFaction != null)
        //    {
        //        MainWindow.squadronFactionDropDown.SelectedItem = squadronFaction.name;
        //    }
        //}

        private void eventSquadronStatus(SquadronStatusEvent theEvent)
        {
            var configuration = EDDIConfiguration.FromFile();
            switch (theEvent.status)
            {
                case "created":
                    MainWindow.eddiSquadronNameText.Text = theEvent.name;
                    MainWindow.squadronRankDropDown.SelectedItem = configuration.SquadronRank.localizedName;
                    configuration = MainWindow.resetSquadronRank(configuration);
                    break;
                case "joined":
                    MainWindow.eddiSquadronNameText.Text = theEvent.name;
                    break;
                case "disbanded":
                case "kicked":
                case "left":
                    MainWindow.eddiSquadronNameText.Text = string.Empty;
                    MainWindow.eddiSquadronIDText.Text = string.Empty;
                    configuration = MainWindow.resetSquadronRank(configuration);
                    break;
            }
        }

        private void eventSquadronRank(SquadronRankEvent theEvent)
        {
            var rank = SquadronRank.FromRank(theEvent.newrank + 1);

            MainWindow.eddiSquadronNameText.Text = theEvent.name;
            MainWindow.squadronRankDropDown.SelectedItem = rank.localizedName;
        }
    }
}
