using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverAudible.DbContexts
{
    public class MainDbContextFactory
    {
        private readonly Action<DbContextOptionsBuilder> _configureDbContext;

        public MainDbContextFactory(Action<DbContextOptionsBuilder> configureDbContext)
        {
            _configureDbContext = configureDbContext;
        }

        public MainDbContext CreateDbContext()
        {
            DbContextOptionsBuilder<MainDbContext> options = new DbContextOptionsBuilder<MainDbContext>();

            _configureDbContext(options);

            return new MainDbContext(options.Options);
        }
    }
}
