#nullable enable

using EddiIPC_Service.Server;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EddiVoiceAttackResponder
{
    /// <summary>
    /// IPC command dispatcher for VoiceAttack command execution.
    /// </summary>
    internal sealed class VoiceAttackCommandDispatcher : ICommandDispatcher
    {
        /// <summary>
        /// Dispatch a VoiceAttack command to the invocation handler.
        /// </summary>
        /// <param name="commandName">The command identifier.</param>
        /// <param name="parameters">Optional command parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        public Task DispatchAsync(string commandName, IReadOnlyDictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(commandName))
            {
                throw new ArgumentException(@"Command name cannot be null or whitespace.", nameof(commandName));
            }

            VoiceAttackInvokationHandler.HandleInvokedCommand(commandName, parameters);
            return Task.CompletedTask;
        }
    }
}
