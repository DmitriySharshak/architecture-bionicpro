﻿-- Создание базы данных
CREATE DATABASE IF NOT EXISTS bionicpro_analytics;

USE bionicpro_analytics;

--телеметрия
CREATE TABLE IF NOT EXISTS telemetry (
    user_id UInt32,
    prosthesis_type String,
    muscle_group String,
    signal_frequency Float32,
    signal_duration UInt32,
    signal_amplitude UInt32,
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

-- Добавляем тестовые данные для витрины
INSERT INTO telemetry 
SELECT 
    number % 5 + 1 AS user_id,
    'BionicPRO X1',
    CASE 
        WHEN (110 + rand() % 60) < 100 THEN 'Biceps'
        WHEN (110 + rand() % 60) < 130 THEN 'Triceps'
        ELSE 'Gastrocnemius'
    END,
    0.65 + rand() % 20 / 100.0,
    75 + rand() % 20,
    95 + rand() % 40,
    toDateTime('2024-01-01 00:00:00') + rand() % (86400 * 366)
FROM numbers(150);

