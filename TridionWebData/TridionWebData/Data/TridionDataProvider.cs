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
    /*
     * To be refactored to use Tridion Broker API instead, and native Broker cache.
     * Should avoid double caching... If I get it to work :)
     * Don't really like the IQueryable requirement to have to load all data in one go... will be fine for small datasets
     * But definitely doubleplusungood for large datasets.
     * 
     * Given Tridion native cache is in Java, I don't actually know if there will be a large performance penalty in just loading the items
     * from cache, given we'd be loadinga lot of them. I guess I can keep the .NET cache and reduce the sliding expiration to 5 minutes instead of 30.
     */
    public static class TridionDataProvider
    {
        private static readonly ContentDeliveryService CDS;
        private static readonly MemoryCache Cache;
        private static readonly CacheItemPolicy Policy;
        private static List<ComponentPresentation> _componentPresentations = new List<ComponentPresentation>();


        static TridionDataProvider()
        {
            Cache = new MemoryCache(Constants.CacheName);
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
            if (Cache.Get(Constants.ArticleCacheKey) != null)
                return (List<Article>)Cache.Get(Constants.ArticleCacheKey);
            foreach (ComponentPresentation cp in ComponentPresentations)
            {
                string data = cp.PresentationContent;
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