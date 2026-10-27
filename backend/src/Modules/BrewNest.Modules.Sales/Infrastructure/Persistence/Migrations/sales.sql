IF OBJECT_ID(N'[sales].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'sales') IS NULL EXEC(N'CREATE SCHEMA [sales];');
    CREATE TABLE [sales].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    IF SCHEMA_ID(N'sales') IS NULL EXEC(N'CREATE SCHEMA [sales];');
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE TABLE [sales].[Orders] (
        [Id] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(40) NOT NULL,
        [OrderType] nvarchar(20) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CustomerId] uniqueidentifier NULL,
        [DiningTableId] uniqueidentifier NULL,
        [CustomerName] nvarchar(200) NOT NULL,
        [CustomerPhone] nvarchar(50) NOT NULL,
        [DeliveryAddress] nvarchar(500) NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE TABLE [sales].[StatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [PreviousStatus] nvarchar(20) NOT NULL,
        [NewStatus] nvarchar(20) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ChangedBy] nvarchar(100) NOT NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StatusHistory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE TABLE [sales].[Bills] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [BillNumber] nvarchar(40) NOT NULL,
        [GrossAmount] decimal(18,2) NOT NULL,
        [DiscountAmount] decimal(18,2) NOT NULL,
        [NetAmount] decimal(18,2) NOT NULL,
        [IssuedAt] datetimeoffset NOT NULL,
        [IssuedBy] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Bills] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Bills_DiscountAmount] CHECK ([DiscountAmount] >= 0 AND [DiscountAmount] <= [GrossAmount]),
        CONSTRAINT [CK_Bills_GrossAmount] CHECK ([GrossAmount] >= 0),
        CONSTRAINT [CK_Bills_NetAmount] CHECK ([NetAmount] = [GrossAmount] - [DiscountAmount]),
        CONSTRAINT [FK_Bills_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [sales].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE TABLE [sales].[Discounts] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [Percentage] decimal(5,2) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [RequestedBy] nvarchar(100) NOT NULL,
        [RequestedAt] datetimeoffset NOT NULL,
        [ApprovedBy] nvarchar(100) NULL,
        [ApprovedAt] datetimeoffset NULL,
        [DecisionRemarks] nvarchar(500) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Discounts] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Discounts_Amount] CHECK ([Amount] >= 0),
        CONSTRAINT [CK_Discounts_Percentage] CHECK ([Percentage] > 0 AND [Percentage] <= 100),
        CONSTRAINT [FK_Discounts_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [sales].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE TABLE [sales].[OrderLines] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [ItemCode] nvarchar(30) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [SpecialInstructions] nvarchar(500) NOT NULL,
        [IsCancelled] bit NOT NULL,
        [CancellationReason] nvarchar(500) NULL,
        [AddedAt] datetimeoffset NOT NULL,
        [AddedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_OrderLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_OrderLines_Quantity_Positive] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_OrderLines_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [sales].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bills_BillNumber] ON [sales].[Bills] ([BillNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Bills_OrderId] ON [sales].[Bills] ([OrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE INDEX [IX_Discounts_OrderId_Status] ON [sales].[Discounts] ([OrderId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE INDEX [IX_OrderLines_OrderId_AddedAt] ON [sales].[OrderLines] ([OrderId], [AddedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Orders_DiningTableId] ON [sales].[Orders] ([DiningTableId]) WHERE [DiningTableId] IS NOT NULL AND [Status] IN (''Draft'',''Confirmed'',''Billed'')');
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [sales].[Orders] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE INDEX [IX_Orders_Status_CreatedAt] ON [sales].[Orders] ([Status], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    CREATE INDEX [IX_StatusHistory_OrderId_ChangedAt] ON [sales].[StatusHistory] ([OrderId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [sales].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923173618_InitialSales'
)
BEGIN
    INSERT INTO [sales].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923173618_InitialSales', N'10.0.12');
END;

COMMIT;
GO
