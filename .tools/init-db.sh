#!/bin/bash
# This script initializes the SQL Server database for tests

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
MAX_RETRIES=90
RETRY_INTERVAL=2
count=0

# Function to check SQL Server connection
check_sqlserver() {
  $SQLCMD -S mssql -U sa -P RCPassword1! -Q "SELECT 1" &> /dev/null
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
  echo "Checking if container is running..."
  if [ -z "$(docker ps | grep mssql)" ]; then
    echo "SQL Server container is not running!"
  else
    echo "SQL Server container is running but not responding."
  fi
  echo "SQL Server logs:"
  docker logs mssql
  exit 1
fi

echo "SQL Server is ready!"

echo "Initializing database..."
# Create the database if it doesn't exist
$SQLCMD -S mssql -U sa -P RCPassword1! -Q "IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'refactoringchallenge') CREATE DATABASE refactoringchallenge;"

echo "Running schema creation script..."
# Run the database initialization script
$SQLCMD -S mssql -U sa -P RCPassword1! -i /app/RefactoringChallenge.Output/DatabaseSchema.sql

echo "Verifying database creation..."
$SQLCMD -S mssql -U sa -P RCPassword1! -Q "SELECT name FROM sys.databases WHERE name = 'refactoringchallenge'"

echo "Database initialization completed."
