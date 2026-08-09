using Cottle;
using EddiScriptResolverService;
using EddiSpeechResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
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

        private static string ResolveScript ( string script, Dictionary<string, Script> scripts = null )
        {
            var resolver = new ScriptResolver( scripts ?? new Dictionary<string, Script>
            {
                [ "test" ] = new( "test", null, false, script )
            } );
            return resolver.resolveFromName( "test", new Dictionary<string, Tuple<Type, Value>>(), true );
        }

        private const string DefaultApproximateScript =
            "{set number to NumberDetails(args.number)}\r\n" +
            "{set word_minus to \"minus \"}\r\n" +
            "{set word_one to \"one\"}\r\n" +
            "{set word_just_over to \"just over \"}\r\n" +
            "{set word_over to \"over \"}\r\n" +
            "{set word_well_over to \"well over \"}\r\n" +
            "{set word_nearly to \"nearly \"}\r\n" +
            "{set word_around to \"around \"}\r\n" +
            "{set word_and_a_half to \" and a half\"}\r\n" +
            "{set magnitude to \"\"}\r\n" +
            "{if number.magnitudename:\r\n" +
            "    {set magnitude to cat(\" \", number.magnitudename)}\r\n" +
            "}\r\n" +
            "{set sign to \"\"}\r\n" +
            "{if number.isnegative:\r\n" +
            "    {set sign to word_minus}\r\n" +
            "}\r\n" +
            "{set quantity to number.quantity}\r\n" +
            "{if number.number = 1 && (number.format = \"nearly_half\" || number.format = \"around_half\" || number.format = \"over_half\"):\r\n" +
            "    {set quantity to word_one}\r\n" +
            "}\r\n" +
            "{if number.format = \"zero\" || number.format = \"small\" || number.format = \"verbatim\":\r\n" +
            "    {number.quantity}\r\n" +
            "|elif number.format = \"short_decimal\" || number.format = \"integer_mantissa\":\r\n" +
            "    {sign}{number.quantity}{magnitude}\r\n" +
            "|elif number.format = \"just_over\":\r\n" +
            "    {sign}{word_just_over}{number.quantity}{magnitude}\r\n" +
            "|elif number.format = \"over\":\r\n" +
            "    {sign}{word_over}{number.quantity}{magnitude}\r\n" +
            "|elif number.format = \"well_over\":\r\n" +
            "    {sign}{word_well_over}{number.quantity}{magnitude}\r\n" +
            "|elif number.format = \"nearly\":\r\n" +
            "    {sign}{word_nearly}{number.quantity}{magnitude}\r\n" +
            "|elif number.format = \"nearly_half\":\r\n" +
            "    {sign}{word_nearly}{quantity}{word_and_a_half}{magnitude}\r\n" +
            "|elif number.format = \"around_half\":\r\n" +
            "    {sign}{word_around}{quantity}{word_and_a_half}{magnitude}\r\n" +
            "|elif number.format = \"over_half\":\r\n" +
            "    {sign}{word_over}{quantity}{word_and_a_half}{magnitude}\r\n" +
            "|elif number.format = \"well_over_half\":\r\n" +
            "    {sign}{word_nearly}{number.number + 1}{magnitude}\r\n" +
            "|else:\r\n" +
            "    {number.quantity}\r\n" +
            "}";

        private static Dictionary<string, Script> DefaultApproximateScripts (
            string testScriptName,
            string testScript,
            string approximateScript = DefaultApproximateScript )
        {
            return new Dictionary<string, Script>
            {
                [testScriptName] = new(testScriptName, null, false, testScript),
                ["Approximate"] = new("Approximate", null, false, approximateScript, null, approximateScript)
            };
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
        public void PreferredPronunciationAliasMatchesLegacyFunction ()
        {
            Assert.AreEqual(
                ResolveScript( "{P('Alrai')}" ),
                ResolveScript( "{PronounceForContext('Alrai')}" ) );

            Assert.AreEqual(
                ResolveScript( "{P('Aisling Duval', 'power')}" ),
                ResolveScript( "{PronounceForContext('Aisling Duval', 'power')}" ) );
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
        public void PreferredInvokeScriptAliasMatchesLegacyFunction ()
        {
            var scripts = new Dictionary<string, Script>
            {
                [ "func" ] = new( "func", null, false, "Hello" ),
                [ "test" ] = new( "test", null, false, "Well {InvokeScript(\"func\")}" )
            };

            Assert.AreEqual( "Well Hello", ResolveScript( scripts[ "test" ].Value, scripts ) );
        }

        [TestMethod]
        public void TestResolverFunctionWithArgumentMap()
        {
            var scripts = new Dictionary<string, Script>
            {
                ["func"] = new("func", null, false, "{args.foo} {args.bar}"),
                ["test"] = new("test", null, false, "Well {F(\"func\", [\"foo\": \"hello\", \"bar\": 3])}")
            };
            var resolver = new ScriptResolver(scripts);

            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual("Well hello 3", result);
        }

        [TestMethod]
        public void PreferredInvokeScriptAliasPassesArgumentMap ()
        {
            var scripts = new Dictionary<string, Script>
            {
                [ "func" ] = new( "func", null, false, "{args.foo} {args.bar}" ),
                [ "test" ] = new( "test", null, false, "Well {InvokeScript(\"func\", [\"foo\": \"hello\", \"bar\": 3])}" )
            };

            Assert.AreEqual( "Well hello 3", ResolveScript( scripts[ "test" ].Value, scripts ) );
        }

        [TestMethod]
        public void TestResolverFunctionArgumentMapShadowsNestedArgs()
        {
            var scripts = new Dictionary<string, Script>
            {
                ["inner"] = new("inner", null, false, "{args.value}"),
                ["outer"] = new("outer", null, false, "{set before to args.value}{set nested to F(\"inner\", [\"value\": \"inner\"])}{before}/{nested}/{args.value}"),
                ["test"] = new("test", null, false, "{F(\"outer\", [\"value\": \"outer\"])}")
            };
            var resolver = new ScriptResolver(scripts);

            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual("outer/inner/outer", result);
        }

        [TestMethod]
        public async Task InvokeScriptArgumentMapsRemainIsolatedAcrossConcurrentResolvers ()
        {
            static string ResolveWithValue ( string value )
            {
                var scripts = new Dictionary<string, Script>
                {
                    [ "inner" ] = new( "inner", null, false, "{args.value}:{shared}:state={if state:ok|else:missing}" ),
                    [ "outer" ] = new( "outer", null, false, "{set before to args.value}{set nested to InvokeScript(\"inner\", [\"value\": args.value])}{before}/{nested}/{args.value}/{shared}" ),
                    [ "test" ] = new( "test", null, false, "{InvokeScript(\"outer\", [\"value\": root])}" )
                };
                var resolver = new ScriptResolver( scripts );
                var dict = new Dictionary<string, Tuple<Type, Value>>
                {
                    [ "root" ] = new( typeof( string ), value ),
                    [ "shared" ] = new( typeof( string ), value )
                };

                return resolver.resolveFromName( "test", dict, true );
            }

            var alpha = Task.Run( () => ResolveWithValue( "alpha" ) );
            var beta = Task.Run( () => ResolveWithValue( "beta" ) );
            var results = await Task.WhenAll( alpha, beta );

            var expected = new[]
                {
                    "alpha/alpha:alpha:state=ok/alpha/alpha",
                    "beta/beta:beta:state=ok/beta/beta"
                };
            CollectionAssert.AreEquivalent(
                expected,
                results );
        }

        [TestMethod]
        public void TestResolverFunctionRejectsNonMapArguments()
        {
            var scripts = new Dictionary<string, Script>
            {
                ["func"] = new("func", null, false, "{args.foo}"),
                ["test"] = new("test", null, false, "{F(\"func\", 1)}")
            };
            var resolver = new ScriptResolver(scripts);
            var result = resolver.resolveFromName( "test", new Dictionary<string, Tuple<Type, Value>>(), true );

            Assert.Contains( "arguments which are not a map value", result );
        }

        [TestMethod]
        public void PreferredInvokeScriptAliasRejectsNonMapArguments ()
        {
            var scripts = new Dictionary<string, Script>
            {
                [ "func" ] = new( "func", null, false, "{args.foo}" ),
                [ "test" ] = new( "test", null, false, "{InvokeScript(\"func\", 1)}" )
            };

            Assert.Contains( "arguments which are not a map value", ResolveScript( scripts[ "test" ].Value, scripts ) );
        }

        [TestMethod, DoNotParallelize]
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
        [DataRow("{Humanise(1110001)}", "just over 1 million")]
        [DataRow("{Humanise(-1110001)}", "minus just over 1 million")]
        [DataRow("{Humanise(1410001)}", "nearly one and a half million")]
        [DataRow("{Humanise(1510001)}", "around one and a half million")]
        [DataRow("{Humanise(1810001)}", "nearly 2 million")]
        [DataRow("{Humanise(1000000)}", "1,000,000")]
        [DataRow("{Humanise(10051)}", "just over 10 thousand")]
        [DataRow("{Humanise(111050)}", "just over 111 thousand")]
        [DataRow("{Humanise(1800001)}", "1.8 million")]
        [DataRow("{Humanise(945710000000)}", "over 940 billion")]
        public void TestHumaniseDefaultScriptRendering ( string script, string expected )
        {
            var resolver = new ScriptResolver(DefaultApproximateScripts("test", script));
            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void PreferredApproximateAliasMatchesLegacyFunction ()
        {
            Assert.AreEqual(
                ResolveScript( "{Humanise(1110001)}", DefaultApproximateScripts( "test", "{Humanise(1110001)}" ) ),
                ResolveScript( "{Approximate(1110001)}", DefaultApproximateScripts( "test", "{Approximate(1110001)}" ) ) );
        }

        [TestMethod]
        public void PreferredSpellOutAliasMatchesLegacyFunction ()
        {
            Assert.AreEqual(
                ResolveScript( "{Spacialise('SRV')}" ),
                ResolveScript( "{SpellOut('SRV')}" ) );
        }

        [TestMethod]
        [DataRow("{NumberDetails(1110001).rawvalue}", "1110001")]
        [DataRow("{NumberDetails(-1110001).absolutevalue}", "1110001")]
        [DataRow("{NumberDetails(0.123).absolutevalue}", "0.123")]
        [DataRow("{NumberDetails(-1110001).isnegative}", "true")]
        [DataRow("{NumberDetails(1510001).number}", "1")]
        [DataRow("{NumberDetails(1510001).nextdigit}", "5")]
        [DataRow("{NumberDetails(1800001).quantity}", "1.8")]
        [DataRow("{NumberDetails(1510001).format}", "around_half")]
        [DataRow("{NumberDetails(10051).format}", "just_over")]
        [DataRow("{NumberDetails(10051).magnitudename}", "thousand")]
        [DataRow("{NumberDetails(1510001).magnitudename}", "million")]
        [DataRow("{NumberDetails(1510001).invariantmagnitudename}", "million")]
        [DataRow("{NumberDetails(1510001).ordermultiplier}", "1000000")]
        [DataRow("{NumberDetails(1000000).quantity}", "1,000,000")]
        [DataRow("{if NumberDetails(1000000).fallback:bad|else:empty}", "empty")]
        [DataRow("{if NumberDetails(null).quantity:bad|else:empty}", "empty")]
        [DataRow("{if NumberDetails(1500000).magnitude:bad|else:empty}", "empty")]
        public void TestNumberDetailsFunction ( string script, string expected )
        {
            var resolver = new ScriptResolver(DefaultApproximateScripts("test", script));
            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void TestHumaniseCustomScriptCanReorderDecomposedProperties()
        {
            var scripts = DefaultApproximateScripts(
                "test",
                "{Humanise(1500000)} credits",
                "{set details to NumberDetails(args.number)}{details.magnitudename} :: {details.number} :: {details.nextdigit}" );
            var resolver = new ScriptResolver(scripts);

            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual("million :: 1 :: 5 credits", result);
        }

        [TestMethod]
        public void TestNumberDetailsCanBeUsedDirectlyInScripts()
        {
            var scripts = DefaultApproximateScripts(
                "test",
                "{set details to NumberDetails(1500000)}{details.magnitudename} :: {details.number} :: {details.nextdigit}" );
            var resolver = new ScriptResolver(scripts);

            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual("million :: 1 :: 5", result);
        }

        [TestMethod]
        public void TestHumaniseFailsVisiblyWhenScriptIsMissingDisabledOrEmpty()
        {
            var missing = new ScriptResolver(new Dictionary<string, Script>
            {
                ["test"] = new("test", null, false, "{Humanise(-1110001)}")
            });
            Assert.AreEqual(
                "Cottle speech system configuration error: Approximate script not found.",
                missing.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true));

            var disabledScripts = DefaultApproximateScripts("test", "{Humanise(-1110001)}");
            disabledScripts["Approximate"].Enabled = false;
            Assert.AreEqual(
                "Cottle speech system configuration error: Approximate script is disabled.",
                new ScriptResolver(disabledScripts).resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true));

            var emptyScripts = DefaultApproximateScripts("test", "{Humanise(-1110001)}", "");
            Assert.AreEqual(
                "Cottle speech system configuration error: Approximate script is empty.",
                new ScriptResolver(emptyScripts).resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true));

            var parseErrorScripts = DefaultApproximateScripts("test", "{Humanise(-1110001)}", "{");
            Assert.StartsWith(
                "There is a problem with the script \"Approximate\"",
                new ScriptResolver(parseErrorScripts).resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true));
        }

        [TestMethod]
        public void TestHumaniseRecursiveScriptFailsVisibly()
        {
            var scripts = DefaultApproximateScripts("test", "{Humanise(-1110001)}", "{set details to NumberDetails(args.number)}outer [{Humanise(details.rawvalue)}]");
            var resolver = new ScriptResolver(scripts);

            var result = resolver.resolveFromName("test", new Dictionary<string, Tuple<Type, Value>>(), true);

            Assert.AreEqual(
                "outer [Cottle speech system configuration error: Recursive Approximate() or Humanise() calls are not supported.]",
                result);
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

        [TestMethod, DoNotParallelize]
        public void TestSetClipboard()
        {
            ExceptionDispatchInfo exception = null;
            var testThread = new Thread(() =>
            {
                try
                {
                    var originalClipboardText = TryGetClipboardText();
                    try
                    {
                        var scripts = new Dictionary<string, Script>
                        {
                            {"test1", new Script("test1", null, false, @"{SetClipboard(""A"")}")},
                            {"test2", new Script("test2", null, false, @"{SetClipboard(""B"")}")},
                            {"test3", new Script("test3", null, false, @"{SetClipboard(""C"")}")},
                        };
                        var resolver = new ScriptResolver(scripts);
                        var dict = new Dictionary<string, Tuple<Type, Value>>();

                        ResolveAndAssertClipboard( resolver, "test1", dict, "A" );
                        ResolveAndAssertClipboard( resolver, "test2", dict, "B" );
                        ResolveAndAssertClipboard( resolver, "test3", dict, "C" );
                    }
                    finally
                    {
                        RestoreClipboardText( originalClipboardText );
                    }
                }
                catch ( Exception ex )
                {
                    exception = ExceptionDispatchInfo.Capture( ex );
                }
            });
            if (!testThread.TrySetApartmentState(ApartmentState.STA))
            {
                Assert.Fail("Unable to set thread to single thread apartment (STA) mode");
            }
            testThread.Start();
            testThread.Join();
            exception?.Throw();
        }

        private static void ResolveAndAssertClipboard (
            ScriptResolver resolver,
            string scriptName,
            Dictionary<string, Tuple<Type, Value>> dict,
            string expectedText )
        {
            COMException lastException = null;
            string lastText = null;

            for ( var attempt = 0; attempt < 10; attempt++ )
            {
                resolver.resolveFromName( scriptName, dict, true );
                try
                {
                    lastText = Clipboard.GetText();
                    if ( lastText == expectedText )
                    {
                        return;
                    }
                }
                catch ( COMException ex ) when ( IsClipboardBusy( ex ) )
                {
                    lastException = ex;
                }

                Thread.Sleep( 50 );
            }

            if ( lastException != null )
            {
                Assert.Fail( $"Unable to read clipboard after retrying: {lastException.Message}" );
            }
            Assert.AreEqual( expectedText, lastText );
        }

        private static string TryGetClipboardText ()
        {
            try
            {
                return Clipboard.GetText();
            }
            catch ( COMException ex ) when ( IsClipboardBusy( ex ) )
            {
                return null;
            }
        }

        private static void RestoreClipboardText ( string text )
        {
            try
            {
                Clipboard.Clear();
                if ( text != null )
                {
                    Clipboard.SetText( text );
                }
            }
            catch ( COMException ex ) when ( IsClipboardBusy( ex ) )
            {
                // Cleanup should not mask the assertion result.
            }
        }

        private static bool IsClipboardBusy ( COMException ex )
        {
            return ex.HResult == unchecked((int)0x800401D0);
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
