#nullable enable

using System;
using System.Threading.Tasks;

namespace EddiVoiceAttackAdapter.Extensions
{
    internal static class TaskExtensions
    {
        public static void SafeFireAndForget ( this Task task, Action<Exception>? onException = null )
        {
            ArgumentNullException.ThrowIfNull( task );

            _ = ObserveAsync( task, onException );
        }

        private static async Task ObserveAsync ( Task task, Action<Exception>? onException )
        {
            try
            {
                await task.ConfigureAwait( false );
            }
            catch ( Exception ex )
            {
                onException?.Invoke( ex );
            }
        }
    }
}
