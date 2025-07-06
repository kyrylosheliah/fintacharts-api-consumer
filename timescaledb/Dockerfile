FROM timescale/timescaledb:latest-pg17

COPY entrypoint.sh /docker-entrypoint-initdb.d/entrypoint.sh
RUN chmod +x /docker-entrypoint-initdb.d/entrypoint.sh

COPY init.sql /docker-entrypoint-initdb.d/init.sql