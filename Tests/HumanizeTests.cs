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
    }
}
