using Cottle.Stores;
using EddiEvents;
using EddiSpeechResponder.Service;
using EddiVoiceAttackResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace GeneratorTests
{
    [TestClass]
    public class GeneratorTests
    {
        private string initialCurrentDirectory;

        [TestInitialize]
        public void SetOutputDirectory()
        {
            initialCurrentDirectory = Directory.GetCurrentDirectory();
            string newCurrentDirectory = initialCurrentDirectory.Replace(@"Tests\", "");
            Directory.SetCurrentDirectory(newCurrentDirectory);
        }

        [TestCleanup]
        public void RestoreOutuptDirectory()
        {
            Directory.SetCurrentDirectory(initialCurrentDirectory);
        }

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
                        output.Add("When using this event in the [Speech responder](Speech-Responder) the information about this event is available under the `event` object.  The available variables are as follows:");
                        output.Add("");
                        output.Add("");
                        foreach (KeyValuePair<string, string> variable in Events.VARIABLES[entry.Key])
                        {
                            output.Add("  * `" + variable.Key + "` " + variable.Value);
                            output.Add("");
                        }

                        output.Add("To respond to this event in VoiceAttack, create a command entitled ((EDDI " + entry.Key.ToLowerInvariant() + ")). VoiceAttack variables will be generated to allow you to access the event information.");
                        output.Add("Where values are indexed (the compartments on a ship for example), the zero-based index will be represented by '*\\<index\\>*' and a value ending in 'entries' will identify the total number of entries in that index. For example, if index values of 0 and 1 are available then the value of the corresponding 'entries' variable will be 2.");
                        output.Add("The following VoiceAttack variables are available for this event:");
                        output.Add("");
                        output.Add("");
                        List<VoiceAttackVariable> setVars = new List<VoiceAttackVariable>();
                        VoiceAttackVariables.PrepareEventVariables(entry.Key, $"EDDI {entry.Key.ToLowerInvariant()}", entry.Value, ref setVars);
                        foreach (var variable in setVars)
                        {
                            // Get descriptions for our top level variables
                            if (variable.IsTopLevel)
                            {
                                string lastWord = variable.Key.Split(' ').Last();
                                foreach (KeyValuePair<string, string> variableDescription in Events.VARIABLES[entry.Key])
                                {
                                    if (variableDescription.Key == lastWord)
                                    {
                                        variable.Value = variableDescription.Value;
                                        break;
                                    }
                                }
                            }
                        }
                        void WriteVariableToOutput(VoiceAttackVariable variable)
                        {
                            if (variable.Type == typeof(string))
                            {
                                output.Add("  * {TXT:" + variable.Key + "} " + variable.Value);
                            }
                            else if (variable.Type == typeof(int))
                            {
                                output.Add("  * {INT:" + variable.Key + "} " + variable.Value);
                            }
                            else if (variable.Type == typeof(bool))
                            {
                                output.Add("  * {BOOL:" + variable.Key + "} " + variable.Value);
                            }
                            else if (variable.Type == typeof(decimal))
                            {
                                output.Add("  * {DEC:" + variable.Key + "} " + variable.Value);
                            }
                            else if (variable.Type == typeof(DateTime))
                            {
                                output.Add("  * {DATE:" + variable.Key + "} " + variable.Value);
                            }
                        }
                        foreach (var variable in setVars.Where(v => v.Value != null).OrderBy(i => i.Key))
                        {
                            // Write variables with descriptions first
                            WriteVariableToOutput(variable);
                        }
                        foreach (var variable in setVars.Where(v => v.Value == null).OrderBy(i => i.Key))
                        {
                            // Write variables without descriptions second
                            WriteVariableToOutput(variable);
                        }
                        output.Add("");
                        output.Add("");
                    }
                    output.Add("For more details on VoiceAttack integration, see https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration.");
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
            // Prepare our functions
            var functionsList = new List<ICustomFunction>();
            var resolver = new ScriptResolver(null);
            var store = new BuiltinStore();
            var assy = Assembly.GetAssembly(typeof(ScriptResolver));
            foreach (var type in assy.GetTypes()
                .Where(t => t.IsClass && t.GetInterface(nameof(ICustomFunction)) != null))
            {
                var function = (ICustomFunction)(type.GetConstructor(Type.EmptyTypes) != null
                    ? Activator.CreateInstance(type) :
                    Activator.CreateInstance(type, resolver, store));

                if (function != null)
                {
                    functionsList.Add(function);
                }
            }

            // Organize functions in alphabetical order (except exclude functions that we've flagged as hidden)
            functionsList = functionsList
                .Where(f => f.Category != FunctionCategory.Hidden)
                .OrderBy(f => f.name)
                .ToList();

            // Prepare Help.md
            List<string> help = new List<string>();
            help.Add("");
            help.Add(EddiSpeechResponder.Properties.CustomFunctions_Untranslated.HelpHeader);
            help.Add("");

            foreach (var function in functionsList)
            {
                help.Add($"### {function.name}()");
                help.Add("");
                help.Add(function.description);
                help.Add("");
            }

            // Prepare Functions.md
            List<string> functions = new List<string>();
            functions.Add("");
            functions.Add(EddiSpeechResponder.Properties.CustomFunctions_Untranslated.FunctionsHeader);
            functions.Add("");

            functionsList = functionsList.OrderBy(f => f.name).ToList();
            foreach (var function in functionsList)
            {
                functions.Add($"* {function.name}()");
            }

            // Make sure that a Wiki directory exists
            Directory.CreateDirectory(@"Wiki\");

            // Write our results
            File.WriteAllLines(@"Help.md", help);
            File.WriteAllLines(@"Wiki\Help.md", help);
            File.WriteAllLines(@"Wiki\Functions.md", functions);
        }
    }
}
