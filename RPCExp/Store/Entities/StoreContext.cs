using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RPCExp.Common;

namespace RPCExp.Store.Entities
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
                .WithMany(d => d.Templates)
                .HasForeignKey(d => d.DeviceId);

            modelBuilder.Entity<DeviceToTemplate>()
                .HasOne(d => d.Template)
                .WithMany(d => d.Devices)
                .HasForeignKey(d => d.TemplateId);

        }

        public DbSet<ConnectionSourceCfg> Connections { get; set; }

        public DbSet<FacilityCfg> Facilities { get; set; }

        public DbSet<DeviceCfg> Devices { get; set; }

        public DbSet<Template> Templates { get; set; }

        public DbSet<TagsGroup> TagsGroups { get; set; }

        public TagsGroup GetOrCreateTagsGroup(TagsGroup group)
        {
            var storedGroup = TagsGroups.Local.FirstOrDefault(storedG => storedG.Name == group.Name);

            if (storedGroup == default)
                storedGroup = TagsGroups.FirstOrDefault(storedG => storedG.Name == group.Name);

            if (storedGroup == default)
            {
                storedGroup = new TagsGroup (group);
                TagsGroups.Add(storedGroup);
            }
            return storedGroup;
        }

        /// <summary>
        /// Связь many2many
        /// </summary>
        public DbSet<DeviceToTemplate> DeviceToTemplates { get; set; }


    }

    internal static class DbSetExtentions
    {
        public static T GetOrCreate<T>(this DbSet<T> dbSet, T entity, Func<T,bool> predicate)
            where T: class, ICopyFrom, new()
        {
            var stored = dbSet.Local.FirstOrDefault(predicate);

            if(stored == default)
                stored = dbSet.FirstOrDefault(predicate);

            if (stored == default)
            {
                stored = new T();                
                dbSet.Add(stored);
                stored.CopyFrom(entity);
            }

            return stored;
        }
    }

}
