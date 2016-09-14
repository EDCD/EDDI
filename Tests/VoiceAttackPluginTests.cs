using Microsoft.VisualStudio.TestTools.UnitTesting;
using EliteDangerousSpeechService;
using EDDIVAPlugin;
using System.Collections.Generic;
using System;
using EDDI;
using EliteDangerousDataDefinitions;
using EliteDangerousDataProviderService;

namespace Tests
{
    [TestClass]
    [DeploymentItem(@"x86\SQLite.Interop.dll", "x86")]
    public class VoiceAttackPluginTests
    {
        //[TestMethod]
        //public void TestVAVisitStarSystems()
        //{
        //    Dictionary<string, object> state = new Dictionary<string, object>();
        //    Dictionary<string, short?> shortIntValues = new Dictionary<string, short?>();
        //    Dictionary<string, string> textValues = new Dictionary<string, string>();
        //    Dictionary<string, int?> intValues = new Dictionary<string, int?>();
        //    Dictionary<string, decimal?> decimalValues = new Dictionary<string, decimal?>();
        //    Dictionary<string, bool?> booleanValues = new Dictionary<string, bool?>();
        //    Dictionary<string, DateTime?> dateTimeValues = new Dictionary<string, DateTime?>();
        //    Dictionary<string, object> extendedValues = new Dictionary<string, object>();
        //    VoiceAttackPlugin.VA_Init1(ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

        //    VoiceAttackPlugin.updateSystem("LFT 926");
        //    VoiceAttackPlugin.VA_Invoke1("system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

        //    VoiceAttackPlugin.updateSystem("Shinrarta Dezhra");
        //    VoiceAttackPlugin.VA_Invoke1("system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);

        //    VoiceAttackPlugin.updateSystem("LFT 926");
        //    VoiceAttackPlugin.VA_Invoke1("system", ref state, ref shortIntValues, ref textValues, ref intValues, ref decimalValues, ref booleanValues, ref dateTimeValues, ref extendedValues);
        //}

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
            Assert.AreEqual("Hip 1 2 3 4 5", Translations.StarSystem("HIP 12345"));
            Assert.AreEqual("L H S 1 2 3 4 5", Translations.StarSystem("LHS 12345"));
            Assert.AreEqual("H R 1 2 3 4 5", Translations.StarSystem("HR 12345"));
            Assert.AreEqual("V1 3 9 7 Orionis", Translations.StarSystem("V1397 Orionis"));
            Assert.AreEqual("2 mass J2 1 5 4 1 8 7 7 plus 4 7 1 2 0 9 6", Translations.StarSystem("2MASS J21541877+4712096"));
            Assert.AreEqual("C X O U J0 6 1 7 0 5 point 3 plus 2 2 2 1 2 7", Translations.StarSystem("CXOU J061705.3+222127"));
            Assert.AreEqual("S D S S J1 4 1 6 plus 1 3 4 8", Translations.StarSystem("SDSS J1416+1348"));
            Assert.AreEqual("C F B D S I R 1 4 5 8 plus 10 B", Translations.StarSystem("CFBDSIR 1458+10 B"));
            Assert.AreEqual("B D plus 18 7 1 1", Translations.StarSystem("BD+18 711"));
            Assert.AreEqual("U G C S J1 2 2 0 3 1 point 5 6 plus 2 4 3 6 1 4 point 8", Translations.StarSystem("UGCS J122031.56+243614.8"));
            Assert.AreEqual("X T E J1 7 4 8 minus 2 8 8", Translations.StarSystem("XTE J1748-288"));
            Assert.AreEqual("X T E P S R J1 8 1 0 minus 1 9 7", Translations.StarSystem("XTE PSR J1810-197"));
            Assert.AreEqual("Gliese 3 9 8 point 2", Translations.StarSystem("Gliese 398.2"));
            Assert.AreEqual("Coll 2 8 5 Sector Q H dash U c3 dash 22", Translations.StarSystem("Col 285 Sector QH-U c3-22"));
            Assert.AreEqual("Hip 72", Translations.StarSystem("HIP 72"));
            Assert.AreEqual("", Translations.StarSystem(""));
        }

        [TestMethod]
        [DeploymentItem(@"..\..\starsystems.txt")]
        public void TestTranslateStarSystemsStability()
        {
            string[] starSystems = System.IO.File.ReadAllLines(@"starsystems.txt");
            foreach (string starSystem in starSystems)
            {
                Translations.StarSystem(starSystem);
            }
        }

        [TestMethod]
        public void TestTranslateCallsigns()
        {
            Assert.AreEqual("<phoneme alphabet=\"ipa\" ph=\"ɡɒlf\">golf</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈælfə\">alpha</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈeksˈrei\">x-ray</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈwʌn\">one</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈzɪərəʊ\">zero</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈnaɪnər\">niner</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈfoʊ.ər\">fawer</phoneme>", Translations.CallSign("GAX-1094"));
        }

        [TestMethod]
        public void TestSqlRepositoryPresent()
        {
            StarSystemRepository starSystemRepository = StarSystemSqLiteRepository.Instance;
            StarSystem DBData = starSystemRepository.GetStarSystem("Nangkano");
            Assert.IsNotNull(DBData);
            Assert.AreEqual("Nangkano", DBData.name);
        }

        [TestMethod]
        public void TestSqlRepositoryMissing()
        {
            StarSystemRepository starSystemRepository = StarSystemSqLiteRepository.Instance;
            StarSystem DBData = starSystemRepository.GetStarSystem("Not here");
            Assert.IsNull(DBData);
        }
    }
}