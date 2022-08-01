using Newtonsoft.Json;
using OverAudible.API;
using OverAudible.Models;
using ShellUI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Services
{
    [Inject(InjectionType.Singleton)]
    public class HomeService
    {
        private readonly Lazy<Task> _initLazy;

        private AudiblePage _page;

        public HomeService()
        {
            _initLazy = new Lazy<Task>(InitAsync);
            _page = new AudiblePage();
        }

        private async Task InitAsync()
        {
            ApiClient api = await ApiClient.GetInstance();

            var homePage = await api.GetHomePage();

            if (homePage is null)
                return;

            AudiblePage page = JsonConvert.DeserializeObject<AudiblePage>(homePage.ToString());

            if (page is null)
                return;

            ValidatePage(page);

            _page = page;
        }

        private void ValidatePage(AudiblePage page)
        {
            var l = page.page.sections.ToList();
            List<Section> toRemove = new();
            List<Section> toMove = new();
            foreach (var s in l)
            {
                if (s.model == null || s.model.products == null)
                    toRemove.Add(s);

                else if (s.model != null && s.model.headers != null && s.model.headers.Length > 0 && s.model.headers[0] == "Continue listening")
                {
                    toMove.Add(s);
                }
            }

            foreach (var s1 in toRemove)
                l.Remove(s1);
            foreach (var s2 in toMove)
            {
                l.Remove(s2);
                l.Insert(0, s2);
            }
            page.page.sections = l.ToArray();
        }

        public async Task<AudiblePage> GetPage()
        {
            await _initLazy.Value;

            return _page;
        }
    }
}
