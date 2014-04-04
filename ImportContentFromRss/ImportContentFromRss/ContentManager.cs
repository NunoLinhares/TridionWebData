using System.ComponentModel;
using ImportContentFromRss.Content;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CoreService.Client;
using ItemType = Tridion.ContentManager.CoreService.Client.ItemType;

namespace ImportContentFromRss
{
    public class ContentManager
    {
        private List<Person> _persons;
        private List<Source> _sources;
        private List<string> _keywords;
        private bool _keywordDirty;
        private static readonly Dictionary<string, string> ResolvedUrls = new Dictionary<string, string>();
        private readonly SessionAwareCoreServiceClient _client;
        private readonly ReadOptions _readOptions;

        public ContentManager(SessionAwareCoreServiceClient client)
        {
            _client = client;
            _readOptions = new ReadOptions();
        }

        public bool IsArticleInPage(Article article)
        {
            UsingItemsFilterData filter = new UsingItemsFilterData { ItemTypes = new[] { ItemType.Page } };
            return _client.GetListXml(article.Id, filter).Nodes().Any();
        }

        //public List<Page> GetNewsletterPages()
        //{
        //    List<Page> result = new List<Page>();
        //    StructureGroup sg = (StructureGroup)_session.GetObject(Constants.NewsletterStructureGroupUrl);
        //    OrganizationalItemItemsFilter filter = new OrganizationalItemItemsFilter(_session) { ItemTypes = new[] { ItemType.Page } };

        //    foreach (Page page in sg.GetItems(filter))
        //    {
        //        result.Add(page);
        //    }
        //    return result;
        //}

        public List<Article> GetArticlesForSource(Source source)
        {
            List<Article> result = new List<Article>();
            UsingItemsFilterData filter = new UsingItemsFilterData { ItemTypes = new[] { ItemType.Component } };
            foreach (var xNode in _client.GetListXml(source.Id, filter).Nodes())
            {
                var node = (XElement) xNode;
                result.Add(new Article((ComponentData)_client.Read(node.Attribute("ID").Value, _readOptions), _client));
            }
            return result;
        }

        public List<Article> GetArticlesForPerson(Person person)
        {
            List<Article> result = new List<Article>();
            UsingItemsFilterData filter = new UsingItemsFilterData {ItemTypes = new[] {ItemType.Component}};
            foreach (var xNode in _client.GetListXml(person.Id, filter).Nodes())
            {
                var node = (XElement) xNode;
                result.Add(new Article((ComponentData)_client.Read(node.Attribute("ID").Value, _readOptions), _client));
            }
            return result;
        }

        public List<Article> GetArticlesForDate(DateTime date)
        {
            List<Article> result = new List<Article>();
            string folderId = GetFolderForDate(date);
            OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Component } };

            foreach (var xNode in _client.GetListXml(folderId, filter).Nodes())
            {
                var node = (XElement) xNode;
                result.Add(new Article((ComponentData)_client.Read(node.Attribute("ID").Value, _readOptions), _client));
            }

