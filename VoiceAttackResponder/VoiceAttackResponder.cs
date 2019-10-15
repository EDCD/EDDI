using Eddi;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Windows.Controls;
using Utilities;

namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// A responder for EDDI to provide information to VoiceAttack.  This is very simple, just adding events to the VoiceAttack plugin's event queue
    /// </summary>
    class VoiceAttackResponder : EDDIResponder
    {
        public static event EventHandler<Event> RaiseEvent;

        protected virtual void OnEvent(EventArgs @eventArgs, Event @event)
        {
            RaiseEvent?.Invoke(@eventArgs, @event);
        }

        public string ResponderName()
        {
            return "VoiceAttack responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.VoiceAttack.name;
        }

        public string ResponderDescription()
        {
            return Properties.VoiceAttack.desc;
        }

        public VoiceAttackResponder()
        {
            Logging.Info("Started VoiceAttack responder");
        }

        public void Handle(Event @event)
        {
            if (@event.fromLoad)
            {
                return;
            }

            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));
            OnEvent(EventArgs.Empty, @event);
        }

        public bool Start()
        {
            if (EDDI.FromVA)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Stop()
        {
        }

        public void Reload()
        {
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
