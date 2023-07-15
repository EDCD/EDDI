﻿using EddiSpeechService;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    // this class is pure and doesn't need TestBase.MakeSafe()
    public class TranslationTests
    {
        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("a", @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme>")]
        [DataRow("ab", @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>")]
        [DataRow("abc", @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme>")]
        [DataRow("$@!^", "")]
        public void TestICAO(string source, string expected)
        {
            Assert.AreEqual(expected, Translations.ICAO(source));
        }

        [TestMethod]
        public void TestTranslateCallsigns()
        {
            Assert.AreEqual("<phoneme alphabet=\"ipa\" ph=\"ɡɒlf\">golf</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈælfə\">alpha</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈeksˈrei\">x-ray</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈwʌn\">one</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈzɪərəʊ\">zero</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈnaɪnər\">niner</phoneme> <phoneme alphabet=\"ipa\" ph=\"ˈfoʊ.ər\">fawer</phoneme>", Translations.ICAO("GAX-1094"));
        }

        [DataTestMethod]
        [DataRow("Shinrarta Dezhra A 1", 
            @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <say-as interpret-as=""characters"">A</say-as> 1", 
            @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>"
        )]
        [DataRow("Shinrarta Dezhra AB 1",
            @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> 1",
            @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>"
        )]
        [DataRow("Col 107 Sector AI-H b40-6 AB 3",
            @"Coll 1 0 7 Sector <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">I</say-as> dash <say-as interpret-as=""characters"">H</say-as> <say-as interpret-as=""characters"">b</say-as> 40 dash 6 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> 3",
            @"Coll <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> Sector <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈindiˑɑ"">india</phoneme> dash <phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme>"
        )]
        [DataRow("HR 7756 ABCDE 6 b",
            @"<say-as interpret-as=""characters"">H</say-as> <say-as interpret-as=""characters"">R</say-as> 7 7 5 6 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">D</say-as> <say-as interpret-as=""characters"">E</say-as> 6 <say-as interpret-as=""characters"">b</say-as>",
            @"<phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>"
        )]
        [DataRow("HIP 63835 ABCD 1 c a",
            @"Hip 6 3 8 3 5 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">D</say-as> 1 <say-as interpret-as=""characters"">c</say-as> <say-as interpret-as=""characters"">a</say-as>",
            @"Hip <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme>"
        )]
        [DataRow("Boewnst TQ-K d9-740 A A Belt",
            @"Boewnst <say-as interpret-as=""characters"">T</say-as> <say-as interpret-as=""characters"">Q</say-as> dash <say-as interpret-as=""characters"">K</say-as> <say-as interpret-as=""characters"">d</say-as> 9 dash 740 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">A</say-as> Belt",
            @"Boewnst <phoneme alphabet=""ipa"" ph=""ˈtænɡo"">tango</phoneme> <phoneme alphabet=""ipa"" ph=""keˈbek"">quebec</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈkiːlo"">kilo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt"
        )]
        [DataRow("Calea B A Belt Cluster 1",
            @"Calea <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">A</say-as> Belt Cluster 1",
            @"Calea <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> Belt Cluster <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>"
        )]
        public void TestTranslateBody(string source, string expected, string expectedICAO)
        {
            Assert.AreEqual(expected, Translations.GetTranslation(source));
            Assert.AreEqual(expectedICAO, Translations.GetTranslation(source, true));
        }

        [DataTestMethod]
        [DataRow("A 1", 
            @"<say-as interpret-as=""characters"">A</say-as> 1", 
            @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>"
        )]
        [DataRow("B 2 a", 
            @"<say-as interpret-as=""characters"">B</say-as> 2 <say-as interpret-as=""characters"">a</say-as>", 
            @"<phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme>"
        )]
        [DataRow("C 3", 
            @"<say-as interpret-as=""characters"">C</say-as> 3", 
            @"<phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme>"
        )]
        [DataRow("D 4 b", 
            @"<say-as interpret-as=""characters"">D</say-as> 4 <say-as interpret-as=""characters"">b</say-as>", 
            @"<phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>"
        )]
        [DataRow("E 5", 
            @"<say-as interpret-as=""characters"">E</say-as> 5", 
            @"<phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme>"
        )]
        [DataRow("6 f",
            @"6 <say-as interpret-as=""characters"">f</say-as>",
            @"<phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfɒkstrɒt"">foxtrot</phoneme>"
        )]
        [DataRow("AB 9", 
            @"<say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> 9",
            @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme>"
        )]
        [DataRow("ABC 10", 
            @"<say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">C</say-as> 10", 
            @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme>"
        )]
        [DataRow("ABCD 11", 
            @"<say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">D</say-as> 11", 
            @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>"
        )]
        [DataRow("ABCDE 12", 
            @"<say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">D</say-as> <say-as interpret-as=""characters"">E</say-as> 12", 
            @"<phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>"
        )]
        public void TestTranslateShortBody(string source, string expected, string expectedICAO)
        {
            Assert.AreEqual(expected, Translations.GetTranslation(source));
            Assert.AreEqual(expectedICAO, Translations.GetTranslation(source, true));
        }

        [DataTestMethod]
        [DataRow("Shinrarta Dezhra",
            @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme>",
            @"<phoneme alphabet=""ipa"" ph=""ʃɪnˈrɑːrtə"">Shinrarta</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdezɦrə"">Dezhra</phoneme>"
        )]
        [DataRow("HIP 12345",
            @"Hip 1 2 3 4 5",
            @"Hip <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme>"
        )]
        [DataRow("Skull and Crossbones Neb. Sector AW-M b7-0 4",
            @"Skull and Crossbones Sector <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">W</say-as> dash <say-as interpret-as=""characters"">M</say-as> <say-as interpret-as=""characters"">b</say-as> 7 dash 0 4",
            @"Skull and Crossbones Sector <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwiski"">whiskey</phoneme> dash <phoneme alphabet=""ipa"" ph=""maɪk"">mike</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme>"
        )]
        [DataRow("2MASS J21541877+4712096",
            @"2 mass <say-as interpret-as=""characters"">J</say-as> 2 1 5 4 1 8 7 7 plus 4 7 1 2 0 9 6",
            @"<phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> mass <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> plus <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɪks"">six</phoneme>"
        )]
        [DataRow("V1397 Orionis",
            @"<say-as interpret-as=""characters"">V</say-as> 1 3 9 7 Orionis",
            @"<phoneme alphabet=""ipa"" ph=""ˈvɪktə"">victor</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> Orionis"
        )]
        [DataRow("XTE PSR J1810-197",
            @"<say-as interpret-as=""characters"">X</say-as> <say-as interpret-as=""characters"">T</say-as> <say-as interpret-as=""characters"">E</say-as> <say-as interpret-as=""characters"">P</say-as> <say-as interpret-as=""characters"">S</say-as> <say-as interpret-as=""characters"">R</say-as> <say-as interpret-as=""characters"">J</say-as> 1 8 1 0 dash 1 9 7",
            @"<phoneme alphabet=""ipa"" ph=""ˈeksˈrei"">x-ray</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtænɡo"">tango</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeko"">echo</phoneme> <phoneme alphabet=""ipa"" ph=""pəˈpɑ"">papa</phoneme> <phoneme alphabet=""ipa"" ph=""siˈerə"">sierra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme>"
        )]
        [DataRow("Gliese 398.2",
            @"<phoneme alphabet=""ipa"" ph=""ˈɡliːzə"">Gliese</phoneme> 3 9 8 point 2",
            @"<phoneme alphabet=""ipa"" ph=""ˈɡliːzə"">Gliese</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> <phoneme alphabet=""ipa"" ph=""ˈnaɪnər"">niner</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> point <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>"
        )]
        [DataRow("Col 285 Sector QH-U c3-22",
            @"Coll 2 8 5 Sector <say-as interpret-as=""characters"">Q</say-as> <say-as interpret-as=""characters"">H</say-as> dash <say-as interpret-as=""characters"">U</say-as> <say-as interpret-as=""characters"">c</say-as> 3 dash 22",
            @"Coll <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> Sector <phoneme alphabet=""ipa"" ph=""keˈbek"">quebec</phoneme> <phoneme alphabet=""ipa"" ph=""hoːˈtel"">hotel</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈjuːnifɔːm"">uniform</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtriː"">tree</phoneme> dash <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>"
        )]
        [DataRow("HIP 72",
            @"Hip 72",
            @"Hip <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>"
        )]
        [DataRow("CFBDSIR 1458+10 B",
            @"<say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">F</say-as> <say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">D</say-as> <say-as interpret-as=""characters"">S</say-as> <say-as interpret-as=""characters"">I</say-as> <say-as interpret-as=""characters"">R</say-as> 1 4 5 8 plus 10 <say-as interpret-as=""characters"">B</say-as>",
            @"<phoneme alphabet=""ipa"" ph=""ˈtʃɑɹli"">charlie</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfɒkstrɒt"">foxtrot</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> <phoneme alphabet=""ipa"" ph=""siˈerə"">sierra</phoneme> <phoneme alphabet=""ipa"" ph=""ˈindiˑɑ"">india</phoneme> <phoneme alphabet=""ipa"" ph=""ˈroːmiˑo"">romeo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfoʊ.ər"">fawer</phoneme> <phoneme alphabet=""ipa"" ph=""ˈfaɪf"">fife</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> plus <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>"
        )]
        [DataRow("BD+18 711",
            @"<say-as interpret-as=""characters"">B</say-as> <say-as interpret-as=""characters"">D</say-as> plus 18 7 1 1",
            @"<phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdɛltə"">delta</phoneme> plus <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈeɪt"">eight</phoneme> <phoneme alphabet=""ipa"" ph=""ˈsɛvɛn"">seven</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme>"
        )]
        public void TestTranslateStarSystemWithICAO(string source, string expected, string expectedICAO)
        {
            Assert.AreEqual(expected, Translations.GetTranslation(source));
            Assert.AreEqual(expectedICAO, Translations.GetTranslation(source, true));
        }

        [DataTestMethod]
        [DataRow("", "")]
        [DataRow("LHS 12345", "<say-as interpret-as=\"characters\">L</say-as> <say-as interpret-as=\"characters\">H</say-as> <say-as interpret-as=\"characters\">S</say-as> 1 2 3 4 5")]
        [DataRow("HR 12345", @"<say-as interpret-as=""characters"">H</say-as> <say-as interpret-as=""characters"">R</say-as> 1 2 3 4 5")]
        [DataRow("CXOU J061705.3+222127", @"<say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">X</say-as> <say-as interpret-as=""characters"">O</say-as> <say-as interpret-as=""characters"">U</say-as> <say-as interpret-as=""characters"">J</say-as> 0 6 1 7 0 5 point 3 plus 2 2 2 1 2 7")]
        [DataRow("SDSS J1416+1348", @"<say-as interpret-as=""characters"">S</say-as> <say-as interpret-as=""characters"">D</say-as> <say-as interpret-as=""characters"">S</say-as> <say-as interpret-as=""characters"">S</say-as> <say-as interpret-as=""characters"">J</say-as> 1 4 1 6 plus 1 3 4 8")]
        [DataRow("UGCS J122031.56+243614.8", @"<say-as interpret-as=""characters"">U</say-as> <say-as interpret-as=""characters"">G</say-as> <say-as interpret-as=""characters"">C</say-as> <say-as interpret-as=""characters"">S</say-as> <say-as interpret-as=""characters"">J</say-as> 1 2 2 0 3 1 point 56 plus 2 4 3 6 1 4 point 8")]
        [DataRow("XTE J1748-288", @"<say-as interpret-as=""characters"">X</say-as> <say-as interpret-as=""characters"">T</say-as> <say-as interpret-as=""characters"">E</say-as> <say-as interpret-as=""characters"">J</say-as> 1 7 4 8 dash 2 8 8")]
        [DataRow("9 G. Carinae", @"9 <say-as interpret-as=""characters"">G</say-as> <phoneme alphabet=""ipa"" ph=""kəˈriːniː"">Carinae</phoneme>")]
        [DataRow("LP 355-64", @"<say-as interpret-as=""characters"">L</say-as> <say-as interpret-as=""characters"">P</say-as> 3 5 5 dash 64")]
        [DataRow("Llyr", @"Llyr")]
        [DataRow( "Achelous", "Achelous" )]
        [DataRow( "ADS 9338 AC", @"<say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">D</say-as> <say-as interpret-as=""characters"">S</say-as> 9 3 3 8 <say-as interpret-as=""characters"">A</say-as> <say-as interpret-as=""characters"">C</say-as>" )]
        public void TestTranslateStarSystems(string source, string expected)
        {
            Assert.AreEqual(expected, Translations.GetTranslation(source));
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

        [TestMethod]
        public void TestTranslateOrigamiAerospaceIndustries()
        {
            Assert.AreEqual("ORIGAMI AEROSPACE INDUSTRIES", Translations.GetTranslation("ORIGAMI AEROSPACE INDUSTRIES", false, "faction"));
        }
    }
}