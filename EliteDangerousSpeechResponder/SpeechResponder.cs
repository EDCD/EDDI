using System.Collections.Generic;
using Cottle.Values;
using EliteDangerousSpeechService;
using Utilities;
using Newtonsoft.Json;
using EliteDangerousEvents;
using EDDI;
using System.Windows.Controls;

namespace EliteDangerousSpeechResponder
{
    /// <summary>
    /// A responder that responds to events with a speech
    /// </summary>
    public class SpeechResponder : EDDIResponder
    {
        private Dictionary<string, Script> scripts;

        private ScriptResolver scriptResolver;
        private SpeechService speechService;

        public string ResponderName()
        {
            return "Speech responder";
        }

        public string ResponderVersion()
        {
            return "1.0.0";
        }

        public string ResponderDescription()
        {
            return "Plugin to respond to events with scripts.  Scripts can be individually enabled and customised";
        }

        public SpeechResponder()
        {
            ScriptsConfiguration configuration = ScriptsConfiguration.FromFile();
            scripts = configuration.Scripts;
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
        {
            scriptResolver = new ScriptResolver(scripts);
            speechService = new SpeechService(SpeechServiceConfiguration.FromFile());
            return true;
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
            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));
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

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
