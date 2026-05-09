using LinqToDB;
using LinqToDB.Data;
using LinqToDB.DataProvider.PostgreSQL;
using LinqToDB.Mapping;

namespace BionicPRO.Data
{
    public class AnalyticsDbContext
    {
        private readonly DataConnection _connection;

        public AnalyticsDbContext(string connectionString)
        {
            _connection = new DataConnection(PostgreSQLTools.GetDataProvider(PostgreSQLVersion.v93), connectionString);
        }

        public ITable<T> GetTable<T>() where T : class
        {
            return _connection.GetTable<T>();
        }


        public DataConnection Connection { get { return this._connection; } }

        public ITable<Reporting> Reporting { get { return GetTable<Reporting>(); } }
    }

    [Table("reporting_daily", Schema = "bionicpro_analytics")]
    public class Reporting
    {
        [PrimaryKey, Identity]
        [Column("id")]
        public long Id { get; set; }

        [Column("report_date")]
        public DateTime ReportDate { get; set; }

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("user_name")]
        public string UserName { get; set; } = string.Empty;

        [Column("prosthesis_model")]
        public string ProsthesisModel { get; set; } = string.Empty;

        [Column("avg_signal_strength")]
        public double AvgSignalStrength { get; set; }

        [Column("avg_battery_level")]
        public int AvgBatteryLevel { get; set; }

        [Column("avg_response_time_ms")]
        public int AvgResponseTimeMs { get; set; }

        [Column("total_movements")]
        public int TotalMovements { get; set; }

        [Column("most_used_movement")]
        public string MostUsedMovement { get; set; } = string.Empty;

        [Column("total_errors")]
        public int TotalErrors { get; set; }

        [Column("uptime_hours")]
        public double UptimeHours { get; set; }

        [Column("performance_grade")]
        public string PerformanceGrade { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
