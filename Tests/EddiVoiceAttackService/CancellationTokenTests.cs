#nullable enable

using EddiIPC_Service.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    [TestClass, TestCategory( "UnitTests" )]
    public class CancellationTokenTests
    {
        public TestContext TestContext { get; set; } = null!;

        [TestMethod]
        [Timeout( 5000, CooperativeCancellation = true )]
        public async Task StartAsync_WithCancelledToken_LeavesServerStopped()
        {
            using var server = new IPCServer();
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>( () => server.StartAsync( cts.Token ) );

            Assert.IsFalse( server.IsRunning );
        }

        [TestMethod]
        [Timeout( 5000, CooperativeCancellation = true )]
        public async Task StartAsync_CompletesNormally_WhenNotCancelled()
        {
            using var server = new IPCServer();
            using var cts = new CancellationTokenSource();

            await server.StartAsync( cts.Token );

            Assert.IsTrue( server.IsRunning );
            Assert.IsGreaterThan( 0, server.Port );
            await server.StopAsync( CancellationToken.None );
            Assert.IsFalse( server.IsRunning );
        }

        [TestMethod]
        [Timeout( 5000, CooperativeCancellation = true )]
        public async Task StartAsync_DefaultToken_Works()
        {
            using var server = new IPCServer();

            await server.StartAsync();

            Assert.IsTrue( server.IsRunning );
            await server.StopAsync( CancellationToken.None );
            Assert.IsFalse( server.IsRunning );
        }

        [TestMethod]
        [Timeout( 5000, CooperativeCancellation = true )]
        public async Task StopAsync_WithCancelledToken_LeavesServerRunningUntilUncancelledStop()
        {
            using var server = new IPCServer();
            await server.StartAsync( TestContext.CancellationToken );
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await server.StopAsync( cts.Token );

            Assert.IsTrue( server.IsRunning );
            await server.StopAsync( CancellationToken.None );
            Assert.IsFalse( server.IsRunning );
        }
    }
}
