#nullable enable

using EddiEvents;
using EddiVoiceAttackResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    [TestClass, TestCategory( "UnitTests" )]
    public sealed class VoiceAttackResponderModeTests : TestBase
    {
        private readonly List<string> _calls = [];

        [TestInitialize]
        public void Start ()
        {
            MakeSafe();

            VoiceAttackResponderMode.Shutdown();
            VoiceAttackResponderMode.ResetTestHooks();

            _calls.Clear();
        }

        [TestCleanup]
        public void Stop ()
        {
            VoiceAttackResponderMode.Shutdown();
            VoiceAttackResponderMode.ResetTestHooks();
        }

        [TestMethod, DoNotParallelize]
        public async Task InitializeAsync_DoesNotReplayStandardVoiceAttackVariables ()
        {
            VoiceAttackResponderMode.InitializeStandardValues = () => _calls.Add( "initializeStandardValues" );
            VoiceAttackResponderMode.SetStatus = ( status, _ ) => _calls.Add( $"setStatus:{status}" );
            VoiceAttackResponderMode.EnqueueVAInitializedEvent = _ => _calls.Add( "VAInitializedEvent" );

            await VoiceAttackResponderMode.InitializeAsync().ConfigureAwait( false );

            Assert.HasCount( 0, _calls,
                "InitializeAsync must not publish standard VoiceAttack variables before the VA IPC handshake." );
        }

        [TestMethod, DoNotParallelize]
        public async Task ReplayStandardValuesAsync_ReplaysVariablesSetsStatusAndQueuesInitializedEvent ()
        {
            VoiceAttackResponderMode.InitializeStandardValues = () => _calls.Add( "initializeStandardValues" );
            VoiceAttackResponderMode.SetStatus = ( status, _ ) => _calls.Add( $"setStatus:{status}" );
            VoiceAttackResponderMode.EnqueueVAInitializedEvent = e =>
            {
                Assert.IsInstanceOfType( e, typeof( VAInitializedEvent ) );
                _calls.Add( "VAInitializedEvent" );
            };

            await VoiceAttackResponderMode.ReplayStandardValuesAsync(
                "unit test",
                CancellationToken.None ).ConfigureAwait( false );

            CollectionAssert.AreEqual(
                (string[])
                [
                    "initializeStandardValues",
                    "setStatus:Operational",
                    "VAInitializedEvent"
                ],
                _calls );
        }

        [TestMethod, DoNotParallelize]
        public async Task ReplayStandardValuesAsync_ReleasesSemaphoreAfterFailure ()
        {
            VoiceAttackResponderMode.InitializeStandardValues = () =>
                throw new InvalidOperationException( "Test failure" );

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(
                () => VoiceAttackResponderMode.ReplayStandardValuesAsync(
                    "expected failure",
                    CancellationToken.None ) ).ConfigureAwait( false );

            VoiceAttackResponderMode.InitializeStandardValues = () => _calls.Add( "initializeStandardValues" );
            VoiceAttackResponderMode.SetStatus = ( status, _ ) => _calls.Add( $"setStatus:{status}" );
            VoiceAttackResponderMode.EnqueueVAInitializedEvent = _ => _calls.Add( "VAInitializedEvent" );

            await VoiceAttackResponderMode.ReplayStandardValuesAsync(
                "after failure",
                CancellationToken.None ).ConfigureAwait( false );

            CollectionAssert.AreEqual(
                (string[])
                [
                    "initializeStandardValues",
                    "setStatus:Operational",
                    "VAInitializedEvent"
                ],
                _calls );
        }

        [TestMethod, DoNotParallelize]
        public async Task ReplayStandardValuesAsync_HonorsPreCanceledToken ()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsExactlyAsync<TaskCanceledException>(
                () => VoiceAttackResponderMode.ReplayStandardValuesAsync(
                    "canceled",
                    // ReSharper disable once AccessToDisposedClosure (This is intended to test that the method properly observes the cancellation token.)
                    cts.Token ) ).ConfigureAwait( false );

            Assert.HasCount( 0, _calls );
        }
    }
}