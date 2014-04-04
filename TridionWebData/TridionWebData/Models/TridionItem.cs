using System;

namespace TridionWebData.Models
{
    public abstract class TridionItem
    {
        //public int SchemaId { get; set; }
        //public bool Multimedia { get; set; }
        //public int MinorVersion { get; set; }
        public int MajorVersion { get; set; }
        //public int OwningPublication { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime ModificationDate { get; set; }
        //public int TemplateId { get; set; }
        //public string OrganizationalItemTitle { get; set; }
        //public int OrganizationalItemId { get; set; }
        public DateTime PublicationDate { get; set; }
        //public DateTime TemplateModifiedDate { get; set; }
        public string Title { get; set; }
        //public int TcmId { get; set; }
        //public int PublicationId { get; set; }
    }
}