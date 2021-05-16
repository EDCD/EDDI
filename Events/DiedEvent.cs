using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class DiedEvent : Event
    {
        public const string NAME = "Died";
        public const string DESCRIPTION = "Triggered when you have died";
        public const string SAMPLE = @"{ ""timestamp"":""2016-12-29T10:15:26Z"", ""event"":""Died"", ""KillerName"":""$ShipName_Military_Federation;"", ""KillerName_Localised"":""Federal Navy Ship"", ""KillerShip"":""viper"", ""KillerRank"":""Deadly"" }";
        //        public const string SAMPLE = @"{ ""timestamp"":""2016-06-10T14:32:03Z"", ""event"":""Died"", ""Killers"":[ { ""Name"":""Cmdr HRC1"", ""Ship"":""Vulture"", ""Rank"":""Competent"" }, { ""Name"":""Cmdr HRC2"", ""Ship"":""Python"", ""Rank"":""Master"" } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static DiedEvent()
        {
            VARIABLES.Add("killers", "A list of objects describing your killers");
        }

        public List<Killer> killers { get; private set; }

        public DiedEvent(DateTime timestamp, List<Killer> killers) : base(timestamp, NAME)
        {
            this.killers = killers;
        }
    }
}
