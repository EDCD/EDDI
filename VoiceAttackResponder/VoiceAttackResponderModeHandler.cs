#nullable enable

using EddiCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal static class VoiceAttackResponderModeHandler
    {
        private const string VoiceAttackResponderName = "VoiceAttack responder";

        public static System.Version? VoiceAttackVersion { get; private set; }

        /// <summary>
        /// Enable or disable VoiceAttack responder mode.
        /// </summary>
        public static async Task SetResponderModeAsync( bool enable, System.Version? voiceAttackVersion,
            CancellationToken cancellationToken = default )
        {
            cancellationToken.ThrowIfCancellationRequested();

            EDDI.Instance.FromVA = enable;
            if ( voiceAttackVersion != null )
            {
                VoiceAttackVersion = voiceAttackVersion;
                Logging.Info( $"Set VoiceAttackResponderModeHandler.VoiceAttackVersion = {voiceAttackVersion}" );
            }

            Logging.Info( $"Set EDDI.FromVA = {enable}" );

            if ( enable )
            {
                EDDI.Instance.EnableResponder( VoiceAttackResponderName );
                await VoiceAttackResponderMode.InitializeAsync().ConfigureAwait( false );
                VoiceAttackVariables.NotifyVoiceAttackRuntimeSessionReady();
                _ = Task.Run( async () =>
                {
                    try
                    {
                        await VoiceAttackResponderMode.ReplayStandardValuesAsync(
                            "VoiceAttack IPC responder-mode background variable sync",
                            CancellationToken.None ).ConfigureAwait( false );
                        VoiceAttackVariables.WriteRuntimeLog(
                            "EDDI VoiceAttack variables synchronized.",
                            "green" );
                    }
                    catch ( Exception ex )
                    {
                        Logging.Error( "VoiceAttack responder-mode background variable sync failed", ex );
                        VoiceAttackVariables.setStatus( "VoiceAttack variable sync failed", ex );
                    }
                }, cancellationToken );
            }
            else
            {
                EDDI.Instance.DisableResponder( VoiceAttackResponderName );
                VoiceAttackVariables.ClearDispatchCache();
                VoiceAttackResponderMode.Shutdown();
            }
        }
    }
}
