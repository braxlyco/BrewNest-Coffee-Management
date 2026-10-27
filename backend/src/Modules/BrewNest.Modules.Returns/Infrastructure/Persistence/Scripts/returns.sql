IF OBJECT_ID(N'[returns].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'returns') IS NULL EXEC(N'CREATE SCHEMA [returns];');
    CREATE TABLE [returns].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    IF SCHEMA_ID(N'returns') IS NULL EXEC(N'CREATE SCHEMA [returns];');
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE TABLE [returns].[CreditNotes] (
        [Id] uniqueidentifier NOT NULL,
        [ReturnRequestId] uniqueidentifier NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_CreditNotes] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE TABLE [returns].[ReturnRequests] (
        [Id] uniqueidentifier NOT NULL,
        [RmaNumber] nvarchar(40) NOT NULL,
        [SalesOrderId] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(40) NOT NULL,
        [BillId] uniqueidentifier NOT NULL,
        [BillNumber] nvarchar(40) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [CreditNoteNumber] nvarchar(40) NULL,
        [RefundReference] nvarchar(100) NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ReturnRequests] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE TABLE [returns].[StatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [DocumentType] nvarchar(50) NOT NULL,
        [DocumentId] uniqueidentifier NOT NULL,
        [PreviousStatus] nvarchar(30) NOT NULL,
        [NewStatus] nvarchar(30) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [ChangedBy] nvarchar(200) NOT NULL,
        [ChangedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_StatusHistory] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE TABLE [returns].[VoidRequests] (
        [Id] uniqueidentifier NOT NULL,
        [SalesOrderId] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(40) NOT NULL,
        [BillNumber] nvarchar(40) NULL,
        [IsAfterPayment] bit NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [RefundReference] nvarchar(100) NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VoidRequests] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE TABLE [returns].[ReturnRequestLines] (
        [Id] uniqueidentifier NOT NULL,
        [ReturnRequestId] uniqueidentifier NOT NULL,
        [SalesLineId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [ItemCode] nvarchar(80) NOT NULL,
        [ItemName] nvarchar(200) NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [IsSealed] bit NOT NULL,
        [IsPackagingIntact] bit NOT NULL,
        [ManufacturingDateChecked] bit NOT NULL,
        [ManufacturingDate] date NULL,
        [ExpiryDate] date NULL,
        [LotNumber] nvarchar(100) NULL,
        [WarehouseId] uniqueidentifier NULL,
        [BatchVerified] bit NOT NULL,
        [QualityPassed] bit NOT NULL,
        [Disposition] nvarchar(30) NOT NULL,
        [Inspector] nvarchar(200) NULL,
        [InspectedAt] datetimeoffset NULL,
        [Criterion] nvarchar(200) NULL,
        [Result] nvarchar(500) NULL,
        [Remarks] nvarchar(500) NULL,
        [SupportingReference] nvarchar(500) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_ReturnRequestLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ReturnRequestLines_ReturnRequests_ReturnRequestId] FOREIGN KEY ([ReturnRequestId]) REFERENCES [returns].[ReturnRequests] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE TABLE [returns].[ApprovalDecisions] (
        [Id] uniqueidentifier NOT NULL,
        [ReturnRequestId] uniqueidentifier NULL,
        [VoidRequestId] uniqueidentifier NULL,
        [Sequence] int NOT NULL,
        [RequiredRole] nvarchar(100) NOT NULL,
        [Decision] nvarchar(30) NOT NULL,
        [DecidedBy] nvarchar(200) NULL,
        [DecidedAt] datetimeoffset NULL,
        [Remarks] nvarchar(500) NULL,
        CONSTRAINT [PK_ApprovalDecisions] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_ReturnApproval_OneOwner] CHECK (([ReturnRequestId] IS NULL AND [VoidRequestId] IS NOT NULL) OR ([ReturnRequestId] IS NOT NULL AND [VoidRequestId] IS NULL)),
        CONSTRAINT [FK_ApprovalDecisions_ReturnRequests_ReturnRequestId] FOREIGN KEY ([ReturnRequestId]) REFERENCES [returns].[ReturnRequests] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_ApprovalDecisions_VoidRequests_VoidRequestId] FOREIGN KEY ([VoidRequestId]) REFERENCES [returns].[VoidRequests] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE INDEX [IX_ApprovalDecisions_ReturnRequestId] ON [returns].[ApprovalDecisions] ([ReturnRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE INDEX [IX_ApprovalDecisions_VoidRequestId] ON [returns].[ApprovalDecisions] ([VoidRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CreditNotes_Number] ON [returns].[CreditNotes] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_CreditNotes_ReturnRequestId] ON [returns].[CreditNotes] ([ReturnRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE INDEX [IX_ReturnRequestLines_ReturnRequestId] ON [returns].[ReturnRequestLines] ([ReturnRequestId]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReturnRequests_Number] ON [returns].[ReturnRequests] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_ReturnRequests_RmaNumber] ON [returns].[ReturnRequests] ([RmaNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VoidRequests_Number] ON [returns].[VoidRequests] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [returns].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926114926_InitialReturns'
)
BEGIN
    INSERT INTO [returns].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926114926_InitialReturns', N'10.0.12');
END;

COMMIT;
GO
