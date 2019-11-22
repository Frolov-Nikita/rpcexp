using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
            Database.SetCommandTimeout(TimeSpan.FromSeconds(5));

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlite("Data Source=" + dbName)
                //.UseLoggerFactory(MyLoggerFactory)
                ;
        }

        private static readonly ILoggerFactory MyLoggerFactory = LoggerFactory.Create(builder => { builder.AddDebug(); });

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<TagLogData>()
                .HasKey(t => new {t.TimeStamp, t.TagLogInfoId });
        }

        public DbSet<TagLogData> TagLogData { get; set; }

        public DbSet<TagLogInfo> TagLogInfo { get; set; }
    }//TagLogContext
}
