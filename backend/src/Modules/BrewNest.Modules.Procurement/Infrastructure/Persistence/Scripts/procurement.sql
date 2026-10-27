IF OBJECT_ID(N'[procurement].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'procurement') IS NULL EXEC(N'CREATE SCHEMA [procurement];');
    CREATE TABLE [procurement].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    IF SCHEMA_ID(N'procurement') IS NULL EXEC(N'CREATE SCHEMA [procurement];');
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[PurchaseOrders] (
        [Id] uniqueidentifier NOT NULL,
        [RequisitionId] uniqueidentifier NOT NULL,
        [QuotationId] uniqueidentifier NOT NULL,
        [VendorId] uniqueidentifier NOT NULL,
        [VendorName] nvarchar(200) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PurchaseOrders] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[PurchaseRequisitions] (
        [Id] uniqueidentifier NOT NULL,
        [Department] nvarchar(100) NOT NULL,
        [RequiredOn] date NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_PurchaseRequisitions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[StatusHistory] (
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
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[VendorReturns] (
        [Id] uniqueidentifier NOT NULL,
        [GoodsReceiptId] uniqueidentifier NOT NULL,
        [Reason] nvarchar(500) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VendorReturns] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[ApprovalDecisions] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseOrderId] uniqueidentifier NOT NULL,
        [Sequence] int NOT NULL,
        [RequiredRole] nvarchar(100) NOT NULL,
        [Decision] nvarchar(30) NOT NULL,
        [DecidedBy] nvarchar(200) NULL,
        [DecidedAt] datetimeoffset NULL,
        [Remarks] nvarchar(500) NULL,
        CONSTRAINT [PK_ApprovalDecisions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ApprovalDecisions_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [procurement].[PurchaseOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[GoodsReceipts] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseOrderId] uniqueidentifier NOT NULL,
        [SupplierReference] nvarchar(100) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_GoodsReceipts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoodsReceipts_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [procurement].[PurchaseOrders] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[PurchaseOrderLines] (
        [Id] uniqueidentifier NOT NULL,
        [PurchaseOrderId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [ReceivedQuantity] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_PurchaseOrderLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseOrderLines_PurchaseOrders_PurchaseOrderId] FOREIGN KEY ([PurchaseOrderId]) REFERENCES [procurement].[PurchaseOrders] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[PurchaseRequisitionLines] (
        [Id] uniqueidentifier NOT NULL,
        [RequisitionId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [EstimatedUnitPrice] decimal(18,2) NOT NULL,
        [Notes] nvarchar(500) NOT NULL,
        CONSTRAINT [PK_PurchaseRequisitionLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PurchaseRequisitionLines_PurchaseRequisitions_RequisitionId] FOREIGN KEY ([RequisitionId]) REFERENCES [procurement].[PurchaseRequisitions] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[RequestsForQuotation] (
        [Id] uniqueidentifier NOT NULL,
        [RequisitionId] uniqueidentifier NOT NULL,
        [ResponseDeadline] date NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RequestsForQuotation] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RequestsForQuotation_PurchaseRequisitions_RequisitionId] FOREIGN KEY ([RequisitionId]) REFERENCES [procurement].[PurchaseRequisitions] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[VendorReturnLines] (
        [Id] uniqueidentifier NOT NULL,
        [VendorReturnId] uniqueidentifier NOT NULL,
        [GoodsReceiptLineId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        CONSTRAINT [PK_VendorReturnLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VendorReturnLines_VendorReturns_VendorReturnId] FOREIGN KEY ([VendorReturnId]) REFERENCES [procurement].[VendorReturns] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[GoodsReceiptLines] (
        [Id] uniqueidentifier NOT NULL,
        [GoodsReceiptId] uniqueidentifier NOT NULL,
        [PurchaseOrderLineId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [WarehouseId] uniqueidentifier NOT NULL,
        [LotNumber] nvarchar(100) NOT NULL,
        [ManufacturingDate] date NULL,
        [ExpiryDate] date NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [Inspection] nvarchar(30) NOT NULL,
        CONSTRAINT [PK_GoodsReceiptLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_GoodsReceiptLines_GoodsReceipts_GoodsReceiptId] FOREIGN KEY ([GoodsReceiptId]) REFERENCES [procurement].[GoodsReceipts] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[RfqVendors] (
        [Id] uniqueidentifier NOT NULL,
        [RfqId] uniqueidentifier NOT NULL,
        [VendorId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_RfqVendors] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RfqVendors_RequestsForQuotation_RfqId] FOREIGN KEY ([RfqId]) REFERENCES [procurement].[RequestsForQuotation] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[VendorQuotations] (
        [Id] uniqueidentifier NOT NULL,
        [RfqId] uniqueidentifier NOT NULL,
        [VendorId] uniqueidentifier NOT NULL,
        [VendorName] nvarchar(200) NOT NULL,
        [ValidUntil] date NOT NULL,
        [Reference] nvarchar(100) NOT NULL,
        [Status] nvarchar(30) NOT NULL,
        [Number] nvarchar(40) NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(200) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(200) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_VendorQuotations] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VendorQuotations_RequestsForQuotation_RfqId] FOREIGN KEY ([RfqId]) REFERENCES [procurement].[RequestsForQuotation] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[QualityInspectionResults] (
        [Id] uniqueidentifier NOT NULL,
        [GoodsReceiptLineId] uniqueidentifier NOT NULL,
        [CriterionId] uniqueidentifier NOT NULL,
        [Criterion] nvarchar(200) NOT NULL,
        [Result] nvarchar(500) NOT NULL,
        [Passed] bit NOT NULL,
        [Remarks] nvarchar(500) NOT NULL,
        [SupportingReference] nvarchar(500) NOT NULL,
        [Inspector] nvarchar(200) NOT NULL,
        [InspectedAt] datetimeoffset NOT NULL,
        CONSTRAINT [PK_QualityInspectionResults] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_QualityInspectionResults_GoodsReceiptLines_GoodsReceiptLineId] FOREIGN KEY ([GoodsReceiptLineId]) REFERENCES [procurement].[GoodsReceiptLines] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE TABLE [procurement].[VendorQuotationLines] (
        [Id] uniqueidentifier NOT NULL,
        [QuotationId] uniqueidentifier NOT NULL,
        [ItemId] uniqueidentifier NOT NULL,
        [Quantity] decimal(18,3) NOT NULL,
        [UnitPrice] decimal(18,2) NOT NULL,
        [LeadTimeDays] int NOT NULL,
        CONSTRAINT [PK_VendorQuotationLines] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_VendorQuotationLines_VendorQuotations_QuotationId] FOREIGN KEY ([QuotationId]) REFERENCES [procurement].[VendorQuotations] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_ApprovalDecisions_PurchaseOrderId] ON [procurement].[ApprovalDecisions] ([PurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_GoodsReceiptLines_GoodsReceiptId] ON [procurement].[GoodsReceiptLines] ([GoodsReceiptId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_GoodsReceipts_Number] ON [procurement].[GoodsReceipts] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_GoodsReceipts_PurchaseOrderId] ON [procurement].[GoodsReceipts] ([PurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_PurchaseOrderLines_PurchaseOrderId] ON [procurement].[PurchaseOrderLines] ([PurchaseOrderId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseOrders_Number] ON [procurement].[PurchaseOrders] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_PurchaseRequisitionLines_RequisitionId] ON [procurement].[PurchaseRequisitionLines] ([RequisitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PurchaseRequisitions_Number] ON [procurement].[PurchaseRequisitions] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_QualityInspectionResults_GoodsReceiptLineId] ON [procurement].[QualityInspectionResults] ([GoodsReceiptLineId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RequestsForQuotation_Number] ON [procurement].[RequestsForQuotation] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_RequestsForQuotation_RequisitionId] ON [procurement].[RequestsForQuotation] ([RequisitionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RfqVendors_RfqId_VendorId] ON [procurement].[RfqVendors] ([RfqId], [VendorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_VendorQuotationLines_QuotationId] ON [procurement].[VendorQuotationLines] ([QuotationId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VendorQuotations_Number] ON [procurement].[VendorQuotations] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_VendorQuotations_RfqId] ON [procurement].[VendorQuotations] ([RfqId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE INDEX [IX_VendorReturnLines_VendorReturnId] ON [procurement].[VendorReturnLines] ([VendorReturnId]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    CREATE UNIQUE INDEX [IX_VendorReturns_Number] ON [procurement].[VendorReturns] ([Number]);
END;

IF NOT EXISTS (
    SELECT * FROM [procurement].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926112229_InitialProcurement'
)
BEGIN
    INSERT INTO [procurement].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926112229_InitialProcurement', N'10.0.12');
END;

COMMIT;
GO
