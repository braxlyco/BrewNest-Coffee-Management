IF OBJECT_ID(N'[payments].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'payments') IS NULL EXEC(N'CREATE SCHEMA [payments];');
    CREATE TABLE [payments].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    IF SCHEMA_ID(N'payments') IS NULL EXEC(N'CREATE SCHEMA [payments];');
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE TABLE [payments].[Payments] (
        [Id] uniqueidentifier NOT NULL,
        [SalesOrderId] uniqueidentifier NOT NULL,
        [OrderNumber] nvarchar(40) NOT NULL,
        [BillNumber] nvarchar(40) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Method] nvarchar(30) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [PayerReference] nvarchar(150) NOT NULL,
        [GatewayReference] nvarchar(150) NULL,
        [Notes] nvarchar(500) NOT NULL,
        [VerifiedAt] datetimeoffset NULL,
        [VerifiedBy] nvarchar(200) NULL,
        [SettledAt] datetimeoffset NULL,
        [SettledBy] nvarchar(200) NULL,
        [SettlementReference] nvarchar(150) NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Payments] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE TABLE [payments].[StatusHistory] (
        [Id] uniqueidentifier NOT NULL,
        [DocumentType] nvarchar(30) NOT NULL,
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
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE TABLE [payments].[Refunds] (
        [Id] uniqueidentifier NOT NULL,
        [SourceReference] nvarchar(100) NOT NULL,
        [PaymentId] uniqueidentifier NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [GatewayReference] nvarchar(150) NULL,
        [ProcessedAt] datetimeoffset NULL,
        [ProcessedBy] nvarchar(200) NULL,
        [Remarks] nvarchar(500) NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Refunds] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Refunds_Payments_PaymentId] FOREIGN KEY ([PaymentId]) REFERENCES [payments].[Payments] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Payments_Number] ON [payments].[Payments] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE INDEX [IX_Payments_SalesOrderId_BillNumber] ON [payments].[Payments] ([SalesOrderId], [BillNumber]);
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Refunds_Number] ON [payments].[Refunds] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE INDEX [IX_Refunds_PaymentId] ON [payments].[Refunds] ([PaymentId]);
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Refunds_SourceReference] ON [payments].[Refunds] ([SourceReference]);
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    CREATE INDEX [IX_StatusHistory_DocumentType_DocumentId_ChangedAt] ON [payments].[StatusHistory] ([DocumentType], [DocumentId], [ChangedAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926132359_InitialPayments'
)
BEGIN
    INSERT INTO [payments].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926132359_InitialPayments', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [payments].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261019100000_LinkRefundToSalesOrder'
)
BEGIN
    ALTER TABLE [payments].[Refunds] ADD [SalesOrderId] uniqueidentifier NULL;
    UPDATE r SET r.[SalesOrderId] = p.[SalesOrderId]
    FROM [payments].[Refunds] r
    INNER JOIN [payments].[Payments] p ON r.[PaymentId] = p.[Id]
    WHERE r.[SalesOrderId] IS NULL;
    CREATE INDEX [IX_Refunds_SalesOrderId] ON [payments].[Refunds] ([SalesOrderId]);
    INSERT INTO [payments].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261019100000_LinkRefundToSalesOrder', N'10.0.12');
END;
GO

COMMIT;
GO
