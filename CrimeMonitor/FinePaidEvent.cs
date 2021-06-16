﻿using EddiEvents;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiCrimeMonitor
{
    public class FinePaidEvent : Event
    {
        public const string NAME = "Fine paid";
        public const string DESCRIPTION = "Triggered when you pay a fine";
        public const string SAMPLE = "{ \"timestamp\":\"2018-03-19T10:24:21Z\", \"event\":\"PayFines\", \"Amount\":250, \"AllFines\":false, \"Faction\":\"Batz Transport Commodities\", \"ShipID\":9 }";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FinePaidEvent()
        {
            VARIABLES.Add("amount", "The amount of the fine paid");
            VARIABLES.Add("brokerpercentage", "Broker percentage (if paid via a Broker)");
            VARIABLES.Add("allfines", "Whether this payment covers all current fines (true or false)");
            VARIABLES.Add("faction", "The faction to which the fine was paid (if the payment does not cover all current fines)");
            VARIABLES.Add("shipid", "The ship id of the ship associated with the fine");
        }

        [PublicAPI]
        public long amount { get; private set; }

        [PublicAPI]
        public decimal? brokerpercentage { get; private set; }

        [PublicAPI]
        public bool allfines { get; private set; }

        [PublicAPI]
        public string faction { get; private set; }

        [PublicAPI]
        public int shipid { get; private set; }

        public FinePaidEvent(DateTime timestamp, long amount, decimal? brokerpercentage, bool allFines, string faction, int shipId) : base(timestamp, NAME)
        {
            this.amount = amount;
            this.brokerpercentage = brokerpercentage;
            this.allfines = allFines;
            this.faction = faction;
            this.shipid = shipId;
        }
    }
}
