using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImportContentFromRss;
using ImportContentFromRss.Content;
using Tridion.ContentManager.CoreService.Client;

namespace PublishItemsThatAreNotPublished
{
    class Program
    {
        static void Main()
        {
            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient("netTcp_2013");

            ContentManager cm = new ContentManager(client);
            List<Source> sources = cm.GetSources();
            foreach (var source in sources)
            {
                List<Article> articles = cm.GetArticlesForSource(source);
                foreach (Article article in articles)
                {
                    string id = article.Id.ToString().Replace("tcm:4", "tcm:5");
                    if(!client.IsPublished(id, "tcm:0-2-65537", true))
                        cm.Publish(new []{id}, "tcm:0-2-65537");
                }
            }
        }
    }
}
