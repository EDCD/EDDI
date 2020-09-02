using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using EddiCore;
using EddiDataDefinitions;
using EddiEvents;

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

        public void Handle(Event theEvent)
        {
            if (MainWindow == null)
                return;

            switch (theEvent)
            {
                case SquadronStatusEvent statusEvent:
                    eventSquadronStatus(statusEvent);
                    break;
                case SquadronRankEvent rankEvent:
                    eventSquadronRank(rankEvent);
                    break;
                default:
                    break;
            }
        }

        public System.Windows.Controls.UserControl ConfigurationTabItem() => null;

        private void eventSquadronStatus(SquadronStatusEvent theEvent)
        {

        }

        private void eventSquadronRank(SquadronRankEvent theEvent)
        {
            var rank = SquadronRank.FromRank(theEvent.newrank + 1);

            MainWindow.eddiSquadronNameText.Text = theEvent.name;
            MainWindow.squadronRankDropDown.SelectedItem = rank.localizedName;
        }
    }
}
