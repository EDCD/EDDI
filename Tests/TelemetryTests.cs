using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class TelemetryTests : TestBase
    {
        private Dictionary<string, object> PrepRollbarData(object data)
        {
            PrivateType privateType = new PrivateType(typeof(Logging));
            Dictionary<string, object> result = (Dictionary<string, object>)privateType.InvokeStatic("FilterAndRedactData", new object[] { data });
            return result;
        }

        [TestMethod]
        public void TestString()
        {
            string data = "This is a string data package";
            Dictionary<string, object> result = PrepRollbarData(data);
            result.TryGetValue("message", out object message);
            Assert.AreEqual(data, (string)message);
        }

        [TestMethod]
        public void TestOther()
        {
            Event data = new CommanderLoadingEvent(DateTime.UtcNow, "testCmdr", "F111111");
            Dictionary<string, object> result = PrepRollbarData(data);
            Assert.IsFalse(result.TryGetValue("frontierID", out object frontierID), "'frontierID' property should have been removed");
            Assert.IsTrue(result.TryGetValue("type", out object type));
        }

        [TestMethod]
        public void TestException()
        {
            Exception exception = new InvalidCastException();
            Dictionary<string, object> result = PrepRollbarData(exception);

            result.TryGetValue("Message", out object message);
            Assert.AreEqual(exception.Message, message?.ToString());

            result.TryGetValue("StackTraceString", out object stacktrace);
            Assert.AreEqual(exception.StackTrace, stacktrace?.ToString());
        }

        [TestMethod]
        public void TestDictionary()
        {
            string str = "This is a Dictionary payload";
            Event @event = new CommanderLoadingEvent(DateTime.UtcNow, "testCmdr", "F111111");
            Exception exception = new InvalidCastException();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("message", str);
            data.Add("event", @event);
            data.Add("exception", exception);

            Dictionary<string, object> result = PrepRollbarData(data);

            result.TryGetValue("message", out object message);
            Assert.AreEqual(str, message?.ToString());

            result.TryGetValue("event", out object theEvent);
            ((JObject)theEvent).TryGetValue("frontierID", out JToken frontierID);
            Assert.IsNull(frontierID?.ToString());
            ((JObject)theEvent).TryGetValue("type", out JToken type);
            Assert.IsNotNull(type);
            Assert.AreEqual(@event.type, type?.ToString());

            result.TryGetValue("exception", out object theException);
            ((JObject)theException).TryGetValue("Message", out JToken theExceptionMessage);
            Assert.AreEqual(exception.Message, theExceptionMessage?.ToString());
        }

        [TestMethod]
        public void TestArray()
        {
            string[] data = { "a", "b", "c" };
            Dictionary<string, object> result = PrepRollbarData(data);
            Assert.IsTrue(result.TryGetValue("data", out object package));
            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], ((JArray)package)[i]);
            }
        }
    }
}
