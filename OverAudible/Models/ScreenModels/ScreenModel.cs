using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Models.ScreenModels
{
    public class Screen
    {
        public string page_type { get; set; }
        public string page_id { get; set; }
        public Page_Info page_info { get; set; }
        public object page_details { get; set; }
        public Section[] sections { get; set; }
        public object pagination_token { get; set; }
    }

    public class Page_Info
    {
        public string layoutId { get; set; }
    }

    public class Section
    {
        public string id { get; set; }
        public Model model { get; set; }
        public View3 view { get; set; }
        public object pagination { get; set; }
    }

    public class Model
    {
        public Local_Data local_data { get; set; }
        public Item[] items { get; set; }
        public Section_Header section_header { get; set; }
        public Api_Data api_data { get; set; }
    }

    public class Local_Data
    {
        public string data_source { get; set; }
        public Additional_Data additional_data { get; set; }
    }

    public class Additional_Data
    {
        public string keywords { get; set; }
        public bool always_display { get; set; }
    }

    public class Section_Header
    {
        public View view { get; set; }
        public Model1 model { get; set; }
    }

    public class View
    {
        public string template { get; set; }
    }

    public class Model1
    {
        public Row[] rows { get; set; }
    }

    public class Row
    {
        public View1 view { get; set; }
        public Model2 model { get; set; }
    }

    public class View1
    {
        public string template { get; set; }
    }

    public class Model2
    {
        public Text text { get; set; }
        public Accessibility accessibility { get; set; }
        public Button[] buttons { get; set; }
        public bool is_selected { get; set; }
        public Action action { get; set; }
    }

    public class Text
    {
        public string value { get; set; }
    }

    public class Accessibility
    {
        public string identifier { get; set; }
        public string label { get; set; }
        public string hint { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public Payload payload { get; set; }
    }

    public class Payload
    {
        public string url { get; set; }
    }

    public class Button
    {
        public Text1 text { get; set; }
        public Accessibility1 accessibility { get; set; }
        public Style style { get; set; }
        public Action1 action { get; set; }
        public Image image { get; set; }
    }

    public class Text1
    {
        public string value { get; set; }
        public Color color { get; set; }
    }

    public class Color
    {
        public string type { get; set; }
        public string colorTag { get; set; }
    }

    public class Accessibility1
    {
        public string label { get; set; }
        public string identifier { get; set; }
        public string hint { get; set; }
    }

    public class Style
    {
        public string type { get; set; }
    }

    public class Action1
    {
        public string type { get; set; }
        public Payload1 payload { get; set; }
    }

    public class Payload1
    {
        public string url { get; set; }
    }

    public class Image
    {
        public string type { get; set; }
        public string name { get; set; }
    }

    public class Api_Data
    {
        public Logging_Data logging_data { get; set; }
        public Sort[] sort { get; set; }
        public Refinement[] refinements { get; set; }
        public Result_Count result_count { get; set; }
        public Promoted_Refinements[] promoted_refinements { get; set; }
        public bool spell_corrected { get; set; }
        public string lens_name { get; set; }
        public string module_name { get; set; }
    }

    public class Logging_Data
    {
        public string query_id { get; set; }
        public string sr_prefix { get; set; }
        public string engine_query { get; set; }
        public string rank_order { get; set; }
        public string search_index { get; set; }
        public string asis_request_id { get; set; }
    }

    public class Result_Count
    {
        public string range_low { get; set; }
        public string range_high { get; set; }
        public string total { get; set; }
        public string max_displayable_result { get; set; }
    }

    public class Sort
    {
        public string sort_option_id { get; set; }
        public string display_name { get; set; }
        public string short_name { get; set; }
        public bool selected { get; set; }
    }

    public class Refinement
    {
        public string bin_id { get; set; }
        public string display_name { get; set; }
        public Filter[] filters { get; set; }
        public Ancestor[] ancestors { get; set; }
        public string search_bin_type { get; set; }
    }

    public class Filter
    {
        public string id { get; set; }
        public string display_name { get; set; }
        public bool selected { get; set; }
    }

    public class Ancestor
    {
        public string id { get; set; }
        public string display_name { get; set; }
    }

    public class Promoted_Refinements
    {
        public string bin_id { get; set; }
        public string display_name { get; set; }
        public Filter1[] filters { get; set; }
        public object[] ancestors { get; set; }
        public string search_bin_type { get; set; }
    }

    public class Filter1
    {
        public string id { get; set; }
        public string display_name { get; set; }
        public bool selected { get; set; }
    }

    public class Item
    {
        public View2 view { get; set; }
        public Model3 model { get; set; }
    }

    public class View2
    {
        public string template { get; set; }
    }

    public class Model3
    {
        public Product_Metadata product_metadata { get; set; }
        public Accessibility3 accessibility { get; set; }
        public Tap_Action tap_action { get; set; }
        public string asin_row_accessory_combination { get; set; }
    }

    public class Product_Metadata
    {
        public string asin { get; set; }
        public Cover_Art cover_art { get; set; }
        public Title title { get; set; }
        public Author_Name author_name { get; set; }
        public Rating rating { get; set; }
        public bool is_progress_disabled { get; set; }
        public Tag[] tags { get; set; }
    }

    public class Cover_Art
    {
        public string url { get; set; }
        public Url_Map url_map { get; set; }
    }

    public class Url_Map
    {
        public string _500 { get; set; }
    }

    public class Title
    {
        public string value { get; set; }
    }

    public class Author_Name
    {
        public string value { get; set; }
    }

    public class Rating
    {
        public float value { get; set; }
        public int count { get; set; }
    }

    public class Tag
    {
        public string tag_type { get; set; }
        public string tag_link { get; set; }
        public string text { get; set; }
        public string short_text { get; set; }
        public Graphic graphic { get; set; }
        public Accessibility2 accessibility { get; set; }
    }

    public class Graphic
    {
        public string graphic_type { get; set; }
        public string icon_type { get; set; }
        public string image_url { get; set; }
    }

    public class Accessibility2
    {
        public string label { get; set; }
        public string hint { get; set; }
        public string image_alt_text { get; set; }
    }

    public class Accessibility3
    {
        public string identifier { get; set; }
        public string label { get; set; }
    }

    public class Tap_Action
    {
        public string type { get; set; }
        public Payload2 payload { get; set; }
    }

    public class Payload2
    {
        public string url { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Metadata
    {
        public string asin { get; set; }
        public string content_delivery_type { get; set; }
    }

    public class View3
    {
        public string templates { get; set; }
        public string location { get; set; }
    }

}
