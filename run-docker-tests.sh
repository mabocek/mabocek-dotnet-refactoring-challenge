#!/bin/bash
# Script to run tests with better error handling

set -e  # Exit on error

# Define the SQL command tool based on what's available
if [ -f "/opt/sqlcmd/bin/sqlcmd" ]; then
  SQLCMD="/opt/sqlcmd/bin/sqlcmd"
elif command -v sqlcmd &> /dev/null; then
  SQLCMD="sqlcmd"
else
  echo "Error: sqlcmd tool not found"
  exit 1
fi

# Add the sqlcmd directory to the PATH and ensure it's used
export PATH="/opt/sqlcmd/bin:$PATH"
echo "Current PATH: $PATH"
which sqlcmd || echo "sqlcmd not in PATH"

echo "Using SQL command tool: $SQLCMD"

# Function to check if the database is ready
check_db() {
  echo "Checking SQL Server connection..."
  if $SQLCMD -S mssql -U sa -P RCPassword1! -Q "SELECT 1" 2>/dev/null; then
    echo "Connection successful!"
    return 0
  else
    echo "Connection failed: $?"
    return 1
  fi
}

# Function to check if database exists
check_db_exists() {
  echo "Checking if database 'refactoringchallenge' exists..."
  if $SQLCMD -S mssql -U sa -P RCPassword1! -Q "SELECT COUNT(*) FROM sys.databases WHERE name = 'refactoringchallenge'" -h -1 2>/dev/null | grep -q "1"; then
    echo "Database exists!"
    return 0
  else
    echo "Database does not exist."
    return 1
  fi
}

# Wait for SQL Server to be ready with exponential backoff
echo "Waiting for SQL Server to start..."
attempt=1
max_attempts=60  # Maximum number of attempts
backoff=5        # Starting backoff time in seconds
max_backoff=30   # Maximum backoff time in seconds

until check_db || [ $attempt -gt $max_attempts ]
do
  echo "Attempt $attempt/$max_attempts: SQL Server not ready yet, waiting for $backoff seconds..."
  sleep $backoff
  attempt=$((attempt+1))
  
  # Exponential backoff with maximum cap
  backoff=$((backoff * 2))
  if [ $backoff -gt $max_backoff ]; then
    backoff=$max_backoff
  fi
done

if [ $attempt -gt $max_attempts ]; then
  echo "ERROR: SQL Server did not become available in time"
  echo "Running diagnostic checks..."
  
  # Run the SQL Server status check script
  /bin/bash /app/.tools/check-sql-status.sh
  
  # Try to ping the SQL Server host
  echo "Pinging mssql host..."
  ping -c 3 mssql
  
  # Try to see if port is open
  echo "Checking if port 1433 is open..."
  nc -zv mssql 1433
  
  # Wait a bit more and try one last time
  echo "Waiting 60 more seconds and trying one final connection attempt..."
  sleep 60
  
  if check_db; then
    echo "SQL Server finally available after additional wait!"
  else
    echo "SQL Server still not available. Exiting."
    exit 1
  fi
fi

echo "SQL Server is ready!"

# Now wait for the database to be created
echo "Waiting for database 'refactoringchallenge' to be created..."
attempt=1
max_attempts=30

until check_db_exists || [ $attempt -gt $max_attempts ]
do
  echo "Attempt $attempt/$max_attempts: Database not ready yet, waiting..."
  sleep 5
  attempt=$((attempt+1))
done

if [ $attempt -gt $max_attempts ]; then
  echo "ERROR: Database 'refactoringchallenge' was not created in time"
  echo "Attempting to create the database ourselves..."
  
  # Drop the database if it exists
  $SQLCMD -S mssql -U sa -P RCPassword1! -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'refactoringchallenge') DROP DATABASE refactoringchallenge;"
  echo "Creating database..."
  $SQLCMD -S mssql -U sa -P RCPassword1! -Q "CREATE DATABASE refactoringchallenge;"
  
  if ! check_db_exists; then
    echo "Failed to create database. Exiting."
    exit 1
  fi
fi

echo "SQL Server and database are ready!"

# Initialize the database using the new script
echo "Initializing database with go-sqlcmd..."
/bin/bash /app/.tools/init-db-with-go-sqlcmd.sh

# Run tests with Docker settings
echo "Running tests..."
dotnet test --settings docker.runsettings
