using Cottle;
using EddiSpeechResponder;
using EddiSpeechResponder.ScriptResolverService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class ScriptResolverTest : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private static string Render ( string template, Dictionary<Value, Value> vars )
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

            var results = new List<string>();
            for (var i = 0; i < 1000; i++)
            {
                results.Add(Render(template, vars).Trim());
            }
            Assert.Contains( @"The letter is a.", results );
            results.RemoveAll(result => result == @"The letter is a.");
            Assert.Contains( @"The letter is b.", results );
            results.RemoveAll(result => result == @"The letter is b.");
            Assert.Contains( @"The letter is c.", results );
            results.RemoveAll(result => result == @"The letter is c.");
            Assert.Contains( @"The letter is d.", results );
            results.RemoveAll(result => result == @"The letter is d.");
            Assert.Contains( @"The letter is .", results );
            results.RemoveAll(result => result == @"The letter is .");
            Assert.IsEmpty(results);
        }

        [TestMethod]
        public void TestResolverSimple()
        {
            var scripts = new Dictionary<string, Script>
            {
                {"test", new Script("test", null, false, "Hello {name}")}
            };
            var resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple<Type, Value>> { ["name"] = new(typeof(string), "world") };
            var result = resolver.resolveFromName("test", dict, true);
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
            var dict = new Dictionary<string, Tuple<Type, Value>> { ["name"] = new(typeof(string), "world") };
            var result = resolver.resolveFromName("test", dict, true);
            Assert.AreEqual("Well Hello world", result);
        }

        [TestMethod]
        public void TestResolverNativeSetCustomFunction()
        {
            var scripts = new Dictionary<string, Script>
            {
                {"test", new Script("test", null, false, "{set x to \"Hello\"} {OneOf(\"{x} world\")}")}
            };
            var resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple < Type, Value >>();
            var result = resolver.resolveFromName("test", dict, true);
            Assert.AreEqual("Hello world", result);
        }

        [TestMethod]
        public void TestResolverRecursedCustomFunctions()
        {
            var scripts = new Dictionary<string, Script>
            {
                {"test", new Script("test", null, false, "The letter is {OneOf(\"a\", F(\"func\"), \"{c}\")}.")},
                {"func", new Script("func", null, false, "b")}
            };
            var resolver = new ScriptResolver(scripts);
            var dict = new Dictionary<string, Tuple<Type, Value>> { ["c"] = new(typeof(string), "c") };

            var results = new List<string>();
            for (var i = 0; i < 1000; i++)
            {
                results.Add(resolver.resolveFromName("test", dict, true));
            }
            Assert.Contains( @"The letter is a.", results );
            results.RemoveAll(result => result == @"The letter is a.");
            Assert.Contains( @"The letter is b.", results);
            results.RemoveAll(result => result == @"The letter is b.");
            Assert.Contains( @"The letter is c.", results );
            results.RemoveAll(result => result == @"The letter is c.");
            Assert.IsEmpty(results);
        }

        [TestMethod]
        public void TestUpgradeScript_FromDefault()
        {
            var script = new Script("testScript", "Test script", false, "Test script", 3, "Test script");
            var newDefaultScript = new Script("testScript", "Updated Test script Description", true, "Updated Test script", 3, "Updated Test script");

            Assert.IsTrue(script.Default);
            Assert.AreEqual(script.Name, newDefaultScript.Name);

            Assert.AreNotEqual(script.Description, newDefaultScript.Description);
            Assert.AreNotEqual(script.Responder, newDefaultScript.Responder);
            Assert.AreNotEqual(script.Value, newDefaultScript.Value);
            Assert.AreNotEqual(script.defaultValue, newDefaultScript.defaultValue);
            Assert.AreNotEqual(script.Priority, newDefaultScript.Priority);

            var upgradedScript = Personality.UpgradeScript(script, newDefaultScript);

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
            var script = new Script("testScript", "Test script", true, "Test script customized", 4, "Test script");
            var newDefaultScript = new Script("testScript", "Updated Test script Description", true, "Updated Test script", 3, "Updated Test script");

            Assert.IsFalse(script.Default);
            Assert.AreEqual(script.Name, newDefaultScript.Name);

            Assert.AreNotEqual(script.Description, newDefaultScript.Description);
            Assert.AreEqual(script.Responder, newDefaultScript.Responder);
            Assert.AreNotEqual(script.Value, newDefaultScript.Value);
            Assert.AreNotEqual(script.defaultValue, newDefaultScript.defaultValue);
            Assert.AreNotEqual(script.Priority, newDefaultScript.Priority);

            var upgradedScript = Personality.UpgradeScript(script, newDefaultScript);

            Assert.IsFalse(upgradedScript.Default);

            Assert.AreEqual(newDefaultScript.Description, upgradedScript.Description);
            Assert.AreEqual(newDefaultScript.Responder, upgradedScript.Responder);
            Assert.AreNotEqual(newDefaultScript.Value, upgradedScript.Value);
            Assert.AreEqual(newDefaultScript.defaultValue, upgradedScript.defaultValue);
            Assert.AreNotEqual(newDefaultScript.Priority, upgradedScript.Priority);
        }

        [TestMethod]
        public void TestUpgradeScript_ClonesDefaultWhenPersonalityScriptIsMissing()
        {
            var newDefaultScript = new Script("testScript", "Updated Test script Description", true, "Updated Test script", 3, "Updated Test script");

            var upgradedScript = Personality.UpgradeScript(null, newDefaultScript);

            Assert.AreNotSame(newDefaultScript, upgradedScript);
            Assert.AreEqual(newDefaultScript.Name, upgradedScript.Name);
            Assert.AreEqual(newDefaultScript.Value, upgradedScript.Value);
        }

        [TestMethod]
        public void TestSetClipboard()
        {
            var testThread = new Thread(() =>
            {
                var scripts = new Dictionary<string, Script>
                {
                    {"test1", new Script("test1", null, false, @"{SetClipboard(""A"")}")},
                    {"test2", new Script("test2", null, false, @"{SetClipboard(""B"")}")},
                    {"test3", new Script("test3", null, false, @"{SetClipboard(""C"")}")},
                };
                var resolver = new ScriptResolver(scripts);
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

        [ TestMethod ]
        [DataRow( "{", "", 0, 1, "{{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 0 }\r\n{set i to i + 1}" )]
        [DataRow( "", "}", 1, 1, "{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 0 }\r\n{set i to i + 1}}\r\n{set j to j + 2}\r\n{_ End of prepended script 1 }\r\n{set i to i + 1}" )]
        [DataRow( "{", "", 2, 2, "{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 0 }\r\n{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 1 }\r\n{set i to i + 1}\r\n{{set j to j + 2}\r\n{_ End of prepended script 2 }\r\n{set i to i + 1}" )]
        [DataRow( "", "}", 1, 2, "{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 0 }\r\n{set i to i + 1}\r\n{set j to j + 2}}\r\n{_ End of prepended script 1 }\r\n{set i to i + 1}" )]
        [DataRow( "", "", 0, 0, "{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 0 }\r\n{set i to i + 1}" )]
        [DataRow( "", "", 1, 0, "{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 0 }\r\n{set i to i + 1}\r\n{set j to j + 2}\r\n{_ End of prepended script 1 }\r\n{set i to i + 1}" )]
        public void TestTemplateBuilder (string flaw_start, string flaw_end, int flawedTemplateNumber, int flawedTemplateLine, string expectedOutout )
        {
            var templateBuilder = new TemplateBuilder ();
            int i;
            for ( i = 0; i < (flawedTemplateNumber + 1); i++ )
            {
                templateBuilder.Append( i.ToString(), 
                    ( i == flawedTemplateNumber && flawedTemplateLine == 1 ? flaw_start : "" ) + 
                    @"{set i to i + 1}" +
                    ( i == flawedTemplateNumber && flawedTemplateLine == 1 ? flaw_end : "" ) + 
                    Environment.NewLine +
                    ( i == flawedTemplateNumber && flawedTemplateLine == 2 ? flaw_start : "" ) + 
                    "{set j to j + 2}" +
                    ( i == flawedTemplateNumber && flawedTemplateLine == 2 ? flaw_end : "" ), true );
            }
            templateBuilder.Append( i.ToString(), @"{set i to i + 1}", false );
            var combinedTemplates = templateBuilder.Render();

            Assert.AreEqual(expectedOutout, combinedTemplates);

            // Verify that error locations are captured correctly
            if ( flaw_start != "" || flaw_end != "" )
            {
                var e = Assert.ThrowsExactly<Cottle.Exceptions.ParseException>( () => Render( combinedTemplates, new Dictionary<Value, Value>() ) );
                templateBuilder.FetchTemplateItemFromOffset( combinedTemplates, e.LocationStart, out var scriptName, out var scriptLine );
                Assert.AreEqual( flawedTemplateNumber.ToString(), scriptName );
                Assert.AreEqual( flawedTemplateLine, scriptLine );
            }
        }
    }
}
