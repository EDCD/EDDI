using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace UnitTests
{
    [TestClass]
    public class VersioningTests
    {
        [TestMethod]
        public void TestBetaVersionToString()
        {
            Version v = new Version(1, 2, 3, Version.TestPhase.b, 4);
            string s = v.ToString();
            Assert.AreEqual("1.2.3-b4", s);
        }

        [TestMethod]
        public void TestVersionWithBetaPhaseAsString()
        {
            Version v = new Version(1, 2, 3, "b", 4);
            Assert.AreEqual(Version.TestPhase.b, v.phase);
        }

        [TestMethod]
        public void TestBetaVersionShortString()
        {
            Version v = new Version(1, 2, 3, Version.TestPhase.b, 4);
            string s = v.ShortString;
            Assert.AreEqual("1.2.3", s);
        }

        [TestMethod]
        public void TestVersionWithInvalidPhaseAsStringThows()
        {
            try
            {
                Version v = new Version(1, 2, 3, "invalid", 4);
            }
            catch(System.ArgumentException)
            {
                // pass
                return;
            }
            Assert.Fail("Expected an ArgumentException");
        }

        [TestMethod]
        public void TestFinalVersionToString()
        {
            Version v = new Version(1, 2, 3, Version.TestPhase.final, 0);
            string s = v.ToString();
            Assert.AreEqual("1.2.3", s);
        }

        [TestMethod]
        public void TestShortFinalVersionToString()
        {
            Version v = new Version(1, 2, 3);
            string s = v.ToString();
            Assert.AreEqual("1.2.3", s);
        }

        [TestMethod]
        public void TestParseBetaVersion()
        {
            string s = "1.2.3-b4";
            Version v = new Version(s);
            Assert.AreEqual(1, v.major);
            Assert.AreEqual(2, v.minor);
            Assert.AreEqual(3, v.patch);
            Assert.AreEqual(Version.TestPhase.b, v.phase);
            Assert.AreEqual(4, v.iteration);
        }

        [TestMethod]
        public void TestParseFinalVersion()
        {
            string s = "1.2.3";
            Version v = new Version(s);
            Assert.AreEqual(1, v.major);
            Assert.AreEqual(2, v.minor);
            Assert.AreEqual(3, v.patch);
            Assert.AreEqual(Version.TestPhase.final, v.phase);
            Assert.AreEqual(0, v.iteration);
        }

        [TestMethod]
        public void TestParseInvalidVersion()
        {
            string s = "totally invalid string";
            try
            {
                Version v = new Version(s);
            }
            catch (System.Exception)
            {
                // pass
                return;
            }
            Assert.Fail("Expected an Exception");
        }

        [TestMethod]
        public void TestParseInvalidPhase()
        {
            string s = "1.2.3-invalid42";
            try
            {
                Version v = new Version(s);
            }
            catch (System.Exception)
            {
                // pass
                return;
            }
            Assert.Fail("Expected an Exception");
        }

        [TestMethod]
        public void TestEquality()
        {
            Version v1 = new Version(1, 2, 3, "b", 4);
            Version v2 = new Version(1, 2, 3, "b", 4);
            Assert.IsTrue(v1 == v2);
        }

        [TestMethod]
        public void TestInEquality()
        {
            Version v1 = new Version(1, 2, 3, "b", 4);
            Version v2 = new Version(1, 2, 3, "b", 5);
            Assert.IsTrue(v1 != v2);
        }

        [TestMethod]
        public void TestLessThan()
        {
            Version v1 = new Version(1, 2, 3, "b", 4);
            Version v2 = new Version(1, 2, 3, "b", 5);
            Assert.IsTrue(v1 < v2);
        }

        [TestMethod]
        public void TestLessThanOrEqual()
        {
            Version v1 = new Version(1, 2, 3, "b", 4);
            Version v2 = new Version(1, 2, 3, "b", 4);
            Assert.IsTrue(v1 <= v2);
        }

        [TestMethod]
        public void TestGreaterThan()
        {
            Version v1 = new Version(1, 2, 3, "b", 4);
            Version v2 = new Version(1, 2, 3, "b", 5);
            Assert.IsTrue(v2 > v1);
        }

        [TestMethod]
        public void TestGreaterThanOrEqual()
        {
            Version v1 = new Version(1, 2, 3, "b", 4);
            Version v2 = new Version(1, 2, 3, "b", 4);
            Assert.IsTrue(v2 >= v1);
        }

        [TestMethod]
        public void TestVersion1()
        {
            Assert.AreEqual(1, Version.CompareStrings("1.1.0", "1.0.1"));
        }

        [TestMethod]
        public void TestVersion2()
        {
            Assert.AreEqual(0, Version.CompareStrings("1.1.0", "1.1.0"));
        }

        [TestMethod]
        public void TestVersion3()
        {
            Assert.AreEqual(-1, Version.CompareStrings("1.0.0-b1", "1.0.0-b2"));
        }

        [TestMethod]
        public void TestVersion4()
        {
            Assert.AreEqual(-1, Version.CompareStrings("1.0.0-b1", "1.0.0"));
        }

        [TestMethod]
        public void TestVersion5()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.0.10", "2.0.11"));
        }

        [TestMethod]
        public void TestVersion6()
        {
            Assert.AreEqual(-1, Version.CompareStrings("1.0.0-a5", "1.0.0-b1"));
        }

        [TestMethod]
        public void TestVersion7()
        {
            Assert.AreEqual(-1, Version.CompareStrings("1.0.0", "1.0.1-a5"));
        }

        [TestMethod]
        public void TestVersion8()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.1.0-b3", "2.1.0"));
        }

        [TestMethod]
        public void TestVersionAlphaToRC()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.1.0-a3", "2.1.0-rc1"));
        }

        [TestMethod]
        public void TestVersionBetaToRC()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.1.0-b3", "2.1.0-rc1"));
        }

        [TestMethod]
        public void TestVersionRCToRC()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.1.0-rc1", "2.1.0-rc2"));
        }

        [TestMethod]
        public void TestVersionRCToFinal()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.1.0-rc3", "2.1.0"));
        }

        [TestMethod]
        public void TestVersionOlderFinalToRC()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.0.0", "2.1.0-rc1"));
        }

        [TestMethod]
        public void TestVersionRCToNewerAlpha()
        {
            Assert.AreEqual(-1, Version.CompareStrings("2.1.0-rc1", "2.2.0-a1"));
        }
    }
}
