using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddIdempotencyKeyToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                table: "orders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                table: "orders");
        }
    }
}
