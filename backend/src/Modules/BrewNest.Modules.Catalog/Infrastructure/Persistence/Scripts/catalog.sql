IF OBJECT_ID(N'[catalog].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'catalog') IS NULL EXEC(N'CREATE SCHEMA [catalog];');
    CREATE TABLE [catalog].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    IF SCHEMA_ID(N'catalog') IS NULL EXEC(N'CREATE SCHEMA [catalog];');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[Customers] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Customers] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[DiningTables] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Capacity] int NOT NULL,
        [Area] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_DiningTables] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[ItemGroups] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [AppliesTo] nvarchar(30) NULL,
        [RequiresInspection] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ItemGroups] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[Riders] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [VehicleRegistration] nvarchar(50) NOT NULL,
        [IsAvailable] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Riders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[StatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] uniqueidentifier NOT NULL,
        [PreviousStatus] nvarchar(30) NOT NULL,
        [NewStatus] nvarchar(30) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ChangedBy] nvarchar(100) NOT NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StatusHistory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[UnitsOfMeasure] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Symbol] nvarchar(20) NOT NULL,
        [Category] nvarchar(50) NOT NULL,
        [DecimalPlaces] int NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_UnitsOfMeasure] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[Vendors] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [ContactPerson] nvarchar(200) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [TaxRegistrationNumber] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Vendors] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[Warehouses] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Warehouses] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[Items] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [ItemType] nvarchar(30) NOT NULL,
        [UnitOfMeasureId] uniqueidentifier NOT NULL,
        [ItemGroupId] uniqueidentifier NULL,
        [SalesPrice] decimal(18,2) NULL,
        [StandardCost] decimal(18,2) NOT NULL,
        [ShelfLifeDays] int NULL,
        [RequiresInspection] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Items] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Items_ItemGroups_ItemGroupId] FOREIGN KEY ([ItemGroupId]) REFERENCES [catalog].[ItemGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Items_UnitsOfMeasure_UnitOfMeasureId] FOREIGN KEY ([UnitOfMeasureId]) REFERENCES [catalog].[UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[InspectionRequirements] (
        [Id] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NULL,
        [ItemGroupId] uniqueidentifier NULL,
        [Name] nvarchar(200) NOT NULL,
        [IsRequired] bit NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_InspectionRequirements] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InspectionRequirements_ItemGroups_ItemGroupId] FOREIGN KEY ([ItemGroupId]) REFERENCES [catalog].[ItemGroups] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_InspectionRequirements_Items_ItemId] FOREIGN KEY ([ItemId]) REFERENCES [catalog].[Items] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[Recipes] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(30) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [MenuItemId] uniqueidentifier NOT NULL,
        [YieldQuantity] decimal(18,3) NOT NULL,
        [YieldUnitOfMeasureId] uniqueidentifier NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Recipes] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Recipes_Items_MenuItemId] FOREIGN KEY ([MenuItemId]) REFERENCES [catalog].[Items] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Recipes_UnitsOfMeasure_YieldUnitOfMeasureId] FOREIGN KEY ([YieldUnitOfMeasureId]) REFERENCES [catalog].[UnitsOfMeasure] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[InspectionCriteria] (
        [Id] uniqueidentifier NOT NULL,
        [InspectionRequirementId] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [ResultType] nvarchar(20) NOT NULL,
        [MinimumValue] nvarchar(100) NULL,
        [MaximumValue] nvarchar(100) NULL,
        [Unit] nvarchar(30) NULL,
        [IsMandatory] bit NOT NULL,
        [Sequence] int NOT NULL,
        [AddedAt] datetimeoffset NOT NULL,
        [AddedBy] nvarchar(100) NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        [RevokedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_InspectionCriteria] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_InspectionCriteria_InspectionRequirements_InspectionRequirementId] FOREIGN KEY ([InspectionRequirementId]) REFERENCES [catalog].[InspectionRequirements] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE TABLE [catalog].[RecipeLines] (
        [Id] uniqueidentifier NOT NULL,
        [RecipeId] uniqueidentifier NOT NULL,
        [IngredientItemId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [WastagePercentage] decimal(5,2) NOT NULL,
        [Sequence] int NOT NULL,
        [AddedAt] datetimeoffset NOT NULL,
        [AddedBy] nvarchar(100) NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        [RevokedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RecipeLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RecipeLines_Items_IngredientItemId] FOREIGN KEY ([IngredientItemId]) REFERENCES [catalog].[Items] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RecipeLines_Recipes_RecipeId] FOREIGN KEY ([RecipeId]) REFERENCES [catalog].[Recipes] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Customers_Code] ON [catalog].[Customers] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_DiningTables_Code] ON [catalog].[DiningTables] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InspectionCriteria_InspectionRequirementId_Sequence] ON [catalog].[InspectionCriteria] ([InspectionRequirementId], [Sequence]) WHERE [RevokedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InspectionRequirements_ItemGroupId] ON [catalog].[InspectionRequirements] ([ItemGroupId]) WHERE [ItemGroupId] IS NOT NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_InspectionRequirements_ItemId] ON [catalog].[InspectionRequirements] ([ItemId]) WHERE [ItemId] IS NOT NULL AND [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ItemGroups_Code] ON [catalog].[ItemGroups] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Items_Code] ON [catalog].[Items] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE INDEX [IX_Items_ItemGroupId] ON [catalog].[Items] ([ItemGroupId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE INDEX [IX_Items_UnitOfMeasureId] ON [catalog].[Items] ([UnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE INDEX [IX_RecipeLines_IngredientItemId] ON [catalog].[RecipeLines] ([IngredientItemId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RecipeLines_RecipeId_Sequence] ON [catalog].[RecipeLines] ([RecipeId], [Sequence]) WHERE [RevokedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Recipes_Code] ON [catalog].[Recipes] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_Recipes_MenuItemId] ON [catalog].[Recipes] ([MenuItemId]) WHERE [IsActive] = 1');
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE INDEX [IX_Recipes_YieldUnitOfMeasureId] ON [catalog].[Recipes] ([YieldUnitOfMeasureId]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Riders_Code] ON [catalog].[Riders] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE INDEX [IX_StatusHistory_EntityType_EntityId_ChangedAt] ON [catalog].[StatusHistory] ([EntityType], [EntityId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_UnitsOfMeasure_Code] ON [catalog].[UnitsOfMeasure] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Vendors_Code] ON [catalog].[Vendors] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Warehouses_Code] ON [catalog].[Warehouses] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [catalog].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920163609_InitialCatalog'
)
BEGIN
    INSERT INTO [catalog].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920163609_InitialCatalog', N'10.0.12');
END;

COMMIT;
GO
