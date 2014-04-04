using System;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;

namespace Tridion.Templates.Generic.Xml.Data
{
    public class InformationSource : TridionComponent
    {
        private readonly int _publicationId;

        public InformationSource(Component component, Engine engine)
        {
            Data = engine;
            SourceComponent = component;
            if (SourceComponent.Schema.Title != "InformationSource")
            {
                throw new Exception(string.Format("Specified component ID \"{0}\" is not an InformationSource!", SourceComponent.Id));
            }
            InitializeContent();
        }

        public string Id { get { return SourceComponent.Id.ToString().Replace(":", "-"); } }

        public string WebsiteUrl
        {
            get { return GetTextFieldValue("WebsiteUrl"); }
            set { SetFieldValue(value); }
        }
        public string RssFeedUrl
        {
            get { return GetUrlFieldValue("RssFeedUrl"); }
            set { SetFieldValue(value); }
        }

    }
}
