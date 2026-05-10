using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using BionicPRO.Models;
using BionicPRO.Services;

namespace BionicPRO.Controllers
{
    [ApiController]
    [Route("reports")]
    [Authorize]
    public class ReportsController : ControllerBase
    {

        private readonly IClickHouseService _clickHouseService;
        private readonly IReportService _reportService;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(ILogger<ReportsController> logger, IClickHouseService clickHouseService, IReportService reportService)
        {
            _clickHouseService = clickHouseService;
            _reportService          = reportService;
            _logger                 = logger;
        }


        //[HttpGet()]
        //public async Task<ActionResult<ApiResponse<string>>> Get()
        //{
        //    try
        //    {
        //        var userId = GetCurrentUserId();
        //        return this.Ok(ApiResponse<string>.Ok(userId));
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        return Unauthorized(ApiResponse<ReportResponse>.Error(ex.Message));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error getting reports for user");
        //        return StatusCode(500, ApiResponse<ReportResponse>.Error("Internal server error"));
        //    }
        //}

        private string GetCurrentUserId()
        {
            // Извлекаем user_id из JWT токена
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            //             ?? User.FindFirst("preferred_username")?.Value
            //             ?? User.FindFirst("sub")?.Value;

            var userId = User.FindFirst("preferred_username")?.Value;
                         

            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID not found in token");
            }

            Console.WriteLine($"User ID:{userId}");

            return userId;
        }

        [HttpGet()]
        public async Task<ActionResult<ApiResponse<ReportResponse>>> GetMyReports([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            try
            {
                var userId = GetCurrentUserId();

                // По умолчанию - последние 30 дней, но не включая сегодня (данные еще не обработаны)
                var end   = endDate ?? DateTime.Today.AddDays(-1);
                var start = startDate ?? end.AddDays(-29);

                _logger.LogInformation("Пользователь {UserId} запросил отчеты с {StartDate} по {EndDate}", userId, start, end);

                var reports = await _clickHouseService.GetUserReportsAsync(userId, start, end);

                var response = new ReportResponse
                {
                    Reports     = reports,
                    PeriodStart = start,
                    PeriodEnd   = end,
                    TotalCount  = reports.Count
                };

                return Ok(ApiResponse<ReportResponse>.Ok(response, "Отчеты успешно получены"));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<ReportResponse>.Error(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка получения отчетов для пользователя");
                return StatusCode(500, ApiResponse<ReportResponse>.Error("Internal server error"));
            }
        }
    }
}
