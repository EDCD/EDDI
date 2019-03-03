using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class RedactionTests : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        [TestMethod]
        public void TestAppdataRedaction()
        {
            string source = @"%APPDATA%\EDDI\eddi.json";
            string rawPath = Environment.ExpandEnvironmentVariables(source);
            string redacted = Utilities.Redaction.RedactEnvironmentVariables(rawPath);
            Assert.AreEqual(source, redacted);
        }
    }
}
