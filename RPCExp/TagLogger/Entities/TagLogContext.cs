using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.TagLogger.Entities
{
    internal class TagLogContext : DbContext
    {
        string dbName;

        public TagLogContext(string dbName = "tagLog.sqlite3")
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
            modelBuilder.Entity<TagLogData>()
                .HasKey(t => new {t.TimeStamp, t.TagLogInfo });
        }

        public DbSet<TagLogData> TagLogData { get; set; }

        public DbSet<TagLogInfo> TagLogInfo { get; set; }
    }//TagLogContext
}
