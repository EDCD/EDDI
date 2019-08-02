using Eddi;
using EddiDataProviderService;
using EddiEvents;
using EddiInaraService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Utilities;

namespace EddiInaraResponder
{
    public class InaraResponder : EDDIResponder
    {
        private Thread updateThread;

        public string ResponderName()
        {
            return "Inara responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.InaraResources.name;
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return Properties.InaraResources.desc;
        }

        public InaraResponder()
        {
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
        {
            Reload();
            return InaraService.Instance != null;
        }

        public void Reload()
        { }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }

        public void Stop()
        {
            updateThread?.Abort();
            updateThread = null;
        }

        public void Handle(Event theEvent)
        {
            if (EDDI.Instance.inCQC)
            {
                // We don't do anything whilst in CQC
                return;
            }

            if (EDDI.Instance.inCrew)
            {
                // We don't do anything whilst in multicrew
                return;
            }

            if (EDDI.Instance.inBeta)
            {
                // We don't send data whilst in beta
                return;
            }

            if (InaraService.Instance != null)
            {
                throw new NotImplementedException();
            }
        }
    }
}
