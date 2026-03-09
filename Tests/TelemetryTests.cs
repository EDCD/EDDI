using EddiEvents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class TelemetryTests : TestBase
    {
        private Dictionary<string, object> PrepTelemetryData(object data)
        {
            return Logging.PrepareData( JToken.FromObject( data ) );
        }

        [TestMethod]
        public void TestString()
        {
            var data = "This is a string data package";
            var result = PrepTelemetryData(data);
            result.TryGetValue("message", out var message);
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

            if ( result.TryGetValue( "Message", out var message ) )
            {
                Assert.AreEqual( exception.Message, message.ToString() );
            }
            else { Assert.Fail(); }

            if ( result.TryGetValue( "StackTraceString", out var stacktrace ) )
            {
                Assert.AreEqual( exception.StackTrace ?? string.Empty, stacktrace?.ToString() ?? string.Empty );
            }
            else { Assert.Fail(); }
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

            if ( result.TryGetValue( "message", out var message ) )
            {
                Assert.AreEqual( str, message.ToString() );
            }
            else { Assert.Fail(); }

            if ( result.TryGetValue( "event", out var theEvent ) )
            {
                ( (JObject)theEvent ).TryGetValue( "frontierID", out var frontierID );
                Assert.IsNull( frontierID?.ToString() );
                ( (JObject)theEvent ).TryGetValue( "type", out var type );
                Assert.IsNotNull( type );
                Assert.AreEqual( @event.type, type.ToString() );
            }
            else { Assert.Fail(); }

            if ( result.TryGetValue( "exception", out var theException ) )
            {
                ( (JObject)theException ).TryGetValue( "Message", out var theExceptionMessage );
                Assert.IsNotNull( theExceptionMessage );
                Assert.AreEqual( exception.Message, theExceptionMessage.ToString() );
            }
            else
            { Assert.Fail(); }
        }

        [TestMethod]
        public void TestArray()
        {
            string[] data = [ "a", "b", "c" ];
            var result = PrepTelemetryData(data);
            Assert.IsTrue(result.TryGetValue("data", out var package));
            for (var i = 0; i < data.Length; i++)
            {
                Assert.AreEqual(data[i], ((JArray)package)[i]);
            }
        }
    }
}
