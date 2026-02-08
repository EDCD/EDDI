using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass, TestCategory( "UnitTests" )]
    public class EdsmHttpClientTests : TestBase
    {
        [TestMethod]
        public async Task GetAsync_WithValidUrl_ReturnsResponseContent ()
        {
            // Arrange
            var url = "https://www.edsm.net/api/v1/systems";
            var expectedContent = "{\"result\":\"success\"}";
            FakeEdsmHttpClient.Expect( url, expectedContent );

            // Act
            var result = await FakeEdsmHttpClient.GetAsync(url, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull( result );
            Assert.AreEqual( expectedContent, result );
        }

        [TestMethod]
        public async Task PostAsync_WithValidUrlAndContent_ReturnsResponseContent ()
        {
            // Arrange
            var url = "https://www.edsm.net/api/v1/profile";
            var expectedContent = "{\"cmdName\":\"TestCmdr\"}";
            FakeEdsmHttpClient.Expect( url, expectedContent );

            // Act
            var result = await FakeEdsmHttpClient.PostAsync(url, null, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull( result );
            Assert.AreEqual( expectedContent, result );
        }

        [TestMethod]
        public async Task GetAsync_WithUnregisteredUrl_ThrowsKeyNotFoundException ()
        {
            // Arrange
            var url = "https://www.edsm.net/api/v1/unknown";

            // Act & Assert
            await Assert.ThrowsExactlyAsync<KeyNotFoundException>( () =>
                FakeEdsmHttpClient.GetAsync( url, CancellationToken.None ) ).ConfigureAwait( false );
        }

        [TestMethod]
        public async Task GetAsync_WithEmptyResponse_ReturnsEmptyString ()
        {
            // Arrange
            var url = "https://www.edsm.net/api/v1/systems";
            FakeEdsmHttpClient.Expect( url, "" );

            // Act
            var result = await FakeEdsmHttpClient.GetAsync(url, CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.IsNotNull( result );
            Assert.AreEqual( "", result );
        }
    }
}