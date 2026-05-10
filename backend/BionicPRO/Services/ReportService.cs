using BionicPRO.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BionicPRO.Services
{
    public class ReportService : IReportService
    {
        private readonly ILogger<ReportService> _logger;

        public ReportService(ILogger<ReportService> logger)
        {
            _logger                   = logger;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        //public async Task<byte[]> GeneratePdfReportAsync(ReportModel report)
        //{
        //    return await Task.Run(() =>
        //    {
        //        var document = Document.Create(container =>
        //        {
        //            container.Page(page =>
        //            {
        //                page.Size(PageSizes.A4);
        //                page.Margin(2, Unit.Centimetre);
        //                page.DefaultTextStyle(x => x.FontSize(12));

        //                page.Header()
        //                    .Text("BionicPRO - Отчет о работе протеза")
        //                    .SemiBold()
        //                    .FontSize(20)
        //                    .FontColor(Colors.Blue.Darken2);

        //                page.Content()
        //                    .PaddingVertical(1, Unit.Centimetre)
        //                    .Column(column =>
        //                    {
        //                        column.Item().Text($"Дата отчета: {report.ReportDate:dd.MM.yyyy}");
        //                        column.Item().Text($"Пользователь: {report.UserName}");
        //                        column.Item().Text($"Модель протеза: {report.ProsthesisModel}");
        //                        column.Item().PaddingBottom(1, Unit.Centimetre);

        //                        column.Item().Table(table =>
        //                        {
        //                            table.ColumnsDefinition(columns =>
        //                            {
        //                                columns.ConstantColumn(150);
        //                                columns.RelativeColumn();
        //                            });

        //                            table.Cell().Text("Средняя сила сигнала:").Bold();
        //                            table.Cell().Text($"{report.AvgSignalStrength:F2}");

        //                            table.Cell().Text("Средний уровень заряда:").Bold();
        //                            table.Cell().Text($"{report.AvgBatteryLevel}%");

        //                            table.Cell().Text("Среднее время реакции:").Bold();
        //                            table.Cell().Text($"{report.AvgResponseTimeMs} мс");

        //                            table.Cell().Text("Всего движений:").Bold();
        //                            table.Cell().Text(report.TotalMovements.ToString());

        //                            table.Cell().Text("Чаще всего используемое движение:").Bold();
        //                            table.Cell().Text(report.MostUsedMovement);

        //                            table.Cell().Text("Ошибок за период:").Bold();
        //                            table.Cell().Text(report.TotalErrors.ToString());

        //                            table.Cell().Text("Время активности:").Bold();
        //                            table.Cell().Text($"{report.UptimeHours:F1} часов");

        //                            table.Cell().Text("Оценка производительности:").Bold();
        //                            table.Cell().Text(GetGradeText(report.PerformanceGrade))
        //                                .FontColor(GetGradeColor(report.PerformanceGrade));
        //                        });

        //                        column.Item().PaddingTop(1, Unit.Centimetre)
        //                            .Text(GetRecommendations(report.PerformanceGrade));
        //                    });

        //                page.Footer()
        //                    .AlignCenter()
        //                    .Text(x =>
        //                    {
        //                        x.Span("BionicPRO — инновационные бионические протезы");
        //                        x.Span(" | ");
        //                        x.Span($"Сгенерировано: {DateTime.Now:dd.MM.yyyy HH:mm}");
        //                    });
        //            });
        //        });

        //        return document.GeneratePdf();
        //    });
        //}

        private string GetGradeText(string grade) => grade switch
        {
            "Excellent" => "Отлично",
            "Good" => "Хорошо",
            "Needs Calibration" => "Требуется калибровка",
            _ => grade
        };

        private string GetGradeColor(string grade) => grade switch
        {
            "Excellent" => Colors.Green.Medium,
            "Good" => Colors.Orange.Medium,
            "Needs Calibration" => Colors.Red.Medium,
            _ => Colors.Black
        };

        private string GetRecommendations(string grade) => grade switch
        {
            "Excellent" => "✅ Протез работает в оптимальном режиме. Продолжайте использовать.",
            "Good" => "⚠️ Протез работает хорошо, но рекомендуется плановая проверка.",
            "Needs Calibration" => "🔧 Требуется калибровка протеза. Пожалуйста, обратитесь в сервисный центр.",
            _ => "📋 Обратитесь в службу поддержки для получения рекомендаций."
        };
    }
}
