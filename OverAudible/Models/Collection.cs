using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public class Collection
    {
        public string Image1 { get; set; } = string.Empty;
        public string Image2 { get; set; } = string.Empty;
        public string Image3 { get; set; } = string.Empty;
        public string Image4 { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string CollectionId { get; set; } = string.Empty;
        public string CreationDate { get; set; } = string.Empty;
        public string Count { get; set; } = string.Empty;
        public string StateToken { get; set; } = string.Empty;
        public List<string> BookAsins { get; set; } = new List<string>();

    }
}
