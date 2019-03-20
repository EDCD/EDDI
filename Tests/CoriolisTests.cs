using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class CoriolisTests
    {
        [TestMethod]
        public void TestUri()
        {
            string data = "BZ+24 123";
            string uriData = Uri.EscapeDataString(data);
            Assert.AreEqual("BZ%2B24%20123", uriData);
        }
    }
}
