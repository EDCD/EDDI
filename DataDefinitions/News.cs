using System;
using Utilities;

namespace EddiDataDefinitions
{
    /// <summary>
    /// A news item
    /// </summary>
    public class News ( string id, string category, string title, string content, DateTime published, bool read )
    {
        [PublicAPI]
        public string id { get; private set; } = id;

        [PublicAPI]
        public DateTime published { get; private set; } = published;

        [PublicAPI]
        public string category { get; private set; } = category;

        [PublicAPI]
        public string title { get; private set; } = title;

        [PublicAPI]
        public string content { get; private set; } = content;

        [PublicAPI]
        public bool read { get; private set; } = read;
    }
}
