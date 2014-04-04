using Tridion.ContentManager;
using Tridion.ContentManager.CommunicationManagement;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;
using Tridion.ContentManager.Templating.Assembly;
using ComponentPresentation = Tridion.ContentManager.CommunicationManagement.ComponentPresentation;

namespace Tridion.Templates.Generic.Xml
{
    [TcmTemplateTitle("Get Item Xml")]
    public class GetItemXml : ITemplate
    {
        private Engine _engine;
        private Package _package;
        private TemplatingLogger _log;

        public void Transform(Engine engine, Package package)
        {
            _engine = engine;
            _package = package;
            _log = TemplatingLogger.GetLogger(GetType());
            TemplateType templateType;

            if (package.GetByName(Package.ComponentName) != null)
                templateType = TemplateType.Component;
            else
                templateType = TemplateType.Page;

            Item output = null;
            if (templateType == TemplateType.Page)
            {
                Page page = (Page)engine.GetObject(package.GetByName(Package.PageName));
                page.Load(LoadFlags.KeywordXlinks);
                output = package.CreateXmlDocumentItem(ContentType.Xml, page.ToXml(XmlFormat.R6Native, XmlSections.All).OwnerDocument);

                foreach (ComponentPresentation cp in page.ComponentPresentations)
                {
                    engine.RenderComponentPresentation(cp.Component.Id, cp.ComponentTemplate.Id);
                }
            }
            if (templateType == TemplateType.Component)
            {
                Component component = (Component)engine.GetObject(package.GetByName(Package.ComponentName));
                component.Load(LoadFlags.KeywordXlinks);
                output = package.CreateXmlDocumentItem(ContentType.Xml, component.ToXml(XmlFormat.R6Native, XmlSections.All).OwnerDocument);
                if(component.BinaryContent != null)
                {
                    package.PushItem(package.CreateMultimediaItem(component));
                }
            }
            if (output != null)
                package.PushItem(Package.OutputName, output);

        }
    }
}
