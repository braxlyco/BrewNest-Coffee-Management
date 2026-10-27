using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BrewNest.Modules.Kitchen.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialKitchen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "kitchen");

            migrationBuilder.CreateTable(
                name: "StatusHistory",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PreviousStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatusHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tickets",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    SalesOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesOrderNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tickets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReservationLinks",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InventoryReservationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IngredientItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservationLinks", x => x.Id);
                    table.CheckConstraint("CK_ReservationLinks_Quantity_Positive", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_ReservationLinks_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "kitchen",
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TicketLines",
                schema: "kitchen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TicketId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SalesOrderLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MenuItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PreparationStartedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ReadyAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLines", x => x.Id);
                    table.CheckConstraint("CK_TicketLines_Quantity_Positive", "[Quantity] > 0");
                    table.ForeignKey(
                        name: "FK_TicketLines_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalSchema: "kitchen",
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReservationLinks_InventoryReservationId",
                schema: "kitchen",
                table: "ReservationLinks",
                column: "InventoryReservationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservationLinks_TicketId",
                schema: "kitchen",
                table: "ReservationLinks",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_StatusHistory_TicketId_ChangedAt",
                schema: "kitchen",
                table: "StatusHistory",
                columns: new[] { "TicketId", "ChangedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_SalesOrderLineId",
                schema: "kitchen",
                table: "TicketLines",
                column: "SalesOrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketLines_TicketId",
                schema: "kitchen",
                table: "TicketLines",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_SalesOrderId_Sequence",
                schema: "kitchen",
                table: "Tickets",
                columns: new[] { "SalesOrderId", "Sequence" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_Status_CreatedAt",
                schema: "kitchen",
                table: "Tickets",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_TicketNumber",
                schema: "kitchen",
                table: "Tickets",
                column: "TicketNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReservationLinks",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "StatusHistory",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "TicketLines",
                schema: "kitchen");

            migrationBuilder.DropTable(
                name: "Tickets",
                schema: "kitchen");
        }
    }
}
