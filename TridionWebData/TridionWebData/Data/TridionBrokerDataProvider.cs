using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web.Caching;
using Tridion.ContentDelivery.DynamicContent;
using Tridion.ContentDelivery.DynamicContent.Query;
using Tridion.ContentDelivery.Web.Utilities;
using TridionWebData.Models;

namespace TridionWebData.Data
{
    public class TridionBrokerDataProvider
    {
        private static int _jsonComponentTemplateId = 0;
        private static int _publicationId = 0;
        private static readonly MemoryCache Cache;
        private static readonly CacheItemPolicy Policy;

        static TridionBrokerDataProvider()
        {
            Cache = new MemoryCache(Constants.CacheName);
            Policy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 5, 0) };
        }

        internal static IEnumerable<ComponentPresentation> ComponentPresentations
        {
            get
            {
                // Find Publication
                if (_publicationId == 0)
                {

                    PublicationTitleCriteria publicationTitleCriteria = new PublicationTitleCriteria(Constants.WebSiteName);
                    ResultFilter limit = new LimitFilter(1);
                    Query q = new Query(CriteriaFactory.And(new Criteria[] { publicationTitleCriteria }));
                    q.SetResultFilter(limit);

                    string[] result = q.ExecuteQuery();
                    if (!result.Any())
                    {
                        // oh oh
                        throw new Exception("Could not find a Publication with name \"" + Constants.WebSiteName + "\".");
                    }
                    TcmUri publicationUri = new TcmUri(result[0]);
                    _publicationId = publicationUri.PublicationId;
                }


                // Find Component Template
                if (_jsonComponentTemplateId == 0)
                {
                    ItemTypeCriteria componentTemplates = new ItemTypeCriteria(32);
                    ItemTitleCriteria componentTemplateTitle = new ItemTitleCriteria(Constants.ComponentTemplateName);
                    Query q = new Query(CriteriaFactory.And(new Criteria[] { componentTemplates, componentTemplateTitle }));
                    string[] result = q.ExecuteQuery();
                    if (!result.Any())
                    {
                        // oh oh
                        throw new Exception("Could not find a Component Template with name \"" + Constants.ComponentTemplateName + "\".");
                    }
                    TcmUri templateUri = new TcmUri(result[0]);
                    _jsonComponentTemplateId = templateUri.ItemId;
                }


                ItemTypeCriteria components = new ItemTypeCriteria(16);
                Query query = new Query(CriteriaFactory.And(new Criteria[] { components }));
                string[] results = query.ExecuteQuery();
                ComponentPresentationFactory factory = new ComponentPresentationFactory(_publicationId);
                List<ComponentPresentation> componentPresentations = new List<ComponentPresentation>();
                foreach (string result in results)
                {
                    ComponentPresentation cp = factory.GetComponentPresentation(new TcmUri(result).ItemId,
                        _jsonComponentTemplateId);
                    componentPresentations.Add(cp);
                }
                return componentPresentations;
            }
        }

        public static List<Article> GetAllArticles()
        {
            List<Article> result = new List<Article>();
            if (Cache.Get(Constants.ArticleCacheKey) != null)
                return (List<Article>)Cache.Get(Constants.ArticleCacheKey);
            foreach (ComponentPresentation cp in ComponentPresentations)
            {
                string data = cp.GetContent(false);
                Article a = JsonConvert.DeserializeObject<Article>(data);
                result.Add(a);
            }
            Cache.Add(Constants.ArticleCacheKey, result, Policy);
            return result;
        }

        public static List<Author> GetAllAuthors()
        {
            List<string> ids = new List<string>();
            List<Author> result = new List<Author>();
            if (Cache.Get(Constants.AuthorCacheKey) != null)
                return (List<Author>)Cache.Get(Constants.AuthorCacheKey);
            foreach (Article a in GetAllArticles())
            {
                Author b = a.Author;
                if (ids.Contains(b.Id)) continue;
                ids.Add(b.Id);
                result.Add(b);
            }
            Cache.Add(Constants.AuthorCacheKey, result, Policy);
            return result;
        }

        public static List<InformationSource> GetAllInformationSources()
        {
            List<string> ids = new List<string>();
            List<InformationSource> result = new List<InformationSource>();
            if (Cache.Get(Constants.SourceCacheKey) != null)
                return (List<InformationSource>)Cache.Get(Constants.SourceCacheKey);
            foreach (Article a in GetAllArticles())
            {
                InformationSource b = a.InformationSource;
                if (ids.Contains(b.Id)) continue;
                ids.Add(b.Id);
                result.Add(b);
            }
            Cache.Add(Constants.SourceCacheKey, result, Policy);
            return result;
        }
    }
}