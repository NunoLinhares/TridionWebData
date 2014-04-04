using Newtonsoft.Json;
using System.Xml;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;

namespace Tridion.Templates.Generic.Xml
{
    public class BsonFromXml : ITemplate 
    {
        public void Transform(Engine engine, Package package)
        {
            Component c = (Component)engine.GetObject(package.GetByName(Package.ComponentName));

            XmlElement xdoc = c.Content;

            package.PushItem(Package.OutputName, package.CreateStringItem(ContentType.Text, JsonConvert.SerializeXmlNode(xdoc)));


        }
    }
}
