using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class UpdateAccessory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AccessoryId",
                table: "ProductCustom",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Accessory",
                columns: table => new
                {
                    AccessoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accessory", x => x.AccessoryId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustom_AccessoryId",
                table: "ProductCustom",
                column: "AccessoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductCustom_Accessory",
                table: "ProductCustom",
                column: "AccessoryId",
                principalTable: "Accessory",
                principalColumn: "AccessoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductCustom_Accessory",
                table: "ProductCustom");

            migrationBuilder.DropTable(
                name: "Accessory");

            migrationBuilder.DropIndex(
                name: "IX_ProductCustom_AccessoryId",
                table: "ProductCustom");

            migrationBuilder.DropColumn(
                name: "AccessoryId",
                table: "ProductCustom");
        }
    }
}
