using EddiDataDefinitions;
using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class DiedEvent : Event
    {
        public const string NAME = "Died";
        public const string DESCRIPTION = "Triggered when you have died";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:15:26Z"", ""event"":""Died"", ""KillerName"":""$ShipName_Military_Federation;"", ""KillerName_Localised"":""Federal Navy Ship"", ""KillerShip"":""viper"", ""KillerRank"":""Deadly"" }";

        [PublicAPI("A list of objects describing your killers")]
        public List<Killer> killers { get; private set; }

        public DiedEvent(DateTime timestamp, List<Killer> killers) : base(timestamp, NAME)
        {
            this.killers = killers;
        }
    }
}
