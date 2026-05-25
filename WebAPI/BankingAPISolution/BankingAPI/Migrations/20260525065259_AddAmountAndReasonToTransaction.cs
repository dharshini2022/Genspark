using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAmountAndReasonToTransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "Amount",
                table: "Transactions",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Transactions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Transactions");
        }
    }
}
