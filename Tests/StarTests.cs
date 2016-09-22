using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteDangerousCompanionAppService;
using EliteDangerousDataDefinitions;
using System.Collections.Generic;
using System;
using EDDIVAPlugin;
using MathNet.Numerics.Distributions;

namespace Tests
{
    [TestClass]
    public class StarTests
    {
        [TestMethod]
        public void TestStarEravarenth()
        {
            decimal temp = StarClass.temperature(StarClass.luminosity(6.885925M), 526252032M);

            Assert.AreEqual(4138, (int)temp);
        }

        [TestMethod]
        public void TestStarUniformDistribution()
        {
            IContinuousDistribution distribution = new ContinuousUniform(0.08, 0.6);
            double cdf = distribution.CumulativeDistribution(0.8);
            Assert.AreEqual(1, cdf);
        }
    }
}
