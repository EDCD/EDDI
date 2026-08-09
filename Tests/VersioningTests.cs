using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiVoiceAttackAdapter;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Utilities;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class VersioningTests
    {
        private static readonly string SolutionDirectory = Path.GetFullPath(
            Path.Combine( System.AppContext.BaseDirectory, "..", "..", ".." ) );

        [TestMethod]
        public void InstallerVersion_MatchesRuntimeConstant ()
        {
            Assert.AreEqual( Constants.EDDI_VERSION.ToString(), GetInstallerVersion() );
        }

        [TestMethod]
        public void VoiceAttackAdapterVisibleMetadata_UsesEddiVersionWithoutSourceRevisionSuffix ()
        {
            var productVersion = Constants.EDDI_VERSION.ToString();
            var fileVersion = $"{Constants.EDDI_VERSION.ShortString}.{Constants.EDDI_VERSION.iteration}";

            AssertAssemblyVisibleVersionMetadata( typeof( VoiceAttackPlugin ).Assembly.Location, productVersion, fileVersion );
        }

        [TestMethod]
        public void VoiceAttackDisplayName_UsesEddiVersion ()
        {
            Assert.AreEqual( $"EDDI {Constants.EDDI_VERSION}", VoiceAttackPlugin.VA_DisplayName() );
        }

        [TestMethod]
        public void TestBetaVersionToString()
        {
            var v = new Version(1, 2, 3, Version.TestPhase.b, 4);
            var s = v.ToString();
            Assert.AreEqual("1.2.3-b4", s);
        }

        [TestMethod]
        public void TestVersionWithBetaPhaseAsString()
        {
            var v = new Version(1, 2, 3, "b", 4);
            Assert.AreEqual(Version.TestPhase.b, v.phase);
        }

        [TestMethod]
        public void TestBetaVersionShortString()
        {
            var v = new Version(1, 2, 3, Version.TestPhase.b, 4);
            var s = v.ShortString;
            Assert.AreEqual("1.2.3", s);
        }

        [TestMethod]
        public void TestVersionWithInvalidPhaseAsStringThows()
        {
            try
            {
                var v = new Version(1, 2, 3, "invalid", 4);
            }
            catch (System.ArgumentException)
            {
                // pass
                return;
            }
            Assert.Fail("Expected an ArgumentException");
        }

        [TestMethod]
        public void TestFinalVersionToString()
        {
            var v = new Version(1, 2, 3, Version.TestPhase.final, 0);
            var s = v.ToString();
            Assert.AreEqual("1.2.3", s);
        }

        [TestMethod]
        public void TestShortFinalVersionToString()
        {
            var v = new Version(1, 2, 3);
            var s = v.ToString();
            Assert.AreEqual("1.2.3", s);
        }

        [TestMethod]
        public void TestParseBetaVersion()
        {
            var s = "1.2.3-b4";
            var v = new Version(s);
            Assert.AreEqual(1, v.major);
            Assert.AreEqual(2, v.minor);
            Assert.AreEqual(3, v.patch);
            Assert.AreEqual(Version.TestPhase.b, v.phase);
            Assert.AreEqual(4, v.iteration);
        }

        [TestMethod]
        public void TestParseFinalVersion()
        {
            var s = "1.2.3";
            var v = new Version(s);
            Assert.AreEqual(1, v.major);
            Assert.AreEqual(2, v.minor);
            Assert.AreEqual(3, v.patch);
            Assert.AreEqual(Version.TestPhase.final, v.phase);
            Assert.AreEqual(0, v.iteration);
        }

        [TestMethod]
        public void TestParseInvalidVersion()
        {
            var s = "totally invalid string";
            try
            {
                var v = new Version(s);
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
            var s = "1.2.3-invalid42";
            try
            {
                var v = new Version(s);
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
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 4);
            Assert.AreEqual(v1, v2);
        }

        [TestMethod]
        public void TestInEquality()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 5);
            Assert.AreNotEqual(v1, v2);
        }

        [TestMethod]
        public void TestHashCodeIsStable()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 4);
            var hash1 = v1.GetHashCode();
            var hash2 = v2.GetHashCode();
            Assert.AreEqual(hash1, hash2);
        }

        [TestMethod]
        public void TestInequalityToObject()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var o = new object();
            Assert.IsFalse(v1.Equals(o));
        }

        [TestMethod]
        public void TestEqualityToObject()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            object o = new Version(1, 2, 3, "b", 4);
            Assert.IsTrue(v1.Equals(o));
        }

        [TestMethod]
        public void TestMajorFieldDifferences()
        {
            var v1 = new Version(1, 5, 7, "b", 5);
            var v2 = new Version(2, 3, 4, "a", 4); // major is greater, all subordinates are lesser
            Assert.IsTrue(v1 < v2);
            Assert.IsTrue(v2 > v1);
        }

        [TestMethod]
        public void TestMinorFieldDifferences()
        {
            var v1 = new Version(1, 2, 5, "b", 5);
            var v2 = new Version(1, 3, 4, "a", 4); // minor is greater, all subordinates are lesser
            Assert.IsTrue(v1 < v2);
            Assert.IsTrue(v2 > v1);
        }

        [TestMethod]
        public void TestPatchDifferences()
        {
            var v1 = new Version(1, 2, 3, "b", 5);
            var v2 = new Version(1, 2, 4, "a", 4); // patch is greater, all subordinates are lesser
            Assert.IsTrue(v1 < v2);
            Assert.IsTrue(v2 > v1);
        }

        [TestMethod]
        public void TestPhaseDifferences()
        {
            var v1 = new Version(1, 2, 3, "a", 5);
            var v2 = new Version(1, 2, 3, "b", 4); // phase is greater, all subordinates are lesser
            Assert.IsTrue(v1 < v2);
            Assert.IsTrue(v2 > v1);
        }

        [TestMethod]
        public void TestIterationLessThan()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 5);
            Assert.IsTrue(v1 < v2);
        }

        [TestMethod]
        public void TestIterationLessThanOrEqual()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 4);
            Assert.IsTrue(v1 <= v2);
        }

        [TestMethod]
        public void TestIterationGreaterThan()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 5);
            Assert.IsTrue(v2 > v1);
        }

        [TestMethod]
        public void TestIterationGreaterThanOrEqual()
        {
            var v1 = new Version(1, 2, 3, "b", 4);
            var v2 = new Version(1, 2, 3, "b", 4);
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

        private static void AssertAssemblyVisibleVersionMetadata (
            string assemblyPath,
            string expectedProductVersion,
            string expectedFileVersion )
        {
            var versionInfo = FileVersionInfo.GetVersionInfo( assemblyPath );

            Assert.AreEqual( expectedProductVersion, versionInfo.ProductVersion );
            Assert.AreEqual( expectedFileVersion, versionInfo.FileVersion );
            Assert.IsFalse(
                versionInfo.ProductVersion?.Contains( '+' ) ?? false,
                $"Visible product metadata must not include a source revision suffix: {assemblyPath}" );
        }

        private static string GetInstallerVersion ()
        {
            var installerPath = Path.Combine( SolutionDirectory, "Installer.iss" );
            var versionLine = File.ReadLines( installerPath )
                .First( line => line.TrimStart().StartsWith( "#define MyAppVersion ", System.StringComparison.Ordinal ) );

            return versionLine.Split( '"' )[ 1 ];
        }
    }
}
