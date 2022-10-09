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
                    foreach (Type type in typeof(Event).Assembly.GetTypes())
                    {
                        if (!type.IsInterface && !type.IsAbstract)
                        {
                            if (type.IsSubclassOf(typeof(Event)))
                            {
                                // Ensure that the static constructor of the class has been run
                                System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);

                                if (type.GetField("NAME") != null)
                                {
                                    string eventName = (string)type.GetField("NAME").GetValue(null);

                                    TYPES.Add(eventName, type);

                                    if (type.GetField("DESCRIPTION") != null)
                                    {
                                        string eventDescription = (string)type.GetField("DESCRIPTION").GetValue(null);
                                        if (eventDescription != null)
                                        {
                                            DESCRIPTIONS.Add(eventName, eventDescription);
                                        }
                                    }

                                    if (type.GetField("DEFAULT") != null)
                                    {
                                        string eventDefault = (string)type.GetField("DEFAULT").GetValue(null);
                                        if (eventDefault != null)
                                        {
                                            DEFAULTS.Add(eventName, eventDefault);
                                        }
                                    }

                                    if (type.GetField("SAMPLE") != null)
                                    {
                                        object eventSample = type.GetField("SAMPLE").GetValue(null);
                                        if (eventSample != null)
                                        {
                                            SAMPLES.Add(eventName, eventSample);
                                        }
                                    }
                                }
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
