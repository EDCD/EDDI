using Cottle.Documents;
using Cottle.Functions;
using Cottle.Stores;
using EddiSpeechResponder;
using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UnitTests
{
    [TestClass]
    public class ScriptResolverTest
    {
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
            var document = new SimpleDocument(@"You are entering the {System(system)} system.");
            var store = new BuiltinStore();
            store["System"] = new NativeFunction((values) =>
            {
                return Translations.StarSystem(values[0].AsString);
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
            var document = new SimpleDocument("The letter is {OneOf(\"a\", \"b\", \"c\", \"d\", null)}.");
            var store = new BuiltinStore();
            store["OneOf"] = new NativeFunction((values) =>
            {
                return values[random.Next(values.Count)];
            });
            store["system"] = "Alrai";
            List<string> results = new List<string>();
            for (int i = 0; i < 1000; i++)
            {
                results.Add(document.Render(store));
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
            Dictionary<string, Script> scripts = new Dictionary<string, Script>();
            scripts.Add("test", new Script("test", null, false, "Hello {name}"));
            ScriptResolver resolver = new ScriptResolver(scripts);
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>();
            dict["name"] = "world";
            string result = resolver.resolve("test", dict);
            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void TestResolverFunctions()
        {
            Dictionary<string, Script> scripts = new Dictionary<string, Script>();
            scripts.Add("func", new Script("func", null, false, "Hello {name}"));
            scripts.Add("test", new Script("test", null, false, "Well {F(\"func\")}"));
            ScriptResolver resolver = new ScriptResolver(scripts);
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>();
            dict["name"] = "world";
            string result = resolver.resolve("test", dict);
            Assert.AreEqual("Well Hello world", result);
        }

        [TestMethod]
        public void TestResolverCallsign()
        {
            Assert.AreEqual(new Regex("[^a-zA-Z0-9]").Replace("a-b. c", "").ToUpperInvariant().Substring(0, 3), "ABC");
        }
    }
}
