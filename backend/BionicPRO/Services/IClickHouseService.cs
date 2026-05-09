using BionicPRO.Models;

namespace BionicPRO.Services
{
    public interface IClickHouseService
    {
        Task<List<ReportModel>> GetUserReportsAsync(string userId, DateTime startDate, DateTime endDate);
    }
}
