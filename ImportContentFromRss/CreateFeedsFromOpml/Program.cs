using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ImportContentFromRss;
using ImportContentFromRss.Content;
using Tridion.ContentManager.CoreService.Client;

namespace CreateFeedsFromOpml
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient("netTcp_2013");

            ContentManager cm = new ContentManager(client);
            List<Source> sources = cm.GetSources();

            XDocument opml = XDocument.Load("feeds.xml");
            foreach (XElement node in opml.Root.Elements("body").Elements("outline"))
            {
                // check if it already exists
                string url = node.Attribute("xmlUrl").Value;
                bool found = sources.Any(source => source.RssFeedUrl == url);
                if (!found)
                {
                    Source source = new Source(client)
                    {
                        RssFeedUrl = url,
                        Title = node.Attribute("text").Value
                    };
                    source.Save();
                    Console.WriteLine("Created new source: " + source.Title);
                }
            }
        }
    }
}
