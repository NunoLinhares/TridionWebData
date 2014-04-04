using System.Linq;
using TridionWebData.Models;

namespace TridionWebData.Context
{
    public class TridionDataContext
    {
        public IQueryable<Article> Articles
        {
            get { return Article.GetArticles().AsQueryable(); }
        }

        public IQueryable<Author> Authors
        {
            get { return Author.GetAuthors().AsQueryable(); }
        }

        public IQueryable<InformationSource> InformationSources
        {
            get { return InformationSource.GetInformationSources().AsQueryable(); }
        }
    }
}