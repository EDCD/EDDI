using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using System.Collections.Generic;
using System;
using EDDIVAPlugin;

namespace Tests
{
    [TestClass]
    public class StarTests
    {
        [TestMethod]
        public void TestStarEravarenth()
        {
            decimal temp = StarClass.temperature(6.885925M, 526252032M);

            Assert.AreEqual(4138, temp);
        }
    }
}
