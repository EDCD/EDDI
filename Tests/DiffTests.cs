using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Utilities;
using static Utilities.Diff;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class DiffTests
    {
        [TestMethod]
        public void TestDiff1()
        {
            string a = "Line 1\r\nLine 2\r\nLine 3";
            string b = "Line 1\r\nLine 3";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(3, diffItems.Count);
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
            string a = "The quick brown fox jumped over the lazy dog";
            string b = "The quick brown fox jumps over the lazy dog";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(2, diffItems.Count);
            Assert.AreEqual(DiffItem.DiffType.Deleted, diffItems[0].type);
            Assert.AreEqual("The quick brown fox jumped over the lazy dog", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[1].type);
            Assert.AreEqual("The quick brown fox jumps over the lazy dog", diffItems[1].data);
        }

        [TestMethod]
        public void TestDiff3()
        {
            string a = "Line 1\r\nLine 2\r\nLine 3";

            List<DiffItem> diffItems = DiffTexts(a, a);

            Assert.AreEqual(3, diffItems.Count);
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
            string a = "Line 1\r\nLine 2\r\nLine 3";
            string b = "Something completely different\r\n\r\n";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(6, diffItems.Count);
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
            string a = "";
            string b = "Line 1\r\nLine 2\r\nLine 3";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(3, diffItems.Count);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual(DiffItem.DiffType.Inserted, diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }
    }
}