            return result;
        }

        public string GetFolderForDate(DateTime date)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            //Console.WriteLine("Searching folder for date " + date.ToString("yyyy-MM-dd"));
            //Folder start = (Folder)_session.GetObject(Constants.ArticleLocationUrl);
            OrganizationalItemItemsFilterData folders = new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Folder } };

            string year = null;
            string month = null;
            string day = null;

            string yearName = date.Year.ToString(CultureInfo.InvariantCulture);
            string monthName;
            if (date.Month < 10)
                monthName = "0" + date.Month;
            else
                monthName = date.Month.ToString(CultureInfo.InvariantCulture);
            monthName += " " + date.ToString("MMMM");

            string dayName;
            if (date.Day < 10)
                dayName = "0" + date.Day;
            else
                dayName = date.Day.ToString(CultureInfo.InvariantCulture);

            foreach (var xNode in _client.GetListXml(Constants.ArticleLocationUrl, folders).Nodes()) //start.GetListItems(folders))
            {
                var folderElement = (XElement) xNode;
                if (!folderElement.Attribute("Title").Value.Equals(yearName)) continue;
                year = folderElement.Attribute("ID").Value;
                break;
            }
            if (year == null)
            {
                FolderData f =
                    (FolderData)_client.GetDefaultData(ItemType.Folder, Constants.ArticleLocationUrl, _readOptions);
                f.Title = yearName;
                f = (FolderData)_client.Save(f, _readOptions);
                year = f.Id;
            }

            foreach (var xNode in _client.GetListXml(year, folders).Nodes())//year.GetListItems(folders))
            {
                var monthFolder = (XElement) xNode;
                if (monthFolder.Attribute("Title").Value.Equals(monthName))
                {

                    month = monthFolder.Attribute("ID").Value;
                    break;
                }
            }
            if (month == null)
            {
                FolderData f = (FolderData)_client.GetDefaultData(ItemType.Folder, year, _readOptions);
                f.Title = monthName;
                f = (FolderData)_client.Save(f, _readOptions);
                month = f.Id;
            }
            foreach (var xNode in _client.GetListXml(month, folders).Nodes())//month.GetListItems(folders))
            {
                var dayFolder = (XElement) xNode;
                if (dayFolder.Attribute("Title").Value.Equals(dayName))
                {
                    day = dayFolder.Attribute("ID").Value;
                    break;
                }
            }
            if (day == null)
            {
                FolderData f = (FolderData)_client.GetDefaultData(ItemType.Folder, month, _readOptions);
                f.Title = dayName;
                f = (FolderData)_client.Save(f, _readOptions);
                day = f.Id;
            }
            watch.Stop();
            //Console.WriteLine("Returning folder " + day + " in " + watch.ElapsedMilliseconds + " milliseconds");
            return day;
        }


        public Person FindPersonByNameOrAlternate(string name, bool refresh = false)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            //Console.WriteLine("Searching for author " + name);
            foreach (Person person in GetPersons(refresh))
            {
                if (person.Name.ToLower().Equals(name.ToLower()))
                {
                    watch.Stop();
                    //Console.WriteLine("Found author in " + watch.ElapsedMilliseconds + " milliseconds");
                    return person;
                }
                if (person.AlternateNames.Count > 0)
                {
                    if (person.AlternateNames.Any(alternateName => alternateName.ToLower().Equals(name.ToLower())))
                    {
                        watch.Stop();
                        //Console.WriteLine("Found author in " + watch.ElapsedMilliseconds + " milliseconds");
                        return person;
                    }
                }
            }
            watch.Stop();
            //Console.WriteLine("Could not find author (" + watch.ElapsedMilliseconds + " milliseconds)");
            return null;
        }

        public string ResolveUrl(string webdavUrl)
        {
            if (ResolvedUrls.ContainsKey(webdavUrl))
            {
                return ResolvedUrls[webdavUrl];
            }
            Stopwatch watch = new Stopwatch(); watch.Start();
            IdentifiableObjectData i = _client.Read(webdavUrl, _readOptions);
            ResolvedUrls.Add(webdavUrl, i.Id);
            watch.Stop();
            Console.WriteLine("Resolved URL in " + watch.ElapsedMilliseconds + " milliseconds (" + webdavUrl + ")");
            return i.Id;
        }

        public List<Source> GetSources()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            Console.WriteLine("Getting list of sources...");
            if (_sources == null)
            {
                _sources = new List<Source>();
                //Folder sourceFolder = (Folder)_session.GetObject(Constants.SourceLocationUrl);
                OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Component } };
                foreach (var xNode in _client.GetListXml(Constants.SourceLocationUrl, filter).Nodes())
                {
                    var node = (XElement) xNode;
                    _sources.Add(new Source((ComponentData)_client.Read(node.Attribute("ID").Value, _readOptions), _client));
                    Console.Write(".");
                }
                Console.WriteLine(Environment.NewLine);
            }
            watch.Stop();
            //Console.WriteLine("Returning " + _sources.Count + " sources in " + watch.ElapsedMilliseconds + " milliseconds");
            return _sources;
        }

        public List<Person> GetPersons(bool refresh = false)
        {
            //Stopwatch watch = new Stopwatch();
            //watch.Start();
            
            if (refresh) _persons = null;
            if (_persons == null)
            {
                Console.WriteLine("Loading people...");
                _persons = new List<Person>();
                //Folder people = (Folder)_session.GetObject(Constants.PersonLocationUrl);
                OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Component } };
                foreach (var xNode in _client.GetListXml(Constants.PersonLocationUrl, filter).Nodes())
                {
                    var node = (XElement) xNode;
                    _persons.Add(new Person((ComponentData)_client.Read(node.Attribute("ID").Value, _readOptions), _client));
                    Console.Write(".");
                }
                Console.WriteLine(Environment.NewLine);
            }
            //watch.Stop();
            //Console.WriteLine("Returning " + _persons.Count + " authors in " + watch.ElapsedMilliseconds + " milliseconds");
            return _persons;
        }

        public List<string> GetExistingKeywords(bool refresh = false)
        {
            if (_keywords == null || refresh)
            {
                _keywords = new List<string>();
                OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData
                {
                    ItemTypes = new[] {ItemType.Keyword}
                };
                foreach (var xNode in _client.GetListXml(ResolveUrl(Constants.ContentCategoryUrl), filter).Nodes())
                {
                    var node = (XElement) xNode;
                    _keywords.Add(node.Attribute("Title").Value);
                }
            }
            return _keywords;
        }

        public void CreateKeywords(List<string> keywords)
        {

            List<string> currentKeywords = GetExistingKeywords(_keywordDirty);
            foreach (var keyword in keywords)
            {
                if (currentKeywords.Contains(keyword)) continue;
                // Must create a new one
                _keywordDirty = true;
                KeywordData newKeyword = (KeywordData)_client.GetDefaultData(ItemType.Keyword,
                                                                             ResolveUrl(Constants.ContentCategoryUrl), _readOptions);
                newKeyword.Title = keyword;
                _client.Save(newKeyword, null);
                Console.WriteLine("Created new keyword: \"{0}\".", newKeyword.Title);
            }
        }

        public string GetStructureGroup(string sgTitle, string parentStructureGroup)
        {
            OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData
            {
                ItemTypes = new[] {ItemType.StructureGroup}
            };
            foreach (var xNode in _client.GetListXml(parentStructureGroup, filter).Nodes())
            {
                var node = (XElement) xNode;
                if (node.Attribute("Title").Value.Equals(sgTitle))
                    return node.Attribute("ID").Value;
            }
            StructureGroupData sg =
                (StructureGroupData)
                _client.GetDefaultData(ItemType.StructureGroup, parentStructureGroup, _readOptions);
            sg.Title = sgTitle;
            sg.Directory = Regex.Replace(sgTitle, "\\W", "").ToLowerInvariant().Replace("á", "a").Replace("ó", "o");
            sg = (StructureGroupData)_client.Save(sg, _readOptions);
            return sg.Id;
        }

        internal string GetUriInBlueprintContext(string itemId, string publicationId)
        {
            if (TcmUri.UriNull == itemId)
                return null;
            TcmUri itemUri = new TcmUri(itemId);
            TcmUri publicationUri = new TcmUri(publicationId);
            TcmUri inContext = new TcmUri(itemUri.ItemId, itemUri.ItemType, publicationUri.ItemId);
            return inContext.ToString();
        }

        public string AddToPage(string sg, Article article)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            string foldername;
            if (article.Date.Month < 10)
                foldername = "0" + article.Date.Month;
            else
                foldername = article.Date.Month.ToString(CultureInfo.InvariantCulture);
            foldername += " " + article.Date.ToString("MMMM");
            PageData page = (PageData)_client.Read(GetPage(sg, foldername), _readOptions);
            //if (!page.IsEditable.GetValueOrDefault())
            //{
            //    page = (PageData)_client.CheckOut(page.Id, true, _readOptions);
            //}

            List<ComponentPresentationData> componentPresentations = page.ComponentPresentations.ToList();
            string articleId = GetUriInBlueprintContext(article.Id, ResolveUrl(Constants.WebSitePublication));
            string ctId = GetUriInBlueprintContext(ResolveUrl(Constants.ArticleComponentTemplateUrl),
                                                   ResolveUrl(Constants.WebSitePublication));
            ComponentPresentationData cp = new ComponentPresentationData();
            if (articleId != null && articleId != TcmUri.UriNull)
            {
                cp.Component = new LinkToComponentData { IdRef = articleId };
                cp.ComponentTemplate = new LinkToComponentTemplateData { IdRef = ctId };
                componentPresentations.Add(cp);
                page.ComponentPresentations = componentPresentations.ToArray();
            }
            page = (PageData)_client.Update(page, _readOptions);
            // Looks like it's still checked out at the end of this...
            if (page.IsEditable.HasValue && page.IsEditable == true)
                _client.CheckIn(GetVersionlessUri(page.Id), null);

            watch.Stop();
            Console.WriteLine("Added component presentation in " + watch.ElapsedMilliseconds + " milliseconds");
            return page.Id;
        }

        public string GetPage(string sg, string pageTitle)
        {
            OrganizationalItemItemsFilterData filter = new OrganizationalItemItemsFilterData { ItemTypes = new[] { ItemType.Page } };
            foreach (var xNode in _client.GetListXml(sg, filter).Nodes())
            {
                var node = (XElement) xNode;
                if (node.Attribute("Title").Value.Equals(pageTitle))
                    return node.Attribute("ID").Value;
            }
            PageData page = (PageData)_client.GetDefaultData(ItemType.Page, sg, _readOptions);
            page.Title = pageTitle;
            page.FileName = Regex.Replace(pageTitle, "\\W", "").ToLowerInvariant();
            page = (PageData)_client.Save(page, _readOptions);
            _client.CheckIn(page.Id, null);
            return GetVersionlessUri(page.Id);
        }

        public string GetPageIdForArticle(Article article)
        {
            UsingItemsFilterData filter = new UsingItemsFilterData { ItemTypes = new[] { ItemType.Page } };
            foreach (var xNode in _client.GetListXml(article.Id, filter).Nodes())
            {
                var node = (XElement) xNode;
                return node.Attribute("ID").Value;
            }
            return null;
        }

        public string GetVersionlessUri(string id)
        {
            TcmUri uri = new TcmUri(id);
            if (uri.IsVersionless) return id;
            uri = new TcmUri(uri.ItemId, uri.ItemType, uri.PublicationId);
            return uri.ToString();
        }

        public void Publish(string[] itemIds, string targetId)
        {
            RenderInstructionData renderInstruction = new RenderInstructionData();
            ResolveInstructionData resolveInstruction = new ResolveInstructionData();

            PublishInstructionData publishInstruction = new PublishInstructionData
                                                            {
                                                                DeployAt = DateTime.Now,
                                                                MaximumNumberOfRenderFailures = 0,
                                                                RenderInstruction = renderInstruction,
                                                                ResolveInstruction = resolveInstruction,
                                                                StartAt = DateTime.Now
                                                            };
            _client.PublishAsync(itemIds, publishInstruction, new[] { targetId }, PublishPriority.Normal, null);
        }

        public void Unpublish(string[] itemIds, string targetId)
        {

            ResolveInstructionData resolveInstruction = new ResolveInstructionData
            {
                IncludeComponentLinks = true,
                Purpose = ResolvePurpose.UnPublish,
                IncludeChildPublications = true
            };
            UnPublishInstructionData unPublishInstruction = new UnPublishInstructionData
            {
                StartAt = DateTime.Now,
                ResolveInstruction = resolveInstruction,
            };
            _client.UnPublishAsync(itemIds, unPublishInstruction, new []{targetId}, PublishPriority.Normal, null);
        }

    }
}
