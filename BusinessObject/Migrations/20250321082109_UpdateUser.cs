using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessObject.Entities
{
    /// <inheritdoc />
    public partial class UpdateUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employee_Role",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_ProductCustom1",
                table: "Order");

            //migrationBuilder.DropIndex(
            //    name: "IX_Order_ProductCustomId",
            //    table: "Order");

            //migrationBuilder.DropIndex(
            //    name: "IX_Employee_RoleId",
            //    table: "Employee");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "ProductCustom",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Employee",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Customer",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_User", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_User_Role",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductCustom_OrderId",
                table: "ProductCustom",
                column: "OrderId",
                unique: true,
                filter: "[OrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_UserId",
                table: "Employee",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customer_UserId",
                table: "Customer",
                column: "UserId",
                unique: true,
                filter: "[UserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Customer",
                table: "Customer",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Employee",
                table: "Employee",
                column: "UserId",
                principalTable: "User",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_ProductCustom",
                table: "ProductCustom",
                column: "OrderId",
                principalTable: "Order",
                principalColumn: "OrderId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Customer",
                table: "Customer");

            migrationBuilder.DropForeignKey(
                name: "FK_User_Employee",
                table: "Employee");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_ProductCustom",
                table: "ProductCustom");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropIndex(
                name: "IX_ProductCustom_OrderId",
                table: "ProductCustom");

            migrationBuilder.DropIndex(
                name: "IX_Employee_UserId",
                table: "Employee");

            migrationBuilder.DropIndex(
                name: "IX_Customer_UserId",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "ProductCustom");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Employee");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Customer");

            migrationBuilder.CreateIndex(
                name: "IX_Order_ProductCustomId",
                table: "Order",
                column: "ProductCustomId");

            migrationBuilder.CreateIndex(
                name: "IX_Employee_RoleId",
                table: "Employee",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employee_Role",
                table: "Employee",
                column: "RoleId",
                principalTable: "Role",
                principalColumn: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_ProductCustom1",
                table: "Order",
                column: "ProductCustomId",
                principalTable: "ProductCustom",
                principalColumn: "ProductCustomId");
        }
    }
}
