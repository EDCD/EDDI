using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OAuthClientIDTests
{
    [TestClass]
    public class OAuthClientIDTests
    {
        [TestMethod, TestCategory("Credentials")]
        public void TestClientIDNotNull()
        {
            string ID = EddiCompanionAppService.ClientId.ID;
            Assert.IsNotNull(ID);
        }
    }
}
