using System.Collections.Generic;
using System.Data.Services.Common;
using TridionWebData.Data;

namespace TridionWebData.Models
{
    [DataServiceKey("Id")]
    public class InformationSource : TridionItem
    {
        public string Id { get; set; }
        public string WebsiteUrl { get; set; }
        public string RssFeedUrl { get; set; }

        public static List<InformationSource> GetInformationSources()
        {
            return TridionDataProvider.GetAllInformationSources();
        }
    }
}