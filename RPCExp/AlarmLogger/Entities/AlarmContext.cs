using Microsoft.EntityFrameworkCore;

namespace RPCExp.AlarmLogger.Entities
{
    internal class AlarmContext : DbContext
    {
        private string dbName;

        public AlarmContext(string dbName = "alarmLog.sqlite3")
        {
            this.dbName = dbName;
            var created = Database.EnsureCreated();
            if (created)
                CreateAlarmsView();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=" + dbName);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Alarm>()
                .HasKey(e => new { e.TimeStamp, e.AlarmInfoId });
        }

        public DbSet<Alarm> Alarms { get; set; }

        public DbSet<AlarmInfo> AlarmsInfo { get; set; }

        public DbSet<AlarmCategory> AlarmCategories { get; set; }

        internal void CreateAlarmsView()
        {
            var sql = @"CREATE VIEW IF NOT EXISTS AlarmsView AS
                        SELECT 
	                        Alarms.TimeStamp,
	                        AlarmCategories.Name,
	                        AlarmsInfo.Condition,
	                        AlarmsInfo.Description,
	                        AlarmsInfo.FacilityAccessName,
	                        AlarmsInfo.DeviceName,
	                        REPLACE( REPLACE( REPLACE( REPLACE(
			                        AlarmsInfo.TemplateTxt, 
			                        '{{Custom4}}', ifnull(Alarms.Custom4, '')), 
			                        '{{Custom3}}', ifnull(Alarms.Custom3, '')), 
			                        '{{Custom2}}', ifnull(Alarms.Custom2, '')), 
			                        '{{Custom1}}', ifnull(Alarms.Custom1, '')) as message
                        FROM 
	                        Alarms
                        LEFT JOIN
	                        AlarmsInfo 
	                        ON AlarmsInfo.Id == Alarms.AlarmInfoId
                        LEFT JOIN
	                        AlarmCategories 
	                        ON AlarmCategories.Id == AlarmsInfo.CategoryId";

            Database.ExecuteSqlRaw(sql); //""{Custom1}"" не нравится стрингбильдеру
        }
    }
}
