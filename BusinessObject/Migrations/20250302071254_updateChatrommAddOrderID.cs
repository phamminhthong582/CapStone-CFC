using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class updateChatrommAddOrderID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "ChatRoom",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatRoom_OrderId",
                table: "ChatRoom",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatRoom_Order",
                table: "ChatRoom",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatRoom_Order",
                table: "ChatRoom");

            migrationBuilder.DropIndex(
                name: "IX_ChatRoom_OrderId",
                table: "ChatRoom");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ChatRoom");
        }
    }
}
