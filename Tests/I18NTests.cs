using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utilities;
using System;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class I18NTests
    {
        [TestMethod]
        public void EnStringTest()
        {
        }

        [TestMethod]
        public void FallbackStringTest()
        {

        }

        [TestMethod]
        public void LangPathTest()
        {
            Console.WriteLine(I18N.getLangPath());
        }

        [TestMethod]
        public void AvailableLangTest()
        {
            List<string> langs = I18N.getAvailableLang();
            Assert.IsTrue(langs.Contains("en"));
            Assert.IsTrue(langs.Contains("fr"));
            foreach (string lang in langs)
                Console.WriteLine(lang + " is available");
        }
    }
}
