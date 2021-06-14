using Cottle.Stores;
using EddiEvents;
using EddiSpeechResponder.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Utilities;

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
        public void RestoreOutputDirectory()
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
                    Events.DESCRIPTIONS[entry.Key] + ".",
                    ""
                };

                var vars = new MetaVariables(entry.Value).Results;
                var CottleVars = vars.AsCottleVariables();
                var VoiceAttackVars = vars.AsVoiceAttackVariables("EDDI", entry.Key);

                if (!vars.Any())
                {
                    output.Add("This event has no variables.");
                    output.Add("To respond to this event in VoiceAttack, create a command entitled ((EDDI " + entry.Key.ToLowerInvariant() + ")).");
                    output.Add("");
                }

                if (vars.Any(v => v.keysPath.Any(k => k.Contains(@"<index"))))
                {
                    output.Add("Where values are indexed (the compartments on a ship for example), the index will be represented by '*\\<index\\>*'.");
                    if (VoiceAttackVars.Any(v => v.key.Contains(@"<index")))
                    {
                        output.Add("For VoiceAttack, a variable with the root name of the indexed array shall identify the total number of entries in the array. For example, if compartments 1 and 2 are available then the value of the corresponding 'compartments' variable will be 2.");
                    }
                    output.Add("");
                }

                if (CottleVars.Any())
                {
                    output.Add("When using this event in the [Speech responder](Speech-Responder) the information about this event is available under the `event` object.  The available variables are as follows:");
                    output.Add("");
                    output.Add("");

                    foreach (var cottleVariable in CottleVars.OrderBy(i => i.key))
                    {
                        var description = !string.IsNullOrEmpty(cottleVariable.description) ? $" - {cottleVariable.description}" : "";
                        output.Add($"  - *{{event.{cottleVariable.key}}}* {description}");
                        output.Add("");
                    }
                }

                if (VoiceAttackVars.Any())
                {
                    output.Add("");
                    output.Add("To respond to this event in VoiceAttack, create a command entitled ((EDDI " + entry.Key.ToLowerInvariant() + ")). VoiceAttack variables will be generated to allow you to access the event information.");
                    output.Add("");
                    output.Add("The following VoiceAttack variables are available for this event:");
                    output.Add("");
                    output.Add("");

                    void WriteVariableToOutput(VoiceAttackVariable variable)
                    {
                        var description = !string.IsNullOrEmpty(variable.description) ? $" - {variable.description}" : "";
                        if (variable.variableType == typeof(string))
                        {
                            output.Add($"  - *{{TXT:{variable.key}}}* {description}");
                        }
                        else if (variable.variableType == typeof(int))
                        {
                            output.Add($"  - *{{INT:{variable.key}}}* {description}");
                        }
                        else if (variable.variableType == typeof(bool))
                        {
                            output.Add($"  - *{{BOOL:{variable.key}}}* {description}");
                        }
                        else if (variable.variableType == typeof(decimal))
                        {
                            output.Add($"  - *{{DEC:{variable.key}}}* {description}");
                        }
                        else if (variable.variableType == typeof(DateTime))
                        {
                            output.Add($"  - *{{DATE:{variable.key}}}* {description}");
                        }
                        output.Add("");
                    }

                    foreach (var variable in VoiceAttackVars.OrderBy(i => i.key))
                    {
                        WriteVariableToOutput(variable);
                    }

                    output.Add("");
                    output.Add("For more details on VoiceAttack integration, see https://github.com/EDCD/EDDI/wiki/VoiceAttack-Integration.");
                    output.Add("");
                }
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

            foreach (var type in Events.TYPES)
            {
                var vars = new MetaVariables(type.Value).Results;
                foreach (var key in vars.SelectMany(v => v.keysPath))
                {
                    eventVars.Add(key);
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
