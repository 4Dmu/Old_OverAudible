using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverAudible.Models.DTOs;

namespace OverAudible.DbContexts
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ItemDTO> OfflineLibrary { get; set; }

        public DbSet<NoMetaItemDTO> FullLibrary { get; set; }

        public DbSet<CatalogItemDTO> Wishlist { get; set; }

    }
}
