using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackResponder;
using System;
using EddiEvents;
using System.Collections.Generic;
using System.Linq;

namespace Tests
{
    [TestClass]
    public class GeneratorTests
    {
        [TestMethod]
        public void TestGenerate()
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
                        Console.WriteLine("Variables set with this events are as follows:");
                        Console.WriteLine();
                        foreach (KeyValuePair<string, string> variable in variables.OrderBy(i => i.Key))
                        {
                            System.Reflection.MethodInfo method = entry.Value.GetMethod("get_" + variable.Key);
                            if (method != null)
                            {
                                if (method.ReturnType == typeof(string))
                                {
                                    Console.WriteLine("    * {TXT:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                                else if (method.ReturnType == typeof(int))
                                {
                                    Console.WriteLine("    * {INT:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                                else if (method.ReturnType == typeof(bool))
                                {
                                    Console.WriteLine("    * {BOOL:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                                else if (method.ReturnType == typeof(decimal) || method.ReturnType == typeof(double) || method.ReturnType == typeof(long))
                                {
                                    Console.WriteLine("    * {DEC:EDDI " + entry.Key.ToLowerInvariant() + " " + variable.Key + "} " + variable.Value);
                                }
                            }
                        }
                    }
                }
                Console.WriteLine();
            }
        }
    }
}
