USE SmartOrderDB;
GO

BEGIN TRANSACTION;

BEGIN TRY

    -- 1. Populate Roles Table
    INSERT INTO Roles (RoleName)
    VALUES 
    (N'Administrator'),
    (N'Manager'),
    (N'Cashier');

    -- 2. Populate Categories Table
    INSERT INTO Categories (CategoryName, Description)
    VALUES 
    (N'Beverages', N'Hot and cold drinks including coffee, tea, and soda'),
    (N'Bakery', N'Freshly baked bread, pastries, and cakes'),
    (N'Merchandise', N'Branded cups, shirts, and accessories');

    -- 3. Populate Customers Table
    INSERT INTO Customers (CustomerName, Phone, Email, Address)
    VALUES 
    (N'Walk-in Customer', NULL, NULL, NULL),
    (N'John Doe', N'+1234567890', N'john.doe@example.com', N'123 Main Street'),
    (N'Jane Smith', N'+1987654321', N'jane.smith@example.com', N'456 Oak Avenue');

    -- 4. Populate Users Table (Requires RoleID)
    -- RoleID 1 = Administrator, 2 = Manager, 3 = Cashier
    INSERT INTO Users (Username, Password, FullName, Email, Phone, RoleID)
    VALUES 
    (N'admin', N'hashed_password_123', N'System Administrator', N'admin@cafe.com', N'0123456789', 1),
    (N'manager1', N'hashed_password_456', N'Alice Johnson', N'alice@cafe.com', N'0123456780', 2),
    (N'cashier1', N'hashed_password_789', N'Bob Smith', N'bob@cafe.com', N'0123456781', 3);

    -- 5. Populate Products Table (Requires CategoryID and loads Image Files)
    -- CategoryID 1 = Beverages, 2 = Bakery, 3 = Merchandise
    -- Adjust 'C:\SampleFiles\...' to actual paths on your SQL Server machine if needed.
    
    INSERT INTO Products (ProductName, Price, StockQuantity, CategoryID, Description, ProductImage)
    SELECT N'Espresso', 3.50, 100, 1, N'Rich double shot espresso', BulkColumn 
    FROM OPENROWSET(BULK N'C:\Users\ASUS\Downloads\SmartOrder\espresso.png', SINGLE_BLOB) AS ImageFile;

    INSERT INTO Products (ProductName, Price, StockQuantity, CategoryID, Description, ProductImage)
    SELECT N'Croissant', 2.75, 30, 2, N'Butter croissant', BulkColumn 
    FROM OPENROWSET(BULK N'C:\Users\ASUS\Downloads\SmartOrder\croissant.png', SINGLE_BLOB) AS ImageFile;

    INSERT INTO Products (ProductName, Price, StockQuantity, CategoryID, Description, ProductImage)
    SELECT N'Ceramic Mug', 12.00, 50, 3, N'Branded 12oz ceramic mug', BulkColumn 
    FROM OPENROWSET(BULK N'C:\Users\ASUS\Downloads\SmartOrder\mug.png', SINGLE_BLOB) AS ImageFile;


    -- 6. Populate Orders Table (Requires CustomerID and UserID)
    -- Order 1: Walk-in (CustomerID 1), Cashier (UserID 3) -> Total 6.25
    -- Order 2: John Doe (CustomerID 2), Cashier (UserID 3) -> Total 14.75
    -- Order 3: Jane Smith (CustomerID 3), Manager (UserID 2) -> Total 24.00
    INSERT INTO Orders (WaitingNumber, CustomerID, UserID, TotalAmount, QRCodeText, Notes)
    VALUES 
    (101, 1, 3, 6.25, N'QR_ORDER_101', N'No sugar in Espresso'),
    (102, 2, 3, 14.75, N'QR_ORDER_102', N'Takeaway'),
    (103, 3, 2, 24.00, N'QR_ORDER_103', N'For dine-in');


    -- 7. Populate OrderItems Table (Requires OrderID and ProductID)
    -- Subtotal is a computed column, so it is omitted from the insert statements.
    
    -- Order 1 Items (1x Espresso @ 3.50, 1x Croissant @ 2.75 = 6.25)
    INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice)
    VALUES 
    (1, 1, 1, 3.50), 
    (1, 2, 1, 2.75);

    -- Order 2 Items (1x Croissant @ 2.75, 1x Ceramic Mug @ 12.00 = 14.75)
    INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice)
    VALUES 
    (2, 2, 1, 2.75), 
    (2, 3, 1, 12.00);

    -- Order 3 Items (2x Ceramic Mug @ 12.00 = 24.00)
    INSERT INTO OrderItems (OrderID, ProductID, Quantity, UnitPrice)
    VALUES 
    (3, 3, 2, 12.00);


    -- 8. Populate Invoices Table (Requires OrderID and loads PDF Files)
    -- Adjust 'C:\SampleFiles\...' to actual paths on your SQL Server machine if needed.
    
    INSERT INTO Invoices (OrderID, TotalAmount, InvoiceFile)
    SELECT 1, 6.25, BulkColumn 
    FROM OPENROWSET(BULK N'C:\Users\ASUS\Downloads\cafe\invoice_1.pdf', SINGLE_BLOB) AS FileData;

    INSERT INTO Invoices (OrderID, TotalAmount, InvoiceFile)
    SELECT 2, 14.75, BulkColumn 
    FROM OPENROWSET(BULK N'C:\Users\ASUS\Downloads\cafe\invoice_2.pdf', SINGLE_BLOB) AS FileData;

    INSERT INTO Invoices (OrderID, TotalAmount, InvoiceFile)
    SELECT 3, 24.00, BulkColumn 
    FROM OPENROWSET(BULK N'C:\Users\ASUS\Downloads\cafe\invoice_3.pdf', SINGLE_BLOB) AS FileData;


    -- 9. Populate Payments Table (Requires InvoiceID)
    INSERT INTO Payments (InvoiceID, PaymentMethod, Amount)
    VALUES 
    (1, N'Cash', 6.25),
    (2, N'ABA', 14.75),
    (3, N'Card', 24.00);


    -- 10. Populate OrderLogs Table (Requires OrderID)
    INSERT INTO OrderLogs (OrderID, Action, PerformedBy)
    VALUES 
    (1, N'Order Created', N'Bob Smith'),
    (1, N'Payment Received', N'System'),
    (2, N'Order Created', N'Bob Smith'),
    (2, N'Payment Received', N'System'),
    (3, N'Order Created', N'Alice Johnson'),
    (3, N'Payment Received', N'System');

    COMMIT TRANSACTION;
    PRINT 'Data populated successfully.';

END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;
    PRINT 'An error occurred. Transaction rolled back.';
    THROW;
END CATCH;
GO