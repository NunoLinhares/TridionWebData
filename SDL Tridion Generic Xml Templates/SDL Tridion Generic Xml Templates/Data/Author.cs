using System;
using System.Security;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.Templating;

namespace Tridion.Templates.Generic.Xml.Data
{
    public class Author : TridionComponent
    {

        public Author(Component component, Engine engine)
        {
            Data = engine;
            SourceComponent = component;
            if (SourceComponent.Schema.Title != "Person")
            {
                throw new Exception(string.Format("Specified component ID \"{0}\" is not an Author!", component.Id));
            }
            InitializeContent();
        }

        public string Id { get { return SourceComponent.Id.ToString().Replace(":", "-"); } }

        public string Name
        {
            get { return GetTextFieldValue("PersonName"); }
            set { SetFieldValue(value); }
        }

        public string EscapedName
        {
            get { return Name.ToAscii(); }
            set { SetFieldValue(value); }
        }

    }
}
