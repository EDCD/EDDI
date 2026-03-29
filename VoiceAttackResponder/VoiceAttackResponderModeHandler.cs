#nullable enable

using Eddi;
using EddiCore;
using System.Threading;
using System.Threading.Tasks;
using Utilities;

namespace EddiVoiceAttackResponder
{
    internal static class VoiceAttackResponderModeHandler
    {
        private const string VoiceAttackResponderName = "VoiceAttack responder";

        /// <summary>
        /// Enable or disable VoiceAttack responder mode.
        /// </summary>
        public static Task SetResponderModeAsync( bool enable, System.Version? voiceAttackVersion,
            CancellationToken cancellationToken = default )
        {
            cancellationToken.ThrowIfCancellationRequested();

            EDDI.Instance.FromVA = enable;
            if ( voiceAttackVersion != null )
            {
                App.VoiceAttackVersion = voiceAttackVersion;
                Logging.Info( $"Set App.VoiceAttackVersion = {voiceAttackVersion}" );
            }

            Logging.Info( $"Set EDDI.FromVA = {enable}" );

            if ( enable )
            {
                EDDI.Instance.EnableResponder( VoiceAttackResponderName );
            }
            else
            {
                EDDI.Instance.DisableResponder( VoiceAttackResponderName );
            }

            return Task.CompletedTask;
        }
    }
}
