using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tests.Properties;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class TranslationTests
    {
        [TestMethod]
        public void TestICAOEmpty()
        {
            string source = "";
            string icao = Translations.ICAO(source);
            Assert.AreEqual("", icao);
        }

        [TestMethod]
        public void TestICAO1Char()
        {
            string source = "a";
            string icao = Translations.ICAO(source);
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme>", icao);
        }

        [TestMethod]
        public void TestICAO2Chars()
        {
            string source = "ab";
            string icao = Translations.ICAO(source);
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>", icao);
        }

        [TestMethod]
        public void TestICAO3Chars()
        {
            string source = "abc";
            string icao = Translations.ICAO(source);
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme>", icao);
        }

        [TestMethod]
        public void TestICAOAllSymbols()
        {
            string source = "$@!^";
            string icao = Translations.ICAO(source);
            Assert.AreEqual("", icao);
        }

        [TestMethod]
        public void TestTranslateCallsigns()
        {
            Assert.AreEqual("<phoneme alphabet=\"ipa\" ph=\"ɡɒlf\">golf</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈælfə\">alpha</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈeksˈrei\">x-ray</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈwʌn\">one</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈzɪərəʊ\">zero</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈnaɪnər\">niner</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈfoʊ.ər\">fawer</phoneme>", Translations.ICAO("GAX-1094"));
        }

        [TestMethod]
        public void TestTranslateBody1()
        {
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <say-as interpret-as=""characters"">A</say-as> 1", Translations.GetTranslation("Shinrarta Dezhra A 1"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.GetTranslation("Shinrarta Dezhra A 1", true));
        }

        [TestMethod]
        public void TestTranslateBody2()
        {
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> A B 1", Translations.GetTranslation("Shinrarta Dezhra AB 1"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.GetTranslation("Shinrarta Dezhra AB 1", true));
        }

        [TestMethod]
        public void TestTranslateBody3()
        {
            Assert.AreEqual(@"Coll 1 0 7 Sector A I dash H b 40 dash 6 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> 3", Translations.GetTranslation("Col 107 Sector AI-H b40-6 A B 3"));
            Assert.AreEqual(@"Coll <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> Sector <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈindiˑɑ"">india</phoneme> dash <phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme>", Translations.GetTranslation("Col 107 Sector AI-H b40-6 AB 3", true));
        }

        [TestMethod]
        public void TestTranslateBody4()
        {
            Assert.AreEqual(@"H R 7 7 5 6 A B C D E 6 <say-as interpret-as=""characters"">b</say-as>", Translations.GetTranslation("HR 7756 ABCDE 6 b"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>", Translations.GetTranslation("HR 7756 ABCDE 6 b", true));
        }

        [TestMethod]
        public void TestTranslateBody5()
        {
            Assert.AreEqual(@"Hip 6 3 8 3 5 A B C D 1 <say-as interpret-as=""characters"">c</say-as> <say-as interpret-as=""characters"">a</say-as>", Translations.GetTranslation("HIP 63835 ABCD 1 c a"));
            Assert.AreEqual(@"Hip <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme>", Translations.GetTranslation("HIP 63835 ABCD 1 c a", true));
        }

        [TestMethod]
        public void TestTranslateBody6()
        {
            Assert.AreEqual(@"Boewnst T Q dash K d 9 dash 7 4 0 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">A</say-as> Belt", Translations.GetTranslation("Boewnst TQ-K d9-740 A A Belt"));
            Assert.AreEqual(@"Boewnst <phoneme alphabet=""ipa"" ph=""ˈtænɡo"">tango</phoneme> <phoneme alphabet=""ipa"" ph=""keˈbek"">quebec</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈkiːlo"">kilo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt", Translations.GetTranslation("Boewnst TQ-K d9-740 A A Belt", true));
        }

        [TestMethod]
        public void TestTranslateBody7()
        {
            Assert.AreEqual(@"Calea <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">A</say-as> Belt Cluster 1", Translations.GetTranslation("Calea B A Belt Cluster 1"));
            Assert.AreEqual(@"Calea <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt Cluster <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.GetTranslation("Calea B A Belt Cluster 1", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem1()
        {
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme>", Translations.GetTranslation("Shinrarta Dezhra"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme>", Translations.GetTranslation("Shinrarta Dezhra", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem2()
        {
            Assert.AreEqual(@"Hip 1 2 3 4 5", Translations.GetTranslation("HIP 12345"));
            Assert.AreEqual(@"Hip <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme>", Translations.GetTranslation("HIP 12345", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem3()
        {
            Assert.AreEqual(@"Skull and Crossbones Sector A W dash M b 7 dash 0 4", Translations.GetTranslation("Skull and Crossbones Neb. Sector AW-M b7-0 4"));
            Assert.AreEqual(@"Skull and Crossbones Sector <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwiski"">whiskey</phoneme> dash <phoneme alphabet=""ipa"" ph=""maɪk"">mike</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme>", Translations.GetTranslation("Skull and Crossbones Neb. Sector AW-M b7-0 4", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem4()
        {
            Assert.AreEqual(@"2 mass J 2 1 5 4 1 8 7 7 plus 4 7 1 2 0 9 6", Translations.GetTranslation("2MASS J21541877+4712096"));
            Assert.AreEqual(@"2 mass <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme><phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> plus <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme>", Translations.GetTranslation("2MASS J21541877+4712096", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem5()
        {
            Assert.AreEqual(@"V 1 3 9 7 Orionis", Translations.GetTranslation("V1397 Orionis"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈvɪktə"">victor</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme><phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> Orionis", Translations.GetTranslation("V1397 Orionis", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem6()
        {
            Assert.AreEqual(@"X T E P S R J 1 8 1 0 minus 1 9 7", Translations.GetTranslation("XTE PSR J1810-197"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈeksˈrei"">x-ray</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtænɡo"">tango</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""pəˈpɑ"">papa</phoneme> <phoneme alphabet=""ipa"" ph=""siˈerə"">sierra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme><phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> minus <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme>", Translations.GetTranslation("XTE PSR J1810-197", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem7()
        {
            Assert.AreEqual(@"Gliese 3 9 8 point 2", Translations.GetTranslation("Gliese 398.2"));
            Assert.AreEqual(@"Gliese <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> point 2", Translations.GetTranslation("Gliese 398.2", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem8()
        {
            Assert.AreEqual(@"Coll 2 8 5 Sector Q H dash U c 3 dash 22", Translations.GetTranslation("Col 285 Sector QH-U c3-22"));
            Assert.AreEqual(@"Coll <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> Sector <phoneme alphabet=""ipa"" ph=""keˈbek"">quebec</phoneme> <phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈjuːnifɔːm"">uniform</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>", Translations.GetTranslation("Col 285 Sector QH-U c3-22", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem9()
        {
            Assert.AreEqual(@"Hip 72", Translations.GetTranslation("HIP 72"));
            Assert.AreEqual(@"Hip <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>", Translations.GetTranslation("HIP 72", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem10()
        {
            Assert.AreEqual(@"C F B D S I R 1 4 5 8 plus 10 <say-as interpret-as=""characters"">B</say-as>", Translations.GetTranslation("CFBDSIR 1458+10 B"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfɒkstrɒt"">foxtrot</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""siˈerə"">sierra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈindiˑɑ"">india</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> plus 10 <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>", Translations.GetTranslation("CFBDSIR 1458+10 B", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem11()
        {
            Assert.AreEqual(@"B D plus 18 7 1 1", Translations.GetTranslation("BD+18 711"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> plus 18 <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.GetTranslation("BD+18 711", true));
        }

        [TestMethod]
        public void TestTranslateStarSystems()
        {
            Assert.AreEqual("L H S 1 2 3 4 5", Translations.GetTranslation("LHS 12345"));
            Assert.AreEqual("H R 1 2 3 4 5", Translations.GetTranslation("HR 12345"));
            Assert.AreEqual("C X O U J 0 6 1 7 0 5 point 3 plus 2 2 2 1 2 7", Translations.GetTranslation("CXOU J061705.3+222127"));
            Assert.AreEqual("S D S S J 1 4 1 6 plus 1 3 4 8", Translations.GetTranslation("SDSS J1416+1348"));
            Assert.AreEqual("U G C S J 1 2 2 0 3 1 point 5 6 plus 2 4 3 6 1 4 point 8", Translations.GetTranslation("UGCS J122031.56+243614.8"));
            Assert.AreEqual("X T E J 1 7 4 8 minus 2 8 8", Translations.GetTranslation("XTE J1748-288"));
            Assert.AreEqual("", Translations.GetTranslation(""));
        }

        [TestMethod]
        public void TestTranslateStarSystemsStability()
        {
            string[] starSystems = Resources.starsystems.Split(new[] { '\r', '\n' });

            foreach (string starSystem in starSystems)
            {
                Translations.GetTranslation(starSystem);
            }
        }

        [TestMethod]
        public void TestTranslateStation()
        {
            Assert.AreEqual("Or-bis Starport", Translations.GetTranslation("Orbis Starport"));
            Assert.AreEqual("Mega-ship", Translations.GetTranslation("Megaship"));
        }

        [TestMethod]
        public void TestTranslateRing()
        {
            Assert.AreEqual(@"Oponner 6 <say-as interpret-as=""characters"">A</say-as> Ring", Translations.GetTranslation("Oponner 6 A Ring"));
            Assert.AreEqual(@"Oponner <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Ring", Translations.GetTranslation("Oponner 6 A Ring", true));
        }

        [TestMethod]
        public void TestTranslateBelt()
        {
            Assert.AreEqual(@"Wolf 2 0 2 <say-as interpret-as=""characters"">A</say-as> Belt Cluster 1", Translations.GetTranslation("Wolf 202 A Belt Cluster 1"));
            Assert.AreEqual(@"Wolf <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt Cluster <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.GetTranslation("Wolf 202 A Belt Cluster 1", true));
        }

        [TestMethod]
        public void TestSpokenShipModel()
        {
            var fromEDName = Translations.GetTranslation("CobraMkIII");
            var fromName = Translations.GetTranslation("Cobra Mk. III");
            string expected = "<phoneme alphabet=\"ipa\" ph=\"ˈkəʊbrə\">cobra</phoneme> " + "<phoneme alphabet=\"ipa\" ph=\"mɑːk\">Mark</phoneme> " + "<phoneme alphabet=\"ipa\" ph=\"θriː\">3</phoneme>";
            Assert.AreEqual(expected, fromEDName);
            Assert.AreEqual(expected, fromName);
        }

        [TestMethod]
        public void TestSpokenShipManufacturer()
        {
            var fromName = Translations.GetTranslation("Lakon Spaceways");
            string expected = "<phoneme alphabet=\"ipa\" ph=\"leɪkɒn\">Lakon</phoneme> " + "<phoneme alphabet=\"ipa\" ph=\"speɪsweɪz\">Spaceways</phoneme>";
            Assert.AreEqual(expected, fromName);
        }
    }
}