using AudibleApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OverAudible.Exceptions;
using System.Threading.Tasks;
using Dinah.Core.Net.Http;
using Newtonsoft.Json.Linq;
using OverAudible.Models;
using Newtonsoft.Json;
using AutoMapper;
using OverAudible.Services;
using Microsoft.Extensions.DependencyInjection;
using ShellUI.Controls;

namespace OverAudible.API
{
    public class ApiClient
    {
        public Api Api { get; }

        private static Task<ApiClient> _instance;

        public const string defaultCollectionURL = "https://www.colorado.edu/brand/sites/default/files/styles/large_square_thumbnail/public/callout/color-dark-gray.png?itok=qXourhch";

        public ApiClient(Api api)
        {
            Api = api;
        }

        public static Task<ApiClient> GetInstance(string? email = null, string? pass = null)
        {
            if (_instance is null || _instance.Exception is not null || _instance.IsFaulted is true)
            {
                if (email == null || pass == null)
                    throw new ApiClientNullArgumentsException("Email and or pass was null");
                _instance = UserSetup.Run(email, pass);
            }

            return _instance;
        }

        public async Task<JObject> GetHomePage()
        {
            string url = "/1.0/pages/ios-app-home?response_groups="
                           + "media,product_plans,view,product_attrs,contributors,product_desc,sample,in_wishlist,rating"
                           + "&image_dpi=489"
                           + "&local_tim=2022-01-01T12:00:00+01:00"
                           + "&local=en-US"
                           + "&os=15.2"
                           + "&session_id=123-1234567-1234567"
                           + "&surface=iOS";

            var responseMsg = await Api.AdHocAuthenticatedGetAsync(url);
            var jobj = await responseMsg.Content.ReadAsJObjectAsync();
            return jobj;
        }

        public async Task<Item> GetCatalogItemAsync(string asin, CatalogOptions.ResponseGroupOptions responseGroups)
        {
            var i = await Api.GetCatalogProductAsync(asin, responseGroups);

            return Shell.DependencyService.ServiceProvider.GetRequiredService<IMapper>().Map<Item>(i);
        }

        public async Task<Item> GetLibraryItemAsync(string asin, LibraryOptions.ResponseGroupOptions responseGroupOptions)
        {
            var i = await Api.GetLibraryBookAsync(asin, responseGroupOptions);

            return Shell.DependencyService.ServiceProvider.GetRequiredService<IMapper>().Map<Item>(i);
        }

        public async Task<List<Item>> GetLibraryAsync()
        {
            LibraryOptions options = new();
            options.ResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;
            var library = await Api.getAllLibraryItemsAsync(options);

            return Shell.DependencyService.ServiceProvider.GetRequiredService<IMapper>().Map<List<Item>>(library);
        }

        public async Task<List<Item>> GetLibraryPartsAsync(int pagNumber)
        {
            LibraryOptions options = new();
            options.NumberOfResultPerPage = 25;
            options.PageNumber = pagNumber;
            options.ResponseGroups = LibraryOptions.ResponseGroupOptions.ALL_OPTIONS;
            var library = await Api.getAllLibraryItemsAsync(options);

            return Shell.DependencyService.ServiceProvider.GetRequiredService<IMapper>().Map<List<Item>>(library);
        }

        public async Task<int> GetLibraryTotal()
        {
            LibraryOptions options = new();
            options.ResponseGroups = LibraryOptions.ResponseGroupOptions.None;
            var c = await Api.getAllLibraryJObjectsAsync(options);

            return c.Count;
        }
        
        public async Task<List<Item>> GetWishlistAsync()
        {
            List<Item> wishlist = new List<Item>();

            int page = -1;
            int itemPerPage = 50;

            while (true)
            {
                try
                {
                    page++;
                    string url = "/1.0/wishlist?sort_by=-DateAdded"
                    + $"&num_results={itemPerPage}" + $"&page={page}"
                    + "&" + CatalogOptions.ResponseGroupOptions.ALL_OPTIONS.ToQueryString();
                    var result = await Api.AdHocAuthenticatedGetAsync(url);
                    var obj = await result.Content.ReadAsJObjectAsync();
                    AudibleApi.Common.ProductsDtoV10 dto = AudibleApi.Common.ProductsDtoV10.FromJson(obj.ToString());

                    if (dto.Products == null || dto.Products.Length == 0)
                        break;

                    wishlist.AddRange(Shell.DependencyService.ServiceProvider.GetRequiredService<IMapper>().Map<List<Item>>(dto.Products.ToList()));
                }
                catch (Exception ex)
                {
                    break;
                }
            }
            return wishlist;
        }

