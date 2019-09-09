using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPCExp.Common;

namespace RPCExp.Store
{
    class StoreContext : DbContext
    {
        string dbName;

        public StoreContext(string dbName = "storeCfg.sqlite3")
        {
            this.dbName = dbName;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + dbName);
        }

        public DbSet<Facility> Facilities { get; set; }
    }
}
