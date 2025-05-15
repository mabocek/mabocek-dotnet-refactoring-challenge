-- This script will be run after the database is already created
-- so we just need to ensure we're using the right database
USE refactoringchallenge;
GO

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
);