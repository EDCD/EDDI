using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.IO.Compression;
using NetMQ;
using NetMQ.Sockets;

namespace Tests
{
    [TestClass]
    public class EddnTests
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")] // this usage is perfectly correct
        [TestMethod]
        public void TestListen()
        {
            using (var subscriber = new SubscriberSocket())
            {
                int i = 0;
                subscriber.Connect("tcp://eddn.edcd.io:9500");
                subscriber.Subscribe("");
                while (i < 10)
                {
                    string data;
                    byte[] compressed = subscriber.ReceiveFrameBytes();
                    using (var stream = new MemoryStream(compressed, 2, compressed.Length - 2))
                    using (var inflater = new DeflateStream(stream, CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(inflater))
                    {
                        data = streamReader.ReadToEnd();
                    }
                    Console.WriteLine(data);
                    i++;
                }
            }
        }

        [TestMethod()]
        public void TestEDDNResponderGoodMatch()
        {
            EDDNResponder.EDDNResponder responder = new EDDNResponder.EDDNResponder();
            var privateObject = new PrivateObject(responder);

            // The EDDN responder tracks system names and coordinates independently. 
            // Intentionally place our EDDN responder in a state with no coordinates available.
            privateObject.SetFieldOrProperty("systemName", "Sol");
            privateObject.SetFieldOrProperty("systemX", 0.0M);
            privateObject.SetFieldOrProperty("systemY", 0.0M);
            privateObject.SetFieldOrProperty("systemZ", 0.0M);

            // Force a call to the method
            bool matched = responder.eventSystemNameMatches("Sol");
            Assert.IsTrue(matched);
        }

        [TestMethod()]
        public void TestEDDNResponderBadInitialGoodFinal()
        {
            EDDNResponder.EDDNResponder responder = new EDDNResponder.EDDNResponder();
            var privateObject = new PrivateObject(responder);

            // The EDDN responder tracks system names and coordinates independently. 
            // Intentionally place our EDDN responder in a state with no coordinates available.
            privateObject.SetFieldOrProperty("systemName", "Not in this galaxy");
            privateObject.SetFieldOrProperty("systemX", null);
            privateObject.SetFieldOrProperty("systemY", null);
            privateObject.SetFieldOrProperty("systemZ", null);

            // Force a call to the method
            bool matched = responder.eventSystemNameMatches("Artemis");
            Assert.IsTrue(matched);

            // Test that the results, including coordinates, have been correctly retrieved by the EDDN responder
            Assert.AreEqual("Artemis", responder.systemName);
            Assert.AreEqual(14.28125M, (decimal)responder.systemX);
            Assert.AreEqual(-63.1875M, (decimal)responder.systemY);
            Assert.AreEqual(-24.875M, (decimal)responder.systemZ);
        }
    }
}
