using EddiCompanionAppService;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tests
{
    [TestClass, TestCategory( "Credentials" )]
    public class OAuthClientIDTests
    {
        [TestMethod]
        public void TestClientIDNotNull()
        {
            var secrets = new ConfigurationBuilder().AddUserSecrets<CompanionAppService>().Build();
            var clientID = secrets[ "CompanionAppService:ClientId" ];
            Assert.IsInstanceOfType( clientID, typeof(string));
            Assert.IsNotNull( clientID );
        }
    }
}
