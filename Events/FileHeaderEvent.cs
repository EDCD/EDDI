using System;
using System.Collections.Generic;
using Utilities;

namespace EddiEvents
{
    [PublicAPI]
    public class FileHeaderEvent : Event
    {
        public const string NAME = "File Header";
        public const string DESCRIPTION = "Triggered when the file header is read";
        public const string SAMPLE = @"{""timestamp"":""2017-09-04T00:20:48Z"", ""event"":""Fileheader"", ""part"":1, ""language"":""English\\UK"", ""gameversion"":""2.4 (Beta 4)"", ""build"":""r153766/r0 "" }";

        [PublicAPI("The version of the game")]
        public string version { get; private set; }

        [PublicAPI("The build of the game")]
        public string build { get; private set; }

        // Not intended to be user facing

        public string filename { get; private set; }

        public FileHeaderEvent(DateTime timestamp, string filename, string version, string build) : base(timestamp, NAME)
        {
            this.filename = filename;
            this.version = version;
            this.build = build;
        }

        public static bool Handle ( DateTime timestamp, string journalFileName, string line, IDictionary<string, object> data, ref List<Event> events, bool fromLogLoad )
        {
            var version = JsonParsing.getString(data, "gameversion")?.Trim();
            var build = JsonParsing.getString(data, "build")?.Trim();
            Logging.Info( $"GameVersion: {version}, Build {build}." );
            events.Add( new FileHeaderEvent( timestamp, journalFileName, version, build ) { raw = line, fromLoad = fromLogLoad } );
            return true;
        }
    }
}
