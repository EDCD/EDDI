using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class HumanizeTests
    {
        [TestMethod]
        public void TestNullInput()
        {
            Assert.IsNull(Translations.Humanize(null));
        }

        [DataTestMethod]
        [DataRow(0, "zero")]
        [DataRow(456, "456")]
        [DataRow(-1000, "minus 1000")]
        [DataRow(100000, "100000")]
        [DataRow(51000001, "51000000")]
        [DataRow(-51000000, "minus 51000000")]
        [DataRow(-12345, "well over minus 12 thousand")]
        [DataRow(1800001, "1.8 million")]
        [DataRow(-1999001, "nearly minus 2 million")]
        public void TestIntToFloatingMantissa(int number, string expected)
        {
            Assert.AreEqual(expected, Translations.Humanize(number));
        }

        [DataTestMethod]
        [DataRow(-12345.0, "well over minus 12 thousand")]
        [DataRow(0.15555555, "0.16")]
        [DataRow(0.015555555, "0.016")]
        [DataRow(0.0015555555, "0.0016")]
        [DataRow(-0.15555555, "minus 0.16")]
        [DataRow(-0.015555555, "minus 0.016")]
        [DataRow(-0.0015555555, "minus 0.0016")]
        [DataRow(-12.1, "minus 12.1")]
        [DataRow(-12.01, "minus 12")]
        [DataRow(6.459E5, "over 640 thousand")]
        [DataRow(1.8E6, "1.8 million")]
        [DataRow(9.4571E11, "over 940 billion")]
        [DataRow(4.36156E14, "over 430 trillion")]
        [DataRow(9.1235E17, "over 912 quadrillion")]
        public void TestDoubleToFloatingMantissa(double number, string expected)
        {
            Assert.AreEqual(expected, Translations.Humanize((decimal)number));
        }

        [DataTestMethod]
        [DataRow(1100001, "1.1 million")]
        [DataRow(1110001, "just over 1 million")]
        [DataRow(1210001, "over 1 million")]
        [DataRow(1310001, "well over 1 million")]
        [DataRow(1410001, "nearly 1 and a half million")]
        [DataRow(1510001, "around 1 and a half million")]
        [DataRow(1610001, "over 1 and a half million")]
        [DataRow(1810001, "well over 1 and a half million")]
        [DataRow(1999001, "nearly 2 million")]
        public void Test2ndDigitRangePositive(int number, string expected)
        {
            Assert.AreEqual(expected, Translations.Humanize(number));
        }

        [DataTestMethod]
        [DataRow(-1100001, "minus 1.1 million")]
        [DataRow(-1110001, "just over minus 1 million")]
        [DataRow(-1210001, "over minus 1 million")]
        [DataRow(-1310001, "well over minus 1 million")]
        [DataRow(-1410001, "nearly minus 1 and a half million")]
        [DataRow(-1510001, "around minus 1 and a half million")]
        [DataRow(-1610001, "over minus 1 and a half million")]
        [DataRow(-1810001, "well over minus 1 and a half million")]
        [DataRow(-1999001, "nearly minus 2 million")]
        public void Test2ndDigitRangeNegative(int number, string expected)
        {
            Assert.AreEqual(expected, Translations.Humanize(number));
        }

        [DataTestMethod]
        [DataRow(111000, "111000")]
        [DataRow(111100, "just over 111 thousand")]
        [DataRow(111200, "over 111 thousand")]
        [DataRow(111700, "nearly 112 thousand")]
        public void TestNumbersWith3DigitMantissa(int number, string expected)
        {
            Assert.AreEqual(expected, Translations.Humanize(number));
        }
    }
}
