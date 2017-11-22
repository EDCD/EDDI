﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;

namespace Tests
{
    [TestClass]
    public class VersioningTests
    {
        [TestMethod]
        public void TestVersion1()
        {
            Assert.AreEqual(1, Versioning.Compare("1.1.0", "1.0.1"));
        }

        [TestMethod]
        public void TestVersion2()
        {
            Assert.AreEqual(0, Versioning.Compare("1.1.0", "1.1.0"));
        }

        [TestMethod]
        public void TestVersion3()
        {
            Assert.AreEqual(-1, Versioning.Compare("1.0.0-b1", "1.0.0-b2"));
        }

        [TestMethod]
        public void TestVersion4()
        {
            Assert.AreEqual(-1, Versioning.Compare("1.0.0-b1", "1.0.0"));
        }

        [TestMethod]
        public void TestVersion5()
        {
            Assert.AreEqual(-1, Versioning.Compare("2.0.10", "2.0.11"));
        }

        [TestMethod]
        public void TestVersion6()
        {
            Assert.AreEqual(-1, Versioning.Compare("1.0.0-a5", "1.0.0-b1"));
        }

        [TestMethod]
        public void TestVersion7()
        {
            Assert.AreEqual(-1, Versioning.Compare("1.0.0", "1.0.1-a5"));
        }

        [TestMethod]
        public void TestVersion8()
        {
            Assert.AreEqual(-1, Versioning.Compare("2.1.0-b3", "2.1.0"));
        }
    }
}
