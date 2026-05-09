-- Создание базы данных
CREATE DATABASE IF NOT EXISTS bionicpro_analytics;

USE bionicpro_analytics;

-- Таблица для сырых данных телеметрии
CREATE TABLE IF NOT EXISTS raw_telemetry (
    prosthesis_id String,
    user_id String,
    timestamp Date,
    signal_strength Float32,
    battery_level UInt8,
    movement_type String,
    response_time_ms UInt16,
    error_count UInt16
) ENGINE = MergeTree()
ORDER BY (user_id, timestamp)
TTL timestamp + INTERVAL 90 DAY;

-- Таблица для данных из CRM
CREATE TABLE IF NOT EXISTS raw_crm (
    user_id String,
    user_name String,
    email String,
    prosthesis_model String,
    warranty_end Date,
    last_service Date,
    status String
) ENGINE = MergeTree()
ORDER BY user_id;

-- ВИТРИНА ОТЧЕТОВ (готовые данные для быстрых запросов)
CREATE TABLE IF NOT EXISTS reporting_daily (
    report_date Date,
    user_id String,
    user_name String,
    prosthesis_model String,
    avg_signal_strength Float32,
    avg_battery_level UInt8,
    avg_response_time_ms UInt16,
    total_movements UInt32,
    most_used_movement String,
    total_errors UInt16,
    uptime_hours Float32,
    performance_grade String
) ENGINE = SummingMergeTree()
ORDER BY (report_date, user_id);

-- Материализованное представление для автоматической агрегации
CREATE MATERIALIZED VIEW IF NOT EXISTS reporting_daily_mv
ENGINE = SummingMergeTree()
ORDER BY (report_date, user_id)
AS SELECT
    toDate(timestamp) as report_date,
    user_id,
    '' as user_name,
    '' as prosthesis_model,
    avg(signal_strength) as avg_signal_strength,
    avg(battery_level) as avg_battery_level,
    avg(response_time_ms) as avg_response_time_ms,
    count() as total_movements,
    '' as most_used_movement,
    sum(error_count) as total_errors,
    count() / 3600.0 as uptime_hours,
    '' as performance_grade
FROM raw_telemetry
GROUP BY report_date, user_id;

-- Добавляем тестовые данные для витрины
INSERT INTO reporting_daily 
SELECT 
    today() - number,
    'user1',
    'Иван Петров',
    'BionicPRO X1',
    0.65 + rand() % 20 / 100.0,
    75 + rand() % 20,
    95 + rand() % 40,
    500 + rand() % 500,
    'GRAB',
    rand() % 10,
    8.5,
    CASE 
        WHEN (95 + rand() % 40) < 100 THEN 'Excellent'
        WHEN (95 + rand() % 40) < 130 THEN 'Good'
        ELSE 'Needs Calibration'
    END
FROM numbers(30);

INSERT INTO reporting_daily 
SELECT 
    today() - number,
    'user2',
    'Мария Сидорова',
    'BionicPRO X2',
    0.55 + rand() % 20 / 100.0,
    65 + rand() % 20,
    110 + rand() % 60,
    300 + rand() % 400,
    'POINT',
    rand() % 15,
    5.2,
    CASE 
        WHEN (110 + rand() % 60) < 100 THEN 'Excellent'
        WHEN (110 + rand() % 60) < 130 THEN 'Good'
        ELSE 'Needs Calibration'
    END
FROM numbers(30);

INSERT INTO reporting_daily 
SELECT 
    today() - number,
    'user3',
    'Алексей Иванов',
    'BionicPRO X1',
    0.45 + rand() % 20 / 100.0,
    55 + rand() % 20,
    140 + rand() % 80,
    200 + rand() % 300,
    'THUMBS_UP',
    rand() % 25,
    3.8,
    'Needs Calibration'
FROM numbers(30);