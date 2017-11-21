using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using static Utilities.Diff;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class DiffTests
    {
        [TestMethod]
        public void TestDiff1()
        {
            string a = "Line 1\r\nLine 2\r\nLine 3";
            string b = "Line 1\r\nLine 3";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(3, diffItems.Count);
            Assert.AreEqual("Unmodified", diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual("Deleted", diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual("Unmodified", diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }

        [TestMethod]
        public void TestDiff2()
        {
            string a = "The quick brown fox jumped over the lazy dog";
            string b = "The quick brown fox jumps over the lazy dog";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(2, diffItems.Count);
            Assert.AreEqual(diffItems[0].type, "Deleted");
            Assert.AreEqual("The quick brown fox jumped over the lazy dog", diffItems[0].data);
            Assert.AreEqual(diffItems[1].type, "Inserted");
            Assert.AreEqual("The quick brown fox jumps over the lazy dog", diffItems[1].data);
        }

        [TestMethod]
        public void TestDiff3()
        {
            string a = "Line 1\r\nLine 2\r\nLine 3";

            List<DiffItem> diffItems = DiffTexts(a, a);

            Assert.AreEqual(3, diffItems.Count);
            Assert.AreEqual("Unmodified", diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual("Unmodified", diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual("Unmodified", diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }

        [TestMethod]
        public void TestDiff4()
        {
            string a = "Line 1\r\nLine 2\r\nLine 3";
            string b = "Something completely different\r\n\r\n";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(6, diffItems.Count);
            Assert.AreEqual("Deleted", diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual("Deleted", diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual("Deleted", diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
            Assert.AreEqual("Inserted", diffItems[3].type);
            Assert.AreEqual("Something completely different", diffItems[3].data);
            Assert.AreEqual("Inserted", diffItems[4].type);
            Assert.AreEqual("", diffItems[4].data);
            Assert.AreEqual("Inserted", diffItems[5].type);
            Assert.AreEqual("", diffItems[5].data);
        }

        [TestMethod]
        public void TestDiff5()
        {
            string a = "";
            string b = "Line 1\r\nLine 2\r\nLine 3";

            List<DiffItem> diffItems = DiffTexts(a, b);

            Assert.AreEqual(3, diffItems.Count);
            Assert.AreEqual("Inserted", diffItems[0].type);
            Assert.AreEqual("Line 1", diffItems[0].data);
            Assert.AreEqual("Inserted", diffItems[1].type);
            Assert.AreEqual("Line 2", diffItems[1].data);
            Assert.AreEqual("Inserted", diffItems[2].type);
            Assert.AreEqual("Line 3", diffItems[2].data);
        }
    }
}
