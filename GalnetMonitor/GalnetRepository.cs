namespace GalnetMonitor
{
    public interface GalnetRepository
    {
        News GetArticle(string uuid);
        //Dictionary<string, List<News>> GetLatest(int entries, string category);
        void SaveNews(News news);
        void MarkRead(News news);
        void MarkUnread(News news);
    }
}
