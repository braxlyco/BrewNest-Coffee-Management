IF OBJECT_ID(N'[production].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'production') IS NULL EXEC(N'CREATE SCHEMA [production];');
    CREATE TABLE [production].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    IF SCHEMA_ID(N'production') IS NULL EXEC(N'CREATE SCHEMA [production];');
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE TABLE [production].[Plans] (
        [Id] uniqueidentifier NOT NULL,
        [PlanNumber] nvarchar(40) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [PlannedDate] date NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Plans] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE TABLE [production].[StatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [DocumentType] nvarchar(30) NOT NULL,
        [DocumentId] uniqueidentifier NOT NULL,
        [PreviousStatus] nvarchar(20) NOT NULL,
        [NewStatus] nvarchar(20) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ChangedBy] nvarchar(100) NOT NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StatusHistory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE TABLE [production].[Orders] (
        [Id] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(40) NOT NULL,
        [PlanId] uniqueidentifier NOT NULL,
        [PlanLineId] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [PlannedQuantity] decimal(18,3) NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [CompletedAt] datetimeoffset NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProductionOrders_PlannedQuantity] CHECK ([PlannedQuantity] > 0),
        CONSTRAINT [FK_Orders_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [production].[Plans] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE TABLE [production].[PlanLines] (
        [Id] uniqueidentifier NOT NULL,
        [PlanId] uniqueidentifier NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_PlanLines] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProductionPlanLines_Quantity] CHECK ([Quantity] > 0),
        CONSTRAINT [FK_PlanLines_Plans_PlanId] FOREIGN KEY ([PlanId]) REFERENCES [production].[Plans] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE TABLE [production].[MaterialRequirements] (
        [Id] uniqueidentifier NOT NULL,
        [OrderId] uniqueidentifier NOT NULL,
        [IngredientItemId] uniqueidentifier NOT NULL,
        [BomQuantity] decimal(18,3) NOT NULL,
        [ExpectedWastage] decimal(18,3) NOT NULL,
        [RequiredQuantity] decimal(18,3) NOT NULL,
        [ActualConsumption] decimal(18,3) NOT NULL,
        [ActualWastage] decimal(18,3) NOT NULL,
        [Variance] decimal(18,3) NOT NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_MaterialRequirements] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ProductionRequirements_Quantities] CHECK ([BomQuantity] >= 0 AND [ExpectedWastage] >= 0 AND [RequiredQuantity] > 0 AND [ActualConsumption] >= 0 AND [ActualWastage] >= 0),
        CONSTRAINT [FK_MaterialRequirements_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [production].[Orders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE UNIQUE INDEX [IX_MaterialRequirements_OrderId_IngredientItemId] ON [production].[MaterialRequirements] ([OrderId], [IngredientItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_OrderNumber] ON [production].[Orders] ([OrderNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE INDEX [IX_Orders_PlanId] ON [production].[Orders] ([PlanId]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Orders_PlanLineId] ON [production].[Orders] ([PlanLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PlanLines_PlanId_MenuItemId] ON [production].[PlanLines] ([PlanId], [MenuItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Plans_PlanNumber] ON [production].[Plans] ([PlanNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE INDEX [IX_Plans_Status_PlannedDate] ON [production].[Plans] ([Status], [PlannedDate]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    CREATE INDEX [IX_StatusHistory_DocumentType_DocumentId_ChangedAt] ON [production].[StatusHistory] ([DocumentType], [DocumentId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [production].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924173739_InitialProduction'
)
BEGIN
    INSERT INTO [production].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924173739_InitialProduction', N'10.0.12');
END;

COMMIT;
GO
