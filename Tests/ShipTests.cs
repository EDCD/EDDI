using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackResponder;
using System.Collections.Generic;
using System;
using System.Speech.Synthesis;
using System.IO;
using System.Speech.AudioFormat;
using System.Threading;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;
using EddiSpeechService;
using CSCore;
using CSCore.Codecs.WAV;
using CSCore.SoundOut;
using CSCore.Streams.Effects;
using EddiDataDefinitions;
using Utilities;

namespace Tests
{
    [TestClass]
    public class ShipTests
    {
        [TestMethod]
        public void TestShipSpokenName1()
        {
            Ship ship = new Ship();
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your ship", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName2()
        {
            Ship ship = new Ship();
            ship.name = "";
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your ship", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName3()
        {
            Ship ship = ShipDefinitions.FromModel("Anaconda");
            string spokenName = ship.SpokenName();
            Assert.AreEqual("your Anaconda", spokenName);
        }

        [TestMethod]
        public void TestShipSpokenName4()
        {
            Ship ship = ShipDefinitions.FromModel("Anaconda");
            ship.name = "Testy";
            string spokenName = ship.SpokenName();
            Assert.AreEqual("Testy", spokenName);
        }
    }
}
