namespace BionicPRO.Models
{
    public class ReportModel
    {
        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string UserName { get; set; } = string.Empty;

        /// <summary>
        /// Модель протеза
        /// </summary>
        public string ProsthesisModel { get; set; } = string.Empty;

        /// <summary>
        /// Дата отчета
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// Средняя сила сигнала
        /// </summary>
        public double AvgSignalStrength { get; set; }

        /// <summary>
        /// Средний уровень заряда
        /// </summary>
        public int AvgBatteryLevel { get; set; }

        /// <summary>
        /// Среднее время реакции
        /// </summary>
        public int AvgResponseTimeMs { get; set; }

        /// <summary>
        /// Всего движений
        /// </summary>
        public int TotalMovements { get; set; }

        /// <summary>
        /// Чаще всего используемое движение
        /// </summary>
        public string MostUsedMovement { get; set; } = string.Empty;

        /// <summary>
        /// Ошибок за период
        /// </summary>
        public int TotalErrors { get; set; }

        /// <summary>
        /// Время активности
        /// </summary>
        public double UptimeHours { get; set; }

        /// <summary>
        /// Оценка производительности
        /// </summary>
        public string PerformanceGrade { get; set; } = string.Empty;
    }
}
