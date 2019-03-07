using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiSpeechService;
using System.Linq;

namespace UnitTests
{
    [TestClass]
    public class SpeechUnitTests : TestBase
    {
        PrivateObject speechService = new PrivateObject(SpeechService.Instance);

        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestSpeechPriorityIfInOrder()
        {
            speechService.SetFieldOrProperty("speechQueue", new SpeechQueue());
            SpeechQueue speechQueue = (SpeechQueue)speechService.GetFieldOrProperty("speechQueue");

            EddiSpeech speech1 = new EddiSpeech("Priority 1", null, 1);
            EddiSpeech speech2 = new EddiSpeech("Priority 3", null, 3);

            speechQueue.Enqueue(speech1);
            speechQueue.Enqueue(speech2);

            PrivateObject speechQueueObject = new PrivateObject(speechQueue);
            speechQueue.TryDequeue(out EddiSpeech result1);
            speechQueue.TryDequeue(out EddiSpeech result2);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            // Speech should be output in order of priority
            Assert.AreEqual("Priority 1", result1.message);
            Assert.AreEqual("Priority 3", result2.message);
        }

        [TestMethod]
        public void TestSpeechPriorityIfOutOfOrder()
        {
            speechService.SetFieldOrProperty("speechQueue", new SpeechQueue());
            SpeechQueue speechQueue = (SpeechQueue)speechService.GetFieldOrProperty("speechQueue");

            EddiSpeech speech1 = new EddiSpeech("Priority 3", null, 3);
            EddiSpeech speech2 = new EddiSpeech("Priority 1", null, 1);

            speechQueue.Enqueue(speech1);
            speechQueue.Enqueue(speech2);

            PrivateObject speechQueueObject = new PrivateObject(speechQueue);
            speechQueue.TryDequeue(out EddiSpeech result1);
            speechQueue.TryDequeue(out EddiSpeech result2);

            Assert.IsNotNull(result1);
            Assert.IsNotNull(result2);

            // Speech should be output in order of priority
            Assert.AreEqual("Priority 1", result1.message);
            Assert.AreEqual("Priority 3", result2.message);
        }

        [TestMethod]
        public void TestActiveSpeechPriority()
        {
            EddiSpeech priority5speech = new EddiSpeech("Priority 5", null, 5);
            EddiSpeech priority4speech = new EddiSpeech("Priority 2", null, 4);
            EddiSpeech priority2speech = new EddiSpeech("Priority 4", null, 2);
            EddiSpeech priority1speech = new EddiSpeech("Priority 1", null, 1);

            // Set up priority 5 speech
            speechService.SetFieldOrProperty("activeSpeechPriority", priority5speech.priority);
            Assert.AreEqual(5, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 5 speech IS interrupted by priority 4 speech.
            Assert.IsTrue((bool)speechService.Invoke("checkSpeechInterrupt", new object[] { priority4speech.priority }));

            // Set up priority 4 speech
            speechService.SetFieldOrProperty("activeSpeechPriority", priority4speech.priority);
            Assert.AreEqual(4, (int)speechService.GetFieldOrProperty("activeSpeechPriority"));

            // Check that priority 4 speech IS NOT interrupted by priority 2 speech.
            Assert.IsFalse((bool)speechService.Invoke("checkSpeechInterrupt", new object[] { priority2speech.priority }));

            // Check that priority 4 speech IS interrupted by priority 1 speech.
            Assert.IsTrue((bool)speechService.Invoke("checkSpeechInterrupt", new object[] { priority1speech.priority }));
        }

        [TestMethod]
        public void TestClearSpeechQueue()
        {
            EddiSpeech speech = new EddiSpeech("Priority 3", null, 3);
            SpeechQueue speechQueue = new SpeechQueue();
            Assert.IsTrue(speechQueue.priorityQueues.ElementAtOrDefault(speech.priority) != null);

            speechQueue.priorityQueues[speech.priority].Enqueue(speech);
            Assert.AreEqual(1, speechQueue.priorityQueues[speech.priority].Count);

            speechQueue.DequeueAllSpeech();

            Assert.AreEqual(0, speechQueue.priorityQueues[speech.priority].Count);
        }

        [TestMethod]
        public void TestFilterSpeechQueue()
        {
            EddiSpeech speech1 = new EddiSpeech("Jumped", null, 3, null, false, "FSD engaged");
            EddiSpeech speech2 = new EddiSpeech("Refueled", null, 3, null, false, "Ship refueled");
            EddiSpeech speech3 = new EddiSpeech("Scanned", null, 3, null, false, "Body scan");

            SpeechQueue speechQueue = new SpeechQueue();
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
    }
}
