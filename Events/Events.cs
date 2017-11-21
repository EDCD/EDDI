using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Utilities;

namespace EddiEvents
{
    public class Events
    {
        public static IDictionary<string, Type> TYPES = new Dictionary<string, Type>();
        public static IDictionary<string, object> SAMPLES = new Dictionary<string, object>();
        public static IDictionary<string, string> DEFAULTS = new Dictionary<string, string>();
        public static IDictionary<string, string> DESCRIPTIONS = new Dictionary<string, string>();
        public static IDictionary<string, IDictionary<string, string>> VARIABLES = new Dictionary<string, IDictionary<string, string>>();

        static Events()
        {
            lock (SAMPLES)
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                List<Type> events = new List<Type>();
                Type eventType = typeof(Event);
                foreach (FileInfo file in dir.GetFiles("*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(file.FullName);
                        if (assembly == null)
                        {
                            Logging.Warn("Failed to read assembly for file " + file.FullName);
                        }
                        else
                        {
                            try
                            {

                                foreach (Type type in assembly.GetTypes())
                                {
                                    if (type.IsInterface || type.IsAbstract)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        if (type.IsSubclassOf(eventType))
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

                                                if (type.GetField("VARIABLES") != null)
                                                {
                                                    Dictionary<string, string> eventVariables = (Dictionary<string, string>)type.GetField("VARIABLES").GetValue(null);
                                                    if (eventVariables != null)
                                                    {
                                                        VARIABLES.Add(eventName, eventVariables);
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
                    catch (BadImageFormatException)
                    {
                        // Ignore this; probably due to CPU architecure mismatch
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn("Exception: ", ex);
                    }
                }
            }
        }

        public static Type TypeByName(string name)
        {
            Type value;
            TYPES.TryGetValue(name, out value);
            return value;
        }

        public static object SampleByName(string name)
        {
            object value;
            SAMPLES.TryGetValue(name, out value);
            return value;
        }

        public static string DescriptionByName(string name)
        {
            string value;
            DESCRIPTIONS.TryGetValue(name, out value);
            return value;
        }

        public static string DefaultByName(string name)
        {
            string value;
            DEFAULTS.TryGetValue(name, out value);
            return value;
        }
    }
}
