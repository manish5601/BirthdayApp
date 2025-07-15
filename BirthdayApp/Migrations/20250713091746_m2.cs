using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BirthdayApp.Migrations
{
    /// <inheritdoc />
    public partial class m2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BirthdayWishes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WishMessage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FromUserId = table.Column<int>(type: "int", nullable: false),
                    ToUserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ToUserId = table.Column<int>(type: "int", nullable: false),
                    BirthdayDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BirthdayWishes", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BirthdayWishes");
        }
    }
}
