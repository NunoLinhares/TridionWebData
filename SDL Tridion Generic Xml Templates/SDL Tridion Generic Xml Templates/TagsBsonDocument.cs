using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Templates.Generic.Xml.Data;


namespace Tridion.Templates.Generic.Xml
{
    [TcmTemplateTitle("Generate Tags")]
    public class TagsBsonDocument : ITemplate
    {
        public void Transform(Engine engine, Package package)
        {

            Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));
            TcmUri articleSchemaUri = new TcmUri(Data.Constants.SchemaArticleId, ItemType.Schema, page.Id.PublicationId);
            UsingItemsFilter filter = new UsingItemsFilter(engine.GetSession()) { ItemTypes = new[] { ItemType.Component } };
            Schema schema = (Schema)engine.GetObject(articleSchemaUri);


            TagCloud tagCloud = new TagCloud {Tags = new List<Tag>(), PublicationId = page.Id.PublicationId, TcmId = page.Id.ItemId, PageTitle = page.Title};

            SortedList<string, int> tags = new SortedList<string, int>(StringComparer.CurrentCultureIgnoreCase);

            foreach (Component c in schema.GetUsingItems(filter))
            {
                Article a = new Article(c, engine);
                foreach (string tag in a.ContentCategories)
                {
                    
                    if (tags.ContainsKey(tag))
                    {
                        tags[tag] = tags[tag] + 1;
                    }
                    else
                    {
                        tags.Add(tag, 1);
                    }
                }

            }

            foreach (var tag in tags)
            {
                tagCloud.Tags.Add(new Tag{TagName = tag.Key, TagValue = tag.Value});
            }

            //string content = "TagCloud" + Environment.NewLine;
            //content += tagCloud.ToBsonDocument().ToJson(new JsonWriterSettings{OutputMode = JsonOutputMode.Strict});
            string content = JsonConvert.SerializeObject(tagCloud);
            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Text, content));

        }
    }
}
