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
        /// Почта
        /// </summary>
        public string UserEmail { get; set; } = string.Empty;

        /// <summary>
        /// Тип протеза
        /// </summary>
        public string ProsthesisType { get; set; } = string.Empty;

        /// <summary>
        /// Всего сигналов
        /// </summary>
        public long TotalSignals { get; set; }

        /// <summary>
        /// Дата отчета
        /// </summary>
        public DateTime ReportDate { get; set; }

        /// <summary>
        /// "Средняя частота сигнала (Гц)
        /// </summary>
        public double AvgSignalFrequency { get; set; }

        /// <summary>
        /// Средняя длительность (мс)
        /// </summary>
        public double AvgSignalDuration { get; set; }

        /// <summary>
        /// Средняя амплитуда (мкВ)
        /// </summary>
        public double AvgSignalAmplitude { get; set; }
       
    }
}
