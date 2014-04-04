using Tridion.ContentManager.CoreService.Client;

namespace ImportContentFromRss.Content
{
    public class Source : ContentItem
    {
        public Source(ComponentData content, SessionAwareCoreServiceClient client)
            : base(content, client)
        {

        }

        public Source(SessionAwareCoreServiceClient client)
            : base(client)
        {
            Content = (ComponentData)Client.GetDefaultData(ItemType.Component, Constants.SourceLocationUrl, ReadOptions);
            Content.Schema = new LinkToSchemaData { IdRef = ContentManager.ResolveUrl(Constants.SourceSchemaUrl) };
        }

        public string WebsiteUrl
        {
            get { return Fields["WebsiteUrl"].Value; }
            set { Fields["WebsiteUrl"].Value = value; }
        }

        public string RssFeedUrl
        {
            get { return Fields["RssFeedUrl"].Value; }
            set { Fields["RssFeedUrl"].Value = value; }
        }

        public Organization Organization
        {
            get
            {
                var organizationId = Fields["Organization"].Value;
                return new Organization((ComponentData)Client.Read(organizationId, ReadOptions), Client);
            }
            set { Fields["Organization"].Value = value.Id; }
        }

        public Person DefaultAuthor
        {
            get
            {
                var defaultAuthor = Fields["DefaultAuthor"];
                if (defaultAuthor.Values.Count > 0)
                {
                    return new Person((ComponentData)Client.Read(defaultAuthor.Value, ReadOptions), Client);
                }
                return null;
            }
            set { Fields["DefaultAuthor"].Value = value.Id; }
        }

        public bool IsStackOverflow
        {
            get { return Title.Equals("Stack Overflow"); }
        }
    }
}
