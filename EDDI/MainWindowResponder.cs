using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EddiCore;
using EddiEvents;

namespace Eddi
{
    public class MainWindowResponder: EDDIResponder
    {
        public string ResponderName() => "MainWindow";

        public string LocalizedResponderName() => "MainWindow";

        public string ResponderDescription() => "Main Window Description";

        public bool Start() => true;

        public void Stop() { }

        public void Reload() { }

        public void Handle(Event theEvent)
        {
            throw new NotImplementedException();
        }

        public System.Windows.Controls.UserControl ConfigurationTabItem() => null;
    }
}
