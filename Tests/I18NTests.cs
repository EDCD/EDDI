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
        private Utilities.I18N bundle;

        [TestInitialize]
        public void TestInitialize()
        {
            bundle = Utilities.I18N.ResetInstance();
        }

        [TestMethod]
        public void GetInstanceTest()
        {
            Assert.IsNotNull(bundle);
        }

        [TestMethod]
        public void GetAvailableLangsTest()
        {
            List<string> langs = bundle.GetAvailableLangs();
            Assert.IsTrue(langs.Count > 0);
            Assert.IsTrue(langs.Contains("en"));
            Assert.IsFalse(langs.Contains("IDontExist"));

            Assert.IsTrue(bundle.IsAvailableLang("en"));
            Assert.IsTrue(bundle.IsAvailableLang("fr"));
            Assert.IsFalse(bundle.IsAvailableLang("IDontExist"));
        }

        [TestMethod]
        public void GetStringDefault()
        {
            Assert.AreEqual("it works (en)", bundle.GetString("test_string"));
        }

        [TestMethod]
        public void GetStringWithSpecifiedLangTest()
        {
            List<string> langs = bundle.GetAvailableLangs();
            foreach (string lang in langs)
            {
                Assert.AreEqual("it works ("+lang+")", bundle.GetString("test_string", lang));
            }
        }

        [TestMethod]
        public void GetStringFallbackEn()
        {
            Assert.AreEqual("it works (en)", bundle.GetString("test_string"), "IDontExist");
        }

        [TestMethod]
        public void GetStringWithLangChanges()
        {
            List<string> langs = bundle.GetAvailableLangs();
            foreach(string lang in langs)
            {
                Assert.IsTrue(bundle.SetLang(lang));
                Assert.AreEqual("it works (" + lang + ")", bundle.GetString("test_string"));
            }
        }

        [TestMethod]
        public void SetLangOrFallbackTest()
        {
            Assert.IsTrue(bundle.SetLangOrFallback("fr"));
            Assert.IsFalse(bundle.SetLangOrFallback("IDontExist"));
            Assert.AreEqual("en", bundle.GetLang());
        }
    }
}
