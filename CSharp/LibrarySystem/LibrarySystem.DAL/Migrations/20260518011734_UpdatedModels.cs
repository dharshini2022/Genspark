using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LibrarySystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Members_Phone",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "Members",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DamageLogs",
                columns: table => new
                {
                    DamageId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MemberId = table.Column<int>(type: "integer", nullable: false),
                    CopyId = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    DateOfEntry = table.Column<DateTime>(type: "Date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DamageLogs", x => x.DamageId);
                    table.ForeignKey(
                        name: "FK_DamageLogs_BookCopies_CopyId",
                        column: x => x.CopyId,
                        principalTable: "BookCopies",
                        principalColumn: "CopyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DamageLogs_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "MemberId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DamageLogs_CopyId",
                table: "DamageLogs",
                column: "CopyId");

            migrationBuilder.CreateIndex(
                name: "IX_DamageLogs_MemberId",
                table: "DamageLogs",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DamageLogs");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Members");

            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Members",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_Phone",
                table: "Members",
                column: "Phone",
                unique: true);
        }
    }
}
