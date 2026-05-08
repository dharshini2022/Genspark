using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusBookingSystem.Migrations
{
    /// <inheritdoc />
    public partial class AddBusFeaturesAndPhotos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Features",
                table: "Buses",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Photos",
                table: "Buses",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$F6SM4nH1uALYyYzrPmDNPOTvDh/DnkNmJ8dfZSa8C4debXn/Xitlq");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "PasswordHash",
                value: "$2a$11$PtdQgN.G9Y1A/8GI6H5goOkm.0cV2NZvuKmQHFypW1h31KHR1cOIq");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Features",
                table: "Buses");

            migrationBuilder.DropColumn(
                name: "Photos",
                table: "Buses");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$K7Ij6KO.7CJI5QXQ4QEa7OmL6ndFIo76UnosZO8DgNL8N/T98XLJW");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 7,
                column: "PasswordHash",
                value: "$2a$11$KFv/41mysE9tThcTTl66mO1ZvUN/6mRyfyxc4aIsqtpuKGRHmbqGq");
        }
    }
}
