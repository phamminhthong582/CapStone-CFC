using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class UpdateStyleAndAccessoryCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Style",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Accessory",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Style_CategoryId",
                table: "Style",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Accessory_CategoryId",
                table: "Accessory",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accessory_Category",
                table: "Accessory",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Style_Category",
                table: "Style",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accessory_Category",
                table: "Accessory");

            migrationBuilder.DropForeignKey(
                name: "FK_Style_Category",
                table: "Style");

            migrationBuilder.DropIndex(
                name: "IX_Style_CategoryId",
                table: "Style");

            migrationBuilder.DropIndex(
                name: "IX_Accessory_CategoryId",
                table: "Accessory");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Style");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Accessory");
        }
    }
}
