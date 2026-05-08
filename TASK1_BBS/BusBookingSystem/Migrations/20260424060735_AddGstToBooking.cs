using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddGstToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "GST",
                table: "Bookings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "GST",
                table: "Bookings");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$u0K.39GyzLoO58j1t662mOinVh/FNNy7jVLbxoFqWUb5f7/Ifmpwy");
        }
    }
}
