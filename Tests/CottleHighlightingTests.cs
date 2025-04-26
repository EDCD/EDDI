using EddiSpeechResponder.AvalonEdit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Media;

namespace Tests
{
    [TestClass, TestCategory("UnitTests")]
    public class CottleHighlightingTests : TestBase
    {
        private CottleHighlighting cottleHighlighting;

        [TestInitialize]
        public void start()
        {
            cottleHighlighting = new CottleHighlighting();
        }

        [TestMethod]
        public void TestInstantiate()
        {
            Assert.IsNotNull(cottleHighlighting);
        }

        [TestMethod]
        public void TestSetSupportedColorKey()
        {
            cottleHighlighting.SetBackgroundColor("Comment", Colors.BlanchedAlmond);
            var highlightingColor = cottleHighlighting.Definition.GetNamedColor("Comment");
            var brush = highlightingColor.Background;
            var newColor = brush.GetColor(null);
            Assert.AreEqual(Colors.BlanchedAlmond, newColor);
        }

        [TestMethod]
        public void TestSetUnsupportedColorKey()
        {
            Assert.ThrowsExactly<KeyNotFoundException>(
                () => cottleHighlighting.SetBackgroundColor("NotInTheGrammar", Colors.BlanchedAlmond)
                );
        }
    }
}
