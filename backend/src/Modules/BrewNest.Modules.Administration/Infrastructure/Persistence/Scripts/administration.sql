IF OBJECT_ID(N'[administration].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'administration') IS NULL EXEC(N'CREATE SCHEMA [administration];');
    CREATE TABLE [administration].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    IF SCHEMA_ID(N'administration') IS NULL EXEC(N'CREATE SCHEMA [administration];');
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[AlertSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [DaysBefore] int NOT NULL,
        [Severity] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_AlertSettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[ApprovalPolicies] (
        [Id] uniqueidentifier NOT NULL,
        [ProcessCode] nvarchar(80) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [MinimumAmount] decimal(18,2) NULL,
        [MaximumAmount] decimal(18,2) NULL,
        [ApprovalMode] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ApprovalPolicies] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[AuditLogs] (
        [Id] uniqueidentifier NOT NULL,
        [EntityType] nvarchar(100) NOT NULL,
        [EntityId] nvarchar(100) NOT NULL,
        [Action] nvarchar(80) NOT NULL,
        [Actor] nvarchar(100) NOT NULL,
        [OccurredAt] datetimeoffset NOT NULL,
        [Reason] nvarchar(500) NULL,
        [BeforeJson] nvarchar(max) NULL,
        [AfterJson] nvarchar(max) NULL,
        CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[Branches] (
        [Id] uniqueidentifier NOT NULL,
        [Code] nvarchar(20) NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [TimeZoneId] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Branches] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[CompanySettings] (
        [Id] uniqueidentifier NOT NULL,
        [LegalName] nvarchar(200) NOT NULL,
        [TradingName] nvarchar(200) NOT NULL,
        [TaxRegistrationNumber] nvarchar(100) NOT NULL,
        [CurrencyCode] nvarchar(3) NOT NULL,
        [TimeZoneId] nvarchar(100) NOT NULL,
        [Address] nvarchar(500) NOT NULL,
        [Phone] nvarchar(50) NOT NULL,
        [Email] nvarchar(256) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CompanySettings] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[FiscalYears] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(100) NOT NULL,
        [StartsOn] date NOT NULL,
        [EndsOn] date NOT NULL,
        [Status] nvarchar(20) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_FiscalYears] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[ApprovalSteps] (
        [Id] uniqueidentifier NOT NULL,
        [ApprovalPolicyId] uniqueidentifier NOT NULL,
        [Sequence] int NOT NULL,
        [RequiredRole] nvarchar(100) NOT NULL,
        [GrantedAt] datetimeoffset NOT NULL,
        [GrantedBy] nvarchar(100) NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        [RevokedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ApprovalSteps] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApprovalSteps_ApprovalPolicies_ApprovalPolicyId] FOREIGN KEY ([ApprovalPolicyId]) REFERENCES [administration].[ApprovalPolicies] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[DocumentNumberSequences] (
        [Id] uniqueidentifier NOT NULL,
        [DocumentType] nvarchar(80) NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [Prefix] nvarchar(30) NOT NULL,
        [NextNumber] bigint NOT NULL,
        [Padding] int NOT NULL,
        [Suffix] nvarchar(30) NOT NULL,
        [ResetPolicy] nvarchar(20) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_DocumentNumberSequences] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DocumentNumberSequences_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [administration].[Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE TABLE [administration].[ExpenseAllocationSettings] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(200) NOT NULL,
        [Method] nvarchar(30) NOT NULL,
        [BranchId] uniqueidentifier NULL,
        [FixedPercentage] decimal(5,2) NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ExpenseAllocationSettings] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ExpenseAllocationSettings_Branches_BranchId] FOREIGN KEY ([BranchId]) REFERENCES [administration].[Branches] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'CreatedBy', N'DaysBefore', N'IsActive', N'ModifiedAt', N'ModifiedBy', N'Name', N'Severity') AND [object_id] = OBJECT_ID(N'[administration].[AlertSettings]'))
        SET IDENTITY_INSERT [administration].[AlertSettings] ON;
    EXEC(N'INSERT INTO [administration].[AlertSettings] ([Id], [Code], [CreatedAt], [CreatedBy], [DaysBefore], [IsActive], [ModifiedAt], [ModifiedBy], [Name], [Severity])
    VALUES (''23000000-0000-0000-0000-000000000001'', N''ExpiryFirst'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', 15, CAST(1 AS bit), NULL, NULL, N''First expiry alert'', N''Information''),
    (''23000000-0000-0000-0000-000000000002'', N''ExpiryEscalated'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', 7, CAST(1 AS bit), NULL, NULL, N''Escalated expiry alert'', N''Escalated''),
    (''23000000-0000-0000-0000-000000000003'', N''ExpiryCritical'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', 3, CAST(1 AS bit), NULL, NULL, N''Critical expiry alert'', N''Critical'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Code', N'CreatedAt', N'CreatedBy', N'DaysBefore', N'IsActive', N'ModifiedAt', N'ModifiedBy', N'Name', N'Severity') AND [object_id] = OBJECT_ID(N'[administration].[AlertSettings]'))
        SET IDENTITY_INSERT [administration].[AlertSettings] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApprovalMode', N'CreatedAt', N'CreatedBy', N'IsActive', N'MaximumAmount', N'MinimumAmount', N'ModifiedAt', N'ModifiedBy', N'Name', N'ProcessCode') AND [object_id] = OBJECT_ID(N'[administration].[ApprovalPolicies]'))
        SET IDENTITY_INSERT [administration].[ApprovalPolicies] ON;
    EXEC(N'INSERT INTO [administration].[ApprovalPolicies] ([Id], [ApprovalMode], [CreatedAt], [CreatedBy], [IsActive], [MaximumAmount], [MinimumAmount], [ModifiedAt], [ModifiedBy], [Name], [ProcessCode])
    VALUES (''21000000-0000-0000-0000-000000000001'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Discount approval'', N''Discount''),
    (''21000000-0000-0000-0000-000000000002'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Void before payment'', N''VoidBeforePayment''),
    (''21000000-0000-0000-0000-000000000003'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Void after payment'', N''VoidAfterPayment''),
    (''21000000-0000-0000-0000-000000000004'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Refund approval'', N''Refund''),
    (''21000000-0000-0000-0000-000000000005'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Purchase requisition'', N''PurchaseRequisition''),
    (''21000000-0000-0000-0000-000000000006'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Returned-food restocking'', N''ReturnedFoodRestocking''),
    (''21000000-0000-0000-0000-000000000007'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Stock adjustment'', N''StockAdjustment''),
    (''21000000-0000-0000-0000-000000000008'', N''Any'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, NULL, NULL, NULL, N''Wastage approval'', N''Wastage''),
    (''21000000-0000-0000-0000-000000000009'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), 25000.0, 0.0, NULL, NULL, N''PO up to PKR 25,000'', N''PurchaseOrder''),
    (''21000000-0000-0000-0000-000000000010'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), 100000.0, 25000.01, NULL, NULL, N''PO PKR 25,001 to PKR 100,000'', N''PurchaseOrder''),
    (''21000000-0000-0000-0000-000000000011'', N''All'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', CAST(1 AS bit), NULL, 100000.01, NULL, NULL, N''PO above PKR 100,000'', N''PurchaseOrder'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApprovalMode', N'CreatedAt', N'CreatedBy', N'IsActive', N'MaximumAmount', N'MinimumAmount', N'ModifiedAt', N'ModifiedBy', N'Name', N'ProcessCode') AND [object_id] = OBJECT_ID(N'[administration].[ApprovalPolicies]'))
        SET IDENTITY_INSERT [administration].[ApprovalPolicies] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'CreatedAt', N'CreatedBy', N'CurrencyCode', N'Email', N'LegalName', N'ModifiedAt', N'ModifiedBy', N'Phone', N'TaxRegistrationNumber', N'TimeZoneId', N'TradingName') AND [object_id] = OBJECT_ID(N'[administration].[CompanySettings]'))
        SET IDENTITY_INSERT [administration].[CompanySettings] ON;
    EXEC(N'INSERT INTO [administration].[CompanySettings] ([Id], [Address], [CreatedAt], [CreatedBy], [CurrencyCode], [Email], [LegalName], [ModifiedAt], [ModifiedBy], [Phone], [TaxRegistrationNumber], [TimeZoneId], [TradingName])
    VALUES (''20000000-0000-0000-0000-000000000001'', N'''', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''PKR'', N'''', N''BrewNest Cafe'', NULL, NULL, N'''', N'''', N''Asia/Karachi'', N''BrewNest'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Address', N'CreatedAt', N'CreatedBy', N'CurrencyCode', N'Email', N'LegalName', N'ModifiedAt', N'ModifiedBy', N'Phone', N'TaxRegistrationNumber', N'TimeZoneId', N'TradingName') AND [object_id] = OBJECT_ID(N'[administration].[CompanySettings]'))
        SET IDENTITY_INSERT [administration].[CompanySettings] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApprovalPolicyId', N'GrantedAt', N'GrantedBy', N'RequiredRole', N'RevokedAt', N'RevokedBy', N'Sequence') AND [object_id] = OBJECT_ID(N'[administration].[ApprovalSteps]'))
        SET IDENTITY_INSERT [administration].[ApprovalSteps] ON;
    EXEC(N'INSERT INTO [administration].[ApprovalSteps] ([Id], [ApprovalPolicyId], [GrantedAt], [GrantedBy], [RequiredRole], [RevokedAt], [RevokedBy], [Sequence])
    VALUES (''22000000-0000-0000-0000-000000000001'', ''21000000-0000-0000-0000-000000000001'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000002'', ''21000000-0000-0000-0000-000000000002'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Sales Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000003'', ''21000000-0000-0000-0000-000000000003'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Sales Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000004'', ''21000000-0000-0000-0000-000000000003'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Finance'', NULL, NULL, 2),
    (''22000000-0000-0000-0000-000000000005'', ''21000000-0000-0000-0000-000000000004'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Sales Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000006'', ''21000000-0000-0000-0000-000000000004'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Finance'', NULL, NULL, 2),
    (''22000000-0000-0000-0000-000000000007'', ''21000000-0000-0000-0000-000000000005'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Requesting Department Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000008'', ''21000000-0000-0000-0000-000000000006'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Store/QC Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000009'', ''21000000-0000-0000-0000-000000000007'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Store Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000010'', ''21000000-0000-0000-0000-000000000008'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Kitchen Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000011'', ''21000000-0000-0000-0000-000000000008'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Store Manager'', NULL, NULL, 2),
    (''22000000-0000-0000-0000-000000000012'', ''21000000-0000-0000-0000-000000000009'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Purchasing Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000013'', ''21000000-0000-0000-0000-000000000010'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Purchasing Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000014'', ''21000000-0000-0000-0000-000000000010'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Finance Manager'', NULL, NULL, 2),
    (''22000000-0000-0000-0000-000000000015'', ''21000000-0000-0000-0000-000000000011'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Purchasing Manager'', NULL, NULL, 1),
    (''22000000-0000-0000-0000-000000000016'', ''21000000-0000-0000-0000-000000000011'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Finance Manager'', NULL, NULL, 2),
    (''22000000-0000-0000-0000-000000000017'', ''21000000-0000-0000-0000-000000000011'', ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'', N''Owner/Admin'', NULL, NULL, 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'ApprovalPolicyId', N'GrantedAt', N'GrantedBy', N'RequiredRole', N'RevokedAt', N'RevokedBy', N'Sequence') AND [object_id] = OBJECT_ID(N'[administration].[ApprovalSteps]'))
        SET IDENTITY_INSERT [administration].[ApprovalSteps] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_AlertSettings_Code] ON [administration].[AlertSettings] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ApprovalPolicies_ProcessCode_MinimumAmount_MaximumAmount] ON [administration].[ApprovalPolicies] ([ProcessCode], [MinimumAmount], [MaximumAmount]) WHERE [MinimumAmount] IS NOT NULL AND [MaximumAmount] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_ApprovalSteps_ApprovalPolicyId_Sequence] ON [administration].[ApprovalSteps] ([ApprovalPolicyId], [Sequence]) WHERE [RevokedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE INDEX [IX_AuditLogs_EntityType_EntityId_OccurredAt] ON [administration].[AuditLogs] ([EntityType], [EntityId], [OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Branches_Code] ON [administration].[Branches] ([Code]);
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE INDEX [IX_DocumentNumberSequences_BranchId] ON [administration].[DocumentNumberSequences] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_DocumentNumberSequences_DocumentType_BranchId] ON [administration].[DocumentNumberSequences] ([DocumentType], [BranchId]) WHERE [BranchId] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE INDEX [IX_ExpenseAllocationSettings_BranchId] ON [administration].[ExpenseAllocationSettings] ([BranchId]);
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FiscalYears_Name] ON [administration].[FiscalYears] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [administration].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920155229_InitialAdministration'
)
BEGIN
    INSERT INTO [administration].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920155229_InitialAdministration', N'10.0.12');
END;

COMMIT;
GO
