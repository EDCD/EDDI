using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace UnitTests
{
    [TestClass]
    public class RedactionTests : TestBase
    {
        private void TestRoundTrip(string source)
        {
            string rawPath = source != null ? Environment.ExpandEnvironmentVariables(source) : null;
            string redacted = Utilities.Redaction.RedactEnvironmentVariables(rawPath);
            string expected = source?.Replace("%TMP%", "%TEMP%"); // these are exact synonyms and we normalise on %TEMP%
            Assert.AreEqual(expected, redacted);
        }

        [TestMethod]
        public void TestNullRedaction()
        {
            string source = null;
            TestRoundTrip(source);
        }

        [TestMethod]
        public void TestEmptyRedaction()
        {
            string source = "";
            TestRoundTrip(source);
        }

        [TestMethod]
        public void TestAppdataRedaction()
        {
            string source = @"%APPDATA%\EDDI\eddi.json";
            TestRoundTrip(source);
        }

        [TestMethod]
        public void TestLocalappdataRedaction()
        {
            string source = @"%LOCALAPPDATA%\EDDI\eddi.json";
            TestRoundTrip(source);
        }

        [TestMethod]
        public void TestMedleyRedaction()
        {
            string source = @"ice cream %USERNAME% foo %TMP% bar %TEMP% baz %APPDATA% quux %USERNAME% womble";
            TestRoundTrip(source);
        }

        [TestMethod]
        public void TestMissingEnvVarRedaction()
        {
            string oldVal = Environment.GetEnvironmentVariable("HOMEPATH");
            Environment.SetEnvironmentVariable("HOMEPATH", null);
            string source = @"C:\EDDI\eddi.json";
            string redacted = Utilities.Redaction.RedactEnvironmentVariables(source);
            string expected = source;
            Assert.AreEqual(expected, redacted);
            Environment.SetEnvironmentVariable("HOMEPATH", oldVal);
        }

        [TestMethod]
        public void TestEmptyEnvVarRedaction()
        {
            string oldVal = Environment.GetEnvironmentVariable("HOMEPATH");
            Environment.SetEnvironmentVariable("HOMEPATH", "");
            string source = @"C:\EDDI\eddi.json";
            string redacted = Utilities.Redaction.RedactEnvironmentVariables(source);
            string expected = source;
            Assert.AreEqual(expected, redacted);
            Environment.SetEnvironmentVariable("HOMEPATH", oldVal);
        }

        [TestMethod]
        public void TestBackslashEnvVarRedaction()
        {
            string oldVal = Environment.GetEnvironmentVariable("HOMEPATH");
            Environment.SetEnvironmentVariable("HOMEPATH", @"\");
            string source = @"C:\EDDI\eddi.json";
            string redacted = Utilities.Redaction.RedactEnvironmentVariables(source);
            string expected = source;
            Assert.AreEqual(expected, redacted);
            Environment.SetEnvironmentVariable("HOMEPATH", oldVal);
        }
    }
}
