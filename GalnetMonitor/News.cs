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
        public DateTime published { get; private set; }
        public string title { get; private set; }
        public string content { get; private set; }

        public News(DateTime published, string title, string content)
        {
            this.published = published;
            this.title = title;
            this.content = content;
        }
    }
}
