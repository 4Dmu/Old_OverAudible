using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models
{
    public class AudiblePage
    {
        public Page page { get; set; }
    }

    public class Page
    {
        public string id { get; set; }
        public object pagination_token { get; set; }
        public Section[] sections { get; set; }
    }

    public class Section
    {
        public string id { get; set; }
        public Model model { get; set; }
        public View view { get; set; }
        public object pagination { get; set; }
    }

    public class Model
    {
        public string[] flags { get; set; }
        public string[] headers { get; set; }
        public bool isPersonalized { get; set; }
        public string[] pLinks { get; set; }
        public string[] pageLoadIds { get; set; }
        public Item[] products { get; set; }
        public string[] refTags { get; set; }
        public string copy { get; set; }
        public Image[] images { get; set; }
        public Link[] links { get; set; }
    }

    public class Image
    {
        public string url { get; set; }
    }

    public class Link
    {
        public string __type { get; set; }
        public string label { get; set; }
        public string type { get; set; }
        public string url { get; set; }
        public string description { get; set; }
    }

    public class View
    {
        public string templates { get; set; }
        public string location { get; set; }
    }
}
