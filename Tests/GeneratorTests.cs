using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using EddiEvents;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Tests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void TestGenerateWikiEvents()
        {
            foreach (KeyValuePair<string, Type> entry in Events.TYPES.OrderBy(i => i.Key))
            {
                List<string> output = new List<string>();
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

                        output.Add("To use this event in VoiceAttack, create a command entitled ((EDDI " + entry.Key.ToLowerInvariant() + ")). The event information can be accessed using the following VoiceAttack variables");
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
