-- Create the database
--CREATE DATABASE SmartOrderDB;
--GO

USE SmartOrderDB;
GO

-- 1. Roles Table
CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);

-- 2. Users Table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    RoleID INT,
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);

-- 3. Customers Table
CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Address NVARCHAR(255),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- 4. Categories Table
CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

-- 5. Products Table
CREATE TABLE Products (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(100) NOT NULL,
    Price DECIMAL(10,2) NOT NULL,
    StockQuantity INT NOT NULL,
    CategoryID INT,
    Description NVARCHAR(255),
    ProductImage VARBINARY(MAX), -- Changed from NVARCHAR(255) to VARBINARY(MAX) to store image data directly
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID)
);

-- 6. Orders Table
CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    WaitingNumber INT,
    CustomerID INT,
    UserID INT,
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    QRCodeText NVARCHAR(255),
    Notes NVARCHAR(255),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);

-- 7. OrderItems Table
CREATE TABLE OrderItems (
    OrderItemID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT,
    ProductID INT,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    -- Computed column syntax for SQL Server
    Subtotal AS (Quantity * UnitPrice) PERSISTED, 
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);

-- 8. Invoices Table
CREATE TABLE Invoices (
    InvoiceID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT UNIQUE, -- 1:1 relationship with Orders
    InvoiceDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(10,2) NOT NULL,
    InvoiceFile VARBINARY(MAX), -- Renamed from FilePath and changed to VARBINARY(MAX) to store file data directly
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);

-- 9. Payments Table
CREATE TABLE Payments (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    InvoiceID INT UNIQUE, -- 1:1 relationship with Invoices
    PaymentMethod NVARCHAR(50), -- e.g., 'Cash', 'ABA', 'Card'
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InvoiceID) REFERENCES Invoices(InvoiceID)
);

-- 10. OrderLogs Table
CREATE TABLE OrderLogs (
    LogID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT,
    Action NVARCHAR(100) NOT NULL,
    ActionDate DATETIME DEFAULT GETDATE(),
    PerformedBy NVARCHAR(100),
    FOREIGN KEY (OrderID) REFERENCES Orders(OrderID)
);
GO