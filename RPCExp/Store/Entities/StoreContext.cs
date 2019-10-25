using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPCExp.Common;

namespace RPCExp.Store.Entities
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }

        public DbSet<ConnectionSourceCfg> Connections { get; set; }

        public DbSet<FacilityCfg> Facilities { get; set; }
        
        public DbSet<Template> Templates { get; set; }

    }
}
