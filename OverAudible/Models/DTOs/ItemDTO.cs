using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudibleApi;

namespace OverAudible.Models.DTOs
{
    public class ItemDTO
    {
        [Key]
        public string Asin { get; set; }

        public string Item { get; set; }

        public string ContentMetadataJson { get; set; }
        
    }

    public class NoMetaItemDTO
    {
        [Key]
        public string Asin { get; set; }

        public string Item { get; set; }
    }

    public class CatalogItemDTO
    {
        [Key]
        public string Asin { get; set; }

        public string Item { get; set; }
    }
}
