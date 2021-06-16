﻿using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class SquadronStatusEvent : Event
    {
        public const string NAME = "Squadron status";
        public const string DESCRIPTION = "Triggered when your status with a squadron has changed";
        public const string SAMPLE = null;
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SquadronStatusEvent()
        {
            VARIABLES.Add("name", "The squadron name");
            VARIABLES.Add("status", "The squadron status (`applied`, `created`, `disbanded`, `invited`, `joined`, `kicked`, `left`)");
        }

        [PublicAPI]
        public string name { get; private set; }

        [PublicAPI]
        public string status { get; private set; }

        public SquadronStatusEvent(DateTime timestamp, string name, string status) : base(timestamp, NAME)
        {
            this.name = name;
            this.status = status;
        }
    }
}
