using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using PropertyBill.Api.Data;

#nullable disable

namespace PropertyBill.Api.Migrations;

[DbContext(typeof(AppDbContext))]
[Migration("20260725040000_AddAdditionalCharges")]
public partial class AddAdditionalCharges : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AdditionalCharges",
            columns: table => new
            {
                AdditionalChargeId = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                UnitId = table.Column<int>(type: "integer", nullable: false),
                BillId = table.Column<int>(type: "integer", nullable: true),
                BillingPeriod = table.Column<string>(type: "text", nullable: false),
                Category = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                Amount = table.Column<decimal>(type: "numeric", nullable: false),
                Source = table.Column<string>(type: "text", nullable: false),
                Status = table.Column<string>(type: "text", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AdditionalCharges", x => x.AdditionalChargeId);
                table.ForeignKey("FK_AdditionalCharges_Bills_BillId", x => x.BillId, "Bills", "BillId", onDelete: ReferentialAction.SetNull);
                table.ForeignKey("FK_AdditionalCharges_Units_UnitId", x => x.UnitId, "Units", "UnitId", onDelete: ReferentialAction.Restrict);
            });
        migrationBuilder.CreateIndex(name: "IX_AdditionalCharges_BillId", table: "AdditionalCharges", column: "BillId");
        migrationBuilder.CreateIndex(name: "IX_AdditionalCharges_UnitId", table: "AdditionalCharges", column: "UnitId");
    }

    protected override void Down(MigrationBuilder migrationBuilder) => migrationBuilder.DropTable(name: "AdditionalCharges");
}
