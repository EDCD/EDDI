using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;
using System.Collections.Generic;
using System;

namespace Tests
{
    [TestClass]
    public class VoiceAttackPluginTests
    {
        [TestMethod]
        public void TestVAPluginHumanize1()
        {
            Assert.AreEqual("on the way to 12 and a half thousand", VoiceAttackPlugin.humanize(12345));
        }

        [TestMethod]
        public void TestVAPluginHumanize2()
        {
            Assert.AreEqual(null, VoiceAttackPlugin.humanize(null));
        }

        [TestMethod]
        public void TestVAPluginHumanize3()
        {
            Assert.AreEqual("0", VoiceAttackPlugin.humanize(0));
        }


        [TestMethod]
        public void TestVAVisitStarSystems()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            Dictionary<string, short?> shortIntValues = new Dictionary<string, short?>();
            Dictionary<string, string> textValues = new Dictionary<string, string>();
            Dictionary<string, int?> intValues = new Dictionary<string, int?>();
            Dictionary<string, decimal?> decimalValues = new Dictionary<string, decimal?>();
            Dictionary<string, bool?> booleanValues = new Dictionary<string, bool?>();
            Dictionary<string, DateTime?> dateTimeValues = new Dictionary<string, DateTime?>();
            Dictionary<string, object> extendedValues = new Dictionary<string, object>();
            VoiceAttackPlugin.VA_Init1(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

            VoiceAttackPlugin.updateSystem("LFT 926");
            VoiceAttackPlugin.VA_Invoke1("system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

            VoiceAttackPlugin.updateSystem("Shinrarta Dezhra");
            VoiceAttackPlugin.VA_Invoke1("system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

            VoiceAttackPlugin.updateSystem("LFT 926");
            VoiceAttackPlugin.VA_Invoke1("system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
        }

        [TestMethod]
        public void TestOutfittingCosts()
        {
            Dictionary<string, object> state = new Dictionary<string, object>();
            Dictionary<string, short?> shortIntValues = new Dictionary<string, short?>();
            Dictionary<string, string> textValues = new Dictionary<string, string>();
            Dictionary<string, int?> intValues = new Dictionary<string, int?>();
            Dictionary<string, decimal?> decimalValues = new Dictionary<string, decimal?>();
            Dictionary<string, bool?> booleanValues = new Dictionary<string, bool?>();
            Dictionary<string, DateTime?> dateTimeValues = new Dictionary<string, DateTime?>();
            Dictionary<string, object> extendedValues = new Dictionary<string, object>();
            VoiceAttackPlugin.VA_Init1(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
        }

        [TestMethod]
        public void TestTranslateStarSystems()
        {
            Assert.AreEqual("Hip 1 2 3 4 5", VATranslations.StarSystem("HIP 12345"));
            Assert.AreEqual("LHS 1 2 3 4 5", VATranslations.StarSystem("LHS 12345"));
            Assert.AreEqual("HR 1 2 3 4 5", VATranslations.StarSystem("HR 12345"));
            Assert.AreEqual("V 1 3 9 7 Orionis", VATranslations.StarSystem("V1397 Orionis"));
            Assert.AreEqual("2 mass J 2 1 5 4 1 8 7 7 plus 4 7 1 2 0 9 6", VATranslations.StarSystem("2MASS J21541877+4712096"));
            Assert.AreEqual("CXOU J 0 6 1 7 0 5 point 3 plus 2 2 2 1 2 7", VATranslations.StarSystem("CXOU J061705.3+222127"));
            Assert.AreEqual("SDSS J 1 4 1 6 plus 1 3 4 8", VATranslations.StarSystem("SDSS J1416+1348"));
            Assert.AreEqual("CFBDSIR 1 4 5 8 plus 1 0 B", VATranslations.StarSystem("CFBDSIR 1458+10 B"));
            Assert.AreEqual("BD plus 1 8 7 1 1", VATranslations.StarSystem("BD+18 711"));
            Assert.AreEqual("UGCS J 1 2 2 0 3 1 point 5 6 plus 2 4 3 6 1 4 point 8", VATranslations.StarSystem("UGCS J122031.56+243614.8"));
            Assert.AreEqual("XTE J 1 7 4 8 minus 2 8 8", VATranslations.StarSystem("XTE J1748-288"));
            Assert.AreEqual("XTE PSR J 1 8 1 0 minus 1 9 7", VATranslations.StarSystem("XTE PSR J1810-197"));
            Assert.AreEqual("Gliese 3 9 8 point 2", VATranslations.StarSystem("Gliese 398.2"));
            Assert.AreEqual("", VATranslations.StarSystem(""));
            Assert.AreEqual("", VATranslations.StarSystem(""));
        }
    }
}
