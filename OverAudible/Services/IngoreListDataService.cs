using AudibleApi.Common;
using Microsoft.EntityFrameworkCore;
using OverAudible.DbContexts;
using OverAudible.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.Services
{
    public class IgnoreListDataService : IDataService<IgnoreItem>
    {
        private readonly MainDbContextFactory _factory;

        public IgnoreListDataService(MainDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<IgnoreItem> Create(IgnoreItem entity)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                if (context.IgnoreList.Contains(entity))
                    context.IgnoreList.Update(entity);
                else
                    await context.IgnoreList.AddAsync(entity);
                
                await context.SaveChangesAsync();
                return entity;
            }
        }

        public async Task<bool> Delete(string id)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                IgnoreItem? item = await context.IgnoreList.FirstOrDefaultAsync(x => x.Asin == id);
                if (item == null)
                    return false;

                context.IgnoreList.Remove(item);
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<bool> DeleteAll()
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                context.IgnoreList.RemoveRange(context.IgnoreList.ToList());
                await context.SaveChangesAsync();
                return true;
            }
        }

        public async Task<List<IgnoreItem>> GetAll()
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                return await context.IgnoreList.ToListAsync();
            }
        }

        public async Task<IgnoreItem> GetById(string id)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                return await context.IgnoreList.FirstOrDefaultAsync(x => x.Asin == id);
            }
        }

        public async Task<IgnoreItem> Update(string id, IgnoreItem entity)
        {
            using (MainDbContext context = _factory.CreateDbContext())
            {
                context.IgnoreList.Update(entity);
                await context.SaveChangesAsync();

                return entity;
            }
        }

        public Task UpdateMetadata(string id, ContentMetadata extra)
        {
            throw new NotImplementedException();
        }

        public Task<List<(IgnoreItem, ContentMetadata)>> GetAllWithMetadata()
        {
            throw new NotImplementedException();
        }

        public Task<(IgnoreItem, ContentMetadata)> GetByIdWithMetadata(string id)
        {
            throw new NotImplementedException();
        }
    }
}
