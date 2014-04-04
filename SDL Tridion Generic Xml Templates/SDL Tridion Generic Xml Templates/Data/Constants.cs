using System;
using System.Xml.Linq;

namespace Tridion.Templates.Generic.Xml.Data
{
    public static class Constants
    {
        public static readonly Uri ContentServiceEndpointUri = new Uri("http://localhost:88/odata.svc");
        public const string MongoConnectionString = "mongodb://localhost";
        public const string MongoSyncLogDatabase = "SyncLog";
        public const string MongoSyncLogCollection = "SyncLog";
        public const string MongoDatabase = "TridionArticles";
        public const string MongoArticleCollection = "Articles";


        public const int XmlTemplateId = 1999;
        public const int SchemaArticleId = 62;
        public const int SchemaAuthorId = 60;
        public const int SchemaInformationSourceId = 61;

        public static readonly XNamespace TcmR6Namespace = XNamespace.Get("http://www.sdltridion.com/ContentManager/R6");
        public static readonly XNamespace ArticleNamespace = XNamespace.Get("http://www.sdltridionworld.com/Content/Article");
        public static readonly XNamespace XLinkNamespace = XNamespace.Get("http://www.w3.org/1999/xlink");
        public static readonly XNamespace PersonNamespace = XNamespace.Get("http://www.sdltridionworld.com/Content/Person");

        public static readonly XNamespace InformationSourceNamespace =
            XNamespace.Get("http://www.sdltridionworld.com/Content/Source");

    }
}
