using EddiDataDefinitions;
using EddiSpeechResponder.CustomFunctions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class CustomFunctions : TestBase
    {
        [TestInitialize]
        public void start()
        {
            MakeSafe();
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
    }
}
