using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.API
{
    public static class UserSetup
    {
        public const string LOCALE_NAME = "us";

        // will be created if it does not exist
        public static string IDENTITY_FILE_PATH = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + "OverAudibleIdentity.json";

        // if your json file is complex: you can specify the jsonPath within that file where identity is/should be stored.
        // else: null
        public const string JSON_PATH = null;

        public static async Task<ApiClient> Run(string email, string pass)
        {
            var client = await Bridge.CreateClientAsync(email, pass);
            return client;

            //await client.AccountInfoAsync();
            //await client.PrintLibraryAsync();
            //await client.DownloadLibraryToFileAsync();
            //AudibleApiClient.AnaylzeLibrary();

            //// get all books in library (page 1)
            //await client.PrintLibraryAsync();

            //// get book info from library
            //// book i own
            //await client.Api.GetLibraryBookAsync(AudibleApiClient.MEDIUM_BOOK_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS);
            //// throws: not present in customer library
            //try { await client.Api.GetLibraryBookAsync(AudibleApiClient.DO_NOT_OWN_ASIN, LibraryOptions.ResponseGroupOptions.ALL_OPTIONS); }
            //catch (Exception ex) { }

            //// get general book info. doesn't matter if in library. will return less info
            //await client.GetBookInfoAsync(AudibleApiClient.MEDIUM_BOOK_ASIN);
            //await client.GetBookInfoAsync(AudibleApiClient.DO_NOT_OWN_ASIN);

            //await client.PodcastTestsAsync();
        }
    }
}
