using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace RPCExp.AlarmLogger.Entities
{
    internal class AlarmContext : DbContext
    {
        string dbName;

        public AlarmContext(string dbName = "alarmLog.sqlite3")
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
            modelBuilder.Entity<Alarm>()
                .HasKey(e => new {e.TimeStamp, e.AlarmInfoId });

        }

        public DbSet<Alarm> Alarms { get; set; }

        public DbSet<AlarmInfo> AlarmsInfo { get; set; }

        public DbSet<AlarmCategory> AlarmCategories { get; set; }
    }
}
