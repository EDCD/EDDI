using System;

namespace GalnetMonitor
{
    /// <summary>
    /// A news item
    /// </summary>
    public class News
    {
        public string id { get; private set; }
        public DateTime published { get; private set; }
        public string category { get; private set; }
        public string title { get; private set; }
        public string content { get; private set; }
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
