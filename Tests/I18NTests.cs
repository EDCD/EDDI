using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Utilities;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Tests
{
    [TestClass]
    public class I18NTests
    {
        private static JObject json = JObject.Parse(File.ReadAllText(I18N.langsFile));

        [TestInitialize]
        public void TestInitialize()
        {
            I18N.Reset();
            I18N.FallbackLang(); //for tests to works properly, lang needs to by defaultLang (en)
        }

        [TestMethod]
        public void TestInitIsCorrect()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach(string lang in langs)
            {
                Assert.IsTrue(I18N.GetString("test_string").StartsWith("it works"));
            }
            Assert.AreNotEqual(I18N.GetString("test_string", "en"), I18N.GetString("test_string", "fr"));
            Assert.AreEqual(langs.Count, json["langs"].ToObject<List<string>>().Count);
            Assert.AreEqual(I18N.GetKeys().Count, json["translations"].ToList().Count);
            Assert.IsNull(I18N.GetString("test_null"));
        }

        [TestMethod]
        public void TestGetAvailableLangsTest()
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
        public void TestGetStringDefault()
        {
            Assert.AreEqual("it works (en)", I18N.GetString("test_string"));
        }

        [TestMethod]
        public void TestGetStringWithSpecifiedLangTest()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach (string lang in langs)
            {
                Assert.AreEqual("it works ("+lang+")", I18N.GetString("test_string", lang));
            }
        }

        [TestMethod]
        public void TestGetStringFallbackEn()
        {
            Assert.AreEqual("it works (en)", I18N.GetString("test_string"), "IDontExist");
        }

        [TestMethod]
        public void TestGetStringWithLangChanges()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach(string lang in langs)
            {
                Assert.IsTrue(I18N.SetLang(lang));
                Assert.AreEqual("it works (" + lang + ")", I18N.GetString("test_string"));
            }
        }

        [TestMethod]
        public void TestSetLangOrFallbackTest()
        {
            Assert.IsTrue(I18N.SetLangOrFallback("fr"));
            Assert.IsFalse(I18N.SetLangOrFallback("IDontExist"));
            Assert.AreEqual("en", I18N.GetLang());
        }

        [TestMethod]
        public void TestGetStringWithArgs()
        {
            Assert.AreEqual("arg0:test0 arg1:test1", I18N.GetStringWithArgs("test_string_args", new string[] { "test0", "test1" }));
        }

        [TestMethod]
        public void TestGetStringWithArgsWithLangChanges()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach (string lang in langs)
            {
                Assert.IsTrue(I18N.SetLang(lang));
                Assert.AreEqual("arg0:test0 arg1:test1 ("+lang+")", I18N.GetStringWithArgs("test_string_args_lang", new string[] { "test0", "test1" }));
            }
        }

        [TestMethod]
        public void TestGetStringWithArgsWithSpecifiedLang()
        {
            List<string> langs = I18N.GetAvailableLangs();
            foreach (string lang in langs)
            {
                Assert.IsTrue(I18N.SetLang(lang));
                Assert.AreEqual("arg0:test0 arg1:test1 (" + lang + ")", I18N.GetStringWithArgs("test_string_args_lang", new string[] { "test0", "test1" }));
            }
        }
    }
}