        public async Task<List<Collection>> GetCollectionsWithItemsAsync()
        {
            var url = "/1.0/collections";
            var d = await Api.AdHocAuthenticatedGetAsync(url);
            var s = await d.Content.ReadAsJObjectAsync();
            List<Collection> collections = new();
            foreach (var i in s["collections"])
            {
                Collection c = new Collection();
                c.Title = i["name"].ToString();
                c.CreationDate = i["creation_date"].ToString();
                c.CollectionId = i["collection_id"].ToString();
                c.Description = i["description"].ToString();
                c.Count = i["total_count"].ToString();
                c.StateToken = i["state_token"].ToString();
                c.Title = i["name"].ToString();
                c.Image1 =
                    defaultCollectionURL;
                c.Image2 =
                    defaultCollectionURL;
                c.Image3 =
                    defaultCollectionURL;
                c.Image4 =
                    defaultCollectionURL;
                collections.Add(c);
            }
            foreach (var c in collections)
            {
                var url2 =
                    "/1.0/collections/" + c.CollectionId + "/items?response_groups=always-returned";
                var d2 = await Api.AdHocAuthenticatedGetAsync(url2);
                var i2 = await d2.Content.ReadAsJObjectAsync();
                foreach (var item in i2["items"])
                {
                    c.BookAsins.Add(item["asin"].ToString());
                }
            }
            return collections;
        }

        public async Task<string> DeleteCollectionAsync(string collectionId)
        {
            var url = "/1.0/collections/" + collectionId;
            var d = await Api.AdHocAuthenticatedDeleteAsync(url);
            var result = await d.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<string> CreateCollectionAsync( string name,string description, List<string> items)
        {
            var asins = "";
            foreach (var item in items)
            {
                asins += "\"" + item + "\",";
            }
            if (asins.Length > 0)
                asins = asins.Substring(0, asins.Length - 1);
            var url = "/1.0/collections";
            var json =
                "{\"name\": \""
                + name
                + "\", \"asins\":["
                + asins
                + "],"
                + "\"description\":"
                + "\""
                + description
                + "\" }";
            var d = await Api.AdHocAuthenticatedPostAsync(url, json);
            
            var result = await d.Content.ReadAsStringAsync();
            return result;
        }

        public async Task<JObject> CreateCollectionAsync(string name, string description, List<string> items, bool returnJObject)
        {
            var asins = "";
            foreach (var item in items)
            {
                asins += "\"" + item + "\",";
            }
            if (asins.Length > 0)
                asins = asins.Substring(0, asins.Length - 1);
            var url = "/1.0/collections";
            var json =
                "{\"name\": \""
                + name
                + "\", \"asins\":["
                + asins
                + "],"
                + "\"description\":"
                + "\""
                + description
                + "\" }";
            var d = await Api.AdHocAuthenticatedPostAsync(url, json);

            var result = await d.Content.ReadAsJObjectAsync();
            return result;
        }

        public async Task<List<Item>> GetCatalogItemsAsync(int numResults, int pageNumber, Categorie categorie, List<string> keywordList, CatalogOptions.ResponseGroupOptions responseGroups, CatalogOptions.SortByOptions sortOptions)
        {
            int actualNumResults = numResults.Clamp(1, 50);
            string categoryId = ((long)categorie).ToString();
            string keywords = String.Join(',', keywordList);
            string actualResponseGroups = responseGroups.ToQueryString();


            string url = "/1.0/catalog/products";
            url =
                url
                + $"?num_results={actualNumResults}"
                + (categoryId != "0" ? $"&category_id={categoryId}" : String.Empty)
                + (keywords.Count() > 0 ? $"&keywords={keywords}" : String.Empty)
                + $"&page={pageNumber}"
                + $"&sort_by={ModelExtensions.GetDescription(sortOptions)}"
                + "&" + actualResponseGroups;
            url += url.Contains("?") ? "&" : "?";
            var responseMsg = await Api.AdHocAuthenticatedGetAsync(url);
            var jObj = await responseMsg.Content.ReadAsJObjectAsync();

            var dto = AudibleApi.Common.ProductsDtoV10.FromJson(jObj.ToString());

            var l = Shell.DependencyService.ServiceProvider.GetRequiredService<IMapper>().Map<List<Item>>(dto.Products.ToList());

            return l;
        }
        
        public async Task AddItemToCollection(Item item, Collection collection)
        {
            string url = $"/1.0/collections/{collection.CollectionId}/items";
            
        }

        public async Task<string> AddItemsToCollectionAsync(string collectionId, List<string> asins)
        {
            var asinsJson = "";
            foreach (var item in asins)
            {
                asinsJson += "\"" + item + "\",";
            }
            if (asinsJson.Length > 0)
                asinsJson = asinsJson.Substring(0, asinsJson.Length - 1);
            var url = "/1.0/collections/" + collectionId + "/items";
            var json =
                "{ \"collection_id\":"
                + "\""
                + collectionId
                + "\","
                + "\"asins\": ["
                + asinsJson
                + "]}";
            var d = await Api.AdHocAuthenticatedPostAsync(url, json);
            var result = await d.Content.ReadAsStringAsync();
            return result;
        }
    }
}
