using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class HumanizeTests
    {
        [TestMethod]
        public void TestSpeechHumanize1()
        {
            Assert.AreEqual("well over minus 12 thousand", Translations.Humanize(-12345));
        }

        [TestMethod]
        public void TestSpeechHumanize2()
        {
            Assert.AreEqual(null, Translations.Humanize(null));
        }

        [TestMethod]
        public void TestSpeechHumanize3()
        {
            Assert.AreEqual("zero", Translations.Humanize(0));
        }

        [TestMethod]
        public void TestSpeechHumanize4()
        {
            Assert.AreEqual("0.16", Translations.Humanize(0.15555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize5()
        {
            Assert.AreEqual("0.016", Translations.Humanize(0.015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize6()
        {
            Assert.AreEqual("0.0016", Translations.Humanize(0.0015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize7()
        {
            Assert.AreEqual("minus 51000000", Translations.Humanize(-51000000));
        }

        [TestMethod]
        public void TestSpeechHumanize8()
        {
            Assert.AreEqual("51000000", Translations.Humanize(51000001));
        }

        [TestMethod]
        public void TestSpeechHumanize9()
        {
            Assert.AreEqual("10000", Translations.Humanize(10000));
        }

        [TestMethod]
        public void TestSpeechHumanize10()
        {
            Assert.AreEqual("100000", Translations.Humanize(100000));
        }

        [TestMethod]
        public void TestSpeechHumanize11()
        {
            Assert.AreEqual("minus 0.16", Translations.Humanize(-0.15555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize12()
        {
            Assert.AreEqual("minus 0.016", Translations.Humanize(-0.015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize13()
        {
            Assert.AreEqual("minus 0.0016", Translations.Humanize(-0.0015555555M));
        }

        [TestMethod]
        public void TestSpeechHumanize14()
        {
            Assert.AreEqual("minus 12.1", Translations.Humanize(-12.1M));
        }

        [TestMethod]
        public void TestSpeechHumanize15()
        {
            Assert.AreEqual("minus 12", Translations.Humanize(-12.01M));
        }

        [TestMethod]
        public void TestSpeechHumanize16()
        {
            Assert.AreEqual("over 430 trillion", Translations.Humanize(4.36156E14M));
        }

        [TestMethod]
        public void TestSpeechHumanize17()
        {
            Assert.AreEqual("over 940 billion", Translations.Humanize(9.4571E11M));
        }

        [TestMethod]
        public void TestSpeechHumanize18()
        {
            Assert.AreEqual("over 912 quadrillion", Translations.Humanize(9.1235E17M));
        }

        [TestMethod]
        public void TestSpeechHumanize19()
        {
            Assert.AreEqual("over 640 thousand", Translations.Humanize(6.459E5M));
        }

        [TestMethod]
        public void TestSpeechHumanize20()
        {
            Assert.AreEqual("456", Translations.Humanize(456));
        }

        [TestMethod]
        public void TestSpeechHumanize21()
        {
            Assert.AreEqual("1.8 million", Translations.Humanize(1.8E6M));
        }

        [TestMethod]
        public void TestSpeechHumanize22()
        {
            Assert.AreEqual("1.8 million", Translations.Humanize(1800001));
        }

        [TestMethod]
        public void TestSpeechHumanize23()
        {
            Assert.AreEqual("minus 1000", Translations.Humanize(-1000));
        }
    }
}