using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A news item
    /// </summary>
    public class News ( string id, string category, string title, string content, DateTime published, bool read )
    {
        [PublicAPI( "the news item's unique identifier" )]
        public string id { get; private set; } = id;

        [PublicAPI( "the date and time the news item was published, as a DateTime object" )]
        public DateTime published { get; private set; } = published;

        [PublicAPI( "the news item's category" )]
        public string category { get; private set; } = category;

        [PublicAPI( "the news item's title" )]
        public string title { get; private set; } = title;

        [PublicAPI( "the news item's content" )]
        public string content { get; private set; } = content;

        [PublicAPI( "whether the news item has been read" )]
        public bool read { get; private set; } = read;
    }
}
