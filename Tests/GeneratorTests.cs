using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackResponder;
using System;
using EddiEvents;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using CSCore.DMO;

namespace Tests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void TestGenerateVoiceAttack()
        {
            foreach (KeyValuePair<string, Type> entry in Events.TYPES.OrderBy(i => i.Key))
            {
                Console.WriteLine("### " + entry.Key);
                Console.WriteLine(Events.DESCRIPTIONS[entry.Key] + ".");
                Console.WriteLine("To run a command when this event occurs you should create the command with the name ((EDDI " + entry.Key.ToLowerInvariant() + "))");
                IDictionary<string, string> variables;
                if (Events.VARIABLES.TryGetValue(entry.Key, out variables))
                {
                    if (variables.Count > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine("Variables set with this event are as follows:");
                        Console.WriteLine();
                        foreach (KeyValuePair<string, string> variable in variables.OrderBy(i => i.Key))
                        {
                            System.Reflection.MethodInfo method = entry.Value.GetMethod("get_" + variable.Key);
                            if (method != null)
                            {
                                Type returnType = method.ReturnType;
                                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    returnType = Nullable.GetUnderlyingType(returnType);
                                }

                                if (returnType == typeof(string))
                                {
                                    Console.WriteLine("  * {TXT:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                                else if (returnType == typeof(int))
                                {
                                    Console.WriteLine("  * {INT:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                                else if (returnType == typeof(bool))
                                {
                                    Console.WriteLine("  * {BOOL:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                                else if (returnType == typeof(decimal) || returnType == typeof(double) || returnType == typeof(long))
                                {
                                    Console.WriteLine("  * {DEC:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void TestGenerateCottle()
        {
            foreach (KeyValuePair<string, Type> entry in Events.TYPES.OrderBy(i => i.Key))
            {
                Console.WriteLine("# " + entry.Key + " event");
                Console.WriteLine(Events.DESCRIPTIONS[entry.Key] + ".");

                IDictionary<string, string> variables;
                if (Events.VARIABLES.TryGetValue(entry.Key, out variables))
                {
                    if (variables.Count == 0)
                    {
                        Console.WriteLine("This event has no variables.");
                    }
                    else
                    {
                        Console.WriteLine("Information about this event is available under the `event` object.\n\n");
                        foreach (KeyValuePair<string, string> variable in Events.VARIABLES[entry.Key])
                        {
                            Console.WriteLine("  * `" + variable.Key + "` " + variable.Value + "\n");
                        }
                    }
                }
                Console.WriteLine();
            }
        }

        [TestMethod]
        public void TestGenerateWikiEvents()
        {
            foreach (KeyValuePair<string, Type> entry in Events.TYPES.OrderBy(i => i.Key))
            {
                List<string> output = new List<string>();
                Console.WriteLine("# " + entry.Key + " event");
                output.Add(Events.DESCRIPTIONS[entry.Key] + ".");

                IDictionary<string, string> variables;
                if (Events.VARIABLES.TryGetValue(entry.Key, out variables))
                {
                    if (variables.Count == 0)
                    {
                        output.Add("This event has no variables.");
                    }
                    else
                    {
                        output.Add("When using this event in the [Speech responder](Speech-Responder) the information about this event is available under the `event` object.  The available variables are as follows");
                        output.Add("");
                        output.Add("");
                        foreach (KeyValuePair<string, string> variable in Events.VARIABLES[entry.Key])
                        {
                            output.Add("  * `" + variable.Key + "` " + variable.Value);
                            output.Add("");
                        }

                        output.Add("When using this event in VoiceAttack the information about this event is available as follows");
                        output.Add("");
                        output.Add("");
                        foreach (KeyValuePair<string, string> variable in variables.OrderBy(i => i.Key))
                        {
                            System.Reflection.MethodInfo method = entry.Value.GetMethod("get_" + variable.Key);
                            if (method != null)
                            {
                                Type returnType = method.ReturnType;
                                if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    returnType = Nullable.GetUnderlyingType(returnType);
                                }

                                if (returnType == typeof(string))
                                {
                                    output.Add("  * `{TXT:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "}` " + variable.Value);
                                }
                                else if (returnType == typeof(int))
                                {
                                    output.Add("  * `{INT:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "}` " + variable.Value);
                                }
                                else if (returnType == typeof(bool))
                                {
                                    output.Add("  * `{BOOL:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "}` " + variable.Value);
                                }
                                else if (returnType == typeof(decimal) || returnType == typeof(double) || returnType == typeof(long))
                                {
                                    output.Add("  * `{DEC:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "}` " + variable.Value);
                                }
                            }
                        }
                    }
                }
                output.Add("");
                Directory.CreateDirectory(@"Wiki\events\");
                File.WriteAllLines(@"Wiki\events\" + entry.Key.Replace(" ", "-") + "-event.md", output);
            }
        }

        [TestMethod]
        public void TestGenerateWikiEventsList()
        {

            List<string> output = new List<string>();

            // This is the header row for the index of events
            output.Add(
                "EDDI generates a large number of events, triggered from changes in-game as well as from a number of external sources (e.g. Galnet RSS feed).  " +
                "A brief description of all available events is below, along with a link to more detailed information about each event:");
            output.Add("");

            // This is the list of events in markdown format
            foreach (KeyValuePair<string, Type> entry in Events.TYPES.OrderBy(i => i.Key))
            {
                output.Add("## [" + entry.Key + "](" + entry.Key.Replace(" ", "-") + "-event)");
                output.Add(Events.DESCRIPTIONS[entry.Key] + ".");
                output.Add("");
            }
            Directory.CreateDirectory(@"Wiki\");
            File.WriteAllLines(@"Wiki\Events.md", output);
        }
    }
}
