﻿using EddiEvents;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Windows.Controls;

namespace EddiCore
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
        /// A localized name for the monitor
        /// </summary>
        string LocalizedMonitorName();

        /// <summary>
        /// A brief description of the monitor
        /// </summary>
        string MonitorDescription();

        /// <summary>
        /// If the monitor is required to be running. This must be true unless the monitor does not consume events (e.g. the event and profile handler methods are empty).
        /// </summary>
        bool IsRequired();

        /// <summary>
        /// If the monitor needs to be explicitly started/stopped
        /// </summary>
        bool NeedsStart();

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
        /// Called prior to responders running.  This should be used to update global states.
        /// </summary>
        void PreHandle(Event @event);

        /// <summary>
        /// Called after responders running.  This should be used to generate follow-on events which do not change global states.
        /// </summary>
        void PostHandle(Event @event);

        /// <summary>
        /// Called to receive information from the Frontier API
        /// </summary>
        void HandleProfile(JObject profile);

        /// <summary>
        /// Provide any local variables
        /// </summary>
        IDictionary<string, object> GetVariables();

        UserControl ConfigurationTabItem();
    }
}
