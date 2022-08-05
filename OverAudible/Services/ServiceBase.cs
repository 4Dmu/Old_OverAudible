using OverAudible.Models;
using ShellUI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Services
{
    public class AsyncServiceBase
    {
        private readonly Lazy<Task> _initLazy;

        public AsyncServiceBase()
        {
            _initLazy = new Lazy<Task>(InitAsync);
        }

        internal virtual async Task InitAsync()
        {
            await Task.CompletedTask;
        }

        public async Task ExecuteLazyAsync()
        {
            await _initLazy.Value;
        }
    }

    [Inject(InjectionType.Singleton)]
    public class IngoreListService : AsyncServiceBase
    {
        private List<IgnoreItem> IgnoredItems { get; set; }

        private readonly IgnoreListDataService _ignoreListDateService;

        public IngoreListService(IgnoreListDataService ignoreListDateService)
        {
            IgnoredItems = new();
            _ignoreListDateService = ignoreListDateService;
        }

        internal override async Task InitAsync()
        {
            if (IgnoredItems.Count > 0)
                IgnoredItems.Clear();
            IgnoredItems.AddRange(await _ignoreListDateService.GetAll());
            await base.InitAsync();
        }

        public async Task<List<IgnoreItem>> GetIngoredItems()
        {
            await ExecuteLazyAsync();

            return IgnoredItems;
        }

        public async Task AddIgnoreItem(IgnoreItem item)
        {
            IgnoredItems.Add(item);
            await _ignoreListDateService.Create(item);
        }

        public async Task RemoveIgnoreItem(IgnoreItem item)
        {
            IgnoredItems.Remove(item);
            await _ignoreListDateService.Delete(item.Asin);
        }

        public async Task RemoveAllIgnoredItems()
        {
            IgnoredItems.Clear();
            await _ignoreListDateService.DeleteAll();
        }

    }
}
