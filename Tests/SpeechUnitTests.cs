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
            EddiSpeech speech1 = new EddiSpeech("Priority 3", true, null, 3);
            EddiSpeech speech2 = new EddiSpeech("Priority 1", true, null, 1);
            speechService.SetFieldOrProperty("activeSpeech", (ISoundOut)speechService.Invoke("GetSoundOut", new object[] { }));
            speechService.SetFieldOrProperty("activeSpeechPriority", speech1.priority);

            Assert.IsNotNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));
            speechService.Invoke("checkSpeechPriority", new object[] { speech2 });

            // If our new speech is more urgent, active speech should be purged to make room for the new speech
            Assert.IsNull((ISoundOut)speechService.GetFieldOrProperty("activeSpeech"));
        }
    }
}
