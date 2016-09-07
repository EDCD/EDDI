using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EliteDangerousJournalMonitor;
using Cottle.Values;
using EliteDangerousSpeechService;
using Utilities;
using Newtonsoft.Json;
using EliteDangerousEvents;

namespace EDDI
{
    /// <summary>
    /// A responder that responds to events with a speech
    /// </summary>
    public class SpeechResponder : EDDIResponder
    {
        private ScriptResolver scriptResolver;
        private SpeechService speechService;

        public SpeechResponder()
        {
            Logging.Info("Started speech responder");
        }

        public void Start()
        {
            scriptResolver = new ScriptResolver(Eddi.Instance.configuration.Scripts);
            speechService = new SpeechService(SpeechServiceConfiguration.FromFile());
        }

        public void Stop()
        {
            if (speechService != null)
            {
                speechService.ShutdownSpeech();
            }
        }

        // Say something with the default resolver
        public void Handle(Event theEvent)
        {
            Say(scriptResolver, theEvent.type);
        }

        // Say something with a custom resolver
        public void Say(ScriptResolver resolver, string scriptName)
        {
            Dictionary<string, Cottle.Value> dict = createVariables();
            string result = resolver.resolve(scriptName, dict);
            speechService.Say(Eddi.Instance.Cmdr, Eddi.Instance.Ship, result);
        }

        // Create Cottle variables from the EDDI information
        private Dictionary<string, Cottle.Value> createVariables()
        {
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>();

            // Start with commander variables
            if (Eddi.Instance.Cmdr != null)
            {
                dict["cmdr"] = new ReflectionValue(Eddi.Instance.Cmdr);
            }

            if (Eddi.Instance.Ship != null)
            {
                dict["ship"] = new ReflectionValue(Eddi.Instance.Ship);
            }

            if (Eddi.Instance.HomeStarSystem != null)
            {
                dict["homesystem"] = new ReflectionValue(Eddi.Instance.HomeStarSystem);
            }

            if (Eddi.Instance.CurrentStarSystem != null)
            {
                dict["system"] = new ReflectionValue(Eddi.Instance.CurrentStarSystem);
            }

            if (Eddi.Instance.LastStarSystem != null)
            {
                dict["lastsystem"] = new ReflectionValue(Eddi.Instance.LastStarSystem);
            }
            return dict;
        }
    }
}
