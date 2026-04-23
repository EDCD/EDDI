using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass, TestCategory( "Credentials" )]
    public class OAuthClientIDTests
    {
        [TestMethod]
        public void TestClientIDNotNull()
        {
            var clientID = EddiCompanionAppService.ClientId.ID;
            Assert.IsInstanceOfType( clientID, typeof(string));
            Assert.IsNotNull( clientID );
        }
    }
}
