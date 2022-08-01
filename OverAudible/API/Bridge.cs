using AudibleApi;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.API
{
    public record Secrets(string email, string password, string jsonPath, int accountIndex);

    public static class Bridge
    {
        public static Secrets GetSecrets()
        {
            // store somewhere that can't accidentally be added to git
            var secretsPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "SECRET.txt"
            );
            if (!File.Exists(secretsPath))
                return null;

            try
            {
                var pwParts = File.ReadAllLines(secretsPath);

                var acctIndex = pwParts.Length <= 3 ? 0 : int.Parse(pwParts[3]) - 1; // change 1-based to 0-based index

                return new Secrets(pwParts[0], pwParts[1], pwParts[2], acctIndex);
            }
            catch
            {
                return null;
            }
        }

        public async static Task<ApiClient> CreateClientAsync(string email, string pass)
        {
            var locale = UserSetup.LOCALE_NAME;
            var identityFilePath = UserSetup.IDENTITY_FILE_PATH;
            var jsonPath = UserSetup.JSON_PATH;

            var secrets = Bridge.GetSecrets();
            if (secrets is not null)
            {
                try
                {
                    var accountsSettingsJsonPath = secrets.jsonPath;
                    var accountsSettingsJson = File.ReadAllText(accountsSettingsJsonPath);
                    var jObj = JObject.Parse(accountsSettingsJson);

                    var acctSettingsJsonPath = $"$.Accounts[{secrets.accountIndex}].IdentityTokens";

                    var localeName = jObj.SelectToken(acctSettingsJsonPath + ".LocaleName")
                        .Value<string>();

                    // success. set var.s
                    locale = localeName;
                    identityFilePath = accountsSettingsJsonPath;
                    jsonPath = acctSettingsJsonPath;
                }
                catch { }
            }


            var api = await EzApiCreator.GetApiAsync(
                new LoginCallback(email, pass),
                Localization.Get(locale),
                identityFilePath,
                jsonPath
            );

            return new ApiClient(api);
        }
    }
}
