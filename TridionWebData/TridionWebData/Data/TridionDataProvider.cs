using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Runtime.Caching;
using Newtonsoft.Json;
using TridionWebData.Models;
using TridionWebData.Tridion;

namespace TridionWebData.Data
{
    public static class TridionDataProvider
    {
        private static readonly ContentDeliveryService CDS;
        private static readonly MemoryCache Cache;
        private static readonly CacheItemPolicy Policy;
        private const string ArticleCacheKey = "ListArticles";
        private const string AuthorCacheKey = "ListAuthors";
        private const string SourceCacheKey = "ListSources";
        private const string CacheName = "TridionData";
        private static List<ComponentPresentation> _componentPresentations = new List<ComponentPresentation>();

        static TridionDataProvider()
        {
            Cache = new MemoryCache(CacheName);
            CDS = new ContentDeliveryService(new Uri(ConfigurationManager.AppSettings["TridionOdataServiceUrl"]));
            Policy = new CacheItemPolicy { SlidingExpiration = new TimeSpan(0, 0, 30, 0) };
        }

        private static IEnumerable<ComponentPresentation> ComponentPresentations
        {
            get
            {
                if (_componentPresentations.Count > 0)
                    return _componentPresentations;
                _componentPresentations = CDS.ComponentPresentations.ToList();
                return _componentPresentations;
            }
        }

        public static List<Article> GetAllArticles()
        {
            List<Article> result = new List<Article>();
            if (Cache.Get(ArticleCacheKey) != null)
                return (List<Article>)Cache.Get(ArticleCacheKey);
            foreach (ComponentPresentation cp in ComponentPresentations)
            {
                string data = cp.PresentationContent;
                Article a = JsonConvert.DeserializeObject<Article>(data);
                result.Add(a);
            }
            Cache.Add(ArticleCacheKey, result, Policy);
            return result;
        }

        public static List<Author> GetAllAuthors()
        {
            List<string> ids = new List<string>();
            List<Author> result = new List<Author>();
            if (Cache.Get(AuthorCacheKey) != null)
                return (List<Author>)Cache.Get(AuthorCacheKey);
            foreach (Article a in GetAllArticles())
            {
                Author b = a.Author;
                if (ids.Contains(b.Id)) continue;
                ids.Add(b.Id);
                result.Add(b);
            }
            Cache.Add(AuthorCacheKey, result, Policy);
            return result;
        }

        public static List<InformationSource> GetAllInformationSources()
        {
            List<string> ids = new List<string>();
            List<InformationSource> result = new List<InformationSource>();
            if (Cache.Get(SourceCacheKey) != null)
                return (List<InformationSource>)Cache.Get(SourceCacheKey);
            foreach (Article a in GetAllArticles())
            {
                InformationSource b = a.InformationSource;
                if (ids.Contains(b.Id)) continue;
                ids.Add(b.Id);
                result.Add(b);
            }
            Cache.Add(SourceCacheKey, result, Policy);
            return result;
        }
    }

}