using System;
using System.Collections.Generic;
using System.Data.Services.Common;
using Newtonsoft.Json;
using TridionWebData.Data;

namespace TridionWebData.Models
{
    [DataServiceKey("Id")]
    public class Article : TridionItem
    {
        public string Id { get; set; }
        //public string PageUrl { get; set; }
        public string ArticleTitle { get; set; }
        public string ArticleSummary { get; set; }
        public string ArticleBody { get; set; }
        public string ArticleUrl { get; set; }
        [JsonProperty("Date")]
        public DateTime ArticleDate { get; set; }
        public Author Author { get; set; }
        public List<string> ContentCategories { get; set; }
        public InformationSource InformationSource { get; set; }

        public static List<Article> GetArticles()
        {
            return TridionDataProvider.GetAllArticles();
        }
    }
}