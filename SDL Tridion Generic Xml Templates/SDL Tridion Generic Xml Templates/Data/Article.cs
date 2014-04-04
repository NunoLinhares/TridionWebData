using System;
using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;

namespace Tridion.Templates.Generic.Xml.Data
{
    public class Article : TridionComponent
    {

        internal Article(Component source, Engine engine)
        {
            SourceComponent = source;
            Data = engine;
            InitializeContent();
        }

        public string Id { get { return SourceComponent.Id.ToString().Replace(":", "-"); } }

        public string PageUrl
        {
            get
            {
                UsingItemsFilter filter = new UsingItemsFilter(SourceComponent.Session)
                    {
                        ItemTypes = new[] {ItemType.Page}
                    };
                List<IdentifiableObject> items = SourceComponent.GetUsingItems(filter).ToList();
                if (items.Count > 0)
                {
                    Page page = (Page) items[0];
                    return page.PublishLocationUrl;
                }
                return "";
            }
            set { SetFieldValue(value); }
        }

        public string ArticleTitle { get { return GetTextFieldValue("ArticleTitle"); } set { SetFieldValue(value); } }

        public string ArticleSummary
        {
            get { return GetTextFieldValue("ArticleSummary").TruncateHtml(200, " [...]"); }
            set { SetFieldValue(value); }
        }
        public string ArticleBody { get { return GetTextFieldValue("ArticleBody"); } set { SetFieldValue(value); } }
        public string ArticleUrl { get { return GetUrlFieldValue("ArticleUrl"); } set { SetFieldValue(value); } }
        public DateTime Date { get { return GetDateTimeFieldValue("Date"); } set { SetFieldValue(value.ToShortDateString()); } }
        public Author Author
        {
            get
            {
                InitializeContent();
                return new Author(((ComponentLinkField)Content["Author"]).Value, Data);
            }
            set { SetFieldValue(value.ToString()); }
        }
        public List<string> ContentCategories
        {
            get { return GetKeywordFieldValues("ContentCategory"); }
            set { SetFieldValue(value.ToString()); }
        }
        public InformationSource InformationSource
        {
            get
            {
                InitializeContent();
                return new InformationSource(((ComponentLinkField)Content["ArticleSource"]).Value, Data);
            }
            set { SetFieldValue(value.ToString()); }
        }

        // There's a constraint that Articles must be based on schema Constants.ArticleSchema, and the 
        // Namespace must be http://www.stridionworld.com/Content/Article


    }
}
