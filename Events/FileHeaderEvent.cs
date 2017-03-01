using EddiDataDefinitions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EddiEvents
{
    public class FileHeaderEvent : Event
    {
        public const string NAME = "File Header";
        public const string DESCRIPTION = "Triggered when the file header is read";
        public const string SAMPLE = @"{""timestamp"":""2016-06-10T14:31:00Z"", “event”:”FileHeader”, ""part"":1, ""gameversion"":""2.2"", ""build"":""r113684"" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FileHeaderEvent()
        {
            VARIABLES.Add("version", "The version of the game");
            VARIABLES.Add("build", "The build of the game");
        }

        [JsonProperty("version")]
        public string version { get; private set; }

        [JsonProperty("build")]
        public string build { get; private set; }

        public FileHeaderEvent(DateTime timestamp, string version, string build) : base(timestamp, NAME)
        {
            this.version = version;
            this.build = build;
        }
    }
}
