using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        static Events()
        {
            lock (SAMPLES)
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                List<Type> events = new List<Type>();
                Type eventType = typeof(Event);
                foreach (FileInfo file in dir.GetFiles("*.dll", SearchOption.AllDirectories)
                             .Where(f=> f.Name != "SQLite.Interop.dll")) // SQLite.Interop.dll contains unmanaged code and would throw a BadImageFormatException
                {
                    Type currentEvent = null;
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
                                        currentEvent = type;
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
                        // Ignore this; probably due to CPU architecture mismatch
                    }
                    catch (Exception ex)
                    {
                        Logging.Warn($"Exception with {currentEvent}: ", ex);
                    }
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
