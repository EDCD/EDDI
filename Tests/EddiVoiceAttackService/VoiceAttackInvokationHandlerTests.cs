#nullable enable

using EddiConfigService;
using EddiConfigService.Configurations;
using EddiCore;
using EddiIPC_Service.Messages;
using EddiIPC_Service.Server;
using EddiVoiceAttackResponder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.EddiVoiceAttackService
{
    /// <summary>
    /// Unit tests for <see cref="VoiceAttackInvokationHandler"/> covering script processing,
    /// volume clamping, EDDI state management, and command routing.
    /// </summary>
    [TestClass, TestCategory( "UnitTests" )]
    public class VoiceAttackInvokationHandlerTests : TestBase
    {
        private readonly List<EventData> _dispatchedEvents = [];
        private ManualResetEventSlim _dispatchSignal = null!;
        private bool _fromVASaved;
        private IDisposable? _runtimeEventDispatcherRegistration;

        [TestInitialize]
        public void Initialize ()
        {
            MakeSafe();

            // Preserve and override the VA mode flag to avoid WPF Application.Current dispatcher calls
            _fromVASaved = EDDI.FromVA;
            EDDI.FromVA = true;

            _dispatchedEvents.Clear();
            _dispatchSignal = new ManualResetEventSlim( false );

            // Register a test dispatcher so we can capture what RuntimeWriteToLog / RuntimeSetXxx dispatch
            _runtimeEventDispatcherRegistration = RuntimeEventDispatcher.RegisterDispatcher( ( eventData, _ ) =>
            {
                _dispatchedEvents.Add( eventData );
                _dispatchSignal.Set();
                return Task.FromResult( true );
            } );
        }

        [TestCleanup]
        public void Cleanup ()
        {
            EDDI.FromVA = _fromVASaved;
            _runtimeEventDispatcherRegistration?.Dispose();
            _runtimeEventDispatcherRegistration = null;
            _dispatchSignal.Dispose();
        }

        #region SpeechFromScript

        [TestMethod]
        public void SpeechFromScript_WhenNull_ReturnsNull ()
        {
            Assert.IsNull( VoiceAttackInvokationHandler.SpeechFromScript( null ) );
        }

        [TestMethod]
        public void SpeechFromScript_WhenBracketOptions_SelectsOneOption ()
        {
            var result = VoiceAttackInvokationHandler.SpeechFromScript( "[hello;world]" );

            Assert.IsTrue( result == "hello" || result == "world",
                $"Expected 'hello' or 'world', but got '{result}'" );
        }

        [TestMethod]
        public void SpeechFromScript_WhenSemicolonSeparatedPhrases_SelectsOnePhrases ()
        {
            var result = VoiceAttackInvokationHandler.SpeechFromScript( "hello;world" );

            Assert.IsTrue( result == "hello" || result == "world",
                $"Expected 'hello' or 'world', but got '{result}'" );
        }

        [TestMethod]
        public void SpeechFromScript_WhenCommanderVariablePresent_ReplacesPlaceholder ()
        {
            var result = VoiceAttackInvokationHandler.SpeechFromScript( "Hello $-" );

            Assert.IsNotNull( result );
            Assert.IsFalse( result.Contains( "$-" ),
                $"Expected $- to be replaced, but result was '{result}'" );
        }

        [TestMethod]
        public void SpeechFromScript_WhenShipVariableWithNoCurrentShip_PreservesPlaceholder ()
        {
            EDDI.Instance.CurrentShip = null;

            var result = VoiceAttackInvokationHandler.SpeechFromScript( "Hello $=" );

            Assert.IsNotNull( result );
            Assert.IsTrue( result.Contains( "$=" ),
                $"Expected $= to be preserved with no ship, but result was '{result}'" );
        }

        #endregion

        #region InvokeVolume

        [TestMethod]
        public void HandleInvokedCommand_Volume_WhenNotProvided_SetsDefaultVolume ()
        {
            var defaultVolume = new SpeechServiceConfiguration().Volume;
            var config = ConfigService.Instance.speechServiceConfiguration;
            config.Volume = defaultVolume - 1; // ensure it differs from default
            ConfigService.Instance.speechServiceConfiguration = config;

            VoiceAttackInvokationHandler.HandleInvokedCommand( "volume",
                new Dictionary<string, object>() );

            Assert.AreEqual( defaultVolume, ConfigService.Instance.speechServiceConfiguration.Volume );
        }

        [TestMethod]
        public void HandleInvokedCommand_Volume_WhenNegative_ClampedToZero ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "volume",
                new Dictionary<string, object> { [ "Volume" ] = -5 } );

            Assert.AreEqual( 0, ConfigService.Instance.speechServiceConfiguration.Volume );
        }

        [TestMethod]
        public void HandleInvokedCommand_Volume_WhenOverHundred_ClampedTo100 ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "volume",
                new Dictionary<string, object> { [ "Volume" ] = 150 } );

            Assert.AreEqual( 100, ConfigService.Instance.speechServiceConfiguration.Volume );
        }

        [TestMethod]
        public void HandleInvokedCommand_Volume_WhenInRange_SetsVolume ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "volume",
                new Dictionary<string, object> { [ "Volume" ] = 42 } );

            Assert.AreEqual( 42, ConfigService.Instance.speechServiceConfiguration.Volume );
        }

        #endregion

        #region InvokeSetState

        [TestMethod]
        public void HandleInvokedCommand_SetState_WhenVariableNameMissing_DoesNotModifyState ()
        {
            const string sentinelKey = "state_missing_name_sentinel";
            EDDI.Instance.State.Remove( sentinelKey );

            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>() ); // no "State variable" key

            Assert.IsFalse( EDDI.Instance.State.ContainsKey( sentinelKey ) );
        }

        [TestMethod]
        public void HandleInvokedCommand_SetState_WhenTextValueProvided_StoresString ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>
                {
                    [ "State variable" ] = "set_state_str_test",
                    [ "State variable text value" ] = "hello"
                } );

            Assert.AreEqual( "hello", EDDI.Instance.State[ "set_state_str_test" ] );
        }

        [TestMethod]
        public void HandleInvokedCommand_SetState_WhenIntValueProvided_StoresInt ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>
                {
                    [ "State variable" ] = "set_state_int_test",
                    [ "State variable int value" ] = 99
                } );

            Assert.AreEqual( 99, EDDI.Instance.State[ "set_state_int_test" ] );
        }

        [TestMethod]
        public void HandleInvokedCommand_SetState_WhenBoolValueProvided_StoresBool ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>
                {
                    [ "State variable" ] = "set_state_bool_test",
                    [ "State variable bool value" ] = "true"
                } );

            Assert.AreEqual( true, EDDI.Instance.State[ "set_state_bool_test" ] );
        }

        [TestMethod]
        public void HandleInvokedCommand_SetState_WhenDecimalValueProvided_StoresDecimal ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>
                {
                    [ "State variable" ] = "set_state_dec_test",
                    [ "State variable decimal value" ] = "3.14"
                } );

            Assert.AreEqual( 3.14m, EDDI.Instance.State[ "set_state_dec_test" ] );
        }

        [TestMethod]
        public void HandleInvokedCommand_SetState_WhenNoValueProvided_StoresNull ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>
                {
                    [ "State variable" ] = "set_state_null_test"
                } );

            Assert.IsTrue( EDDI.Instance.State.ContainsKey( "set_state_null_test" ) );
            Assert.IsNull( EDDI.Instance.State[ "set_state_null_test" ] );
        }

        [TestMethod]
        public void HandleInvokedCommand_SetState_VariableNameNormalisedToLowerSnakeCase ()
        {
            VoiceAttackInvokationHandler.HandleInvokedCommand( "setstate",
                new Dictionary<string, object>
                {
                    [ "State variable" ] = "My TEST Variable",
                    [ "State variable text value" ] = "normalised"
                } );

            Assert.AreEqual( "normalised", EDDI.Instance.State[ "my_test_variable" ] );
        }

        #endregion

        #region Command routing

        [TestMethod]
        public void HandleInvokedCommand_WhenContextIsUnrecognized_DoesNotThrow ()
        {
            // Arrange & Act & Assert - no exception expected
            VoiceAttackInvokationHandler.HandleInvokedCommand( "totally_unrecognised_context_xyz", null );
        }

        [TestMethod]
        [Timeout( 5000 )]
        public void HandleInvokedCommand_InitializeEddi_WhenFromVAIsTrue_DispatchesOperationalMessage ()
        {
            // Act - fire-and-forget; wait for the dispatcher signal
            VoiceAttackInvokationHandler.HandleInvokedCommand( "initialize eddi", null );

            Assert.IsTrue( _dispatchSignal.Wait( 3000 ),
                "Expected a log dispatch within 3 seconds" );
            Assert.IsTrue( _dispatchedEvents.Count > 0 );

            var payload = _dispatchedEvents[ 0 ].EventPayload;
            Assert.IsNotNull( payload );
            Assert.IsTrue( payload.TryGetValue( "message", out var msg ) );
            StringAssert.Contains( msg?.ToString(), "fully operational" );
        }

        #endregion
    }
}
