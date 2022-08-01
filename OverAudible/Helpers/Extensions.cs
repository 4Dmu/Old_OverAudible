using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudibleApi;
using OverAudible.API;
using OverAudible.Models;

namespace OverAudible.Helpers
{
    public static class Extensions
    {
        public static async Task<Item> GetSelfFromCatalog(this Item @this, CatalogOptions.ResponseGroupOptions options)
        {
            var api = await ApiClient.GetInstance();

            return await api.GetCatalogItemAsync(@this.Asin, options);
        }

        public static async Task<Item> GetSelfFromLibrary(this Item @this, LibraryOptions.ResponseGroupOptions options)
        {
            var api = await ApiClient.GetInstance();

            return await api.GetLibraryItemAsync(@this.Asin, options);
        }

    }
}
