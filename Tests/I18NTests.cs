using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Resources;
using Utilities;

namespace Tests
{
    [TestClass]
    public class I18NTests
    {
        [TestInitialize]
        public void TestInitialize()
        {
            I18N.Reset();
        }

        [TestMethod]
        public void GetAvailableLangsTest()
        {
            List<string> langs = I18N.GetAvailableLangs();
            Assert.IsTrue(langs.Count > 0);
            Assert.IsTrue(langs.Contains("en"));
            Assert.IsFalse(langs.Contains("IDontExist"));

            Assert.IsTrue(I18N.IsAvailableLang("en"));
            Assert.IsTrue(I18N.IsAvailableLang("fr"));
            Assert.IsFalse(I18N.IsAvailableLang("IDontExist"));
        }

        [TestMethod]
        public void GetStringDefault()
        {
            Assert.AreEqual("it works (en)", I18N.GetString("test_string"));
        }

        [TestMethod]
        public void GetStringWithSpecifiedLangTest()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach (string lang in langs)
            {
                Assert.AreEqual("it works ("+lang+")", I18N.GetString("test_string", lang));
            }
        }

        [TestMethod]
        public void GetStringFallbackEn()
        {
            Assert.AreEqual("it works (en)", I18N.GetString("test_string"), "IDontExist");
        }

        [TestMethod]
        public void GetStringWithLangChanges()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach(string lang in langs)
            {
                Assert.IsTrue(I18N.SetLang(lang));
                Assert.AreEqual("it works (" + lang + ")", I18N.GetString("test_string"));
            }
        }

        [TestMethod]
        public void SetLangOrFallbackTest()
        {
            Assert.IsTrue(I18N.SetLangOrFallback("fr"));
            Assert.IsFalse(I18N.SetLangOrFallback("IDontExist"));
            Assert.AreEqual("en", I18N.GetLang());
        }
    }
}
