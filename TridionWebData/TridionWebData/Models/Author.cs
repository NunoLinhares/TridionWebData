using System.Collections.Generic;
using System.Data.Services.Common;
using TridionWebData.Data;

namespace TridionWebData.Models
{
    [DataServiceKey("Id")]
    public class Author : TridionItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EscapedName { get; set; }

        public static List<Author> GetAuthors()
        {
            return TridionDataProvider.GetAllAuthors();
        }
    }
}