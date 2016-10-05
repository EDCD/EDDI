using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using System;
using MathNet.Numerics.Distributions;
using EddiEvents;
using Utilities;

namespace Tests
{
    [TestClass]
    public class VersioningTests
    {
        [TestMethod]
        public void TestVersion1()
        {
            Assert.AreEqual(1, Versioning.Compare("1.1", "1.0"));
        }

        [TestMethod]
        public void TestVersion2()
        {
            Assert.AreEqual(0, Versioning.Compare("1.1", "1.1.0"));
        }

        [TestMethod]
        public void TestVersion3()
        {
            Assert.AreEqual(-1, Versioning.Compare("1.0.0b1", "1.0.0b2"));
        }

        [TestMethod]
        public void TestVersion4()
        {
            Assert.AreEqual(-1, Versioning.Compare("1.0.0a2", "1.0.0b1"));
        }
    }
}
