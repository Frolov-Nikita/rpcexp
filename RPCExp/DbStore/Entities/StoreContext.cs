using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPCExp.Common;

namespace RPCExp.DbStore.Entities
{
    internal class StoreContext : DbContext
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

            modelBuilder.Entity<DeviceToTemplate>()
                .HasKey(d => new { d.DeviceId, d.TemplateId });

            modelBuilder.Entity<DeviceToTemplate>()
                .HasOne(d => d.Device)
                .WithMany(d => d.DeviceToTemplates)
                .HasForeignKey(d => d.DeviceId);

            modelBuilder.Entity<DeviceToTemplate>()
                .HasOne(d => d.Template)
                .WithMany(d => d.DeviceToTemplates)
                .HasForeignKey(d => d.TemplateId);

            modelBuilder.Entity<TagsToTagsGroups>()
                .HasKey(e => new {e.TagId, e.TagsGroupId });

            modelBuilder.Entity<TagsToTagsGroups>()
                .HasOne(e => e.TagCfg)
                .WithMany(e => e.TagsToTagsGroups)
                .HasForeignKey(e => e.TagId);

            modelBuilder.Entity<TagsToTagsGroups>()
                .HasOne(e => e.TagsGroupCfg)
                .WithMany(e => e.TagsToTagsGroups)
                .HasForeignKey(e => e.TagsGroupId);

            modelBuilder.Entity<ArchiveCfg>()
                .HasOne(e => e.Tag)
                .WithOne(e => e.ArchiveCfg);
        }

        public DbSet<ConnectionSourceCfg> Connections { get; set; }

        public DbSet<FacilityCfg> Facilities { get; set; }

        public DbSet<DeviceCfg> Devices { get; set; }

        public DbSet<Template> Templates { get; set; }

        public DbSet<TagsGroupCfg> TagsGroups { get; set; }

        public DbSet<TagCfg> Tags{ get; set; }

        public DbSet<AlarmCfg> Alarms { get; set; }

        public DbSet<ArchiveCfg> Archives { get; set; }
        
        
        // Связи many2many:


        public DbSet<DeviceToTemplate> DeviceToTemplates { get; set; }

        public DbSet<TagsToTagsGroups> TagsToTagsGroups { get; set; }


    }

    internal static class DbSetExtentions
    {
        public static T GetOrCreate<T>(this DbSet<T> dbSet, Func<T,bool> predicate)
            where T: class, new()
        {
            var stored = dbSet.Local.FirstOrDefault(predicate);

            if(stored == default)
                stored = dbSet.FirstOrDefault(predicate);

            if (stored == default)
            {
                stored = new T();                
                dbSet.Add(stored);
            }

            return stored;
        }
    }

}
