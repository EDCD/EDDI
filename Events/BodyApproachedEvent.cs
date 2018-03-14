using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace EddiEvents
{
    public class BodyApproachedEvent : Event
    {
        public const string NAME = "Body approached";
        public const string DESCRIPTION = "Triggered when you approach a landable planet and enter orbital cruise";
        public const string SAMPLE = @"{ ""timestamp"":""2016-09-22T15:24:14Z"", ""event"":""ApproachBody"", ""StarSystem"":""Eranin"", ""Body"":""Eranin 2""}";

        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static BodyApproachedEvent()
        {
            VARIABLES.Add("system", "The name of the starsystem");
            VARIABLES.Add("body", "The name of the body");
        }

        public string system { get; private set; }
        public string body { get; private set; }

        public BodyApproachedEvent(DateTime timestamp, string system, string body) : base(timestamp, NAME)
        {
            this.system = system;
            this.body = body;
        }
    }
}
