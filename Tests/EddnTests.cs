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
    }
}
