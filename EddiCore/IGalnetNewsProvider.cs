using EddiDataDefinitions;
using System.Collections.Generic;

namespace EddiCore
{
    public interface IGalnetNewsProvider
    {
        News GetArticle ( string uuid );
        List<News> GetArticles ( string category = null, bool includeRead = false );
        void DeleteArticle ( string uuid );
        void MarkArticleRead ( string uuid );
        void MarkArticleUnread ( string uuid );
    }
}
