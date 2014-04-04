using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Tridion.Templates.Generic.Xml.Data
{
    public class GenerateAuthorCloud : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {
            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));
            TcmUri articleSchemaUri = new TcmUri(Constants.SchemaArticleId, ItemType.Schema, page.Id.PublicationId);
            UsingItemsFilter filter = new UsingItemsFilter(engine.GetSession()) { ItemTypes = new[] { ItemType.Component } };
            Schema schema = (Schema)engine.GetObject(articleSchemaUri);

            TagCloud tagCloud = new TagCloud
                {
                    Tags = new List<Tag>(),
                    PublicationId = page.Id.PublicationId,
                    TcmId = page.Id.ItemId,
                    PageTitle = page.Title
                };

            SortedList<string, int> tags = new SortedList<string, int>(StringComparer.CurrentCultureIgnoreCase);

            foreach (Component c in schema.GetUsingItems(filter))
            {
                Article a = new Article(c, engine);
                string tag = a.Author.Name;
                if (tags.ContainsKey(tag))
                {
                    tags[tag] = tags[tag] + 1;
                }
                else
                {
                    tags.Add(tag, 1);
                }
            }

            foreach (var tag in tags)
            {
                tagCloud.Tags.Add(new Tag { TagName = tag.Key.ToAscii(), TagValue = tag.Value });
            }


            string content = JsonConvert.SerializeObject(tagCloud);
            //content += tagCloud.ToBsonDocument().ToJson(new JsonWriterSettings { OutputMode = JsonOutputMode.Strict });
            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Text, content));
        }
    }
}
