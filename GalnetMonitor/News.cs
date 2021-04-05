using System;
using Utilities;

namespace GalnetMonitor
{
    /// <summary>
    /// A news item
    /// </summary>
    public class News
    {
        [PublicAPI]
        public string id { get; private set; }

        [PublicAPI]
        public DateTime published { get; private set; }

        [PublicAPI]
        public string category { get; private set; }

        [PublicAPI]
        public string title { get; private set; }

        [PublicAPI]
        public string content { get; private set; }

        [PublicAPI]
        public bool read { get; private set; }

        public News(string id, string category, string title, string content, DateTime published, bool read)
        {
            this.id = id;
            this.category = category;
            this.title = title;
            this.content = content;
            this.published = published;
            this.read = read;
        }
    }
}
