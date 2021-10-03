﻿using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class CargoEvent : Event
    {
        public const string NAME = "Cargo";
        public const string DESCRIPTION = "Triggered when a vehicle cargo inventory is updated";
        public const string SAMPLE = null;

        // Not intended to be user facing

        public bool update { get; private set; }

        public string vessel { get; private set; }

        public List<CargoInfo> inventory { get; private set; }

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
