using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateDeleteStoreId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customer_Store",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_Flower_Store",
                table: "Flower");

            migrationBuilder.DropForeignKey(
                name: "FK_FlowerBasket_Store",
                table: "FlowerBasket");

            migrationBuilder.DropForeignKey(
                name: "FK_Product_Store",
                table: "Product");

            // migrationBuilder.DropIndex(
            //     name: "IX_Product_StoreId",
            //     table: "Product");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_FlowerBasket_StoreId",
            //     table: "FlowerBasket");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_Flower_StoreId",
            //     table: "Flower");
            //
            // migrationBuilder.DropIndex(
            //     name: "IX_Customer_StoreId",
            //     table: "Customer");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Product");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "FlowerBasket");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Flower");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Cart");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Product",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "FlowerBasket",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Flower",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Customer",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoreId",
                table: "Cart",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Product_StoreId",
                table: "Product",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowerBasket_StoreId",
                table: "FlowerBasket",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Flower_StoreId",
                table: "Flower",
                column: "StoreId");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_StoreId",
                table: "Customer",
                column: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customer_Store",
                table: "Customer",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Flower_Store",
                table: "Flower",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_FlowerBasket_Store",
                table: "FlowerBasket",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "StoreId");

            migrationBuilder.AddForeignKey(
                name: "FK_Product_Store",
                table: "Product",
                column: "StoreId",
                principalTable: "Store",
                principalColumn: "StoreId");
        }
    }
}
