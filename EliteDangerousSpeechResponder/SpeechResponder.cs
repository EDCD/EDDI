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
        private ScriptResolver scriptResolver;

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
            return "Respond to events with pre-scripted responses using the information available.  Scripts can be individually enabled and customised";
        }

        public SpeechResponder()
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            Personality personality = null;
            if (configuration != null && configuration.Personality != null)
            {
                personality = Personality.FromName(configuration.Personality);
            }
            if (personality == null)
            { 
                personality = Personality.Default();
                //configuration.Personality = personality.Name;
                //configuration.ToFile();
            }
            scriptResolver = new ScriptResolver(personality.Scripts);
            Logging.Info("Initialised " + ResponderName() + " " + ResponderVersion());
        }

        public bool Start()
        {
            return true;
        }

        public void Stop()
        {
            SpeechService.Instance.Say(Eddi.Instance.Cmdr, Eddi.Instance.Ship, "Goodbye.", false, true, false);
        }

        public void Reload()
        {
            SpeechResponderConfiguration configuration = SpeechResponderConfiguration.FromFile();
            Personality personality = Personality.FromName(configuration.Personality);
            if (personality == null)
            {
                Logging.Warn("Failed to find named personality; falling back to default");
                personality = Personality.Default();
                configuration.Personality = personality.Name;
                configuration.ToFile();
            }
            scriptResolver = new ScriptResolver(personality.Scripts);
            Logging.Info("Reloaded " + ResponderName() + " " + ResponderVersion());
        }

        // Say something with the default resolver
        public void Handle(Event theEvent)
        {
            Logging.Debug("Received event " + JsonConvert.SerializeObject(theEvent));
            Say(scriptResolver, theEvent.type, theEvent);
        }

        // Say something with a custom resolver
        public void Say(ScriptResolver resolver, string scriptName, Event theEvent = null)
        {
            Dictionary<string, Cottle.Value> dict = createVariables(theEvent);
            string result = resolver.resolve(scriptName, dict);
            if (result != null)
            {
                SpeechService.Instance.Say(Eddi.Instance.Cmdr, Eddi.Instance.Ship, result, false, false, resolver.isInterruptable(scriptName));
            }
        }

        // Create Cottle variables from the EDDI information
        private Dictionary<string, Cottle.Value> createVariables(Event theEvent = null)
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

            if (theEvent != null)
            {
                dict["event"] = new ReflectionValue(theEvent);
            }

            return dict;
        }

        public UserControl ConfigurationTabItem()
        {
            return new ConfigurationWindow();
        }
    }
}
