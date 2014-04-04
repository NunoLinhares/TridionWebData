using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImportContentFromRss;
using ImportContentFromRss.Content;
using Tridion.ContentManager.CoreService.Client;

namespace Create_UpdatePagesForMissingContent
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient("netTcp_2013");

            ContentManager cm = new ContentManager(client);
            List<Source> sources = cm.GetSources();

            foreach (Source source in sources)
            {
                
                List<Article> articles = cm.GetArticlesForSource(source);
                Console.WriteLine("Checking {0} articles for source {1}", articles.Count, source.Title);
                foreach (Article article in articles)
                {
                    if(cm.IsArticleInPage(article))
                        continue;
                    Console.WriteLine("Article {0} was not in any page... adding.", article.Title);
                    string sg = cm.GetStructureGroup(source.Title, cm.ResolveUrl(Constants.RootStructureGroup));
                    string yearSg = cm.GetStructureGroup(article.Date.Year.ToString(CultureInfo.InvariantCulture), sg);
                    cm.AddToPage(yearSg, article);
                }

            }

        }
    }
}
