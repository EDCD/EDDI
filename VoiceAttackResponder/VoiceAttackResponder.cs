using Eddi;
using Utilities;
using EddiEvents;
using Newtonsoft.Json;
using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

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

        private static List<Event> eventQueue = new List<Event>();

        private static bool enqueueEvents;

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

        public void Handle(Event @event)
        {
            // Beginning with Elite Dangerous v. 3.3, the primary star scan is delivered via a Scan with scantype `Autoscan` 
            // when you jump into the system. Secondary stars are delivered in a burst following an FSSDiscoveryScan.
            // Since each source has a different trigger, we need to re-order events and ensure the main star scan 
            // completes after the the FSSDicoveryScan and before secondary star scans, 
            // regardless of the timing of the `Autoscan` Scan and FSSDiscoveryScan events
            if (@event is FSSDiscoveryScanEvent)
            {
                enqueueEvents = true;
            }
            if (@event is StarScannedEvent starScannedEvent)
            {
                if (enqueueEvents)
                {
                    if (starScannedEvent.mainstar)
                    {
                        enqueueEvents = false;
                        SendtoPlugin(@event);
                        foreach (Event theEvent in eventQueue.OfType<StarScannedEvent>())
                        {
                            SendtoPlugin(theEvent);
                        }
                        eventQueue = (List<Event>)eventQueue.SkipWhile(s => (StarScannedEvent)s != null);
                    }
                    else
                    {
                        eventQueue.Add(@event);
                        eventQueue.OrderBy(s => ((StarScannedEvent)s)?.distance);
                    }
                }
                else
                {
                    SendtoPlugin(@event);
                }
            }
            SendtoPlugin(@event);
        }

        private void SendtoPlugin(Event @event)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(@event));
            VoiceAttackPlugin.EventQueue.Add(@event);
            OnEvent(new EventArgs(), @event);
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
