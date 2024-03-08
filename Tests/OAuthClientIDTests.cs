using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OAuthClientIDTests
{
    [TestClass]
    public class OAuthClientIDTests
    {
        [TestMethod, TestCategory("Credentials")]
        public void TestClientIDNotNull()
        {
            var clientIDClass = new PrivateType(typeof(EddiCompanionAppService.ClientId));
            object clientID = clientIDClass.GetStaticField("ID");
            Assert.IsInstanceOfType(clientID, typeof(string));
            Assert.IsNotNull( clientID );
        }
    }
}
