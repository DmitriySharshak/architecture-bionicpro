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
                


                await using var connection    = new ClickHouseConnection(_connectionString);
                await connection.OpenAsync();

                const string query = @"
                SELECT 
                    user_id,
                    user_name,
                    user_email,
                    prosthesis_type,
                    total_signals,
                    avg_signal_frequency,
                    avg_signal_duration,
                    avg_signal_amplitude,
                    report_period_start,
                    report_period_end,
                    last_signal_time,
                    report_generated_at    
                    
                FROM bionicpro_analytics.reporting
                WHERE user_name = {userId:String}
                ORDER BY report_period_start DESC";

                // AND report_period_start BETWEEN {startDate:Date} AND {endDate:Date}
                // 
                await using var command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.Add(new ClickHouseDbParameter()
                {
                    ParameterName = "userId",
                    Value = userId,
                });
                //command.Parameters.Add(new ClickHouseDbParameter()
                //{
                //    ParameterName = "startDate",
                //    Value = startDateOnly,
                //});
                //command.Parameters.Add(new ClickHouseDbParameter()
                //{
                //    ParameterName = "endDate",
                //    Value = endDateOnly,
                //});

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    reports.Add(new ReportModel
                    {
                        UserId             = reader.GetString(0),
                        UserName           = reader.GetString(1),
                        UserEmail          = reader.GetString(2),
                        ProsthesisType     = reader.GetString(3),
                        //TotalSignals       = reader.GetFieldValue<long>(4),
                        AvgSignalFrequency = reader.GetDouble(5),
                        AvgSignalDuration  = reader.GetDouble(6),
                        AvgSignalAmplitude = reader.GetDouble(7),
                        ReportDate         = reader.GetDateTime(8)
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
