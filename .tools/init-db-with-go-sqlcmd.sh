#!/bin/bash
# Script to initialize the database using go-sqlcmd

set -e  # Exit on error

echo "Starting database initialization with go-sqlcmd..."

# Define the SQL command tool based on what's available
export PATH="/opt/sqlcmd/bin:$PATH"
echo "Current PATH: $PATH"
which sqlcmd || echo "sqlcmd not in PATH"

if [ -f "/opt/sqlcmd/bin/sqlcmd" ]; then
  SQLCMD="/opt/sqlcmd/bin/sqlcmd"
elif command -v sqlcmd &> /dev/null; then
  SQLCMD="sqlcmd"
else
  echo "Error: sqlcmd tool not found"
  exit 1
fi

echo "Using SQL command tool: $SQLCMD"

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to start..."
attempt=1
max_attempts=30
sleep_time=5

while [ $attempt -le $max_attempts ]
do
  echo "Attempt $attempt/$max_attempts: Checking if SQL Server is ready..."
  if $SQLCMD -S mssql -U sa -P RCPassword1! -Q "SELECT 1" &>/dev/null; then
    echo "SQL Server is ready!"
    break
  fi
  
  echo "SQL Server not ready yet, waiting for $sleep_time seconds..."
  sleep $sleep_time
  attempt=$((attempt+1))
done

if [ $attempt -gt $max_attempts ]; then
  echo "ERROR: SQL Server did not become available in time"
  exit 1
fi

echo "Dropping database if it exists..."
$SQLCMD -S mssql -U sa -P RCPassword1! -Q "IF EXISTS (SELECT name FROM sys.databases WHERE name = 'refactoringchallenge') DROP DATABASE refactoringchallenge;"

echo "Creating database..."
$SQLCMD -S mssql -U sa -P RCPassword1! -Q "CREATE DATABASE refactoringchallenge;"

echo "Creating schema..."
$SQLCMD -S mssql -U sa -P RCPassword1! -d refactoringchallenge -Q "
CREATE TABLE Customers (
    Id INT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    IsVip BIT NOT NULL DEFAULT 0,
    RegistrationDate DATETIME NOT NULL
);

CREATE TABLE Products (
    Id INT PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Category NVARCHAR(50) NOT NULL,
    Price DECIMAL(18, 2) NOT NULL
);

CREATE TABLE Inventory (
    ProductId INT PRIMARY KEY,
    StockQuantity INT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Orders (
    Id INT PRIMARY KEY,
    CustomerId INT NOT NULL,
    OrderDate DATETIME NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    DiscountPercent DECIMAL(5, 2) NULL,
    DiscountAmount DECIMAL(18, 2) NULL,
    Status NVARCHAR(20) NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id)
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18, 2) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE OrderLogs (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    LogDate DATETIME NOT NULL,
    Message NVARCHAR(500) NOT NULL,
    FOREIGN KEY (OrderId) REFERENCES Orders(Id)
);"

echo "Database initialization complete!"
