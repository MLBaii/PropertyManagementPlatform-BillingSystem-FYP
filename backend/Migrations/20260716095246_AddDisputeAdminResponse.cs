using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PropertyBill.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDisputeAdminResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminResponse",
                table: "Disputes",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminResponse",
                table: "Disputes");
        }
    }
}
