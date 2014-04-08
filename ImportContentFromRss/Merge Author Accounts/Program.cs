using ImportContentFromRss;
using ImportContentFromRss.Content;
using System;
using System.Collections.Generic;
using Tridion.ContentManager.CoreService.Client;

namespace Merge_Author_Accounts
{
    class Program
    {
        static void Main(string[] args)
        {
            SessionAwareCoreServiceClient client = new SessionAwareCoreServiceClient("netTcp_2013");

            const string primaryAccountName = "Chris Summers";
            List<string> alternateNames = new List<string> { "blog.nospam@nospam.urbancherry.net (chris)" };

            ContentManager cm = new ContentManager(client);
            List<Person> persons = cm.GetPersons();
            Person primary = null;

            foreach (Person p in persons)
            {
                if (p.Name == primaryAccountName)
                {
                    primary = p;
                    break;
                }
            }
            if (primary == null)
            {
                Console.WriteLine("Could not find primary account with name " + primaryAccountName);
                return;
            }
            
            List<Person> duplicates = new List<Person>();
            foreach (string name in alternateNames)
            {
                foreach (Person p in persons)
                {
                    if(p.Name == name)
                        duplicates.Add(p);
                }
            }

            // 1. Add alternate names to primary account
            // 2. Move author of alternate content to primary account
            // 3. Republish content
            // 4. Delete alternate person accounts

            List<string> currentNames = primary.AlternateNames;
            bool changedAuthorName = false;
            foreach (string name in alternateNames)
            {
                if (!currentNames.Contains(name))
                {
                    currentNames.Add(name);
                    changedAuthorName = true;
                }
            }

            if (changedAuthorName)
            {
                primary.AlternateNames = currentNames;
                primary.Save(true);
            }
            List<string> articles = new List<string>();
            foreach (Person p in duplicates)
            {
                foreach (Article a in cm.GetArticlesForPerson(p))
                {
                    // BUG in a.Authors!!
                    // It is not replacing the values, just adding.
                    a.Authors = new List<Person>{primary};
                    a.Save(true);
                    articles.Add(a.Id.GetVersionlessUri().ToString().Replace("tcm:4-", "tcm:5-"));
                }
            }

            cm.Publish(articles.ToArray(), Constants.TargetUri);



            foreach (Person p in duplicates)
            {
                try
                {
                    client.Delete(p.Id);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Could not delete component with id " + p.Id + ". Exception: " + ex);
                }
            }

        }
    }
}
