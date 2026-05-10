-- Создание базы данных
CREATE DATABASE IF NOT EXISTS bionicpro_analytics;

--телеметрия
CREATE TABLE IF NOT EXISTS bionicpro_analytics.telemetry (
    user_id UInt32,
    prosthesis_type String,
    muscle_group String,
    signal_frequency Float32,
    signal_duration Float32,
    signal_amplitude Float32,
    signal_time DateTime
) ENGINE = MergeTree()
ORDER BY (user_id, prosthesis_type, signal_time);

--Витрина для отчётов о работе протезов
CREATE TABLE IF NOT EXISTS bionicpro_analytics.reporting (
    -- Идентификаторы
    user_id UInt32,
    user_name String,
    user_email String,
    
    -- Информация о протезе
    prosthesis_type String,
    
    -- Агрегированные метрики телеметрии (за период)
    total_signals UInt64,
    avg_signal_frequency Float64,
    avg_signal_duration Float64,
    avg_signal_amplitude Float64,
    
    -- Статистика по группам мышц
    muscle_groups_used Array(String),
    most_active_muscle_group String,
    
    -- Временные метки
    report_period_start DateTime,
    report_period_end DateTime,
    last_signal_time DateTime,
  
    -- Метаданные
    report_generated_at DateTime DEFAULT now()
) ENGINE = MergeTree()
ORDER BY (user_id, report_period_start, prosthesis_type);

-- Вставка данных простыми INSERT
INSERT INTO bionicpro_analytics.telemetry VALUES
(1, 'BionicPRO X1', 'Biceps', 65.50, 75.00, 95.50, '2024-01-15 10:00:00'),
(1, 'BionicPRO X1', 'Biceps', 68.30, 78.00, 98.30, '2024-01-15 11:00:00'),
(1, 'BionicPRO X1', 'Triceps', 70.20, 80.00, 102.40, '2024-01-15 12:00:00'),
(2, 'BionicPRO X2', 'Triceps', 72.80, 82.00, 105.20, '2024-01-15 10:30:00'),
(2, 'BionicPRO X2', 'Triceps', 75.10, 85.00, 108.90, '2024-01-15 11:30:00'),
(2, 'BionicPRO X2', 'Biceps', 70.50, 80.00, 101.50, '2024-01-15 12:30:00'),
(3, 'BionicPRO X3', 'Gastrocnemius', 68.40, 78.00, 97.30, '2024-01-15 11:00:00'),
(3, 'BionicPRO X3', 'Gastrocnemius', 71.20, 81.00, 100.80, '2024-01-15 12:00:00'),
(4, 'BionicPRO X4', 'Quadriceps', 80.00, 90.00, 110.30, '2024-01-15 09:00:00'),
(4, 'BionicPRO X4', 'Quadriceps', 83.50, 93.00, 114.20, '2024-01-15 10:00:00'),
(5, 'BionicPRO X5', 'Quadriceps', 82.50, 92.00, 112.80, '2024-01-15 14:00:00'),
(5, 'BionicPRO X5', 'Quadriceps', 86.20, 96.00, 117.40, '2024-01-15 15:00:00');
