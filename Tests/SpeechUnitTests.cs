using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiSpeechService;
using CSCore.SoundOut;
using Rollbar;

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
    }
}
