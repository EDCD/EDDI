using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalnetMonitor
{
    /// <summary>
    /// A news item
    /// </summary>
    public class News
    {
        public string uuid { get; private set; }
        public DateTime published { get; private set; }
        public string category { get; private set; }
        public string title { get; private set; }
        public string content { get; private set; }

        public News(string uuid, DateTime published, string category, string title, string content)
        {
            this.uuid = uuid;
            this.published = published;
            this.category = category;
            this.title = title;
            this.content = content;
        }
    }
}
