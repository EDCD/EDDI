using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class RedactionTests : TestBase
    {
        private static void TestRoundTrip(string source)
        {
            var rawPath = source != null ? Environment.ExpandEnvironmentVariables(source) : null;
            var redacted = Redaction.RedactEnvironmentVariables(rawPath);
            var expected = source?.Replace("%TMP%", "%TEMP%"); // these are exact synonyms and we normalise on %TEMP%
            Assert.AreEqual(expected, redacted);
        }

        [TestMethod]
        public void TestNullRedaction()
        {
            TestRoundTrip( null );
        }

        [TestMethod]
        public void TestEmptyRedaction()
        {
            TestRoundTrip( "" );
        }

        [TestMethod]
        public void TestAppdataRedaction()
        {
            TestRoundTrip( @"%APPDATA%\EDDI\eddi.json" );
        }

        [TestMethod]
        public void TestLocalappdataRedaction()
        {
            TestRoundTrip( @"%LOCALAPPDATA%\EDDI\eddi.json" );
        }

        [TestMethod]
        public void TestMedleyRedaction()
        {
            TestRoundTrip( @"ice cream %USERNAME% foo %TMP% bar %TEMP% baz %APPDATA% quux %USERNAME% womble" );
        }

        [TestMethod]
        public void TestMissingEnvVarRedaction()
        {
            var oldVal = Environment.GetEnvironmentVariable("HOMEPATH");
            Environment.SetEnvironmentVariable("HOMEPATH", null);
            var source = @"C:\EDDI\eddi.json";
            var redacted = Redaction.RedactEnvironmentVariables(source);
            var expected = source;
            Assert.AreEqual(expected, redacted);
            Environment.SetEnvironmentVariable("HOMEPATH", oldVal);
        }

        [TestMethod]
        public void TestEmptyEnvVarRedaction()
        {
            var oldVal = Environment.GetEnvironmentVariable("HOMEPATH");
            Environment.SetEnvironmentVariable("HOMEPATH", "");
            var source = @"C:\EDDI\eddi.json";
            var redacted = Redaction.RedactEnvironmentVariables(source);
            var expected = source;
            Assert.AreEqual(expected, redacted);
            Environment.SetEnvironmentVariable("HOMEPATH", oldVal);
        }

        [TestMethod]
        public void TestBackslashEnvVarRedaction()
        {
            var oldVal = Environment.GetEnvironmentVariable("HOMEPATH");
            Environment.SetEnvironmentVariable("HOMEPATH", @"\");
            var source = @"C:\EDDI\eddi.json";
            var redacted = Redaction.RedactEnvironmentVariables(source);
            var expected = source;
            Assert.AreEqual(expected, redacted);
            Environment.SetEnvironmentVariable("HOMEPATH", oldVal);
        }
    }
}
