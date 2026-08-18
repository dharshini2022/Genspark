using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class RenameCreatedAtToUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "reviews",
                newName: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "reviews",
                newName: "CreatedAt");
        }
    }
}
