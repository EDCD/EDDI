using Cottle.Documents;
using Cottle.Functions;
using Cottle.Stores;
using EDDI;
using EliteDangerousSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class TemplateTest
    {
        [TestMethod]
        public void TestSimpleTemplate()
        {
            var document = new SimpleDocument(@"Hello {name}!");
            var store = new BuiltinStore();
            store["name"] = "world";
            var result = document.Render(store);
            Assert.AreEqual("Hello world!", result);
        }

        [TestMethod]
        public void TestFunctionalTemplate()
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
        public void TestConditionalTemplate()
        {
        }

        [TestMethod]
        public void TestOneOfTemplate()
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
        public void TestResolver1()
        {
            ScriptResolver resolver = new ScriptResolver();
            Dictionary<string, Cottle.Value> dict = new Dictionary<string, Cottle.Value>();
            dict.Add("name", "world");
            string result = resolver.resolve("Hello {name}", dict);
            Assert.AreEqual("Hello world", result);
        }
    }
}
