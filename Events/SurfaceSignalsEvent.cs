using EddiDataDefinitions;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class SurfaceSignalsEvent : Event
    {
        public const string NAME = "Surface signals detected";
        public const string DESCRIPTION = "Triggered when surface signal sources are detected";
        public const string SAMPLE = @"{ ""timestamp"":""2019-08-19T00:36:40Z"", ""event"":""SAASignalsFound"", ""BodyName"":""Oponner 6 a"", ""SystemAddress"":3721345878371, ""BodyID"":30, ""Signals"":[ { ""Type"":""$SAA_SignalType_Geological;"", ""Type_Localised"":""Geological"", ""Count"":48 } ] }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static SurfaceSignalsEvent()
        {
            VARIABLES.Add("bodyname", "The body where surface signals were detected");
            VARIABLES.Add("surfacesignals", "A list of SignalAmount objects (with properties 'source' and 'amount')");
        }

        public string bodyname { get; private set; }
        public List<SignalAmount> surfacesignals { get; private set; }

        // Not intended to be user facing
        public long? systemAddress { get; private set; }
        public long bodyId { get; private set; }

        public SurfaceSignalsEvent(DateTime timestamp, long? systemAddress, string bodyName, long bodyId, List<SignalAmount> surfaceSignals) : base(timestamp, NAME)
        {
            this.systemAddress = systemAddress;
            this.bodyname = bodyName;
            this.bodyId = bodyId;
            this.surfacesignals = surfaceSignals;
        }
    }
}
