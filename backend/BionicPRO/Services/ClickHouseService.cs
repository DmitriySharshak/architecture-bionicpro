using BionicPRO.Models;
using ClickHouse.Client.ADO;
using ClickHouse.Client.ADO.Parameters;
using ClickHouse.Client.Utility;
using System.Data;

namespace BionicPRO.Services
{
    public class ClickHouseService : IClickHouseService
    {
        private readonly string _connectionString;
        private readonly ILogger<ClickHouseService> _logger;

        public ClickHouseService(IConfiguration configuration, ILogger<ClickHouseService> logger)
        {
            var host     = configuration["ClickHouse:Host"] ?? "localhost";
            var port     = configuration["ClickHouse:Port"] ?? "9000";
            var database = configuration["ClickHouse:Database"] ?? "default";
            var user     = configuration["ClickHouse:User"] ?? "default";
            var password = configuration["ClickHouse:Password"] ?? "";

            _connectionString = $"Host={host};Port={port};Database={database};User={user};Password={password}";
            _logger           = logger;
        }

        public async Task<List<ReportModel>> GetUserReportsAsync(string userId, DateTime startDate, DateTime endDate)
        {
            var reports = new List<ReportModel>();

            try
            {
                // Преобразуем DateTime в DateOnly
                // Форматируем как строку без времени
                //var startDateOnly = startDate.ToString("yyyy-MM-dd");
                //var endDateOnly   = endDate.ToString("yyyy-MM-dd");

                var startDateOnly = startDate.ToString("yyyy-MM-dd");
                var endDateOnly   = endDate.ToString("yyyy-MM-dd");
                
                var safeUserId = userId.Replace("'", "''");// Экранируем userId для безопасной вставки
                //var safeUserId    = userId.Replace("'", "''"); // Экранирование кавычек



                await using var connection    = new ClickHouseConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                SELECT 
                    user_id,
                    user_name,
                    prosthesis_model,
                    report_date,
                    avg_signal_strength,
                    avg_battery_level,
                    avg_response_time_ms,
                    total_movements,
                    most_used_movement,
                    total_errors,
                    uptime_hours,
                    performance_grade
                FROM reporting_daily
                WHERE user_id = {userId:String}
                        AND report_date BETWEEN {startDate:Date} AND {endDate:Date}
                ORDER BY report_date DESC";

                // 
                // 
                await using var command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.Add(new ClickHouseDbParameter()
                {
                    ParameterName = "userId",
                    Value = userId,
                });
                command.Parameters.Add(new ClickHouseDbParameter()
                {
                    ParameterName = "startDate",
                    Value = startDateOnly,
                });
                command.Parameters.Add(new ClickHouseDbParameter()
                {
                    ParameterName = "endDate",
                    Value = endDateOnly,
                });

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    reports.Add(new ReportModel
                    {
                        UserId            = reader.GetString(0),
                        UserName          = reader.GetString(1),
                        ProsthesisModel   = reader.GetString(2),
                        ReportDate        = reader.GetDateTime(3),
                        AvgSignalStrength = reader.GetFloat(4),
                        //AvgBatteryLevel   = reader.GetByte(5),
                        //AvgResponseTimeMs = reader.GetFieldValue<ushort>(6),
                        //TotalMovements    = reader.GetFieldValue<ushort>(7),
                        //MostUsedMovement  = reader.GetString(8),
                        //TotalErrors       = reader.GetByte(9),
                        //UptimeHours       = reader.GetFloat(10),
                        //PerformanceGrade  = reader.GetString(11)
                    });
                }

                _logger.LogInformation("Retrieved {Count} reports for user {UserId} from {StartDate} to {EndDate}",
                    reports.Count, userId, startDateOnly, endDateOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving reports for user {UserId}", userId);
                throw;
            }

            return reports;
        }
    }
}
