using System;
using System.Globalization;
using Newtonsoft.Json;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Templates.Generic.Xml.Data;

namespace Tridion.Templates.Generic.Xml
{
    public class GenerateBsonDocument : ITemplate 
    {
        public void Transform(Engine engine, Package package)
        {
            Component c = (Component) engine.GetObject(package.GetByName(Package.ComponentName));
            if (c.Schema.Title != "Article" || c.Schema.Title == "Label") return;
            Article article = new Article(c, engine);
            //string content = PluralizationService.CreateService(CultureInfo.CurrentCulture).Pluralize(c.Schema.Title) + Environment.NewLine;
            
            //string content = article.ToBsonDocument().ToJson(new JsonWriterSettings{OutputMode = JsonOutputMode.Strict});
            string content = JsonConvert.SerializeObject(article);
            
            
            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Text, content));

        }
    }
}
