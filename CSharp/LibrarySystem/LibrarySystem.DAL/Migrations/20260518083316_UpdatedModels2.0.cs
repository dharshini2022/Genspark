using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibrarySystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModels20 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "DamageLogs",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "DamageLogs");
        }
    }
}
