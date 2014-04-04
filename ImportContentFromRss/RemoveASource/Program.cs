using ImportContentFromRss;
using ImportContentFromRss.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.CoreService.Client;

namespace RemoveASource
{
    class Program
    {
        static void Main(string[] args)
        {
            const string titleOfSourceToRemove = "Meet John Song";
            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient("netTcp_2013");

            ContentManager cm = new ContentManager(client);
            List<Source> sources = cm.GetSources();
            Source sourceToRemove = sources.FirstOrDefault(source => source.Title == titleOfSourceToRemove);
            if (sourceToRemove == null)
            {
                Console.WriteLine("Could not find source with name " + titleOfSourceToRemove);
                return;
            }

            // find all usages of the source
            // Delete all pages for the source
            // Unpublish all pages & Components
            // Delete
            List<Article> articles = cm.GetArticlesForSource(sourceToRemove);
            List<string> ids = new List<string>();
            List<string> pageIds = new List<string>();
            foreach (Article article in articles)
            {
                ids.Add(article.Id);
                string pageId = cm.GetPageIdForArticle(article);
                if (pageId != null)
                {
                    pageIds.Add(pageId);
                }
            }

            // need pages
            cm.Unpublish(ids.ToArray(), "tcm:0-2-65537");
            cm.Unpublish(pageIds.ToArray(), "tcm:0-2-65537");
            Console.WriteLine("Wait for unpublish to finish...");
            Console.Read();
            // Must delete pages first


            foreach (string pageId in pageIds)
            {
                try
                {
                    if(client.IsExistingObject(pageId))
                        client.Delete(pageId);
                }
                catch (Exception ex)
                { Console.WriteLine("Could not delete item with ID: " + pageId + ex.ToString()); }

            }

            foreach (string id in ids)
            {
                try
                {
                    if(client.IsExistingObject(id))
                        client.Delete(id);
                }
                catch(Exception ex)
                { Console.WriteLine("Could not delete item with ID: " + id + ex.ToString());}
                
            }
        }
    }
}
