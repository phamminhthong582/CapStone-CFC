using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class UpdateFlowerBasket2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "FlowerBasket",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Category",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBasket_CategoryId",
                table: "FlowerBasket",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowerBasket_Category",
                table: "FlowerBasket",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FlowerBasket_Category",
                table: "FlowerBasket");

            migrationBuilder.DropIndex(
                name: "IX_FlowerBasket_CategoryId",
                table: "FlowerBasket");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "FlowerBasket");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Category");
        }
    }
}
