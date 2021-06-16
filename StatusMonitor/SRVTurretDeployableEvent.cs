﻿using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class SRVTurretDeployableEvent : Event
    {
        public const string NAME = "SRV turret deployable";
        public const string DESCRIPTION = "Triggered when your SRV enters or leaves the restriction zone around a ship.";
        public static SRVTurretDeployableEvent SAMPLE = new SRVTurretDeployableEvent(DateTime.UtcNow, true);
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SRVTurretDeployableEvent()
        {
            VARIABLES.Add("deployable", "A boolean value. True if you are leaving the restriction zone around a ship.");
        }

        [PublicAPI]
        public bool deployable { get; }

        public SRVTurretDeployableEvent(DateTime timestamp, bool deployable) : base(timestamp, NAME)
        {
            this.deployable = deployable;
        }
    }
}
