using Microsoft.VisualStudio.TestTools.UnitTesting;
using EDDIVAPlugin;

namespace Tests
{
    [TestClass]
    public class CoriolisTests
    {
        [TestMethod]
        public void TestCompressData()
        {
            string data = "1111111111111111";
            string base64Data = LZString.compressToBase64(data);
            Assert.AreEqual("Iw1/EA==", base64Data);
        }
        [TestMethod]
        public void TestDecompressData()
        {
            string base64Data = "Iw1/EA==";
            var data = LZString.decompressFromBase64(base64Data);
            Assert.AreEqual("1111111111111111", data);
        }

        [TestMethod]
        public void TestDecompressData2()
        {
            string base64Data = "IwgMIyKA";
            var data = LZString.decompressFromBase64(base64Data);
            Assert.AreEqual("1110111111111111", data);
        }

        [TestMethod]
        public void TestDecompressData3()
        {
            string base64Data = "AwRj4yg=";
            var data = LZString.decompressFromBase64(base64Data);
            Assert.AreEqual("0111111111111111", data);
        }
        
    }
}
