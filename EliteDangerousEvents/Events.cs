using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace EliteDangerousEvents
{
    public class Events
    {
        public static Dictionary<string, Type> TYPES = new Dictionary<string, Type>();
        public static Dictionary<string, Event> SAMPLES = new Dictionary<string, Event>();
        public static Dictionary<string, string> DEFAULTS = new Dictionary<string, string>();
        public static Dictionary<string, string> DESCRIPTIONS = new Dictionary<string, string>();

        static Events()
        {
            lock (SAMPLES)
            {
                DirectoryInfo dir = new DirectoryInfo(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
                List<Type> events = new List<Type>();
                Type eventType = typeof(Event);
                foreach (FileInfo file in dir.GetFiles("EliteDangerousEvents.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        Assembly assembly = Assembly.LoadFrom(file.FullName);
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
                                            Event eventSample = (Event)type.GetField("SAMPLE").GetValue(null);
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
                    catch (BadImageFormatException)
                    {
                        // Ignore this; probably due to CPU architecure mismatch
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

        public static Event SampleByName(string name)
        {
            Event value;
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
