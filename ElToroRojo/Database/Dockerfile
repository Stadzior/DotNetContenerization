FROM mcr.microsoft.com/mssql/server:2022-latest

LABEL purpose=db
LABEL case=eltororojo
LABEL apptype=MSSQL

ENV \
    ACCEPT_EULA=Y \
    MSSQL_SA_PASSWORD=zaq1@WSX \
    MSSQL_PID=Express

# Create a config directory
RUN mkdir -p /usr/setup
WORKDIR /usr/setup

# Bundle config source
COPY . .
EXPOSE 1433
ENTRYPOINT ./entrypoint ${SEED}