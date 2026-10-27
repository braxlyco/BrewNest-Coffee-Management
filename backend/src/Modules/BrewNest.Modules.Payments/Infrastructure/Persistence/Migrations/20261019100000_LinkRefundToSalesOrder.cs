using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BrewNest.Modules.Payments.Infrastructure.Persistence.Migrations;

/// <summary>Associates new refund requests with their Sales order for amount control and gateway routing.</summary>
[DbContext(typeof(PaymentsDbContext))]
[Migration("20261019100000_LinkRefundToSalesOrder")]
public sealed class LinkRefundToSalesOrder : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "SalesOrderId", schema: "payments", table: "Refunds",
            type: "uniqueidentifier", nullable: true);
        migrationBuilder.Sql("UPDATE r SET r.SalesOrderId = p.SalesOrderId FROM [payments].[Refunds] r JOIN [payments].[Payments] p ON r.PaymentId = p.Id WHERE r.SalesOrderId IS NULL");
        migrationBuilder.CreateIndex(
            name: "IX_Refunds_SalesOrderId", schema: "payments", table: "Refunds", column: "SalesOrderId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(name: "IX_Refunds_SalesOrderId", schema: "payments", table: "Refunds");
        migrationBuilder.DropColumn(name: "SalesOrderId", schema: "payments", table: "Refunds");
    }
}
