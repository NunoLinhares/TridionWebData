using System;
using System.Collections.Generic;
using System.Linq;
using Tridion.ContentManager.ContentManagement;
using Tridion.ContentManager.ContentManagement.Fields;
using Tridion.ContentManager.Templating;

namespace Tridion.Templates.Generic.Xml.Data
{
    public abstract class TridionComponent : PublishableItem
    {
        public int SchemaId { get; set; }
        public bool? Multimedia { get; set; }
        internal Component SourceComponent { get; set; }
        internal ItemFields Content { get; set; }

        internal void InitializeContent()
        {
            if (Content != null) return;
            Content = new ItemFields(SourceComponent.Content, SourceComponent.Schema);
            TcmId = SourceComponent.Id.ItemId;
            Title = SourceComponent.Title;
            MajorVersion = SourceComponent.Version;
            MinorVersion = SourceComponent.Revision;
            OwningPublication = SourceComponent.OwningRepository.Id.ItemId;
            SchemaId = SourceComponent.Schema.Id.ItemId;
            CreationDate = SourceComponent.CreationDate;
            ModificationDate = SourceComponent.RevisionDate;
            Multimedia = SourceComponent.Schema.Purpose == SchemaPurpose.Multimedia;
            PublicationId = SourceComponent.ContextRepository.Id.ItemId;
            TemplateId = Data.PublishingContext.ResolvedItem.Template.Id.ItemId;
            OrganizationalItemId = SourceComponent.OrganizationalItem.Id.ItemId;
            OrganizationalItemTitle = SourceComponent.OrganizationalItem.Title;
            PublicationDate = DateTime.Now;
            TemplateModifiedDate = Data.PublishingContext.ResolvedItem.Template.RevisionDate;

            //Implement others as needed
        }


        internal string SetFieldValue(string value)
        {
            return "Fields are read only.";
        }

        internal string GetUrlFieldValue(string fieldName)
        {
            InitializeContent();
            return ((ExternalLinkField) Content[fieldName]).Value;
        }

        internal List<string> GetTextFieldValues(string fieldName)
        {
            InitializeContent();
            SingleLineTextField field = (SingleLineTextField)Content[fieldName];
            return field.Values.ToList();
            
        }

        internal List<string> GetKeywordFieldValues(string fieldName)
        {
            KeywordField field = (KeywordField) Content[fieldName];
            return field.Values.Select(key => key.Title).ToList();
        }

        internal string GetTextFieldValue(string fieldName)
        {
            InitializeContent();

            ItemField field = Content[fieldName];
            if (field is XhtmlField)
            {
                XhtmlField xField = (XhtmlField) field;
                return TemplateUtilities.ResolveRichTextFieldXhtml(xField.Value);
            }
            return ((TextField) field).Value;
        }

        internal DateTime GetDateTimeFieldValue(string fieldName)
        {
            InitializeContent();
            return ((DateField) Content[fieldName]).Value;

        }


    }
}
