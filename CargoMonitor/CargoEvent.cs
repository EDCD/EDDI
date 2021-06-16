﻿using EddiDataDefinitions;
using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCargoMonitor
{
    public class CargoEvent : Event
    {
        public const string NAME = "Cargo";
        public const string DESCRIPTION = "Triggered when a vehicle cargo inventory is updated";
        public const string SAMPLE = null;

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static CargoEvent()
        {
        }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public bool update { get; private set; }

        [VoiceAttackIgnore]
        public string vessel { get; private set; }

        [VoiceAttackIgnore]
        public List<CargoInfo> inventory { get; private set; }

        [VoiceAttackIgnore]
        public int cargocarried { get; private set; }

        public CargoEvent(DateTime timestamp, bool update, string vessel, List<CargoInfo> inventory, int cargocarried) : base(timestamp, NAME)
        {
            this.update = update;
            this.vessel = vessel;
            this.inventory = inventory;
            this.cargocarried = cargocarried;
        }
    }
}
