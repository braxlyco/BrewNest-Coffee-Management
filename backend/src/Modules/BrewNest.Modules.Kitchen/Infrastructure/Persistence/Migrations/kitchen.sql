IF OBJECT_ID(N'[kitchen].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'kitchen') IS NULL EXEC(N'CREATE SCHEMA [kitchen];');
    CREATE TABLE [kitchen].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    IF SCHEMA_ID(N'kitchen') IS NULL EXEC(N'CREATE SCHEMA [kitchen];');
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE TABLE [kitchen].[StatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [LineId] uniqueidentifier NULL,
        [PreviousStatus] nvarchar(20) NOT NULL,
        [NewStatus] nvarchar(20) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ChangedBy] nvarchar(100) NOT NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StatusHistory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE TABLE [kitchen].[Tickets] (
        [Id] uniqueidentifier NOT NULL,
        [TicketNumber] nvarchar(40) NOT NULL,
        [SalesOrderId] uniqueidentifier NOT NULL,
        [SalesOrderNumber] nvarchar(40) NOT NULL,
        [Sequence] int NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Tickets] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE TABLE [kitchen].[ReservationLinks] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [InventoryReservationId] uniqueidentifier NOT NULL,
        [IngredientItemId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_ReservationLinks] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ReservationLinks_Quantity_Positive] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_ReservationLinks_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [kitchen].[Tickets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE TABLE [kitchen].[TicketLines] (
        [Id] uniqueidentifier NOT NULL,
        [TicketId] uniqueidentifier NOT NULL,
        [SalesOrderLineId] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [SpecialInstructions] nvarchar(500) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [PreparationStartedAt] datetimeoffset NULL,
        [ReadyAt] datetimeoffset NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_TicketLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_TicketLines_Quantity_Positive] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_TicketLines_Tickets_TicketId] FOREIGN KEY ([TicketId]) REFERENCES [kitchen].[Tickets] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReservationLinks_InventoryReservationId] ON [kitchen].[ReservationLinks] ([InventoryReservationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE INDEX [IX_ReservationLinks_TicketId] ON [kitchen].[ReservationLinks] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE INDEX [IX_StatusHistory_TicketId_ChangedAt] ON [kitchen].[StatusHistory] ([TicketId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE INDEX [IX_TicketLines_SalesOrderLineId] ON [kitchen].[TicketLines] ([SalesOrderLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE INDEX [IX_TicketLines_TicketId] ON [kitchen].[TicketLines] ([TicketId]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tickets_SalesOrderId_Sequence] ON [kitchen].[Tickets] ([SalesOrderId], [Sequence]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE INDEX [IX_Tickets_Status_CreatedAt] ON [kitchen].[Tickets] ([Status], [CreatedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Tickets_TicketNumber] ON [kitchen].[Tickets] ([TicketNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [kitchen].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924171727_InitialKitchen'
)
BEGIN
    INSERT INTO [kitchen].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924171727_InitialKitchen', N'10.0.12');
END;

COMMIT;
GO
