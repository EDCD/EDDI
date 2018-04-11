using Microsoft.VisualStudio.TestTools.UnitTesting;
using EddiSpeechService;

namespace UnitTests
{
    [TestClass]
    public class TranslationTests
    {
        [TestMethod]
        public void TestTranslateBody1()
        {
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> A 1", Translations.Body("Shinrarta Dezhra A 1"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.Body("Shinrarta Dezhra A 1", true));
        }

        [TestMethod]
        public void TestTranslateBody2()
        {
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> A B 1", Translations.Body("Shinrarta Dezhra AB 1"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.Body("Shinrarta Dezhra AB 1", true));
        }

        [TestMethod]
        public void TestTranslateBody3()
        {
            Assert.AreEqual(@"Coll 1 0 7 Sector A I dash H b 40 dash 6 A B 3", Translations.Body("Col 107 Sector AI-H b40-6 A B 3"));
            Assert.AreEqual(@"Coll <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> Sector <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈindiˑɑ"">india</phoneme> dash <phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme>", Translations.Body("Col 107 Sector AI-H b40-6 AB 3", true));
        }

        [TestMethod]
        public void TestTranslateBody4()
        {
            Assert.AreEqual(@"H R 7 7 5 6 A B C D E 6 b", Translations.Body("HR 7756 ABCDE 6 b"));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>", Translations.Body("HR 7756 ABCDE 6 b", true));
        }

        [TestMethod]
        public void TestTranslateBody5()
        {
            Assert.AreEqual(@"Hip 6 3 8 3 5 A B C D 1 c a", Translations.Body("HIP 63835 ABCD 1 c a"));
            Assert.AreEqual(@"Hip <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme>", Translations.Body("HIP 63835 ABCD 1 c a", true));
        }

        [TestMethod]
        public void TestTranslateBody6()
        {
            Assert.AreEqual(@"Boewnst T Q dash K d 9 dash 7 4 0 A A Belt", Translations.Body("Boewnst TQ-K d9-740 A A Belt"));
            Assert.AreEqual(@"Boewnst <phoneme alphabet=""ipa"" ph=""ˈtænɡo"">tango</phoneme> <phoneme alphabet=""ipa"" ph=""keˈbek"">quebec</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈkiːlo"">kilo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt", Translations.Body("Boewnst TQ-K d9-740 A A Belt", true));
        }

        [TestMethod]
        public void TestTranslateBody7()
        {
            Assert.AreEqual(@"Calea B A Belt Cluster 1", Translations.Body("Calea B A Belt Cluster 1"));
            Assert.AreEqual(@"Calea <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt Cluster <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.Body("Calea B A Belt Cluster 1", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem1()
        {
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme>", Translations.StarSystem("Shinrarta Dezhra", false));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme>", Translations.StarSystem("Shinrarta Dezhra", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem2()
        {
            Assert.AreEqual(@"Hip 1 2 3 4 5", Translations.StarSystem("HIP 12345", false));
            Assert.AreEqual(@"Hip <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme>", Translations.StarSystem("HIP 12345", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem3()
        {
            Assert.AreEqual(@"Skull and Crossbones Sector A W dash M b 7 dash 0 4", Translations.StarSystem("Skull and Crossbones Neb. Sector AW-M b7-0 4", false));
            Assert.AreEqual(@"Skull and Crossbones Sector <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwiski"">whiskey</phoneme> dash <phoneme alphabet=""ipa"" ph=""maɪk"">mike</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme>", Translations.StarSystem("Skull and Crossbones Neb. Sector AW-M b7-0 4", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem4()
        {
            Assert.AreEqual(@"2 mass J 2 1 5 4 1 8 7 7 plus 4 7 1 2 0 9 6", Translations.StarSystem("2MASS J21541877+4712096", false));
            Assert.AreEqual(@"2 mass <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme><phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> plus <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme>", Translations.StarSystem("2MASS J21541877+4712096", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem5()
        {
            Assert.AreEqual(@"V 1 3 9 7 Orionis", Translations.StarSystem("V1397 Orionis", false));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈvɪktə"">victor</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme><phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> Orionis", Translations.StarSystem("V1397 Orionis", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem6()
        {
            Assert.AreEqual(@"X T E P S R J 1 8 1 0 minus 1 9 7", Translations.StarSystem("XTE PSR J1810-197", false));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈeksˈrei"">x-ray</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtænɡo"">tango</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""pəˈpɑ"">papa</phoneme> <phoneme alphabet=""ipa"" ph=""siˈerə"">sierra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme><phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> minus <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme>", Translations.StarSystem("XTE PSR J1810-197", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem7()
        {
            Assert.AreEqual(@"Gliese 3 9 8 point 2", Translations.StarSystem("Gliese 398.2", false));
            Assert.AreEqual(@"Gliese <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> point 2", Translations.StarSystem("Gliese 398.2", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem8()
        {
            Assert.AreEqual(@"Coll 2 8 5 Sector Q H dash U c 3 dash 22", Translations.StarSystem("Col 285 Sector QH-U c3-22", false));
            Assert.AreEqual(@"Coll <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> Sector <phoneme alphabet=""ipa"" ph=""keˈbek"">quebec</phoneme> <phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈjuːnifɔːm"">uniform</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>", Translations.StarSystem("Col 285 Sector QH-U c3-22", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem9()
        {
            Assert.AreEqual(@"Hip 72", Translations.StarSystem("HIP 72", false));
            Assert.AreEqual(@"Hip 72", Translations.StarSystem("HIP 72", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem10()
        {
            Assert.AreEqual(@"C F B D S I R 1 4 5 8 plus 10 B", Translations.StarSystem("CFBDSIR 1458+10 B", false));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈtʃɑːli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfɒkstrɒt"">foxtrot</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""siˈerə"">sierra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈindiˑɑ"">india</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> plus 10 <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>", Translations.StarSystem("CFBDSIR 1458+10 B", true));
        }

        [TestMethod]
        public void TestTranslateStarSystem11()
        {
            Assert.AreEqual(@"B D plus 18 7 1 1", Translations.StarSystem("BD+18 711", false));
            Assert.AreEqual(@"<phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> plus 18 <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>", Translations.StarSystem("BD+18 711", true));
        }
    }
}