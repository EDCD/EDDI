using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiDataDefinitions;
using System;
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
            IContinuousDistribution distribution = new Gamma(9, 1/5000.0);
            Console.WriteLine(distribution.CumulativeDistribution(30000));
            //IContinuousDistribution distribution = new Gamma(3, 0.05);
            for (double d = 0; d < 50000; d = d + 1000)
            {
                //Console.WriteLine(Gamma.PDF(3, 0.05, d));
                //Console.WriteLine(Gamma.CDF(3, 0.05, d));
                Console.WriteLine("" + d + ": " + distribution.CumulativeDistribution(d));
            }
        }
    }
}
