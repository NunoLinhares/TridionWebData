using Newtonsoft.Json;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using Tridion.Templates.Generic.Xml.Data;

namespace Tridion.Templates.Generic.Xml
{
    public class BsonPage : ITemplate 
    {
        public void Transform(Engine engine, Package package)
        {
            Page page = (Page) engine.GetObject(package.GetByName(Package.PageName));
            TridionPage tridionPage = new TridionPage {Data = engine};
            tridionPage.InitializeContent(page);

            //string content = "Pages" + Environment.NewLine;
            //string content = tridionPage.ToBsonDocument().ToJson(new JsonWriterSettings {OutputMode = JsonOutputMode.Strict});
            string content = JsonConvert.SerializeObject(tridionPage);

            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Text, content));
        }
    }
}
