using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountFieldsToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountAppliedAt",
                table: "carts",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountCode",
                table: "carts",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountAppliedAt",
                table: "carts");

            migrationBuilder.DropColumn(
                name: "DiscountCode",
                table: "carts");
        }
    }
}
