using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiSpeechService;
using CSCore.SoundOut;
using Rollbar;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class SpeechUnitTests
    {
        PrivateObject speechService = new PrivateObject(SpeechService.Instance);

        [TestInitialize]
        public void start()
        {
            // Prevent telemetry data from being reported based on test results
            RollbarLocator.RollbarInstance.Config.Enabled = false;
        }

        [TestMethod]
        public void TestSpeechPriorityIfInOrder()
        {
            EddiSpeech speech1 = new EddiSpeech("Priority 1", true, null, 1);
            EddiSpeech speech2 = new EddiSpeech("Priority 3", true, null, 3);

            speechService.Invoke("enqueueSpeech", new object[] { speech1 });
            speechService.Invoke("enqueueSpeech", new object[] { speech2 });

            EddiSpeech result1 = (EddiSpeech)speechService.Invoke("dequeueSpeech", new object[] { });
            EddiSpeech result2 = (EddiSpeech)speechService.Invoke("dequeueSpeech", new object[] { });

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            // Speech should be output in order of priority
            Assert.AreEqual("Priority 1", result1.message);
            Assert.AreEqual("Priority 3", result2.message);
        }

        [TestMethod]
        public void TestSpeechPriorityIfOutOfOrder()
        {
            EddiSpeech speech1 = new EddiSpeech("Priority 3", true, null, 3);
            EddiSpeech speech2 = new EddiSpeech("Priority 1", true, null, 1);

            speechService.Invoke("enqueueSpeech", new object[] { speech1 });
            speechService.Invoke("enqueueSpeech", new object[] { speech2 });

            EddiSpeech result1 = (EddiSpeech)speechService.Invoke("dequeueSpeech", new object[] { });
            EddiSpeech result2 = (EddiSpeech)speechService.Invoke("dequeueSpeech", new object[] { });

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            // Speech should be output in order of priority
            Assert.AreEqual("Priority 1", result1.message);
            Assert.AreEqual("Priority 3", result2.message);
        }

        [TestMethod]
        public void TestActiveSpeechPriority()
        {
            EddiSpeech speech1 = new EddiSpeech("Priority 5", true, null, 5);
            EddiSpeech speech2 = new EddiSpeech("Priority 4", true, null, 4);
            EddiSpeech speech3 = new EddiSpeech("Priority 2", true, null, 2);
            EddiSpeech speech4 = new EddiSpeech("Priority 1", true, null, 1);

            // Set up priority 5 speech
            speechService.SetFieldOrProperty("activeSpeech", (ISoundOut)speechService.Invoke("GetSoundOut", new object[] { }));
            speechService.SetFieldOrProperty("activeSpeechPriority", speech1.priority);
            Assert.IsNotNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));
            Assert.AreEqual(5, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 5 speech IS interrupted by priority 4 speech.
            speechService.Invoke("checkSpeechInterrupt", new object[] { speech2 });
            Assert.IsNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));

            // Set up priority 4 speech
            speechService.SetFieldOrProperty("activeSpeech", (ISoundOut)speechService.Invoke("GetSoundOut", new object[] { }));
            speechService.SetFieldOrProperty("activeSpeechPriority", speech2.priority);
            Assert.IsNotNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));
            Assert.AreEqual(4, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 4 speech IS NOT interrupted by priority 2 speech.
            speechService.Invoke("checkSpeechInterrupt", new object[] { speech3 });
            Assert.IsNotNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));
            Assert.AreEqual(4, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 4 speech IS interrupted by priority 1 speech.
            speechService.Invoke("checkSpeechInterrupt", new object[] { speech4 });
            Assert.IsNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));
        }

        [TestMethod]
        public void TestClearSpeechQueue()
        {
            EddiSpeech speech = new EddiSpeech("Priority 3", true, null, 3);
            List<ConcurrentQueue<EddiSpeech>> speechQueues = (List<ConcurrentQueue<EddiSpeech>>)speechService.GetFieldOrProperty("speechQueues", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.IsTrue(speechQueues.ElementAtOrDefault(speech.priority) != null);

            speechQueues[speech.priority].Enqueue(speech);
            Assert.AreEqual(1, speechQueues[speech.priority].Count);

            speechService.Invoke("ClearSpeech", new object[] { });

            Assert.AreEqual(0, speechQueues[speech.priority].Count);
        }

        [TestMethod]
        public void TestFilterSpeechQueue()
        {
            EddiSpeech speech1 = new EddiSpeech("Jumped", true, null, 3, null, false, "FSD engaged");
            EddiSpeech speech2 = new EddiSpeech("Refueled", true, null, 3, null, false, "Ship refueled");
            EddiSpeech speech3 = new EddiSpeech("Scanned", true, null, 3, null, false, "Body scan");

            List<ConcurrentQueue<EddiSpeech>> speechQueues = (List<ConcurrentQueue<EddiSpeech>>)speechService.GetFieldOrProperty("speechQueues", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
            Assert.IsTrue(speechQueues.ElementAtOrDefault(3) != null);
            speechQueues[speech1.priority].Enqueue(speech1);
            speechQueues[speech2.priority].Enqueue(speech2);
            speechQueues[speech3.priority].Enqueue(speech3);

            Assert.AreEqual(3, speechQueues[3].Count);

            speechService.Invoke("ClearSpeechOfType", new object[] { "Body scan" });

            Assert.AreEqual(2, speechQueues[3].Count);
            if (speechQueues[3].TryDequeue(out EddiSpeech result1))
            {
                Assert.AreEqual("FSD engaged", result1.eventType);
            }
            if (speechQueues[3].TryDequeue(out EddiSpeech result2))
            {
                Assert.AreEqual("Ship refueled", result2.eventType);
            }
        }
    }
}
