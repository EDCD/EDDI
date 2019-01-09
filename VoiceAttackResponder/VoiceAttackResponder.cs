using Eddi;
using Utilities;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Windows.Controls;

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
            if (RaiseEvent != null)
            {
                RaiseEvent(@eventArgs, @event);
            }
        }

        public string ResponderName()
        {
            return "VoiceAttack responder";
        }

        public string LocalizedResponderName()
        {
            return Properties.VoiceAttack.name;
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return Properties.VoiceAttack.desc;
        }

        public VoiceAttackResponder()
        {
            Logging.Info("Started VoiceAttack responder");
        }

        public void Handle(Event theEvent)
        {
            if (theEvent.fromLoad)
            {
                return;
            }

            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));
            VoiceAttackPlugin.EventQueue.Add(theEvent);
            OnEvent(new EventArgs(), theEvent);
        }

        public bool Start()
        {
            return true;
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
