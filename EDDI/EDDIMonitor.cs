using System.Windows.Controls;
using EddiEvents;
using System.Collections.Generic;
using EddiCompanionAppService;

namespace Eddi
{
    /// <summary>
    /// The methods required for an EDDI monitor.
    /// </summary>
    public interface EDDIMonitor
    {
        /// <summary>
        /// A short name for the monitor
        /// </summary>
        string MonitorName();

        /// <summary>
        /// The version of the monitor
        /// </summary>
        string MonitorVersion();

        /// <summary>
        /// A brief description of the monitor
        /// </summary>
        string MonitorDescription();

        /// <summary>
        /// If the monitor is required to be running
        /// </summary>
        bool IsRequired();

        /// <summary>
        /// Called when this monitor is started.  This is not expected to return whilst the monitor is running
        /// </summary>
        void Start();

        /// <summary>
        /// Called when this monitor is stopped.  This should tell the monitor to shut down
        /// </summary>
        void Stop();

        /// <summary>
        /// Called when this monitor needs to reload its configuration
        /// </summary>
        void Reload();

        /// <summary>
        /// Called to receive information about other events
        /// </summary>
        void Handle(Event @event);

        /// <summary>
        /// Called to receive information from the Frontier API
        /// </summary>
        void Handle(Profile profile);

        /// <summary>
        /// Provide any local variables
        /// </summary>
        IDictionary<string, object> GetVariables();

        UserControl ConfigurationTabItem();
    }
}
