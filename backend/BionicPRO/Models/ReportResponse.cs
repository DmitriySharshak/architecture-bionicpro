namespace BionicPRO.Models
{
    public class ReportResponse
    {
        public List<ReportModel> Reports { get; set; } = new();
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public int TotalCount { get; set; }
    }
}
