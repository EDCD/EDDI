using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using static Utilities.Diff;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class DiffTests
    {
        [TestMethod]
        public void TestDiff1()
        {
            var a = "Line 1\r\nLine 2\r\nLine 3";
            var b = "Line 1\r\nLine 3";

            var diffItems = DiffTexts(a, b);

            Assert.HasCount( 3, diffItems);
            Assert.AreEqual(DiffItem.DiffType.Unmodified, diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Deleted, diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual(DiffItem.DiffType.Unmodified, diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }

        [TestMethod]
        public void TestDiff2()
        {
            var a = "The quick brown fox jumped over the lazy dog";
            var b = "The quick brown fox jumps over the lazy dog";

            var diffItems = DiffTexts(a, b);

            Assert.HasCount( 2, diffItems);
            Assert.AreEqual(DiffItem.DiffType.Deleted, diffItems[0].type);
            Assert.AreEqual("The quick brown fox jumped over the lazy dog", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[1].type);
            Assert.AreEqual("The quick brown fox jumps over the lazy dog", diffItems[1].data);
        }

        [TestMethod]
        public void TestDiff3()
        {
            var a = "Line 1\r\nLine 2\r\nLine 3";

            var diffItems = DiffTexts(a, a);

            Assert.HasCount( 3, diffItems );
            Assert.AreEqual(DiffItem.DiffType.Unmodified, diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Unmodified, diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual(DiffItem.DiffType.Unmodified, diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }

        [TestMethod]
        public void TestDiff4()
        {
            var a = "Line 1\r\nLine 2\r\nLine 3";
            var b = "Something completely different\r\n\r\n";

            var diffItems = DiffTexts(a, b);

            Assert.HasCount( 6, diffItems );
            Assert.AreEqual(DiffItem.DiffType.Deleted, diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Deleted, diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual(DiffItem.DiffType.Deleted, diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[3].type);
            Assert.AreEqual("Something completely different", diffItems[3].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[4].type);
            Assert.AreEqual("", diffItems[4].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[5].type);
            Assert.AreEqual("", diffItems[5].data);
        }

        [TestMethod]
        public void TestDiff5()
        {
            var a = "";
            var b = "Line 1\r\nLine 2\r\nLine 3";

            var diffItems = DiffTexts(a, b);

            Assert.HasCount( 3, diffItems );
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }
    }
}
