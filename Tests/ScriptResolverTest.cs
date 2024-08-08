using Cottle;
using EddiSpeechResponder;
using EddiSpeechResponder.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace UnitTests
{
    [TestClass]
    public class ScriptResolverTest : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private string Render ( string template, Dictionary<Value, Value> vars )
        {
            var document = Document.CreateDefault( template ).DocumentOrThrow;
            var store = Context.CreateBuiltin( vars );
            return document.Render(store);
        }

        [TestMethod]
        public void TestTemplateSimple()
        {
            var template = @"Hello {name}!";
            var vars = new Dictionary<Value, Value> { [ "name" ] = "world" };

            var result = Render(template, vars);
            Assert.AreEqual("Hello world!", result);
        }

        [TestMethod]
        public void TestTemplateFunctional()
        {
            var template = @"You are entering the {P(system)} system.";
            var vars = new Dictionary<Value, Value>
            {
                ["system"] = "Alrai",
                ["P"] = Value.FromFunction(ScriptResolver.GetCustomFunctions().FirstOrDefault(f => f.name == "P")?.function)
            };

            var result = Render( template, vars );
            Assert.AreEqual("You are entering the <phoneme alphabet=\"ipa\" ph=\"ˈalraɪ\">Alrai</phoneme> system.", result);
        }

        [TestMethod]
        public void TestTemplateConditional()
        {
            var template = @"{if value = 1:foo|else:{if value = 2:bar|else:baz}}";
            var vars = new Dictionary<Value, Value> { [ "value" ] = 1 };
            Assert.AreEqual("foo", Render( template, vars ) );
            vars[ "value" ] = 2;
            Assert.AreEqual("bar", Render( template, vars ) );
            vars[ "value" ] = 3;
            Assert.AreEqual("baz", Render( template, vars ) );
        }

        [TestMethod]
        public void TestTemplateOneOf()
        {
            var template = "{set result to OneOf(\"a\", \"b\", \"c\", \"d\", null)} The letter is {OneOf(result)}.";
            var vars = new Dictionary<Value, Value>
            {
                ["system"] = "Alrai",
                ["OneOf"] = Value.FromFunction(ScriptResolver.GetCustomFunctions().FirstOrDefault(f => f.name == "OneOf")?.function)
            };

            List<string> results = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(Render(template, vars).Trim());
            }
            Assert.IsTrue(results.Contains(@"The letter is a."));
            results.RemoveAll(result => result == @"The letter is a.");
            Assert.IsTrue(results.Contains(@"The letter is b."));
            results.RemoveAll(result => result == @"The letter is b.");
            Assert.IsTrue(results.Contains(@"The letter is c."));
            results.RemoveAll(result => result == @"The letter is c.");
            Assert.IsTrue(results.Contains(@"The letter is d."));
            results.RemoveAll(result => result == @"The letter is d.");
            Assert.IsTrue(results.Contains(@"The letter is ."));
            results.RemoveAll(result => result == @"The letter is .");
            Assert.IsTrue(results.Count == 0);
        }

        [TestMethod]
        public void TestResolverSimple()
        {
            Dictionary<string, Script> scripts = new Dictionary<string, Script>
            {
                {"test", new Script("test", null, false, "Hello {name}")}
            };
            ScriptResolver resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple<Type, Value>> { ["name"] = new Tuple<Type, Value>(typeof(string), "world") };
            string result = resolver.resolveFromName("test", dict, true);
            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void TestResolverFunctions()
        {
            var scripts = new Dictionary<string, Script>
            {
                {"func", new Script("func", null, false, "Hello {name}")},
                {"test", new Script("test", null, false, "Well {F(\"func\")}")}
            };
            var resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple<Type, Value>> { ["name"] = new Tuple<Type, Value>(typeof(string), "world") };
            string result = resolver.resolveFromName("test", dict, true);
            Assert.AreEqual("Well Hello world", result);
        }

        [TestMethod]
        public void TestResolverNativeSetCustomFunction()
        {
            Dictionary<string, Script> scripts = new Dictionary<string, Script>
            {
                {"test", new Script("test", null, false, "{set x to \"Hello\"} {OneOf(\"{x} world\")}")}
            };
            ScriptResolver resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple < Type, Value >>();
            string result = resolver.resolveFromName("test", dict, true);
            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void TestResolverRecursedCustomFunctions()
        {
            Dictionary<string, Script> scripts = new Dictionary<string, Script>
            {
                {"test", new Script("test", null, false, "The letter is {OneOf(\"a\", F(\"func\"), \"{c}\")}.")},
                {"func", new Script("func", null, false, "b")}
            };
            ScriptResolver resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple<Type, Value>> { ["c"] = new Tuple<Type, Value>(typeof(string), "c") };

            List<string> results = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(resolver.resolveFromName("test", dict, true));
            }
            Assert.IsTrue(results.Contains(@"The letter is a."));
            results.RemoveAll(result => result == @"The letter is a.");
            Assert.IsTrue(results.Contains(@"The letter is b."));
            results.RemoveAll(result => result == @"The letter is b.");
            Assert.IsTrue(results.Contains(@"The letter is c."));
            results.RemoveAll(result => result == @"The letter is c.");
            Assert.IsTrue(results.Count == 0);
        }

        [TestMethod]
        public void TestResolverCallsign()
        {
            Assert.AreEqual(new Regex("[^a-zA-Z0-9]").Replace("a-b. c", "").ToUpperInvariant().Substring(0, 3), "ABC");
        }

        [TestMethod]
        public void TestUpgradeScript_FromDefault()
        {
            Script script = new Script("testScript", "Test script", false, "Test script", 3, "Test script");
            Script newDefaultScript = new Script("testScript", "Updated Test script Description", true, "Updated Test script", 3, "Updated Test script");

            Assert.IsTrue(script.Default);
            Assert.AreEqual(script.Name, newDefaultScript.Name);

            Assert.AreNotEqual(script.Description, newDefaultScript.Description);
            Assert.AreNotEqual(script.Responder, newDefaultScript.Responder);
            Assert.AreNotEqual(script.Value, newDefaultScript.Value);
            Assert.AreNotEqual(script.defaultValue, newDefaultScript.defaultValue);
            Assert.AreNotEqual(script.Priority, newDefaultScript.Priority);

            Script upgradedScript = Personality.UpgradeScript(script, newDefaultScript);

            Assert.IsTrue(upgradedScript.Default);

            Assert.AreEqual(newDefaultScript.Description, upgradedScript.Description);
            Assert.AreEqual(newDefaultScript.Responder, upgradedScript.Responder);
            Assert.AreEqual(newDefaultScript.Value, upgradedScript.Value);
            Assert.AreEqual(newDefaultScript.defaultValue, upgradedScript.defaultValue);
            Assert.AreEqual(newDefaultScript.Priority, upgradedScript.Priority);
        }

        [TestMethod]
        public void TestUpgradeScript_FromCustomized()
        {
            Script script = new Script("testScript", "Test script", true, "Test script customized", 4, "Test script");
            Script newDefaultScript = new Script("testScript", "Updated Test script Description", true, "Updated Test script", 3, "Updated Test script");

            Assert.IsFalse(script.Default);
            Assert.AreEqual(script.Name, newDefaultScript.Name);

            Assert.AreNotEqual(script.Description, newDefaultScript.Description);
            Assert.AreEqual(script.Responder, newDefaultScript.Responder);
            Assert.AreNotEqual(script.Value, newDefaultScript.Value);
            Assert.AreNotEqual(script.defaultValue, newDefaultScript.defaultValue);
            Assert.AreNotEqual(script.Priority, newDefaultScript.Priority);

            Script upgradedScript = Personality.UpgradeScript(script, newDefaultScript);

            Assert.IsFalse(upgradedScript.Default);

            Assert.AreEqual(newDefaultScript.Description, upgradedScript.Description);
            Assert.AreEqual(newDefaultScript.Responder, upgradedScript.Responder);
            Assert.AreNotEqual(newDefaultScript.Value, upgradedScript.Value);
            Assert.AreEqual(newDefaultScript.defaultValue, upgradedScript.defaultValue);
            Assert.AreNotEqual(newDefaultScript.Priority, upgradedScript.Priority);
        }

        [TestMethod]
        public void TestSetClipboard()
        {
            var testThread = new Thread(() =>
            {
                Dictionary<string, Script> scripts = new Dictionary<string, Script>
                {
                    {"test1", new Script("test1", null, false, @"{SetClipboard(""A"")}")},
                    {"test2", new Script("test2", null, false, @"{SetClipboard(""B"")}")},
                    {"test3", new Script("test3", null, false, @"{SetClipboard(""C"")}")},
                };
                ScriptResolver resolver = new ScriptResolver(scripts);
                var dict = new Dictionary<string, Tuple<Type, Value>>();

                resolver.resolveFromName("test1", dict, true);
                Assert.AreEqual("A", Clipboard.GetText());

                resolver.resolveFromName("test2", dict, true);
                Assert.AreEqual("B", Clipboard.GetText());

                resolver.resolveFromName("test3", dict, true);
                Assert.AreEqual("C", Clipboard.GetText());
            });
            if (!testThread.TrySetApartmentState(ApartmentState.STA))
            {
                Assert.Fail("Unable to set thread to single thread apartment (STA) mode");
            }
            testThread.Start();
            testThread.Join();
        }
    }
}
