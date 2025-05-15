#!/bin/bash
# This script initializes the SQL Server database in Docker

# Define the SQL command tool based on what's available
if command -v sqlcmd &> /dev/null; then
  SQLCMD="sqlcmd"
elif [ -f "/opt/mssql-tools18/bin/sqlcmd" ]; then
  SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
elif [ -f "/opt/mssql-tools/bin/sqlcmd" ]; then
  SQLCMD="/opt/mssql-tools/bin/sqlcmd"
else
  echo "Error: sqlcmd tool not found"
  exit 1
fi

echo "Using SQL command tool: $SQLCMD"

echo "Waiting for SQL Server to start..."
# More robust method to wait for SQL Server to be ready
MAX_RETRIES=60
RETRY_INTERVAL=5
count=0

# Function to check SQL Server connection
check_sqlserver() {
  $SQLCMD -S localhost -U sa -P RCPassword1! -Q "SELECT 1" &> /dev/null
  return $?
}

# Wait loop with progress
until check_sqlserver || [ $count -ge $MAX_RETRIES ]; do
  count=$((count + 1))
  echo "Waiting for SQL Server to start... ($count/$MAX_RETRIES)"
  sleep $RETRY_INTERVAL
done

# Final check
if ! check_sqlserver; then
  echo "SQL Server did not start within the timeout period."
  exit 1
fi

echo "SQL Server is ready!"

echo "Initializing database..."
# Initialize the database using sqlcmd
$SQLCMD -S localhost -U sa -P RCPassword1! -i /docker-entrypoint-initdb.d/db-init.sql

echo "Verifying database creation..."
$SQLCMD -S localhost -U sa -P RCPassword1! -Q "SELECT name FROM sys.databases WHERE name = 'refactoringchallenge'"

echo "Database initialization completed."
