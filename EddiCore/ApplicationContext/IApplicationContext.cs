using System;
using System.Threading.Tasks;

namespace EddiCore.ApplicationContext
{
    /// <summary>
    /// Abstracts UI thread operations to support both WPF and headless modes.
    /// Allows the core EDDI engine to work with or without a UI dispatcher.
    /// </summary>
    public interface IApplicationContext
    {
        /// <summary>
        /// Whether a UI dispatcher is available (true for UI mode, false for headless)
        /// </summary>
        bool HasUIDispatcher { get; }

        /// <summary>
        /// Invoke an action on the UI thread, if available. Otherwise executes on current thread.
        /// </summary>
        void InvokeOnUIThread(Action action);

        /// <summary>
        /// Invoke an async action on the UI thread, if available. Otherwise executes on current thread.
        /// </summary>
        Task InvokeOnUIThreadAsync(Func<Task> action);

        /// <summary>
        /// Invoke a function on the UI thread and return its result, if available. Otherwise executes on current thread.
        /// </summary>
        T InvokeOnUIThread<T>(Func<T> function);

        /// <summary>
        /// Invoke an async function on the UI thread and return its result, if available. Otherwise executes on current thread.
        /// </summary>
        Task<T> InvokeOnUIThreadAsync<T>(Func<Task<T>> function);
    }
}
