#!/bin/bash

# Start the script to create the DB and user
/usr/setup/configure-db.sh &

# Start SQL Server
/opt/mssql/bin/sqlservr