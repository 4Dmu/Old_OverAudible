using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OverAudible.DbContexts;
using OverAudible.Models;
using OverAudible.Models.DTOs;

namespace OverAudible.Services
{
    public class WishlistDataService : IDataService<Item>
    {
        private readonly MainDbContextFactory _factory;

        public WishlistDataService(MainDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<Item> Create(Item entity)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                var dto = new CatalogItemDTO()
                {
                    Asin = entity.Asin,
                    Item = JsonConvert.SerializeObject(entity)
                };

                if (context.Wishlist.Contains(dto))
                {
                    return await Update(entity.Asin, entity);
                }

                await context.Wishlist.AddAsync(dto);
                await context.SaveChangesAsync();

                return entity;
            }
        }

        public async Task<bool> Delete(string id)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                CatalogItemDTO? dto = await context.Wishlist.FirstOrDefaultAsync(x =>
                    x.Asin == id);
                if (dto == null)
                    return false;

                context.Wishlist.Remove(dto);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteAll()
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                context.Wishlist.RemoveRange(context.Wishlist.ToList());
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<Item>> GetAll()
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                var items = await context.Wishlist.ToListAsync();

                List<Item> result = new List<Item>();

                foreach (var item in items)
                {
                    result.Add(JsonConvert.DeserializeObject<Item>(item.Item));
                }

                return result;
            }
        }

        public async Task<Item> GetById(string id)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                CatalogItemDTO dto = await context.Wishlist.FirstOrDefaultAsync(x => x.Asin == id);
                return JsonConvert.DeserializeObject<Item>(dto.Item);
            }
        }

        public async Task<Item> Update(string id, Item entity)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                entity.Asin = id;

                var dto = new CatalogItemDTO()
                {
                    Asin = entity.Asin,
                    Item = JsonConvert.SerializeObject(entity)
                };

                context.Wishlist.Update(dto);
                await context.SaveChangesAsync();

                return entity;
            }
        }


        #region Not Implemented
        public async Task UpdateMetadata(string id, AudibleApi.Common.ContentMetadata extra)
        {
            throw new NotImplementedException();
        }

        public async Task<List<(Item, AudibleApi.Common.ContentMetadata)>> GetAllWithMetadata()
        {
            throw new NotImplementedException();
        }

        public async Task<(Item, AudibleApi.Common.ContentMetadata)> GetByIdWithMetadata(string id)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
