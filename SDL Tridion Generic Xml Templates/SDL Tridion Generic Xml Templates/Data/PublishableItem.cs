using System;

namespace Tridion.Templates.Generic.Xml.Data
{
    public abstract class PublishableItem : TridionItem
    {
        public int MinorVersion { get; set; }
        public int? MajorVersion { get; set; }
        public int? OwningPublication { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public long TemplateId { get; set; }
        public string OrganizationalItemTitle { get; set; }
        public int OrganizationalItemId { get; set; }
        public DateTime PublicationDate { get;set; }
        // Needed for Experience Manager
        public DateTime TemplateModifiedDate { get; set; }
        
    }
}
