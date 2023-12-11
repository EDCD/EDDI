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
        private Dictionary<string, object> PrepTelemetryData(object data)
        {
            var privateType = new PrivateType(typeof(Logging));
            var result = (Dictionary<string, object>)privateType.InvokeStatic("PrepareData", JToken.FromObject(data) );
            return result;
        }

        [TestMethod]
        public void TestString()
        {
            var data = "This is a string data package";
            var result = PrepTelemetryData(data);
            result.TryGetValue("message", out object message);
            Assert.AreEqual(data, (string)message);
        }

        [TestMethod]
        public void TestOther()
        {
            var data = new CommanderLoadingEvent(DateTime.UtcNow, "testCmdr", "F111111");
            var result = PrepTelemetryData(data);
            Assert.IsFalse(result.TryGetValue("frontierID", out _), "'frontierID' property should have been removed");
            Assert.IsTrue(result.TryGetValue("type", out _));
        }

        [TestMethod]
        public void TestException()
        {
            var exception = new InvalidCastException();
            var result = PrepTelemetryData(exception);

            result.TryGetValue("Message", out object message);
            Assert.AreEqual(exception.Message, message?.ToString());

            result.TryGetValue("StackTraceString", out object stacktrace);
            Assert.AreEqual(exception.StackTrace, stacktrace?.ToString());
        }

        [TestMethod]
        public void TestDictionary()
        {
            var str = "This is a Dictionary payload";
            var @event = new CommanderLoadingEvent(DateTime.UtcNow, "testCmdr", "F111111");
            Assert.IsNotNull(@event);
            var exception = new InvalidCastException();

            var data = new Dictionary<string, object>
            {
                { "message", str },
                { "event", @event },
                { "exception", exception }
            };

            var result = PrepTelemetryData(data);

            result.TryGetValue("message", out object message);
            Assert.AreEqual(str, message?.ToString());

            result.TryGetValue("event", out object theEvent);
            Assert.IsNotNull(theEvent);
            ((JObject)theEvent).TryGetValue("frontierID", out JToken frontierID);
            Assert.IsNull(frontierID?.ToString());
            ((JObject)theEvent).TryGetValue("type", out JToken type);
            Assert.IsNotNull(type);
            Assert.AreEqual(@event.type, type?.ToString());

            result.TryGetValue("exception", out object theException);
            Assert.IsNotNull(theException);
            ((JObject)theException).TryGetValue("Message", out JToken theExceptionMessage);
            Assert.AreEqual(exception.Message, theExceptionMessage?.ToString());
        }

        [TestMethod]
        public void TestArray()
        {
            string[] data = { "a", "b", "c" };
            var result = PrepTelemetryData(data);
            Assert.IsTrue(result.TryGetValue("data", out object package));
            for (int i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], ((JArray)package)[i]);
            }
        }
    }
}
