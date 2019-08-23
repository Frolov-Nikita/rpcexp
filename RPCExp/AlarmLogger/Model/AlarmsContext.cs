using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
