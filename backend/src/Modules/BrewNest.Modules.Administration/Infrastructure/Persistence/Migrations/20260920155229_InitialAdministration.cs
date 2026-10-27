using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BrewNest.Modules.Administration.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "administration");

            migrationBuilder.CreateTable(
                name: "AlertSettings",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DaysBefore = table.Column<int>(type: "int", nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalPolicies",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProcessCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    MaximumAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ApprovalMode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Actor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CompanySettings",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LegalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TradingName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TaxRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    TimeZoneId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FiscalYears",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StartsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    EndsOn = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiscalYears", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalSteps",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovalPolicyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GrantedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    GrantedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RevokedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalSteps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApprovalSteps_ApprovalPolicies_ApprovalPolicyId",
                        column: x => x.ApprovalPolicyId,
                        principalSchema: "administration",
                        principalTable: "ApprovalPolicies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DocumentNumberSequences",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Prefix = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NextNumber = table.Column<long>(type: "bigint", nullable: false),
                    Padding = table.Column<int>(type: "int", nullable: false),
                    Suffix = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ResetPolicy = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentNumberSequences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentNumberSequences_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "administration",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseAllocationSettings",
                schema: "administration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FixedPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseAllocationSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseAllocationSettings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalSchema: "administration",
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "administration",
                table: "AlertSettings",
                columns: new[] { "Id", "Code", "CreatedAt", "CreatedBy", "DaysBefore", "IsActive", "ModifiedAt", "ModifiedBy", "Name", "Severity" },
                values: new object[,]
                {
                    { new Guid("23000000-0000-0000-0000-000000000001"), "ExpiryFirst", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", 15, true, null, null, "First expiry alert", "Information" },
                    { new Guid("23000000-0000-0000-0000-000000000002"), "ExpiryEscalated", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", 7, true, null, null, "Escalated expiry alert", "Escalated" },
                    { new Guid("23000000-0000-0000-0000-000000000003"), "ExpiryCritical", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", 3, true, null, null, "Critical expiry alert", "Critical" }
                });

            migrationBuilder.InsertData(
                schema: "administration",
                table: "ApprovalPolicies",
                columns: new[] { "Id", "ApprovalMode", "CreatedAt", "CreatedBy", "IsActive", "MaximumAmount", "MinimumAmount", "ModifiedAt", "ModifiedBy", "Name", "ProcessCode" },
                values: new object[,]
                {
                    { new Guid("21000000-0000-0000-0000-000000000001"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Discount approval", "Discount" },
                    { new Guid("21000000-0000-0000-0000-000000000002"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Void before payment", "VoidBeforePayment" },
                    { new Guid("21000000-0000-0000-0000-000000000003"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Void after payment", "VoidAfterPayment" },
                    { new Guid("21000000-0000-0000-0000-000000000004"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Refund approval", "Refund" },
                    { new Guid("21000000-0000-0000-0000-000000000005"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Purchase requisition", "PurchaseRequisition" },
                    { new Guid("21000000-0000-0000-0000-000000000006"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Returned-food restocking", "ReturnedFoodRestocking" },
                    { new Guid("21000000-0000-0000-0000-000000000007"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Stock adjustment", "StockAdjustment" },
                    { new Guid("21000000-0000-0000-0000-000000000008"), "Any", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, null, null, null, "Wastage approval", "Wastage" },
                    { new Guid("21000000-0000-0000-0000-000000000009"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, 25000m, 0m, null, null, "PO up to PKR 25,000", "PurchaseOrder" },
                    { new Guid("21000000-0000-0000-0000-000000000010"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, 100000m, 25000.01m, null, null, "PO PKR 25,001 to PKR 100,000", "PurchaseOrder" },
                    { new Guid("21000000-0000-0000-0000-000000000011"), "All", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", true, null, 100000.01m, null, null, "PO above PKR 100,000", "PurchaseOrder" }
                });

            migrationBuilder.InsertData(
                schema: "administration",
                table: "CompanySettings",
                columns: new[] { "Id", "Address", "CreatedAt", "CreatedBy", "CurrencyCode", "Email", "LegalName", "ModifiedAt", "ModifiedBy", "Phone", "TaxRegistrationNumber", "TimeZoneId", "TradingName" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), "", new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "PKR", "", "BrewNest Cafe", null, null, "", "", "Asia/Karachi", "BrewNest" });

            migrationBuilder.InsertData(
                schema: "administration",
                table: "ApprovalSteps",
                columns: new[] { "Id", "ApprovalPolicyId", "GrantedAt", "GrantedBy", "RequiredRole", "RevokedAt", "RevokedBy", "Sequence" },
                values: new object[,]
                {
                    { new Guid("22000000-0000-0000-0000-000000000001"), new Guid("21000000-0000-0000-0000-000000000001"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000002"), new Guid("21000000-0000-0000-0000-000000000002"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Sales Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000003"), new Guid("21000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Sales Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000004"), new Guid("21000000-0000-0000-0000-000000000003"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Finance", null, null, 2 },
                    { new Guid("22000000-0000-0000-0000-000000000005"), new Guid("21000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Sales Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000006"), new Guid("21000000-0000-0000-0000-000000000004"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Finance", null, null, 2 },
                    { new Guid("22000000-0000-0000-0000-000000000007"), new Guid("21000000-0000-0000-0000-000000000005"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Requesting Department Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000008"), new Guid("21000000-0000-0000-0000-000000000006"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Store/QC Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000009"), new Guid("21000000-0000-0000-0000-000000000007"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Store Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000010"), new Guid("21000000-0000-0000-0000-000000000008"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Kitchen Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000011"), new Guid("21000000-0000-0000-0000-000000000008"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Store Manager", null, null, 2 },
                    { new Guid("22000000-0000-0000-0000-000000000012"), new Guid("21000000-0000-0000-0000-000000000009"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Purchasing Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000013"), new Guid("21000000-0000-0000-0000-000000000010"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Purchasing Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000014"), new Guid("21000000-0000-0000-0000-000000000010"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Finance Manager", null, null, 2 },
                    { new Guid("22000000-0000-0000-0000-000000000015"), new Guid("21000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Purchasing Manager", null, null, 1 },
                    { new Guid("22000000-0000-0000-0000-000000000016"), new Guid("21000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Finance Manager", null, null, 2 },
                    { new Guid("22000000-0000-0000-0000-000000000017"), new Guid("21000000-0000-0000-0000-000000000011"), new DateTimeOffset(new DateTime(2026, 9, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "initial-migration", "Owner/Admin", null, null, 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertSettings_Code",
                schema: "administration",
                table: "AlertSettings",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalPolicies_ProcessCode_MinimumAmount_MaximumAmount",
                schema: "administration",
                table: "ApprovalPolicies",
                columns: new[] { "ProcessCode", "MinimumAmount", "MaximumAmount" },
                unique: true,
                filter: "[MinimumAmount] IS NOT NULL AND [MaximumAmount] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalSteps_ApprovalPolicyId_Sequence",
                schema: "administration",
                table: "ApprovalSteps",
                columns: new[] { "ApprovalPolicyId", "Sequence" },
                unique: true,
                filter: "[RevokedAt] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId_OccurredAt",
                schema: "administration",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Branches_Code",
                schema: "administration",
                table: "Branches",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberSequences_BranchId",
                schema: "administration",
                table: "DocumentNumberSequences",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentNumberSequences_DocumentType_BranchId",
                schema: "administration",
                table: "DocumentNumberSequences",
                columns: new[] { "DocumentType", "BranchId" },
                unique: true,
                filter: "[BranchId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseAllocationSettings_BranchId",
                schema: "administration",
                table: "ExpenseAllocationSettings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FiscalYears_Name",
                schema: "administration",
                table: "FiscalYears",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertSettings",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "ApprovalSteps",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "AuditLogs",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "CompanySettings",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "DocumentNumberSequences",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "ExpenseAllocationSettings",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "FiscalYears",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "ApprovalPolicies",
                schema: "administration");

            migrationBuilder.DropTable(
                name: "Branches",
                schema: "administration");
        }
    }
}
