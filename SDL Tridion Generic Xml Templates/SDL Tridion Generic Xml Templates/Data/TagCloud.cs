using System.Collections.Generic;

namespace Tridion.Templates.Generic.Xml.Data
{
    public class TagCloud
    {
        public string Id;
        public List<Tag> Tags;
        public int PublicationId { get; set; }
        public int TcmId { get; set; }
        public string PageTitle { get;set; }
    }
}
