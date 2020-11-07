using EddiEvents;
using EddiSpeechResponder.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cottle.Stores;

namespace GeneratorTests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod, TestCategory("DocGen")]
        public void TestGenerateWikiEvents()
        {
            foreach (KeyValuePair<string, Type> entry in Events.TYPES.OrderBy(i => i.Key))
            {
                List<string> output = new List<string>
                {
                    Events.DESCRIPTIONS[entry.Key] + "."
                };

                if (Events.VARIABLES.TryGetValue(entry.Key, out IDictionary<string, string> variables))
                {
                    if (variables.Count == 0)
                    {
                        output.Add("This event has no variables.");
                        output.Add("To respond to this event in VoiceAttack, create a command entitled ((EDDI " + entry.Key.ToLowerInvariant() + ")).");
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

                        output.Add("To respond to this event in VoiceAttack, create a command entitled ((EDDI " + entry.Key.ToLowerInvariant() + ")). The event information can be accessed using the following VoiceAttack variables");
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

        [TestMethod, TestCategory("DocGen")]
        public void TestGenerateWikiEventsList()
        {
            List<string> output = new List<string>
            {

                // This is the header row for the index of events
                "EDDI generates a large number of events, triggered from changes in-game as well as from a number of external sources (e.g. Galnet RSS feed).  " +
                "A brief description of all available events is below, along with a link to more detailed information about each event:",
                ""
            };

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

        [TestMethod, TestCategory("DocGen")]
        // Generates the list of keywords used by the Cottle grammar "SpeechResponder\Cottle.xshd".
        // Paste the output into the "Custom properties" section of that file.
        public void TestGenerateEventVariables()
        {
            SortedSet<string> eventVars = new SortedSet<string>();
            foreach (IDictionary<string, string> variableList in Events.VARIABLES.Values)
            {
                foreach (string variableName in variableList.Keys)
                {
                    eventVars.Add(variableName);
                }
            }

            string output = "      <Word>" + string.Join("</Word>\r\n      <Word>", eventVars) + " </Word>\r\n";
            Directory.CreateDirectory(@"Cottle\");
            File.WriteAllText(@"Cottle\Custom keywords.txt", output);
        }

        // Generates a list of custom script resolver functions in the file `Help.md`.
        [TestMethod, TestCategory("DocGen")]
        public void TestGenerateFunctionsHelp()
        {
            List<string> output = new List<string>();
            output.Add(@"
# Templating with EDDI

EDDI's speech responder uses Cottle for templating.  Cottle has a number of great features, including:

* Ability to set and update variables, including arrays
* Loops
* Conditionals
* Subroutines

Information on how to write Cottle templates is available at https://cottle.readthedocs.io/en/stable/, and EDDI's default templates use a lot of the functions available.

## State Variables

Cottle does not retain state between templates, but EDDI provides a way of doing this with state variables.  State variables are provided to each Cottle template, and templates can set state variables that will be made available in future templates.

State variables are available for individual templates in the 'state' object.  Note that state variables are not persistent, and the state is empty whenever EDDI restarts.  Also, because EDDI responders run asynchronously and concurrently there is no guarantee that, for example, the speech responder for an event will finish before the VoiceAttack responder for an event starts (or vice versa).

## Context

EDDI uses the idea of context to attempt to keep track of what it is talking about.  This can enhance the experience when used with VoiceAttack by allowing repetition and more detailed information to be provided.

## EDDI Functions

In addition to the basic Cottle features EDDI has a number of features that provide added functionality and specific information for Elite: Dangerous.  Details of these functions are as follows:
");

            // Prepare functions
            var functions = new List<ICustomFunction>();
            var resolver = new ScriptResolver(null);
            var store = new BuiltinStore();
            var assy = Assembly.GetAssembly(typeof(ScriptResolver));
            foreach (var type in assy.GetTypes()
                .Where(t => t.IsClass && t.GetInterface(nameof(ICustomFunction)) != null))
            {
                var function = (ICustomFunction)(type.GetConstructor(Type.EmptyTypes) != null
                    ? Activator.CreateInstance(type) :
                    Activator.CreateInstance(type, resolver,  store));

                if (function != null)
                {
                    functions.Add(function);
                }
            }

            // Write results in alphabetical order (except exclude functions that we've flagged as hidden)
            functions = functions.OrderBy(f => f.name).ToList();
            foreach (var function in functions)
            {
                if (function.Category != FunctionCategory.Hidden)
                {
                    output.Add($"### {function.name}()");
                    output.Add(function.description);
                    output.Add("");
                }
            }

            Directory.CreateDirectory(@"Wiki\");
            File.WriteAllLines(@"Wiki\Functions.md", output);
        }
    }
}
