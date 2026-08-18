using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Ecommerce.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscountReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedCount",
                table: "discounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "discount_reservations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OrderId = table.Column<int>(type: "integer", nullable: false),
                    DiscountId = table.Column<int>(type: "integer", nullable: false),
                    ReservedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    IsReleased = table.Column<bool>(type: "boolean", nullable: false, defaultValueSql: "false"),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp without time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_discount_reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_discount_reservations_discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_discount_reservations_orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_discount_reservations_DiscountId",
                table: "discount_reservations",
                column: "DiscountId");

            migrationBuilder.CreateIndex(
                name: "IX_discount_reservations_IsReleased",
                table: "discount_reservations",
                column: "IsReleased");

            migrationBuilder.CreateIndex(
                name: "IX_discount_reservations_OrderId_DiscountId",
                table: "discount_reservations",
                columns: new[] { "OrderId", "DiscountId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "discount_reservations");

            migrationBuilder.DropColumn(
                name: "ReservedCount",
                table: "discounts");
        }
    }
}
