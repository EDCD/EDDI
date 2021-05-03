using Cottle.Builtins;
using Cottle.Documents;
using Cottle.Functions;
using Cottle.Settings;
using Cottle.Stores;
using EddiSpeechResponder;
using EddiSpeechResponder.Service;
using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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

        [TestMethod]
        public void TestTemplateSimple()
        {
            var document = new SimpleDocument(@"Hello {name}!");
            var store = new BuiltinStore();
            store["name"] = "world";
            var result = document.Render(store);
            Assert.AreEqual("Hello world!", result);
        }

        [TestMethod]
        public void TestTemplateFunctional()
        {
            var document = new SimpleDocument(@"You are entering the {P(system)} system.");
            var store = new BuiltinStore();
            store["P"] = new NativeFunction((values) =>
            {
                return Translations.GetTranslation(values[0].AsString);
            }, 1);
            store["system"] = "Alrai";
            var result = document.Render(store);
            Assert.AreEqual("You are entering the <phoneme alphabet=\"ipa\" ph=\"ˈalraɪ\">Alrai</phoneme> system.", result);
        }

        [TestMethod]
        public void TestTemplateConditional()
        {
            var document = new SimpleDocument("{if value = 1:foo|else:{if value = 2:bar|else:baz}}");
            var store = new BuiltinStore();
            store["value"] = 1;
            var result = document.Render(store);
            Assert.AreEqual("foo", result);
            store["value"] = 2;
            result = document.Render(store);
            Assert.AreEqual("bar", result);
            store["value"] = 3;
            result = document.Render(store);
            Assert.AreEqual("baz", result);
        }

        [TestMethod]
        public void TestTemplateOneOf()
        {
            Random random = new Random();
            var document = new SimpleDocument("{set result to OneOf(\"a\", \"b\", \"c\", \"d\", null)} The letter is {OneOf(result)}.");
            var store = new BuiltinStore();
            store["OneOf"] = new NativeFunction((values) =>
            {
                return values[random.Next(values.Count)];
            });
            List<string> results = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(document.Render(store).Trim());
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
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value> { ["name"] = "world" };
            string result = resolver.resolveFromName("test", dict, true);
            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void TestResolverFunctions()
        {
            Dictionary<string, Script> scripts = new Dictionary<string, Script>
            {
                {"func", new Script("func", null, false, "Hello {name}")},
                {"test", new Script("test", null, false, "Well {F(\"func\")}")}
            };
            ScriptResolver resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Cottle.Value> { ["name"] = "world" };
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
            var dict = new Dictionary<string, Cottle.Value>();
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
            var dict = new Dictionary<string, Cottle.Value> { ["c"] = "c" };

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
    }
}
