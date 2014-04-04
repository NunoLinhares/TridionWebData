using Tridion.ContentManager.Templating;

namespace Tridion.Templates.Generic.Xml.Data
{
    public abstract class TridionItem
    {
        internal Engine Data;

        public string Title { get; set; }
        public int TcmId { get; set; }
        public int PublicationId { get; set; }
    }
}
