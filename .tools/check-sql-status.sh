#!/bin/bash
# Script to check SQL Server status and provide diagnostics

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
echo "Checking SQL Server container status..."

# Check if we can ping the SQL Server host
echo "Pinging mssql container..."
ping -c 3 mssql || echo "Cannot ping SQL Server container"

# Check if port 1433 is listening
echo "Checking port 1433..."
nc -zv mssql 1433 || echo "Port 1433 is not accessible"

# Check if we can connect directly
echo "Attempting SQL connection..."
$SQLCMD -S mssql -U sa -P RCPassword1! -Q "SELECT @@VERSION" || echo "SQL query failed"

echo "SQL Server container diagnostics completed."
