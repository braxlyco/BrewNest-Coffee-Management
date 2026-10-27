IF OBJECT_ID(N'[inventory].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'inventory') IS NULL EXEC(N'CREATE SCHEMA [inventory];');
    CREATE TABLE [inventory].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    IF SCHEMA_ID(N'inventory') IS NULL EXEC(N'CREATE SCHEMA [inventory];');
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE TABLE [inventory].[StockLots] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [WarehouseId] uniqueidentifier NOT NULL,
        [LotNumber] nvarchar(100) NOT NULL,
        [ManufacturingDate] date NULL,
        [ExpiryDate] date NULL,
        [OnHandQuantity] decimal(18,3) NOT NULL,
        [ReservedQuantity] decimal(18,3) NOT NULL,
        [IsBlocked] bit NOT NULL,
        [BlockReason] nvarchar(500) NULL,
        [BlockedAt] datetimeoffset NULL,
        [BlockedBy] nvarchar(100) NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_StockLots] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_StockLots_Dates] CHECK ([ManufacturingDate] IS NULL OR [ExpiryDate] IS NULL OR [ManufacturingDate] <= [ExpiryDate]),
        CONSTRAINT [CK_StockLots_OnHand_Nonnegative] CHECK ([OnHandQuantity] >= 0),
        CONSTRAINT [CK_StockLots_Reserved_Nonnegative] CHECK ([ReservedQuantity] >= 0),
        CONSTRAINT [CK_StockLots_Reserved_NotOverOnHand] CHECK ([ReservedQuantity] <= [OnHandQuantity])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE TABLE [inventory].[StockMovements] (
        [Id] uniqueidentifier NOT NULL,
        [StockLotId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [WarehouseId] uniqueidentifier NOT NULL,
        [MovementType] nvarchar(40) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [OnHandAfter] decimal(18,3) NOT NULL,
        [ReservedAfter] decimal(18,3) NOT NULL,
        [ReferenceType] nvarchar(80) NOT NULL,
        [ReferenceNumber] nvarchar(100) NOT NULL,
        [Remarks] nvarchar(500) NOT NULL,
        [PerformedBy] nvarchar(100) NOT NULL,
        [PerformedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StockMovements] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE TABLE [inventory].[Reservations] (
        [Id] uniqueidentifier NOT NULL,
        [StockLotId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [WarehouseId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [ReleasedQuantity] decimal(18,3) NOT NULL,
        [ConsumedQuantity] decimal(18,3) NOT NULL,
        [ReferenceType] nvarchar(80) NOT NULL,
        [ReferenceId] nvarchar(100) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [ExpiresAt] datetimeoffset NULL,
        [Remarks] nvarchar(500) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Reservations] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Reservations_Consumed_Nonnegative] CHECK ([ConsumedQuantity] >= 0),
        CONSTRAINT [CK_Reservations_Quantity_Positive] CHECK ([Quantity] > 0),
        CONSTRAINT [CK_Reservations_Released_Nonnegative] CHECK ([ReleasedQuantity] >= 0),
        CONSTRAINT [CK_Reservations_Remaining] CHECK ([ReleasedQuantity] + [ConsumedQuantity] <= [Quantity]),
        CONSTRAINT [FK_Reservations_StockLots_StockLotId] FOREIGN KEY ([StockLotId]) REFERENCES [inventory].[StockLots] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE INDEX [IX_Reservations_ReferenceType_ReferenceId] ON [inventory].[Reservations] ([ReferenceType], [ReferenceId]);
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE INDEX [IX_Reservations_StockLotId_Status] ON [inventory].[Reservations] ([StockLotId], [Status]);
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE INDEX [IX_StockLots_ExpiryDate] ON [inventory].[StockLots] ([ExpiryDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE UNIQUE INDEX [IX_StockLots_ItemId_WarehouseId_LotNumber] ON [inventory].[StockLots] ([ItemId], [WarehouseId], [LotNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE INDEX [IX_StockMovements_ItemId_WarehouseId_PerformedAt] ON [inventory].[StockMovements] ([ItemId], [WarehouseId], [PerformedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    CREATE INDEX [IX_StockMovements_StockLotId_PerformedAt] ON [inventory].[StockMovements] ([StockLotId], [PerformedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [inventory].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923155930_InitialInventory'
)
BEGIN
    INSERT INTO [inventory].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923155930_InitialInventory', N'10.0.12');
END;

COMMIT;
GO
