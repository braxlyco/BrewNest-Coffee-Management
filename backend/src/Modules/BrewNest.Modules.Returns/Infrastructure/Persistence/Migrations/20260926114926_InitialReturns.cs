using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrewNest.Modules.Returns.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialReturns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "returns");

            migrationBuilder.CreateTable(
                name: "CreditNotes",
                schema: "returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Number = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditNotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequests",
                schema: "returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RmaNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BillId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CreditNoteNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    RefundReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Number = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StatusHistory",
                schema: "returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DocumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PreviousStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VoidRequests",
                schema: "returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    BillNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    IsAfterPayment = table.Column<bool>(type: "bit", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RefundReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Number = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoidRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnRequestLines",
                schema: "returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemCode = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsSealed = table.Column<bool>(type: "bit", nullable: false),
                    IsPackagingIntact = table.Column<bool>(type: "bit", nullable: false),
                    ManufacturingDateChecked = table.Column<bool>(type: "bit", nullable: false),
                    ManufacturingDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ExpiryDate = table.Column<DateOnly>(type: "date", nullable: true),
                    LotNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    WarehouseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    BatchVerified = table.Column<bool>(type: "bit", nullable: false),
                    QualityPassed = table.Column<bool>(type: "bit", nullable: false),
                    Disposition = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Inspector = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InspectedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Criterion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Result = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SupportingReference = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnRequestLines_ReturnRequests_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalSchema: "returns",
                        principalTable: "ReturnRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ApprovalDecisions",
                schema: "returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VoidRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RequiredRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DecidedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DecidedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApprovalDecisions", x => x.Id);
                    table.CheckConstraint("CK_ReturnApproval_OneOwner", "([ReturnRequestId] IS NULL AND [VoidRequestId] IS NOT NULL) OR ([ReturnRequestId] IS NOT NULL AND [VoidRequestId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_ApprovalDecisions_ReturnRequests_ReturnRequestId",
                        column: x => x.ReturnRequestId,
                        principalSchema: "returns",
                        principalTable: "ReturnRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ApprovalDecisions_VoidRequests_VoidRequestId",
                        column: x => x.VoidRequestId,
                        principalSchema: "returns",
                        principalTable: "VoidRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDecisions_ReturnRequestId",
                schema: "returns",
                table: "ApprovalDecisions",
                column: "ReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ApprovalDecisions_VoidRequestId",
                schema: "returns",
                table: "ApprovalDecisions",
                column: "VoidRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_Number",
                schema: "returns",
                table: "CreditNotes",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditNotes_ReturnRequestId",
                schema: "returns",
                table: "CreditNotes",
                column: "ReturnRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequestLines_ReturnRequestId",
                schema: "returns",
                table: "ReturnRequestLines",
                column: "ReturnRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_Number",
                schema: "returns",
                table: "ReturnRequests",
                column: "Number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReturnRequests_RmaNumber",
                schema: "returns",
                table: "ReturnRequests",
                column: "RmaNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VoidRequests_Number",
                schema: "returns",
                table: "VoidRequests",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApprovalDecisions",
                schema: "returns");

            migrationBuilder.DropTable(
                name: "CreditNotes",
                schema: "returns");

            migrationBuilder.DropTable(
                name: "ReturnRequestLines",
                schema: "returns");

            migrationBuilder.DropTable(
                name: "StatusHistory",
                schema: "returns");

            migrationBuilder.DropTable(
                name: "VoidRequests",
                schema: "returns");

            migrationBuilder.DropTable(
                name: "ReturnRequests",
                schema: "returns");
        }
    }
}
