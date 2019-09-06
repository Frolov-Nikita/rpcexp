using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RPCExp.AlarmLogger.Model
{
    public class AlarmsContext : DbContext
    {
        public AlarmsContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=alarms.sqlite3");
        }

        public DbSet<AlarmCategory> AlarmCategories { get; set; }

        public DbSet<AlarmConfig> AlarmsConfig { get; set; }

        public DbSet<Alarm> Alarms { get; set; }

        public int MaxLimit { get; set; } = 10_000;

        public async Task Maintenance() 
        {
            var currCount = await Alarms.CountAsync();            
            if(currCount > MaxLimit)
            {
                var itemsToDelete = Alarms.OrderBy(d => d.DateTime).Take(MaxLimit - currCount);
                Alarms.RemoveRange(itemsToDelete);
                await SaveChangesAsync();
                await Database.ExecuteSqlCommandAsync(new RawSqlString("VACUUM ;"));
            }

        }
    }
}
