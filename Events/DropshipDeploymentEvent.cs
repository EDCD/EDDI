﻿using System;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DropshipDeploymentEvent : Event
    {
        public const string NAME = "Dropship deployment";
        public const string DESCRIPTION = "Triggered when exiting a military dropship at an on-foot conflict zone";
        public const string SAMPLE = "{ \"timestamp\":\"2021-05-25T04:03:12Z\", \"event\":\"DropshipDeploy\", \"StarSystem\":\"HIP 61072\", \"SystemAddress\":1694104586603, \"Body\":\"HIP 61072 A 3 a\", \"BodyID\":21, \"OnStation\":false, \"OnPlanet\":true }";

        [PublicAPI("The name of the star system where the commander is deployed")]
        public string systemname { get; }

        [PublicAPI("The name of the body where the commander is deployed")]
        public string bodyname { get; private set; }

        // Not intended to be user facing

        public ulong systemAddress { get; }

        public int? bodyId { get; }

        public DropshipDeploymentEvent(DateTime timestamp, string system, ulong systemAddress, string body, int? bodyId) : base(timestamp, NAME)
        {
            this.systemname = system;
            this.systemAddress = systemAddress;
            this.bodyname = body;
            this.bodyId = bodyId;
        }
    }
}