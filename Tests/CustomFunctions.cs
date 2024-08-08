using Cottle;
using EddiDataDefinitions;
using EddiSpeechResponder;
using EddiSpeechResponder.CustomFunctions;
using EddiSpeechResponder.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class CustomFunctions : TestBase
    {
        private readonly ScriptResolver resolver = new ScriptResolver(new Dictionary<string, Script>());

        [TestInitialize]
        public void start()
        {
            MakeSafe();
        }

        private string ResolveScript(string script, Dictionary<string, Tuple<Type, Value>> vars = null)
        {
            return ScriptResolver.resolveFromValue(script, ScriptResolver.buildContext(vars), true);
        }

        [DataTestMethod]
        [DataRow("", "", "")] // Manufacturer: Empty, ID with greater than 3 characters
        [DataRow("BelugaLiner", "", "")] // Manufacturer: Saud Kruger, empty ID
        [DataRow("Adder", "J", @"Zorgon Peterson <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme>")] // Manufacturer: Zorgon Peterson, alphanumeric ID with less than 3 characters
        [DataRow("DiamondBackXL", "J-12", @"<phoneme alphabet=""ipa"" ph=""leɪkɒn"">Lakon</phoneme> <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈtuː"">two</phoneme>")] // Manufacturer: Zorgon Peterson, alphanumeric ID with 3 characters and a symbol
        [DataRow("CobraMkIII", "", "")] // Manufacturer: Faulcon DeLacy, empty ID
        [DataRow("CobraMkIII", "J", @"DeLacy <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme>")] // Manufacturer: Faulcon DeLacy, ID with less than 3 characters
        [DataRow("CobraMkIII", "Jameson", @"DeLacy <phoneme alphabet=""ipa"" ph=""ˈdʒuːliˑˈet"">juliet</phoneme> <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""maɪk"">mike</phoneme>")] // Manufacturer: Faulcon DeLacy, ID with greater than 3 characters
        [DataRow("CobraMkIII", "A-1-B", @"DeLacy <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈwʌn"">one</phoneme> <phoneme alphabet=""ipa"" ph=""ˈbrɑːˈvo"">bravo</phoneme>")] // Manufacturer: Faulcon DeLacy, alphanumeric ID with 3 characters and two symbols
        [DataRow("CobraMkIII", "--A--", @"DeLacy <phoneme alphabet=""ipa"" ph=""ˈælfə"">alpha</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme> <phoneme alphabet=""ipa"" ph=""ˈzɪərəʊ"">zero</phoneme>")] // Manufacturer: Faulcon DeLacy, alphanumeric ID with 1 character and symbols
        public void TestShipCallsignFunction(string shipModel, string id, string expected)
        {
            Ship ship = ShipDefinitions.FromEDModel(shipModel);
            PrivateType privateType = new PrivateType(typeof(ShipCallsign));
            Assert.AreEqual(expected, (string)privateType.InvokeStatic("phoneticCallsign", new object[] { ship, id }));
        }

        [DataTestMethod]
        [DataRow("{Occasionally(1, 'A')}{Occasionally(1, 'B')}C", "ABC")]
        [DataRow("{Occasionally(1, 'A')} {Occasionally(1, 'B')} C", "A B C")]
        [DataRow("{Occasionally(1, '  A    ')}{Occasionally(1, '  B    ')} C", "A B C")]
        [DataRow("{Occasionally(1, '  A    ')} {Occasionally(1, '  B    ')} C", "A B C")]
        [DataRow("   {Occasionally(1, '  A    ')} {Occasionally(1, '  B    ')} C  ", "A B C")]
        public void TestCustomFunctionTrimming(string rawCottle, string expected)
        {
            var actual = ResolveScript(rawCottle);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void TestShipDetails()
        {
            // The inputs to this function might include phonetic SSML tags (added to improve phonetic pronunciations).
            // Test that we can correctly identify all such ship models.
            foreach (var model in ShipDefinitions.ShipModels)
            {
                var ship = ShipDefinitions.FromModel(model);
                var spokenModel = ship.SpokenModel();
                if (model != spokenModel)
                {
                    var resolvedModel = ResolveScript("{ShipDetails('" + spokenModel + "').model}");
                    Assert.AreEqual(model, resolvedModel);
                }
            }
        }
    }
}
