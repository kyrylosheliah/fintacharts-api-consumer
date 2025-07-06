CREATE TABLE IF NOT EXISTS assets (
    time TIMESTAMPTZ NOT NULL,
    sensor_id TEXT NOT NULL,
    value DOUBLE PRECISION
);

SELECT create_hypertable('sensor_data', 'time', if_not_exists => TRUE);

-- Optional test insert
INSERT INTO sensor_data (time, sensor_id, value)
VALUES (NOW(), 'sensor-a', 23.5);
