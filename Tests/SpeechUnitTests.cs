using EddiSpeechResponder;
using EddiSpeechService;
using EddiSpeechService.SpeechPreparation;
using EddiVoiceAttackResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class SpeechUnitTests : TestBase
    {

        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private string CondenseSpaces(string s)
        {
            return System.Text.RegularExpressions.Regex.Replace(s, @"\s+", " ");
        }

        [DataTestMethod]
        [DataRow( "Priority 1", "Priority 3", 1, 3, "Priority 1", "Priority 3" )] // Queued in order
        [DataRow( "Priority 3", "Priority 1", 3, 1, "Priority 1", "Priority 3" )] // Queued out of order
        public void TestSpeechPriority(string message1, string message2, int priority1, int priority2, string expectedResult1, string expectedResult2 )
        {
            var speechService = new PrivateObject(new SpeechService());
            speechService.SetFieldOrProperty("speechQueue", new SpeechQueue());
            var speechQueue = (SpeechQueue)speechService.GetFieldOrProperty("speechQueue");

            var speech1 = new EddiSpeech(message1, null, priority1);
            var speech2 = new EddiSpeech(message2, null, priority2);

            if ( speechQueue is null )
            {
                Assert.Fail();
            }

            speechQueue.Enqueue(speech1);
            speechQueue.Enqueue(speech2);

            if ( speechQueue.TryDequeue( out var result1 ))
            {
                Assert.IsNotNull( result1 );
            }
            else
            {
                Assert.Fail();
            }

            if ( speechQueue.TryDequeue( out var result2 ))
            {
                Assert.IsNotNull( result2 );
            }
            else
            {
                Assert.Fail();
            }

            Assert.AreEqual( expectedResult1, result1.message);
            Assert.AreEqual( expectedResult2, result2.message);
        }

        [TestMethod]
        public void TestActiveSpeechPriority()
        {
            var speechService = new PrivateObject(new SpeechService());

            var priority5speech = new EddiSpeech("Priority 5", null, 5);
            var priority4speech = new EddiSpeech("Priority 2", null, 4);
            var priority2speech = new EddiSpeech("Priority 4", null, 2);
            var priority1speech = new EddiSpeech("Priority 1", null, 1);

            // Set up priority 5 speech
            speechService.SetFieldOrProperty("activeSpeechPriority", priority5speech.priority);
            Assert.AreEqual(5, (int?)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 5 speech IS interrupted by priority 4 speech.
            Assert.IsTrue((bool?)speechService.Invoke("checkSpeechInterrupt", priority4speech.priority ));

            // Set up priority 4 speech
            speechService.SetFieldOrProperty("activeSpeechPriority", priority4speech.priority);
            Assert.AreEqual(4, (int?)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 4 speech IS NOT interrupted by priority 2 speech.
            Assert.IsFalse((bool?)speechService.Invoke("checkSpeechInterrupt", priority2speech.priority ));

            // Check that priority 4 speech IS interrupted by priority 1 speech.
            Assert.IsTrue((bool?)speechService.Invoke("checkSpeechInterrupt", priority1speech.priority ));
        }

        [TestMethod]
        public void TestClearSpeechQueue()
        {
            var speech = new EddiSpeech("Priority 3", null, 3);
            var speechQueue = new SpeechQueue();
            Assert.IsTrue(speechQueue.priorityQueues.ElementAtOrDefault(speech.priority) != null);

            speechQueue.priorityQueues[speech.priority].Enqueue(speech);
            Assert.AreEqual(1, speechQueue.priorityQueues[speech.priority].Count);

            speechQueue.DequeueAllSpeech();

            Assert.AreEqual(0, speechQueue.priorityQueues[speech.priority].Count);
        }

        [TestMethod]
        public void TestFilterSpeechQueue()
        {
            var speech1 = new EddiSpeech("Jumped", null, 3, null, false, "FSD engaged");
            var speech2 = new EddiSpeech("Refueled", null, 3, null, false, "Ship refueled");
            var speech3 = new EddiSpeech("Scanned", null, 3, null, false, "Body scan");

            var speechQueue = new SpeechQueue();
            Assert.IsTrue(speechQueue.priorityQueues.ElementAtOrDefault(3) != null);
            speechQueue.priorityQueues[speech1.priority].Enqueue(speech1);
            speechQueue.priorityQueues[speech2.priority].Enqueue(speech2);
            speechQueue.priorityQueues[speech3.priority].Enqueue(speech3);

            Assert.AreEqual(3, speechQueue.priorityQueues[3].Count);

            speechQueue.DequeueSpeechOfType("Body scan");

            Assert.AreEqual(2, speechQueue.priorityQueues[3].Count);
            if (speechQueue.priorityQueues[3].TryDequeue(out EddiSpeech result1))
            {
                Assert.AreEqual("FSD engaged", result1.eventType);
            }
            if (speechQueue.priorityQueues[3].TryDequeue(out EddiSpeech result2))
            {
                Assert.AreEqual("Ship refueled", result2.eventType);
            }
        }

        [TestMethod]
        public void TestPathingString1()
        {
            var pathingString = @"There are [4;5] lights";
            var pathingOptions = new List<string>() {
                 "There are 4 lights"
                ,"There are 5 lights"
            };

            var pathingResults = new HashSet<string>();
            for ( var i = 0; i < 1000; i++)
            {
                var pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            var expectedHashSet = new HashSet<string>(pathingOptions.Select(CondenseSpaces));
            Assert.IsTrue(pathingResults.SetEquals(expectedHashSet));
        }

        [TestMethod]
        public void TestPathingString2()
        {
            var pathingString = @"There are [4;5;] lights";
            var pathingOptions = new List<string>() {
                 "There are 4 lights"
                ,"There are 5 lights"
                ,"There are  lights"
            };

            var pathingResults = new HashSet<string>();
            for ( var i = 0; i < 1000; i++)
            {
                var pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            var expectedHashSet = new HashSet<string>(pathingOptions.Select(CondenseSpaces));
            Assert.IsTrue(pathingResults.SetEquals(expectedHashSet));
        }

        [TestMethod]
        public void TestPathingString3()
        {
            var pathingString = @"There [are;might be;could be] [4;5;] lights;It's dark in here;";
            var pathingOptions = new List<string>() {
                 "There are 4 lights"
                ,"There are 5 lights"
                ,"There are  lights"
                ,"There might be 4 lights"
                ,"There might be 5 lights"
                ,"There might be  lights"
                ,"There could be 4 lights"
                ,"There could be 5 lights"
                ,"There could be  lights"
                ,"It's dark in here"
                ,""
            };

            var pathingResults = new HashSet<string>();
            for ( var i = 0; i < 1000; i++)
            {
                var pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            var expectedHashSet = new HashSet<string>(pathingOptions.Select(CondenseSpaces));
            Assert.IsTrue(pathingResults.SetEquals(expectedHashSet));
        }

        [TestMethod]
        public void TestPathingString4()
        {
            var pathingString = @";;;;;;Seven;;;";
            var pathingOptions = new List<string>() {
                 ""
                ,"Seven"
            };

            var sevenCount = 0;
            var pathingResults = new HashSet<string>();
            for ( var i = 0; i < 10000; i++)
            {
                var pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add( pathedString );
                if ( pathedString == "Seven")
                {
                    sevenCount++;
                }
            }

            Assert.IsTrue(sevenCount > 750);
            Assert.IsTrue(sevenCount < 1500);

            var expectedHashSet = new HashSet<string>(pathingOptions.Select(CondenseSpaces));
            Assert.IsTrue( pathingResults.SetEquals( expectedHashSet ) );
        }

        [TestMethod]
        public void TestPathingString5()
        {
            var pathingString = @"You leave me [no choice].";
            var pathingOptions = new List<string>() {
                "You leave me no choice."
            };

            var pathingResults = new HashSet<string>();
            for (var i = 0; i < 1000; i++)
            {
                var pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestPathingString6()
        {
            var pathingString = @"[There can be only one.]";
            var pathingOptions = new List<string>() {
                "There can be only one."
            };

            var pathingResults = new HashSet<string>();
            for (var i = 0; i < 1000; i++)
            {
                var pathedString = VoiceAttackPlugin.SpeechFromScript(pathingString);
                pathingResults.Add(pathedString);
            }

            Assert.IsTrue(pathingResults.SetEquals(new HashSet<string>(pathingOptions)));
        }

        [TestMethod]
        public void TestSectorTranslations()
        {
            Assert.AreEqual("Swoiwns <say-as interpret-as=\"characters\">N</say-as> <say-as interpret-as=\"characters\">Y</say-as> dash <say-as interpret-as=\"characters\">B</say-as> <say-as interpret-as=\"characters\">a</say-as> 95 dash 0", Translations.GetTranslation("Swoiwns NY-B a95-0"));
            Assert.AreEqual("<say-as interpret-as=\"characters\">P</say-as> <say-as interpret-as=\"characters\">P</say-as> <say-as interpret-as=\"characters\">M</say-as> 5 2 8 7", Translations.GetTranslation("PPM 5287"));
        }

        [TestMethod]
        public void TestTranslationVesper()
        {
            Assert.AreEqual(Translations.GetTranslation("VESPER-M4"), "Vesper M 4");
        }

        [TestMethod]
        public void TestSpeechQueue_DequeueSpeechOfType()
        {
            var privateObject = new PrivateObject(new SpeechQueue());
            privateObject.Invoke("DequeueAllSpeech", Array.Empty<object>());
            privateObject.Invoke("Enqueue", new EddiSpeech("Test speech 1", null, 3, null, false, null) );
            privateObject.Invoke("Enqueue", new EddiSpeech("Test speech 2", null, 4, null, false, "Hull damaged") );
            privateObject.Invoke("Enqueue", new EddiSpeech("Test speech 3", null, 3, null, false, "Body scanned") );

            var priorityQueues = (List<ConcurrentQueue<EddiSpeech>>)privateObject.GetFieldOrProperty("priorityQueues");
            Assert.AreEqual(3, priorityQueues?.SelectMany(q => q).Count());
            try
            {
                // Only the speech of type "Hull damaged" should be removed, null types and other types should remain in place.
                privateObject.Invoke("DequeueSpeechOfType", "Hull damaged" );
                Assert.AreEqual(2, priorityQueues?.SelectMany(q => q).Count());
                // Verify that the order of remaining speech of the same priority is unchanged.
                Assert.AreEqual("Test speech 1", priorityQueues?[3].First().message);
                Assert.AreEqual("Test speech 3", priorityQueues?[3].Last().message);
            }
            catch (Exception)
            {
                Assert.Fail();
            }
            privateObject.Invoke("DequeueAllSpeech", Array.Empty<object>());
        }

        [DataTestMethod]
        // Test escaping for invalid ssml.
        [DataRow("<invalid>test</invalid> <invalid withattribute='attribute'>test2</invalid>", "&lt;invalid&gt;test&lt;/invalid&gt; &lt;invalid withattribute='attribute'&gt;test2&lt;/invalid&gt;")]
        // Test escaping for double quotes, single quotes, and <phoneme> ssml commands. XML characters outside of ssml elements are escaped.
        [DataRow(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet='ipa' ph='ˈdezɦrə'>Dezhra</phoneme> & Co's shop", "<phoneme alphabet=\"ipa\" ph=\"ʃɪnˈrɑːrtə\">Shinrarta</phoneme> <phoneme alphabet='ipa' ph='ˈdezɦrə'>Dezhra</phoneme> &amp; Co&apos;s shop")]
        // Test escaping for <break> elements. XML characters outside of ssml elements are escaped.
        [DataRow(@"<break time=""100ms""/>He said ""Foo"".", "<break time=\"100ms\"/>He said &quot;Foo&quot;.")] 
        // Test escaping for Cereproc unique <usel> and <spurt> elements. Input and output should be equal.
        [DataRow(@"<spurt audio='g0001_004'>cough</spurt> This is a <usel variant=""1"">test</usel> sentence.", @"<spurt audio='g0001_004'>cough</spurt> This is a <usel variant=""1"">test</usel> sentence.")]
        // Test escaping for characters included in the escape sequence ('X' in this case)
        [DataRow(@"Brazilian armada <say-as interpret-as=""characters"">X</say-as>", @"Brazilian armada <say-as interpret-as=""characters"">X</say-as>")]
        public void TestSpeechServiceEscaping(string input, string expectedOutput)
        {
            
            var result = SpeechFormatter.EscapeSSML(input);
            Assert.AreEqual(expectedOutput, result);
        }

        [TestMethod]
        public void TestDisableIPA()
        {
            // Test removal of <phoneme> tags (and only phenome tags) when the user has indicated that they would like to disable phonetic speech
            var line = @"<break time=""100ms""/><phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet='ipa' ph='ˈdezɦrə'>Dezhra</phoneme> & Co's shop";
            var result = SpeechFormatter.DisableIPA(line);
            Assert.AreEqual(@"<break time=""100ms""/>Shinrarta Dezhra & Co's shop", result);
        }

        [TestMethod]
        public void TestPersonalityLocalizedScriptsAreComplete()
        {
            // Make sure that all default scripts in our invariant personality also exist in localized default personalities
            var dirInfo = new DirectoryInfo(AppContext.BaseDirectory);
            var @default = Personality.FromFile(dirInfo?.FullName + "\\eddi.json", true);

            Assert.IsNotNull(@default);
            var missingScripts = new Dictionary<string, List<string>>();
            foreach (var fileInfo in dirInfo.GetFiles().Where(f => f.Name.StartsWith("eddi") && f.Name != "eddi.json"))
            {
                var localizedDefaultPersonality = Personality.FromFile(fileInfo.FullName);

                foreach (var script in @default.Scripts)
                {
                    if (!@localizedDefaultPersonality.Scripts.ContainsKey(script.Key))
                    {
                        // Missing script found
                        if (!missingScripts.ContainsKey(fileInfo.Name))
                        {
                            // Make sure we've initialized a list to record it
                            missingScripts[fileInfo.Name] = new List<string>();
                        }
                        // Record the missing script
                        missingScripts[fileInfo.Name].Add(script.Key);
                    }
                }
            }
            Assert.AreEqual(0, missingScripts.Count);
        }

        [DataTestMethod]
        [DataRow(@"   ", "")]
        [DataRow(@"Test <break time=""3s""/>", "Test")]
        [DataRow(@"<break time=""3ms""/> Test", @"<break time=""3ms""/> Test")]
        [DataRow(@"<break time=""3ms""/> <break time=""3ms""/>", "")]
        [DataRow(@"<break time=""3s""/> Test <break time=""3s""/>", @"<break time=""3s""/> Test")]
        public void TestSpeechServiceTrimming(string input, string output)
        {
            Assert.AreEqual(output, SpeechFormatter.TrimSpeech(input));
        }

        [DataTestMethod]
        [DataRow( "{body.", "body" ) ]
        [DataRow( "{set test to body.", "body" ) ]
        [DataRow( "{set test to body.materials[0].", @"body.materials.<index\>" ) ]
        [DataRow( "{set test to body.materials[0].Category.", @"body.materials.<index\>.Category" ) ]
        [DataRow( "{StationDetails().", "StationDetails()" )]
        public void TestSpeechResponderTextCompletionLookupItem ( string lineTxt, string result )
        {
            Assert.AreEqual(result, EditScriptWindow.GetTextCompletionLookupItem(lineTxt));
        }
    }
}
