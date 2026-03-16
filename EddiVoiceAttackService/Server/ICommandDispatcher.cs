#nullable enable

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EddiVoiceAttackService.Server
{
    /// <summary>
    /// Dispatches IPC commands to the application command handling pipeline.
    /// </summary>
    public interface ICommandDispatcher
    {
        /// <summary>
        /// Dispatch a command for execution.
        /// </summary>
        /// <param name="commandName">The command identifier.</param>
        /// <param name="parameters">Optional command parameters.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task DispatchAsync(string commandName, IReadOnlyDictionary<string, object>? parameters = null,
            CancellationToken cancellationToken = default);
    }
}
