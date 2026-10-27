IF OBJECT_ID(N'[identity].[__EFMigrationsHistory]') IS NULL
BEGIN
    IF SCHEMA_ID(N'identity') IS NULL EXEC(N'CREATE SCHEMA [identity];');
    CREATE TABLE [identity].[__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    IF SCHEMA_ID(N'identity') IS NULL EXEC(N'CREATE SCHEMA [identity];');
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[Permissions] (
        [Id] uniqueidentifier NOT NULL,
        [Name] nvarchar(150) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Group] nvarchar(100) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_Permissions] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[Roles] (
        [Id] uniqueidentifier NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[SecurityAuditEvents] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NULL,
        [EventType] nvarchar(80) NOT NULL,
        [Succeeded] bit NOT NULL,
        [OccurredAt] datetimeoffset NOT NULL,
        [Actor] nvarchar(200) NOT NULL,
        [IpAddress] nvarchar(64) NULL,
        [UserAgent] nvarchar(500) NULL,
        [Details] nvarchar(1000) NOT NULL,
        CONSTRAINT [PK_SecurityAuditEvents] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[Users] (
        [Id] uniqueidentifier NOT NULL,
        [FullName] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedBy] nvarchar(100) NOT NULL,
        [ModifiedAt] datetimeoffset NULL,
        [ModifiedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[RoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_RoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[RolePermissions] (
        [Id] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [PermissionId] uniqueidentifier NOT NULL,
        [GrantedAt] datetimeoffset NOT NULL,
        [GrantedBy] nvarchar(100) NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        [RevokedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RolePermissions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RolePermissions_Permissions_PermissionId] FOREIGN KEY ([PermissionId]) REFERENCES [identity].[Permissions] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_RolePermissions_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[RefreshTokens] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [FamilyId] uniqueidentifier NOT NULL,
        [TokenHash] nvarchar(128) NOT NULL,
        [ExpiresAt] datetimeoffset NOT NULL,
        [CreatedAt] datetimeoffset NOT NULL,
        [CreatedByIp] nvarchar(64) NULL,
        [RevokedAt] datetimeoffset NULL,
        [RevokedByIp] nvarchar(64) NULL,
        [RevocationReason] nvarchar(200) NULL,
        [ReplacedByTokenId] uniqueidentifier NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[UserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] uniqueidentifier NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_UserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[UserLogins] (
        [LoginProvider] nvarchar(450) NOT NULL,
        [ProviderKey] nvarchar(450) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[UserRoleAssignments] (
        [Id] uniqueidentifier NOT NULL,
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        [AssignedAt] datetimeoffset NOT NULL,
        [AssignedBy] nvarchar(100) NOT NULL,
        [RevokedAt] datetimeoffset NULL,
        [RevokedBy] nvarchar(100) NULL,
        [RowVersion] rowversion NOT NULL,
        CONSTRAINT [PK_UserRoleAssignments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRoleAssignments_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_UserRoleAssignments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[UserRoles] (
        [UserId] uniqueidentifier NOT NULL,
        [RoleId] uniqueidentifier NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [identity].[Roles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE TABLE [identity].[UserTokens] (
        [UserId] uniqueidentifier NOT NULL,
        [LoginProvider] nvarchar(450) NOT NULL,
        [Name] nvarchar(450) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [identity].[Users] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''10000000-0000-0000-0000-000000000001'', N''identity.users.read'', N''View users and their active roles.'', N''Users'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration''),
    (''10000000-0000-0000-0000-000000000002'', N''identity.users.manage'', N''Create, update, disable, and reset users.'', N''Users'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration''),
    (''10000000-0000-0000-0000-000000000003'', N''identity.roles.read'', N''View roles and their permission grants.'', N''Roles'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration''),
    (''10000000-0000-0000-0000-000000000004'', N''identity.roles.manage'', N''Create and update authorization roles.'', N''Roles'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration''),
    (''10000000-0000-0000-0000-000000000005'', N''identity.permissions.read'', N''View the permission catalog.'', N''Permissions'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration''),
    (''10000000-0000-0000-0000-000000000006'', N''identity.permissions.manage'', N''Grant and revoke role permissions.'', N''Permissions'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration''),
    (''10000000-0000-0000-0000-000000000007'', N''identity.audit.read'', N''View Identity security audit history.'', N''Audit'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''initial-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Permissions_Name] ON [identity].[Permissions] ([Name]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE UNIQUE INDEX [IX_RefreshTokens_TokenHash] ON [identity].[RefreshTokens] ([TokenHash]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_RefreshTokens_UserId_FamilyId] ON [identity].[RefreshTokens] ([UserId], [FamilyId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_RoleClaims_RoleId] ON [identity].[RoleClaims] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_RolePermissions_PermissionId] ON [identity].[RolePermissions] ([PermissionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_RolePermissions_RoleId_PermissionId] ON [identity].[RolePermissions] ([RoleId], [PermissionId]) WHERE [RevokedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [identity].[Roles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_SecurityAuditEvents_UserId_OccurredAt] ON [identity].[SecurityAuditEvents] ([UserId], [OccurredAt]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_UserClaims_UserId] ON [identity].[UserClaims] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_UserLogins_UserId] ON [identity].[UserLogins] ([UserId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_UserRoleAssignments_RoleId] ON [identity].[UserRoleAssignments] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_UserRoleAssignments_UserId_RoleId] ON [identity].[UserRoleAssignments] ([UserId], [RoleId]) WHERE [RevokedAt] IS NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [identity].[UserRoles] ([RoleId]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [identity].[Users] ([NormalizedEmail]);
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [identity].[Users] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920111642_InitialIdentity'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920111642_InitialIdentity', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920170000_AddAdministrationPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''11000000-0000-0000-0000-000000000001'', N''administration.settings.read'', N''View company, branch, fiscal, numbering, alert, and allocation settings.'', N''Administration'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''administration-migration''),
    (''11000000-0000-0000-0000-000000000002'', N''administration.settings.manage'', N''Create and update operational configuration.'', N''Administration'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''administration-migration''),
    (''11000000-0000-0000-0000-000000000003'', N''administration.approvals.read'', N''View configurable approval routes and value tiers.'', N''Approvals'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''administration-migration''),
    (''11000000-0000-0000-0000-000000000004'', N''administration.approvals.manage'', N''Create, update, activate, and deactivate approval routes.'', N''Approvals'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''administration-migration''),
    (''11000000-0000-0000-0000-000000000005'', N''administration.audit.read'', N''View the Administration configuration audit history.'', N''Audit'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''administration-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920170000_AddAdministrationPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920170000_AddAdministrationPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920200000_AddCatalogPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''12000000-0000-0000-0000-000000000001'', N''catalog.masterdata.read'', N''View Catalog master data.'', N''Catalog'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''catalog-migration''),
    (''12000000-0000-0000-0000-000000000002'', N''catalog.masterdata.manage'', N''Create and update Catalog master data.'', N''Catalog'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''catalog-migration''),
    (''12000000-0000-0000-0000-000000000003'', N''catalog.recipes.read'', N''View recipes and bills of materials.'', N''Recipes'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''catalog-migration''),
    (''12000000-0000-0000-0000-000000000004'', N''catalog.recipes.manage'', N''Create and revise recipes and bills of materials.'', N''Recipes'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''catalog-migration''),
    (''12000000-0000-0000-0000-000000000005'', N''catalog.quality.read'', N''View inspection requirements and criteria.'', N''Quality'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''catalog-migration''),
    (''12000000-0000-0000-0000-000000000006'', N''catalog.quality.manage'', N''Create and revise inspection requirements and criteria.'', N''Quality'', CAST(1 AS bit), ''2026-09-20T00:00:00.0000000+00:00'', N''catalog-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260920200000_AddCatalogPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260920200000_AddCatalogPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923200000_AddInventoryPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''13000000-0000-0000-0000-000000000001'', N''inventory.stock.read'', N''View lots, balances, and stock movements.'', N''Inventory'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''inventory-migration''),
    (''13000000-0000-0000-0000-000000000002'', N''inventory.stock.manage'', N''Receive, issue, block, and unblock stock.'', N''Inventory'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''inventory-migration''),
    (''13000000-0000-0000-0000-000000000003'', N''inventory.reservations.manage'', N''Create and release stock reservations.'', N''Inventory'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''inventory-migration''),
    (''13000000-0000-0000-0000-000000000004'', N''inventory.adjustments.manage'', N''Create stock adjustments.'', N''Inventory'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''inventory-migration''),
    (''13000000-0000-0000-0000-000000000005'', N''inventory.wastage.manage'', N''Record stock wastage.'', N''Inventory'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''inventory-migration''),
    (''13000000-0000-0000-0000-000000000006'', N''inventory.expiry.read'', N''View expiry alerts.'', N''Inventory'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''inventory-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923200000_AddInventoryPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923200000_AddInventoryPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923300000_AddSalesPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''14000000-0000-0000-0000-000000000001'', N''sales.orders.read'', N''View sales orders, bills, and status history.'', N''Sales'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''sales-migration''),
    (''14000000-0000-0000-0000-000000000002'', N''sales.orders.manage'', N''Create and amend dine-in, takeaway, and delivery orders.'', N''Sales'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''sales-migration''),
    (''14000000-0000-0000-0000-000000000003'', N''sales.discounts.request'', N''Request a discount for manager approval.'', N''Sales'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''sales-migration''),
    (''14000000-0000-0000-0000-000000000004'', N''sales.discounts.approve'', N''Approve or reject requested discounts.'', N''Sales'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''sales-migration''),
    (''14000000-0000-0000-0000-000000000005'', N''sales.billing.manage'', N''Confirm orders and issue final bills.'', N''Sales'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''sales-migration''),
    (''14000000-0000-0000-0000-000000000006'', N''sales.orders.close'', N''Close or cancel sales orders.'', N''Sales'', CAST(1 AS bit), ''2026-09-23T00:00:00.0000000+00:00'', N''sales-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260923300000_AddSalesPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260923300000_AddSalesPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddKitchenPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''15000000-0000-0000-0000-000000000001'', N''kitchen.queue.read'', N''View KOTs, preparation queues, and status history.'', N''Kitchen'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''kitchen-migration''),
    (''15000000-0000-0000-0000-000000000002'', N''kitchen.tickets.generate'', N''Generate initial and additional KOTs from confirmed Sales orders.'', N''Kitchen'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''kitchen-migration''),
    (''15000000-0000-0000-0000-000000000003'', N''kitchen.preparation.manage'', N''Start preparation and mark KOT lines ready.'', N''Kitchen'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''kitchen-migration''),
    (''15000000-0000-0000-0000-000000000004'', N''kitchen.tickets.cancel'', N''Cancel queued KOTs and release their ingredient reservations.'', N''Kitchen'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''kitchen-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924090000_AddKitchenPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924090000_AddKitchenPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924190000_AddProductionPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''16000000-0000-0000-0000-000000000001'', N''production.read'', N''View production plans, orders, requirements, and variance.'', N''Production'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''production-migration''),
    (''16000000-0000-0000-0000-000000000002'', N''production.plans.manage'', N''Create, release, and cancel production plans.'', N''Production'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''production-migration''),
    (''16000000-0000-0000-0000-000000000003'', N''production.orders.release'', N''Release or cancel production orders and reservations.'', N''Production'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''production-migration''),
    (''16000000-0000-0000-0000-000000000004'', N''production.materials.issue'', N''Issue production materials and record actual consumption and wastage.'', N''Production'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''production-migration''),
    (''16000000-0000-0000-0000-000000000005'', N''production.completion.manage'', N''Complete production orders.'', N''Production'', CAST(1 AS bit), ''2026-09-24T00:00:00.0000000+00:00'', N''production-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260924190000_AddProductionPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260924190000_AddProductionPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926090000_AddProcurementPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''17000000-0000-0000-0000-000000000001'', N''procurement.read'', N''View procurement data.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000002'', N''procurement.requisitions.manage'', N''Manage purchase requisitions.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000003'', N''procurement.requisitions.approve'', N''Approve purchase requisitions.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000004'', N''procurement.sourcing.manage'', N''Manage RFQs and quotations.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000005'', N''procurement.orders.manage'', N''Create purchase orders.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000006'', N''procurement.orders.approve'', N''Approve purchase orders.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000007'', N''procurement.inspections.manage'', N''Perform receipt inspections.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000008'', N''procurement.receipts.manage'', N''Manage goods receipts.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration''),
    (''17000000-0000-0000-0000-000000000009'', N''procurement.returns.manage'', N''Manage vendor returns.'', N''Procurement'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''procurement-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926090000_AddProcurementPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926090000_AddProcurementPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926150000_AddReturnsPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''18000000-0000-0000-0000-000000000001'', N''returns.read'', N''View returns and voids.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration''),
    (''18000000-0000-0000-0000-000000000002'', N''returns.manage'', N''Create return requests.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration''),
    (''18000000-0000-0000-0000-000000000003'', N''returns.approve'', N''Approve return requests.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration''),
    (''18000000-0000-0000-0000-000000000004'', N''returns.inspect'', N''Inspect returned items.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration''),
    (''18000000-0000-0000-0000-000000000005'', N''returns.voids.manage'', N''Create void requests.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration''),
    (''18000000-0000-0000-0000-000000000006'', N''returns.voids.approve'', N''Approve void requests.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration''),
    (''18000000-0000-0000-0000-000000000007'', N''returns.refunds.manage'', N''Submit refund requests.'', N''Returns'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''returns-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926150000_AddReturnsPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926150000_AddReturnsPermissions', N'10.0.12');
END;

COMMIT;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926160000_AddPaymentsPermissions'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] ON;
    EXEC(N'INSERT INTO [identity].[Permissions] ([Id], [Name], [Description], [Group], [IsActive], [CreatedAt], [CreatedBy])
    VALUES (''19000000-0000-0000-0000-000000000001'', N''payments.read'', N''View payments and settlement history.'', N''Payments'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''payments-migration''),
    (''19000000-0000-0000-0000-000000000002'', N''payments.create'', N''Record customer payments.'', N''Payments'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''payments-migration''),
    (''19000000-0000-0000-0000-000000000003'', N''payments.verify'', N''Verify or reject pending payments.'', N''Payments'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''payments-migration''),
    (''19000000-0000-0000-0000-000000000004'', N''payments.settle'', N''Settle verified payments.'', N''Payments'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''payments-migration''),
    (''19000000-0000-0000-0000-000000000005'', N''payments.refunds.read'', N''View refund requests.'', N''Payments'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''payments-migration''),
    (''19000000-0000-0000-0000-000000000006'', N''payments.refunds.process'', N''Process or reject refund requests.'', N''Payments'', CAST(1 AS bit), ''2026-09-26T00:00:00.0000000+00:00'', N''payments-migration'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Name', N'Description', N'Group', N'IsActive', N'CreatedAt', N'CreatedBy') AND [object_id] = OBJECT_ID(N'[identity].[Permissions]'))
        SET IDENTITY_INSERT [identity].[Permissions] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [identity].[__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260926160000_AddPaymentsPermissions'
)
BEGIN
    INSERT INTO [identity].[__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260926160000_AddPaymentsPermissions', N'10.0.12');
END;

COMMIT;
GO
