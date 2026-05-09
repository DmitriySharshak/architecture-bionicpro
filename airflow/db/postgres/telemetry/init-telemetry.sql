-- Создание базы данных для телеметрии
CREATE DATABASE IF NOT EXISTS bionicpro_telemetry;

\c bionicpro_telemetry;

-- Таблица телеметрии (секционирована по месяцам)
CREATE TABLE IF NOT EXISTS telemetry_logs (
    log_id BIGSERIAL,
    prosthesis_id UUID NOT NULL,
    user_id VARCHAR(100) NOT NULL,
    timestamp TIMESTAMP NOT NULL,
    signal_strength FLOAT,
    battery_level INT,
    movement_type VARCHAR(50),
    response_time_ms INT,
    error_count INT DEFAULT 0,
);



