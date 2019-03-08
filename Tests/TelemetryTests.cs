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
            Dictionary<string, object> result = (Dictionary<string, object>)privateType.InvokeStatic("PrepRollbarData", new object[] { data });
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
            Event data = new UnhandledEvent(DateTime.UtcNow, "testEvent");
            Dictionary<string, object> result = PrepRollbarData(data);
            Assert.IsFalse(result.TryGetValue("timestamp", out object timestamp));
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
            Event @event = new UnhandledEvent(DateTime.UtcNow, "testEvent");
            Exception exception = new InvalidCastException();

            Dictionary<string, object> data = new Dictionary<string, object>();
            data.Add("message", str);
            data.Add("event", @event);
            data.Add("exception", exception);

            Dictionary<string, object> result = PrepRollbarData(data);

            result.TryGetValue("message", out object message);
            Assert.AreEqual(str, message?.ToString());

            result.TryGetValue("event", out object theEvent);
            ((JObject)theEvent).TryGetValue("timestamp", out JToken timestamp);
            Assert.IsNull(timestamp?.ToString());
            ((JObject)theEvent).TryGetValue("type", out JToken type);
            Assert.IsNotNull(type);
            Assert.AreEqual(@event.type, type?.ToString());

            result.TryGetValue("exception", out object theException);
            ((JObject)theException).TryGetValue("Message", out JToken theExceptionMessage);
            Assert.AreEqual(exception.Message, theExceptionMessage?.ToString());
        }
    }
}
