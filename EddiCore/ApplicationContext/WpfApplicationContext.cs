using System;
using System.Threading.Tasks;
using System.Windows;
using Utilities;

namespace EddiCore.ApplicationContext
{
    /// <summary>
    /// WPF-specific application context that uses the Application.Current dispatcher.
    /// </summary>
    public class WpfApplicationContext : IApplicationContext
    {
        public bool HasUIDispatcher => Application.Current?.Dispatcher != null;

        public void InvokeOnUIThread(Action action)
        {
            if ( action == null )
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (HasUIDispatcher && Application.Current.Dispatcher.CheckAccess())
            {
                // Already on UI thread
                action();
            }
            else if (HasUIDispatcher)
            {
                // Dispatch to UI thread
                Application.Current.Dispatcher.Invoke(action);
            }
            else
            {
                // No dispatcher available - execute on current thread
                Logging.Debug("WPF context: executing action on current thread (no UI dispatcher)");
                action();
            }
        }

        public async Task InvokeOnUIThreadAsync(Func<Task> action)
        {
            if ( action == null )
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (HasUIDispatcher && Application.Current.Dispatcher.CheckAccess())
            {
                // Already on UI thread
                await action();
            }
            else if (HasUIDispatcher)
            {
                // Dispatch to UI thread
                await Application.Current.Dispatcher.InvokeAsync(action);
            }
            else
            {
                // No dispatcher available - execute on current thread
                Logging.Debug("WPF context: executing async action on current thread (no UI dispatcher)");
                await action();
            }
        }

        public T InvokeOnUIThread<T>(Func<T> function)
        {
            if ( function == null )
            {
                throw new ArgumentNullException(nameof(function));
            }

            if (HasUIDispatcher && Application.Current.Dispatcher.CheckAccess())
            {
                // Already on UI thread
                return function();
            }

            if (HasUIDispatcher)
            {
                // Dispatch to UI thread
                return Application.Current.Dispatcher.Invoke(function);
            }

            // No dispatcher available - execute on current thread
            Logging.Debug("WPF context: executing function on current thread (no UI dispatcher)");
            return function();
        }

        public async Task<T> InvokeOnUIThreadAsync<T>(Func<Task<T>> function)
        {
            if ( function == null )
            {
                throw new ArgumentNullException(nameof(function));
            }

            if (HasUIDispatcher && Application.Current.Dispatcher.CheckAccess())
            {
                // Already on UI thread
                return await function();
            }

            if (HasUIDispatcher)
            {
                // Dispatch to UI thread and await the result
                var dispatcherTask = Application.Current.Dispatcher.InvokeAsync(async () => await function());
                return await dispatcherTask.Task.Unwrap();
            }

            // No dispatcher available - execute on current thread
            Logging.Debug("WPF context: executing async function on current thread (no UI dispatcher)");
            return await function();
        }
    }
}
