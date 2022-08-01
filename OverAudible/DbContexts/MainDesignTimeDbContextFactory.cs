using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.DbContexts
{
    internal class MainDesignTimeDbContextFactory : IDesignTimeDbContextFactory<MainDbContext>
    {
        public MainDbContext CreateDbContext(string[] args)
        {
            var dbContextOptions = new DbContextOptionsBuilder().UseSqlite("Data Source=OverAudible.db").Options;

            return new MainDbContext(dbContextOptions);
        }
    }
}
