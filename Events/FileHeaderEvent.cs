using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    public class FileHeaderEvent : Event
    {
        public const string NAME = "File Header";
        public const string DESCRIPTION = "Triggered when the file header is read";
        public const string SAMPLE = @"{""timestamp"":""2017-09-04T00:20:48Z"", ""event"":""Fileheader"", ""part"":1, ""language"":""English\\UK"", ""gameversion"":""2.4 (Beta 4)"", ""build"":""r153766/r0 "" }";
        public static Dictionary<string, string> VARIABLES = new Dictionary<string, string>();

        static FileHeaderEvent()
        {
            VARIABLES.Add("version", "The version of the game");
            VARIABLES.Add("build", "The build of the game");
        }

        public string version { get; private set; }

        public string build { get; private set; }

        // Not intended to be user facing

        [VoiceAttackIgnore]
        public string filename { get; private set; }

        public FileHeaderEvent(DateTime timestamp, string filename, string version, string build) : base(timestamp, NAME)
        {
            this.filename = filename;
            this.version = version;
            this.build = build;
        }
    }
}
