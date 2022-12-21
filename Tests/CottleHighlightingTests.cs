using EddiSpeechResponder.AvalonEdit;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Windows.Media;

namespace UnitTests
{
    [TestClass]
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
            HighlightingColor highlightingColor = cottleHighlighting.Definition.GetNamedColor("Comment");
            HighlightingBrush brush = highlightingColor.Background;
            Color? newColor = brush.GetColor(null);
            Assert.AreEqual(Colors.BlanchedAlmond, newColor);
        }

        [TestMethod]
        public void TestSetUnsupportedColorKey()
        {
            Assert.ThrowsException<KeyNotFoundException>(
                () => cottleHighlighting.SetBackgroundColor("NotInTheGrammar", Colors.BlanchedAlmond)
                );
        }
    }
}
