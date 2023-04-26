using System;
using System.Collections.Generic;
using System.Reflection;

namespace EddiEvents
{
    public class Events
    {
        public static IDictionary<string, Type> TYPES = new Dictionary<string, Type>();
        public static IDictionary<string, object> SAMPLES = new Dictionary<string, object>();
        public static IDictionary<string, string> DEFAULTS = new Dictionary<string, string>();
        public static IDictionary<string, string> DESCRIPTIONS = new Dictionary<string, string>();

        static Events()
        {
            lock (SAMPLES)
            {
                try
                {
                    foreach (var type in typeof(Event).Assembly.GetTypes())
                    {
                        if ( type.IsInterface ||
                             type.IsAbstract ||
                             !type.IsSubclassOf( typeof(Event) ) )
                        {
                            continue;
                        }

                        // Ensure that the static constructor of the class has been run
                        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                        if (type.GetField("NAME") is var nameField && 
                            nameField?.GetValue( null ) is string eventName)
                        {
                            TYPES.Add(eventName, type);

                            if (type.GetField("DESCRIPTION") is var descriptionField && 
                                descriptionField?.GetValue(null) is string eventDescription)
                            {
                                DESCRIPTIONS.Add(eventName, eventDescription);
                            }

                            if (type.GetField("DEFAULT") is var defaultField &&
                                defaultField?.GetValue(null) is string eventDefault)
                            {
                                DEFAULTS.Add(eventName, eventDefault);
                            }

                            if (type.GetField("SAMPLE") is var sampleField &&
                                sampleField?.GetValue(null) is var eventSample )
                            {
                                SAMPLES.Add(eventName, eventSample);
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    // DLL we can't parse; ignore
                }
            }
        }

        public static Type TypeByName(string name)
        {
            TYPES.TryGetValue(name, out Type value);
            return value;
        }

        public static object SampleByName(string name)
        {
            SAMPLES.TryGetValue(name, out object value);
            return value;
        }

        public static string DescriptionByName(string name)
        {
            DESCRIPTIONS.TryGetValue(name, out string value);
            return value;
        }

        public static string DefaultByName(string name)
        {
            DEFAULTS.TryGetValue(name, out string value);
            return value;
        }
    }
}
