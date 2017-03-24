using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalnetMonitor
{
    public interface GalnetRepository
    {
        News GetNews(string uuid);
        Dictionary<string, List<News>> GetLatest(int entries, string category);
        void SaveNews(News news);
        void MarkRead(News news);
        void MarkUnread(News news);
    }
}
