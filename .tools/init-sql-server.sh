#!/bin/bash
# Script to initialize the SQL Server database

set -e

# Wait for SQL Server to start with longer timeout
echo "Waiting for SQL Server to start..."
max_attempts=60
counter=0
until /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "SELECT 1" &> /dev/null
do
  counter=$((counter + 1))
  if [ $counter -gt $max_attempts ]; then
    echo "ERROR: SQL Server did not become available in time"
    exit 1
  fi
  echo "SQL Server is starting up... (Attempt $counter/$max_attempts)"
  sleep 5
done

echo "SQL Server started successfully after $counter attempts"

# Check if the database already exists
DB_EXISTS=$(/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "SELECT COUNT(*) FROM sys.databases WHERE name = 'refactoringchallenge'" -h -1)

if [ "$DB_EXISTS" -eq "1" ]; then
  echo "Database 'refactoringchallenge' already exists."
  echo "Dropping existing database to ensure clean state..."
  /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "DROP DATABASE refactoringchallenge"
fi

# Create the database
echo "Creating database..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -Q "CREATE DATABASE refactoringchallenge;"

# Run the SQL scripts to set up the schema
echo "Initializing database schema..."
/opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P $SA_PASSWORD -d refactoringchallenge -i /docker-entrypoint-initdb.d/DatabaseSchema.sql

echo "Database initialization completed"
