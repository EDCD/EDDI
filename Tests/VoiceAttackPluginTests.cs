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
    }
}
